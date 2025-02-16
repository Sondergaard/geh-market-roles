﻿// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.MarketRoles.Application.ChangeOfSupplier.Processing;
using Energinet.DataHub.MarketRoles.Application.ChangeOfSupplier.Processing.ConsumerDetails;
using Energinet.DataHub.MarketRoles.Application.ChangeOfSupplier.Processing.EndOfSupplyNotification;
using Energinet.DataHub.MarketRoles.Application.ChangeOfSupplier.Processing.MeteringPointDetails;
using Energinet.DataHub.MarketRoles.Domain.Consumers;
using Energinet.DataHub.MarketRoles.Domain.EnergySuppliers;
using Energinet.DataHub.MarketRoles.Domain.MeteringPoints;
using Energinet.DataHub.MarketRoles.Domain.MeteringPoints.Events;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MarketRoles.IntegrationTests.Application.ChangeOfSupplier.Processing
{
    [IntegrationTest]
    public class ProcessManagerRouterTests : TestHost
    {
        private readonly ProcessManagerRouter _router;
        private readonly BusinessProcessId _businessProcessId;
        private readonly AccountingPoint _accountingPoint;
        private readonly EnergySupplier _energySupplier;
        private readonly Transaction _transaction;

        public ProcessManagerRouterTests(DatabaseFixture databaseFixture)
            : base(databaseFixture)
        {
            Consumer consumer = CreateConsumer();
            _energySupplier = CreateEnergySupplier(Guid.NewGuid(), SampleData.GlnNumber);
            _accountingPoint = CreateAccountingPoint();
            _transaction = CreateTransaction();

            EnergySupplier newEnergySupplier = CreateEnergySupplier(Guid.NewGuid(), "7495563456235");
            SetConsumerMovedIn(_accountingPoint, consumer.ConsumerId, _energySupplier.EnergySupplierId);
            RegisterChangeOfSupplier(_accountingPoint, newEnergySupplier.EnergySupplierId, _transaction);
            MarketRolesContext.SaveChanges();

            _businessProcessId = GetBusinessProcessId(_transaction);
            _router = new ProcessManagerRouter(ProcessManagerRepository, CommandScheduler);
        }

        [Fact]
        public async Task EnergySupplierChangeIsRegistered_WhenStateIsNotStarted_ForwardMasterDataDetailsCommandIsEnqueued()
        {
            await _router.Handle(CreateSupplierChangeRegisteredEvent(), CancellationToken.None).ConfigureAwait(false);
            await UnitOfWork.CommitAsync().ConfigureAwait(false);

            var command = await GetEnqueuedCommandAsync<ForwardMeteringPointDetails>().ConfigureAwait(false);

            Assert.NotNull(command);
            Assert.Equal(_businessProcessId.Value, command?.BusinessProcessId);
        }

        [Fact]
        public async Task MeteringPointDetailsAreDispatched_WhenStateIsAwaitingMeteringPointDetailsDispatch_ForwardConsumerDetailsCommandIsEnqueued()
        {
            await _router.Handle(CreateSupplierChangeRegisteredEvent(), CancellationToken.None).ConfigureAwait(false);
            await UnitOfWork.CommitAsync().ConfigureAwait(false);

            await _router.Handle(new MeteringPointDetailsDispatched(_accountingPoint.Id, _businessProcessId, Transaction), CancellationToken.None).ConfigureAwait(false);
            await UnitOfWork.CommitAsync().ConfigureAwait(false);

            var command = await GetEnqueuedCommandAsync<ForwardConsumerDetails>().ConfigureAwait(false);
            Assert.NotNull(command);
            Assert.Equal(_businessProcessId.Value, command?.BusinessProcessId);
        }

        [Fact]
        public async Task ConsumerDetailsAreDispatched_WhenStateIsAwaitingConsumerDetailsDispatch_NotifyCurrentSupplierCommandIsEnqueued()
        {
            await _router.Handle(CreateSupplierChangeRegisteredEvent(), CancellationToken.None).ConfigureAwait(false);
            await UnitOfWork.CommitAsync().ConfigureAwait(false);

            await _router.Handle(new MeteringPointDetailsDispatched(_accountingPoint.Id, _businessProcessId, Transaction), CancellationToken.None).ConfigureAwait(false);
            await UnitOfWork.CommitAsync().ConfigureAwait(false);

            await _router.Handle(new ConsumerDetailsDispatched(_accountingPoint.Id, _businessProcessId, Transaction), CancellationToken.None).ConfigureAwait(false);
            await UnitOfWork.CommitAsync().ConfigureAwait(false);

            var command = await GetEnqueuedCommandAsync<NotifyCurrentSupplier>().ConfigureAwait(false);
            Assert.NotNull(command);
            Assert.Equal(_businessProcessId.Value, command?.BusinessProcessId);
        }

        [Fact]
        public async Task CurrentSupplierIsNotified_WhenStateIsAwaitingCurrentSupplierNotification_ChangeSupplierCommandIsScheduled()
        {
            await _router.Handle(CreateSupplierChangeRegisteredEvent(), CancellationToken.None).ConfigureAwait(false);
            await UnitOfWork.CommitAsync().ConfigureAwait(false);

            await _router.Handle(new MeteringPointDetailsDispatched(_accountingPoint.Id, _businessProcessId, Transaction), CancellationToken.None).ConfigureAwait(false);
            await UnitOfWork.CommitAsync().ConfigureAwait(false);

            await _router.Handle(new ConsumerDetailsDispatched(_accountingPoint.Id, _businessProcessId, Transaction), CancellationToken.None).ConfigureAwait(false);
            await UnitOfWork.CommitAsync().ConfigureAwait(false);

            await _router.Handle(new CurrentSupplierNotified(_accountingPoint.Id, _businessProcessId, Transaction), CancellationToken.None).ConfigureAwait(false);
            await UnitOfWork.CommitAsync().ConfigureAwait(false);

            var command = await GetEnqueuedCommandAsync<ChangeSupplier>().ConfigureAwait(false);
            Assert.NotNull(command);
            Assert.Equal(_accountingPoint.Id.Value, command?.AccountingPointId);
        }

        [Fact]
        public async Task SupplierIsChanged_WhenStateIsAwaitingSupplierChange_ProcessIsCompleted()
        {
            await _router.Handle(CreateSupplierChangeRegisteredEvent(), CancellationToken.None).ConfigureAwait(false);
            await UnitOfWork.CommitAsync().ConfigureAwait(false);

            await _router.Handle(new MeteringPointDetailsDispatched(_accountingPoint.Id, _businessProcessId, Transaction), CancellationToken.None).ConfigureAwait(false);
            await UnitOfWork.CommitAsync().ConfigureAwait(false);

            await _router.Handle(new ConsumerDetailsDispatched(_accountingPoint.Id, _businessProcessId, Transaction), CancellationToken.None).ConfigureAwait(false);
            await UnitOfWork.CommitAsync().ConfigureAwait(false);

            await _router.Handle(new CurrentSupplierNotified(_accountingPoint.Id, _businessProcessId, Transaction), CancellationToken.None).ConfigureAwait(false);
            await UnitOfWork.CommitAsync().ConfigureAwait(false);

            await _router.Handle(CreateEnergySupplierChangedEvent(), CancellationToken.None).ConfigureAwait(false);
            await UnitOfWork.CommitAsync().ConfigureAwait(false);

            var processManager = await ProcessManagerRepository.GetAsync<ChangeOfSupplierProcessManager>(_businessProcessId).ConfigureAwait(false);
            Assert.True(processManager?.IsCompleted());
        }

        private EnergySupplierChangeRegistered CreateSupplierChangeRegisteredEvent()
        {
            return new EnergySupplierChangeRegistered(
                _accountingPoint.Id,
                _accountingPoint.GsrnNumber,
                _businessProcessId,
                _transaction,
                EffectiveDate,
                _energySupplier.EnergySupplierId);
        }

        private EnergySupplierChanged CreateEnergySupplierChangedEvent()
        {
            return new EnergySupplierChanged(
                _accountingPoint.Id.Value,
                _accountingPoint.GsrnNumber.Value,
                _businessProcessId.Value,
                _transaction.Value,
                _energySupplier.EnergySupplierId.Value,
                EffectiveDate);
        }
    }
}

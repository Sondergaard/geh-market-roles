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
using System.Threading.Tasks;
using Energinet.DataHub.MarketRoles.Application.Common.Commands;
using Energinet.DataHub.MarketRoles.Application.Common.Processing;
using Energinet.DataHub.MarketRoles.Application.MoveIn;
using Energinet.DataHub.MarketRoles.Application.MoveIn.Processing;
using Energinet.DataHub.MarketRoles.Domain.MeteringPoints;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MarketRoles.IntegrationTests.Application.MoveIn.Processing
{
    [IntegrationTest]
    public class MoveInProcessManagerTests : TestHost
    {
        private readonly MoveInProcessManagerRouter _router;

        public MoveInProcessManagerTests(DatabaseFixture databaseFixture)
            : base(databaseFixture)
        {
            _router = new MoveInProcessManagerRouter(GetService<IProcessManagerRepository>(), GetService<ICommandScheduler>());
        }

        [Fact]
        public async Task ConsumerMoveInAccepted_WhenStateIsNotStarted_EffectuateCommandIsEnqueued()
        {
            var (transaction, businessProcessId) = await SetupScenario().ConfigureAwait(false);

            var command = await GetEnqueuedCommandAsync<EffectuateConsumerMoveIn>(businessProcessId).ConfigureAwait(false);

            Assert.NotNull(command);
            Assert.Equal(transaction.Value, command?.Transaction);
        }

        [Fact]
        public async Task ConsumerMoveIn_WhenStateIsAwaitingEffectuation_ProcessIsCompleted()
        {
            var (_, businessProcessId) = await SetupScenario().ConfigureAwait(false);

            var effectuateConsumerMoveInCommand = await GetEnqueuedCommandAsync<EffectuateConsumerMoveIn>(businessProcessId).ConfigureAwait(false);
            await InvokeCommandAsync(effectuateConsumerMoveInCommand!).ConfigureAwait(false);

            var processManager = await ProcessManagerRepository.GetAsync<MoveInProcessManager>(businessProcessId).ConfigureAwait(false);
            Assert.True(processManager?.IsCompleted());
        }

        private async Task<(Transaction Transaction, BusinessProcessId BusinessProcessId)> SetupScenario()
        {
            _ = CreateAccountingPoint();
            _ = CreateEnergySupplier(Guid.NewGuid(), SampleData.GlnNumber);
            _ = CreateConsumer();
            SaveChanges();

            var transaction = CreateTransaction();

            await SendRequestAsync(new RequestMoveIn(
                transaction.Value,
                SampleData.GlnNumber,
                SampleData.ConsumerSSN,
                string.Empty,
                SampleData.ConsumerName,
                SampleData.GsrnNumber,
                SampleData.MoveInDate)).ConfigureAwait(false);

            return (transaction, GetBusinessProcessId(transaction));
        }
    }
}

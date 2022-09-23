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
using Messaging.Application.Actors;
using Messaging.Application.Configuration;
using Messaging.Application.Configuration.DataAccess;
using Messaging.Application.OutgoingMessages;
using Messaging.Application.OutgoingMessages.Common;
using Messaging.Application.Transactions;
using Messaging.Application.Transactions.MoveIn;
using Messaging.Domain.MasterData.MarketEvaluationPoints;
using Messaging.Domain.OutgoingMessages;
using Messaging.Domain.Transactions.MoveIn;
using Messaging.IntegrationTests.Assertions;
using Messaging.IntegrationTests.Fixtures;
using Xunit;
using MarketEvaluationPoint = Messaging.Domain.MasterData.MarketEvaluationPoints.MarketEvaluationPoint;

namespace Messaging.IntegrationTests.Application.Transactions.MoveIn;

public class WhenAConsumerHasMovedInTests : TestBase
{
    private readonly IMoveInTransactionRepository _transactionRepository;

    public WhenAConsumerHasMovedInTests(DatabaseFixture databaseFixture)
        : base(databaseFixture)
    {
        _transactionRepository = GetService<IMoveInTransactionRepository>();
    }

    [Fact]
    public async Task An_exception_is_thrown_if_transaction_cannot_be_located()
    {
        var processId = "Not existing";
        var command = new SetConsumerHasMovedIn(processId);

        await Assert.ThrowsAsync<TransactionNotFoundException>(() => InvokeCommandAsync(command)).ConfigureAwait(false);
    }

    [Fact]
    public async Task Business_process_is_marked_as_completed()
    {
        await ConsumerHasMovedIn().ConfigureAwait(false);

        AssertQueuedCommand.QueuedCommand<CreateEndOfSupplyNotification>(GetService<IDbConnectionFactory>());
        AssertTransaction()
            .BusinessProcessCompleted();
    }

    [Fact]
    public async Task The_current_energy_supplier_is_notified_about_end_of_supply()
    {
        var transaction = await ConsumerHasMovedIn().ConfigureAwait(false);

        await InvokeCommandAsync(CreateCommand()).ConfigureAwait(false);

        AssertTransaction()
            .HasEndOfSupplyNotificationState(MoveInTransaction.EndOfSupplyNotificationState.EnergySupplierWasNotified);
        AssertMessage(DocumentType.GenericNotification, BusinessReasonCode.CustomerMoveInOrMoveOut.Code)
            .HasReceiverId(transaction.CurrentEnergySupplierId!)
            .HasReceiverRole(MarketRoles.EnergySupplier)
            .HasSenderId(DataHubDetails.IdentificationNumber)
            .HasSenderRole(MarketRoles.MeteringPointAdministrator)
            .WithMarketActivityRecord()
            .HasId()
            .HasValidityStart(SampleData.SupplyStart)
            .HasOriginalTransactionId(transaction.TransactionId)
            .HasMarketEvaluationPointId(transaction.MarketEvaluationPointId);
    }

    [Fact]
    public async Task Grid_operator_is_notified_about_the_move_in()
    {
        await ConsumerHasMovedIn().ConfigureAwait(false);

        AssertOutgoingMessage.OutgoingMessage(
                SampleData.TransactionId,
                DocumentType.GenericNotification.Name,
                ProcessType.MoveIn.Code,
                GetService<IDbConnectionFactory>())
            .HasSenderId(DataHubDetails.IdentificationNumber)
            .HasSenderRole(MarketRoles.MeteringPointAdministrator)
            .HasReceiverRole(MarketRoles.GridOperator)
            .HasReceiverId(SampleData.NumberOfGridOperatorForMeteringPoint)
            .WithMarketActivityRecord()
            .HasValidityStart(SampleData.SupplyStart)
            .HasId()
            .HasOriginalTransactionId(SampleData.TransactionId);
    }

    private static CreateEndOfSupplyNotification CreateCommand()
    {
        return new CreateEndOfSupplyNotification(SampleData.TransactionId, SampleData.SupplyStart, SampleData.MeteringPointNumber, SampleData.CurrentEnergySupplierNumber);
    }

    private AssertOutgoingMessage AssertMessage(DocumentType documentType, string processType)
    {
        return AssertOutgoingMessage.OutgoingMessage(SampleData.TransactionId, documentType.Name, processType, GetService<IDbConnectionFactory>());
    }

    private async Task<MoveInTransaction> ConsumerHasMovedIn()
    {
        var transaction = await StartMoveInTransaction();
        await InvokeCommandAsync(new SetConsumerHasMovedIn(transaction.ProcessId!)).ConfigureAwait(false);
        return transaction;
    }

    private async Task<MoveInTransaction> StartMoveInTransaction()
    {
        await SetupGridOperatorDetailsAsync();
        await SetupMasterDataDetailsAsync();
        var transaction = new MoveInTransaction(
            SampleData.TransactionId,
            SampleData.MeteringPointNumber,
            SampleData.SupplyStart,
            SampleData.CurrentEnergySupplierNumber,
            SampleData.OriginalMessageId,
            SampleData.NewEnergySupplierNumber,
            SampleData.ConsumerId,
            SampleData.ConsumerName,
            SampleData.ConsumerIdType);

        transaction.AcceptedByBusinessProcess(BusinessRequestResult.Succeeded(Guid.NewGuid().ToString()).ProcessId!, SampleData.MeteringPointNumber);
        transaction.MarkMeteringPointMasterDataAsSent();
        _transactionRepository.Add(transaction);
        await GetService<IUnitOfWork>().CommitAsync().ConfigureAwait(false);
        return transaction;
    }

    private Task SetupGridOperatorDetailsAsync()
    {
        return InvokeCommandAsync(new CreateActor(
            SampleData.IdOfGridOperatorForMeteringPoint.ToString(),
            SampleData.NumberOfGridOperatorForMeteringPoint));
    }

    private Task SetupMasterDataDetailsAsync()
    {
        var marketEvaluationPoint = MarketEvaluationPoint.Create(
            SampleData.CurrentEnergySupplierNumber,
            SampleData.MeteringPointNumber);
        marketEvaluationPoint.SetGridOperatorId(SampleData.IdOfGridOperatorForMeteringPoint);

        GetService<IMarketEvaluationPointRepository>()
            .Add(marketEvaluationPoint);
        return Task.CompletedTask;
    }

    private AssertTransaction AssertTransaction()
    {
        return MoveIn.AssertTransaction
            .Transaction(SampleData.TransactionId, GetService<IDbConnectionFactory>());
    }
}

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
using System.Xml.Linq;
using JetBrains.Annotations;
using MediatR;
using Messaging.Application.OutgoingMessages.Peek;
using Messaging.Domain.Actors;
using Messaging.Domain.OutgoingMessages;
using Messaging.Domain.OutgoingMessages.Peek;
using Messaging.Domain.SeedWork;
using Messaging.Infrastructure.Configuration.DataAccess;
using Messaging.IntegrationTests.Application.IncomingMessages;
using Messaging.IntegrationTests.Application.Transactions.MoveIn;
using Messaging.IntegrationTests.Assertions;
using Messaging.IntegrationTests.Factories;
using Messaging.IntegrationTests.Fixtures;
using Messaging.IntegrationTests.TestDoubles;
using Microsoft.EntityFrameworkCore.SqlServer.NodaTime.Extensions;
using Xunit;

namespace Messaging.IntegrationTests.Application.OutgoingMessages;

public class WhenAPeekIsRequestedTests : TestBase
{
    public WhenAPeekIsRequestedTests(DatabaseFixture databaseFixture)
        : base(databaseFixture)
    {
    }

    [Fact]
    public async Task When_no_messages_are_available_return_empty_result()
    {
        var command = new PeekRequest(ActorNumber.Create(SampleData.NewEnergySupplierNumber), MessageCategory.AggregationData);

        var result = await InvokeCommandAsync(command).ConfigureAwait(false);

        Assert.Null(result.Bundle);
    }

    [Fact]
    public async Task A_message_bundle_is_returned()
    {
        await GivenTwoMoveInTransactionHasBeenAccepted();

        var command = new PeekRequest(ActorNumber.Create(SampleData.NewEnergySupplierNumber), MessageCategory.MasterData);
        var result = await InvokeCommandAsync(command).ConfigureAwait(false);

        Assert.NotNull(result.Bundle);

        AssertXmlMessage.Document(XDocument.Load(result.Bundle!))
            .IsDocumentType(DocumentType.ConfirmRequestChangeOfSupplier)
            .IsProcesType(ProcessType.MoveIn)
            .HasMarketActivityRecordCount(2);
    }

    [Fact]
    public async Task Bundled_message_contains_maximum_number_of_payloads()
    {
        var bundleConfiguration = (BundleConfigurationStub)GetService<IBundleConfiguration>();
        bundleConfiguration.MaxNumberOfPayloadsInBundle = 1;
        await GivenTwoMoveInTransactionHasBeenAccepted().ConfigureAwait(false);

        var command = new PeekRequest(ActorNumber.Create(SampleData.NewEnergySupplierNumber), MessageCategory.MasterData);
        var result = await InvokeCommandAsync(command).ConfigureAwait(false);

        AssertXmlMessage.Document(XDocument.Load(result.Bundle!))
            .IsDocumentType(DocumentType.ConfirmRequestChangeOfSupplier)
            .IsProcesType(ProcessType.MoveIn)
            .HasMarketActivityRecordCount(1);
    }

    private static IncomingMessageBuilder MessageBuilder()
    {
        return new IncomingMessageBuilder()
            .WithEnergySupplierId(SampleData.NewEnergySupplierNumber)
            .WithMessageId(SampleData.OriginalMessageId)
            .WithTransactionId(SampleData.TransactionId);
    }

    private async Task GivenAMoveInTransactionHasBeenAccepted()
    {
        var incomingMessage = MessageBuilder()
            .WithProcessType(ProcessType.MoveIn.Code)
            .WithReceiver(SampleData.ReceiverId)
            .WithSenderId(SampleData.SenderId)
            .WithConsumerName(SampleData.ConsumerName)
            .Build();

        await InvokeCommandAsync(incomingMessage).ConfigureAwait(false);
    }

    private async Task GivenTwoMoveInTransactionHasBeenAccepted()
    {
        await GivenAMoveInTransactionHasBeenAccepted().ConfigureAwait(false);

        var message = MessageBuilder()
            .WithProcessType(ProcessType.MoveIn.Code)
            .WithReceiver(SampleData.ReceiverId)
            .WithSenderId(SampleData.SenderId)
            .WithEffectiveDate(EffectiveDateFactory.OffsetDaysFromToday(1))
            .WithConsumerId(ConsumerFactory.CreateConsumerId())
            .WithConsumerName(ConsumerFactory.CreateConsumerName())
            .WithTransactionId(Guid.NewGuid().ToString()).Build();

        await InvokeCommandAsync(message).ConfigureAwait(false);
    }
}

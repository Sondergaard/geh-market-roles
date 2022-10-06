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

using System.Threading.Tasks;
using Messaging.Application.Configuration;
using Messaging.Application.Configuration.DataAccess;
using Messaging.Application.OutgoingMessages;
using Messaging.Domain.Actors;
using Messaging.Domain.OutgoingMessages;
using Messaging.Domain.SeedWork;
using Messaging.Infrastructure.OutgoingMessages;
using Messaging.IntegrationTests.Application.IncomingMessages;
using Messaging.IntegrationTests.Fixtures;
using Messaging.IntegrationTests.TestDoubles;
using Xunit;
using Xunit.Categories;

namespace Messaging.IntegrationTests.Infrastructure.OutgoingMessages
{
    [IntegrationTest]
    public class MessageAvailabilityPublisherTests : TestBase
    {
        private readonly IOutgoingMessageStore _outgoingMessageStore;
        private readonly MessageAvailabilityPublisher _messageAvailabilityPublisher;
        private readonly NewMessageAvailableNotifierSpy _newMessageAvailableNotifierSpy;

        public MessageAvailabilityPublisherTests(DatabaseFixture databaseFixture)
            : base(databaseFixture)
        {
            _outgoingMessageStore = GetService<IOutgoingMessageStore>();
            _messageAvailabilityPublisher = GetService<MessageAvailabilityPublisher>();
            _newMessageAvailableNotifierSpy = (NewMessageAvailableNotifierSpy)GetService<INewMessageAvailableNotifier>();
        }

        [Fact]
        public async Task Outgoing_messages_are_published()
        {
            var outgoingMessage = CreateOutgoingMessage();
            await StoreOutgoingMessage(outgoingMessage).ConfigureAwait(false);

            await _messageAvailabilityPublisher.PublishAsync().ConfigureAwait(false);

            var unpublishedMessages = _outgoingMessageStore.GetUnpublished();
            var publishedMessage = _newMessageAvailableNotifierSpy.GetMessageFrom(outgoingMessage.Id);
            Assert.Empty(unpublishedMessages);
            Assert.NotNull(publishedMessage);
        }

        private static OutgoingMessage CreateOutgoingMessage()
        {
            var transaction = new IncomingMessageBuilder()
                .WithSenderId("1234567890123")
                .WithReceiver("1234567890124")
                .Build();
            return new OutgoingMessage(
                DocumentType.GenericNotification,
                ActorNumber.Create(transaction.Message.ReceiverId),
                transaction.MarketActivityRecord.Id,
                transaction.Message.ProcessType,
                EnumerationType.FromName<MarketRole>(transaction.Message.ReceiverRole),
                ActorNumber.Create(transaction.Message.SenderId),
                EnumerationType.FromName<MarketRole>(transaction.Message.SenderRole),
                string.Empty);
        }

        private async Task StoreOutgoingMessage(OutgoingMessage outgoingMessage)
        {
            _outgoingMessageStore.Add(outgoingMessage);
            await GetService<IUnitOfWork>().CommitAsync().ConfigureAwait(false);
        }
    }
}

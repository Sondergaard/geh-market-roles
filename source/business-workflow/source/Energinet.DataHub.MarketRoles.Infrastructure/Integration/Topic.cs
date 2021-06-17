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
using Azure.Messaging.ServiceBus;

namespace Energinet.DataHub.MarketRoles.Infrastructure.Integration
{
    public abstract class Topic : IAsyncDisposable // singleton in DI
    {
        private readonly ServiceBusClient _client;
        private readonly Lazy<ServiceBusSender> _senderCreator;

        public Topic(ServiceBusClient client)
        {
            _client = client;
            _senderCreator = new Lazy<ServiceBusSender>(() => _client.CreateSender(TopicName));
        }

        public abstract string TopicName { get; }

        public ServiceBusSender CreateSender()
        {
            return _senderCreator.Value;
        }

        public async ValueTask DisposeAsync()
        {
            await _client.DisposeAsync();

            if (_senderCreator.IsValueCreated)
            {
                await _senderCreator.Value.DisposeAsync();
            }

            GC.SuppressFinalize(this);
        }
    }
}

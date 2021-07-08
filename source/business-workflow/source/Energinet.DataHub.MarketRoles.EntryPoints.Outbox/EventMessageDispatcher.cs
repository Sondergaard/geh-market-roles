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
using Energinet.DataHub.MarketRoles.Application.Integration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.MarketRoles.EntryPoints.Outbox
{
    public class EventMessageDispatcher
    {
        private readonly IIntegrationEventDispatchOrchestrator _integrationEventDispatchOrchestrator;
        private readonly ILogger _logger;

        public EventMessageDispatcher(IIntegrationEventDispatchOrchestrator integrationEventDispatchOrchestrator, ILogger logger)
        {
            _integrationEventDispatchOrchestrator = integrationEventDispatchOrchestrator;
            _logger = logger;
        }

        [Function("EventMessageDispatcher")]
        public async Task RunAsync(
            [TimerTrigger("%EVENT_MESSAGE_DISPATCH_TRIGGER_TIMER%")] TimerInfo timerInfo, FunctionContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTimeOffset.UtcNow} (UTC)");
            _logger.LogInformation($"Next timer schedule at: {timerInfo?.ScheduleStatus?.Next}");
            await _integrationEventDispatchOrchestrator.ProcessEventsAsync().ConfigureAwait(false);
        }
    }
}

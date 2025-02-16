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
using Energinet.DataHub.MarketRoles.Application.Common.Commands;
using Energinet.DataHub.MarketRoles.Application.Common.DomainEvents;
using Energinet.DataHub.MarketRoles.Domain.MeteringPoints;
using MediatR;

namespace Energinet.DataHub.MarketRoles.Application.ChangeOfSupplier.Processing.ConsumerDetails
{
    public class ForwardConsumerDetailsHandler : ICommandHandler<ForwardConsumerDetails>
    {
        private readonly IDomainEventPublisher _domainEventPublisher;
        private readonly IConsumerDetailsForwarder _consumerDetailsForwarder;

        public ForwardConsumerDetailsHandler(IDomainEventPublisher domainEventPublisher, IConsumerDetailsForwarder consumerDetailsForwarder)
        {
            _domainEventPublisher = domainEventPublisher ?? throw new ArgumentNullException(nameof(domainEventPublisher));
            _consumerDetailsForwarder = consumerDetailsForwarder ?? throw new ArgumentNullException(nameof(consumerDetailsForwarder));
        }

        public async Task<Unit> Handle(ForwardConsumerDetails request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            await _domainEventPublisher.PublishAsync(new ConsumerDetailsDispatched(
                AccountingPointId.Create(request.AccountingPointId),
                BusinessProcessId.Create(request.BusinessProcessId),
                Transaction.Create(request.Transaction))).ConfigureAwait(false);

            return Unit.Value;
        }
    }
}

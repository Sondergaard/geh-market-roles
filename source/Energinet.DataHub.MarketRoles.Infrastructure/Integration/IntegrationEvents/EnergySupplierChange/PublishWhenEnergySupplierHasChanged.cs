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
using Dapper;
using Energinet.DataHub.MarketRoles.Domain.EnergySuppliers;
using Energinet.DataHub.MarketRoles.Infrastructure.DataAccess;
using Energinet.DataHub.MarketRoles.Infrastructure.Outbox;
using MediatR;

namespace Energinet.DataHub.MarketRoles.Infrastructure.Integration.IntegrationEvents.EnergySupplierChange
{
    public class PublishWhenEnergySupplierHasChanged : INotificationHandler<Energinet.DataHub.MarketRoles.Domain.MeteringPoints.Events.EnergySupplierChanged>
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IOutbox _outbox;
        private readonly IOutboxMessageFactory _outboxMessageFactory;

        public PublishWhenEnergySupplierHasChanged(IDbConnectionFactory connectionFactory, IOutbox outbox, IOutboxMessageFactory outboxMessageFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _outbox = outbox ?? throw new ArgumentNullException(nameof(outbox));
            _outboxMessageFactory = outboxMessageFactory ?? throw new ArgumentNullException(nameof(outboxMessageFactory));
        }

        public async Task Handle(Energinet.DataHub.MarketRoles.Domain.MeteringPoints.Events.EnergySupplierChanged notification, CancellationToken cancellationToken)
        {
            if (notification == null) throw new ArgumentNullException(nameof(notification));

            var supplierGlnNumber = await GetSupplierGlnNumberAsync(new EnergySupplierId(notification.EnergySupplierId)).ConfigureAwait(false);
            var integrationEvent = new EnergySupplierChangedIntegrationEvent(
                notification.AccountingPointId,
                notification.GsrnNumber,
                supplierGlnNumber,
                notification.StartOfSupplyDate);

            var message = _outboxMessageFactory.CreateFrom(integrationEvent, OutboxMessageCategory.IntegrationEvent);
            _outbox.Add(message);
        }

        private async Task<string> GetSupplierGlnNumberAsync(EnergySupplierId energySupplierId)
        {
            var sql = $"SELECT GlnNumber FROM [dbo].[EnergySuppliers] WHERE Id = @EnergySupplierId";
            return await _connectionFactory.GetOpenConnection().QuerySingleOrDefaultAsync<string>(sql, new { EnergySupplierId = energySupplierId.Value }).ConfigureAwait(false);
        }
    }
}

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

using Energinet.DataHub.MarketRoles.Domain.Consumers;
using Energinet.DataHub.MarketRoles.Domain.SeedWork;
using NodaTime;

namespace Energinet.DataHub.MarketRoles.Domain.MeteringPoints
{
    public class ConsumerRegistration : Entity
    {
        public ConsumerRegistration(ConsumerId consumerId, BusinessProcessId businessProcessId)
        {
            Id = ConsumerRegistrationId.New();
            ConsumerId = consumerId;
            BusinessProcessId = businessProcessId;
        }

        private ConsumerRegistration(ConsumerId consumerId, Instant moveInDate, BusinessProcessId businessProcessId)
            : this(consumerId, businessProcessId)
        {
            ConsumerId = consumerId;
            MoveInDate = moveInDate;
            BusinessProcessId = businessProcessId;
        }

        public ConsumerRegistrationId Id { get; }

        public ConsumerId ConsumerId { get; }

        public Instant? MoveInDate { get; private set; }

        public BusinessProcessId BusinessProcessId { get; }

        public void SetMoveInDate(Instant effectiveDate)
        {
            MoveInDate = effectiveDate;
        }
    }
}

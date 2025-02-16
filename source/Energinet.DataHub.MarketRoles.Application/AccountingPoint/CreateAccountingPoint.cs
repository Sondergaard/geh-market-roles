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
using Energinet.DataHub.MarketRoles.Application.Common.Commands;
using Energinet.DataHub.MarketRoles.Application.Common.Transport;
using Energinet.DataHub.MarketRoles.Domain.MeteringPoints;

namespace Energinet.DataHub.MarketRoles.Application.AccountingPoint
{
    public class CreateAccountingPoint : InternalCommand, IOutboundMessage, IInboundMessage
    {
        public CreateAccountingPoint(string meteringPointId, string gsrnNumber, string meteringPointType, string physicalState)
        {
            AccountingPointId = meteringPointId;
            GsrnNumber = gsrnNumber;
            MeteringPointType = meteringPointType;
            PhysicalState = physicalState;
        }

        public string GsrnNumber { get; }

        public string MeteringPointType { get; }

        public string AccountingPointId { get; }

        public string PhysicalState { get; }
    }
}

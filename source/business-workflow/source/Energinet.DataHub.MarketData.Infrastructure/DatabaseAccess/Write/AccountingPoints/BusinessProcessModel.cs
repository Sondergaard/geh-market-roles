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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Energinet.DataHub.MarketData.Domain.BusinessProcesses;
using Energinet.DataHub.MarketData.Domain.MeteringPoints;
using NodaTime;

namespace Energinet.DataHub.MarketData.Infrastructure.DatabaseAccess.Write.MeteringPoints
{
    public class BusinessProcessModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecordId { get; set; }

        public AccountingPointModel AccountingPointModel { get; set; } = null!;

        [ForeignKey("AccountingPointModel")]
        public int AccountingPointId { get; set; }

        public ProcessId ProcessId { get; set; } = null!;

        public Instant EffectiveDate { get; set; }

        public BusinessProcessType ProcessType { get; set; } = null!;

        public BusinessProcessStatus Status { get;  set; } = null!;
    }
}

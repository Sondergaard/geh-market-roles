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

namespace Messaging.IntegrationTests.Application.Transactions.AggregatedTimeSeries;

internal class SampleData
{
    internal static string GridOperatorNumber => "1234567890123";

    internal static string GridAreaCode => "870";

    internal static Guid ResultId => Guid.Parse("42AB7292-FE2E-4F33-B537-4A15FEDB9754");

    internal static string MeteringPointType => "E18";

    internal static string MeasureUnitType => "KWH";

    internal static string Resolution => "PT1H";

    internal static string TransactionId => "E71FC033-FBE4-4064-BD38-19705021CAF4";
}

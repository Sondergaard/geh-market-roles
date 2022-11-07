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
using System.Linq;
using Energinet.DataHub.MarketRoles.ApplyDBMigrationsApp.Helpers;

namespace Energinet.DataHub.MarketRoles.ApplyDBMigrationsApp
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var connectionString = ConnectionStringFactory.GetConnectionString(args);
            var filter = EnvironmentFilter.GetFilter(args);
            var isDryRun = args.Contains("dryRun");
            var isManagedIdentity = args.Contains("isManagedIdentity");

            Console.WriteLine($"Args have been resolved to: dryRun = ${isDryRun}, isManagedIdentity = ${isManagedIdentity}");
            var upgrader = UpgradeFactory.GetUpgradeEngine(connectionString, filter, isDryRun, isManagedIdentity);

            var result = upgrader.PerformUpgrade();

            return ResultReporter.ReportResult(result);
        }
    }
}

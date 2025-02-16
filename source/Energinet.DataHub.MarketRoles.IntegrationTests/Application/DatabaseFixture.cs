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
using Energinet.DataHub.MarketRoles.ApplyDBMigrationsApp.Helpers;
using Squadron;

namespace Energinet.DataHub.MarketRoles.IntegrationTests.Application
{
    public class DatabaseFixture : SqlServerResource
    {
        private const string LocalConnectionStringName = "IntegrationTests_ConnectionString";

        private static bool UseLocalDatabase =>
            Environment.GetEnvironmentVariable(LocalConnectionStringName) != null;

        private static string LocalConnectionString =>
            Environment.GetEnvironmentVariable(LocalConnectionStringName)
            ?? throw new InvalidOperationException($"{LocalConnectionStringName} config not set");

        public override Task InitializeAsync()
        {
            // If we're using a local database, skip the squadron initialization.
            return UseLocalDatabase
                ? Task.CompletedTask
                : base.InitializeAsync();
        }

        public string GetConnectionString()
        {
            var connectionString = CreateConnectionString();
            UpdateDatabase(connectionString);
            return connectionString;
        }

        private static void UpdateDatabase(string connectionString)
        {
            var upgrader = UpgradeFactory.GetUpgradeEngine(connectionString, x => true, false);
            var result = upgrader.PerformUpgrade();
            if (!result.Successful)
            {
                throw new InvalidOperationException("Couldn't start test SQL server");
            }
        }

        private string CreateConnectionString()
        {
            return UseLocalDatabase
                ? LocalConnectionString
                : CreateConnectionString(UniqueNameGenerator.Create("db"));
        }
    }
}

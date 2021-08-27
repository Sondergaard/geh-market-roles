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
using Energinet.DataHub.MarketRoles.ApplyDBMigrationsApp.Helpers;
using Squadron;

namespace Energinet.DataHub.MarketRoles.IntegrationTests.Application
{
    public class DatabaseFixture : SqlServerResource
    {
        private string _localConnectionStringName = "MarketRoles_IntegrationTests_ConnectionString";

        public DatabaseFixture()
        {
            GetConnectionString = new Lazy<string>(() =>
            {
                var connectionString = GetLocalOrContainerConnectionString();
                var upgrader = UpgradeFactory.GetUpgradeEngine(connectionString, x => true, false);
                var result = upgrader.PerformUpgrade();
                if (result.Successful)
                {
                    return connectionString;
                }
                else
                {
                    throw new InvalidOperationException("Couldn't start Squadron SQL server");
                }
            });
        }

        public Lazy<string> GetConnectionString { get; }

        private string GetLocalOrContainerConnectionString()
        {
#if DEBUG
            return Environment.GetEnvironmentVariable(_localConnectionStringName)
                   ?? throw new InvalidOperationException($"{_localConnectionStringName} config not set");
#else
#pragma warning disable VSTHRD002 // Yeah, this is not ideal.
            return CreateDatabaseAsync().Result;
#pragma warning restore VSTHRD002
#endif
        }
    }
}

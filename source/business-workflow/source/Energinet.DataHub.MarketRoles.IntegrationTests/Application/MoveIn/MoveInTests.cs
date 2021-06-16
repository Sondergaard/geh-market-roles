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

using System.Threading.Tasks;
using Energinet.DataHub.MarketRoles.Application.Common;
using Energinet.DataHub.MarketRoles.Application.MoveIn;
using Energinet.DataHub.MarketRoles.Domain.EnergySuppliers;
using Energinet.DataHub.MarketRoles.Domain.MeteringPoints;
using Energinet.DataHub.MarketRoles.Domain.SeedWork;
using Energinet.DataHub.MarketRoles.Infrastructure.EDIMessaging.ENTSOE.CIM.MoveIn;
using Xunit;

namespace Energinet.DataHub.MarketRoles.IntegrationTests.Application.MoveIn
{
    #pragma warning disable
    public class MoveInTests : TestHost
    {
        private readonly AccountingPoint _accountingPoint;

        public MoveInTests()
        {
        }

        [Fact]
        public async Task Accept_WhenConsumerIsRegistered_AcceptMessageIsPublished()
        {
            CreateEnergySupplier();
            CreateAccountingPoint();
            SaveChanges();

            var request = CreateRequest();

            var result = await SendRequest(request);
            var publishedMessage = await GetLastMessageFromOutboxAsync<MoveInRequestAccepted>().ConfigureAwait(false);

            Assert.True(result.Success);
            Assert.NotNull(publishedMessage);
        }

        [Fact]
        public async Task Accept_WhenEnergySupplierDoesNotExists_IsRejected()
        {
            CreateAccountingPoint();
            SaveChanges();

            var request = CreateRequest();

            var result = await SendRequest(request);

            var publishedMessage = await GetLastMessageFromOutboxAsync<MoveInRequestRejected>().ConfigureAwait(false);

            Assert.False(result.Success);
            Assert.NotNull(publishedMessage);
        }

        private RequestMoveIn CreateRequest()
        {
            var consumerSsn = SampleData.ConsumerSSN;
            var moveInDate = GetService<ISystemDateTimeProvider>().Now();
            return new RequestMoveIn(
                SampleData.Transaction,
                SampleData.GlnNumber,
                consumerSsn,
                string.Empty,
                SampleData.ConsumerName,
                SampleData.GsrnNumber,
                moveInDate);
        }
    }
}

// Copyright 2020 Energinet DataHub A/S
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
using System.Threading.Tasks;
using Energinet.DataHub.MarketData.Domain.MeteringPoints;
using Microsoft.EntityFrameworkCore;

namespace Energinet.DataHub.MarketData.Infrastructure.DatabaseAccess.Write.MeteringPoints
{
    public class MeteringPointRepository : IMeteringPointRepository
    {
        private readonly IWriteDatabaseContext _writeDatabaseContext;

        public MeteringPointRepository(IWriteDatabaseContext writeDatabaseContext)
        {
            _writeDatabaseContext = writeDatabaseContext;
        }

        public async Task<AccountingPoint> GetByGsrnNumberAsync(GsrnNumber gsrnNumber)
        {
            if (gsrnNumber is null)
            {
                throw new ArgumentNullException(nameof(gsrnNumber));
            }

            var meteringPoint = await _writeDatabaseContext.AccountingPoints
            .Where(m => m.GsrnNumber == gsrnNumber)
                .FirstOrDefaultAsync();

            return meteringPoint;
        }

        public void Add(AccountingPoint meteringPoint)
        {
            if (meteringPoint is null)
            {
                throw new ArgumentNullException(nameof(meteringPoint));
            }

            _writeDatabaseContext.AccountingPoints.Add(meteringPoint);
        }
    }
}

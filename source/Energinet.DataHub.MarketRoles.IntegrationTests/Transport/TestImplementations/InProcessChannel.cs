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
using Energinet.DataHub.MarketRoles.Infrastructure.Transport;

namespace Energinet.DataHub.MarketRoles.IntegrationTests.Transport.TestImplementations
{
    public class InProcessChannel : Channel
    {
        private byte[]? _writtenBytes;

        public byte[] GetWrittenBytes() => _writtenBytes ?? throw new InvalidOperationException("Write bytes before getting them.");

        public override async Task WriteAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            _writtenBytes = data;
            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}

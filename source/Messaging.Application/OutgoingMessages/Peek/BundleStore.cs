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

using System.IO;
using System.Threading.Tasks;
using Messaging.Domain.Actors;
using Messaging.Domain.OutgoingMessages.Peek;

namespace Messaging.Application.OutgoingMessages.Peek;

public class BundleStore : IBundleStore
{
    public Stream? GetBundleOf(
        string key,
        MessageCategory messageCategory,
        ActorNumber messageReceiverNumber,
        MarketRole roleOfReceiver)
    {
        throw new System.NotImplementedException();
    }

    public void RegisterDocument(string key, Stream document)
    {
        throw new System.NotImplementedException();
    }

    public Task<bool> RegisterKeyAsync(string key)
    {
        throw new System.NotImplementedException();
    }
}

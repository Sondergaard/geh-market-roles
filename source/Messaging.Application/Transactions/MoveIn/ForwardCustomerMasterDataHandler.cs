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
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Messaging.Application.Common;
using Messaging.Application.Configuration;
using Messaging.Application.MasterData;
using Messaging.Application.OutgoingMessages;
using Messaging.Application.OutgoingMessages.CharacteristicsOfACustomerAtAnAp;
using Messaging.Domain.OutgoingMessages;
using Messaging.Domain.Transactions.MoveIn;
using NodaTime.Extensions;

namespace Messaging.Application.Transactions.MoveIn;

public class ForwardCustomerMasterDataHandler : IRequestHandler<ForwardCustomerMasterData, Unit>
{
    private readonly IMoveInTransactionRepository _transactionRepository;
    private readonly IOutgoingMessageStore _outgoingMessageStore;
    private readonly IMarketActivityRecordParser _marketActivityRecordParser;

    public ForwardCustomerMasterDataHandler(
        IMoveInTransactionRepository transactionRepository,
        IOutgoingMessageStore outgoingMessageStore,
        IMarketActivityRecordParser marketActivityRecordParser)
    {
        _transactionRepository = transactionRepository;
        _outgoingMessageStore = outgoingMessageStore;
        _marketActivityRecordParser = marketActivityRecordParser;
    }

    public async Task<Unit> Handle(ForwardCustomerMasterData request, CancellationToken cancellationToken)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        var transaction = _transactionRepository.GetById(request.TransactionId);
        if (transaction is null)
        {
            throw new MoveInException($"Could not find move in transaction '{request.TransactionId}'");
        }

        _outgoingMessageStore.Add(CustomerCharacteristicsMessageFrom(request.CustomerMasterDataContent, transaction));
        transaction.MarkCustomerMasterDataAsSent();
        return await Task.FromResult(Unit.Value).ConfigureAwait(false);
    }

    private static MainAddress CreateAddress(CustomerMasterDataContent masterData)
    {
        return new MainAddress(
            new StreetDetail(
                masterData.Address.StreetCode,
                masterData.Address.StreetName,
                masterData.Address.BuildingNumber,
                masterData.Address.Floor,
                masterData.Address.Room),
            new TownDetail(
                masterData.Address.MunicipalityCode.ToString(CultureInfo.InvariantCulture),
                masterData.Address.City,
                masterData.Address.CitySubDivision,
                masterData.Address.CountryCode),
            masterData.Address.PostCode,
            string.Empty);
    }

    private static OutgoingMessage CreateOutgoingMessage(string id, string documentType, string processType, string receiverId, string @marketActivityRecordPayload)
    {
        return new OutgoingMessage(
            documentType,
            receiverId,
            Guid.NewGuid().ToString(),
            id,
            processType,
            MarketRoles.EnergySupplier,
            DataHubDetails.IdentificationNumber,
            MarketRoles.MeteringPointAdministrator,
            marketActivityRecordPayload,
            null);
    }

    private OutgoingMessage CustomerCharacteristicsMessageFrom(CustomerMasterDataContent requestMasterDataContent, MoveInTransaction transaction)
    {
        var marketEvaluationPoint = CreateMarketEvaluationPoint(requestMasterDataContent, transaction);
        var marketActivityRecord = new MarketActivityRecord(
            Guid.NewGuid().ToString(),
            string.Empty,
            transaction.EffectiveDate,
            marketEvaluationPoint);

        return CreateOutgoingMessage(
            transaction.StartedByMessageId,
            "CharacteristicsOfACustomerAtAnAP",
            "E03",
            transaction.NewEnergySupplierId,
            _marketActivityRecordParser.From(marketActivityRecord));
    }

    private MarketEvaluationPoint CreateMarketEvaluationPoint(CustomerMasterDataContent masterData, MoveInTransaction transaction)
    {
        var address = CreateAddress(masterData);
        var usagePointLocations = CreateUsagePointLocations(masterData);

        return new MarketEvaluationPoint(
            transaction.MarketEvaluationPointId,
            masterData.ElectricalHeating,
            masterData.ElectricalHeatingStart.ToUniversalTime().ToInstant(),
            new MrId(masterData.FirstCustomerId, "ARR"),
            masterData.FirstCustomerName,
            new MrId(masterData.SecondCustomerId, "ARR"),
            masterData.SecondCustomerName,
            masterData.ProtectedName,
            masterData.HasEnergySupplier,
            masterData.SupplyStart.ToUniversalTime().ToInstant(),
            masterData.UsagePoints);
    }

    private UsagePointLocation CreateUsagePointLocations(CustomerMasterDataContent masterData)
    {
        throw new NotImplementedException();
    }
}

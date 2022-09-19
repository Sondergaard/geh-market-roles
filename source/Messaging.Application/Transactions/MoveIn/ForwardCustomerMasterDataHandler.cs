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
using MediatR;
using Messaging.Application.Common;
using Messaging.Application.Configuration;
using Messaging.Application.MasterData;
using Messaging.Application.OutgoingMessages;
using Messaging.Application.OutgoingMessages.CharacteristicsOfACustomerAtAnAp;
using Messaging.Application.OutgoingMessages.Common;
using Messaging.Domain.OutgoingMessages;
using Messaging.Domain.Transactions.MoveIn;

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

    private static OutgoingMessage CreateOutgoingMessage(string id, string processType, string receiverId, string @marketActivityRecordPayload)
    {
        return new OutgoingMessage(
            DocumentType.CharacteristicsOfACustomerAtAnAP,
            receiverId,
            id,
            processType,
            MarketRoles.EnergySupplier,
            DataHubDetails.IdentificationNumber,
            MarketRoles.MeteringPointAdministrator,
            marketActivityRecordPayload,
            null);
    }

    private static MarketEvaluationPoint CreateMarketEvaluationPoint(CustomerMasterDataContent masterData, MoveInTransaction transaction)
    {
        return new MarketEvaluationPoint(
            masterData.MarketEvaluationPoint,
            masterData.ElectricalHeating,
            masterData.ElectricalHeatingStart,
            new MrId(masterData.FirstCustomerId, "ARR"),
            masterData.FirstCustomerName,
            new MrId(masterData.SecondCustomerId, "ARR"),
            masterData.SecondCustomerName,
            masterData.ProtectedName,
            masterData.HasEnergySupplier,
            masterData.SupplyStart,
            masterData.UsagePointLocations);
    }

    private OutgoingMessage CustomerCharacteristicsMessageFrom(CustomerMasterDataContent requestMasterDataContent, MoveInTransaction transaction)
    {
        var marketEvaluationPoint = CreateMarketEvaluationPoint(requestMasterDataContent, transaction);
        var marketActivityRecord = new MarketActivityRecord(
            Guid.NewGuid().ToString(),
            transaction.TransactionId,
            transaction.EffectiveDate,
            marketEvaluationPoint);

        return CreateOutgoingMessage(
            transaction.StartedByMessageId,
            ProcessType.MoveIn.Code,
            transaction.NewEnergySupplierId,
            _marketActivityRecordParser.From(marketActivityRecord));
    }
}

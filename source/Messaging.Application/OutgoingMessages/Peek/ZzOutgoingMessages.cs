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
using Dapper;
using Messaging.Application.Configuration.DataAccess;
using Messaging.Domain.Actors;
using Messaging.Domain.OutgoingMessages;
using Messaging.Domain.OutgoingMessages.Peek;

namespace Messaging.Application.OutgoingMessages.Peek;

public class ZzOutgoingMessages : IOutgoingMessages
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IUnitOfWork _unitOfWork;

    public ZzOutgoingMessages(IDbConnectionFactory dbConnectionFactory, IUnitOfWork unitOfWork)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _unitOfWork = unitOfWork;
    }

    public Task<OutgoingMessage?> GetNextAsync(ActorNumber actorNumber, MessageCategory requestMessageCategory)
    {
        throw new System.NotImplementedException();
    }

    public Task<OutgoingMessage?> GetNextByAsync(DocumentType documentTypeToBundle, string processTypeToBundle, MarketRole actorRoleTypeToBundle)
    {
        throw new System.NotImplementedException();
    }

    public Task EnqueueAsync(OutgoingMessage message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        var sql = @$"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ActorMessageQueue_{message.ReceiverId.Value}' and xtype='U')
        CREATE TABLE [B2B].ActorMessageQueue_{message.ReceiverId.Value}(
        [RecordId]                        [int] IDENTITY (1,1) NOT NULL,
        [Id]                              [uniqueIdentifier] NOT NULL,
        [DocumentType]                    [VARCHAR](255)     NOT NULL,
        [ReceiverId]                      [VARCHAR](255)     NOT NULL,
        [ReceiverRole]                    [VARCHAR](50)      NOT NULL,
        [SenderId]                        [VARCHAR](255)     NOT NULL,
        [SenderRole]                      [VARCHAR](50)      NOT NULL,
        [ProcessType]                     [VARCHAR](50)      NOT NULL,
        [Payload]                         [NVARCHAR](MAX)    NOT NULL,
            CONSTRAINT [PK_ActorMessageQueue_{message.ReceiverId.Value}_Id] PRIMARY KEY NONCLUSTERED
                (
            [Id] ASC
            ) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
            ) ON [PRIMARY];
        INSERT INTO [B2B].[ActorMessageQueue_{message.ReceiverId.Value}] VALUES (@Id, @DocumentType, @ReceiverId, @ReceiverRole, @SenderId, @SenderRole, @ProcessType, @Payload)";

        return _dbConnectionFactory.GetOpenConnection()
            .ExecuteAsync(
                sql,
                new
                {
                    Id = Guid.NewGuid(),
                    DocumentType = message.DocumentType.Name,
                    ReceiverId = message.ReceiverId.Value,
                    ReceiverRole = message.ReceiverRole.Name,
                    SenderId = message.SenderId.Value,
                    SenderRole = message.SenderRole.Name,
                    ProcessType = message.ProcessType,
                    Payload = message.MarketActivityRecordPayload,
                },
                _unitOfWork.CurrentTransaction);
    }
}

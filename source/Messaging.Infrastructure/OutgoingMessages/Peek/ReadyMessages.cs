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
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Messaging.Application.Configuration.DataAccess;
using Messaging.Application.OutgoingMessages.Dequeue;
using Messaging.Application.OutgoingMessages.Peek;
using Messaging.Domain.Actors;
using Messaging.Domain.OutgoingMessages;
using Messaging.Domain.OutgoingMessages.Peek;
using Microsoft.Data.SqlClient;

namespace Messaging.Infrastructure.OutgoingMessages.Peek;

public class ReadyMessages : IReadyMessages
{
    private readonly IDatabaseConnectionFactory _connectionFactory;

    public ReadyMessages(IDatabaseConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> TryAddAsync(ReadyMessage readyMessage)
    {
        ArgumentNullException.ThrowIfNull(readyMessage);

        using var connection = await _connectionFactory.GetConnectionAndOpenAsync().ConfigureAwait(false);
        var insertStatement =
            $"IF NOT EXISTS (SELECT * FROM b2b.ReadyMessages WHERE ReceiverNumber = @ReceiverNumber AND MessageCategory = @MessageCategory)" +
            $"INSERT INTO b2b.ReadyMessages(ReceiverNumber, MessageCategory, Id, MessageIdsIncluded, GeneratedDocument) VALUES(@ReceiverNumber, @MessageCategory, @Id, @MessageIdsIncluded, @GeneratedDocument)";
        using var command = CreateCommand(
            insertStatement,
            new List<KeyValuePair<string, object>>()
            {
                new("@ReceiverNumber", readyMessage.ReceiverNumber.Value),
                new("@MessageCategory", readyMessage.Category.Name),
                new("@GeneratedDocument", readyMessage.GeneratedDocument),
                new("@Id", readyMessage.Id.Value),
                new("@MessageIdsIncluded", string.Join(",", readyMessage.MessageIdsIncluded)),
            },
            connection);

        var result = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        ResetBundleStream(readyMessage.GeneratedDocument);

        return result == 1;
    }

    public async Task<DequeueResult> DequeueAsync(Guid messageId)
    {
        const string deleteStmt = @"
DELETE E FROM [b2b].[EnqueuedMessages] E JOIN
(SELECT EnqueuedMessageId = value FROM [b2b].[ReadyMessages]
CROSS APPLY STRING_SPLIT(MessageIdsIncluded, ',') WHERE Id = @Id) AS P
ON E.Id = P.EnqueuedMessageId;

DELETE FROM [b2b].[ReadyMessages] WHERE Id = @Id;
";

        using var connection = (SqlConnection)await _connectionFactory.GetConnectionAndOpenAsync().ConfigureAwait(false);
        using var transaction = connection.BeginTransaction();
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = deleteStmt;
        command.Parameters.Add(new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = messageId });
        int result;

        try
        {
            result = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            await transaction.CommitAsync().ConfigureAwait(false);
        }
        catch (DbException)
        {
            // Add exception logging
            await transaction.RollbackAsync().ConfigureAwait(false);
            throw; // re-throw exception
        }

        return result > 0 ? new DequeueResult(true) : new DequeueResult(false);
    }

    public virtual async Task<ReadyMessage?> GetAsync(MessageCategory category, ActorNumber receiverNumber)
    {
        ArgumentNullException.ThrowIfNull(category);
        ArgumentNullException.ThrowIfNull(receiverNumber);

        using var connection = await _connectionFactory.GetConnectionAndOpenAsync().ConfigureAwait(false);
        using var command = CreateCommand(
            $"SELECT Id, ReceiverNumber, MessageCategory, MessageIdsIncluded, GeneratedDocument FROM b2b.ReadyMessages WHERE ReceiverNumber = @ReceiverNumber AND MessageCategory = @MessageCategory",
            new List<KeyValuePair<string, object>>
            {
                new("@ReceiverNumber", receiverNumber.Value),
                new("@MessageCategory", category.Name),
            },
            connection);

        using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
        await reader.ReadAsync().ConfigureAwait(false);
        if (reader.HasRows == false)
        {
            return null;
        }

        var id = ReadyMessageId.From(reader.GetGuid(0));
        var messageIdsIncluded = reader
            .GetString(3)
            .Split(",")
            .Select(messageId => Guid.Parse(messageId))
            .AsEnumerable();
        var document = reader.GetStream(4);
        return ReadyMessage.Create(id, receiverNumber, category, messageIdsIncluded, document);
    }

    private static void ResetBundleStream(Stream document)
    {
        document.Position = 0;
    }

    private static SqlCommand CreateCommand(string sqlStatement, List<KeyValuePair<string, object>> parameters, IDbConnection connection)
    {
        var command = connection.CreateCommand();

        command.CommandText = sqlStatement;

        foreach (var parameter in parameters)
        {
            var sqlParameter = new SqlParameter(parameter.Key, parameter.Value);
            command.Parameters.Add(sqlParameter);
        }

        return (SqlCommand)command;
    }
}

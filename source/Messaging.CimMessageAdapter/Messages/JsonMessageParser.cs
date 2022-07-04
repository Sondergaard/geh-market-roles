﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Messaging.Application.IncomingMessages;
using Messaging.Application.IncomingMessages.RequestChangeOfSupplier;
using Messaging.Application.OutgoingMessages;
using Messaging.Application.SchemaStore;
using Messaging.CimMessageAdapter.Errors;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using MessageHeader = Messaging.Application.IncomingMessages.MessageHeader;

namespace Messaging.CimMessageAdapter.Messages;

public class JsonMessageParser : MessageParser
{
    private readonly ISchemaProvider _schemaProvider;

    public JsonMessageParser(string? contentType)
    {
        _schemaProvider = SchemaProviderFactory.GetProvider(contentType);
    }

    public override async Task<MessageParserResult> ParseAsync(Stream message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        string processType;
        try
        {
            processType = GetBusinessProcessType(message);
        }
        catch (JsonException exception)
        {
            return InvalidJsonFailure(exception);
        }

        var schema = await _schemaProvider.GetSchemaAsync<JsonSchema>(processType, "0").ConfigureAwait(false);
        if (schema is null)
        {
            return MessageParserResult.Failure(new UnknownBusinessProcessTypeOrVersion(processType, "0"));
        }

        ResetMessagePosition(message);
        var streamReader = new StreamReader(message, leaveOpen: true);
        try
        {
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                try
                {
                    return ParseJsonData(jsonTextReader);
                }
                catch (JsonException exception)
                {
                    return InvalidJsonFailure(exception);
                }
                catch (ArgumentException argumentException)
                {
                    return InvalidJsonFailure(argumentException);
                }
            }
        }
        catch (IOException e)
        {
            return InvalidJsonFailure(e);
        }
        finally
        {
            streamReader.Dispose();
        }
    }

    protected override string[] SplitNamespace(Stream message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        string[] split;
        ResetMessagePosition(message);
        var streamReader = new StreamReader(message, leaveOpen: true);
        using (var jsonTextReader = new JsonTextReader(streamReader))
        {
            var serializer = new JsonSerializer();
            var deserialized = (JObject)serializer.Deserialize(jsonTextReader);
            var path = deserialized.First.Path;
            split = path.Split('_');
        }

        return split;
    }

    protected override string GetBusinessProcessType(Stream message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        var split = SplitNamespace(message);
        var processType = split[0];
        return processType;
    }

    private static MessageParserResult ParseJsonData(JsonTextReader jsonTextReader)
    {
        var marketActivityRecords = new List<MarketActivityRecord>();
        var serializer = new JsonSerializer();
        var jsonRequest = serializer.Deserialize<JObject>(jsonTextReader);
        var token = jsonRequest.SelectToken("RequestChangeOfSupplier_MarketDocument");
        var messageHeader = MessageHeaderFrom(token);
        marketActivityRecords.AddRange(token["MktActivityRecord"].Select(MarketActivityRecordFrom));

        return MessageParserResult.Succeeded(messageHeader, marketActivityRecords);
    }

    private static void ResetMessagePosition(Stream message)
    {
        if (message.CanRead && message.Position > 0)
            message.Position = 0;
    }

    private static MessageParserResult InvalidJsonFailure(Exception exception)
    {
        return MessageParserResult.Failure(InvalidMessageStructure.From(exception));
    }

    private static MessageHeader MessageHeaderFrom(JToken token)
    {
        return new MessageHeader(
            token["mRID"].ToString(),
            token["process.processType"]["value"].ToString(),
            token["sender_MarketParticipant.mRID"]["value"].ToString(),
            token["sender_MarketParticipant.marketRole.type"]["value"].ToString(),
            token["receiver_MarketParticipant.mRID"]["value"].ToString(),
            token["receiver_MarketParticipant.marketRole.type"]["value"].ToString(),
            token["createdDateTime"].ToString());
    }

    private static MarketActivityRecord MarketActivityRecordFrom(JToken token)
    {
        return new MarketActivityRecord()
        {
            Id = token["mRID"].ToString(),
            ConsumerId = token["marketEvaluationPoint.customer_MarketParticipant.mRID"]["value"].ToString(),
            BalanceResponsibleId = token["marketEvaluationPoint.balanceResponsibleParty_MarketParticipant.mRID"]["value"].ToString(),
            EnergySupplierId = token["marketEvaluationPoint.energySupplier_MarketParticipant.mRID"]["value"].ToString(),
            MarketEvaluationPointId = token["marketEvaluationPoint.mRID"]["value"].ToString(),
            ConsumerName = token["marketEvaluationPoint.customer_MarketParticipant.name"].ToString(),
            EffectiveDate = token["start_DateAndOrTime.dateTime"].ToString(),
        };
    }
}

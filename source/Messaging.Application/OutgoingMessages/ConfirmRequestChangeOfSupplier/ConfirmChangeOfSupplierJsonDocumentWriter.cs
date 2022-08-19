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
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Messaging.Application.Common;
using Messaging.Domain.OutgoingMessages;
using Newtonsoft.Json;

namespace Messaging.Application.OutgoingMessages.ConfirmRequestChangeOfSupplier;

public class ConfirmChangeOfSupplierJsonDocumentWriter : IDocumentWriter
{
    private readonly string _documentType;
    private readonly string _typeCode;
    private readonly IMarketActivityRecordParser _parser;
    //private const string DocumentType = "ConfirmRequestChangeOfSupplier_MarketDocument";
    /*private const string TypeCode = "E44";*/

    public ConfirmChangeOfSupplierJsonDocumentWriter(string documentType, string typeCode, IMarketActivityRecordParser parser)
    {
        _documentType = documentType;
        _typeCode = typeCode;
        _parser = parser;
    }

    public bool HandlesDocumentFormat(CimFormat format)
    {
        return format == CimFormat.Json;
    }

    public bool HandlesDocumentType(string documentType)
    {
        if (documentType == null) throw new ArgumentNullException(nameof(documentType));
        return _documentType.Equals(documentType, StringComparison.OrdinalIgnoreCase);
    }

    public Task<Stream> WriteAsync(MessageHeader header, IReadOnlyCollection<string> marketActivityRecords)
    {
        var stream = new MemoryStream();
        var streamWriter = new StreamWriter(stream);
        using var writer = new JsonTextWriter(streamWriter);

        WriteHeader(header, writer);
        WriteMarketActivityRecords(marketActivityRecords, writer);
        WriteEnd(writer);
        return Task.FromResult<Stream>(stream);
    }

    private static void WriteEnd(JsonTextWriter writer)
    {
        writer.WriteEndObject();
        writer.WriteEndObject();
        writer.Close();
    }

    private void WriteHeader(MessageHeader header, JsonTextWriter writer)
    {
        JsonHeaderWriter.Write(header, _documentType, _typeCode, writer);
    }

    private void WriteMarketActivityRecords(IReadOnlyCollection<string> marketActivityRecords, JsonTextWriter writer)
    {
        if (marketActivityRecords == null) throw new ArgumentNullException(nameof(marketActivityRecords));
        if (writer == null) throw new ArgumentNullException(nameof(writer));

        writer.WritePropertyName("MktActivityRecord");
        writer.WriteStartArray();

        foreach (var marketActivityRecord in ParseFrom<MarketActivityRecord>(marketActivityRecords))
        {
            writer.WriteStartObject();
            writer.WritePropertyName("mRID");
            writer.WriteValue(marketActivityRecord.Id);
            writer.WritePropertyName("marketEvaluationPoint.mRID");
            writer.WriteStartObject();
            writer.WritePropertyName("codingScheme");
            writer.WriteValue("A10");
            writer.WritePropertyName("value");
            writer.WriteValue(marketActivityRecord.MarketEvaluationPointId);
            writer.WriteEndObject();
            writer.WritePropertyName("originalTransactionIDReference_MktActivityRecord.mRID");
            writer.WriteValue(marketActivityRecord.OriginalTransactionId);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    private IReadOnlyCollection<TMarketActivityRecord> ParseFrom<TMarketActivityRecord>(IReadOnlyCollection<string> payloads)
    {
        if (payloads == null) throw new ArgumentNullException(nameof(payloads));
        var marketActivityRecords = new List<TMarketActivityRecord>();
        foreach (var payload in payloads)
        {
            marketActivityRecords.Add(_parser.From<TMarketActivityRecord>(payload));
        }

        return marketActivityRecords;
    }
}

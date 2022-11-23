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
using System.Linq;
using System.Xml.Linq;
using Messaging.Domain.OutgoingMessages;
using Xunit;

namespace Messaging.IntegrationTests.Assertions
{
    internal class AssertXmlMessage
    {
        private const string MarketActivityRecordElementName = "MktActivityRecord";
        private readonly XDocument _document;

        private AssertXmlMessage(XDocument document)
        {
            _document = document;
        }

        internal static AssertXmlMessage Document(XDocument document)
        {
            return new AssertXmlMessage(document);
        }

        internal static string? GetMessageHeaderValue(XDocument document, string elementName)
        {
            var header = GetHeaderElement(document);
            return header?.Element(header.Name.Namespace + elementName)?.Value;
        }

        internal static XElement? GetMarketActivityRecordById(XDocument document, string id)
        {
            var header = document.Root!;
            var ns = header.Name.Namespace;
            return header
                .Elements(ns + MarketActivityRecordElementName)
                .FirstOrDefault(x => x.Element(ns + "mRID")?.Value
                    .Equals(id, StringComparison.OrdinalIgnoreCase) ?? false);
        }

        internal static List<XElement> GetMarketActivityRecords(XDocument document)
        {
            return document.Root?.Elements()
                .Where(x => x.Name.LocalName.Equals(MarketActivityRecordElementName, StringComparison.OrdinalIgnoreCase))
                .ToList() ?? new List<XElement>();
        }

        internal static void AssertHasHeaderValue(XDocument document, string elementName, string? expectedValue)
        {
            Assert.Equal(expectedValue, GetMessageHeaderValue(document, elementName));
        }

        internal static void AssertMarketActivityRecordValue(XElement marketActivityRecord, string elementName, string? expectedValue)
        {
            Assert.Equal(expectedValue, marketActivityRecord.Element(marketActivityRecord.Name.Namespace + elementName)?.Value);
        }

        internal static void AssertMarketActivityRecordCount(XDocument document, int expectedCount)
        {
            Assert.Equal(expectedCount, GetMarketActivityRecords(document).Count);
        }

        internal AssertXmlMessage IsDocumentType(DocumentType documentType)
        {
            Assert.Equal(documentType.Name + "_MarketDocument", _document.Root!.Name.LocalName);
            return this;
        }

        internal AssertXmlMessage IsProcesType(ProcessType processType)
        {
            Assert.Equal(processType.Code, GetMessageHeaderValue("process.processType"));
            return this;
        }

        internal string? GetMessageHeaderValue(string elementName)
        {
            var header = GetHeaderElement(_document);
            return header?.Element(header.Name.Namespace + elementName)?.Value;
        }

        private static XElement? GetHeaderElement(XDocument document)
        {
            return document.Root;
        }
    }
}

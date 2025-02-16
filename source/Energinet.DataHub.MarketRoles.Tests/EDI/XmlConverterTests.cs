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
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Energinet.DataHub.MarketRoles.Application.ChangeOfSupplier;
using Energinet.DataHub.MarketRoles.Application.MoveIn;
using Energinet.DataHub.MarketRoles.Infrastructure.EDI.XmlConverter;
using Energinet.DataHub.MarketRoles.Infrastructure.EDI.XmlConverter.Mappings;
using FluentAssertions;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.MarketRoles.Tests.EDI
{
    [UnitTest]
    public class XmlConverterTests
    {
        private readonly XmlDeserializer _xmlDeserializer;

        public XmlConverterTests()
        {
            var xmlMapper = new XmlMapper(XmlMapperFactory);
            _xmlDeserializer = new XmlDeserializer(xmlMapper);
        }

        [Fact]
        public void AssertConfigurationsValid()
        {
            Action assertConfigurationValid = ConverterMapperConfigurations.AssertConfigurationValid;
            assertConfigurationValid.Should().NotThrow();
        }

        [Fact]
        public async Task ValidateMappingFromChangeOfSupplierCimXml()
        {
            var stream = GetResourceStream("ChangeOfSupplierCimXml.xml");

            var commandsRaw = await _xmlDeserializer.DeserializeAsync(stream).ConfigureAwait(false);
            var commands = commandsRaw.Cast<RequestChangeOfSupplier>();

            var command = commands.First();

            command.SocialSecurityNumber.Should().Be("C87157A9-EC3B-4B2D-B834-8147203EA0FC");
            command.StartDate.Should().Be("1957-08-13");
            command.TransactionId.Should().Be("D4CE11B8-C34B-4931-BD03-BFCEC3B462B9");
            command.EnergySupplierGlnNumber.Should().Be("BAF8F2EA-B7B2-4851-90FD-C7609DE646F6");
            command.AccountingPointGsrnNumber.Should().Be("BD370B4E-28B5-4948-9056-732172CC4B5F");
        }

        [Fact]
        public async Task ValidateMappingFromMoveInCimXml()
        {
            var stream = GetResourceStream("MoveInCimXml.xml");

            var commandsRaw = await _xmlDeserializer.DeserializeAsync(stream).ConfigureAwait(false);
            var commands = commandsRaw.Cast<RequestMoveIn>();

            var command = commands.First();

            command.ConsumerName.Should().Be("Test Name");
            command.MoveInDate.Should().Be("1957-08-13");
            command.TransactionId.Should().Be("8B05EE11-DE70-4820-AF7F-F9F11D0A5614");
            command.EnergySupplierGlnNumber.Should().Be("3CC261B5-F4C5-4A12-9F94-E3050C63B38E");
            command.AccountingPointGsrnNumber.Should().Be("B6953F11-B3F5-48E9-8426-D3448F034614");
            command.VATNumber.Should().BeNullOrEmpty();
            command.SocialSecurityNumber.Should().Be("1234561234");
        }

        private static Stream GetResourceStream(string resourcePath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = new List<string>(assembly.GetManifestResourceNames());

            var resourceName = resourcePath.Replace(@"/", ".", StringComparison.Ordinal);
            var resource = resourceNames.FirstOrDefault(r => r.Contains(resourceName, StringComparison.Ordinal))
                           ?? throw new FileNotFoundException("Resource not found");

            return assembly.GetManifestResourceStream(resource)
                   ?? throw new InvalidOperationException($"Couldn't get requested resource: {resourcePath}");
        }

        private static XmlMappingConfigurationBase XmlMapperFactory(string processType, string type)
        {
            return processType switch
            {
                "E03" => new ChangeOfSupplierXmlMappingConfiguration(),
                "E65" => new RequestMoveInXmlMappingConfiguration(),
                _ => throw new NotImplementedException($"Found no XmlMappingConfiguration matching XML Document with process type {processType} and type {type}"),
            };
        }
    }
}

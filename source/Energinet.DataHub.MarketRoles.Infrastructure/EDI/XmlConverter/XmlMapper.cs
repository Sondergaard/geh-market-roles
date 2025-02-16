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
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Energinet.DataHub.MarketRoles.Application.Common;

namespace Energinet.DataHub.MarketRoles.Infrastructure.EDI.XmlConverter
{
    public class XmlMapper
    {
        private readonly Func<string, string, XmlMappingConfigurationBase> _mappingConfigurationFactory;

        public XmlMapper(Func<string, string, XmlMappingConfigurationBase> mappingConfigurationFactory)
        {
            _mappingConfigurationFactory = mappingConfigurationFactory;
        }

        public IEnumerable<IBusinessRequest> Map(XElement rootElement)
        {
            if (rootElement == null) throw new ArgumentNullException(nameof(rootElement));

            XNamespace ns = rootElement.Attributes().FirstOrDefault(attr => attr.Name.LocalName == "cim")?.Value ?? throw new ArgumentException("Found no namespace for XML Document");

            var headerData = MapHeaderData(rootElement, ns);

            var currentMappingConfiguration = _mappingConfigurationFactory(headerData.ProcessType, headerData.Type);

            var elements = InternalMap(currentMappingConfiguration, rootElement, ns);

            return elements;
        }

        private static XmlHeaderData MapHeaderData(XContainer rootElement, XNamespace ns)
        {
            var mrid = ExtractElementValue(rootElement, ns + "mRID");
            var type = ExtractElementValue(rootElement, ns + "type");
            var processType = ExtractElementValue(rootElement, ns + "process.processType");

            var headerData = new XmlHeaderData(mrid, type, processType);

            return headerData;
        }

        private static string ExtractElementValue(XContainer element, XName name)
        {
            return element.Element(name)?.Value ?? string.Empty;
        }

        private static XElement? GetXmlElement(XContainer? container, Queue<string> hierarchyQueue, XNamespace ns)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));

            var elementName = hierarchyQueue.Dequeue();
            var element = container.Element(ns + elementName);

            if (element is null) return null;

            return hierarchyQueue.Any() ? GetXmlElement(element, hierarchyQueue, ns) : element;
        }

        private static IEnumerable<IBusinessRequest> InternalMap(XmlMappingConfigurationBase xmlMappingConfigurationBase, XElement rootElement, XNamespace ns)
        {
            var configuration = xmlMappingConfigurationBase.Configuration;

            var properties = configuration.Properties;

            var messages = new List<IBusinessRequest>();

            var elements = rootElement.Elements(ns + configuration.XmlElementName);

            foreach (var element in elements)
            {
                var args = properties.Select(property =>
                {
                    if (property.Value is null)
                    {
                        throw new ArgumentNullException($"Missing map for property with name: {property.Key}");
                    }

                    var xmlHierarchyQueue = new Queue<string>(property.Value.XmlHierarchy);
                    var correspondingXmlElement = GetXmlElement(element, xmlHierarchyQueue, ns);

                    return Convert(correspondingXmlElement, property.Value.PropertyInfo.PropertyType, property.Value.TranslatorFunc);
                }).ToArray();

                if (configuration.CreateInstance(args) is not IBusinessRequest instance)
                {
                    throw new InvalidOperationException("Could not create instance");
                }

                messages.Add(instance);
            }

            return messages;
        }

        private static object? Convert(XElement? source, Type dest, Func<XmlElementInfo, object>? valueTranslatorFunc)
        {
            if (source is null) return default;

            var xmlElementInfo = new XmlElementInfo(source.Value, source.Attributes());

            if (dest == typeof(string))
            {
                return valueTranslatorFunc != null ? valueTranslatorFunc(xmlElementInfo) : source.Value;
            }

            if (dest == typeof(bool))
            {
                return valueTranslatorFunc != null ? valueTranslatorFunc(xmlElementInfo) : source.Value;
            }

            return System.Convert.ChangeType(source.Value, dest, CultureInfo.InvariantCulture);
        }
    }
}

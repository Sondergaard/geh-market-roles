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
using System.Collections.ObjectModel;
using Energinet.DataHub.MarketRoles.Domain.Consumers.Rules;
using Energinet.DataHub.MarketRoles.Domain.SeedWork;

namespace Energinet.DataHub.MarketRoles.Domain.Consumers
{
    public class CvrNumber : CustomerId
    {
        public CvrNumber(string value)
        : base(value)
        {
        }

        public static CvrNumber Create(string cvrValue)
        {
            var formattedValue = cvrValue?.Trim();

            if (string.IsNullOrWhiteSpace(formattedValue))
            {
                throw new ArgumentException($"'{nameof(cvrValue)}' cannot be null or whitespace", nameof(cvrValue));
            }

            ThrowIfInvalid(formattedValue);
            return new CvrNumber(formattedValue);
        }

        public static BusinessRulesValidationResult CheckRules(string? cvrValue)
        {
            return new BusinessRulesValidationResult(new Collection<IBusinessRule>()
            {
                new CvrNumberFormatRule(cvrValue),
            });
        }

        private static void ThrowIfInvalid(string cvrValue)
        {
            var result = CheckRules(cvrValue);
            if (!result.Success)
            {
                throw new InvalidCvrNumberRuleException("Invalid CVR number.");
            }
        }
    }
}

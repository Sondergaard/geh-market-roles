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

using Energinet.DataHub.MarketRoles.Domain.Consumers;
using Energinet.DataHub.MarketRoles.Domain.SeedWork;
using FluentValidation;

namespace Energinet.DataHub.MarketRoles.Application.Common.Validation.Consumers
{
    public class VATNumberMustBeValidRule : AbstractValidator<string>
    {
        public VATNumberMustBeValidRule()
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(vatNumber => vatNumber)
                .NotEmpty()
                .WithState(CreateValidationError);

            RuleFor(vatNumber => CvrNumber.CheckRules(vatNumber))
                .Must(x => x.Success)
                .WithState(CreateValidationError);
        }

        private static ValidationError CreateValidationError(string vatNumber)
        {
            return new VATNumberMustBeValidRuleError(vatNumber);
        }
    }
}

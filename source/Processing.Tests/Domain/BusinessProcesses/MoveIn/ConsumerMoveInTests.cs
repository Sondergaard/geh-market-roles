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
using System.Linq;
using NodaTime;
using Processing.Domain.BusinessProcesses.MoveIn;
using Processing.Domain.BusinessProcesses.MoveIn.Errors;
using Processing.Domain.Common;
using Processing.Domain.Consumers;
using Processing.Domain.EnergySuppliers;
using Processing.Domain.MeteringPoints;
using Processing.Domain.MeteringPoints.Events;
using Processing.Domain.MeteringPoints.Rules.ChangeEnergySupplier;
using Processing.Domain.SeedWork;
using Xunit;

namespace Processing.Tests.Domain.BusinessProcesses.MoveIn;

public class ConsumerMoveInTests
{
    private readonly AccountingPoint _accountingPoint;
    private readonly Consumer _consumer;
    private readonly EnergySupplier _energySupplier;
    private readonly ISystemDateTimeProvider _systemDateTimeProvider;
    private readonly ConsumerMoveIn _consumerMoveInProcess;
    private readonly Transaction _transaction;

    public ConsumerMoveInTests()
    {
        _consumerMoveInProcess = new ConsumerMoveIn(EffectiveDatePolicyFactory.CreateEffectiveDatePolicy());
        _systemDateTimeProvider = new SystemDateTimeProviderStub();
        _accountingPoint = AccountingPoint.CreateProduction(GsrnNumber.Create(SampleData.GsrnNumber), true);
        _consumer = new Consumer(ConsumerId.New(), CprNumber.Create(SampleData.ConsumerSocialSecurityNumber), ConsumerName.Create(SampleData.ConsumerName));
        _energySupplier = new EnergySupplier(EnergySupplierId.New(), GlnNumber.Create(SampleData.GlnNumber));
        _transaction = Transaction.Create(SampleData.Transaction);
    }

    [Fact]
    public void Throw_if_any_business_rules_are_broken()
    {
        _consumerMoveInProcess.StartProcess(_accountingPoint, _consumer, _energySupplier, AsOfToday(), _transaction);

        Assert.Throws<BusinessProcessException>(() => _consumerMoveInProcess.StartProcess(_accountingPoint, _consumer, _energySupplier, AsOfToday(), _transaction));
    }

    [Fact]
    public void Consumer_move_in_is_accepted()
    {
        _consumerMoveInProcess.StartProcess(_accountingPoint, _consumer, _energySupplier, AsOfToday(), _transaction);

        Assert.Contains(_accountingPoint.DomainEvents, e => e is ConsumerMoveInAccepted);
    }

    [Fact]
    public void Cannot_move_in_on_a_date_where_a_move_in_is_already_registered()
    {
        var moveInDate = AsOfToday();

        _consumerMoveInProcess.StartProcess(_accountingPoint, _consumer, _energySupplier, moveInDate, _transaction);

        var result = CanStartProcess(moveInDate);
        Assert.Contains(result.Errors, error => error is MoveInRegisteredOnSameDateIsNotAllowedRuleError);
    }

    [Fact]
    public void Cannot_register_a_move_in_on_a_date_where_a_move_in_is_already_effectuated()
    {
        var moveInDate = AsOfToday();
        _consumerMoveInProcess.StartProcess(_accountingPoint, _consumer, _energySupplier, moveInDate, _transaction);
        _accountingPoint.EffectuateConsumerMoveIn(_transaction, _systemDateTimeProvider);

        var result = CanStartProcess(moveInDate);

        Assert.Contains(result.Errors, error => error is MoveInRegisteredOnSameDateIsNotAllowedRuleError);
    }

    [Fact]
    public void Effective_date_must_be_within_allowed_time_range()
    {
        var maxNumberOfDaysAheadOfcurrentDate = 5;
        var policy = EffectiveDatePolicyFactory.CreateEffectiveDatePolicy(maxNumberOfDaysAheadOfcurrentDate);
        var moveProcess = new ConsumerMoveIn(policy);

        var moveInDate = AsOf(_systemDateTimeProvider.Now().Plus(Duration.FromDays(10)));
        var result = moveProcess.CanStartProcess(_accountingPoint, moveInDate, _systemDateTimeProvider.Now());

        AssertValidationError<EffectiveDateIsNotWithinAllowedTimePeriod>(result, "EffectiveDateIsNotWithinAllowedTimePeriod");
    }

    private static void AssertValidationError<TRuleError>(BusinessRulesValidationResult rulesValidationResult, string? expectedErrorCode = null, bool errorExpected = true)
        where TRuleError : ValidationError
    {
        if (rulesValidationResult == null) throw new ArgumentNullException(nameof(rulesValidationResult));
        var error = rulesValidationResult.Errors.FirstOrDefault(error => error is TRuleError);
        if (errorExpected)
        {
            Assert.NotNull(error);
            if (expectedErrorCode is not null)
            {
                Assert.Equal(expectedErrorCode, error?.Code);
            }
        }
        else
        {
            Assert.Null(error);
        }
    }

    private static EffectiveDate AsOf(Instant date)
    {
        var today = date.ToDateTimeUtc();
        var parsed = new DateTime(today.Year, today.Month, today.Day, 22, 0, 0);
        return EffectiveDate.Create(parsed);
    }

    private BusinessRulesValidationResult CanStartProcess(EffectiveDate moveInDate)
    {
        return _consumerMoveInProcess.CanStartProcess(_accountingPoint, moveInDate, _systemDateTimeProvider.Now());
    }

    private EffectiveDate AsOfToday()
    {
        var today = _systemDateTimeProvider.Now().ToDateTimeUtc();
        var parsed = new DateTime(today.Year, today.Month, today.Day, 22, 0, 0);
        return EffectiveDate.Create(parsed);
    }
}

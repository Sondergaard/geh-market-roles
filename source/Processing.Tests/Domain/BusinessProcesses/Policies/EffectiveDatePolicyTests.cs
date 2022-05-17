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
using System.Globalization;
using NodaTime;
using NodaTime.Text;
using Processing.Domain.BusinessProcesses.MoveIn;
using Processing.Domain.BusinessProcesses.MoveIn.Errors;
using Processing.Domain.Common;
using Xunit;
using Xunit.Categories;

namespace Processing.Tests.Domain.BusinessProcesses.Policies
{
    [UnitTest]
    public class EffectiveDatePolicyTests : TestBase
    {
        [Theory]
        [InlineData("2020-12-26T23:00:00Z", 5, true)]
        [InlineData("2020-12-26T23:00:00Z", 10, false)]
        public void Effective_date_is_within_range_of_allowed_number_of_days_before_today(string effectiveDate, int allowedNumberOfDaysBeforeToday, bool expectError)
        {
            const string todayDate = "2021-01-01T11:00:00Z";
            var policy = EffectiveDatePolicyFactory.CreateEffectiveDatePolicy(allowedNumberOfDaysBeforeToday, 0);
            var today = InstantPattern.General.Parse(todayDate).Value;
            var effective = EffectiveDate.Create(effectiveDate);

            var result = policy.Check(today, effective);

            AssertError<EffectiveDateIsNotWithinAllowedTimePeriod>(result, null, expectError);
        }

        [Theory]
        [InlineData("2021-01-10T23:00:00Z", 5, true)]
        [InlineData("2021-01-10T23:00:00Z", 10, false)]
        public void Effective_date_is_within_range_of_allowed_number_of_days_after_today(string effectiveDate, int allowedNumberOfDaysAfterToday, bool expectError)
        {
            const string todayDate = "2021-01-01T11:00:00Z";
            var policy = EffectiveDatePolicyFactory.CreateEffectiveDatePolicy(0, allowedNumberOfDaysAfterToday);
            var today = InstantPattern.General.Parse(todayDate).Value;
            var effective = EffectiveDate.Create(effectiveDate);

            var result = policy.Check(today, effective);

            AssertError<EffectiveDateIsNotWithinAllowedTimePeriod>(result, null, expectError);
        }

        [Fact]
        public void Same_date_is_allowed()
        {
            var policy = EffectiveDatePolicyFactory.CreateEffectiveDatePolicy(10, 10);
            var today = InstantPattern.General.Parse("2021-01-10T10:00:00Z").Value;
            var effective = EffectiveDate.Create("2021-01-10T23:00:00Z");

            var result = policy.Check(today, effective);

            AssertError<EffectiveDateIsNotWithinAllowedTimePeriod>(result, null, false);
        }

        [Theory]
        [InlineData("22:00:00", true)]
        [InlineData("21:00:00", false)]
        public void Time_of_day_must_adhere_to_defined_local_time(string timeOfDay, bool isValid)
        {
            var policy = new EffectiveDatePolicy(0, 0, TimeOfDay.Create(0, 0, 0), DateTimeZoneProviders.Tzdb["Europe/Copenhagen"]);
            var now = SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc();
            int hour = int.Parse(timeOfDay.AsSpan(0, 2), NumberStyles.None, CultureInfo.InvariantCulture);
            int minute = int.Parse(timeOfDay.AsSpan(3, 2), NumberStyles.None, CultureInfo.InvariantCulture);
            int second = int.Parse(timeOfDay.AsSpan(6, 2), NumberStyles.None, CultureInfo.InvariantCulture);

            var dateTime = new DateTime(now.Year, now.Month, now.Day, hour, minute, second);
            var effective = EffectiveDate.Create(dateTime);
            var today = SystemClock.Instance.GetCurrentInstant();

            var result = policy.Check(today, effective);

            AssertError<InvalidEffectiveDateTimeOfDay>(result, "InvalidEffectiveDateTimeOfDay", !isValid);
        }
    }
}

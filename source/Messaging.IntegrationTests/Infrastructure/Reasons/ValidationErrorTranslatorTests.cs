﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Messaging.Application.Common.Reasons;
using Messaging.Application.OutgoingMessages.RejectRequestChangeOfSupplier;
using Messaging.IntegrationTests.Fixtures;
using Xunit;

namespace Messaging.IntegrationTests.Infrastructure.Reasons;

public class ValidationErrorTranslatorTests : TestBase
{
    private readonly IValidationErrorTranslator _validationErrorTranslator;

    public ValidationErrorTranslatorTests(DatabaseFixture databaseFixture)
        : base(databaseFixture)
    {
        _validationErrorTranslator = GetService<IValidationErrorTranslator>();
    }

    [Fact]
    public async Task Translator_can_translate_validation_error_to_reason()
    {
        var validationErrors = new List<string>()
        {
            "CONSUMERDOESNOTEXIST",
        };

        var reasons = await _validationErrorTranslator.TranslateAsync(validationErrors).ConfigureAwait(false);

        Assert.NotEmpty(reasons);
        Assert.Equal("D64", reasons.FirstOrDefault()?.Code);
        Assert.Equal("Mixed", Enum.GetName(typeof(ReasonLanguage), reasons.FirstOrDefault()?.Lang ?? ReasonLanguage.Unknown));
        Assert.Equal("Kundenavn er påkrævet/Consumer name is required", reasons.FirstOrDefault()?.Text);
    }

    [Fact]
    public async Task Translator_can_translate_validation_error_to_reason_by_language()
    {
        var validationErrors = new List<string>()
        {
            "CONSUMERDOESNOTEXIST",
        };

        var reasons = await _validationErrorTranslator.TranslateAsync(validationErrors, "en");

        Assert.NotEmpty(reasons);
        Assert.Equal("D64", reasons.FirstOrDefault()?.Code);
        Assert.Equal("EN", Enum.GetName(typeof(ReasonLanguage), reasons.FirstOrDefault()?.Lang ?? ReasonLanguage.Unknown));
    }
}

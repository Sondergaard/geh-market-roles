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
using System.Net;
using System.Threading.Tasks;
using MediatR;
using Messaging.Application.Transactions.Aggregations;
using Messaging.Infrastructure.Configuration.Serialization;
using Messaging.Infrastructure.Transactions.AggregatedTimeSeries;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Messaging.Api;

public class SendAggregatedTimeSeriesListener
{
    private readonly ISerializer _serializer;
    private readonly IMediator _mediator;
    private readonly FakeAggregatedTimeSeriesResults _aggregatedTimeSeriesResults;

    public SendAggregatedTimeSeriesListener(ISerializer serializer, IAggregatedTimeSeriesResults aggregatedTimeSeriesResults, IMediator mediator)
    {
        _serializer = serializer;
        _mediator = mediator;
        _aggregatedTimeSeriesResults = (FakeAggregatedTimeSeriesResults)aggregatedTimeSeriesResults;
    }

    [Function("SendAggregatedTimeSeries")]
    public async Task<HttpResponseData> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request,
        FunctionContext executionContext)
    {
        ArgumentNullException.ThrowIfNull(request);

        var timeSeriesFromRequest = (AggregatedTimeSeriesResultsDto)await _serializer.DeserializeAsync(request.Body, typeof(AggregatedTimeSeriesResultsDto)).ConfigureAwait(false);
        foreach (var aggregatedTimeSeriesResultDto in timeSeriesFromRequest.Results)
        {
            var testResultId = Guid.NewGuid();
            _aggregatedTimeSeriesResults.Add(testResultId, aggregatedTimeSeriesResultDto);

            await _mediator.Send(new SendAggregatedTimeSeries(testResultId)).ConfigureAwait(false);
        }

        var response = request.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        return response;
    }
}

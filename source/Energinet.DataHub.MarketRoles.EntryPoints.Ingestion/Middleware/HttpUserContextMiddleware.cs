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
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Energinet.DataHub.MarketRoles.Application.Common.Users;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace Energinet.DataHub.MarketRoles.EntryPoints.Ingestion.Middleware
{
    public sealed class HttpUserContextMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly IUserContext _userContext;

        public HttpUserContextMiddleware(
            IUserContext userContext)
        {
            _userContext = userContext;
        }

        public async Task Invoke(FunctionContext context, [NotNull] FunctionExecutionDelegate next)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            // TODO: Update with a proper identity once we know how/what/when.
            _userContext.CurrentUser = new UserIdentity(Id: "Replace me with an identity of the current user");

            await next(context).ConfigureAwait(false);
        }
    }
}

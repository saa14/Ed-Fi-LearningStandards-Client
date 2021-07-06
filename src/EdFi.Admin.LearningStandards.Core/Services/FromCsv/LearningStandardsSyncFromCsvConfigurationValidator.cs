// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Net;
using System.Threading.Tasks;
using EdFi.Admin.LearningStandards.Core.Auth;
using EdFi.Admin.LearningStandards.Core.Configuration;
using EdFi.Admin.LearningStandards.Core.Services.Interfaces.FromCsv;
using Microsoft.Extensions.Logging;

namespace EdFi.Admin.LearningStandards.Core.Services.FromCsv
{
    /// <inheritdoc />
    public class LearningStandardsSyncFromCsvConfigurationValidator : ILearningStandardsSyncFromCsvConfigurationValidator
    {
        private readonly IEdFiOdsApiAuthTokenManagerFactory _edFiOdsApiAuthTokenManagerFactory;

        private readonly ILogger<LearningStandardsSyncFromCsvConfigurationValidator> _logger;

        public LearningStandardsSyncFromCsvConfigurationValidator(
            IEdFiOdsApiAuthTokenManagerFactory edFiOdsApiAuthTokenManagerFactory,
            ILogger<LearningStandardsSyncFromCsvConfigurationValidator> logger)
        {
            _edFiOdsApiAuthTokenManagerFactory = edFiOdsApiAuthTokenManagerFactory;
            _logger = logger;
        }

        public async Task<IResponse> ValidateEdFiOdsApiConfigurationAsync(IEdFiOdsApiConfiguration edFiOdsApiConfiguration)
        {
            try
            {
                string token = await _edFiOdsApiAuthTokenManagerFactory
                    .CreateEdFiOdsApiAuthTokenManager(edFiOdsApiConfiguration)
                    .GetTokenAsync()
                    .ConfigureAwait(false);

                return new ResponseModel(true, string.Empty, token, HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return ex.ToLearningStandardsResponse();
            }
        }
    }
}

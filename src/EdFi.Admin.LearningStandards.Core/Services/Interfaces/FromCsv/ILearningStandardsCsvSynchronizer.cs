// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Threading;
using System.Threading.Tasks;
using EdFi.Admin.LearningStandards.Core.Configuration;

namespace EdFi.Admin.LearningStandards.Core.Services.Interfaces.FromCsv
{
    public interface ILearningStandardsCsvSynchronizer
    {
        Task<IResponse> SynchronizeAsync(
            IEdFiOdsApiConfiguration odsApiConfiguration,
            ILearningStandardsSynchronizationFromCsvOptions options,
            CancellationToken cancellationToken,
            IProgress<LearningStandardsSynchronizerProgressInfo> progress);
    }
}

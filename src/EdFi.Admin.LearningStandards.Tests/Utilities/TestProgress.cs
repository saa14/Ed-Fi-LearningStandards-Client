using System;
using System.Collections.Generic;
using EdFi.Admin.LearningStandards.Core;
using Microsoft.Extensions.Logging;

namespace EdFi.Admin.LearningStandards.Tests.Utilities
{
    public class TestProgress : IProgress<LearningStandardsSynchronizerProgressInfo>
    {
        private readonly ILoggerProvider _loggerProvider;

        public TestProgress(ILoggerProvider loggerProvider)
        {
            _loggerProvider = loggerProvider;
        }

        public List<int> PercentageUpdates { get; set; } = new List<int>();

        public void Report(LearningStandardsSynchronizerProgressInfo value)
        {
            PercentageUpdates.Add(value.CompletedPercentage);
            var logger = _loggerProvider.CreateLogger("LearningStandardsSynchronizerProgressInfo");
            logger.LogInformation($"{value.CompletedPercentage.ToString()}% completed");
        }
    }
}

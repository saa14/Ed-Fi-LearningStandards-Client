// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using CommandLine;

namespace EdFi.Admin.LearningStandards.CLI
{
    [Verb("sync-from-csv", HelpText = "Synchronizes learning standards from provided CSV file, loading them into the specified Ed-Fi ODS API.")]
    public class LearningStandardsCLISyncFromCSVOptions : LearningStandardsCLIBaseOptions
    {
        [Option("ab-connect-id", Required = false, HelpText = "The Academic Benchmarks AB Connect ID to use.")]
        public new string AcademicBenchmarksConnectId { get; set; }

        [Option("ab-connect-key", Required = false, HelpText = "The Academic Benchmarks AB Connect Key to use.")]
        public new string AcademicBenchmarksConnectKey { get; set; }

        [Option("csv-file-path", Required = true, HelpText = "Input CSV file path.")]
        public string InputCsvFullPath { get; set; }

        [Option("resources-meta-data-uri", Required = true, HelpText = "Ed-Fi ODS resources meta data endpoint Uri.")]
        public string ResourcesMetaDataUri { get; set; }

        [Option("force-metadata-reload", Required = false, HelpText = "Instructs the CLI to force meta data reload.")]
        public bool ForceMetaDataReload { get; set; }
    }
}

// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using EdFi.Admin.LearningStandards.Core.Services.Interfaces.FromCsv;

namespace EdFi.Admin.LearningStandards.Core.Services.FromCsv
{
    public class CsvFileProcessor : ICsvFileProcessor
    {
        public IEnumerable<Dictionary<string, string>> GetRows(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Unable to find input CSV file.");
            }

            using var reader = new StreamReader(filePath);
            using var csvReader = new CsvReader(reader);
            csvReader.Read();
            csvReader.ReadHeader();

            var headers = csvReader.Context.HeaderRecord;

            while (csvReader.Read())
                yield return headers.ToDictionary(singleHeader => singleHeader,
                    singleHeader => csvReader[(string) singleHeader]);
        }
    }
}

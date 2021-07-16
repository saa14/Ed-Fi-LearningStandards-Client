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
using MissingFieldException = CsvHelper.MissingFieldException;

namespace EdFi.Admin.LearningStandards.Core.Services.FromCsv
{
    public class CsvFileProcessor : ICsvFileProcessor
    {
        public IList<Exception> InvalidRowsExceptions { get; set; }

        public IEnumerable<Dictionary<string, string>> GetRows(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Unable to find input CSV file.");
            }

            using (var reader = new StreamReader(filePath))
            {
                using (var csvReader = new CsvReader(reader))
                {
                    InvalidRowsExceptions = new List<Exception>();
                    csvReader.Configuration.BadDataFound = context =>
                    {
                        InvalidRowsExceptions.Add(new BadDataException(context, $"Bad data on row number {context.RawRow}"));
                    };

                    csvReader.Configuration.MissingFieldFound = (headerNames, index, context) =>
                    {
                        InvalidRowsExceptions.Add( new MissingFieldException(context, $"Field is missing on row number {context.RawRow}"));
                    };

                    csvReader.Read();
                    csvReader.ReadHeader();
                    var headers = csvReader.Context.HeaderRecord;

                    while (csvReader.Read())
                        yield return headers.ToDictionary(singleHeader => singleHeader,
                            singleHeader => csvReader[(string) singleHeader]);
                }
            }
        }
    }
}

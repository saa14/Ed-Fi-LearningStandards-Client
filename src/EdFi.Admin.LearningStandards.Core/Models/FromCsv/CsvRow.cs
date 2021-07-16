// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Generic;

namespace EdFi.Admin.LearningStandards.Core.Models.FromCsv
{
    public class CsvRow
    {
        private readonly Dictionary<string, string> _csvRow;

        public CsvRow(Dictionary<string, string> csvRow)
        {
            _csvRow = csvRow;
        }

        public string this[string columnName]
        {
            get
            {
                if (!_csvRow.ContainsKey(columnName))
                    throw new Exception($"Missing column(s) in source file: {columnName}");

                return _csvRow[columnName];
            }
        }
    }
}

// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;
using System.Linq;

namespace EdFi.Admin.LearningStandards.Core.Models.FromCsv
{
    public class LearningStandardMetaData
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public bool Required { get; set; }
        public List<LearningStandardMetaData> Children { get; set; } = new List<LearningStandardMetaData>();

        public DataMapper BuildInitialMappings()
        {
            // All array nodes are left with zero initial items.
            return new DataMapper
            {
                Name = Name,
                Children = DataType == "array"
                    ? new List<DataMapper>()
                    : Children.Select(x => x.BuildInitialMappings()).ToList()
            };
        }
    }
}

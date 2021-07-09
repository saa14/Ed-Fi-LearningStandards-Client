// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EdFi.Admin.LearningStandards.Core.Models.FromCsv;
using EdFi.Admin.LearningStandards.Core.Services.Interfaces.FromCsv;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EdFi.Admin.LearningStandards.Core.Services.FromCsv
{
    public class DataMappingProcess : IDataMappingProcess
    {
        private readonly string DataMappingsFilePath = ".//Mappings.json";

        public JObject ApplyMap(IEnumerable<LearningStandardMetaData> learningStandardMetaData,
            IEnumerable<DataMapper> dataMappers, Dictionary<string, string> csvRow)
        {
            var safeCsvRow = new CsvRow(csvRow);

            return MapToJsonObject(learningStandardMetaData.ToList(), dataMappers.ToList(), safeCsvRow);
        }

        public IEnumerable<DataMapper> GetDataMappings()
        {
            if (File.Exists(DataMappingsFilePath))
            {
               return JsonConvert.DeserializeObject<List<DataMapper>>(File.ReadAllText(DataMappingsFilePath));
            }

            throw new FileNotFoundException($"Unable to find data mapping file {DataMappingsFilePath}");
        }

        public JObject MapToJsonObject(IReadOnlyList<LearningStandardMetaData> nodeMetadatas, IReadOnlyList<DataMapper> metadata,
           CsvRow csvRow)
        {
            var result = new JObject();

            foreach (var node in metadata)
            {
                var nodeMetadata = nodeMetadatas.Single(m => m.Name == node.Name);


                if (nodeMetadata.DataType == "array")
                {
                    var arrayItemMetadata = nodeMetadata.Children.Single();

                    var jsonArray = MapToJsonArray(arrayItemMetadata, node.Children, csvRow);

                    if (jsonArray.HasValues)
                        result.Add(new JProperty(nodeMetadata.Name, jsonArray));
                }

                else if (nodeMetadata.Children.Any())
                {
                    var jsonObject = MapToJsonObject(nodeMetadata.Children, node.Children, csvRow);

                    if (jsonObject.HasValues)
                        result.Add(new JProperty(nodeMetadata.Name, jsonObject));
                }
                else
                {
                    var value = MapToValue(nodeMetadata, node, csvRow);

                    if (value != null)
                        result.Add(new JProperty(nodeMetadata.Name, value));
                }
            }

            return result;
        }

        private JArray MapToJsonArray(LearningStandardMetaData arrayItemMetadata, IReadOnlyList<DataMapper> nodes, CsvRow csvRow)
        {
            var result = new JArray();

            foreach (var node in nodes)
            {
                if (arrayItemMetadata.DataType == "array")
                {
                    var nestedArrayItemMetadata = arrayItemMetadata.Children.Single();

                    var jsonArray = MapToJsonArray(nestedArrayItemMetadata, node.Children, csvRow);

                    if (jsonArray.HasValues)
                        result.Add(jsonArray);
                }
                else if (arrayItemMetadata.Children.Any())
                {
                    var jsonObject = MapToJsonObject(arrayItemMetadata.Children, node.Children, csvRow);

                    if (jsonObject.HasValues)
                    {
                        var requiredPropertiesForArrayItemInclusion =
                            arrayItemMetadata.Children.Where(x => x.Required).Select(x => x.Name);

                        var actualProperties = jsonObject.Children<JProperty>().Select(x => x.Name);

                        bool include = requiredPropertiesForArrayItemInclusion.All(x => actualProperties.Contains(x));

                        if (include)
                        {
                            result.Add(jsonObject);
                        }
                    }
                }
                else
                {
                    var value = MapToValue(arrayItemMetadata, node, csvRow);

                    if (value != null)
                        result.Add(value);
                }
            }

            return result;
        }

        private object MapToValue(LearningStandardMetaData nodeMetadata, DataMapper node, CsvRow csvRow)
        {
            string rawValue = RawValue(node, csvRow);

            return ConvertDataType(nodeMetadata, node, rawValue);
        }

        private string RawValue(DataMapper node, CsvRow csvRow)
        {
            if (string.IsNullOrWhiteSpace(node.SourceColumn))
                return !string.IsNullOrWhiteSpace(node.Value) ? node.Value : null;
            string valueFromCsv = csvRow[node.SourceColumn];

            string rawValue = string.IsNullOrWhiteSpace(valueFromCsv) ? null : valueFromCsv;

            if (string.IsNullOrWhiteSpace(rawValue) && !string.IsNullOrWhiteSpace(node.Default))
                rawValue = node.Default;

            return rawValue;

        }

        private object ConvertDataType(LearningStandardMetaData nodeMetadata, DataMapper node, string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
                return null;

            switch (nodeMetadata.DataType)
            {
                case "string":
                case "date-time":
                    return rawValue;

                case "integer":
                    if (int.TryParse(rawValue, out int intValue))
                        return intValue;
                    throw new TypeConversionException(node, nodeMetadata.DataType);

                case "boolean":
                    if (bool.TryParse(rawValue, out bool boolValue))
                        return boolValue;
                    throw new TypeConversionException(node, nodeMetadata.DataType);

                default:
                    throw new TypeConversionException(node, nodeMetadata.DataType, typeIsUnsupported: true);
            }
        }
    }
}

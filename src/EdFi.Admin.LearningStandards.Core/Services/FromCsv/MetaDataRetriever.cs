// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EdFi.Admin.LearningStandards.Core.Models.FromCsv;
using EdFi.Admin.LearningStandards.Core.Services.Interfaces.FromCsv;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EdFi.Admin.LearningStandards.Core.Services.FromCsv
{
    public class MetaDataRetriever : IMetaDataRetriever
    {
        private readonly ILogger<MetaDataRetriever> _logger;
        private readonly string _folder = ".//MetaData";


        public MetaDataRetriever(ILogger<MetaDataRetriever> logger)
        {
            _logger = logger;
        }

        private async Task<string> LoadJsonString(string metaDataUri)
        {
            string swaggerDocument;
            using var webClient = new WebClient();
            {
                _logger.LogInformation($"Loading swagger document from {metaDataUri}.");
                swaggerDocument = await webClient.DownloadStringTaskAsync(new Uri(metaDataUri));
            }
            return swaggerDocument;
        }

        public async Task LoadMetaData(string metaDataUri)
        {
            if (MetadataExists)
            {
                File.Delete(Filename);
            }

            string swaggerDocument = await LoadJsonString(metaDataUri);
            dynamic resources = JObject.Parse(swaggerDocument);
            var resourceDefinitions = resources.definitions;
            var learningStandardsResourceDefinition = resources.definitions.edFi_learningStandard;

            _logger.LogInformation("Converting swagger meta data.");
            var learningStandardMetaData =
                ConvertToLearningStandardMetaData(learningStandardsResourceDefinition, resourceDefinitions);
            if (!Directory.Exists(_folder))
            {
                Directory.CreateDirectory(_folder);
            }

            _logger.LogInformation("Writing to metadata.json file.");
            using var writer = new StreamWriter(Filename);
            await writer.WriteLineAsync(JsonConvert.SerializeObject(learningStandardMetaData, Formatting.None))
                .ConfigureAwait(false);
        }

        private string Filename => Path.Combine(_folder, "metadata.json");

        private bool MetadataExists => File.Exists(Filename);

        public async Task<IEnumerable<LearningStandardMetaData>> GetMetadata(string metaDataUri, bool forceReload)
        {
            if (!MetadataExists || forceReload)
            {
                await LoadMetaData(metaDataUri).ConfigureAwait(false);
            }

            return await ReadMetaData();
        }

        private async Task<IEnumerable<LearningStandardMetaData>> ReadMetaData()
        {
            var result = new List<LearningStandardMetaData>();

            if (!MetadataExists)
            {
                return result;
            }
            _logger.LogInformation("Reading metadata.json file.");
            using var reader = new StreamReader(Filename);

            string resourceMetadata = await reader.ReadToEndAsync()
                .ConfigureAwait(false);
            var properties = JsonConvert.DeserializeObject<LearningStandardMetaData[]>(resourceMetadata);
            result.AddRange(properties);
            reader.Close();
            return result;
        }

        private IEnumerable<LearningStandardMetaData> ConvertToLearningStandardMetaData(JObject resourceJson, JObject fullResourceJson = null)
        {
            fullResourceJson ??= resourceJson;
            var requiredProperties = resourceJson["required"] != null
                ? resourceJson["required"].Select(x => x.ToString()).ToArray()
                : new string[0];

            foreach (var propertyToken in resourceJson["properties"])
            {
                var property = (JProperty)propertyToken;
                var field = new LearningStandardMetaData
                {
                    Name = property.Name,
                    Required = requiredProperties.Contains(property.Name)
                };

                foreach (var detailToken in property)
                {
                    var detail = (JObject)detailToken;
                    field.DataType = detail["$ref"] != null
                        ? GetReferenceName(detail["$ref"])
                        : detail["type"].ToString();

                    if (field.DataType == "array")
                    {
                        string subModel = GetReferenceName(detail["items"]["$ref"]);
                        var subModelJson = fullResourceJson[subModel];

                        field.Children.Add(new LearningStandardMetaData
                        {
                            Name = GetFormattedModelName(subModel),
                            DataType = subModel,
                            Children = ConvertToLearningStandardMetaData((JObject)subModelJson, fullResourceJson).ToList()
                        });
                    }
                    else
                    {
                        string subModel = field.DataType;
                        var subTypeJson = fullResourceJson[subModel];

                        bool fieldTypeHasModelDefinition = subTypeJson != null;

                        if (fieldTypeHasModelDefinition)
                            field.Children = ConvertToLearningStandardMetaData((JObject)subTypeJson, fullResourceJson).ToList();
                    }
                }

                if (IsApplicableField(field))
                    yield return field;
            }
        }

        private readonly string[] _ignoredFields = { "id", "link", "_etag" };

        private bool IsApplicableField(LearningStandardMetaData field)
        {
            return !_ignoredFields.Contains(field.Name);
        }

        private string GetReferenceName(JToken reference)
            => reference.Value<string>().Replace("#/definitions/", "");

        private string GetFormattedModelName(string resource) => resource.Substring(resource.IndexOf('_') + 1);
    }
}

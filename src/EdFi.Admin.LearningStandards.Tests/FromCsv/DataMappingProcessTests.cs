using System;
using System.Collections.Generic;
using System.Linq;
using EdFi.Admin.LearningStandards.Core.Models.FromCsv;
using EdFi.Admin.LearningStandards.Core.Services.FromCsv;
using NUnit.Framework;

namespace EdFi.Admin.LearningStandards.Tests.FromCsv
{
    [TestFixture]
    public class DataMappingProcessTests
    {
        private readonly DataMappingProcess _mapper = new DataMappingProcess();

        [Test]
        public void Will_map_data_from_source_column()
        {
            var resourceMetadata = Metadata(
                Property("PropertyA", "string"),
                Property("PropertyB", "string"),
                Property("PropertyC", "string")
            );

            var jsonMap = JsonMap(
                MapColumn("PropertyA", "Col1"),
                MapColumn("PropertyB", "Col2", "default value"),
                MapColumn("PropertyC", "Col3", "default value")
            );

            var csvRow = new Dictionary<string, string> { { "Col1", "value1" }, { "Col2", "value2" }, { "Col3", "" } };
            var mappedData = _mapper.ApplyMap(resourceMetadata, jsonMap, csvRow);
            Assert.AreEqual("value1", mappedData["PropertyA"].ToString());
            Assert.AreEqual("default value", mappedData["PropertyC"].ToString());
        }

        [Test]
        public void Will_map_known_data_type_successfully()
        {
            var resourceMetadata = Metadata(
                Property("PropertyA", "string"),
                Property("PropertyB", "date-time"),
                Property("PropertyC", "integer"),
                Property("PropertyD", "boolean")
            );

            var jsonMap = JsonMap(
                MapColumn("PropertyA", "Col1"),
                MapColumn("PropertyB", "Col2"),
                MapColumn("PropertyC", "Col3"),
                MapColumn("PropertyD", "Col4")
            );

            var csvRow = new Dictionary<string, string> { { "Col1", "ABC123" }, { "Col2", "2016-08-01" }, { "Col3", "123" }, { "Col4", "true" } };

            var mappedData = _mapper.ApplyMap(resourceMetadata, jsonMap, csvRow);
            Assert.AreEqual("ABC123", mappedData["PropertyA"].ToString());
            Assert.AreEqual("2016-08-01", mappedData["PropertyB"].ToString());
            Assert.AreEqual("123", mappedData["PropertyC"].ToString());
            Assert.AreEqual("True", mappedData["PropertyD"].ToString());
        }

        [Test]
        public void Will_throw_error_for_mismatch_data_type()
        {
            var resourceMetadata = Metadata(
                Property("PropertyA", "string"),
                Property("PropertyB", "date-time"),
                Property("PropertyC", "integer"),
                Property("PropertyD", "boolean")
            );

            var jsonMap = JsonMap(
                MapColumn("PropertyA", "Col1"),
                MapColumn("PropertyB", "Col2"),
                MapColumn("PropertyC", "Col3"),
                MapColumn("PropertyD", "Col4")
            );

            var csvRow = new Dictionary<string, string> { { "Col1", "ABC123" }, { "Col2", "2016-08-01" }, { "Col3", "wrong-value" }, { "Col4", "true" } };

            Assert.Throws<TypeConversionException>(() =>
            {
                _mapper.ApplyMap(resourceMetadata, jsonMap, csvRow);
            });
        }

        [Test]
        public void Will_throw_error_for_unsupported_data_type()
        {
            var resourceMetadata = Metadata(
                Property("PropertyA", "number")
            );

            var jsonMap = JsonMap(
                MapColumn("PropertyA", "Col1")
            );

            var csvRow = new Dictionary<string, string> { { "Col1", "123.4" }};

            Assert.Throws<TypeConversionException>(() =>
            {
               _mapper.ApplyMap(resourceMetadata, jsonMap, csvRow);
            });
        }

        [Test]
        public void Will_throw_error_for_missing_required_column()
        {
            var resourceMetadata = Metadata(
                Property("PropertyName", "string")
            );

            var jsonMap = JsonMap(
                MapColumn("PropertyName", "MissingColumn")
            );

            var csvRow = new Dictionary<string, string> { { "Col1", "value1" } };

            Assert.Throws<Exception>(() =>
            {
               _mapper.ApplyMap(resourceMetadata, jsonMap, csvRow);
            }, "Missing column(s) in source file: MissingColumn");
        }

        [Test]
        public void Will_map_array_of_objects()
        {
            var resourceMetadata = Metadata(
                Array(
                    "arrayProperty",
                    Object("arrayItem", "arrayItemType", Property("title", "string"))
                ));

            var jsonMap = JsonMap(
                MapArray(
                    "arrayProperty",
                    MapObject("arrayItem", MapColumn("title", "Col1")),
                    MapObject("arrayItem", MapColumn("title", "Col2"))
                ));

            var csvRow = new Dictionary<string, string> { { "Col1", "value1" }, { "Col2", "value2" } };

            var mappedData = _mapper.ApplyMap(resourceMetadata, jsonMap, csvRow);
            Assert.NotNull(mappedData);
            Assert.AreEqual(2, mappedData["arrayProperty"].Count());
            Assert.AreEqual("value1", mappedData["arrayProperty"][0]["title"].ToString());
            Assert.AreEqual("value2", mappedData["arrayProperty"][1]["title"].ToString());
        }

        [Test]
        public void Will_map_nested_objects()
        {
            var resourceMetadata = Metadata(Object("Parent", "Parent",
                Object("ChildObject", "ChildObject",
                    Object("GrandchildObject", "GrandchildObject",

                        Object("GreatGrandchildObject", "GreatGrandchildObject",
                            Property("GreatGreatGrandchildProperty", "string")),

                        Property("GreatGrandchildProperty", "string")
                    ),

                    Property("GrandchildProperty", "string")
                ),

                Property("ChildProperty", "string")
            ));

            var jsonMap = JsonMap(MapObject("Parent",
                MapObject("ChildObject",
                    MapObject("GrandchildObject",

                        MapObject("GreatGrandchildObject",
                            MapColumn("GreatGreatGrandchildProperty", "Col1")),

                        MapColumn("GreatGrandchildProperty", "Col2")
                    ),

                    MapColumn("GrandchildProperty", "Col3")
                ),

                MapColumn("ChildProperty", "Col4")
            ));

            var csvRow = new Dictionary<string, string>
                { { "Col1", "value1" }, { "Col2", "value2" }, { "Col3", "value3" }, { "Col4", "value4" } };

            var mappedData = _mapper.ApplyMap(resourceMetadata, jsonMap, csvRow);
            Assert.NotNull(mappedData);
            Assert.AreEqual("value1",
                mappedData["Parent"]["ChildObject"]["GrandchildObject"]["GreatGrandchildObject"][
                    "GreatGreatGrandchildProperty"].ToString());
            Assert.AreEqual("value3",
                mappedData["Parent"]["ChildObject"]["GrandchildProperty"].ToString());
        }

        [Test]
        public void Will_handle_missing_values_while_mapping_properties()
        {
            var csvRow = new Dictionary<string, string>
            {
                { "EmptyColumn", "" },
                { "PopulatedColumn", "PopulatedCellValue" }
            };

            var resourceMetadata = Metadata(

                //Direct Column Reference to Empty Cells
                Property("EmptyColumn_Null", "string"),
                Property("EmptyColumn_Empty", "string"),

                //Direct Column Reference to Populated Cells
                Property("PopulatedColumn_Null", "string"),
                Property("PopulatedColumn_Empty", "string")
            );

            var jsonMap = JsonMap(

                //Direct Column Reference to Empty Cells
                MapColumn("EmptyColumn_Null", "EmptyColumn", @default: null),
                MapColumn("EmptyColumn_Empty", "EmptyColumn", @default: ""),

                //Direct Column Reference to Populated Cells
                MapColumn("PopulatedColumn_Null", "PopulatedColumn", @default: null),
                MapColumn("PopulatedColumn_Empty", "PopulatedColumn", @default: "")
            );

            var mappedData = _mapper.ApplyMap(resourceMetadata, jsonMap, csvRow);
            Assert.NotNull(mappedData);

            // Ignore property with null or empty value
            Assert.IsNull(mappedData["EmptyColumn_Null"]);
            Assert.IsNull(mappedData["EmptyColumn_Empty"]);

            Assert.AreEqual("PopulatedCellValue", mappedData["PopulatedColumn_Null"].ToString());
            Assert.AreEqual("PopulatedCellValue", mappedData["PopulatedColumn_Empty"].ToString());
        }

        [Test]
        public void Will_handle_unmapped_properties()
        {
            var resourceMetadata = Metadata(
                Property("mappedProperty", "string"),
                Property("unmappedProperty1", "string"),
                Property("unmappedProperty2", "string")
            );
            var jsonMap = JsonMap(
                MapColumn("mappedProperty", "Col1"),

                //Unmapped properties may exist because the Swagger metadata describes them, even
                //if no mapping information provided.
                Unmapped("unmappedProperty1"),
                Unmapped("unmappedProperty2")
            );

            var csvRow = new Dictionary<string, string>
                { { "Col1", "mappedValue" }, { "Col2", "value2" }, { "Col3", "value3" }, { "Col4", "value4" } };

            var mappedData = _mapper.ApplyMap(resourceMetadata, jsonMap, csvRow);
            Assert.NotNull(mappedData);

            // Ignore unmapped properties
            Assert.IsNull(mappedData["unmappedProperty1"]);
            Assert.IsNull(mappedData["unmappedProperty2"]);

            Assert.AreEqual("mappedValue", mappedData["mappedProperty"].ToString());
        }

        [Test]
        public void Will_map_nested_resources()
        {
            var resourceMetadata = Metadata(
                Object("resource1Reference", "resource1Reference",
                    Property("resource1Id", "integer")),

                Object("resource2Reference", "resource2Reference",
                    Property("resource2Id", "string")),

                Property("entryDate", "date-time"),

                Property("resourceDescriptor", "string")
            );
            var jsonMap = JsonMap(
                MapObject("resource1Reference",
                    MapColumn("resource1Id", "ResourceOneId")),

                MapObject("resource2Reference",
                    MapColumn("resource2Id", "ResourceTwoId")),

                MapColumn("entryDate", "EntryDate"),

                MapColumn("resourceDescriptor", "Descriptor")
            );

            var csvRow = new Dictionary<string, string>
            {
                { "ResourceOneId", "123" }, { "ResourceTwoId", "456-Id" }, { "EntryDate", "2016-08-01" },
                { "Descriptor", "Descriptor#Value" }
            };

            var mappedData = _mapper.ApplyMap(resourceMetadata, jsonMap, csvRow);
            Assert.NotNull(mappedData);
            Assert.AreEqual("123",
                mappedData["resource1Reference"]["resource1Id"].ToString());
            Assert.AreEqual("456-Id",
                mappedData["resource2Reference"]["resource2Id"].ToString());
            Assert.AreEqual("2016-08-01",
                mappedData["entryDate"].ToString());
            Assert.AreEqual("Descriptor#Value",
                mappedData["resourceDescriptor"].ToString());
        }

        public static DataMapper Unmapped(string name)
        {
            return new DataMapper
            {
                Name = name,
                SourceColumn = null,
                Value = null,
                Default = null
            };
        }

        public static DataMapper MapArray(string name, params DataMapper[] items)
        {
            return new DataMapper
            {
                Name = name,
                SourceColumn = null,
                Value = null,
                Default = null,
                Children = items.ToList()
            };
        }

        public static DataMapper MapObject(string name, params DataMapper[] properties)
        {
            return new DataMapper
            {
                Name = name,
                SourceColumn = null,
                Value = null,
                Default = null,
                Children = properties.ToList()
            };
        }

        public static LearningStandardMetaData Array(string name, LearningStandardMetaData itemMetadata)
        {
            return new LearningStandardMetaData
            {
                Name = name,
                DataType = "array",
                Children = new List<LearningStandardMetaData> { itemMetadata }
            };
        }

        public static LearningStandardMetaData Object(string name, string dataType, params LearningStandardMetaData[] properties)
        {
            return new LearningStandardMetaData
            {
                Name = name,
                DataType = dataType,
                Children = properties.ToList()
            };
        }

        private static LearningStandardMetaData[] Metadata(params LearningStandardMetaData[] metadata)
        {
            return metadata;
        }

        private static DataMapper[] JsonMap(params DataMapper[] dataMappers)
        {
            return dataMappers;
        }

        public static LearningStandardMetaData RequiredProperty(string name, string dataType)
        {
            return new LearningStandardMetaData
            {
                Name = name,
                DataType = dataType,
                Required = true
            };
        }

        public static DataMapper MapColumn(string name, string sourceColumn, string @default = null)
        {
            return new DataMapper
            {
                Name = name,
                SourceColumn = sourceColumn,
                Value = null,
                Default = @default
            };
        }

        public static LearningStandardMetaData Property(string name, string dataType)
        {
            return new LearningStandardMetaData
            {
                Name = name,
                DataType = dataType
            };
        }
    }
}

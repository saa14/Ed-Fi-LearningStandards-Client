using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EdFi.Admin.LearningStandards.Core;
using EdFi.Admin.LearningStandards.Core.Services.FromCsv;
using EdFi.Admin.LearningStandards.Core.Services.Interfaces.FromCsv;
using EdFi.Admin.LearningStandards.Tests.Utilities;
using Moq;
using NUnit.Framework;

namespace EdFi.Admin.LearningStandards.Tests.FromCsv
{
    [TestFixture]
    public class CsvLearningStandardRetrieverTests
    {
        private CsvLearningStandardsDataRetriever _csvLearningStandardsDataRetriever;
        private readonly string _metadataUri = "https://ed-fi-ods.org/metadata/data/v3/resources/swagger.json";
        private readonly string _folderPath = "TestFiles\\Sample-metadata";
        private DataMappingProcess _dataMappingProcess;
        private MetaDataRetriever _metaDataRetriever;
        private readonly NUnitConsoleLogger<CsvLearningStandardsDataRetriever> _logger = new NUnitConsoleLogger<CsvLearningStandardsDataRetriever>();

        [SetUp]
        public void Init()
        {
            var mockSwaggerDocRetriever = new Mock<ISwaggerDocumentRetriever>();
            string swaggerDocument = TestHelpers.ReadTestFile($"{_folderPath}\\Swagger-Document-3.x.json");
            mockSwaggerDocRetriever.Setup(x => x.LoadJsonString(_metadataUri))
                .ReturnsAsync(swaggerDocument);
            _metaDataRetriever = new MetaDataRetriever(new NUnitConsoleLogger<MetaDataRetriever>(), mockSwaggerDocRetriever.Object);
            var csvFileProcessor = new CsvFileProcessor();
            _dataMappingProcess = new DataMappingProcess
            {
                DataMappingsFilePath = Path.Combine(_folderPath, "Mappings.json")
            };
            _csvLearningStandardsDataRetriever = new CsvLearningStandardsDataRetriever(_logger, _metaDataRetriever, csvFileProcessor, _dataMappingProcess);
        }

        [Test]
        public async Task Will_return_expected_mapped_learning_standard_rows()
        {
            //Arrange
            var options =
                new LearningStandardsSynchronizationFromCsvOptions
                {
                    ForceMetaDataReload = false,
                    InputCsvFullPath = Path.Combine(_folderPath, "test-data.csv"),
                    ResourcesMetaDataUri = _metadataUri
                };

            //Act
            var response =
                await _csvLearningStandardsDataRetriever.GetLearningStandards(options, CancellationToken.None);
            var result = response.AsyncEntityEnumerable.GetAsyncEnumeratorAsync();

            //Assert
            Assert.NotNull(result);

            while (result.Result.MoveNextAsync().Result)
            {
                var actual = result.Result.Current;
                Assert.IsInstanceOf<EdFiBulkJsonModel>(actual);
                Assert.IsNotNull(actual.Operation);
                Assert.IsNotNull(actual.Resource);
                Assert.IsNotNull(actual.Data);
                Assert.AreEqual(2, actual.Data.Count);
                Assert.AreEqual("LSCSVId-1234", actual.Data[0]["learningStandardId"].ToString());
                Assert.AreEqual("LSCSVId-5678", actual.Data[1]["learningStandardId"].ToString());
            }
        }

        [Test]
        public async Task Will_return_expected_mapped_learning_standard_rows_and_count()
        {
            //Arrange
            var options =
                new LearningStandardsSynchronizationFromCsvOptions
                {
                    ForceMetaDataReload = false,
                    InputCsvFullPath = Path.Combine(_folderPath, "test-data.csv"),
                    ResourcesMetaDataUri = _metadataUri
                };
            var mockCsvFileProcessor = new Mock<ICsvFileProcessor>();
            const int expectedRecordsCount = 527;
            mockCsvFileProcessor.Setup(x => x.GetRows(It.IsAny<string>())).Returns(TestHelpers.GenerateDataRows(expectedRecordsCount));
            _csvLearningStandardsDataRetriever = new CsvLearningStandardsDataRetriever(_logger, _metaDataRetriever, mockCsvFileProcessor.Object, _dataMappingProcess);

            //Act
            var response =
                await _csvLearningStandardsDataRetriever.GetLearningStandards(options, CancellationToken.None);
            var result = response.AsyncEntityEnumerable.GetAsyncEnumeratorAsync();

            //Assert
            Assert.NotNull(result);
            // Splitting 100 rows per batch. So for 527 rows, expecting 6 batches 
            const int expectedBatchCount = 6;
            int batchCount = 0;
            int totalCount = 0;

            while (result.Result.MoveNextAsync().Result)
            {
                batchCount++;
                var actual = result.Result.Current;
                Assert.IsInstanceOf<EdFiBulkJsonModel>(actual);
                Assert.IsNotNull(actual.Operation);
                Assert.IsNotNull(actual.Resource);
                Assert.IsNotNull(actual.Data);
                totalCount += actual.Data.Count;
            }
            Assert.AreEqual(expectedBatchCount, batchCount);
            Assert.AreEqual(expectedRecordsCount, totalCount);
        }

        [Test]
        public void Will_throw_error_with_no_records()
        {
            var options =
                new LearningStandardsSynchronizationFromCsvOptions
                {
                    ForceMetaDataReload = false,
                    InputCsvFullPath = Path.Combine(_folderPath, "test-no-data.csv"),
                    ResourcesMetaDataUri = _metadataUri
                };
            Assert.ThrowsAsync<Exception>(async () =>
            {
                await _csvLearningStandardsDataRetriever.GetLearningStandards(options,
                    CancellationToken.None);
            });
        }

        [Test]
        public void Will_throw_error_with_invalid_rows()
        {
            var options =
                new LearningStandardsSynchronizationFromCsvOptions
                {
                    ForceMetaDataReload = false,
                    InputCsvFullPath = Path.Combine(_folderPath, "test-invalid-rows.csv"),
                    ResourcesMetaDataUri = _metadataUri
                };
            Assert.ThrowsAsync<Exception>(async () =>
            {
                await _csvLearningStandardsDataRetriever.GetLearningStandards(options,
                    CancellationToken.None);
            }, "Invalid rows found on file: {options.InputCsvFullPath}. Invalid rows details: Field is missing on row number 2");
        }
    }
}

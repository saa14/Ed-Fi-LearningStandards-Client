using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EdFi.Admin.LearningStandards.Core.Services.FromCsv;
using EdFi.Admin.LearningStandards.Core.Services.Interfaces.FromCsv;
using EdFi.Admin.LearningStandards.Tests.Utilities;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace EdFi.Admin.LearningStandards.Tests.FromCsv
{
    [TestFixture]
    public class MetaDataRetrieverTests
    {
        private ILogger<MetaDataRetriever> _logger;
        private readonly string folderPath = "TestFiles\\Sample-metadata";
        private string _fileName = string.Empty;
        private readonly string metadataUri = "https://ed-fi-ods.org/metadata/data/v3/resources/swagger.json";
        private MetaDataRetriever _metaDataRetriever = null;
        private string _expectedMetaDataContent = string.Empty;

        [SetUp]
        public void Init()
        {
          
           _fileName = Path.Combine(folderPath, "metadata.json");
           _logger = new NUnitConsoleLogger<MetaDataRetriever>();
            var mockSwaggerDocRetriever = new Mock<ISwaggerDocumentRetriever>();
            string swaggerDocument = TestHelpers.ReadTestFile($"{folderPath}\\Swagger-Document-3.x.json");
            string expectedLsContent = TestHelpers.ReadTestFile($"{folderPath}\\Expected-LS-Metadata.json");
            mockSwaggerDocRetriever.Setup(x => x.LoadJsonString(metadataUri))
                .ReturnsAsync(swaggerDocument);
            _metaDataRetriever = new MetaDataRetriever(_logger, mockSwaggerDocRetriever.Object);
            _expectedMetaDataContent = Regex.Replace(expectedLsContent, @"\s+", "");
        }

        [Test]
        public async Task Will_return_expected_learning_standards_metadata()
        {
            //Arrange
            IfFileExistsDelete(_fileName);

            //Act
            var metaData =  await _metaDataRetriever.GetMetadata(metadataUri, false, folderPath);
            string jsonString = JsonConvert.SerializeObject(metaData);

            //Assert
            FileAssert.Exists(_fileName);
            Assert.NotNull(metaData);
            Assert.AreEqual(_expectedMetaDataContent, jsonString);
        }

        [Test]
        public async Task Will_force_reload_to_refresh_meta_data()
        {
            //Arrange
            CreateFileWithTestContent(_fileName);

            //Act
            var metaData = await _metaDataRetriever.GetMetadata(metadataUri, true, folderPath);
            string jsonString = JsonConvert.SerializeObject(metaData);

            //Assert
            FileAssert.Exists(_fileName);
            Assert.NotNull(metaData);
            Assert.AreEqual(_expectedMetaDataContent, jsonString);
        }

        private static void IfFileExistsDelete(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        private static void CreateFileWithTestContent(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            const string testContent = "{empty json string}";
            using (var writer = new StreamWriter(filePath))
            {
                writer.WriteLine(testContent);
            }
        }
    }
}

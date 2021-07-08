using System.IO;
using System.Linq;
using System.Reflection;
using EdFi.Admin.LearningStandards.Core.Services.FromCsv;
using NUnit.Framework;

namespace EdFi.Admin.LearningStandards.Tests.FromCsv
{
    [TestFixture]
    [SetCulture("en-US")]
    public class CsvFileProcessorTests
    {
        [Test]
        public void ShouldThrowFileNotFound()
        {
            //Arrange
            var csvFileProcessor = new CsvFileProcessor();
          
            //Assert -> Act
            Assert.Throws<FileNotFoundException>( () =>
            {
                var data = csvFileProcessor.GetRows(Path.Combine(GetAssemblyPath(), "TestFiles/no-file.csv")).ToList();
            });
        }

        [Test]
        public void ShouldReadCsvRowsAsDictionaries()
        {
            //Arrange
            var csvFileProcessor = new CsvFileProcessor();

            //Act
            var data = csvFileProcessor.GetRows(Path.Combine(GetAssemblyPath(), "TestFiles/test-data.csv")).ToList();

            //Assert
            Assert.AreEqual(2, data.Count);
            var row1 = data[0];
            var row2 = data[1];
            Assert.AreEqual("LearningStandardId-test-1", row1["LearningStandardId"]);
            Assert.AreEqual("description2", row2["Description"]);
        }

        public static string GetAssemblyPath()
            => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    }
   
}

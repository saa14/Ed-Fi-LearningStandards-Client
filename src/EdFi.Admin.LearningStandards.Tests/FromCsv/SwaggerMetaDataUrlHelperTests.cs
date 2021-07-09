using EdFi.Admin.LearningStandards.Core;
using EdFi.Admin.LearningStandards.Core.Configuration;
using EdFi.Admin.LearningStandards.Core.Services.FromCsv;
using NUnit.Framework;

namespace EdFi.Admin.LearningStandards.Tests.FromCsv
{
    [TestFixture]
    public class SwaggerMetaDataUrlHelperTests
    {
        private const string BaseUrl = "https://ed-fi-ods.org";

        [Test]
        public void Will_return_expected_uri_with_school_year()
        {
            //Arrange
            const string expectedUrl = "https://ed-fi-ods.org/metadata/data/v3/2018/resources/swagger.json";
            var odsConfiguration =
                new EdFiOdsApiConfiguration(BaseUrl, EdFiOdsApiCompatibilityVersion.v3, null, 2018, null);

            //Act
            string result = SwaggerMetaDataUriHelper.GetUri(odsConfiguration);

            //Assert
            Assert.AreEqual(expectedUrl, result);
        }

        [Test]
        public void Will_return_expected_uri_with_no_school_year()
        {
            //Arrange
            const string expectedUrl = "https://ed-fi-ods.org/metadata/data/v3/resources/swagger.json";
            var odsConfiguration =
                new EdFiOdsApiConfiguration(BaseUrl, EdFiOdsApiCompatibilityVersion.v3, null, null, null);

            //Act
            string result = SwaggerMetaDataUriHelper.GetUri(odsConfiguration);

            //Assert
            Assert.AreEqual(expectedUrl, result);
        }
    }
}

using EdFi.Admin.LearningStandards.Core.Configuration;

namespace EdFi.Admin.LearningStandards.Core.Services.FromCsv
{
    public static class SwaggerMetaDataUriHelper
    {
        public static string GetUri(IEdFiOdsApiConfiguration configuration)
        {
            string path = !(configuration.SchoolYear is null) ? $"data/v3/{configuration.SchoolYear}" : "data/v3";
            return $"{configuration.Url}/metadata/{path}/resources/swagger.json";
        }
    }
}

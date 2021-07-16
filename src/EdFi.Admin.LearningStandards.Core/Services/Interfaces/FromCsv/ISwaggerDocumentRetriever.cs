using System.Threading.Tasks;

namespace EdFi.Admin.LearningStandards.Core.Services.Interfaces.FromCsv
{
    public interface ISwaggerDocumentRetriever
    {
        Task<string> LoadJsonString(string metaDataUri);
    }
}

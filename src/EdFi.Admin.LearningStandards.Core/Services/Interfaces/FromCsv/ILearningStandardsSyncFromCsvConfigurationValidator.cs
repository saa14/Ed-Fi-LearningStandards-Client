using System.Threading.Tasks;
using EdFi.Admin.LearningStandards.Core.Configuration;

namespace EdFi.Admin.LearningStandards.Core.Services.Interfaces.FromCsv
{
    public interface ILearningStandardsSyncFromCsvConfigurationValidator
    {
        Task<IResponse>
            ValidateEdFiOdsApiConfigurationAsync(IEdFiOdsApiConfiguration edFiOdsApiConfiguration);
    }
}

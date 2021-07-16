using System;
using System.Text;
using EdFi.Admin.LearningStandards.Core.Models.FromCsv;

namespace EdFi.Admin.LearningStandards.Core.Services.FromCsv
{
    public class TypeConversionException : Exception
    {
        public TypeConversionException(DataMapper node, string expectedType, bool typeIsUnsupported = false)
            : base(BuildMessage(node, expectedType, typeIsUnsupported))
        {
        }

        private static string BuildMessage(DataMapper node, string expectedType,
            bool typeIsUnsupported = false)
        {
            var message = new StringBuilder();

            message.Append(!string.IsNullOrWhiteSpace(node.SourceColumn)
                ? $"Column \"{node.SourceColumn}\" contains a value for property \"{node.Name}\" which cannot be "
                : $"Static value for property \"{node.Name}\" cannot be ");

            message.Append(
                $"converted to{(typeIsUnsupported ? " unsupported " : " ")}type \"{expectedType}\".");

            return message.ToString();
        }
    }
}

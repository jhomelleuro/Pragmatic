using Newtonsoft.Json.Serialization;

namespace Pragmatic.Helpers
{
    public class PascalCasePropertyNamesContractResolver : DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            // Convert the property name to Pascal case
            return ToPascalCase(propertyName);
        }

        private string ToPascalCase(string s)
        {
            // If the string is null or empty, return it as is
            if (string.IsNullOrEmpty(s))
                return s;

            // Convert the first character to uppercase and concatenate it with the rest of the string
            return char.ToUpper(s[0]) + s.Substring(1);
        }
    }
}

using System.Linq;
using System.Text.RegularExpressions;

namespace Wangkanai.Detection.Extensions
{
    public static class StringExtensions
    {
        public static Match RegexMatch(this Regex regex, string source)
        {
            var match = regex.Match(source);
            return match.Success ? match : Match.Empty;
        }

        public static string RemoveAll(this string source, params string[] strings)
            => strings.Aggregate(source, (current, value)
                => current.Replace(value, "", StringComparison.Ordinal));

        public static string SubstringSafe(this string source, int startIndex, int length)
            => source.Length <= startIndex ? ""
            : source.Length - startIndex <= length ? source.Substring(startIndex)
            : source.Substring(startIndex, length);

        public static string SubstringSafe(this string source, int startIndex)
            => source.Length <= startIndex ? "" : source.Substring(startIndex);
    }
}

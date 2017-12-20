namespace Dingo.Common
{
    public static class StringExtensions
    {
        public static string Truncate(this string value, int maxLength, string trailingString = null)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + trailingString;
        }
    }
}
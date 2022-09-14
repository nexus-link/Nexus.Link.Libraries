using System;

namespace Nexus.Link.Libraries.Core.Misc
{
    public static class StringExtensions
    {
        /// <summary>
        /// Remove <paramref name="prefix"/> from the start of <paramref name="longString"/>, using <paramref name="stringComparison"/>.
        /// </summary>
        public static string RemovePrefix(this string longString, string prefix, StringComparison stringComparison = StringComparison.CurrentCulture)
        {
            if (prefix == null) return longString;
            if (longString == null || longString.Length < prefix.Length) return longString;
            if (!longString.StartsWith(prefix, stringComparison)) return longString;
            return longString.Substring(prefix.Length);
        }

        /// <summary>
        /// Remove <paramref name="suffix"/> from the end of <paramref name="longString"/>, using <paramref name="stringComparison"/>.
        /// </summary>
        public static string RemoveSuffix(this string longString, string suffix, StringComparison stringComparison = StringComparison.CurrentCulture)
        {
            if (suffix == null) return longString;
            if (longString == null || longString.Length < suffix.Length) return longString;
            if (!longString.EndsWith(suffix, stringComparison)) return longString;
            return longString.Substring(0, longString.Length - suffix.Length);
        }

        /// <summary>
        /// Find the last <paramref name="separator"/> in <paramref name="longString"/> and remove the rest of the string from and including that position.
        /// </summary>
        public static string RemoveLastSeparated(this string longString, char separator)
        {
            if (longString == null) return null;
            var parts = longString.Split(separator);
            if (parts.Length < 2) return longString;
            return longString.RemoveSuffix($"{separator}{parts[parts.Length - 1]}");
        }
    }
}

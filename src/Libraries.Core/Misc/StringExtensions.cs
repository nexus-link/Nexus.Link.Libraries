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
            if (longString == null || longString.Length < prefix.Length) return null;
            if (!longString.StartsWith(prefix, stringComparison)) return longString;
            return longString.Substring(prefix.Length);
        }

        /// <summary>
        /// Remove <paramref name="suffix"/> from the end of <paramref name="longString"/>, using <paramref name="stringComparison"/>.
        /// </summary>
        public static string RemoveSuffix(this string longString, string suffix, StringComparison stringComparison = StringComparison.CurrentCulture)
        {
            if (suffix == null) return longString;
            if (longString == null || longString.Length < suffix.Length) return null;
            if (!longString.EndsWith(suffix, stringComparison)) return longString;
            return longString.Substring(0, longString.Length-suffix.Length);
        }
    }
}

using System;
using System.Text;

namespace Common.Domain.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// The set of characters that are unreserved in RFC 2396 but are NOT unreserved in RFC 3986.
        /// </summary>
        private static readonly string[] UriRfc3986CharsToEscape = new[] { "!", "*", "'", "(", ")" };

        public static string EscapeUriDataStringRfc3986(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            // Start with RFC 2396 escaping by calling the .NET method to do the work.
            // This MAY sometimes exhibit RFC 3986 behavior (according to the documentation).
            // If it does, the escaping we do that follows it will be a no-op since the
            // characters we search for to replace can't possibly exist in the string.
            var escaped = new StringBuilder(Uri.EscapeDataString(value));

            // Upgrade the escaping to RFC 3986, if necessary.
            foreach (var t in UriRfc3986CharsToEscape)
            {
                escaped.Replace(t, Uri.HexEscape(t[0]));
            }

            // Return the fully-RFC3986-escaped string.
            return escaped.ToString();
        }
    }
}

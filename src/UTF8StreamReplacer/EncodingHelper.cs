using System.Text;

namespace UTF8StreamReplacer
{
    internal static class EncodingHelper
    {
        public static string GetString(this byte[] b) => Encoding.UTF8.GetString(b);
        public static byte[] GetBytes(this string s) => Encoding.UTF8.GetBytes(s);
    }
}

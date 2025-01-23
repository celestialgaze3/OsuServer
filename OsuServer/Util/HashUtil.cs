using System.Security.Cryptography;
using System.Text;
using System.Text.Unicode;

namespace OsuServer.Util
{
    public class HashUtil
    {
        /// <summary>
        /// Hashes a UTF-16 string as if it were encoded in UTF-8.
        /// </summary>
        /// <param name="utf16String">The UTF-16 string</param>
        /// <returns>The MD5 hash in hex string form</returns>
        public static string MD5HashAsUTF8(string utf16String)
        {
            byte[] utf16Bytes = Encoding.Unicode.GetBytes(utf16String);
            byte[] utf8Bytes = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, utf16Bytes);
            byte[] output = MD5.HashData(utf8Bytes);
            return Convert.ToHexStringLower(output);
        }
    }
}

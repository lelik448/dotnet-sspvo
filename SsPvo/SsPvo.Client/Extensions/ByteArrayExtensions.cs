using System;

namespace SsPvo.Client.Extensions
{
    public static class ByteArrayExtensions
    {
        public static byte[] Base64DecodeString(string inputStr)
        {
            byte[] decodedByteArray = Convert.FromBase64CharArray(inputStr.ToCharArray(), 0, inputStr.Length);
            return decodedByteArray;
        }
    }
}

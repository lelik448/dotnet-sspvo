using System;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SsPvo.Client.Extensions
{
    public static class StringExtensions
    {
        public static string ToBase64String(this string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string FromBase64String(this string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static string ToLowerFirstChar(this string input)
        {
            return input.ReplaceAt(0, Char.ToLower(input[0]));
        }

        public static string ReplaceAt(this string input, int index, char newChar)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            char[] chars = input.ToCharArray();
            chars[index] = newChar;
            return new string(chars);
        }

        public static bool ContainsIgnoreCase(this string source, string fragment) =>
            !string.IsNullOrEmpty(source) && source.IndexOf(fragment, StringComparison.InvariantCultureIgnoreCase) > -1;

        public static bool IsValidJson(this string strInput)
        {
            if (string.IsNullOrWhiteSpace(strInput))
            {
                return false;
            }

            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    //Exception in parsing json
                    Console.WriteLine(jex.Message);
                    return false;
                }
                catch (Exception ex) //some other exception
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static bool IsValidXml(this string strInput)
        {
            if (!string.IsNullOrEmpty(strInput) && strInput.TrimStart().StartsWith("<"))
            {
                try
                {
                    var xdoc = XDocument.Parse(strInput);
                    return xdoc != null;
                }
                catch (Exception e)
                {
                }

                return false;
            }
            else
            {
                return false;
            }
        }
    }
}

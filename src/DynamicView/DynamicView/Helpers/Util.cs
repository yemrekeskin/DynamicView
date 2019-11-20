using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace DynamicView.Helpers
{
    public static class Util
    {
        #region Hash

        public static string ComputeHash(string source)
        {
            return HMACSHA256(source);
            /*using (MD5 md5Hash = MD5.Create())
            {
                return GetMd5Hash(md5Hash, source);
            }*/
        }

        static string GetMd5Hash(MD5 md5Hash, string input)
        {
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        static string HMACSHA256(string data)
        {
            string key = "deniznaviapp";
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(key);

            string hash = String.Empty;
            byte[] mbyte = Encoding.ASCII.GetBytes(data);
            HMACSHA256 hmac = new HMACSHA256(keyByte);
            byte[] hmsg = hmac.ComputeHash(mbyte, 0, Encoding.ASCII.GetByteCount(data));
            foreach (byte theByte in hmsg)
            {
                hash += theByte.ToString("x2");
            }
            return hash;
        }

        #endregion

        #region Files

        public static string GetImagePath(object fileName)
        {
            if (fileName == null || string.IsNullOrEmpty(fileName.ToString()))
                return null;

            return GetImagePath(fileName.ToString());
        }

        public static string GetImagePath(string fileName)
        {
            string root = "/Files";

            return string.Format("{0}/{1}", root, fileName);
        }

        #endregion
    }
}
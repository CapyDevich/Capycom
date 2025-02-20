﻿using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;

namespace Capycom
{
    public class MyConfig
    {
        public string ServerSol {  get; set; }
        public bool AllowSignIn { get; set; }
        public bool AllowLogIn { get; set; }
        public bool AllowCreatePost { get; set; }
        public bool AllowEditPost { get; set; }
        public bool AllowCreateComment { get; set; }
        public bool AllowEditUserInfo { get; set; }
        public bool AllowEditUserIdentity { get; set; }


        public static byte[] GetSha256Hash(string stringToSHA, string sol, string serversol)
        {
            if (stringToSHA == null || stringToSHA == String.Empty)
            {
                throw new ArgumentException("Строка была пустой или null");
            }

            byte[] returnValue;
            returnValue = SHA256.HashData(Encoding.Unicode.GetBytes(stringToSHA + sol + serversol));
            return returnValue;
        }
        public static string GetRandomString(int length)
        {
            Random rnd = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[rnd.Next(s.Length)]).ToArray());
        }
    }
}

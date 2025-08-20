using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;


namespace TimeTracker.Helpers
{
    public class Crypto
    {
        private byte[] _iv = Encoding.ASCII.GetBytes("_0S0A1V203N4N516");

        private byte[] _key = Encoding.ASCII.GetBytes("_IBNCNDCEOFNGTHAICJTKCLEMNNTOEPR");



        public string Decrypt(string inputText)

        {

            byte[] buffer = Convert.FromBase64String(inputText);

            byte[] buffer2 = new byte[buffer.Length];

            RijndaelManaged managed = new RijndaelManaged();



            using (MemoryStream stream = new MemoryStream(buffer))

            {

                using (CryptoStream stream2 = new CryptoStream(stream, managed.CreateDecryptor(this._key, this._iv), CryptoStreamMode.Read))

                {

                    using (StreamReader reader = new StreamReader(stream2, true))

                    {

                        return reader.ReadToEnd();

                    }

                }

            }

        }



        public string Encrypt(string inputText)

        {

            byte[] buffer2;

            byte[] bytes = Encoding.ASCII.GetBytes(inputText);

            RijndaelManaged managed = new RijndaelManaged();

            int keySize = managed.KeySize;



            using (MemoryStream stream = new MemoryStream(bytes.Length))

            {

                using (CryptoStream stream2 = new CryptoStream(stream, managed.CreateEncryptor(this._key, this._iv), CryptoStreamMode.Write))

                {

                    stream2.Write(bytes, 0, bytes.Length);

                    stream2.FlushFinalBlock();

                    stream2.Close();

                }

                buffer2 = stream.ToArray();

            }

            return Convert.ToBase64String(buffer2);

        }
    }
}
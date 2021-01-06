using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Genkey
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

        }
        public static string Encrypt(string toEncrypt, string key, bool useHashing)
        {
            byte[] keyArray;
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);

            if (useHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
            }
            else
                keyArray = UTF8Encoding.UTF8.GetBytes(key);

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }
        public static string Decrypt(string toDecrypt, string key, bool useHashing)
        {
            string ret;
            try
            {
                byte[] keyArray;
                byte[] toEncryptArray = Convert.FromBase64String(toDecrypt);

                if (useHashing)
                {
                    MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                    keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                }
                else
                    keyArray = UTF8Encoding.UTF8.GetBytes(key);

                TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
                tdes.Key = keyArray;
                tdes.Mode = CipherMode.ECB;
                tdes.Padding = PaddingMode.PKCS7;

                ICryptoTransform cTransform = tdes.CreateDecryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                ret = UTF8Encoding.UTF8.GetString(resultArray);
            }
            catch (Exception)
            {
                ret = "";
            }

            return ret;
        }

        public static string LoaiBoKyTuDacBiet(string stringSource)
        {
            string pattern = @"\*|\+|\-|\=|\/|\@|\#|\%|\&|\$|\`|\^";
            Regex myRegex = new Regex(pattern);
            MatchCollection mc = myRegex.Matches(stringSource);
            foreach (Match keySpecial in mc)
            {
                string keyS = keySpecial.Value;
                stringSource = stringSource.Replace(keyS, string.Empty);
            }
            return stringSource;
        }



        public static string GetKeyDate(string StringDate, string StringKey)
        {
            StringDate = HoanVi(StringDate);
            string newString = string.Empty;
            char[] arrDate = StringDate.ToArray();
            char[] arrKey = StringKey.ToArray();
            newString += string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}", arrDate[0], arrKey[0], arrDate[1], arrKey[1], arrDate[2], arrKey[2], arrDate[3], arrKey[3], arrDate[4], arrKey[4], arrDate[5], arrKey[5]);
            string keyformat = string.Format("{0}-{1}-{2}", newString.Substring(0, 4), newString.Substring(4, 4), newString.Substring(8, 4));
            return keyformat;

        }

        public static DateTime GetDateKey(string stringKey)
        {
            stringKey = stringKey.Replace("-", string.Empty);
            char[] arrKey = stringKey.ToArray();
            string date = string.Format("{0}{1}{2}{3}{4}{5}", arrKey[0], arrKey[2], arrKey[4], arrKey[6], arrKey[8], arrKey[10]);
            date = HoanVi(date);
            date = "20" + date;
            DateTime dt = new DateTime(Convert.ToInt32(date.Substring(0, 4)), Convert.ToInt32(date.Substring(4, 2)), Convert.ToInt32(date.Substring(6, 2)));
            return dt;

        }

        private static string HoanVi(string strHoanVi)
        {
            string newString = string.Empty;
            char[] a = strHoanVi.ToArray();
            for (int i = a.Length - 1; i >= 0; i--)
            {
                newString += a[i];
            }
            return newString;
        }


    }

}

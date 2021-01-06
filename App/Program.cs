using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Data.SqlClient;
using Microsoft.Win32;
using DevExpress.XtraEditors;
using QLTS.UI.DB;
using System.Threading;
using System.Security.Cryptography;
using System.Text;
using System.Management;
using System.Text.RegularExpressions;

namespace QLTS.UI
{
    static class Program
    {

     
        
        /// <summary>
        /// The main entry point for the application.
        /// </summary> 
        public static SqlConnection sqlCn;
        public static string Connection;
        public static string Server = "";
        public static string DataBase = "";
        public static string User = "";
        public static string Password = "";
        public static bool Login = false;
        public static HT_User objUserCurrent;
        public static string Key = "sdcom";
        public static DMDonViTrucThuoc DonVi;
        public static int Nam = DateTime.Now.Year;
        public static string MaDangKy = "";
        public static bool DaDangKy = false;
        public static int SoNgayDungThu = 1;
        public static string ConnectionAuthenticaMode = "Data Source=.\\QLTS2015;Initial Catalog=QLTS;Integrated Security = true";
        public static string ConnectionMasterAuthenticaMode = "Data Source=.\\QLTS2015;Initial Catalog=master;Integrated Security = true";
        static void Main()
        {
         //   Thread.CurrentThread.ApartmentState = ApartmentState.STA; 

           
            //DevExpress.UserSkins.BonusSkins.Register();
            //DevExpress.UserSkins.OfficeSkins.Register ();
            //DevExpress.Skins.SkinManager.EnableFormSkins();
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            string strRegistryKey = "Software\\SDCom\\QLTS";
           // DevExpress.XtraBars.Helpers.SkinHelper.InitSkinGallery(rgbiSkins, true);
            RegistryKey key = Registry.CurrentUser.OpenSubKey(strRegistryKey, true); 
            //kiểm tra tôn tại key hay chưa?
            if (key == null)
            {
                key = Registry.CurrentUser.CreateSubKey(strRegistryKey);
                key.SetValue("serial", "");
                key.SetValue("time", Encrypt("100", Key, true));
            }
            else
            {
                if (key.GetValue("time") == null)
                {
                    key.SetValue("serial", "");
                    key.SetValue("time", Encrypt("100", Key, true));
                }
            }
            byte[] arrSkin = (byte[])key.GetValue("Skin", new byte[-1 + 1]);
            
            //@Hòa xem lại đoạn này set kin ko được 
            if (arrSkin.Length == 0)
            {
                DevExpress.LookAndFeel.UserLookAndFeel.Default.SetSkinStyle("Office 2007 Blue");
            }
            else
            {

                DevExpress.LookAndFeel.UserLookAndFeel.Default.SetSkinStyle(System.Text.Encoding.Unicode.GetString(arrSkin));
            
            } 
          
            frmBase frmBase = new frmBase();
            frmBase.CreateWaitDialog("Khởi động chương trình", "Vui lòng chờ");
            GetConn();
            //Kiem tra ban quyen             

            MaDangKy = (string)key.GetValue("serial");
            SoNgayDungThu = int.Parse(Decrypt(key.GetValue("time").ToString(), Key, true));     
            //TruongTV subtring 8 and upper case
            string SerialReal = GetMaDangKy();
            string SerialHash = Encrypt(SerialReal, Key, true);
            SerialHash = LoaiBoKyTuDacBiet(SerialHash);
            SerialHash = SerialHash.Substring(0, 6).ToUpper();
            //Check time licese
            DateTime dtLicence = Program.GetDateKey(MaDangKy);
            //Gen key date 
            string cdKeySystemDate = Program.GetKeyDate(dtLicence.ToString("yyMMdd"), SerialHash);
            //Compare keydate and key regedit 
            if (dtLicence <= DateTime.Now || MaDangKy == "" || MaDangKy != cdKeySystemDate)
            {
                frmDangKy frmDangKy = new frmDangKy();
                frmBase.CloseWaitDialog();
                if (frmDangKy.ShowDialog() == DialogResult.No)
                {
                    return;
                }

            }
            else
            {
                frmBase.CloseWaitDialog();
                DaDangKy = true;
            }

            if ((SoNgayDungThu > 0) || DaDangKy)
            {
                frmBase.CreateWaitDialog("Đang kiểm tra kết nối dữ liệu...", "");
                if (SetConn(Server, DataBase, User, Password) == false)
                {
                    frmConfigServer frmCofig = new frmConfigServer();                            
                    frmBase.CloseWaitDialog();
                    frmCofig.ShowDialog();
                    if (frmCofig.Tag.ToString() == "OK")
                    {
                        Application.Run(new frmMain());                      
                    }
                }
                else
                {
                    frmBase.CloseWaitDialog();
                    var thread = new Thread(new ParameterizedThreadStart(param => { Application.Run(new frmMain()); }));
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();                    
                }
            }            

            ////////////           
    

            //if (SetConn(Server, DataBase, User, Password) == false)
            //{
            //    frmConfigServer frmCofig = new frmConfigServer();
            //    frmCofig.ShowDialog();
            //    if (frmCofig.Tag.ToString() == "OK")
            //    {
            //        var thread = new Thread(new ParameterizedThreadStart(param => { Application.Run(new frmMain()); }));
            //        thread.SetApartmentState(ApartmentState.STA);
            //        thread.Start();
            //      //Application.Run(new frmMain());
            //    }
            //}
            //else
            //{
            //    var thread = new Thread(new ParameterizedThreadStart(param => { Application.Run(new frmMain()); }));
            //    thread.SetApartmentState(ApartmentState.STA);
            //    thread.Start();
            //  // Application.Run(new frmMain());
            //} 
        }
        static void GetConn()
        {
            try
            {
                string strRegistryKey = "Software\\SDCom\\QLTS";
                RegistryKey key = Registry.CurrentUser.OpenSubKey(strRegistryKey, true);
                if ((key != null))
                {
                    // lấy tên server
                    byte[] arrServerName = (byte[])key.GetValue("Server", new byte[-1 + 1]);
                    if (arrServerName.Length == 0)
                    {
                        Server = "";
                    }
                    else
                    {
                        Server = System.Text.Encoding.Unicode.GetString(arrServerName);
                    }

                    // lấy tên Database
                    byte[] arrDataBase = (byte[])key.GetValue("Database", new byte[-1 + 1]);
                    if (arrDataBase.Length == 0)
                    {
                        DataBase = "";
                    }
                    else
                    {
                        DataBase = System.Text.Encoding.Unicode.GetString(arrDataBase);
                    }

                    // lấy tên User
                    byte[] arrUser = (byte[])key.GetValue("UserSQL", new byte[-1 + 1]);
                    if (arrUser.Length == 0)
                    {
                        User = "";
                    }
                    else
                    {
                        User = System.Text.Encoding.Unicode.GetString(arrUser);
                    }

                    // lấy PasswordSQL
                    byte[] arrPassword = (byte[])key.GetValue("PasswordSQL", new byte[-1 + 1]);
                    if (arrPassword.Length == 0)
                    {
                        Password = "";
                    }
                    else
                    {
                        Password = System.Text.Encoding.Unicode.GetString(arrPassword);
                    }
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message);
            }
        }

        public static bool SetConn(string _Server, string _DataBase, string _User, string _Password)
        {
            try
            {
                sqlCn = new SqlConnection();
                sqlCn.ConnectionString = "Server=" + _Server + "; Database=" + _DataBase + "; UID=" + _User + "; PWD=" + _Password;
                Server = sqlCn.ConnectionString;
                Connection = sqlCn.ConnectionString;
                sqlCn.Open();
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public static string GetMaDangKy()
        {
            ManagementObject dsk = new ManagementObject(@"win32_logicaldisk.deviceid=""C" + "" + @":""");
            dsk.Get();
            //TruongTV-serial
            return  dsk["VolumeSerialNumber"].ToString();
            //return Encrypt(dsk["VolumeSerialNumber"].ToString(), Key, true);
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

        

        #region  gen new key 
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
            try
            {
                stringKey = stringKey.Replace("-", string.Empty);
            char[] arrKey = stringKey.ToArray();
            string date = string.Format("{0}{1}{2}{3}{4}{5}", arrKey[0], arrKey[2], arrKey[4], arrKey[6], arrKey[8], arrKey[10]);
            date = HoanVi(date);
            date = "20" + date;
            DateTime dt = new DateTime(Convert.ToInt32(date.Substring(0, 4)), Convert.ToInt32(date.Substring(4, 2)), Convert.ToInt32(date.Substring(6, 2)));
            return dt;
            }
            catch 
            {

                return DateTime.MinValue;
            }
            

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

        #endregion 

    }
}

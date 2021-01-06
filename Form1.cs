using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace Genkey
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            cboMonths.SelectedIndex = 0;
            //Test();
        }

        //private void Test() {
        //    DateTime dt = new DateTime();
        //    dt = DateTime.Now;
        //    dt= dt.AddMonths(6);
        //    string s =  dt.ToLongDateString();
        //    string a = dt.ToLongDateString();
        //    string a2 = Program.Encrypt(a, "sdcom", true);
        //    string dateKeyforGen = dt.ToString("yyMMdd");   
        //    string Key = "XTCD9Y";
        //    string newkey = GetKeyDate(dateKeyforGen, Key);
        //    DateTime dateOld = GetDateKey(newkey);            
        //}

        private string GetKeyDate(string StringDate, string StringKey)
        {
            StringDate = HoanVi(StringDate);
            string newString = string.Empty;
            char[] arrDate = StringDate.ToArray();
            char[] arrKey = StringKey.ToArray();
            newString += string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}", arrDate[0], arrKey[0], arrDate[1], arrKey[1], arrDate[2], arrKey[2], arrDate[3], arrKey[3], arrDate[4], arrKey[4], arrDate[5], arrKey[5]);
            string keyformat = string.Format("{0}-{1}-{2}", newString.Substring(0, 4), newString.Substring(4, 4), newString.Substring(8, 4));
            return keyformat;
        
        }

        private DateTime GetDateKey(string stringKey) 
        {
             stringKey = stringKey.Replace("-",string.Empty);
             char[] arrKey = stringKey.ToArray();
             string date  =string.Format("{0}{1}{2}{3}{4}{5}",arrKey[0],arrKey[2],arrKey[4],arrKey[6],arrKey[8],arrKey[10] );
             date = HoanVi(date);
             date = "20" + date;
             DateTime dt = new DateTime( Convert.ToInt32( date.Substring(0,4 )),Convert.ToInt32( date.Substring(4,2 )),Convert.ToInt32(date.Substring(6,2)));
             return dt;
        
        }



        private string HoanVi(string strHoanVi) {
            string newString = string.Empty ;
            char [] a =  strHoanVi.ToArray();
            for (int i = a.Length-1; i >=0; i--)
            {
                newString += a[i];                
            }
            return newString;
        }
       

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {            
            //TruongTV loại bỏ ký tự đặc biệt trong chuỗi 
            string keyEncr = Program.Encrypt(txtSerial.Text, "sdcom", true);
            string newKey = Program. LoaiBoKyTuDacBiet(keyEncr);                       
            //get datetime 
            int months = Convert.ToInt32(cboMonths.Text);
            DateTime dtLimit = DateTime.Now.AddMonths(months);
            //fomart date
            //string dateKeyforGen = string.Format("{0}{1}{2}", dtLimit.Year.ToString(), dtLimit.Month, dtLimit.Day.ToString());
            string dateKeyforGen = dtLimit.ToString("yyMMdd");            
            //substring key  
            newKey = newKey.Substring(0, 6).ToUpper();
            string newKeyDate =  Program.GetKeyDate(dateKeyforGen, newKey);

            txtKey.Text = newKeyDate; 





           
        }
    }
}

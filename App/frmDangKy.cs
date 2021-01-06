using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Microsoft.Win32;

namespace QLTS.UI
{
    public partial class frmDangKy : frmBase
    {
        Microsoft.Win32.RegistryKey key;        
        public frmDangKy()
        {
            InitializeComponent();
            string strRegistryKey = "Software\\SDCom\\QLTS";             
           
        }

        private void btnDangKy_Click(object sender, EventArgs e)
        {
            key = Registry.CurrentUser.OpenSubKey(strRegistryKey, true);

            //TruongTV loại bỏ ký tự đặc biệt trong chuỗi 
            string keyEncr = Program.Encrypt(txtMaDangKy.Text, Program.Key, true);
            string newKey = Program.LoaiBoKyTuDacBiet(keyEncr);
            //TruongTV Subtring 8 and upper 
            string cdKeySystem = newKey.Substring(0, 6).ToUpper();
            String cdKey = string.Format("{0}-{1}-{2}", txtCDKey1.Text, txtCDkey2.Text,txtCDkey3.Text);
            if (cdKey.Length != 14) {
                ThongBaoLoi("Mã đăng ký không đúng");
                return;
                
            }
            //Check han 
            DateTime dtHan = Program.GetDateKey(cdKey);
            if (dtHan <= DateTime.Now) {
                ThongBaoLoi("Mã đang ký đã hết hạn sử dụng");
                return;
                
            }
            //check key  
            string cdKeySystemDate= Program.GetKeyDate(dtHan.ToString("yyMMdd"), cdKeySystem);


            if (cdKeySystemDate == cdKey)
            {
                Program.DaDangKy = true;
                ThongBao("Đăng ký thành công.");
                key.SetValue("serial", cdKey);
                key.SetValue("time", Program.Encrypt("100", Program.Key, true));
                this.Close();
            }
            else
                ThongBaoLoi("Mã đăng ký không đúng.");
        }


        private int LoadThongTinBanQuyen()
        {
            string strRegistryKey = "Software\\SDCom\\QLTS";
            // DevExpress.XtraBars.Helpers.SkinHelper.InitSkinGallery(rgbiSkins, true);
            RegistryKey key = Registry.CurrentUser.OpenSubKey(strRegistryKey, true);
            string MaDangKy = (string)key.GetValue("serial");
            //Check time licese
            DateTime dtLicence = Program.GetDateKey(MaDangKy);
            TimeSpan timeSpan = new TimeSpan();
            timeSpan = (TimeSpan)(dtLicence - DateTime.Now);
            key.Close();
            return timeSpan.Days;
        }

        private void frmDangKy_Load(object sender, EventArgs e)
        {
            if (LoadThongTinBanQuyen()>0) {
                lblCount.Text = string.Empty;
                txtMaDangKy.Text = Program.GetMaDangKy();
                btnDungThu.Enabled = false;
                labelControl1.Text = string.Empty;                
                labelControl3.Text = string.Empty;
                return;        
            }
            key = Registry.CurrentUser.OpenSubKey(strRegistryKey, true);
            txtMaDangKy.Text = Program.GetMaDangKy();
            string stringValue = (string)key.GetValue("serial");
            int intValue =int.Parse(Program.Decrypt(key.GetValue("time").ToString(),Program.Key,true));
            lblCount.Text = intValue.ToString();
            if (!Program.DaDangKy)
            {
                if (intValue == 0)
                {
                    Program.SoNgayDungThu = 0;
                    btnDungThu.Enabled = false;
                }
              
            }
            key.Close();
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
            this.Close();
        }
    

        private void btnDungThu_Click(object sender, EventArgs e)
        {
            key = Registry.CurrentUser.OpenSubKey(strRegistryKey, true);
            int intValue = int.Parse(Program.Decrypt(key.GetValue("time").ToString(), Program.Key, true));
            key.SetValue("time", Program.Encrypt((intValue - 1).ToString(), Program.Key, true));
            key.Close();
            Program.SoNgayDungThu = intValue - 1;
            this.Close();
        }

     


        private void txtCDKey1_TextChanged(object sender, EventArgs e)
        {
            txtCDKey1.Text = txtCDKey1.Text.ToUpper();
        }

        private void txtCDkey2_TextChanged(object sender, EventArgs e)
        {
            txtCDkey2.Text = txtCDkey2.Text.ToUpper();
        }

        private void txtCDkey3_TextChanged(object sender, EventArgs e)
        {
            txtCDkey3.Text = txtCDkey3.Text.ToUpper();

        }

       

           
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using QLTS.UI.DB;
using Microsoft.Win32;
using System.Collections;
using DevExpress.XtraEditors;

namespace QLTS.UI
{
    public partial class frmLogin : Form
    {
        dbConnectionDataContext db = new dbConnectionDataContext(Program.Connection);
        clsAuthentication EnDyCrypt;
        private int mCountLog = 0;
        private int mMountLog = 3;
        public frmLogin()
        {
            InitializeComponent();
            EnDyCrypt = new clsAuthentication();
            LoadData();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            
            try
            {
                if (!Check_Valid()) return;
                if (!CheckLogIn(UserName.Text, Password.Text))
                {
                    MessageBox.Show("Tên đăng nhập hoặc mật khẩu không chính xác.");
                    mCountLog += 1;
                    if (mCountLog == mMountLog)
                    {
                        MessageBox.Show("Bạn đã đăng nhập " + mMountLog.ToString() + " lần không thành công.\n Chương trình sẽ kết thúc.");
                        Application.Exit();
                    }
                }
                else
                {

                    //Lưu thông tin đăng nhập
                    SaveRegeditLoginInfo();
                    Hashtable hTable = new Hashtable();


                    this.DialogResult = System.Windows.Forms.DialogResult.OK;
                    Program.Login = true; 

                    this.Close();
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message);
            }
        }
        private void LoadData()
        {
            try
            { 
                //Load mật khẩu
                string strRegistryKey = "Software\\SDCom\\QLTS";
                RegistryKey key = Registry.CurrentUser.OpenSubKey(strRegistryKey, true);
                if ((key != null))
                {
                    // lấy tên đăng nhập
                    byte[] arrUsername = (byte[])key.GetValue("UserName", new byte[-1 + 1]);
                    if (arrUsername.Length == 0)
                    {
                        UserName.Text = "";
                    }
                    else
                    {
                        UserName.Text = System.Text.Encoding.Unicode.GetString(arrUsername);
                    }
                    // giải mã mật khẩu lưu trong registry
                    byte[] arrPassword = (byte[])key.GetValue("PassWord", new byte[-1 + 1]);
                    if (arrPassword.Length == 0)
                    {
                        Password.Text = "";
                        return;
                    }
                    Password.Text = EnDyCrypt.Decrypt(System.Text.Encoding.Unicode.GetString(arrPassword));
                    checkNhoMatKhau.Checked = true;
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message);
            }

        }
        private bool Check_Valid()
        {
            bool functionReturnValue = false;
            Control CtrlErr = null;
            if ((DxErrorProvider != null)) DxErrorProvider.Dispose();
            //Tên đăng nhập không được rỗng
            if (UserName.Text == string.Empty)
            {
                DxErrorProvider.SetError(UserName, "Tên đăng nhập không thể rỗng.");
                if (CtrlErr == null) CtrlErr = UserName;
            }
            if (Password.Text == string.Empty)
            {
                DxErrorProvider.SetError(Password, "Mật khẩu không thể rỗng.");
                if (CtrlErr == null) CtrlErr = Password;
            }
            //Kiểm tra đăng nhập thành công
            if ((CtrlErr != null)) goto QUIT;
            functionReturnValue = true;
            return functionReturnValue;
        QUIT:
            functionReturnValue = false;
            CtrlErr.Focus();
            return functionReturnValue;
        }
        /// <summary>
        /// Save to regedit info login
        /// </summary>
        /// <remarks></remarks>
        private void SaveRegeditLoginInfo()
        {
            string strRegistryKey = "Software\\SDCom\\QLTS";
            if (checkNhoMatKhau.Checked)
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(strRegistryKey, true);
                if (key == null)
                {
                    key = Registry.CurrentUser.CreateSubKey(strRegistryKey);
                }
                try
                {
                    byte[] arrUsername = System.Text.Encoding.Unicode.GetBytes(UserName.Text);
                    byte[] arrPasword = System.Text.Encoding.Unicode.GetBytes(EnDyCrypt.Encrypt(Password.Text));
                    // Lưu tên đăng nhập
                    key.SetValue("UserName", arrUsername, RegistryValueKind.Binary);
                    // Lưu mật khẩu
                    key.SetValue("PassWord", arrPasword, RegistryValueKind.Binary);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    if (key != null)
                    {
                        key.Close();
                    }
                }
            }
            else
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(strRegistryKey, true);
                if (key == null)
                {
                    key = Registry.CurrentUser.CreateSubKey(strRegistryKey);
                }
                try
                {
                    key.SetValue("UserName", new byte[-1 + 1]);
                    key.SetValue("PassWord", new byte[-1 + 1]);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    if (key != null)
                    {
                        key.Close();
                    }
                }
            }
        }

        /// <summary>
        /// Kiểm tra đăng nhập
        private bool CheckLogIn(string UserName, string Password)
        {
          //  Password = EnDyCrypt.Encrypt(Password);
            HT_User dm = db.HT_Users.SingleOrDefault(i => i.TenDangNhap ==UserName);
            if (dm != null && dm.Id > 0 && dm.KichHoat == true && EnDyCrypt.Decrypt(dm.MatKhau) == Password)
            {
                Program.objUserCurrent = dm;
                Program.DonVi = db.DMDonViTrucThuocs.SingleOrDefault(i => i.ID == dm.IDDonVi);
                return true;
            }
            else
                return false;
        }
        private void btnConfig_Click(object sender, EventArgs e)
        {
            frmConfigServer frm = new frmConfigServer();
            frm.ShowDialog();
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void frmLogin_Load(object sender, EventArgs e)
        {
            UserName.Focus();
        }

        private void hyperLinkEdit1_OpenLink(object sender, DevExpress.XtraEditors.Controls.OpenLinkEventArgs e)
        {
            frmLayLaiMatKhau frm = new frmLayLaiMatKhau();
            frm.ShowDialog();
        }
    }
}

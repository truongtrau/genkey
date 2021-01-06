using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using DevExpress.Skins;
using DevExpress.XtraBars.Ribbon;
using DevExpress.Utils.Drawing;
using Microsoft.Win32;
using QLTS.UI.DB;
using System.Linq;
using DevExpress.XtraBars.Ribbon.Gallery;
using System.Data.SqlClient;
using DevExpress.Utils;
using System.ServiceProcess;
using System.IO;

namespace QLTS.UI
{
    public partial class frmMain : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        dbConnectionDataContext db = new dbConnectionDataContext(Program.Connection);
        private DevExpress.Utils.WaitDialogForm dlg = null;
        public frmMain()
        {
            InitializeComponent();
             InitSkinGallery(); 
            this.ribbonMain.SelectedPage = this.ribbonPage5;
            LoadSkin();
         

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



        private void LoadSkin() {

            string strRegistryKey = "Software\\SDCom\\QLTS";
            // DevExpress.XtraBars.Helpers.SkinHelper.InitSkinGallery(rgbiSkins, true);
            RegistryKey key = Registry.CurrentUser.OpenSubKey(strRegistryKey, true);
            //kiểm tra tôn tại key hay chưa?            
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
        
        }



        //#region SkinGallery        
        //void InitSkinGallery()
        //{
        //    SimpleButton imageButton = new SimpleButton();
        //    foreach (SkinContainer cnt in SkinManager.Default.Skins)
        //    {
        //        imageButton.LookAndFeel.SetSkinStyle(cnt.SkinName);
        //        GalleryItem gItem = new GalleryItem();
        //        int groupIndex = 0;
        //        if (cnt.SkinName.IndexOf("Office") > -1) 
        //            groupIndex = 1;
        //        rgbiSkins.Gallery.Groups[groupIndex].Items.Add(gItem);
        //        gItem.Caption = cnt.SkinName;
        //        gItem.Image = GetSkinImage(imageButton, 32, 17, 2);
        //        gItem.HoverImage = GetSkinImage(imageButton, 70, 36, 5);
        //        gItem.Caption = cnt.SkinName;
        //        gItem.Hint = cnt.SkinName;
        //    } 
        //}
        //Bitmap GetSkinImage(SimpleButton button, int width, int height, int indent)
        //{
        //    Bitmap image = new Bitmap(width, height);
        //    using (Graphics g = Graphics.FromImage(image))
        //    {
        //        StyleObjectInfoArgs info = new StyleObjectInfoArgs(new GraphicsCache(g));
        //        info.Bounds = new Rectangle(0, 0, width, height);
        //        button.LookAndFeel.Painter.GroupPanel.DrawObject(info);
        //        button.LookAndFeel.Painter.Border.DrawObject(info);
        //        info.Bounds = new Rectangle(indent, indent, width - indent * 2, height - indent * 2);
        //        button.LookAndFeel.Painter.Button.DrawObject(info);
        //    }
        //    return image;
        //}
        //private void rgbiSkins_GalleryInitDropDownGallery(object sender, InplaceGalleryEventArgs e)
        //{
        //    e.PopupGallery.CreateFrom(rgbiSkins.Gallery);
        //    e.PopupGallery.AllowFilter = false;
        //    e.PopupGallery.ShowItemText = true;
        //    e.PopupGallery.ShowGroupCaption = true;
        //    e.PopupGallery.AllowHoverImages = false;
        //    foreach (GalleryItemGroup galleryGroup in e.PopupGallery.Groups)
        //        foreach (GalleryItem item in galleryGroup.Items)
        //            item.Image = item.HoverImage;
        //    e.PopupGallery.ColumnCount = 2;
        //    e.PopupGallery.ImageSize = new Size(70, 36);
        //}

     
        private void rgbiSkins_GalleryItemClick(object sender, GalleryItemClickEventArgs e)
        {
            DevExpress.LookAndFeel.UserLookAndFeel.Default.SetSkinStyle(e.Item.Caption);
            string strRegistryKey = "Software\\SDCom\\QLTS";
            byte[] arrUsername = System.Text.Encoding.Unicode.GetBytes(e.Item.Caption);
            RegistryKey key = Registry.CurrentUser.OpenSubKey(strRegistryKey, true);
            key.SetValue("Skin", arrUsername, RegistryValueKind.Binary);
        }
        //#endregion

        #region SkinGallery
        void InitSkinGallery()
        {
            DevExpress.XtraBars.Helpers.SkinHelper.InitSkinGallery(rgbiSkins, true);
        }
        #endregion
        private void frmMain_Load(object sender, EventArgs e)
        {
            frmStartPage frmStart = new frmStartPage();
                      
            this.CreateMDIForm(frmStart);
           
            frmLogin frmLogin = new frmLogin();
            if (frmLogin.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // lay quyen user login
                GetQuyen_Login(int.Parse("0" + Program.objUserCurrent.IDHT_UserGroup));
                
                //Kiểm tra thời hạn bản quyền
                if (LoadThongTinBanQuyen() <= 30)
                {
                    frmBanQuyen f = new frmBanQuyen();
                    f.ShowDialog();
                }

            }
            if(Program.objUserCurrent!= null)
                bbNamHD1.Caption = "Năm: " + Program.Nam + "     " + Program.objUserCurrent.TenDangNhap  + " - " + Program.objUserCurrent.HoTen; 
        } 
        private void GetQuyen_Login(int IDNhomQuyen)
        {
            frmBase frm = new frmBase();
            var query1 = from _item in db.HT_ChucNangs
                         join c in db.HT_UserGroup_Functions on _item.HT_ChucNangID equals c.IDHT_ChucNang
                         where c.IDHT_UserGroup == IDNhomQuyen
                         select new
                         {
                             HT_ChucNangID = _item.HT_ChucNangID,
                             IDHT_PhanHe = _item.IDHT_PhanHe,
                             TenChucNang = _item.TenChucNang,
                             Tag = _item.Tag,
                             ParentID = _item.ParentID,
                             Level = _item.Level,
                             MoTa = _item.MoTa,
                             barbtnName = _item.barbtnName,
                             KieuRibbon = _item.KieuRibbon
                         };
            DataTable dtTemp = frm.LinqToDataTable(query1);
            // Thuc hien hien thi cac menu da duoc phan quyen
            LoadMenu(dtTemp);
        }
        private void LoadMenu(DataTable dtChucNang)
        {
            RibbonPage rPage;
            DataRow[] arrDrPage;
            arrDrPage = dtChucNang.Select("KieuRibbon = 'RPage'");//RPGroup
            foreach (DataRow dr in arrDrPage)
            {
                rPage = GetPageByName(dr["barbtnName"].ToString());
                if (rPage != null)
                {
                    rPage.Visible = true;
                    VisiblePageGroup(dtChucNang, dr["HT_ChucNangID"].ToString(), rPage);
                }
            }
        }
        private RibbonPage GetPageByName(string Name)
        {
            foreach (RibbonPage rp in this.ribbonMain.Pages)
            {
                if (rp.Name.ToLower() == Name.ToLower())
                    return rp;
            }
            return null;
        }
        private void VisiblePageGroup(DataTable dtChucNang, string HT_ChucNangID, RibbonPage rGroup)
        {
            RibbonPageGroup rpGroup;
            DataRow[] arrDr = dtChucNang.Select("ParentID = " + HT_ChucNangID);
            foreach (DataRow dr in arrDr)
            {
                rpGroup = rGroup.Groups[dr["barbtnName"].ToString()];
                if (rpGroup != null)
                {
                    rpGroup.Visible = true;
                    VisibleBarItem(dtChucNang, dr["HT_ChucNangID"].ToString(), rpGroup);
                }
            }
        }
        private void VisibleBarItem(DataTable dtChucNang, string HT_ChucNangID, RibbonPageGroup rpGroup)
        {
            DataRow[] arrDr = dtChucNang.Select("ParentID = " + HT_ChucNangID);
            foreach (DataRow dr in arrDr)
            {
                for (int i = 0; i < rpGroup.ItemLinks.Count; i++)
                {
                    if (rpGroup.ItemLinks[i].Item.Name == dr["barbtnName"].ToString())
                    {
                        rpGroup.ItemLinks[i].Item.Visibility = BarItemVisibility.Always;
                    }
                }
            }
        }
        public void _Load()
        {
            bbNamHD1.Caption = "Năm: " + Program.Nam; 
            this.Refresh();
        }
        private void ribbon_ItemClick(object sender, ItemClickEventArgs e)
        {

            if (e.Item.Tag == null)
                return; 

            switch (e.Item.Tag.ToString())
            {
               
                #region Phân hệ tài sản
                case "barNhapKhauDS":
                    {
                        frmImport_TS frm = new frmImport_TS();
                        if (!this.CheckFormExist(frm.Tag.ToString()))
                        {
                            this.CreateMDIForm(frm);
                        }
                        break;

                    }

                case "barTaiSan":
                    {
                        frmTaiSan frm = new frmTaiSan();
                        if (!this.CheckFormExist(frm.Tag.ToString()))
                        {
                            this.CreateMDIForm(frm);
                        }
                        break;

                    }

                case "barGhiTang":
                    {
                        frmGhiTang frm = new frmGhiTang();
                        if (!this.CheckFormExist(frm.Tag.ToString()))
                        {
                            this.CreateMDIForm(frm);
                        }
                        break;
                    }

                case "barThayDoiThongTin":
                    {
                        frmThayDoiThongTin frm = new frmThayDoiThongTin();
                        if (!this.CheckFormExist(frm.Tag.ToString()))
                        {
                            this.CreateMDIForm(frm);
                        }
                        break;
                    }

                case "barDanhGiaLai":
                    {
                        frmDanhGiaLai frm = new frmDanhGiaLai();
                        if (!this.CheckFormExist(frm.Tag.ToString()))
                        {
                            this.CreateMDIForm(frm);
                        }
                        break;
                    }

                case "barDieuChuyen":
                    {
                        frmDieuChuyen frm = new frmDieuChuyen();
                        if (!this.CheckFormExist(frm.Tag.ToString()))
                        {
                            this.CreateMDIForm(frm);
                        }
                        break;
                    }

                case "barGhiGiam":
                    {
                        frmGhiGiam frm = new frmGhiGiam();
                        if (!this.CheckFormExist(frm.Tag.ToString()))
                        {
                            this.CreateMDIForm(frm);
                        }
                        break;
                    }

                case "barTinhHaoMon":
                    {
                        frmTinhHaoMon frm = new frmTinhHaoMon();
                        if (!this.CheckFormExist(frm.Tag.ToString()))
                        {
                            this.CreateMDIForm(frm);
                        }
                        break;
                    }

                case "barKiemKe":
                    {
                        frmKiemKe frm = new frmKiemKe();
                        if (!this.CheckFormExist(frm.Tag.ToString()))
                        {
                            this.CreateMDIForm(frm);
                        }
                        break;
                    }
#endregion
                #region Phân hệ tài sản - khác

                case "barKeHoachMuaSam":
                    {
                        frmKeHoachMuaSam frm = new frmKeHoachMuaSam();
                        if (!this.CheckFormExist(frm.Tag.ToString()))
                        {
                            this.CreateMDIForm(frm);
                        }
                        break;
                    }
                case "barDeNghiTrangCap":
                    {
                        frmDeNghiTrangCap frm = new frmDeNghiTrangCap();
                        if (!this.CheckFormExist(frm.Tag.ToString()))
                        {
                            this.CreateMDIForm(frm);
                        }
                        break;
                    }
                case "barBaoDuongTaiSan":
                    {
                        frmBaoDuongTaiSan frm = new frmBaoDuongTaiSan();
                        if (!this.CheckFormExist(frm.Tag.ToString()))
                        {
                            this.CreateMDIForm(frm);
                        }
                        break;
                    }
                case "barKhaiThacTaiSan":
                    {
                        frmKhaiThacTaiSan frm = new frmKhaiThacTaiSan();
                        if (!this.CheckFormExist(frm.Tag.ToString()))
                        {
                            this.CreateMDIForm(frm);
                        }
                        break;
                    }
                case "barSuDungTaiSan":
                    {
                        frmSuDungTaiSan frm = new frmSuDungTaiSan();
                        if (!this.CheckFormExist(frm.Tag.ToString()))
                        {
                            this.CreateMDIForm(frm);
                        }
                        break;
                    }
                case "barDeNghiXuLy":
                    {
                        frmDeNghiXuLy frm = new frmDeNghiXuLy();
                        if (!this.CheckFormExist(frm.Tag.ToString()))
                        {
                            this.CreateMDIForm(frm);
                        }
                        break;
                    }
                #endregion
                #region Phân hệ CCDC

                case "barCCDC":
                    {
                        frmCCDC frm = new frmCCDC();
                        if (!this.CheckFormExist(frm.Tag.ToString()))
                        {
                            this.CreateMDIForm(frm);
                        }
                        break;
                    }

                case "barGhiTang_CCDC":
                    {
                        frmGhiTang_CCDC frm = new frmGhiTang_CCDC();
                        if (!this.CheckFormExist(frm.Tag.ToString()))
                        {
                            this.CreateMDIForm(frm);
                        }
                        break;
                    }

                case "barPhanBo_CCDC":
                    {
                        frmPhanBo_CCDC frm = new frmPhanBo_CCDC();
                        if (!this.CheckFormExist(frm.Tag.ToString()))
                        {
                            this.CreateMDIForm(frm);
                        }
                        break;
                    }

                case "barDieuChuyen_CCDC":
                    {
                        frmDieuChuyen_CCDC frm = new frmDieuChuyen_CCDC();
                        if (!this.CheckFormExist(frm.Tag.ToString()))
                        {
                            this.CreateMDIForm(frm);
                        }
                        break;
                    } 
                case "barGhiGiam_CCDC":
                    {
                        frmGhiGiam_CCDC frm = new frmGhiGiam_CCDC();
                        if (!this.CheckFormExist(frm.Tag.ToString()))
                        {
                            this.CreateMDIForm(frm);
                        }
                        break;
                    } 

                case "barKiemKe_CCDC":
                    {
                        frmKiemKe_CCDC frm = new frmKiemKe_CCDC();
                        if (!this.CheckFormExist(frm.Tag.ToString()))
                        {
                            this.CreateMDIForm(frm);
                        }
                        break;
                    }
                case "barNhapKhauCCDC":
                    {
                        frmImportCCDC frm = new frmImportCCDC();
                        if (!this.CheckFormExist(frm.Tag.ToString()))
                        {
                            this.CreateMDIForm(frm);
                        }
                        break;
                    }
                #endregion
                #region Phân hệ tra cứu

                case "barTraCuuTaiSan":
                    {
                        frmTraCuu_TaiSan frm = new frmTraCuu_TaiSan(true);
                        if (!this.CheckFormExist(frm.Tag.ToString()))
                        {
                            this.CreateMDIForm(frm);
                        }
                        break;
                    }
                case "barTraCuuCCDC":
                    {
                        frmTraCuu_CCDC frm = new frmTraCuu_CCDC(true);
                        if (!this.CheckFormExist(frm.Tag.ToString()))
                        {
                            this.CreateMDIForm(frm);
                        }
                        break;
                    }
                #endregion
                #region Phân hệ danh mục

                case "barDonVi":
                    {
                        frmDonViTrucThuoc frm = new frmDonViTrucThuoc();                      
                        if (!this.CheckFormExist(frm.Tag.ToString()))
                        {
                           this.CreateMDIForm(frm);
                        }
                        break;
                    }
                case "barBoPhan":
                    {
                        frmBoPhan frm = new frmBoPhan();
                        if (!this.CheckFormExist(frm.Tag.ToString()))
                        {
                            this.CreateMDIForm(frm);
                        }
                        break;
                    }
                case "barKhachHang":
                    {
                        frmKhachHang frm = new frmKhachHang();
                        if (!this.CheckFormExist(frm.Tag.ToString()))
                        {
                            this.CreateMDIForm(frm);
                        }
                        break;
                    }
                case "barNguonKinhPhi":
                    {
                        frmNguonKinhPhi frm = new frmNguonKinhPhi();
                        if (!this.CheckFormExist(frm.Tag.ToString()))
                        {
                            this.CreateMDIForm(frm);
                        }
                        break;
                    }
                case "barLoaiTaiSan":
                    {
                        frmLoaiTaiSan frm = new frmLoaiTaiSan();
                        if (!this.CheckFormExist(frm.Tag.ToString()))
                        {
                            this.CreateMDIForm(frm);
                        }
                        break;
                    }
                case "barLoaiCCDC":
                    {
                        frmLoaiCCDC frm = new frmLoaiCCDC();
                        if (!this.CheckFormExist(frm.Tag.ToString()))
                        {
                            this.CreateMDIForm(frm);
                        }
                        break;
                    }
                case "barQuyetDinhTrangCap":
                    {
                        frmQuyetDinhTrangCap frm = new frmQuyetDinhTrangCap();
                        if (!this.CheckFormExist(frm.Tag.ToString()))
                        {
                            this.CreateMDIForm(frm);
                        }
                        break;
                    }
                case "barNguoiSuDung":
                    {
                        frmNguoiSuDung frm = new frmNguoiSuDung();
                        if (!this.CheckFormExist(frm.Tag.ToString()))
                        {
                            this.CreateMDIForm(frm);
                        }
                        break;
                    }
                case "barDuAn":
                    {
                        frmDuAn frm = new frmDuAn();
                        if (!this.CheckFormExist(frm.Tag.ToString()))
                        {
                            this.CreateMDIForm(frm);
                        }
                        break;
                    }
                #endregion
                #region Phân hệ báo cáo

                case "BaoCaoCongKhai":
                    {
                        frmDanhSachBaoCao frm = new frmDanhSachBaoCao(1);
                        if (!this.CheckFormExist(frm.Tag.ToString()))
                        {
                            this.CreateMDIForm(frm);
                        }
                        break;
                    }
                case "BaoCaoKeKhaiTSNN":
                    {
                        frmDanhSachBaoCao frm = new frmDanhSachBaoCao(2);
                        if (!this.CheckFormExist(frm.Tag.ToString()))
                        {
                            this.CreateMDIForm(frm);
                        }
                        break;
                    }
                case "BaoCaoTheoCheDoKeToan":
                    {
                        frmDanhSachBaoCao frm = new frmDanhSachBaoCao(3);
                        if (!this.CheckFormExist(frm.Tag.ToString()))
                        {
                            this.CreateMDIForm(frm);
                        }
                        break;
                    }
                case "barGuiBaoCao":
                    {
                        frmUploadFile frm = new frmUploadFile();
                        if (!this.CheckFormExist(frm.Tag.ToString()))
                        {
                            this.CreateMDIForm(frm);
                        }
                        break;
                    }
                case "barNhanBaoCao":
                    {
                        frmSaveFile frm = new frmSaveFile();
                        if (!this.CheckFormExist(frm.Tag.ToString()))
                        {
                            this.CreateMDIForm(frm);
                        }
                        break;
                    }
                #endregion
                #region Phân hệ quản trị hệ thống
              
                case "barbtnDanhSachChucNang":
                    {
                        //frmDanhSachChucNang frm = new frmDanhSachChucNang();
                        //frm.Tag.ToString();
                        //if (!this.CheckFormExist(frm.Tag.ToString()))
                        //{
                        //    this.CreateMDIForm(frm);
                        //}
                        break;
                    }
                case "barThongTinDonVi":
                    {
                        dlgThongTinDonVi frm = new dlgThongTinDonVi();
                        frm.ShowDialog();
                        break;
                    }
                case "barNguoiDung":
                    {
                        frmNguoiDung frm = new frmNguoiDung();
                        frm.Tag.ToString();
                        if (!this.CheckFormExist(frm.Tag.ToString()))
                        {
                            this.CreateMDIForm(frm);
                        }
                        break;
                    }
                case "barNhomNguoiDung":
                    {
                        frmNhomNguoiDung frm = new frmNhomNguoiDung();
                        frm.Tag.ToString();
                        if (!this.CheckFormExist(frm.Tag.ToString()))
                        {
                            this.CreateMDIForm(frm);
                        }
                        break;
                    }
                case "barNhatKyHeThong":
                    {
                        frmNhatKyHeThong frm = new frmNhatKyHeThong();
                        frm.Tag.ToString();
                        if (!this.CheckFormExist(frm.Tag.ToString()))
                        {
                            this.CreateMDIForm(frm);
                        }
                        break;
                    }              
                case "barDoiMatKhau":
                    {
                        frmDoiMatKhau frm = new frmDoiMatKhau();
                        frm.ShowDialog();
                    }
                    break;
                case "barDangXuat":
                    {
                        LogOut();
                    }
                    break;
                case "barThoat":
                    {
                        Application.Exit();
                    }
                    break;                    
                #endregion
            }
        }
        #region Phương thức
        public void CreateWaitDialog()
        {
            dlg = new DevExpress.Utils.WaitDialogForm("Đang tải dữ liệu", "Tải dữ liệu. Xin vui lòng chờ.");
        }
        public void SetWaitDialogCaption(string fCaption)
        {
            if ((dlg != null)) dlg.Caption = fCaption;
        }
        public void CloseWaitDialog()
        {
            if ((dlg != null)) dlg.Close();
        }
        public void CreateMDIForm(System.Windows.Forms.Form frm)
        {
            CreateWaitDialog();
            frm.MdiParent = this;
            frm.Show();
           // this.xtraTabbedMdiManager.SelectedPage.Image = imageCollection1.Images[0];
            CloseWaitDialog();
        }
        public bool CheckFormExist(string tag)
        {
            foreach (Form frm in this.MdiChildren)
            {
                if (frm.Tag == tag)
                {
                    try
                    {
                        frm.Select();
                    }
                    catch (Exception ex)
                    {
                    }
                    return true;
                }
            }
            return false;
        }
#endregion
        private void LogOut()
        {
            {
                foreach (Form frm in this.MdiChildren)
                {
                    if (frm.Tag != "StartPage")
                        frm.Close();
                }
               

                frmLogin frmLogin = new frmLogin();
                if (frmLogin.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                }
                else
                {
                    //Cập nhật lại quyền
                    this.Close();
                    Application.Exit();
                }
            }
        } 

        private void barNam_ItemClick(object sender, ItemClickEventArgs e)
        {
            frmNam frm = new frmNam();
            frm.ShowDialog();
            _Load();
        }

        private void ribbonMain_Click(object sender, EventArgs e)
        {

        }

        private void barButtonItem1_ItemClick(object sender, ItemClickEventArgs e)
        {
            frmBanQuyen frm = new frmBanQuyen();
            frm.ShowDialog();
        }
        public bool TestSQlServer()
        {
            try
            {
                SqlConnection sqlCn = new SqlConnection();
                sqlCn.ConnectionString = "Server=" + ".\\QLTS2015" + "; Database=" + "master" + "; UID=" + "sa" + "; PWD=" + "qlts2015";
                sqlCn.Open();
                sqlCn.Close();
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        protected void ThongBao(string message)
        {
            XtraMessageBox.Show(message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        protected void ThongBaoLoi(string message)
        {
            XtraMessageBox.Show(message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        private void barSaoLuu_ItemClick(object sender, ItemClickEventArgs e)
        {
          
            SaveFileDialog oFile = new SaveFileDialog();

            oFile.Filter = "Text files (*.bak)|*.bak|All files (*.*)|*.*";

            if (oFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string strPath = oFile.FileName;



                if (TestSQlServer())
                {
                    SqlConnection sqlCn = new SqlConnection();
                    SqlCommand sqlCmd = new SqlCommand();
                    DataTable dtb = new DataTable();
                    SqlDataAdapter sqlDA = new SqlDataAdapter();
                    try
                    {
                        sqlCn.ConnectionString = Program.ConnectionAuthenticaMode;
                        //if (Directory.Exists(strPath) == false)

                        sqlCn.Open();
                        sqlCmd.Connection = sqlCn;
                        string strBackup = "BACKUP DATABASE QLTS " +
                                                   "TO  DISK = \'" + strPath + "'" +
                                                        "WITH " +
                                                   "NOFORMAT," +
                                                   "NOINIT," +
                                                   "NAME = N'QLTS-Full Database Backup'," +
                                                   "SKIP," +
                                                   "STATS = 10";
                        sqlCmd.CommandText = strBackup;
                        sqlCmd.ExecuteNonQuery();
                        ThongBao("Đường dẫn dữ liệu sao lưu: " + strPath);
                    }
                    catch (Exception ex)
                    {
                        ThongBaoLoi(ex.Message);
                        sqlDA.Dispose();
                        sqlDA = null;
                        sqlCmd.Dispose();
                        sqlCmd = null;

                    }
                    finally
                    {

                    }
                }
                else
                {

                    ThongBaoLoi("Máy chưa cài sqlserver.Bạn vui lòng cài rồi chọn lại");

                }
            }
        }

        private void barPhucHoi_ItemClick(object sender, ItemClickEventArgs e)
        {
                if (TestSQlServer())
                {
                    OpenFileDialog open = new OpenFileDialog();
                    open.Filter = "Text files (*.bak)|*.bak|All files (*.*)|*.*";
                    open.ShowDialog();
                    if (open.FileName != "")
                    {
                        WaitDialogForm dlg = new WaitDialogForm("Phục hồi dữ liệu", " Đang phục hồi dữ liệu, vui lòng chờ...");
                        dlg.Show();
                        SqlConnection sqlCn = new SqlConnection();
                        SqlCommand sqlCmd = new SqlCommand();
                        DataTable dtb = new DataTable();
                        SqlDataAdapter sqlDA = new SqlDataAdapter();
                        try
                        {
                            SqlConnection sqlcon = new SqlConnection();
                            sqlcon.ConnectionString = Program.ConnectionAuthenticaMode;
                            sqlcon.Open();
                            string stRestore = "USE MASTER; ALTER DATABASE QLTS SET SINGLE_USER WITH ROLLBACK IMMEDIATE;";
                            stRestore += " RESTORE DATABASE QLTS FROM DISK = N'" + open.FileName + "'";
                            stRestore += " WITH REPLACE;";
                            stRestore += " ALTER DATABASE QLTS SET MULTI_USER WITH ROLLBACK IMMEDIATE;";
                            SqlCommand cmd = new SqlCommand(stRestore, sqlcon);
                            cmd.ExecuteNonQuery();
                            sqlcon.Close();
                            ThongBao("Phục hồi dữ liệu thành công");
                        }
                        catch (Exception ex)
                        {
                            ThongBaoLoi(ex.Message);
                            sqlDA.Dispose();
                            sqlDA = null;
                            sqlCmd.Dispose();
                            sqlCmd = null;
                            dlg.Close();
                        }
                        finally
                        {
                            dlg.Close();
                        }
                    }
                }
                else
                {
                    ThongBaoLoi("Máy chưa cài sqlserver.Bạn vui lòng cài rồi chọn lại");
                }
           
        }

        private void barButtonItem2_ItemClick(object sender, ItemClickEventArgs e)
        { 
            System.Diagnostics.Process.Start(Application.StartupPath + "\\HoTroTuXa\\Teamview.exe");
        }

        private void barButtonItem3_ItemClick(object sender, ItemClickEventArgs e)
        {
            System.Diagnostics.Process.Start(Application.StartupPath + "\\HDSD_QLTS\\HDSD_QLTS.chm");
        }


        
    }
}
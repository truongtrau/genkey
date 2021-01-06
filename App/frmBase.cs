using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraLayout;
using DevExpress.XtraTreeList.Nodes;
using QLTS.UI.DB;

namespace QLTS.UI
{
    public partial class frmBase : DevExpress.XtraEditors.XtraForm
    {
        private DevExpress.Utils.WaitDialogForm dlg = null;
        public string strRegistryKey = "Software\\SDCom\\QLTS";   

       
        public frmBase()
        {
            InitializeComponent();
        }
     
        #region THÔNG BÁO
        protected void ThemThanhCong()
        {
            XtraMessageBox.Show("Thêm thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        protected void SuaThanhCong()
        {
            XtraMessageBox.Show("Sửa thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        protected void XoaThanhCong()
        {
            XtraMessageBox.Show("Xóa thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        protected void LuuThanhCong()
        {
            XtraMessageBox.Show("Lưu thông tin thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        protected void ThongBao(string message)
        {
            XtraMessageBox.Show(message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        protected void ThongBaoLoi(string message)
        {
            XtraMessageBox.Show(message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        protected void CanhBao(string message)
        {
            XtraMessageBox.Show(message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        protected void XoaThatBai()
        {
            XtraMessageBox.Show("Dữ liệu đang dùng không thể xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        protected DialogResult ThongBaoChon(string message)
        {
            return XtraMessageBox.Show(message, "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }


        #endregion

        public void CreateWaitDialog(string str1, string str2)
        {
            dlg = new DevExpress.Utils.WaitDialogForm(str2, str1);
        }

        public void CloseWaitDialog()
        {
            if ((dlg != null)) dlg.Close();
        }
        public void CheckAll(DataTable dt, string Field, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                if (dt != null)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        dt.Rows[i][Field] = 1;
                    }
                }
            }
            if (e.Control && e.KeyCode == Keys.X)
            {
                if (dt != null)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        dt.Rows[i][Field] = 0;
                    }
                }
            }
        }
        public void AnButtonFind(object sender)
        {
            LayoutControl lc = (sender as DevExpress.Utils.Win.IPopupControl).PopupWindow.Controls[2].Controls[0] as LayoutControl;
            ((lc.Items[0] as LayoutControlGroup).Items[1] as LayoutControlGroup).Items[1].Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
        }
        public DataTable LinqToDataTable<T>(System.Collections.Generic.IEnumerable<T> varlist)
        {
            try
            {
                DataTable dtReturn = new DataTable();
                System.Reflection.PropertyInfo[] oProps = null;
                if (varlist == null) return dtReturn;
                foreach (T rec in varlist)
                {
                    if (oProps == null)
                    {
                        oProps = ((Type)rec.GetType()).GetProperties();
                        foreach (System.Reflection.PropertyInfo pi in oProps)
                        {
                            Type colType = pi.PropertyType;
                            if ((colType.IsGenericType) && (colType.GetGenericTypeDefinition()
                            == typeof(Nullable<>)))
                            {
                                colType = colType.GetGenericArguments()[0];
                            }
                            dtReturn.Columns.Add(new System.Data.DataColumn(pi.Name, colType));
                        }
                    }
                    DataRow dr = dtReturn.NewRow();
                    foreach (System.Reflection.PropertyInfo pi in oProps)
                    {
                        dr[pi.Name] = pi.GetValue(rec, null) == null ? DBNull.Value :
                        pi.GetValue(rec, null);
                    }
                    dtReturn.Rows.Add(dr);
                }
                return dtReturn;
            }
            catch (Exception ex) { return null; }
        }
        public NhatKyTruyCap GhiLog(string ManHinh, string ChungTu, string HanhDong, string NoiDungThayDoi)
        { 
            DB.NhatKyTruyCap dm = new NhatKyTruyCap();
            dm.ChungTu = ChungTu;
            dm.HanhDong = HanhDong;
            dm.ManHinh = ManHinh;
            dm.NoiDungThayDoi = NoiDungThayDoi;
            dm.NgayThucHien = DateTime.Now;
            dm.IDHT_User = Program.objUserCurrent.Id;
            return dm;
        }
        #region TruongTV
        //TruongTV: STT
        protected void ShowIndicator(DevExpress.XtraGrid.Views.Grid.RowIndicatorCustomDrawEventArgs e)
        {
            if (e.RowHandle < 0)
                return;
            if (e.Info.IsRowIndicator)
            {
                e.Info.DisplayText = (e.RowHandle + 1).ToString();
                e.Info.ImageIndex = -1;
            }
        }
        //TruongTV
        protected bool ShowQuestion(string message)
        {
            if (XtraMessageBox.Show(message, "Thông báo", MessageBoxButtons.YesNo) == DialogResult.Yes)
                return true;
            else
            {
                return false;
            }
        }
        //TruongTV Gán giá trị handle của row đang được lựa chọn       
        public int FocusedRowHandle
        {
            get;
            set;
        }

        //TruongTV Gán giá trị handle của row đang được lựa chọn       
        public TreeListNode FocusedNode
        {
            get;
            set;
        }
        /// <summary>
        /// Trỏ chuột vào row được lựa chọn trên grid, sau khi thực hiện các sự kiện, thêm, sửa, xóa
        /// </summary>
        /// <param name="pGridView">Grid</param>
        /// <param name="col">Tên cột được set handle</param>
        /// <Modifier>
        /// Author      Date        Comments   
        /// TruongTV    18/06/2014  Modified
        /// </Modifier>
        public void SetFocusedRowHandle(ref DevExpress.XtraGrid.Views.Grid.GridView pGridView)
        {
            pGridView.FocusedRowHandle = FocusedRowHandle;
        }

        public void SetFocusedNode(ref DevExpress.XtraTreeList.TreeList pTreeList)
        {
            if (FocusedNode != null && FocusedNode.Id > 0)
            {
                TreeListNode nodeFocus = pTreeList.FindNodeByID(FocusedNode.Id);
                pTreeList.SetFocusedNode(nodeFocus);

            }
        }

        #endregion TruongTV
    }
}
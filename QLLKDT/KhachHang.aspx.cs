using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;

namespace QLLKDT
{
    // Mỗi trang quản lý một bảng. Kết nối SQL dùng chung KetNoi.cs.
    public partial class KhachHang : SecurePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) TaiDanhSach();
        }

        private void TaiDanhSach()
        {
            try
            {
                const string sql = "SELECT [MaKH],[HoTen],[SDT],[DiaChi],[Email] FROM dbo.[KHACHHANG] WHERE (@tim=N'' OR [MaKH] LIKE N'%'+@tim+N'%' OR [HoTen] LIKE N'%'+@tim+N'%') ORDER BY [MaKH]";
                gvDanhSach.DataSource = KetNoi.ExecuteTable(sql, new SqlParameter("@tim", (txtTim.Text ?? "").Trim()));
                gvDanhSach.DataBind();
            }
            catch (SqlException) { ThongBao("Không đọc được dữ liệu. Kiểm tra kết nối và cấu trúc database.", true); }
        }

        protected void btnTim_Click(object sender, EventArgs e) { TaiDanhSach(); }
        protected void btnMoi_Click(object sender, EventArgs e)
        {
            hdMaCu.Value = "";
            txtMaKH.Text = "";
            txtHoTen.Text = "";
            txtSDT.Text = "";
            txtDiaChi.Text = "";
            txtEmail.Text = "";
            txtMaKH.ReadOnly = false;
            ThongBao("Nhập thông tin mới rồi chọn Lưu.", false);
        }

        protected void gvDanhSach_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (gvDanhSach.SelectedDataKey == null) return;
            string ma = Convert.ToString(gvDanhSach.SelectedDataKey.Value);
            try
            {
                DataTable bang = KetNoi.ExecuteTable("SELECT [MaKH],[HoTen],[SDT],[DiaChi],[Email] FROM dbo.[KHACHHANG] WHERE [MaKH]=@ma", new SqlParameter("@ma", ma));
                if (bang.Rows.Count == 0) { ThongBao("Không tìm thấy bản ghi.", true); return; }
                DataRow dong = bang.Rows[0];
                txtMaKH.Text = dong["MaKH"] == DBNull.Value ? "" : Convert.ToString(dong["MaKH"]);
                txtHoTen.Text = dong["HoTen"] == DBNull.Value ? "" : Convert.ToString(dong["HoTen"]);
                txtSDT.Text = dong["SDT"] == DBNull.Value ? "" : Convert.ToString(dong["SDT"]);
                txtDiaChi.Text = dong["DiaChi"] == DBNull.Value ? "" : Convert.ToString(dong["DiaChi"]);
                txtEmail.Text = dong["Email"] == DBNull.Value ? "" : Convert.ToString(dong["Email"]);
                hdMaCu.Value = ma;
                txtMaKH.ReadOnly = true;
            }
            catch (SqlException) { ThongBao("Không đọc được thông tin đã chọn.", true); }
        }

        protected void btnLuu_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(txtMaKH.Text)) { ThongBao("Vui lòng nhập mã khách hàng.", true); return; }
            if (String.IsNullOrWhiteSpace(txtHoTen.Text)) { ThongBao("Vui lòng nhập họ tên.", true); return; }
            if (String.IsNullOrWhiteSpace(txtSDT.Text)) { ThongBao("Vui lòng nhập số điện thoại.", true); return; }
            try
            {
                bool dangSua = !String.IsNullOrEmpty(hdMaCu.Value);
                string sql = dangSua ? "UPDATE dbo.[KHACHHANG] SET [HoTen]=@HoTen,[SDT]=@SDT,[DiaChi]=@DiaChi,[Email]=@Email WHERE [MaKH]=@MaCu" : "INSERT INTO dbo.[KHACHHANG] ([MaKH],[HoTen],[SDT],[DiaChi],[Email]) VALUES (@MaKH,@HoTen,@SDT,@DiaChi,@Email)";
                using (var ketNoi = KetNoi.CreateConnection())
                using (var lenh = new SqlCommand(sql, ketNoi))
                {
                    lenh.Parameters.AddWithValue("@MaKH", String.IsNullOrWhiteSpace(txtMaKH.Text) ? (object)DBNull.Value : txtMaKH.Text.Trim());
                    lenh.Parameters.AddWithValue("@HoTen", String.IsNullOrWhiteSpace(txtHoTen.Text) ? (object)DBNull.Value : txtHoTen.Text.Trim());
                    lenh.Parameters.AddWithValue("@SDT", String.IsNullOrWhiteSpace(txtSDT.Text) ? (object)DBNull.Value : txtSDT.Text.Trim());
                    lenh.Parameters.AddWithValue("@DiaChi", String.IsNullOrWhiteSpace(txtDiaChi.Text) ? (object)DBNull.Value : txtDiaChi.Text.Trim());
                    lenh.Parameters.AddWithValue("@Email", String.IsNullOrWhiteSpace(txtEmail.Text) ? (object)DBNull.Value : txtEmail.Text.Trim());
                    if (dangSua) lenh.Parameters.AddWithValue("@MaCu", hdMaCu.Value);
                    ketNoi.Open();
                    if (lenh.ExecuteNonQuery() != 1) { ThongBao("Không tìm thấy bản ghi cần sửa.", true); return; }
                }
                btnMoi_Click(sender, e);
                ThongBao("Đã lưu thông tin.", false);
                TaiDanhSach();
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601) ThongBao("Mã đã tồn tại.", true);
                else if (ex.Number == 547) ThongBao("Dữ liệu có liên kết không hợp lệ.", true);
                else ThongBao("Không lưu được dữ liệu. Kiểm tra thông tin và database.", true);
            }
        }

        protected void btnXoa_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(hdMaCu.Value)) { ThongBao("Chọn một bản ghi trước khi xóa.", true); return; }
            try
            {
                using (var ketNoi = KetNoi.CreateConnection())
                using (var lenh = new SqlCommand("DELETE FROM dbo.[KHACHHANG] WHERE [MaKH]=@ma", ketNoi))
                {
                    lenh.Parameters.AddWithValue("@ma", hdMaCu.Value);
                    ketNoi.Open();
                    if (lenh.ExecuteNonQuery() != 1) { ThongBao("Không tìm thấy bản ghi.", true); return; }
                }
                btnMoi_Click(sender, e);
                ThongBao("Đã xóa bản ghi.", false);
                TaiDanhSach();
            }
            catch (SqlException ex)
            {
                ThongBao(ex.Number == 547 ? "Không thể xóa vì dữ liệu đang được sử dụng." : "Không xóa được dữ liệu.", true);
            }
        }

        private void ThongBao(string noiDung, bool laLoi)
        {
            pnlThongBao.Visible = true;
            pnlThongBao.CssClass = laLoi ? "alert bad" : "alert";
            litThongBao.Text = HttpUtility.HtmlEncode(noiDung);
        }
    }
}

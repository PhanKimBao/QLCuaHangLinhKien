using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;

namespace QLLKDT
{
    // Mỗi trang quản lý một bảng. Kết nối SQL dùng chung KetNoi.cs.
    public partial class NhanVien : SecurePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) TaiDanhSach();
        }

        private void TaiDanhSach()
        {
            try
            {
                const string sql = "SELECT [MaNV],[HoTen],[NgaySinh],[GioiTinh],[SDT],[DiaChi],[Email],[ChucVu],[TrangThai],[SoCCCD] FROM dbo.[NHANVIEN] WHERE (@tim=N'' OR [MaNV] LIKE N'%'+@tim+N'%' OR [HoTen] LIKE N'%'+@tim+N'%') ORDER BY [MaNV]";
                gvDanhSach.DataSource = KetNoi.ExecuteTable(sql, new SqlParameter("@tim", (txtTim.Text ?? "").Trim()));
                gvDanhSach.DataBind();
            }
            catch (SqlException) { ThongBao("Không đọc được dữ liệu. Kiểm tra kết nối và cấu trúc database.", true); }
        }

        protected void btnTim_Click(object sender, EventArgs e) { TaiDanhSach(); }
        protected void btnMoi_Click(object sender, EventArgs e)
        {
            hdMaCu.Value = "";
            txtMaNV.Text = "";
            txtHoTen.Text = "";
            txtNgaySinh.Text = "";
            txtGioiTinh.Text = "";
            txtSDT.Text = "";
            txtDiaChi.Text = "";
            txtEmail.Text = "";
            txtChucVu.Text = "";
            txtTrangThai.Text = "";
            txtSoCCCD.Text = "";
            txtMaNV.ReadOnly = false;
            ThongBao("Nhập thông tin mới rồi chọn Lưu.", false);
        }

        protected void gvDanhSach_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (gvDanhSach.SelectedDataKey == null) return;
            string ma = Convert.ToString(gvDanhSach.SelectedDataKey.Value);
            try
            {
                DataTable bang = KetNoi.ExecuteTable("SELECT [MaNV],[HoTen],[NgaySinh],[GioiTinh],[SDT],[DiaChi],[Email],[ChucVu],[TrangThai],[SoCCCD] FROM dbo.[NHANVIEN] WHERE [MaNV]=@ma", new SqlParameter("@ma", ma));
                if (bang.Rows.Count == 0) { ThongBao("Không tìm thấy bản ghi.", true); return; }
                DataRow dong = bang.Rows[0];
                txtMaNV.Text = dong["MaNV"] == DBNull.Value ? "" : Convert.ToString(dong["MaNV"]);
                txtHoTen.Text = dong["HoTen"] == DBNull.Value ? "" : Convert.ToString(dong["HoTen"]);
                txtNgaySinh.Text = dong["NgaySinh"] == DBNull.Value ? "" : Convert.ToDateTime(dong["NgaySinh"]).ToString("yyyy-MM-dd");
                txtGioiTinh.Text = dong["GioiTinh"] == DBNull.Value ? "" : Convert.ToString(dong["GioiTinh"]);
                txtSDT.Text = dong["SDT"] == DBNull.Value ? "" : Convert.ToString(dong["SDT"]);
                txtDiaChi.Text = dong["DiaChi"] == DBNull.Value ? "" : Convert.ToString(dong["DiaChi"]);
                txtEmail.Text = dong["Email"] == DBNull.Value ? "" : Convert.ToString(dong["Email"]);
                txtChucVu.Text = dong["ChucVu"] == DBNull.Value ? "" : Convert.ToString(dong["ChucVu"]);
                txtTrangThai.Text = dong["TrangThai"] == DBNull.Value ? "" : Convert.ToString(dong["TrangThai"]);
                txtSoCCCD.Text = dong["SoCCCD"] == DBNull.Value ? "" : Convert.ToString(dong["SoCCCD"]);
                hdMaCu.Value = ma;
                txtMaNV.ReadOnly = true;
            }
            catch (SqlException) { ThongBao("Không đọc được thông tin đã chọn.", true); }
        }

        protected void btnLuu_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(txtMaNV.Text)) { ThongBao("Vui lòng nhập mã nhân viên.", true); return; }
            if (String.IsNullOrWhiteSpace(txtHoTen.Text)) { ThongBao("Vui lòng nhập họ tên.", true); return; }
            if (String.IsNullOrWhiteSpace(txtSDT.Text)) { ThongBao("Vui lòng nhập số điện thoại.", true); return; }
            if (String.IsNullOrWhiteSpace(txtChucVu.Text)) { ThongBao("Vui lòng nhập chức vụ.", true); return; }
            if (String.IsNullOrWhiteSpace(txtTrangThai.Text)) { ThongBao("Vui lòng nhập trạng thái.", true); return; }
            try
            {
                bool dangSua = !String.IsNullOrEmpty(hdMaCu.Value);
                DateTime ngayNgaySinh;
                if (!String.IsNullOrWhiteSpace(txtNgaySinh.Text) && !DateTime.TryParseExact(txtNgaySinh.Text, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out ngayNgaySinh)) { ThongBao("Ngày sinh không hợp lệ.", true); return; }
                string sql = dangSua ? "UPDATE dbo.[NHANVIEN] SET [HoTen]=@HoTen,[NgaySinh]=@NgaySinh,[GioiTinh]=@GioiTinh,[SDT]=@SDT,[DiaChi]=@DiaChi,[Email]=@Email,[ChucVu]=@ChucVu,[TrangThai]=@TrangThai,[SoCCCD]=@SoCCCD WHERE [MaNV]=@MaCu" : "INSERT INTO dbo.[NHANVIEN] ([MaNV],[HoTen],[NgaySinh],[GioiTinh],[SDT],[DiaChi],[Email],[ChucVu],[TrangThai],[SoCCCD]) VALUES (@MaNV,@HoTen,@NgaySinh,@GioiTinh,@SDT,@DiaChi,@Email,@ChucVu,@TrangThai,@SoCCCD)";
                using (var ketNoi = KetNoi.CreateConnection())
                using (var lenh = new SqlCommand(sql, ketNoi))
                {
                    lenh.Parameters.AddWithValue("@MaNV", String.IsNullOrWhiteSpace(txtMaNV.Text) ? (object)DBNull.Value : txtMaNV.Text.Trim());
                    lenh.Parameters.AddWithValue("@HoTen", String.IsNullOrWhiteSpace(txtHoTen.Text) ? (object)DBNull.Value : txtHoTen.Text.Trim());
                    lenh.Parameters.AddWithValue("@NgaySinh", String.IsNullOrWhiteSpace(txtNgaySinh.Text) ? (object)DBNull.Value : DateTime.ParseExact(txtNgaySinh.Text, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture));
                    lenh.Parameters.AddWithValue("@GioiTinh", String.IsNullOrWhiteSpace(txtGioiTinh.Text) ? (object)DBNull.Value : txtGioiTinh.Text.Trim());
                    lenh.Parameters.AddWithValue("@SDT", String.IsNullOrWhiteSpace(txtSDT.Text) ? (object)DBNull.Value : txtSDT.Text.Trim());
                    lenh.Parameters.AddWithValue("@DiaChi", String.IsNullOrWhiteSpace(txtDiaChi.Text) ? (object)DBNull.Value : txtDiaChi.Text.Trim());
                    lenh.Parameters.AddWithValue("@Email", String.IsNullOrWhiteSpace(txtEmail.Text) ? (object)DBNull.Value : txtEmail.Text.Trim());
                    lenh.Parameters.AddWithValue("@ChucVu", String.IsNullOrWhiteSpace(txtChucVu.Text) ? (object)DBNull.Value : txtChucVu.Text.Trim());
                    lenh.Parameters.AddWithValue("@TrangThai", String.IsNullOrWhiteSpace(txtTrangThai.Text) ? (object)DBNull.Value : txtTrangThai.Text.Trim());
                    lenh.Parameters.AddWithValue("@SoCCCD", String.IsNullOrWhiteSpace(txtSoCCCD.Text) ? (object)DBNull.Value : txtSoCCCD.Text.Trim());
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
                using (var lenh = new SqlCommand("DELETE FROM dbo.[NHANVIEN] WHERE [MaNV]=@ma", ketNoi))
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

using System;
using System.Data;
using System.Data.SqlClient;

namespace QLLKDT
{
    // Tra cứu tồn kho. Kết nối dùng lớp KetNoi chung của project.
    public partial class Kho : SecurePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) TaiTonKho();
        }

        protected void btnTim_Click(object sender, EventArgs e)
        {
            TaiTonKho();
        }

        private void TaiTonKho()
        {
            try
            {
                const string sql = @"SELECT t.MaKho, t.MaLK, l.TenLK, t.SoLuongTon, t.ViTri
                    FROM dbo.TONKHO AS t INNER JOIN dbo.LINHKIEN AS l ON l.MaLK = t.MaLK
                    WHERE @tim = N'' OR t.MaKho LIKE N'%' + @tim + N'%'
                    OR t.MaLK LIKE N'%' + @tim + N'%' OR l.TenLK LIKE N'%' + @tim + N'%'
                    ORDER BY t.MaKho, l.TenLK";
                var thamSo = new SqlParameter("@tim", SqlDbType.NVarChar, 100);
                thamSo.Value = txtTim.Text.Trim();
                gvTonKho.DataSource = KetNoi.ExecuteTable(sql, thamSo);
                gvTonKho.DataBind();
            }
            catch (SqlException)
            {
                pnlThongBao.Visible = true;
                litThongBao.Text = "Không tải được tồn kho. Hãy kiểm tra kết nối và các bảng TONKHO, LINHKIEN.";
            }
        }
    }
}

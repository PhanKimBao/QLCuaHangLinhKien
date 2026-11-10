using System;
using System.Collections.Generic;
using System.IO;
namespace QLLKDT
{
    // Đường dẫn của từng chức năng và tên quyền tương ứng.
    public static class DuongDan
    {
        private static readonly Dictionary<string, string> Trang = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "tongquan", "TongQuan.aspx" },
            { "banhang", "BanHang.aspx" },
            { "sanpham", "SanPham.aspx" },
            { "danhmuc", "DanhMuc.aspx" },
            { "hoadon", "HoaDon.aspx" },
            { "baohanh", "BaoHanh.aspx" },
            { "baocao", "BaoCao.aspx" },
            { "phieunhap", "NhapHang.aspx" },
            { "taikhoan", "TaiKhoan.aspx" },
            { "phanquyen", "PhanQuyen.aspx" },
            { "caidat", "CaiDat.aspx" },
            { "backup", "SaoLuu.aspx" },
            { "nhanvien", "NhanVien.aspx" },
            { "khachhang", "KhachHang.aspx" },
            { "nhacungcap", "NhaCungCap.aspx" },
        };
        public static string ChucNang(string path, string macDinh)
        {
            string file = Path.GetFileName(path);
            if (String.Equals(file, "Default.aspx", StringComparison.OrdinalIgnoreCase)) return "sanpham";
            foreach (var item in Trang)
                if (String.Equals(file, item.Value, StringComparison.OrdinalIgnoreCase)) return item.Key;
            return (macDinh ?? "tongquan").ToLowerInvariant();
        }
        public static string LayTrang(string chucNang)
        {
            string trang;
            return Trang.TryGetValue(chucNang, out trang) ? trang : "QuanLy.aspx";
        }
    }
}

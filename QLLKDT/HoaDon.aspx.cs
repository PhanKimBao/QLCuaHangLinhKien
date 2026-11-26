using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;

namespace QLLKDT
{
    // Xử lý của form HoaDon.
    public partial class HoaDon : QuanLy
    {
        void InvoiceTable(DataTable t)
        {
            b.Append("<table class='datagrid'><thead><tr><th>Mã hóa đơn</th><th>Ngày lập</th><th>Thành tiền</th><th>Trạng thái</th><th></th></tr></thead><tbody>");
            foreach(DataRow r in t.Rows)b.Append("<tr><td>"+E(r["MaHD"])+"</td><td>"+E(Convert.ToDateTime(r["NgayLap"]).ToString("dd/MM/yyyy HH:mm"))+"</td><td>"+E(Money(r["ThanhTien"]))+"</td><td>"+E(r["TrangThai"])+"</td><td><a href='"+E(Url("hoadon","detail",Convert.ToString(r["MaHD"])))+"'>Chi tiết</a></td></tr>");
            b.Append("</tbody></table>");
        }

        protected override void Invoices()
        {
            if(action=="detail")
            {
                DataTable t=Q("SELECT h.*,k.HoTen AS KhachHang,n.HoTen AS NhanVien FROM dbo.HOADON h JOIN dbo.KHACHHANG k ON h.MaKH=k.MaKH JOIN dbo.NHANVIEN n ON h.MaNV=n.MaNV WHERE h.MaHD=@id",P("@id",id));
                if(t.Rows.Count==0)throw new Exception("Không tìm thấy hóa đơn.");
                DataRow r=t.Rows[0];
                b.Append("<div class='title'><div><h1>Chi tiết hóa đơn "+E(id)+"</h1><p>Ngày lập: "+E(Convert.ToDateTime(r["NgayLap"]).ToString("dd/MM/yyyy HH:mm"))+"</p></div></div><section class='contentbox'><p>Khách hàng: <b>"+E(r["KhachHang"])+"</b> &nbsp; | &nbsp; Nhân viên: <b>"+E(r["NhanVien"])+"</b></p><table class='datagrid'><thead><tr><th>Mã linh kiện</th><th>Tên sản phẩm</th><th>Số lượng</th><th>Đơn giá</th><th>Thành tiền</th></tr></thead><tbody>");
                foreach(DataRow line in Q("SELECT c.MaLK,l.TenLK,c.SoLuong,c.DonGiaBan,c.ThanhTien FROM dbo.CT_HOADON c JOIN dbo.LINHKIEN l ON l.MaLK=c.MaLK WHERE c.MaHD=@id",P("@id",id)).Rows)b.Append("<tr><td>"+E(line[0])+"</td><td>"+E(line[1])+"</td><td>"+E(line[2])+"</td><td>"+E(Money(line[3]))+"</td><td>"+E(Money(line[4]))+"</td></tr>");
                b.Append("</tbody></table><p>Giảm giá: "+E(r["GiamGia"]==DBNull.Value?"0 đ":Money(r["GiamGia"]))+"</p><p><strong>Thành tiền: "+E(Money(r["ThanhTien"]))+"</strong></p><p>Tiền khách đưa: "+E(r["TienKhachDua"]==DBNull.Value?"—":Money(r["TienKhachDua"]))+" | Tiền thối: "+E(r["TienThoi"]==DBNull.Value?"—":Money(r["TienThoi"]))+"</p><p>Trạng thái: "+E(r["TrangThai"])+"</p><div class='toolbar'><a class='btnlink secondary' href='"+E(Url("hoadon",null,null))+"'>Quay lại</a><button class='btn' onclick='window.print()'>In hóa đơn</button></div></section>");
                return;
            }
            b.Append("<div class='title'><div><h1>Hóa đơn</h1><p>Xem chi tiết và in hóa đơn</p></div><a class='btnlink' href='"+E(Url("banhang",null,null))+"'>＋ Lập hóa đơn</a></div><section class='contentbox'>");
            InvoiceTable(Q("SELECT MaHD,NgayLap,ThanhTien,TrangThai FROM dbo.HOADON ORDER BY NgayLap DESC,MaHD DESC"));
            b.Append("</section>");
        }
    }
}

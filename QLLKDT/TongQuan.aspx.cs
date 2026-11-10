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
    // Xử lý của form TongQuan.
    public partial class TongQuan : QuanLy
    {
        protected override void Dashboard()
        {
            DataRow r=Q(@"SELECT (SELECT COUNT(*) FROM dbo.LINHKIEN) AS sp,(SELECT COUNT(*) FROM dbo.KHACHHANG) AS kh,(SELECT COUNT(*) FROM dbo.HOADON WHERE NgayLap>=CONVERT(date,GETDATE()) AND NgayLap<DATEADD(day,1,CONVERT(date,GETDATE()))) AS hd,(SELECT ISNULL(SUM(ThanhTien),0) FROM dbo.HOADON WHERE NgayLap>=CONVERT(date,GETDATE()) AND NgayLap<DATEADD(day,1,CONVERT(date,GETDATE()))) AS tien").Rows[0];
            b.Append("<div class='title'><div><h1>Tổng quan cửa hàng</h1><p>Theo dõi hoạt động bán hàng và linh kiện</p></div></div><div class='kpi'><div class='stat'><section>Sản phẩm<strong>"+E(r["sp"])+"</strong></section></div><div class='stat'><section>Khách hàng<strong>"+E(r["kh"])+"</strong></section></div><div class='stat'><section>Hóa đơn hôm nay<strong>"+E(r["hd"])+"</strong></section></div><div class='stat'><section>Giá trị hóa đơn hôm nay<strong>"+E(Money(r["tien"]))+"</strong></section></div></div><section class='contentbox'><h2>Hóa đơn gần đây</h2>");
            InvoiceTable(Q("SELECT TOP 8 MaHD,NgayLap,ThanhTien,TrangThai FROM dbo.HOADON ORDER BY NgayLap DESC,MaHD DESC"));
            b.Append("<p class='hint'>Giá trị hóa đơn được tính từ ThanhTien, gồm tất cả trạng thái; không đồng nghĩa số tiền đã thu.</p></section>");
        }
        void InvoiceTable(DataTable t)
        {
            b.Append("<table class='datagrid'><thead><tr><th>Mã hóa đơn</th><th>Ngày lập</th><th>Thành tiền</th><th>Trạng thái</th><th></th></tr></thead><tbody>");
            foreach(DataRow r in t.Rows)b.Append("<tr><td>"+E(r["MaHD"])+"</td><td>"+E(Convert.ToDateTime(r["NgayLap"]).ToString("dd/MM/yyyy HH:mm"))+"</td><td>"+E(Money(r["ThanhTien"]))+"</td><td>"+E(r["TrangThai"])+"</td><td><a href='"+E(Url("hoadon","detail",Convert.ToString(r["MaHD"])))+"'>Chi tiết</a></td></tr>");
            b.Append("</tbody></table>");
        }

    }
}

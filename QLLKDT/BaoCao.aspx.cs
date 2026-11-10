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
    // Xử lý của form BaoCao.
    public partial class BaoCao : QuanLy
    {
        DataTable ReportData()
        {
            return Q(@"SELECT CONVERT(date,NgayLap) AS Ngay, COUNT(*) AS SoHoaDon, SUM(ThanhTien) AS GiaTri FROM dbo.HOADON GROUP BY CONVERT(date,NgayLap) ORDER BY Ngay DESC");
        }

        protected override void Report()
        {
            b.Append("<div class='title'><div><h1>Báo cáo bán hàng</h1><p>Tổng hợp giá trị hóa đơn theo ngày lập</p></div><a class='btnlink' href='QuanLy.aspx?m=baocao&amp;export=csv'>↓ Tải CSV mở bằng Excel</a></div><section class='contentbox'><table class='datagrid'><thead><tr><th>Ngày</th><th>Số hóa đơn</th><th>Tổng giá trị</th></tr></thead><tbody>");
            foreach(DataRow r in ReportData().Rows)b.Append("<tr><td>"+E(Convert.ToDateTime(r[0]).ToString("dd/MM/yyyy"))+"</td><td>"+E(r[1])+"</td><td>"+E(Money(r[2]))+"</td></tr>");
            b.Append("</tbody></table><p class='hint'>Tổng hợp từ ThanhTien của tất cả hóa đơn, chưa lọc theo trạng thái thanh toán hoặc hủy.</p></section>");
        }

        protected override void ExportReport()
        {
            DataTable t=ReportData();
            exported=true;
            Response.Clear();
            Response.ContentType="text/csv";
            Response.ContentEncoding=new UTF8Encoding(true);
            Response.AddHeader("Content-Disposition","attachment; filename=bao-cao-hoa-don.csv");
            Response.BinaryWrite(new byte[]
            {
                0xEF,0xBB,0xBF
            }
            );
            Response.Write("Ngày,Số hóa đơn,Tổng giá trị\r\n");
            foreach(DataRow r in t.Rows)Response.Write(Convert.ToDateTime(r[0]).ToString("yyyy-MM-dd")+","+Convert.ToString(r[1],CultureInfo.InvariantCulture)+","+Convert.ToDecimal(r[2]).ToString("0.00",CultureInfo.InvariantCulture)+"\r\n");
            Context.ApplicationInstance.CompleteRequest();
        }

        protected override void AdvancedReport()
        {
            int year,month;
            if(!int.TryParse(Request.QueryString["year"],out year)||year<2000||year>2100)year=DateTime.Now.Year;
            if(!int.TryParse(Request.QueryString["month"],out month)||month<0||month>12)month=0;
            DateTime start=new DateTime(year,month==0?1:month,1),end=month==0?start.AddYears(1):start.AddMonths(1);
            b.Append("<section class='contentbox'><h2>Thống kê theo tháng / năm</h2><form class='toolbar' method='get'><input type='hidden' name='m' value='baocao'><input type='number' name='year' value='"+year+"' min='2000' max='2100' required><select name='month'><option value='0'>Cả năm</option>");
            for(int i=1;i<=12;i++)b.Append("<option value='"+i+"' "+(month==i?"selected":"")+">Tháng "+i+"</option>");
            b.Append("</select><button class='btn'>Xem thống kê</button></form>");
            var args=new[]
            {
                P("@start",start),P("@end",end)
            }
            ;
            DataRow summary=Q(@"SELECT (SELECT ISNULL(SUM(SoTien),0) FROM dbo.THANHTOAN WHERE TrangThai=N'Thành công' AND NgayThanhToan>=@start AND NgayThanhToan<@end) AS DaThu,(SELECT ISNULL(SUM(TongTien),0) FROM dbo.PHIEUNHAP WHERE NgayNhap>=@start AND NgayNhap<@end) AS GiaTriNhap",args).Rows[0];
            b.Append("<div class='kpi'><div class='stat'><section>Tiền đã thanh toán<strong>"+E(Money(summary[0]))+"</strong></section></div><div class='stat'><section>Giá trị phiếu nhập<strong>"+E(Money(summary[1]))+"</strong></section></div></div><h2>Top 3 linh kiện trong hóa đơn đã thanh toán</h2><table class='datagrid'><tr><th>Linh kiện</th><th>Số lượng</th><th>Giá trị dòng hàng</th></tr>");
            foreach(DataRow r in Q(@"SELECT TOP 3 l.TenLK,SUM(c.SoLuong),SUM(c.ThanhTien) FROM dbo.CT_HOADON c JOIN dbo.HOADON h ON h.MaHD=c.MaHD JOIN dbo.LINHKIEN l ON l.MaLK=c.MaLK WHERE h.NgayLap>=@start AND h.NgayLap<@end AND EXISTS(SELECT 1 FROM dbo.THANHTOAN t WHERE t.MaHD=h.MaHD AND t.TrangThai=N'Thành công') GROUP BY l.TenLK ORDER BY SUM(c.SoLuong) DESC,l.TenLK",P("@start",start),P("@end",end)).Rows)b.Append("<tr><td>"+E(r[0])+"</td><td>"+E(r[1])+"</td><td>"+E(Money(r[2]))+"</td></tr>");
            b.Append("</table><h2>Top 3 khách hàng theo giá trị hóa đơn đã thanh toán</h2><table class='datagrid'><tr><th>Khách hàng</th><th>Số hóa đơn</th><th>Giá trị hóa đơn</th></tr>");
            foreach(DataRow r in Q(@"SELECT TOP 3 k.HoTen,COUNT(*),SUM(h.ThanhTien) FROM dbo.HOADON h JOIN dbo.KHACHHANG k ON k.MaKH=h.MaKH WHERE h.NgayLap>=@start AND h.NgayLap<@end AND EXISTS(SELECT 1 FROM dbo.THANHTOAN t WHERE t.MaHD=h.MaHD AND t.TrangThai=N'Thành công') GROUP BY k.MaKH,k.HoTen ORDER BY SUM(h.ThanhTien) DESC,k.HoTen",P("@start",start),P("@end",end)).Rows)b.Append("<tr><td>"+E(r[0])+"</td><td>"+E(r[1])+"</td><td>"+E(Money(r[2]))+"</td></tr>");
            b.Append("</table><p class='hint'>Tiền đã thanh toán tính theo ngày thanh toán; top sản phẩm và khách hàng tính theo ngày lập hóa đơn. Giá trị phiếu nhập không phải chi phí đã trả hay lợi nhuận.</p></section>");
        }
    }
}

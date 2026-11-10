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
    // Xử lý của form NhapHang.
    public partial class NhapHang : QuanLy
    {
        protected override void ImportPage()
        {
            b.Append("<h1>Phiếu nhập hàng</h1><section class='contentbox'>");
            ImportCreate();
            b.Append("<h2>Danh sách phiếu nhập</h2><table class='datagrid'><tr><th>Mã phiếu</th><th>Nhà cung cấp</th><th>Nhân viên</th><th>Ngày nhập</th><th>Tổng tiền</th><th></th></tr>");
            foreach(DataRow r in Q("SELECT MaPN,MaNCC,MaNV,NgayNhap,TongTien FROM dbo.PHIEUNHAP ORDER BY NgayNhap DESC").Rows)b.Append("<tr><td>"+E(r[0])+"</td><td>"+E(r[1])+"</td><td>"+E(r[2])+"</td><td>"+E(Convert.ToDateTime(r[3]).ToString("dd/MM/yyyy"))+"</td><td>"+E(Money(r[4]))+"</td><td><a href='"+E(Url("phieunhap","detail",Convert.ToString(r[0])))+"'>Chi tiết</a></td></tr>");
            b.Append("</table>");
            if(action=="detail")
            {
                b.Append("<h2>Chi tiết phiếu "+E(id)+"</h2><table class='datagrid'><tr><th>Mã linh kiện</th><th>Số lượng</th><th>Đơn giá nhập</th><th>Thành tiền</th></tr>");
                foreach(DataRow r in Q("SELECT MaLK,SoLuong,DonGiaNhap,ThanhTien FROM dbo.CT_PHIEUNHAP WHERE MaPN=@id",P("@id",id)).Rows)b.Append("<tr><td>"+E(r[0])+"</td><td>"+E(r[1])+"</td><td>"+E(Money(r[2]))+"</td><td>"+E(Money(r[3]))+"</td></tr>");
                b.Append("</table>");
            }
            b.Append("</section>");
        }

        protected override void AddImport()
        {
            string id=(Request.Form["product"]??"").Trim();
            int qty;
            decimal price;
            if(id.Length==0||id.Length>10||!int.TryParse(Request.Form["qty"],out qty)||qty<1||qty>9999||!decimal.TryParse(Request.Form["price"],NumberStyles.AllowDecimalPoint,CultureInfo.InvariantCulture,out price)||price<0||price>999999999999m)throw new Exception("Vui lòng kiểm tra linh kiện, số lượng và đơn giá nhập.");
            if(Q("SELECT MaLK FROM dbo.LINHKIEN WHERE MaLK=@id",P("@id",id)).Rows.Count!=1)throw new Exception("Không tìm thấy linh kiện.");
            if(ImportCart.ContainsKey(id))throw new Exception("Linh kiện đã có trong phiếu. Xóa dòng cũ trước khi nhập lại.");
            ImportCart[id]=new ImportLine
            {
                Qty=qty,Price=price
            }
            ;
            msg="Đã thêm vào phiếu nhập.";
        }

        protected override void RemoveImport()
        {
            ImportCart.Remove(Request.Form["key"]??"");
            msg="Đã xóa dòng trong phiếu nhập.";
        }

        protected override void SaveImport()
        {
            if(ImportCart.Count==0)throw new Exception("Phiếu nhập chưa có linh kiện.");
            string supplier=(Request.Form["supplier"]??"").Trim(),employee=(Request.Form["employee"]??"").Trim(),note=(Request.Form["note"]??"").Trim();
            if(supplier.Length==0||supplier.Length>10||employee.Length==0||employee.Length>10||note.Length>255)throw new Exception("Kiểm tra nhà cung cấp, nhân viên và ghi chú.");
            string code="PN"+Guid.NewGuid().ToString("N").Substring(0,8).ToUpperInvariant();
            decimal total=ImportCart.Sum(x=>x.Value.Price*x.Value.Qty);
            using(var c=KetNoi.CreateConnection())
            {
                c.Open();
                using(var tx=c.BeginTransaction())
                {
                    using(var cmd=new SqlCommand("INSERT dbo.PHIEUNHAP(MaPN,MaNCC,MaNV,NgayNhap,TongTien,GhiChu) VALUES(@id,@supplier,@nv,GETDATE(),@total,@note)",c,tx))
                    {
                        cmd.Parameters.Add(P("@id",code));
                        cmd.Parameters.Add(P("@supplier",supplier));
                        cmd.Parameters.Add(P("@nv",employee));
                        cmd.Parameters.Add(P("@total",total));
                        cmd.Parameters.Add(P("@note",note.Length==0?(object)DBNull.Value:note));
                        cmd.ExecuteNonQuery();
                    }
                    foreach(var item in ImportCart)
                    {
                        using(var cmd=new SqlCommand("INSERT dbo.CT_PHIEUNHAP(MaPN,MaLK,SoLuong,DonGiaNhap,ThanhTien,ChietKhau) VALUES(@id,@lk,@qty,@price,@sum,0)",c,tx))
                        {
                            cmd.Parameters.Add(P("@id",code));
                            cmd.Parameters.Add(P("@lk",item.Key));
                            cmd.Parameters.Add(P("@qty",item.Value.Qty));
                            cmd.Parameters.Add(P("@price",item.Value.Price));
                            cmd.Parameters.Add(P("@sum",item.Value.Price*item.Value.Qty));
                            cmd.ExecuteNonQuery();
                        }
                    }
                    tx.Commit();
                }
            }
            ImportCart.Clear();
            action="detail";
            id=code;
            msg="Đã tạo phiếu nhập: "+code;
        }

        void ImportCreate()
        {
            b.Append("<h2>Tạo phiếu nhập</h2><p class='hint'>Phiếu nhập ghi nhận chi tiết hàng mua, chưa tự động cộng số lượng vào TONKHO.</p><form method='post' class='toolbar'>"+Hidden("addimport")+"<select name='product' required><option value=''>-- Chọn linh kiện --</option>");
            foreach(DataRow r in Q("SELECT MaLK,TenLK FROM dbo.LINHKIEN ORDER BY TenLK").Rows)b.Append("<option value='"+E(r[0])+"'>"+E(r[1])+" ("+E(r[0])+")</option>");
            b.Append("</select><input type='number' name='qty' value='1' min='1' max='9999' required><input type='number' name='price' value='0' min='0' step='0.01' required placeholder='Đơn giá nhập'><button class='btn'>Thêm dòng</button></form><table class='datagrid'><tr><th>Mã linh kiện</th><th>Số lượng</th><th>Giá nhập</th><th>Thành tiền</th><th></th></tr>");
            decimal total=0;
            foreach(var line in ImportCart)
            {
                decimal subtotal=line.Value.Qty*line.Value.Price;
                total+=subtotal;
                b.Append("<tr><td>"+E(line.Key)+"</td><td>"+line.Value.Qty+"</td><td>"+E(Money(line.Value.Price))+"</td><td>"+E(Money(subtotal))+"</td><td><form method='post' class='inline'>"+Hidden("removeimport")+"<input type='hidden' name='key' value='"+E(line.Key)+"'><button>Xóa</button></form></td></tr>");
            }
            b.Append("</table><p><b>Tổng: "+E(Money(total))+"</b></p><form method='post'>"+Hidden("saveimport")+"<div class='formgrid'><div class='field'><label>Nhà cung cấp *</label><select name='supplier' required><option value=''>-- Chọn --</option>");
            foreach(DataRow r in Q("SELECT MaNCC,TenNCC FROM dbo.NHACUNGCAP ORDER BY TenNCC").Rows)b.Append("<option value='"+E(r[0])+"'>"+E(r[1])+"</option>");
            b.Append("</select></div><div class='field'><label>Nhân viên *</label><select name='employee' required><option value=''>-- Chọn --</option>");
            foreach(DataRow r in Q("SELECT MaNV,HoTen FROM dbo.NHANVIEN ORDER BY HoTen").Rows)b.Append("<option value='"+E(r[0])+"'>"+E(r[1])+"</option>");
            b.Append("</select></div><div class='field'><label>Ghi chú</label><input name='note' maxlength='255'></div></div><div class='toolbar'><button class='btn' "+(ImportCart.Count==0?"disabled":"")+">Lưu phiếu nhập</button></div></form><hr>");
        }
    }
}

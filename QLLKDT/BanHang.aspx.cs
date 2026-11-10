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
    // Xử lý của form BanHang.
    public partial class BanHang : QuanLy
    {
        protected override void AddCart()
        {
            string key=Request.Form["product"]??"";
            int n;
            if(key.Length>10||!Int32.TryParse(Request.Form["qty"],out n)||n<=0||n>9999)throw new Exception("Số lượng phải từ 1 đến 9.999.");
            if(Q("SELECT MaLK FROM dbo.LINHKIEN WHERE MaLK=@id",P("@id",key)).Rows.Count!=1)throw new Exception("Không tìm thấy sản phẩm.");
            if(Cart.ContainsKey(key))
            {
                if(Cart[key]+n>9999)throw new Exception("Tổng số lượng một sản phẩm không được quá 9.999.");
                Cart[key]+=n;
            }
            else Cart[key]=n;
            msg="Đã thêm sản phẩm vào giỏ.";
        }

        protected override void RemoveCart()
        {
            Cart.Remove(Request.Form["key"]??"");
            msg="Đã xóa sản phẩm khỏi giỏ.";
        }

        protected override void Sales()
        {
            b.Append("<div class='title'><div><h1>Bán hàng</h1><p>Chọn sản phẩm và lập hóa đơn</p></div></div><section class='contentbox'><h2>Thêm sản phẩm</h2><form method='post' action='"+E(Url("banhang",null,null))+"' class='toolbar'>"+Hidden("addcart")+"<select name='product' required><option value=''>-- Chọn sản phẩm --</option>");
            foreach(DataRow r in Q("SELECT MaLK,TenLK,GiaBan FROM dbo.LINHKIEN ORDER BY TenLK").Rows)b.Append("<option value='"+E(r[0])+"'>"+E(r[1])+" — "+E(Money(r[2]))+"</option>");
            b.Append("</select><input type='number' name='qty' value='1' min='1' max='9999' required><button class='btn'>＋ Thêm vào giỏ</button></form><h2>Giỏ hàng</h2><table class='datagrid'><thead><tr><th>Sản phẩm</th><th>Số lượng</th><th>Đơn giá hiện tại</th><th>Thành tiền</th><th></th></tr></thead><tbody>");
            decimal total=0;
            foreach(var item in Cart.ToList())
            {
                DataTable t=Q("SELECT TenLK,GiaBan FROM dbo.LINHKIEN WHERE MaLK=@id",P("@id",item.Key));
                if(t.Rows.Count==0)
                {
                    Cart.Remove(item.Key);
                    continue;
                }
                decimal price=Convert.ToDecimal(t.Rows[0]["GiaBan"]),sum=price*item.Value;
                total+=sum;
                b.Append("<tr><td>"+E(t.Rows[0]["TenLK"])+"</td><td>"+item.Value+"</td><td>"+E(Money(price))+"</td><td>"+E(Money(sum))+"</td><td><form class='inline' method='post'>"+Hidden("removecart")+"<input type='hidden' name='key' value='"+E(item.Key)+"'><button>Xóa</button></form></td></tr>");
            }
            b.Append("</tbody></table><p><strong>Tổng tiền: "+E(Money(total))+"</strong></p><p class='hint'>Giá được lấy lại khi tạo hóa đơn. Bản này ghi nhận thanh toán, nhưng chưa kiểm tra hay trừ tồn kho.</p><form method='post' action='"+E(Url("banhang",null,null))+"'>"+Hidden("checkout")+"<div class='formgrid'><div class='field'><label>Khách hàng *</label><select name='customer' required><option value=''>-- Chọn khách hàng --</option>");
            foreach(DataRow r in Q("SELECT MaKH,HoTen FROM dbo.KHACHHANG ORDER BY HoTen").Rows)b.Append("<option value='"+E(r[0])+"'>"+E(r[1])+" ("+E(r[0])+")</option>");
            b.Append("</select></div><div class='field'><label>Nhân viên *</label><select name='employee' required><option value=''>-- Chọn nhân viên --</option>");
            foreach(DataRow r in Q("SELECT MaNV,HoTen FROM dbo.NHANVIEN ORDER BY HoTen").Rows)b.Append("<option value='"+E(r[0])+"'>"+E(r[1])+" ("+E(r[0])+")</option>");
            b.Append("</select></div><div class='field'><label>Phương thức thanh toán *</label><select name='payment'><option>Tiền mặt</option><option>Chuyển khoản</option></select></div><div class='field'><label>Tiền khách đưa (tiền mặt) *</label><input name='tender' type='number' step='0.01' min='0' value='0' required></div></div><div class='toolbar'><button class='btn' "+(Cart.Count==0?"disabled":"")+">Thanh toán và lập hóa đơn</button></div></form></section>");
        }

        protected override void Checkout()
        {
            if(Cart.Count==0)throw new Exception("Giỏ hàng đang trống.");
            string kh=(Request.Form["customer"]??"").Trim(),nv=(Request.Form["employee"]??"").Trim();
            if(kh.Length==0||kh.Length>10||nv.Length==0||nv.Length>10)throw new Exception("Vui lòng chọn khách hàng và nhân viên.");
            string pay=Request.Form["payment"]??"";
            decimal tender;
            if(!new[]
            {
                "Tiền mặt","Chuyển khoản"
            }
            .Contains(pay)||!Decimal.TryParse(Request.Form["tender"],NumberStyles.AllowDecimalPoint,CultureInfo.InvariantCulture,out tender)||tender<0)throw new Exception("Phương thức hoặc số tiền không hợp lệ.");
            string invoice="HD"+Guid.NewGuid().ToString("N").Substring(0,8).ToUpperInvariant();
            using(var c=KetNoi.CreateConnection())
            {
                c.Open();
                using(var tx=c.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        decimal total=0;
                        var prices=new Dictionary<string,decimal>();
                        foreach(var i in Cart)
                        {
                            using(var cmd=new SqlCommand("SELECT GiaBan FROM dbo.LINHKIEN WITH (UPDLOCK,HOLDLOCK) WHERE MaLK=@id",c,tx))
                            {
                                cmd.Parameters.Add(P("@id",i.Key));
                                object result=cmd.ExecuteScalar();
                                if(result==null)throw new Exception("Một sản phẩm trong giỏ không còn tồn tại.");
                                decimal price=Convert.ToDecimal(result);
                                prices[i.Key]=price;
                                total+=price*i.Value;
                            }
                        }
                        if(pay=="Tiền mặt"&&tender<total)throw new Exception("Tiền khách đưa chưa đủ để thanh toán.");
                        decimal given=pay=="Tiền mặt"?tender:total;
                        decimal change=pay=="Tiền mặt"?given-total:0;
                        using(var cmd=new SqlCommand("INSERT INTO dbo.HOADON(MaHD,MaKH,MaNV,NgayLap,TongTien,GiamGia,ThanhTien,TrangThai,TienKhachDua,TienThoi) VALUES(@id,@kh,@nv,GETDATE(),@total,0,@total,N'Đã thanh toán',@given,@change)",c,tx))
                        {
                            cmd.Parameters.Add(P("@id",invoice));
                            cmd.Parameters.Add(P("@kh",kh));
                            cmd.Parameters.Add(P("@nv",nv));
                            cmd.Parameters.Add(P("@total",total));
                            cmd.Parameters.Add(P("@given",given));
                            cmd.Parameters.Add(P("@change",change));
                            cmd.ExecuteNonQuery();
                        }
                        using(var cmd=new SqlCommand("INSERT dbo.THANHTOAN(MaTT,MaHD,NgayThanhToan,SoTien,PhuongThuc,TrangThai) VALUES(@tt,@hd,GETDATE(),@sum,@method,N'Thành công')",c,tx))
                        {
                            cmd.Parameters.Add(P("@tt","TT"+Guid.NewGuid().ToString("N").Substring(0,8).ToUpperInvariant()));
                            cmd.Parameters.Add(P("@hd",invoice));
                            cmd.Parameters.Add(P("@sum",total));
                            cmd.Parameters.Add(P("@method",pay));
                            cmd.ExecuteNonQuery();
                        }
                        foreach(var i in Cart)
                        {
                            using(var cmd=new SqlCommand("INSERT INTO dbo.CT_HOADON(MaHD,MaLK,SoLuong,DonGiaBan,ThanhTien,ChietKhau) VALUES(@id,@lk,@qty,@price,@sum,0)",c,tx))
                            {
                                cmd.Parameters.Add(P("@id",invoice));
                                cmd.Parameters.Add(P("@lk",i.Key));
                                cmd.Parameters.Add(P("@qty",i.Value));
                                cmd.Parameters.Add(P("@price",prices[i.Key]));
                                cmd.Parameters.Add(P("@sum",prices[i.Key]*i.Value));
                                cmd.ExecuteNonQuery();
                            }
                        }
                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
            Cart.Clear();
            Response.Redirect(Url("hoadon","detail",invoice),false);
            Context.ApplicationInstance.CompleteRequest();
            mode="hoadon";
            action="detail";
            id=invoice;
            msg="Đã tạo hóa đơn thành công.";
        }
    }
}

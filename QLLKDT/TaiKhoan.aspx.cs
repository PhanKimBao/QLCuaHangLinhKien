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
    // Xử lý của form TaiKhoan.
    public partial class TaiKhoan : QuanLy
    {
        protected override void Accounts()
        {
            DataTable t=Q("SELECT t.MaTK,t.TenDangNhap,t.MaNV,n.HoTen,t.PhanQuyen,t.TrangThai FROM dbo.TAIKHOAN t JOIN dbo.NHANVIEN n ON n.MaNV=t.MaNV ORDER BY t.MaTK");
            b.Append("<h1>Quản lý tài khoản</h1><section class='contentbox'><p class='hint'>Để trống mật khẩu khi sửa nếu muốn giữ mật khẩu cũ. Mật khẩu mới tối thiểu 8 ký tự.</p><form method='post'>"+Hidden("account")+"<div class='formgrid'><div class='field'><label>Mã tài khoản *</label><input name='code' required maxlength='10'></div><div class='field'><label>Tên đăng nhập *</label><input name='username' maxlength='50' required></div><div class='field'><label>Nhân viên *</label><select name='employee' required><option value=''>-- Chọn --</option>");
            foreach(DataRow r in Q("SELECT MaNV,HoTen FROM dbo.NHANVIEN ORDER BY HoTen").Rows)b.Append("<option value='"+E(r[0])+"'>"+E(r[1])+" ("+E(r[0])+")</option>");
            b.Append("</select></div><div class='field'><label>Mật khẩu</label><input name='password' type='password' maxlength='100' autocomplete='new-password'></div><div class='field'><label>Nhóm quyền *</label><select name='role'><option>Quản trị</option><option>Bán hàng</option><option>Kho</option></select></div><div class='field'><label>Trạng thái *</label><select name='status'><option>Hoạt động</option><option>Đã khóa</option></select></div></div><div class='toolbar'><button class='btn'>Thêm / cập nhật theo mã</button></div></form><h2>Danh sách tài khoản</h2><table class='datagrid'><tr><th>Mã</th><th>Tên đăng nhập</th><th>Nhân viên</th><th>Nhóm</th><th>Trạng thái</th></tr>");
            foreach(DataRow r in t.Rows)b.Append("<tr><td>"+E(r[0])+"</td><td>"+E(r[1])+"</td><td>"+E(r[3])+" ("+E(r[2])+")</td><td>"+E(r[4])+"</td><td>"+E(r[5])+"</td></tr>");
            b.Append("</table></section>");
        }

        protected override void SaveAccount()
        {
            string code=(Request.Form["code"]??"").Trim(),user=(Request.Form["username"]??"").Trim(),employee=(Request.Form["employee"]??"").Trim(),pass=Request.Form["password"]??"",role=Request.Form["role"]??"",status=Request.Form["status"]??"";
            if(code.Length==0||code.Length>10||user.Length==0||user.Length>50||employee.Length==0||employee.Length>10||!new[]
            {
                "Quản trị","Bán hàng","Kho"
            }
            .Contains(role)||!new[]
            {
                "Hoạt động","Đã khóa"
            }
            .Contains(status)||(pass.Length>0&&(pass.Length<8||pass.Length>100)))throw new Exception("Vui lòng kiểm tra thông tin và mật khẩu tối thiểu 8 ký tự.");
            if(code==Convert.ToString(Session["account"])&&(status!="Hoạt động"||role!="Quản trị"))throw new Exception("Không thể khóa hoặc tự hạ quyền tài khoản quản trị đang đăng nhập.");
            using(var c=KetNoi.CreateConnection())
            {
                c.Open();
                using(var tx=c.BeginTransaction(IsolationLevel.Serializable))
                {
                    using(var check=new SqlCommand("SELECT COUNT(*) FROM dbo.TAIKHOAN WITH (UPDLOCK,HOLDLOCK) WHERE TenDangNhap=@name AND MaTK<>@id",c,tx))
                    {
                        check.Parameters.Add(P("@name",user));
                        check.Parameters.Add(P("@id",code));
                        if(Convert.ToInt32(check.ExecuteScalar())>0)throw new Exception("Tên đăng nhập đã tồn tại.");
                    }
                    bool exists;
                    using(var check=new SqlCommand("SELECT COUNT(*) FROM dbo.TAIKHOAN WITH (UPDLOCK,HOLDLOCK) WHERE MaTK=@id",c,tx))
                    {
                        check.Parameters.Add(P("@id",code));
                        exists=Convert.ToInt32(check.ExecuteScalar())>0;
                    }
                    if(!exists&&pass.Length==0)throw new Exception("Cần mật khẩu cho tài khoản mới.");
                    string sql=exists?"UPDATE dbo.TAIKHOAN SET TenDangNhap=@name,MaNV=@nv,PhanQuyen=@role,TrangThai=@status"+(pass.Length>0?",MatKhau=@pw":"")+" WHERE MaTK=@id":"INSERT dbo.TAIKHOAN(MaTK,TenDangNhap,MaNV,PhanQuyen,TrangThai,MatKhau) VALUES(@id,@name,@nv,@role,@status,@pw)";
                    using(var cmd=new SqlCommand(sql,c,tx))
                    {
                        cmd.Parameters.Add(P("@id",code));
                        cmd.Parameters.Add(P("@name",user));
                        cmd.Parameters.Add(P("@nv",employee));
                        cmd.Parameters.Add(P("@role",role));
                        cmd.Parameters.Add(P("@status",status));
                        if(pass.Length>0)cmd.Parameters.Add(P("@pw",Access.Hash(pass)));
                        cmd.ExecuteNonQuery();
                    }
                    tx.Commit();
                }
            }
            msg="Đã lưu tài khoản.";
        }
    }
}

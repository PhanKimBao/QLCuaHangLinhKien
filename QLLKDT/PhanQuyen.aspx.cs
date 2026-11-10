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
    // Xử lý của form PhanQuyen.
    public partial class PhanQuyen : QuanLy
    {
        bool PermissionTableExists()
        {
            DataTable t=Q("SELECT CASE WHEN OBJECT_ID(N'dbo.PHANQUYEN_CHUCNANG', N'U') IS NULL THEN 0 ELSE 1 END");
            return Convert.ToInt32(t.Rows[0][0])==1;
        }

        protected override void InitPermissions()
        {
            using(var c=KetNoi.CreateConnection())
            {
                c.Open();
                using(var tx=c.BeginTransaction(IsolationLevel.Serializable))
                {
                    using(var cmd=new SqlCommand(@"IF OBJECT_ID(N'dbo.PHANQUYEN_CHUCNANG',N'U') IS NULL BEGIN
        CREATE TABLE dbo.PHANQUYEN_CHUCNANG(PhanQuyen nvarchar(50) NOT NULL,ChucNang varchar(30) NOT NULL,CONSTRAINT PK_PHANQUYEN_CHUCNANG PRIMARY KEY(PhanQuyen,ChucNang));
        INSERT dbo.PHANQUYEN_CHUCNANG(PhanQuyen,ChucNang) VALUES
        (N'Bán hàng','tongquan'),(N'Bán hàng','sanpham'),(N'Bán hàng','banhang'),(N'Bán hàng','khachhang'),(N'Bán hàng','hoadon'),(N'Bán hàng','baohanh'),
        (N'Kho','tongquan'),(N'Kho','sanpham'),(N'Kho','danhmuc'),(N'Kho','nhacungcap'),(N'Kho','phieunhap'),(N'Kho','baohanh'); END",c,tx))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    tx.Commit();
                }
            }
            msg="Đã khởi tạo bảng phân quyền. Ông có thể chọn quyền cho từng nhóm.";
        }

        protected override void Permissions()
        {
            if(!PermissionTableExists())
            {
                b.Append("<h1>Phân quyền chức năng</h1><section class='contentbox'><div class='alert bad'>Database chưa có bảng phân quyền PHANQUYEN_CHUCNANG. Tạo bảng một lần để dùng chức năng này.</div><p class='hint'>Chức năng chỉ tạo bảng mới khi chưa có; dữ liệu các bảng khác không bị sửa. Nếu tài khoản SQL không có quyền tạo bảng, chạy file Database/Bo_sung_phan_quyen.sql trong SSMS.</p><form method='post'>"+Hidden("initpermissions")+"<button class='btn'>Khởi tạo phân quyền</button></form></section>");
                return;
            }
            string chosen=Request.HttpMethod=="POST" && Request.Form["operation"]=="permissions" ? Request.Form["role"] : Request.QueryString["role"];
            string selected=chosen=="Kho"?"Kho":"Bán hàng";
            b.Append("<h1>Phân quyền chức năng</h1><section class='contentbox'><p class='hint'>Quản trị luôn có đầy đủ quyền. Chọn quyền cho từng nhóm nhân viên; thay đổi có hiệu lực ở lần truy cập tiếp theo.</p><div class='toolbar'><a class='btnlink secondary' href='"+E(Url("phanquyen",null,null))+"&amp;role="+U("Bán hàng")+"'>Bán hàng</a><a class='btnlink secondary' href='"+E(Url("phanquyen",null,null))+"&amp;role=Kho'>Kho</a></div><h2>Nhóm: "+E(selected)+"</h2><form method='post' action='"+E(Url("phanquyen",null,null)+"&role="+U(selected))+"'>"+Hidden("permissions")+"<input type='hidden' name='role' value='"+E(selected)+"'><div class='formgrid'>");
            var granted=new HashSet<string>();
            foreach(DataRow r in Q("SELECT ChucNang FROM dbo.PHANQUYEN_CHUCNANG WHERE PhanQuyen=@r",P("@r",selected)).Rows)granted.Add(Convert.ToString(r[0]));
            for(int i=0;i<Features.Length;i++)b.Append("<label><input type='checkbox' name='permission' value='"+E(Features[i])+"' "+(granted.Contains(Features[i])?"checked":"")+"> "+E(FeatureLabels[i])+"</label>");
            b.Append("</div><div class='toolbar'><button class='btn'>Lưu phân quyền</button></div></form></section>");
        }

        protected override void SavePermissions()
        {
            string role=Request.Form["role"]??"";
            if(role!="Bán hàng"&&role!="Kho")throw new Exception("Nhóm quyền không hợp lệ.");
            string[] requested=Request.Form.GetValues("permission")??new string[0];
            if(requested.Any(x=>!Features.Contains(x)))throw new Exception("Quyền không hợp lệ.");
            using(var c=KetNoi.CreateConnection())
            {
                c.Open();
                using(var tx=c.BeginTransaction())
                {
                    using(var cmd=new SqlCommand("DELETE dbo.PHANQUYEN_CHUCNANG WHERE PhanQuyen=@r",c,tx))
                    {
                        cmd.Parameters.Add(P("@r",role));
                        cmd.ExecuteNonQuery();
                    }
                    foreach(string feature in requested.Distinct())
                    {
                        using(var cmd=new SqlCommand("INSERT dbo.PHANQUYEN_CHUCNANG(PhanQuyen,ChucNang) VALUES(@r,@f)",c,tx))
                        {
                            cmd.Parameters.Add(P("@r",role));
                            cmd.Parameters.Add(P("@f",feature));
                            cmd.ExecuteNonQuery();
                        }
                    }
                    tx.Commit();
                }
            }
            msg="Đã lưu phân quyền.";
        }
    }
}

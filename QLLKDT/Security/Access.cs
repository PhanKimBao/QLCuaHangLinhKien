using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.UI;

namespace QLLKDT
{
    public static class Access
    {
        public static string Hash(string password)
        {
            byte[] salt=new byte[16];
            using(var rng=RandomNumberGenerator.Create())rng.GetBytes(salt);
            using(var pb=new Rfc2898DeriveBytes(password,salt,100000))return "pbkdf2$100000$"+Convert.ToBase64String(salt)+"$"+Convert.ToBase64String(pb.GetBytes(32));
        }
        public static bool Verify(string password,string stored)
        {
            try
            {
                if(stored.StartsWith("pbkdf2$",StringComparison.Ordinal))
                {
                    string[] p=stored.Split('$');
                    int count=int.Parse(p[1]);
                    if(count<10000||count>500000)return false;
                    byte[] salt=Convert.FromBase64String(p[2]), expected=Convert.FromBase64String(p[3]);
                    if(expected.Length!=32)return false;
                    byte[] actual;
                    using(var pb=new Rfc2898DeriveBytes(password,salt,count))actual=pb.GetBytes(32);
                    int diff=0;
                    for(int i=0;i<32;i++)diff|=expected[i]^actual[i];
                    return diff==0;
                }
                using(var md=MD5.Create())
                {
                    byte[] hash=md.ComputeHash(Encoding.UTF8.GetBytes(password));
                    string value=BitConverter.ToString(hash).Replace("-","").ToLowerInvariant();
                    return value.Equals(stored,StringComparison.OrdinalIgnoreCase);
                }
            }
            catch
            {
                return false;
            }
        }
        public static bool Authenticate(string user,string password)
        {
            using(var c=KetNoi.CreateConnection())using(var cmd=new SqlCommand("SELECT MaTK,MatKhau,TrangThai FROM dbo.TAIKHOAN WHERE TenDangNhap=@u",c))
            {
                cmd.Parameters.AddWithValue("@u",user);
                c.Open();
                string id=null,stored=null,status=null;
                using(var r=cmd.ExecuteReader())
                {
                    if(r.Read())
                    {
                        id=r.GetString(0);
                        stored=r.GetString(1);
                        status=r.GetString(2);
                        if(r.Read())return false;
                    }
                }
                if(id==null||!status.Equals("Hoạt động",StringComparison.OrdinalIgnoreCase)||!Verify(password,stored))return false;
                if(!stored.StartsWith("pbkdf2$",StringComparison.Ordinal))
                {
                    using(var update=new SqlCommand("UPDATE dbo.TAIKHOAN SET MatKhau=@new WHERE MaTK=@id AND MatKhau=@old",c))
                    {
                        update.Parameters.AddWithValue("@new",Hash(password));
                        update.Parameters.AddWithValue("@id",id);
                        update.Parameters.AddWithValue("@old",stored);
                        update.ExecuteNonQuery();
                    }
                }
                HttpContext.Current.Session.Clear();
                HttpContext.Current.Session["account"]=id;
                HttpContext.Current.Session["csrf"]=Guid.NewGuid().ToString("N");
                return true;
            }
        }
        public static string UserRole()
        {
            string id=Convert.ToString(HttpContext.Current.Session["account"]);
            if(String.IsNullOrEmpty(id))return null;
            using(var c=KetNoi.CreateConnection())using(var cmd=new SqlCommand("SELECT PhanQuyen FROM dbo.TAIKHOAN WHERE MaTK=@id AND TrangThai=N'Hoạt động'",c))
            {
                cmd.Parameters.AddWithValue("@id",id);
                c.Open();
                return Convert.ToString(cmd.ExecuteScalar());
            }
        }
        public static bool Allowed(string feature)
        {
            string role=UserRole();
            if(String.IsNullOrEmpty(role))return false;
            if(role=="Quản trị")return true;
            if(feature=="phanquyen"||feature=="taikhoan"||feature=="backup")return false;
            using(var c=KetNoi.CreateConnection())using(var cmd=new SqlCommand("SELECT COUNT(*) FROM dbo.PHANQUYEN_CHUCNANG WHERE PhanQuyen=@role AND ChucNang=@feature",c))
            {
                cmd.Parameters.AddWithValue("@role",role);
                cmd.Parameters.AddWithValue("@feature",feature);
                c.Open();
                return Convert.ToInt32(cmd.ExecuteScalar())>0;
            }
        }
        public static string Token
        {
            get
            {
                var s=HttpContext.Current.Session;
                if(s["csrf"]==null)s["csrf"]=Guid.NewGuid().ToString("N");
                return (string)s["csrf"];
            }
        }
        public static void CheckPost()
        {
            if(HttpContext.Current.Request.HttpMethod!="POST" || !String.Equals(HttpContext.Current.Request.Form["csrf"],Token,StringComparison.Ordinal))throw new InvalidOperationException("Phiên đã hết hạn, vui lòng tải lại trang.");
        }
    }
    public class SecurePage : Page
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            string role;
            try
            {
                role=Access.UserRole();
            }
            catch
            {
                Response.Redirect("Login.aspx?db=1",true);
                return;
            }
            if(String.IsNullOrEmpty(role))
            {
                Response.Redirect("Login.aspx",true);
                return;
            }
            string feature = DuongDan.ChucNang(Request.Path, Request.QueryString["m"]);
            bool allowed;
            try
            {
                allowed=Access.Allowed(feature);
            }
            catch(SqlException ex)
            {
                if(ex.Number==208||ex.Number==229)
                {
                    Response.StatusCode=503;
                    Response.TrySkipIisCustomErrors=true;
                    Response.Write("Chưa thể kiểm tra phân quyền. Nhờ quản trị viên đăng nhập để khởi tạo bảng phân quyền hoặc chạy script Database/Bo_sung_phan_quyen.sql, sau đó kiểm tra quyền SQL.");
                    Response.End();
                    return;
                }
                throw;
            }
            if(!allowed)
            {
                Response.StatusCode=403;
                Response.TrySkipIisCustomErrors=true;
                Response.Write("Bạn không có quyền sử dụng chức năng này.");
                Response.End();
            }
        }
    }
}

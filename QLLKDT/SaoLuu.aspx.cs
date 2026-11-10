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
    // Xử lý của form SaoLuu.
    public partial class SaoLuu : QuanLy
    {
        protected override void BackupPage()
        {
            b.Append("<h1>Sao lưu dữ liệu</h1><section class='contentbox'><p class='hint'>SQL Server sẽ ghi file .bak trên máy chạy SQL Server. Tạo thư mục trước và cấp quyền ghi cho tài khoản dịch vụ SQL Server. Bản sao lưu chứa toàn bộ dữ liệu, cần bảo quản an toàn.</p><form method='post'>"+Hidden("backup")+"<div class='field'><label>Thư mục trên máy SQL Server</label><input name='directory' value='C:\\QLCHLK_Backup' required maxlength='180'></div><div class='toolbar'><button class='btn' onclick=\"return confirm('Tạo bản sao lưu database?')\">Tạo bản sao lưu</button></div></form></section>");
        }

        protected override void MakeBackup()
        {
            string dir=(Request.Form["directory"]??"").Trim().TrimEnd('\\');
            if(dir.Length>180||!System.Text.RegularExpressions.Regex.IsMatch(dir,@"^[A-Za-z]:\\[^:*?\""<>|]+$")||dir.Contains(".."))throw new Exception("Nhập đường dẫn thư mục tuyệt đối trên máy SQL Server, ví dụ C:\\QLCHLK_Backup.");
            string path=dir+"\\QLCHLK_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+"_"+Guid.NewGuid().ToString("N").Substring(0,6)+".bak";
            using(var c=KetNoi.CreateConnection())
            {
                c.Open();
                string db=c.Database;
                if(String.IsNullOrWhiteSpace(db))throw new Exception("Chưa chọn database trong Web.config.");
                using(var cmd=new SqlCommand("BACKUP DATABASE ["+db.Replace("]","]]")+"] TO DISK=@path WITH COPY_ONLY, CHECKSUM",c))
                {
                    cmd.CommandTimeout=300;
                    cmd.Parameters.Add(P("@path",path));
                    cmd.ExecuteNonQuery();
                }
            }
            msg="Đã sao lưu trên máy SQL Server: "+path;
        }
    }
}

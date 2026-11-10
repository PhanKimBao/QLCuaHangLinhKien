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
    // Xử lý của form CaiDat.
    public partial class CaiDat : QuanLy
    {
        protected override void SaveEntity(Entity en)
        {
            bool edit=action=="edit";
            var fields=en.Fields;
            var values=new Dictionary<string,object>();
            string uploaded=en.Table=="LINHKIEN"?UploadImage():null;
            foreach(Field f in fields)
            {
                string v=(Request.Form[f.Name]??"").Trim();
                if(f.Name=="HinhAnh" && uploaded!=null)v=uploaded;
                if(edit&&f.Name==en.Key)v=id;
                if(f.Required&&v.Length==0)throw new Exception("Vui lòng nhập "+f.Label.ToLower()+".");
                if(f.Max>0&&v.Length>f.Max)throw new Exception(f.Label+" tối đa "+f.Max+" ký tự.");
                if(v.Length==0)
                {
                    values[f.Name]=DBNull.Value;
                    continue;
                }
                decimal number;
                int integer;
                DateTime date;
                if(f.Type=="number"||f.Type=="percent")
                {
                    if(!Decimal.TryParse(v,NumberStyles.AllowDecimalPoint,CultureInfo.InvariantCulture,out number)||number<0||(f.Type=="percent"&&number>100))throw new Exception(f.Label+" không hợp lệ.");
                    values[f.Name]=number;
                }
                else if(f.Type=="int")
                {
                    if(!Int32.TryParse(v,out integer)||integer<0)throw new Exception(f.Label+" phải là số nguyên không âm.");
                    values[f.Name]=integer;
                }
                else if(f.Type=="date")
                {
                    if(!DateTime.TryParseExact(v,"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out date))throw new Exception(f.Label+" không hợp lệ.");
                    values[f.Name]=date;
                }
                else values[f.Name]=v;
            }
            using(var c=KetNoi.CreateConnection())using(var cmd=c.CreateCommand())
            {
                if(edit)
                {
                    cmd.CommandText="UPDATE [dbo].["+en.Table+"] SET "+String.Join(",",fields.Where(x=>x.Name!=en.Key).Select(x=>"["+x.Name+"]=@"+x.Name))+" WHERE ["+en.Key+"]=@"+en.Key;
                }
                else
                {
                    cmd.CommandText="INSERT INTO [dbo].["+en.Table+"] ("+String.Join(",",fields.Select(x=>"["+x.Name+"]"))+") VALUES ("+String.Join(",",fields.Select(x=>"@"+x.Name))+")";
                }
                foreach(Field f in fields)cmd.Parameters.Add(P("@"+f.Name,values[f.Name]));
                c.Open();
                if(cmd.ExecuteNonQuery()!=1)throw new Exception("Không tìm thấy bản ghi cần sửa.");
            }
            msg="Đã lưu dữ liệu thành công.";
            action="";
        }

        protected override void DeleteEntity(Entity en)
        {
            string key=Request.Form["key"]??"";
            if(key.Length==0||key.Length>10)throw new Exception("Mã không hợp lệ.");
            using(var c=KetNoi.CreateConnection())using(var cmd=new SqlCommand("DELETE FROM [dbo].["+en.Table+"] WHERE ["+en.Key+"]=@key",c))
            {
                cmd.Parameters.Add(P("@key",key));
                c.Open();
                if(cmd.ExecuteNonQuery()!=1)throw new Exception("Không tìm thấy bản ghi.");
            }
            msg="Đã xóa dữ liệu.";
        }

        protected override void EntityPage(Entity en)
        {
            if(mode=="caidat"&&Access.Allowed("backup"))b.Append("<div class='toolbar'><a class='btnlink secondary' href='QuanLy.aspx?m=backup'>Sao lưu database</a></div>");
            if(action=="add"||action=="edit")
            {
                EntityForm(en);
                return;
            }
            DataTable t=Q("SELECT * FROM [dbo].["+en.Table+"] ORDER BY ["+en.Key+"]");
            b.Append("<div class='title'><div><h1>"+E(en.Title)+"</h1><p>Danh sách dữ liệu cửa hàng</p></div><a class='btnlink' href='"+E(Url(mode,"add",null))+"'>＋ Thêm mới</a></div><section class='contentbox'><div class='toolbar'><input id='quick' placeholder='Tìm nhanh trong danh sách...' oninput='filterRows()'></div><table class='datagrid' id='records'><thead><tr>");
            foreach(Field f in en.Fields.Take(mode=="sanpham"?7:6))b.Append("<th>"+E(f.Label)+"</th>");
            b.Append("<th>Thao tác</th></tr></thead><tbody>");
            foreach(DataRow r in t.Rows)
            {
                b.Append("<tr>");
                foreach(Field f in en.Fields.Take(mode=="sanpham"?7:6))
                {
                    object v=r[f.Name];
                    b.Append("<td>"+(v==DBNull.Value?"—":f.Type=="date"?E(Convert.ToDateTime(v).ToString("dd/MM/yyyy")):f.Type=="number"?E(Money(v)):E(v))+"</td>");
                }
                string key=Convert.ToString(r[en.Key]);
                b.Append("<td class='actions'><a href='"+E(Url(mode,"edit",key))+"'>Sửa</a> &nbsp; <form class='inline' method='post' action='"+E(Url(mode,null,null))+"' onsubmit=\"return confirm('Xóa bản ghi này?')\">"+Hidden("delete")+"<input type='hidden' name='key' value='"+E(key)+"'><button>Xóa</button></form></td></tr>");
            }
            b.Append("</tbody></table><p class='hint'>Tổng: "+t.Rows.Count+" bản ghi. Dữ liệu đang được sử dụng sẽ không thể xóa.</p></section><script>function filterRows(){var q=document.getElementById('quick').value.toLowerCase();document.querySelectorAll('#records tbody tr').forEach(function(r){r.style.display=r.textContent.toLowerCase().indexOf(q)>=0?'':'none'})}</script>");
        }

        void EntityForm(Entity en)
        {
            DataRow row=null;
            if(action=="edit")
            {
                DataTable t=Q("SELECT * FROM [dbo].["+en.Table+"] WHERE ["+en.Key+"]=@id",P("@id",id));
                if(t.Rows.Count!=1)throw new Exception("Không tìm thấy bản ghi.");
                row=t.Rows[0];
            }
            b.Append("<div class='title'><div><h1>"+(row==null?"Thêm ":"Sửa ")+E(en.Title.ToLower())+"</h1><p>Các trường có dấu * là bắt buộc</p></div></div><section class='contentbox'><form method='post' enctype='multipart/form-data' action='"+E(Url(mode,action,id))+"'>"+Hidden("save")+"<div class='formgrid'>");
            foreach(Field f in en.Fields)
            {
                object obj=row==null?null:row[f.Name];
                string value=obj==null||obj==DBNull.Value?"":f.Type=="date"?Convert.ToDateTime(obj).ToString("yyyy-MM-dd"):Convert.ToString(obj,CultureInfo.InvariantCulture);
                if(row==null&&f.Name=="TrangThai")value=mode=="sanpham"?"Đang kinh doanh":mode=="nhanvien"?"Đang làm việc":"";
                b.Append("<div class='field'><label>"+E(f.Label)+(f.Required?" *":"")+"</label>");
                if(f.Type=="category")
                {
                    b.Append("<select name='"+f.Name+"' required><option value=''>-- Chọn danh mục --</option>");
                    foreach(DataRow cat in Q("SELECT MaLoai,TenLoai FROM dbo.LOAILINHKIEN ORDER BY TenLoai").Rows)b.Append("<option value='"+E(cat[0])+"' "+(value==Convert.ToString(cat[0])?"selected":"")+">"+E(cat[1])+"</option>");
                    b.Append("</select>");
                }
                else b.Append("<input name='"+E(f.Name)+"' type='"+(f.Type=="date"?"date":f.Type=="number"||f.Type=="int"||f.Type=="percent"?"number":"text")+"' "+(f.Type=="number"||f.Type=="percent"?"step='0.01' min='0' ":f.Type=="int"?"step='1' min='0' ":"")+(f.Max>0?"maxlength='"+f.Max+"' ":"")+(f.Required?"required ":"")+(row!=null&&f.Name==en.Key?"readonly ":"")+"value='"+E(value)+"'>");
                if(f.Name=="HinhAnh")b.Append("<label>Chọn ảnh JPG hoặc PNG (tối đa 3 MB)</label><input type='file' name='productImage' accept='image/png,image/jpeg'>");
                b.Append("</div>");
            }
            b.Append("</div><div class='toolbar'><button class='btn'>Lưu thông tin</button><a class='btnlink secondary' href='"+E(mode=="sanpham"?"Default.aspx":Url(mode,null,null))+"'>Quay lại</a></div></form></section>");
        }

        string UploadImage()
        {
            var file=Request.Files["productImage"];
            if(file==null||file.ContentLength==0)return null;
            if(file.ContentLength>3*1024*1024)throw new Exception("Ảnh tối đa 3 MB.");
            string ext=System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
            if(ext!=".jpg"&&ext!=".jpeg"&&ext!=".png")throw new Exception("Chỉ chấp nhận ảnh JPG hoặc PNG.");
            string folder=Server.MapPath("~/Images/Uploads");
            System.IO.Directory.CreateDirectory(folder);
            try
            {
                using(var img=System.Drawing.Image.FromStream(file.InputStream,true,true))
                {
                    if(img.Width>6000||img.Height>6000||(long)img.Width*img.Height>20000000)throw new Exception("Kích thước ảnh quá lớn.");
                    bool png=img.RawFormat.Guid==System.Drawing.Imaging.ImageFormat.Png.Guid;
                    bool jpg=img.RawFormat.Guid==System.Drawing.Imaging.ImageFormat.Jpeg.Guid;
                    if(!png&&!jpg)throw new Exception("Dữ liệu phải là ảnh JPG hoặc PNG.");
                    string name=Guid.NewGuid().ToString("N")+(png?".png":".jpg");
                    using(var clean=new System.Drawing.Bitmap(img))
                    {
                        clean.Save(System.IO.Path.Combine(folder,name),png?System.Drawing.Imaging.ImageFormat.Png:System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                    return "Images/Uploads/"+name;
                }
            }
            catch(OutOfMemoryException)
            {
                throw new Exception("Tệp ảnh không hợp lệ.");
            }
            catch(ArgumentException)
            {
                throw new Exception("Tệp ảnh không hợp lệ.");
            }
        }
    }
}

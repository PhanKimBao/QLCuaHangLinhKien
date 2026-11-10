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
    public partial class QuanLy : SecurePage
    {
        protected string Body = "";
        protected string mode, action, id;
        protected StringBuilder b;
        protected string msg = "";
        protected bool error, exported;
        protected override void Render(HtmlTextWriter writer)
        {
            if(!exported) base.Render(writer);
        }
        protected class Field
        {
            public string Name, Label, Type;
            public bool Required;
            public int Max;
            public Field(string n,string l,string t,int max,bool req)
            {
                Name=n;
                Label=l;
                Type=t;
                Max=max;
                Required=req;
            }
        }
        protected class Entity
        {
            public string Table, Key, Title;
            public Field[] Fields;
            public Entity(string t,string k,string title,params Field[] f)
            {
                Table=t;
                Key=k;
                Title=title;
                Fields=f;
            }
        }
        static Field F(string n,string l,int len,bool req)
        {
            return new Field(n,l,"text",len,req);
        }
        static Field N(string n,string l,bool req)
        {
            return new Field(n,l,"number",0,req);
        }
        static Field D(string n,string l)
        {
            return new Field(n,l,"date",0,false);
        }
        static readonly Dictionary<string,Entity> Entities = new Dictionary<string,Entity>
        {
            {
                "danhmuc",new Entity("LOAILINHKIEN","MaLoai","Danh mục linh kiện",F("MaLoai","Mã loại",10,true),F("TenLoai","Tên loại",50,true),F("MoTa","Mô tả",255,false))
            }
            ,
            {
                "khachhang",new Entity("KHACHHANG","MaKH","Khách hàng",F("MaKH","Mã khách hàng",10,true),F("HoTen","Họ tên",50,true),F("SDT","Số điện thoại",15,true),F("DiaChi","Địa chỉ",100,false),F("Email","Email",50,false))
            }
            ,
            {
                "nhacungcap",new Entity("NHACUNGCAP","MaNCC","Nhà cung cấp",F("MaNCC","Mã nhà cung cấp",10,true),F("TenNCC","Tên nhà cung cấp",100,true),F("SDT","Số điện thoại",15,true),F("DiaChi","Địa chỉ",100,false),F("Email","Email",50,false))
            }
            ,
            {
                "nhanvien",new Entity("NHANVIEN","MaNV","Nhân viên",F("MaNV","Mã nhân viên",10,true),F("HoTen","Họ tên",50,true),D("NgaySinh","Ngày sinh"),F("GioiTinh","Giới tính",10,false),F("SDT","Số điện thoại",15,true),F("DiaChi","Địa chỉ",100,false),F("Email","Email",50,false),F("ChucVu","Chức vụ",30,true),F("TrangThai","Trạng thái",20,true),F("SoCCCD","Số CCCD",20,false))
            }
            ,
            {
                "sanpham",new Entity("LINHKIEN","MaLK","Sản phẩm",F("MaLK","Mã linh kiện",10,true),new Field("MaLoai","Danh mục","category",10,true),F("TenLK","Tên linh kiện",100,true),F("ThuongHieu","Thương hiệu",50,false),F("ThongSoKT","Thông số kỹ thuật",255,false),F("DonViTinh","Đơn vị tính",20,true),N("GiaNhap","Giá nhập",true),N("GiaBan","Giá bán",true),new Field("ThoiGianBaoHanh","Bảo hành (tháng)","int",0,false),F("TrangThai","Trạng thái",20,true),F("XuatXu","Xuất xứ",50,false),F("TinhTrang","Tình trạng",50,false),F("HinhAnh","Đường dẫn ảnh trong website",255,false),new Field("ChietKhau","Chiết khấu (%)","percent",0,false))
            }
            ,
            {
                "baohanh",new Entity("BAOHANH","MaBH","Bảo hành",F("MaBH","Mã bảo hành",10,true),F("MaHD","Mã hóa đơn",10,true),F("MaLK","Mã linh kiện",10,true),F("MaNV","Mã nhân viên",10,true),new Field("NgayTiepNhan","Ngày tiếp nhận","date",0,true),new Field("NgayHenTra","Ngày hẹn trả","date",0,true),D("NgayHoanThanh","Ngày hoàn thành"),F("TinhTrang","Tình trạng",255,false),F("KetQua","Kết quả",255,false),F("TrangThai","Trạng thái",20,true))
            }
            ,
            {
                "caidat",new Entity("THONGTINCUAHANG","MaCH","Thông tin cửa hàng",F("MaCH","Mã cửa hàng",10,true),F("TenCH","Tên cửa hàng",50,true),F("DiaChi","Địa chỉ",100,true),F("SDT","Số điện thoại",15,true),F("Email","Email",50,false))
            }
        }
        ;
        protected string E(object x)
        {
            return HttpUtility.HtmlEncode(Convert.ToString(x,CultureInfo.InvariantCulture));
        }
        protected string U(string x)
        {
            return HttpUtility.UrlEncode(x ?? "");
        }
        protected string Url(string m,string a,string i)
        {
            return DuongDan.LayTrang(m)+"?m="+U(m)+(a==null?"":"&action="+U(a))+(i==null?"":"&id="+U(i));
        }
        protected string Money(object x)
        {
            return Convert.ToDecimal(x,CultureInfo.InvariantCulture).ToString("N0",CultureInfo.GetCultureInfo("vi-VN"))+" đ";
        }
        protected DataTable Q(string sql, params SqlParameter[] p)
        {
            return KetNoi.ExecuteTable(sql, p);
        }
        protected SqlParameter P(string name,object val)
        {
            return new SqlParameter(name,val ?? DBNull.Value);
        }
        protected string Token
        {
            get
            {
                if(Session["csrf"]==null)Session["csrf"]=Guid.NewGuid().ToString("N");
                return (string)Session["csrf"];
            }
        }
        protected string Hidden(string operation)
        {
            return "<input type='hidden' name='csrf' value='"+E(Token)+"'><input type='hidden' name='operation' value='"+E(operation)+"'>";
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            mode=DuongDan.ChucNang(Request.Path, Request.QueryString["m"]);
            if (Request.HttpMethod == "GET" && System.IO.Path.GetFileName(Request.Path).Equals("QuanLy.aspx", StringComparison.OrdinalIgnoreCase))
            {
                string trang = DuongDan.LayTrang(mode);
                if (!trang.Equals("QuanLy.aspx", StringComparison.OrdinalIgnoreCase))
                {
                    string query = Request.Url.Query;
                    Response.Redirect(trang + query, true);
                    return;
                }
            }

            action=Request.QueryString["action"]??"";
            id=Request.QueryString["id"]??"";
            if(!new[]
            {
                "tongquan","banhang","sanpham","danhmuc","khachhang","nhacungcap","hoadon","nhanvien","baocao","caidat","baohanh","phieunhap","taikhoan","phanquyen","backup"
            }
            .Contains(mode))mode="tongquan";
            try
            {
                if(Request.HttpMethod=="POST")
                {
                    if(!String.Equals(Request.Form["csrf"],Token,StringComparison.Ordinal))throw new Exception("Phiên làm việc đã thay đổi. Vui lòng tải lại trang.");
                    HandlePost();
                }
                if(mode=="baocao" && Request.QueryString["export"]=="csv")
                {
                    ExportReport();
                    return;
                }
                b=new StringBuilder();
                LayoutStart();
                if(Entities.ContainsKey(mode)) EntityPage(Entities[mode]);
                else if(mode=="tongquan") Dashboard();
                else if(mode=="banhang") Sales();
                else if(mode=="hoadon") Invoices();
                else if(mode=="baocao")
                {
                    Report();
                    AdvancedReport();
                }
                else if(mode=="phanquyen") Permissions();
                else if(mode=="taikhoan") Accounts();
                else if(mode=="backup") BackupPage();
                else if(mode=="phieunhap") ImportPage();
                LayoutEnd();
                Body=b.ToString();
            }
            catch(Exception ex)
            {
                if(b==null)
                {
                    b=new StringBuilder();
                    LayoutStart();
                }
                b.Append("<div class='alert bad'>"+E(SafeError(ex))+"</div><p><a href='"+E(Url(mode,null,null))+"'>Quay lại</a></p>");
                LayoutEnd();
                Body=b.ToString();
            }
        }
        string SafeError(Exception ex)
        {
            var sql=ex as SqlException;
            if(sql!=null)
            {
                if(sql.Number==208 && mode=="phanquyen")return "Chưa có bảng phân quyền. Vào Phân quyền và chọn Khởi tạo hoặc chạy file Database/Bo_sung_phan_quyen.sql trong SSMS.";
                if(sql.Number==207 && mode=="phanquyen")return "Bảng phân quyền không đúng cấu trúc. Kiểm tra PHANQUYEN_CHUCNANG trong database.";
                if(sql.Number==229||sql.Number==262)return "Tài khoản kết nối SQL Server thiếu quyền. Hãy cấp quyền phù hợp hoặc nhờ quản trị viên chạy script trong SSMS.";
                if(sql.Number==2627||sql.Number==2601)return "Mã đã tồn tại, vui lòng nhập mã khác.";
                if(sql.Number==547)return "Dữ liệu đang được sử dụng hoặc không thỏa điều kiện của database.";
                if(sql.Number==8152||sql.Number==2628)return "Có nội dung quá dài so với cấu trúc database.";
                return "Không thể xử lý dữ liệu SQL Server. Kiểm tra kết nối và các thông tin nhập.";
            }
            return ex.Message;
        }
        void LayoutStart()
        {
            if(msg!="")b.Append("<div class='alert "+(error?"bad":"")+"'>"+E(msg)+"</div>");
        }
        void LayoutEnd()
        {
        }
        void HandlePost()
        {
            string op=Request.Form["operation"]??"";
            if(op=="save"&&Entities.ContainsKey(mode))
            {
                SaveEntity(Entities[mode]);
                return;
            }
            if(op=="delete"&&Entities.ContainsKey(mode))
            {
                DeleteEntity(Entities[mode]);
                return;
            }
            if(mode=="phieunhap"&&op=="addimport")
            {
                AddImport();
                return;
            }
            if(mode=="phieunhap"&&op=="removeimport")
            {
                RemoveImport();
                return;
            }
            if(mode=="phieunhap"&&op=="saveimport")
            {
                SaveImport();
                return;
            }
            if(mode=="phanquyen"&&op=="initpermissions")
            {
                InitPermissions();
                return;
            }
            if(mode=="phanquyen"&&op=="permissions")
            {
                SavePermissions();
                return;
            }
            if(mode=="taikhoan"&&op=="account")
            {
                SaveAccount();
                return;
            }
            if(mode=="backup"&&op=="backup")
            {
                MakeBackup();
                return;
            }
            if(mode=="banhang"&&op=="addcart")
            {
                AddCart();
                return;
            }
            if(mode=="banhang"&&op=="removecart")
            {
                RemoveCart();
                return;
            }
            if(mode=="banhang"&&op=="checkout")
            {
                Checkout();
                return;
            }
            throw new Exception("Thao tác không hợp lệ.");
        }
        void Redirect(string url)
        {
            Response.Redirect(url,false);
            Context.ApplicationInstance.CompleteRequest();
        }
        protected Dictionary<string,int> Cart
        {
            get
            {
                if(Session["cart"]==null)Session["cart"]=new Dictionary<string,int>();
                return (Dictionary<string,int>)Session["cart"];
            }
        }
        protected static readonly string[] Features=
        {
            "tongquan","banhang","sanpham","danhmuc","khachhang","nhacungcap","phieunhap","hoadon","baohanh","nhanvien","baocao","caidat"
        }
        ;
        protected static readonly string[] FeatureLabels=
        {
            "Tổng quan","Bán hàng","Sản phẩm","Danh mục","Khách hàng","Nhà cung cấp","Nhập hàng","Hóa đơn","Bảo hành","Nhân viên","Báo cáo","Cài đặt"
        }
        ;
        [Serializable] protected class ImportLine
        {
            public int Qty;
            public decimal Price;
        }
        protected Dictionary<string,ImportLine> ImportCart
        {
            get
            {
                if(Session["importcart"]==null)Session["importcart"]=new Dictionary<string,ImportLine>();
                return (Dictionary<string,ImportLine>)Session["importcart"];
            }
        }
        // Mỗi form ghi đè các phương thức tương ứng với nghiệp vụ của mình.
        protected virtual void EntityPage(Entity en) { throw new InvalidOperationException("Chức năng không thuộc form này."); }
        protected virtual void SaveEntity(Entity en) { throw new InvalidOperationException("Chức năng không thuộc form này."); }
        protected virtual void DeleteEntity(Entity en) { throw new InvalidOperationException("Chức năng không thuộc form này."); }
        protected virtual void Dashboard() { throw new InvalidOperationException("Chức năng không thuộc form này."); }
        protected virtual void Sales() { throw new InvalidOperationException("Chức năng không thuộc form này."); }
        protected virtual void AddCart() { throw new InvalidOperationException("Chức năng không thuộc form này."); }
        protected virtual void RemoveCart() { throw new InvalidOperationException("Chức năng không thuộc form này."); }
        protected virtual void Checkout() { throw new InvalidOperationException("Chức năng không thuộc form này."); }
        protected virtual void Invoices() { throw new InvalidOperationException("Chức năng không thuộc form này."); }
        protected virtual void Report() { throw new InvalidOperationException("Chức năng không thuộc form này."); }
        protected virtual void AdvancedReport() { throw new InvalidOperationException("Chức năng không thuộc form này."); }
        protected virtual void ExportReport() { throw new InvalidOperationException("Chức năng không thuộc form này."); }
        protected virtual void Permissions() { throw new InvalidOperationException("Chức năng không thuộc form này."); }
        protected virtual void InitPermissions() { throw new InvalidOperationException("Chức năng không thuộc form này."); }
        protected virtual void SavePermissions() { throw new InvalidOperationException("Chức năng không thuộc form này."); }
        protected virtual void Accounts() { throw new InvalidOperationException("Chức năng không thuộc form này."); }
        protected virtual void SaveAccount() { throw new InvalidOperationException("Chức năng không thuộc form này."); }
        protected virtual void BackupPage() { throw new InvalidOperationException("Chức năng không thuộc form này."); }
        protected virtual void MakeBackup() { throw new InvalidOperationException("Chức năng không thuộc form này."); }
        protected virtual void ImportPage() { throw new InvalidOperationException("Chức năng không thuộc form này."); }
        protected virtual void AddImport() { throw new InvalidOperationException("Chức năng không thuộc form này."); }
        protected virtual void RemoveImport() { throw new InvalidOperationException("Chức năng không thuộc form này."); }
        protected virtual void SaveImport() { throw new InvalidOperationException("Chức năng không thuộc form này."); }

    }
}

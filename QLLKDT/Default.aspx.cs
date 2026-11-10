using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


namespace QLLKDT
{
    public partial class _Default : SecurePage
    {
        private int CurrentPage
        {
            get
            {
                return (int)(ViewState["CurrentPage"] ?? 0);
            }
            set
            {
                ViewState["CurrentPage"] = Math.Max(0, value);
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    LoadFilters();
                    if (Request.QueryString["category"] != null && ddlCategory.Items.FindByValue(Request.QueryString["category"]) != null) ddlCategory.SelectedValue = Request.QueryString["category"];
                    LoadData();
                }
            }
            catch (Exception)
            {
                ShowError();
            }
        }
        private DataTable Query(string sql, params SqlParameter[] parameters)
        {
            return KetNoi.ExecuteTable(sql, parameters);
        }
        private void LoadFilters()
        {
            var categories = KetNoi.ExecuteTable("SELECT MaLoai, TenLoai FROM dbo.LOAILINHKIEN ORDER BY TenLoai");
            ddlCategory.DataSource = categories;
            ddlCategory.DataTextField = "TenLoai";
            ddlCategory.DataValueField = "MaLoai";
            ddlCategory.DataBind();
            ddlCategory.Items.Insert(0, new ListItem("Tất cả danh mục", ""));
            DataTable chip = categories.Clone();
            DataRow all = chip.NewRow();
            all["MaLoai"] = "";
            all["TenLoai"] = "Tất cả";
            chip.Rows.Add(all);
            foreach (DataRow row in categories.Rows) chip.ImportRow(row);
            rptCategories.DataSource = chip;
            rptCategories.DataBind();
            ddlBrand.DataSource = Query("SELECT DISTINCT ThuongHieu FROM dbo.LINHKIEN WHERE ThuongHieu IS NOT NULL AND ThuongHieu <> N'' ORDER BY ThuongHieu");
            ddlBrand.DataTextField = "ThuongHieu";
            ddlBrand.DataValueField = "ThuongHieu";
            ddlBrand.DataBind();
            ddlBrand.Items.Insert(0, new ListItem("Tất cả thương hiệu", ""));
        }
        private void LoadData()
        {
            var stats = Query(@"SELECT (SELECT COUNT(*) FROM dbo.LINHKIEN WHERE TrangThai = N'Đang kinh doanh') AS Products, (SELECT COUNT(*) FROM dbo.HOADON WHERE NgayLap >= CONVERT(date,GETDATE()) AND NgayLap < DATEADD(day,1,CONVERT(date,GETDATE()))) AS Orders, (SELECT ISNULL(SUM(ThanhTien),0) FROM dbo.HOADON WHERE NgayLap >= CONVERT(date,GETDATE()) AND NgayLap < DATEADD(day,1,CONVERT(date,GETDATE()))) AS Revenue, (SELECT COUNT(*) FROM dbo.KHACHHANG) AS Customers").Rows[0];
            litProducts.Text = Convert.ToString(stats["Products"]);
            litOrders.Text = Convert.ToString(stats["Orders"]);
            litRevenue.Text = Money(stats["Revenue"]);
            litCustomers.Text = Convert.ToString(stats["Customers"]);
            var products = Query(@"SELECT l.MaLK,l.TenLK,l.GiaBan,l.TrangThai,l.HinhAnh,lo.TenLoai FROM dbo.LINHKIEN l INNER JOIN dbo.LOAILINHKIEN lo ON lo.MaLoai=l.MaLoai WHERE (@keyword=N'' OR l.MaLK LIKE N'%'+@keyword+N'%' OR l.TenLK LIKE N'%'+@keyword+N'%') AND (@category='' OR l.MaLoai=@category) AND (@brand=N'' OR l.ThuongHieu=@brand) ORDER BY l.MaLK", new SqlParameter("@keyword",SqlDbType.NVarChar,100)
            {
                Value=txtSearch.Text.Trim()
            }
            ,new SqlParameter("@category",SqlDbType.VarChar,10)
            {
                Value=ddlCategory.SelectedValue
            }
            ,new SqlParameter("@brand",SqlDbType.NVarChar,50)
            {
                Value=ddlBrand.SelectedValue
            }
            );
            int total = products.Rows.Count, size = 6, last = Math.Max(0,(total-1)/size);
            CurrentPage = Math.Min(CurrentPage,last);
            DataTable page = products.Clone();
            for(int i=CurrentPage*size;i<Math.Min(total,(CurrentPage+1)*size);i++) page.ImportRow(products.Rows[i]);
            rptProducts.DataSource=page;
            rptProducts.DataBind();
            pnlEmpty.Visible=total==0;
            litCount.Text = total == 0 ? "Hiển thị 0 sản phẩm" : String.Format("Hiển thị {0}–{1} / {2} sản phẩm",CurrentPage*size+1,Math.Min(total,(CurrentPage+1)*size),total);
            litPage.Text = String.Format("Trang {0} / {1}",CurrentPage+1,last+1);
            btnPrev.Enabled=CurrentPage>0;
            btnNext.Enabled=CurrentPage<last;
        }
        private void ShowError()
        {
            pnlError.Visible=true;
            litError.Text=" Vui lòng xem HƯỚNG DẪN trong tệp tải về.";
        }
        protected void Filter_Click(object sender, EventArgs e)
        {
            try
            {
                CurrentPage=0;
                LoadData();
            }
            catch(Exception)
            {
                ShowError();
            }
        }
        protected void Prev_Click(object sender, EventArgs e)
        {
            try
            {
                CurrentPage--;
                LoadData();
            }
            catch(Exception)
            {
                ShowError();
            }
        }
        protected void Next_Click(object sender, EventArgs e)
        {
            try
            {
                CurrentPage++;
                LoadData();
            }
            catch(Exception)
            {
                ShowError();
            }
        }
        public string Money(object value)
        {
            return Convert.ToDecimal(value).ToString("N0",CultureInfo.GetCultureInfo("vi-VN"))+" đ";
        }
        public string CategoryUrl(object value)
        {
            return "Default.aspx?category="+HttpUtility.UrlEncode(Convert.ToString(value));
        }
        public string ProductPhoto(object code, object image, object name, object category)
        {
            string id=Convert.ToString(code);
            int number;
            if(String.IsNullOrWhiteSpace(Convert.ToString(image)) && id.StartsWith("DSP") && Int32.TryParse(id.Substring(3),out number) && number>=1 && number<=10)
            {
                int pos=number-1;
                int x=(pos%5)*25;
                int y=pos/5*100;
                return "<div class='sample-photo' role='img' aria-label='"+HttpUtility.HtmlAttributeEncode(Convert.ToString(name))+"' style='background-position:"+x+"% "+y+"%'></div>";
            }
            return "<img src='"+HttpUtility.HtmlAttributeEncode(ImageUrl(image,name,category))+"' alt='"+HttpUtility.HtmlAttributeEncode(Convert.ToString(name))+"'/>";
        }
        public string ImageUrl(object image, object name, object category)
        {
            string path=Convert.ToString(image).Trim().Replace('\\','/');
            if(!String.IsNullOrEmpty(path) && !path.Contains("..") && !path.StartsWith("/") && !path.Contains(":") && System.Text.RegularExpressions.Regex.IsMatch(path,@"^Images/[A-Za-z0-9_./-]+$")) return ResolveUrl("~/"+path.TrimStart('~','/'));
            string text=(Convert.ToString(category)+" "+Convert.ToString(name)).ToLowerInvariant();
            if(text.Contains("ram")) return "Images/ram.png";
            if(text.Contains("cpu")) return "Images/cpu.png";
            if(text.Contains("vga")||text.Contains("card")) return "Images/vga.png";
            if(text.Contains("ssd")||text.Contains("ổ cứng")) return "Images/ssd.png";
            if(text.Contains("mainboard")||text.Contains("mạch chủ")) return "Images/mainboard.png";
            if(text.Contains("nguồn")) return "Images/nguon.png";
            return "Images/default.svg";
        }
    }
}

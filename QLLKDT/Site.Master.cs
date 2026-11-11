using System;
using System.Text;
using System.Web;
using System.Web.UI;
namespace QLLKDT {
 public partial class SiteMaster : MasterPage {
  protected string NavHtml="",RoleHtml="",TokenHtml="";
  string current;
  string Link(string key,string icon,string label) {
   if(!Access.Allowed(key=="kho"?"sanpham":key)) return "";
   string url=key=="sanpham"?"Default.aspx":DuongDan.LayTrang(key);
   return "<a class='nav-link "+(current==key?"active":"")+"' href='"+HttpUtility.HtmlAttributeEncode(url)+"'><span class='nav-icon'>"+HttpUtility.HtmlEncode(icon)+"</span>"+HttpUtility.HtmlEncode(label)+"</a>";
  }
  string Group(string icon,string title,string[] keys,string[] labels) {
   var children=new StringBuilder();bool expanded=false;
   for(int i=0;i<keys.Length;i++) { string link=Link(keys[i],"",labels[i]); if(link.Length>0){children.Append(link);if(current==keys[i])expanded=true;} }
   if(children.Length==0)return "";
   return "<details class='nav-group' "+(expanded?"open":"")+"><summary><span class='nav-icon'>"+HttpUtility.HtmlEncode(icon)+"</span>"+HttpUtility.HtmlEncode(title)+"<span class='nav-chevron'>⌄</span></summary><div class='nav-sub'>"+children.ToString()+"</div></details>";
  }
  protected void Page_Load(object sender,EventArgs e){
   current=DuongDan.ChucNang(Request.Path, Request.QueryString["m"]);
   var nav=new StringBuilder();
   nav.Append(Link("tongquan","⌂","Tổng quan"));
   nav.Append(Group("⬡","Linh kiện",new[]{"sanpham","danhmuc","kho"},new[]{"Sản phẩm","Danh mục","Tồn kho"}));
   nav.Append(Group("▤","Nghiệp vụ",new[]{"phieunhap","baohanh","hoadon"},new[]{"Nhập hàng","Bảo hành","Hóa đơn"}));
   NavHtml=nav.ToString();RoleHtml=HttpUtility.HtmlEncode(Access.UserRole());TokenHtml=HttpUtility.HtmlAttributeEncode(Access.Token);
  }
 }
}

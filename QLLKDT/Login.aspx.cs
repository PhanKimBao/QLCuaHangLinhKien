using System;
using System.Web;
using System.Web.UI;
namespace QLLKDT
{
    public partial class Login : Page
    {
        protected string Message="",Token="";
        protected void Page_Load(object sender,EventArgs e)
        {
            Token=HttpUtility.HtmlAttributeEncode(Access.Token);
            if(Request.HttpMethod=="POST")
            {
                try
                {
                    Access.CheckPost();
                    string user=(Request.Form["username"]??"").Trim();
                    string pass=Request.Form["password"]??"";
                    if(Access.Authenticate(user,pass))
                    {
                        Response.Redirect("QuanLy.aspx?m=tongquan",false);
                        Context.ApplicationInstance.CompleteRequest();
                        return;
                    }
                    Message="Tên đăng nhập hoặc mật khẩu không đúng, hoặc tài khoản đã khóa.";
                }
                catch
                {
                    Message="Không thể đăng nhập. Kiểm tra kết nối SQL Server và cấu hình database.";
                }
            }
            else if(Request.QueryString["db"]=="1")Message="Không thể kết nối database. Hãy kiểm tra Web.config.";
        }
    }
}

using System;
using System.Web.UI;
namespace QLLKDT
{
    public partial class Logout : Page
    {
        protected void Page_Load(object sender,EventArgs e)
        {
            try
            {
                Access.CheckPost();
                Session.Clear();
                Session.Abandon();
                Response.Redirect("Login.aspx",false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch
            {
                Response.StatusCode=400;
                Response.Write("Yêu cầu không hợp lệ.");
            }
        }
    }
}

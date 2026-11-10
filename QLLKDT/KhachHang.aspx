<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="KhachHang.aspx.cs" Inherits="QLLKDT.KhachHang" MasterPageFile="~/Site.Master" %>
<asp:Content ID="PageContent" ContentPlaceHolderID="MainContent" runat="server">
<form id="form1" runat="server">
 <div class="title"><div><h1>Quản lý khách hàng</h1><p>Thêm, sửa, xóa và tìm kiếm thông tin</p></div></div>
 <asp:Panel ID="pnlThongBao" runat="server" CssClass="alert" Visible="false"><asp:Literal ID="litThongBao" runat="server" /></asp:Panel>
 <section class="contentbox"><h2>Thông tin khách hàng</h2><asp:HiddenField ID="hdMaCu" runat="server" /><div class="formgrid">
 <div class="field"><label for="<%= txtMaKH.ClientID %>">Mã khách hàng *</label><asp:TextBox ID="txtMaKH" runat="server" MaxLength="10"/></div>
 <div class="field"><label for="<%= txtHoTen.ClientID %>">Họ tên *</label><asp:TextBox ID="txtHoTen" runat="server" MaxLength="50"/></div>
 <div class="field"><label for="<%= txtSDT.ClientID %>">Số điện thoại *</label><asp:TextBox ID="txtSDT" runat="server" MaxLength="15"/></div>
 <div class="field"><label for="<%= txtDiaChi.ClientID %>">Địa chỉ</label><asp:TextBox ID="txtDiaChi" runat="server" MaxLength="100"/></div>
 <div class="field"><label for="<%= txtEmail.ClientID %>">Email</label><asp:TextBox ID="txtEmail" runat="server" MaxLength="50" TextMode="Email"/></div>
 </div><div class="toolbar"><asp:Button ID="btnMoi" runat="server" Text="Thêm mới" CssClass="btnlink secondary" CausesValidation="false" OnClick="btnMoi_Click"/><asp:Button ID="btnLuu" runat="server" Text="Lưu" CssClass="btn" OnClick="btnLuu_Click"/><asp:Button ID="btnXoa" runat="server" Text="Xóa" CssClass="btn danger" CausesValidation="false" OnClientClick="return confirm('Xóa bản ghi đã chọn?');" OnClick="btnXoa_Click"/></div></section>
 <section class="contentbox"><h2>Danh sách</h2><div class="toolbar"><asp:TextBox ID="txtTim" runat="server" placeholder="Nhập mã hoặc tên để tìm..."/><asp:Button ID="btnTim" runat="server" Text="Tìm kiếm" CssClass="btn" CausesValidation="false" OnClick="btnTim_Click"/></div>
 <asp:GridView ID="gvDanhSach" runat="server" CssClass="datagrid" AutoGenerateColumns="true" AutoGenerateSelectButton="true" DataKeyNames="MaKH" OnSelectedIndexChanged="gvDanhSach_SelectedIndexChanged" EmptyDataText="Không có dữ liệu." />
 </section></form></asp:Content>

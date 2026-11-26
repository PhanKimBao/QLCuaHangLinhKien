<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Kho.aspx.cs" Inherits="QLLKDT.Kho" MasterPageFile="~/Site.Master" %>
<asp:Content ID="PageContent" ContentPlaceHolderID="MainContent" runat="server">
<form id="form1" runat="server">
 <div class="title"><div><h1>Tồn kho linh kiện</h1><p>Tra cứu số lượng và vị trí theo dữ liệu trong bảng TONKHO</p></div></div>
 <asp:Panel ID="pnlThongBao" runat="server" CssClass="alert bad" Visible="false"><asp:Literal ID="litThongBao" runat="server" /></asp:Panel>
 <section class="contentbox"><div class="toolbar"><asp:TextBox ID="txtTim" runat="server" placeholder="Tìm mã, tên linh kiện hoặc mã kho"/><asp:Button ID="btnTim" runat="server" Text="Tìm kiếm" CssClass="btn" OnClick="btnTim_Click"/></div>
 <asp:GridView ID="gvTonKho" runat="server" AutoGenerateColumns="false" CssClass="datagrid" EmptyDataText="Không tìm thấy dữ liệu tồn kho."><Columns>
 <asp:BoundField DataField="MaKho" HeaderText="Mã kho"/><asp:BoundField DataField="MaLK" HeaderText="Mã linh kiện"/><asp:BoundField DataField="TenLK" HeaderText="Tên linh kiện"/><asp:BoundField DataField="SoLuongTon" HeaderText="Số lượng tồn"/><asp:BoundField DataField="ViTri" HeaderText="Vị trí"/>
 </Columns></asp:GridView><p class="hint">Trang này chỉ tra cứu số liệu đã có; nhập hàng và bán hàng trong bản hiện tại chưa tự cập nhật tồn kho.</p></section>
</form></asp:Content>

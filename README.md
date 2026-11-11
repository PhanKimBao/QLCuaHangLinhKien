N3 - Quản lý cửa hàng thiết bị linh kiện
Giới thiệu
Đề tài xây dựng website hỗ trợ quản lý thông tin và hoạt động bán hàng của cửa hàng linh kiện điện tử. Website được xây dựng bằng ASP.NET Web Forms và kết nối cơ sở dữ liệu SQL Server.
Công nghệ sử dụng
- Ngôn ngữ: C#.
- Giao diện: ASP.NET Web Forms, HTML và CSS; có sử dụng Bootstrap.
- Công cụ phát triển: Visual Studio 2022.
- Cơ sở dữ liệu: SQL Server 2012.
- Quản lý mã nguồn: Git và GitHub.
Các chức năng trong project
- Đăng nhập, đăng xuất và phân quyền theo nhóm.
- Quản lý thông tin khách hàng, nhà cung cấp và nhân viên.
- Quản lý danh mục và sản phẩm linh kiện, hỗ trợ tải ảnh sản phẩm.
- Bán hàng, lập và xem hóa đơn; lập và xem phiếu nhập.
- Quản lý thông tin bảo hành.
- Xem tổng quan, báo cáo và thống kê.
- Quản lý tài khoản, thông tin cửa hàng và sao lưu cơ sở dữ liệu.
Phân công thành viên
Thành viên	Phần được phân công
Phan Kim Bảo	Quản lý khách hàng, nhà cung cấp, nhân viên, bán hàng, báo cáo và thống kê.
Nguyễn Anh Việt	Quản lý sản phẩm, kho, nhập hàng, bảo hành và hóa đơn.
Phạm Gia Thịnh	Quản lý tài khoản, phân quyền và cài đặt hệ thống.


Các file liên quan đến từng phần
Danh sách sau dựa trên bản project QLLKDT_Code_Trong_Tung_Form.zip:
- Phan Kim Bảo: KhachHang.aspx, NhaCungCap.aspx, NhanVien.aspx, BanHang.aspx, BaoCao.aspx và các file .aspx.cs tương ứng.
- Nguyễn Anh Việt: Default.aspx, SanPham.aspx, NhapHang.aspx, BaoHanh.aspx, HoaDon.aspx và các file .aspx.cs tương ứng. Phần kho có bảng dữ liệu liên quan nhưng cần tiếp tục hoàn thiện chức năng cập nhật tồn kho tự động.
- Phạm Gia Thịnh: TaiKhoan.aspx, PhanQuyen.aspx, CaiDat.aspx, SaoLuu.aspx và các file .aspx.cs tương ứng; xác thực và kiểm tra quyền liên quan tới Security/Access.cs.
Các file dùng chung gồm Site.Master, Site.Master.cs, KetNoi.cs, Web.config, DuongDan.cs, QuanLy.aspx.cs và thư mục Content. Nhóm nên trao đổi trước khi cùng sửa các file này để hạn chế xung đột Git.

Hướng dẫn chạy
1. Giải nén project, mở file QLLKDT.sln bằng Visual Studio 2022 qua File → Open → Project/Solution.
2. Chuẩn bị SQL Server 2012 và cơ sở dữ liệu QLCHLK. Nếu database đã có dữ liệu, không chạy lại script tạo database gốc.
3. Sửa chuỗi kết nối QLCHLK trong Web.config cho đúng tên SQL Server của máy.
4. Nếu chưa có bảng phân quyền, chạy Database/Bo_sung_phan_quyen.sql trong SSMS sau khi kiểm tra đúng database.
5. Chọn Build → Rebuild Solution, đặt Login.aspx làm trang bắt đầu và chạy project.
Dữ liệu mẫu và dữ liệu giao dịch mẫu trong thư mục Database chỉ nên chạy trên bản sao database để thử nghiệm vì có thể ảnh hưởng số liệu báo cáo.
Tình trạng hoàn thiện
- Mã nguồn được chia theo các form và code-behind; một số phần vẫn dùng chung lớp nền QuanLy.aspx.cs.
- Chức năng nhập và bán hàng chưa tự động cộng/trừ tồn kho; khôi phục bản sao lưu và hủy hóa đơn chưa hoàn thiện.
- Cần build và kiểm tra lại toàn bộ chức năng trên Visual Studio, IIS Express và SQL Server của nhóm trước khi triển khai hoặc trình bày.

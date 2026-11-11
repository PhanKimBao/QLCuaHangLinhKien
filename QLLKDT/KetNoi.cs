using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace QLLKDT
{
    /// <summary>
    /// Lớp kết nối và thực thi lệnh SQL Server dùng chung cho toàn bộ website.
    /// Chuỗi kết nối được lấy từ mục QLCHLK trong Web.config.
    /// </summary>
    public static class KetNoi
    {
        /// <summary>Tạo kết nối; hàm gọi chịu trách nhiệm đóng/giải phóng kết nối.</summary>
        public static SqlConnection CreateConnection()
        {
            ConnectionStringSettings setting = ConfigurationManager.ConnectionStrings["QLCHLK"];
            if (setting == null || string.IsNullOrWhiteSpace(setting.ConnectionString))
            {
                throw new ConfigurationErrorsException("Thiếu chuỗi kết nối QLCHLK trong Web.config.");
            }

            return new SqlConnection(setting.ConnectionString);
        }

        /// <summary>Chạy SELECT và trả về bảng dữ liệu.</summary>
        public static DataTable ExecuteQuery(string query, params SqlParameter[] parameters)
        {
            DataTable table = new DataTable();

            using (SqlConnection connection = CreateConnection())
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    connection.Open();
                    adapter.Fill(table);
                }
            }

            return table;
        }

        /// <summary>Chạy INSERT, UPDATE hoặc DELETE và trả về số dòng bị ảnh hưởng.</summary>
        public static int ExecuteNonQuery(string query, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = CreateConnection())
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                connection.Open();
                return command.ExecuteNonQuery();
            }
        }

        /// <summary>Chạy lệnh trả về một giá trị, ví dụ COUNT, SUM hoặc MAX.</summary>
        public static object ExecuteScalar(string query, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = CreateConnection())
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                connection.Open();
                return command.ExecuteScalar();
            }
        }

        /// <summary>Giữ tương thích với những form đang gọi ExecuteTable.</summary>
        public static DataTable ExecuteTable(string sql, params SqlParameter[] parameters)
        {
            return ExecuteQuery(sql, parameters);
        }
    }
}

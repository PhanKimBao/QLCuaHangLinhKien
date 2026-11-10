using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace QLLKDT
{
    // Các lệnh SQL dùng tham số; không ghép dữ liệu nhập vào câu lệnh.
    public static class KetNoi
    {
        public static SqlConnection CreateConnection()
        {
            var setting = ConfigurationManager.ConnectionStrings["QLCHLK"];
            if (setting == null)
                throw new ConfigurationErrorsException("Thiếu chuỗi kết nối QLCHLK trong Web.config.");
            return new SqlConnection(setting.ConnectionString);
        }

        public static DataTable ExecuteTable(string sql, params SqlParameter[] parameters)
        {
            using (var connection = CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            using (var adapter = new SqlDataAdapter(command))
            {
                if (parameters != null) command.Parameters.AddRange(parameters);
                var result = new DataTable();
                adapter.Fill(result);
                return result;
            }
        }
    }
}

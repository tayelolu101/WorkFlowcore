using Sybase.Data.AseClient;
using System;
using System.Configuration;
using System.Data;
using System.Threading.Tasks;

namespace WorkFlowCore.AppCode
{
    public class PhoenixValidator
    {
        public static async Task<Tuple<int, string, string, string>> getStaffPhoenixDetails( int employee_id )
        {
            string staffStatus      = string.Empty;
            string employee_number  = string.Empty;
            string fullname         = string.Empty;
            string connectionString = ConfigurationManager.ConnectionStrings["sybaseconnection"].ToString();
            string queryString      = " SELECT a.staff_id AS employee_number, r.status AS status, a.fullname AS fullname " +
                                        " FROM phoenix..ad_gb_rsm r,  " +
                                        " ZENBASE..zib_applications_users a  " +
                                        " WHERE r.user_name = a.user_id  " +
                                        " AND r.employee_id = "+ employee_id;

            try
            {
                using (AseConnection connection = new AseConnection(connectionString))
                {
                    using (AseCommand command = new AseCommand(queryString, connection))
                    {
                        command.CommandTimeout = 0;
                        connection.Open();
                        AseDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            staffStatus = reader["status"].ToString().Trim();
                            employee_number = reader["employee_number"].ToString().Trim();
                            fullname = reader["fullname"].ToString().Trim();
                        }
                        reader.Close();

                        return await Task.FromResult(Tuple.Create(0, staffStatus, employee_number, fullname));
                    }
                }
            } catch (Exception ex)
            {
                return Tuple.Create(-1,ex.Message, employee_number, fullname) ;
            }

        }

        public async static Task<Tuple<int, string, string >> getAccountType(string account_number)
        {
            string acct_type    =   string.Empty;
            string acct_name    =   string.Empty;
            string query        =   " SELECT a.acct_type AS acct_type, " +
                                    " a.title_1 AS acct_name " +
                                    " FROM phoenix..dp_acct a, " +
                                    " phoenix..rm_acct c " +
                                    " WHERE a.acct_no = '" + account_number + "'" +
                                    " AND a.rim_no  = c.rim_no ";

            using (AseConnection conn = new AseConnection(ConfigurationManager.ConnectionStrings["sybaseconnection"].ConnectionString))
            {
                using (AseCommand cmd = new AseCommand(query, conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = query;
                        cmd.CommandTimeout = 0;

                        AseDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            acct_name = reader["acct_name"].ToString();
                            acct_type = reader["acct_type"].ToString();                            
                        }

                        reader.Close();
                        return await Task.FromResult(Tuple.Create( 0, acct_name, acct_type ));

                    } catch (Exception ex) {
                        return Tuple.Create(-1, string.Empty, ex.Message);
                    }
                };
            };
        }
    }
}
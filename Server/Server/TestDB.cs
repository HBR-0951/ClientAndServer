using System;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;

namespace Server
{
	public class TestDB
	{
        protected static MySqlConnection con;
        protected string sql = "select * from TestDB.Login";
        protected MySqlCommand mySqlCmd;
        MySqlConnectionStringBuilder conn_string = new MySqlConnectionStringBuilder();

#pragma warning disable CS8618 // 退出建構函式時，不可為 Null 的欄位必須包含非 Null 值。請考慮宣告為可為 Null。
        public TestDB()
#pragma warning restore CS8618 // 退出建構函式時，不可為 Null 的欄位必須包含非 Null 值。請考慮宣告為可為 Null。
        {
            conn_string.Server = "127.0.0.1";
            conn_string.Port = 3306;
            conn_string.UserID = "root";
            conn_string.Password = "a0951521314";
            conn_string.Database = "TestDB";

            con = new MySqlConnection(conn_string.ToString());
            mySqlCmd = new MySqlCommand(sql, con);
            
        }

        public void Run()
        {
            try
            {
                con.Open();

                var result = mySqlCmd.ExecuteReader();

                while (result.Read())
                {
                    Console.WriteLine($"userid: [{result.GetString("userID")}] "
                                    + $"password: [{result.GetString("password")}] "
                                    + $"lastLoginTime: [{result.GetString("lastLoginTime")}]");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                con.Close();
            }
        }
    }
}


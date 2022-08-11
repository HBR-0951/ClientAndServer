using System;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;
using System.Text;


namespace Server
{
	public class TestDB
	{
        protected  MySqlConnection con;

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
            //mySqlCmd = new MySqlCommand(sql, con);
            
        }

        public void OnStart()
        {
            try
            {
                con.Open();
                Console.WriteLine("Connect to TestDB successfully!");
            }
            catch (MySqlException ex)
            {
                Console.Write("登入失敗： ");
                switch (ex.Number)
                {
                    case 1042:
                        Console.WriteLine("無法連線，找不到資料庫");
                        break;
                    case 0:
                        Console.WriteLine("使用者帳號or密碼錯誤");
                        break;
                    default:
                        Console.WriteLine("未開啟目標資料庫");
                        break;

                }
            }
        }
        public void OnClosed()
        {
            con.Close();
        }

        public bool UserLogin(string id, string password)
        {
            string sql = $"select * from TestDB.Login where UserID = {id};";
 
            var mySqlCmd = new MySqlCommand(sql, con);
            var result = mySqlCmd.ExecuteReader();
            while (result.Read())
            {
                if(result.GetString("password") == password)
                {
                    result.Close();
                    return true;
                }
            }
            result.Close();
            return false;

        }
    }
}


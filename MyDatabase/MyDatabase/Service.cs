using System;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;
using System.Text;


namespace MyDatabase
{
    public class Service
    {
        protected MySqlConnection con;

        MySqlConnectionStringBuilder conn_string = new MySqlConnectionStringBuilder();



#pragma warning disable CS8618 // 退出建構函式時，不可為 Null 的欄位必須包含非 Null 值。請考慮宣告為可為 Null。
        public Service()
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

        // 啟動
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
        // 關閉
        public void OnClosed()
        {
            Console.WriteLine("Close the connected with TestDB");
            con.Close();
        }

        // 使用者登入
        public bool UserLogin(string id, string password)
        {
            string sql = $"select * from TestDB.Login where UserID = {id};";

            var mySqlCmd = new MySqlCommand(sql, con);
            var result = mySqlCmd.ExecuteReader();
            while (result.Read())
            {
                if (result.GetString("password") == password)
                {
                    result.Close();
                    return true;
                }
            }
            result.Close();
            return false;

        }

        // 使用sql指令
        public bool OnUseSql(string sql)
        {
            try
            {
                var mySqlCmd = new MySqlCommand(sql, con);
                var result = mySqlCmd.ExecuteReader();
                result.Close();
   
            }
            catch (MySqlException e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        //// 新增
        //public void Insert()
        //{
        //    Console.WriteLine("TestDB.Insert");
        //    string sql = $"insert into TestDB.Login values('2', '3456', '2022-10-09 14:43:12');";
        //    try
        //    {
        //        var mySqlCmd = new MySqlCommand(sql, con);
        //        var result = mySqlCmd.ExecuteReader();
        //        result.Close();
        //    }
        //    catch(MySqlException e)
        //    {
        //        Console.WriteLine(e.Message);
        //    }



            //}

            //// 刪除
            //public void Delete()
            //{
            //    Console.WriteLine("TestDB.Delete");

            //    string sql = $"delete from TestDB.Login where userID = 2;";
            //    try
            //    {
            //        var mySqlCmd = new MySqlCommand(sql, con);
            //        var result = mySqlCmd.ExecuteReader();
            //        result.Close();
            //    }
            //    catch (MySqlException e)
            //    {
            //        Console.WriteLine(e.Message);
            //    }


            //}

            //// 修改
            //public void Update()
            //{
            //    Console.WriteLine("TestDB.Update");

            //    string sql = $"update TestDB.Login set userID = 6 where userID = 5;";

            //    try
            //    {
            //        var mySqlCmd = new MySqlCommand(sql, con);
            //        var result = mySqlCmd.ExecuteReader();
            //        result.Close();
            //    }
            //    catch (MySqlException e)
            //    {
            //        Console.WriteLine(e.Message);
            //    }
            //}
            //// 查詢
            //public void Search()
            //{
            //    Console.WriteLine("TestDB.Search");

            //    string sql = $"select * from TestDB.Login";

            //    try
            //    {
            //        var mySqlCmd = new MySqlCommand(sql, con);
            //        var result = mySqlCmd.ExecuteReader();

            //        while (result.Read())
            //        {
            //            Console.WriteLine($"userID: [{result.GetString("userID")}], password: [{result.GetString("password")}], " +
            //                $"lastLoginTime: [{result.GetString("lastLoginTime")}]");
            //        }
            //    }
            //    catch (MySqlException e)
            //    {
            //        Console.WriteLine(e.Message);
            //    }
            //}
    }
}


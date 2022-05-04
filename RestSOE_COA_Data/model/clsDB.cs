using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RestSOE_COA_Data.model
{

    public class clsDB : IDisposable
    {
        private SqlConnection objConn;

        static string _SQL_ConnectString = Setting._SQL_ConnectString;


        public clsDB()
        {
            objConn = new SqlConnection(_SQL_ConnectString);
        }


        public clsDB(string ConnStr)
        {
            objConn = new SqlConnection(ConnStr);
        }


        public void Dispose()
        {
            this.ToConnClose();
        }

        /// <summary>
        /// 開啟連線
        /// </summary>
        public bool ToConnOpen()
        {
            bool result = true;

            try
            {
                if (objConn.State != ConnectionState.Open)
                {
                    objConn.Open();
                }
            }
            catch
            {
                result = false;
            }

            return result;
        }



        /// <summary>
        /// 關閉連線
        /// </summary>
        public bool ToConnClose()
        {
            bool result = true;

            try
            {
                if (objConn.State != ConnectionState.Closed)
                {
                    objConn.Close();
                }
            }
            catch
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// 執行語法
        /// </summary>
        /// <param name="sql">語法</param>
        /// <param name="sqlParams">參數</param>
        /// <param name="IsTry">是否嘗試</param>
        public bool ToExecute(string sql, SqlParameter[] sqlParams, bool IsTry)
        {
            bool result = true;
            SqlCommand objCmd = new SqlCommand(sql, objConn);

            if (IsTry)
            {
                try
                {
                    if (objConn.State != ConnectionState.Open)
                    {
                        objConn.Open();
                    }

                    if (sqlParams != null)
                    {
                        objCmd.Parameters.AddRange(sqlParams);
                    }

                    objCmd.ExecuteNonQuery();

                    if (objConn.State != ConnectionState.Closed)
                    {
                        objConn.Close();
                    }
                }
                catch
                {
                    result = false;
                }
            }
            else
            {
                if (objConn.State != ConnectionState.Open)
                {
                    objConn.Open();
                }

                if (sqlParams != null)
                {
                    objCmd.Parameters.AddRange(sqlParams);
                }

                objCmd.ExecuteNonQuery();

                if (objConn.State != ConnectionState.Closed)
                {
                    objConn.Close();
                }
            }

            return result;
        }



        /// <summary>
        /// 執行語法 -- 不嘗試
        /// </summary>
        /// <param name="sql">語法</param>
        /// <param name="sqlParams">參數</param>
        public bool ToExecute(string sql, SqlParameter[] sqlParams)
        {
            return ToExecute(sql, sqlParams, false);
        }



        /// <summary>
        /// 執行語法 -- 不嘗試
        /// </summary>
        /// <param name="sql">語法</param>
        public bool ToExecute(string sql)
        {
            return ToExecute(sql, null, false);
        }



        /// <summary>
        /// 回傳讀取器 -- 需傳入SQL參數
        /// </summary>
        public SqlDataReader ToReader(string sql, SqlParameter[] sqlParams)
        {
            SqlCommand objCmd = new SqlCommand();
            objCmd.CommandText = sql;
            objCmd.Connection = objConn;

            if (sqlParams != null)
            {
                objCmd.Parameters.AddRange(sqlParams);
            }

            if (objConn.State != ConnectionState.Open)
            {
                objConn.Open();
            }

            SqlDataReader objRdr = objCmd.ExecuteReader();

            return objRdr;
        }



        /// <summary>
        /// 回傳讀取器
        /// </summary>
        public SqlDataReader ToReader(string sql)
        {
            return this.ToReader(sql, null);
        }



        /// <summary>
        /// 回傳資料表 -- 需傳入SQL參數
        /// </summary>
        public DataTable ToDataTable(string sql, SqlParameter[] sqlParams)
        {
            DataTable objTab = new DataTable();

            SqlCommand objCmd = new SqlCommand();
            objCmd.CommandText = sql;
            objCmd.Connection = objConn;

            if (sqlParams != null)
            {
                objCmd.Parameters.AddRange(sqlParams);
            }

            SqlDataAdapter objDa = new SqlDataAdapter(objCmd);
            objDa.Fill(objTab);

            return objTab;
        }



        /// <summary>
        /// 回傳資料表
        /// </summary>
        public DataTable ToDataTable(string sql)
        {
            return this.ToDataTable(sql, null);
        }



        /// <summary>
        /// 回傳資料表 -- 需傳入SQL參數
        /// </summary>
        public DataSet ToDataSet(string sql, SqlParameter[] sqlParams)
        {
            DataSet objDs = new DataSet();

            SqlCommand objCmd = new SqlCommand();
            objCmd.CommandText = sql;
            objCmd.Connection = objConn;

            if (sqlParams != null)
            {
                objCmd.Parameters.AddRange(sqlParams);
            }

            SqlDataAdapter objDa = new SqlDataAdapter(objCmd);
            objDa.Fill(objDs);

            return objDs;
        }



        /// <summary>
        /// 回傳資料表
        /// </summary>
        public DataSet ToDataSet(string sql)
        {
            return this.ToDataSet(sql, null);
        }

        /// <summary>
        /// 回傳單筆資料結果
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public string getResult(string sql)
        {
            DataTable dt = this.ToDataTable(sql);
            if (dt.Rows.Count == 0)
            {
                return string.Empty;
            }
            return dt.Rows[0][0].ToString();
        }

        /// <summary>
        /// 回傳單筆資料結果
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="sqlParams"></param>
        /// <returns></returns>
        public string getResult(string sql, SqlParameter[] sqlParams)
        {
            DataTable dt = this.ToDataTable(sql, sqlParams);
            if (dt.Rows.Count == 0)
            {
                return string.Empty;
            }
            return dt.Rows[0][0].ToString();
        }

        /// <summary>
        /// Table-Value-Paramter， 資料表值參數，SQL2008起支援,需注意有無建立 使用者定義資料表類型, 
        /// ref:http://blog.darkthread.net/post-2014-05-08-tvp-for-where-in-param.aspx
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="colName">自訂欄位名稱</param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static DataTable GetTVPValue<T>(params T[] args)
        {
            DataTable t = new DataTable();
            string colName = "item"; //DB裡的固定名稱
            t.Columns.Add(colName, typeof(T));
            foreach (T item in args)
            {
                t.Rows.Add(item);
            }
            return t;
        }
    }

    //-----------------------------------------------------------------------------------------
    public static class DataTableExtensions
    {
        public static IList<T> ToList<T>(this DataTable table) where T : new()
        {
            IList<PropertyInfo> properties = typeof(T).GetProperties().ToList();
            IList<T> result = new List<T>();

            //取得DataTable所有的row data
            foreach (var row in table.Rows)
            {
                var item = MappingItem<T>((DataRow)row, properties);
                result.Add(item);
            }

            return result;
        }

        private static T MappingItem<T>(DataRow row, IList<PropertyInfo> properties) where T : new()
        {
            T item = new T();
            foreach (var property in properties)
            {
                if (row.Table.Columns.Contains(property.Name))
                {
                    //針對欄位的型態去轉換
                    if (property.PropertyType == typeof(DateTime))
                    {
                        DateTime dt = new DateTime();
                        if (DateTime.TryParse(row[property.Name].ToString(), out dt))
                        {
                            property.SetValue(item, dt, null);
                        }
                        else
                        {
                            property.SetValue(item, null, null);
                        }
                    }
                    else if (property.PropertyType == typeof(decimal))
                    {
                        decimal val = new decimal();
                        decimal.TryParse(row[property.Name].ToString(), out val);
                        property.SetValue(item, val, null);
                    }
                    else if (property.PropertyType == typeof(double))
                    {
                        double val = new double();
                        double.TryParse(row[property.Name].ToString(), out val);
                        property.SetValue(item, val, null);
                    }
                    else if (property.PropertyType == typeof(int))
                    {
                        int val = new int();
                        int.TryParse(row[property.Name].ToString(), out val);
                        property.SetValue(item, val, null);
                    }
                    else
                    {
                        if (row[property.Name] != DBNull.Value)
                        {
                            property.SetValue(item, row[property.Name], null);
                        }
                    }
                }
            }
            return item;
        }
    }
}

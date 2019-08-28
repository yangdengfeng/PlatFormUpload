using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatFormUpload.ServiceInterface
{
    public abstract class SqlHelper
    {
        public static readonly string ConnectionStringMSSQL = ConfigurationManager.ConnectionStrings["conn"].ConnectionString.ToString();
        public static readonly string ConnectionStringMSSQL1 = ConfigurationManager.ConnectionStrings["conn1"].ConnectionString.ToString();
        public static readonly string ConnectionStringMSSQL2 = ConfigurationManager.ConnectionStrings["conn2"].ConnectionString.ToString();
        public static readonly string ConnectionStringMSSQL3 = ConfigurationManager.ConnectionStrings["conn3"].ConnectionString.ToString();
        private static Hashtable parmCache = Hashtable.Synchronized(new Hashtable());
        public static int ExecuteNonQuery(string connectionString, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            //Log.Write(cmdText,"sql",false);
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                cmd.CommandTimeout = 60 * 5;
                int val = cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
                return val;
            }
        }

        public static bool ExecuteNonQuery(string connectionString, List<string> sqlList)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                bool iserror = false;
                string strerror = "";
                SqlTransaction SqlTran = conn.BeginTransaction();
                try
                {
                    for (int i = 0; i < sqlList.Count; i++)
                    {
                        SqlCommand cmd = new SqlCommand();
                        cmd.Connection = conn;
                        cmd.CommandTimeout = 60 * 5;
                        cmd.CommandText = sqlList[i];
                        cmd.Transaction = SqlTran;
                        cmd.CommandType = CommandType.Text;
                        int val = cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    iserror = true;
                    strerror = ex.Message;
                }
                finally
                {

                    if (iserror)
                    {
                        SqlTran.Rollback();
                        throw new Exception(strerror);
                    }
                    else
                    {
                        SqlTran.Commit();
                    }
                }
                if (iserror)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public static int ExecuteNonQuery(string connectionString, string sql)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                int val = 0;
                bool iserror = false;
                string strerror = "";
                SqlTransaction SqlTran = conn.BeginTransaction();
                try
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandTimeout = 60 * 5;
                    cmd.CommandText = sql;
                    cmd.Transaction = SqlTran;
                    cmd.CommandType = CommandType.Text;
                    val = cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    val = 0;
                    iserror = true;
                    strerror = ex.Message.ToString() + ex.StackTrace.ToString();
                }
                finally
                {

                    if (iserror)
                    {
                        SqlTran.Rollback();
                        throw new Exception(strerror);
                    }
                    else
                    {
                        SqlTran.Commit();
                    }
                }
                return val;
            }
        }

        public static int ExecuteNonQuery(SqlConnection connection, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            try
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                else if (connection.State == ConnectionState.Broken)
                {
                    try
                    {
                        connection.Close();
                    }
                    catch
                    {
                        //return -1;
                    }
                    connection.Open();
                }
            }
            catch
            {
            }

            PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
            cmd.CommandTimeout = 60 * 5;
            int val = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            connection.Dispose();
            return val;

        }

        public static int ExecuteNonQuery(SqlTransaction trans, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdText, commandParameters);
            cmd.CommandTimeout = 60 * 5;
            int val = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            //cmd.Dispose();
            return val;
        }

        public static SqlDataReader ExecuteReader(string connectionString, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            SqlConnection conn = new SqlConnection(connectionString);
            SqlDataReader rdr = null;
            try
            {
                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                cmd.CommandTimeout = 60 * 5;
                rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();
                return rdr;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (rdr != null)
                {
                    rdr.Close();
                    rdr.Dispose();
                }
                if (conn != null)
                {
                    conn.Dispose();
                }
            }
        }

        public static DataSet ExecuteDataSet(string connectionString, string cmdText)
        {
            DataSet __ds = new DataSet();
            try
            {
                SqlDataAdapter sda = new SqlDataAdapter(cmdText, connectionString);
                sda.Fill(__ds);
            }
            catch
            {
                throw;
            }
            return __ds;
        }

        public static DataSet ExecuteDataSet(string connectionString, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            DataSet __ds = new DataSet();
            SqlCommand cmd = new SqlCommand();
            SqlConnection conn = new SqlConnection(connectionString);
            SqlDataAdapter sda = new SqlDataAdapter();
            try
            {
                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                sda.SelectCommand = cmd;
                sda.Fill(__ds);
                cmd.Parameters.Clear();
                return __ds;
            }
            catch
            {
                conn.Close();
                //conn.Dispose();
                throw;
            }
            finally
            {
                cmd.Dispose();
                conn.Dispose();
            }
        }

        public static object ExecuteScalar(string connectionString, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
                object val = cmd.ExecuteScalar();
                cmd.Parameters.Clear();
                return val;
            }
        }

        public static object ExecuteScalar(SqlConnection connection, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
            object val = cmd.ExecuteScalar();
            cmd.Parameters.Clear();
            return val;
        }

        public static int UpdateByteField(SqlConnection connection, string p_TableName, string p_FieldName, string whereStr, byte[] p_bDatas)
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
            }
            catch
            {
            }
            if (p_bDatas == null || p_bDatas.Length == 0)
            {
                return ExecuteNonQuery(connection, CommandType.Text, string.Format("update {0} set {1}=NULL where {2}", p_TableName, p_FieldName, whereStr), null);
            }
            string strSql = string.Format("update {0} set {1}=@image where {2}", p_TableName, p_FieldName, whereStr);
            SqlCommand myCommand = new SqlCommand(strSql, connection);
            myCommand.CommandTimeout = 60 * 5;
            myCommand.Parameters.Add(new SqlParameter("@image", SqlDbType.Image, int.MaxValue));
            myCommand.Parameters["@image"].Value = p_bDatas;
            int val = myCommand.ExecuteNonQuery();
            return val;
        }

        public static int UpdateByteField(string connectionString, string p_TableName, string p_FieldName, string whereStr, byte[] p_bDatas)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                if (p_bDatas == null || p_bDatas.Length == 0)
                {
                    return ExecuteNonQuery(connectionString, string.Format("update {0} set {1}=NULL where {2}", p_TableName, p_FieldName, whereStr));
                }
                string strSql = string.Format("update {0} set {1}=@image where {2}", p_TableName, p_FieldName, whereStr);
                bool iserror = false;
                string strerror = "";
                int val = 0;
                SqlTransaction sqlTran = connection.BeginTransaction();
                try
                {
                    SqlCommand myCommand = new SqlCommand(strSql, connection);
                    myCommand.CommandTimeout = 60 * 5;
                    myCommand.Parameters.Add(new SqlParameter("@image", SqlDbType.Image, int.MaxValue));
                    myCommand.Parameters["@image"].Value = p_bDatas;
                    myCommand.Transaction = sqlTran;
                    val = myCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    iserror = true;
                    strerror = ex.Message.ToString() + ex.StackTrace.ToString();
                }
                finally
                {
                    if (iserror)
                    {
                        sqlTran.Rollback();
                        throw new Exception(strerror);
                    }
                    else
                    {
                        sqlTran.Commit();
                    }
                }
                return val;
            }
        }

        public static int RunProcWithReturn(string connectionString, string procName, IDataParameter[] paramAr)
        {
            using (SqlConnection sqlconn = new SqlConnection(connectionString))
            {

                if (sqlconn.State != ConnectionState.Open)
                {
                    sqlconn.Open();
                }

                SqlCommand cmd = new SqlCommand();

                cmd.Connection = sqlconn;
                //cmd.CommandTimeout = 60 * 3;
                //cmd.CommandText = "Categoriestest6";
                cmd.CommandText = procName;

                cmd.CommandType = CommandType.StoredProcedure;

                // 创建参数 

                //IDataParameter[] parameters = { 

                //     new SqlParameter("@Id", SqlDbType.Int,4) , 

                //     new SqlParameter("@CategoryName", SqlDbType.NVarChar,15) , 

                //     new SqlParameter("rval", SqlDbType.Int,4)                   // 返回值 

                // };

                //// 设置参数类型 

                //parameters[0].Direction = ParameterDirection.Output;        // 设置为输出参数 

                //parameters[1].Value = "testCategoryName";                   // 给输入参数赋值 

                //parameters[2].Direction = ParameterDirection.ReturnValue;   // 设置为返回值 

                //// 添加参数 

                //cmd.Parameters.Add(parameters[0]);

                //cmd.Parameters.Add(parameters[1]);

                //cmd.Parameters.Add(parameters[2]);

                if (paramAr != null)
                {
                    for (int i = 0; i < paramAr.Length; i++)
                    {
                        cmd.Parameters.Add(paramAr[i]);
                    }
                }

                cmd.ExecuteNonQuery();
                return Convert.ToInt32(paramAr[paramAr.Length - 1].Value);
            }
        }


        public static void CacheParameters(string cacheKey, params SqlParameter[] commandParameters)
        {
            parmCache[cacheKey] = commandParameters;
        }

        public static SqlParameter[] GetCachedParameters(string cacheKey)
        {
            SqlParameter[] cachedParms = (SqlParameter[])parmCache[cacheKey];
            if (cachedParms == null)
                return null;
            SqlParameter[] clonedParms = new SqlParameter[cachedParms.Length];

            for (int i = 0, j = cachedParms.Length; i < j; i++)
                clonedParms[i] = (SqlParameter)((ICloneable)cachedParms[i]).Clone();
            return clonedParms;
        }

        private static void PrepareCommand(SqlCommand cmd, SqlConnection conn, SqlTransaction trans, CommandType cmdType, string cmdText, SqlParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (trans != null)
                cmd.Transaction = trans;
            cmd.CommandType = cmdType;
            if (cmdParms != null)
            {
                foreach (SqlParameter parm in cmdParms)
                    cmd.Parameters.Add(parm);
            }
            //conn.Dispose();
        }
    }
}

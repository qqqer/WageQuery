﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WageQuery
{
    public static class SQLRepository
    {
        public static readonly string ERP_strConn = "Data Source=192.168.9.100;database=EpicorPilot;Initial Catalog=EpicorPilot;Persist Security Info=True;User ID=sa;Password=Admin@2017";
        public static readonly string APP_strConn = "Data Source=192.168.9.100;database=AppTest;Initial Catalog=AppTest;Persist Security Info=True;User ID=sa;Password=Admin@2017";
        public static readonly string hsbs_strConn = "Data Source = 192.168.9.20; database=HSBS;Initial Catalog = HSBS; Persist Security Info=True;User ID = sa; Password=Admin123!";
        public static readonly string test_strConn = "Data Source = 192.168.9.100; database=AppTest;Initial Catalog = AppTest; Persist Security Info=True;User ID = sa; Password=Admin@2017";

        public static readonly string my_strConn = "Data Source=BSPC251\\SQLEXPRESS;database=test;Initial Catalog=test;Persist Security Info=True;User ID=sa;Password=180014";
        //public static readonly string OA_strConn = ConfigurationManager.ConnectionStrings["OAConnString"].ToString();


        public static int ExecuteNonQuery(string connectionString, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                    int val = cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                    return val;
                }
            }
            catch
            {
                throw;
            }
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
        }

        public static object ExecuteScalarToObject(string connectionString, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
                    object val = cmd.ExecuteScalar();
                    cmd.Parameters.Clear();
                    return val;
                }
            }
            catch
            {
                throw;
            }
        }

        private static SqlDataReader ExecuteReader(string connectionString, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            SqlConnection conn = new SqlConnection(connectionString);

            // we use a try/catch here because if the method throws an exception we want to 
            // close the connection throw code, because no datareader will exist, hence the 
            // commandBehaviour.CloseConnection will not work
            try
            {
                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                SqlDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();

                return rdr;
            }
            catch
            {
                conn.Close();
                conn.Dispose();
                throw;
            }

        }

        private static DataSet convertDataReaderToDataSet(SqlDataReader reader)
        {
            DataSet dataSet = new DataSet();
            do
            {
                //   Create   new   data   table   

                DataTable schemaTable = reader.GetSchemaTable();
                DataTable dataTable = new DataTable();

                if (schemaTable != null)
                {
                    //   A   query   returning   records   was   executed   

                    for (int i = 0; i < schemaTable.Rows.Count; i++)
                    {
                        DataRow dataRow = schemaTable.Rows[i];
                        //   Create   a   column   name   that   is   unique   in   the   data   table   
                        string columnName = (string)dataRow["ColumnName"];   //+   "<C"   +   i   +   "/>";   
                        //   Add   the   column   definition   to   the   data   table   
                        DataColumn column = new DataColumn(columnName, (Type)dataRow["DataType"]);
                        dataTable.Columns.Add(column);
                    }

                    dataSet.Tables.Add(dataTable);

                    //   Fill   the   data   table   we   just   created   

                    while (reader.Read())
                    {

                        DataRow dataRow = dataTable.NewRow();

                        for (int i = 0; i < reader.FieldCount; i++)
                            dataRow[i] = reader.GetValue(i);

                        dataTable.Rows.Add(dataRow);
                    }
                }
                else
                {
                    //   No   records   were   returned   

                    DataColumn column = new DataColumn("RowsAffected");
                    dataTable.Columns.Add(column);
                    dataSet.Tables.Add(dataTable);
                    DataRow dataRow = dataTable.NewRow();
                    dataRow[0] = reader.RecordsAffected;
                    dataTable.Rows.Add(dataRow);
                }
            }
            while (reader.NextResult());
            reader.Close();
            return dataSet;
        }

        public static DataTable ExecuteQueryToDataTable(string connectionString, string sql)
        {
            var dr = ExecuteReader(connectionString, CommandType.Text, sql, null);
            var dt = convertDataReaderToDataSet(dr).Tables[0];

            return dt.Rows.Count > 0 ? dt : null;
        }

    }

}

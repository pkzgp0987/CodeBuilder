using System;
using System.Data;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using CodeHelper;
using System.Data.SQLite;
namespace DbObjects.SQLite
{
    /// <summary>
    /// ���ݿ���Ϣ�ࡣ
    /// </summary>
    public class DbObject 
    {
        //bool isdbosp = false;

        #region  ����
        public string DbType
        {
            get { return "SQLite"; }
        }
        private string _dbconnectStr;
        public string DbConnectStr
        {
            set { _dbconnectStr = value; }
            get { return _dbconnectStr; }
        }
        SQLiteConnection connect = new SQLiteConnection ();

        #endregion

        #region ���캯�������������Ϣ
        public DbObject()
        {
            //IsDboSp();
        }

        /// <summary>
        /// ����һ�����ݿ�����
        /// </summary>
        /// <param name="connect"></param>
        public DbObject(string DbConnectStr)
        {
            _dbconnectStr = DbConnectStr;
            connect.ConnectionString = DbConnectStr;
        }
        /// <summary>
        /// ����һ�������ַ���
        /// </summary>
        /// <param name="SSPI">�Ƿ�windows������֤</param>
        /// <param name="Ip">������IP</param>
        /// <param name="User">�û���</param>
        /// <param name="Pass">����</param>
        public DbObject(bool SSPI, string Ip, string User, string Pass)
        {
            connect = new SQLiteConnection ();
            if (SSPI)
            {
                //_dbconnectStr="Integrated Security=SSPI;Data Source="+Ip+";Initial Catalog=mysql";
                _dbconnectStr = String.Format("Data Source={0}; Password={1}", Ip, Pass);
            }
            else
            {
                _dbconnectStr = String.Format("Data Source={0};Password={1}", Ip, Pass);

            }
            connect.ConnectionString = _dbconnectStr;

        }


        #endregion


        #region �����ݿ� OpenDB(string DbName)

        /// <summary>
        /// �����ݿ�
        /// </summary>
        /// <param name="DbName">Ҫ�򿪵����ݿ�</param>
        /// <returns></returns>
        private SQLiteCommand OpenDB(string DbName)
        {
            try
            {
                if (connect.ConnectionString == "")
                {
                    connect.ConnectionString = _dbconnectStr;
                }
                if (connect.ConnectionString != _dbconnectStr)
                {
                    connect.Close();
                    connect.ConnectionString = _dbconnectStr;
                }
                SQLiteCommand dbCommand = new SQLiteCommand();
                dbCommand.Connection = connect;
                if (connect.State == System.Data.ConnectionState.Closed)
                {
                    connect.Open();
                }
                //dbCommand.CommandText = "use " + DbName + "";
                //dbCommand.ExecuteNonQuery();
                return dbCommand;

            }
            catch (System.Exception ex)
            {
                string str = ex.Message;
                return null;
            }

        }
        #endregion


        #region ��������

        /// <summary>
        /// List�����ַ�������
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>0������ȣ�-1����y����x��1����x����y</returns>
        private int CompareStrByOrder(string x, string y)
        {
            if (x == "")
            {
                if (y == "")
                {
                    return 0;// If x is null and y is null, they're equal. 
                }
                else
                {
                    return -1;// If x is null and y is not null, y is greater. 
                }
            }
            else
            {
                if (y == "")  // ...and y is null, x is greater.
                {
                    return 1;
                }
                else
                {
                    int retval = x.CompareTo(y);
                    return retval;
                }
            }
        }

        #endregion

        #region ADO.NET ����

        public int ExecuteSql(string DbName, string SQLString)
        {
            SQLiteCommand dbCommand = OpenDB(DbName);
            dbCommand.CommandText = SQLString;
            int rows = dbCommand.ExecuteNonQuery();
            return rows;
        }
        public DataSet Query(string DbName, string SQLString)
        {
            DataSet ds = new DataSet();
            try
            {
                OpenDB(DbName);
                SQLiteDataAdapter command = new SQLiteDataAdapter(SQLString, connect);
                command.Fill(ds, "ds");
            }
            catch (System.Data.SQLite.SQLiteException ex)
            {
                throw new Exception(ex.Message);
            }
            return ds;
        }
        public SQLiteDataReader ExecuteReader(string DbName, string strSQL)
        {
            try
            {
                OpenDB(DbName);
                SQLiteCommand cmd = new SQLiteCommand(strSQL, connect);
                SQLiteDataReader myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                return myReader;
            }
            catch (System.Data.SQLite.SQLiteException ex)
            {
                throw ex;
            }
        }
        public object GetSingle(string DbName, string SQLString)
        {
            try
            {
                SQLiteCommand dbCommand = OpenDB(DbName);
                dbCommand.CommandText = SQLString;
                object obj = dbCommand.ExecuteScalar();
                if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                {
                    return null;
                }
                else
                {
                    return obj;
                }
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// ִ�д洢����
        /// </summary>
        /// <param name="storedProcName">�洢������</param>
        /// <param name="parameters">�洢���̲���</param>
        /// <param name="tableName">DataSet����еı���</param>
        /// <returns>DataSet</returns>
        public DataSet RunProcedure(string DbName, string storedProcName, IDataParameter[] parameters, string tableName)
        {

            OpenDB(DbName);
            DataSet dataSet = new DataSet();
            SQLiteDataAdapter sqlDA = new SQLiteDataAdapter();
            sqlDA.SelectCommand = BuildQueryCommand(connect, storedProcName, parameters);
            sqlDA.Fill(dataSet, tableName);

            return dataSet;

        }
        private SQLiteCommand BuildQueryCommand(SQLiteConnection connection, string storedProcName, IDataParameter[] parameters)
        {
            SQLiteCommand command = new SQLiteCommand(storedProcName, connection);
            command.CommandType = CommandType.StoredProcedure;
            foreach (SQLiteParameter parameter in parameters)
            {
                if (parameter != null)
                {
                    // ���δ����ֵ���������,���������DBNull.Value.
                    if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                        (parameter.Value == null))
                    {
                        parameter.Value = DBNull.Value;
                    }
                    command.Parameters.Add(parameter);
                }
            }

            return command;
        }
        #endregion

        #region �õ����ݿ�������б� GetDBList()

        public List<string> GetDBList()
        {
            // TODO:  ��� DbObject.GetDBList ʵ��
            return null;
        }
        #endregion

        #region  �õ����ݿ�����б����ͼ ������
                
        /// <summary>
        /// �õ����ݿ�����б���
        /// </summary>
        /// <param name="DbName">���ݿ�</param>
        /// <returns></returns>
        public List<string> GetTables(string DbName)
        {
            string strSql = "select name from sqlite_master where type='table' order by name";
            List<string> tabNames = new List<string>();
            SQLiteDataReader reader = ExecuteReader(DbName, strSql);
            while (reader.Read())
            {
                tabNames.Add(reader.GetString(0));
            }
            reader.Close();
            //tabNames.Sort(CompareStrByOrder);
            return tabNames;
        }
        public DataTable GetTablesSP(string DbName)
        {
            SQLiteParameter[] parameters = {
					new SQLiteParameter("@table_name",System.Data.DbType.String,384),
					new SQLiteParameter("@table_owner", System.Data.DbType.String,384),
                    new SQLiteParameter("@table_qualifier", System.Data.DbType.String,384),
                    new SQLiteParameter("@table_type",System.Data.DbType.String,100)
            };
            parameters[0].Value = null;
            parameters[1].Value = null;
            parameters[2].Value = null;
            parameters[3].Value = "'TABLE'";

            DataSet ds = RunProcedure(DbName, "sp_tables", parameters, "ds");
            if (ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                dt.Columns["TABLE_QUALIFIER"].ColumnName = "db";
                dt.Columns["TABLE_OWNER"].ColumnName = "cuser";
                dt.Columns["TABLE_NAME"].ColumnName = "name";
                dt.Columns["TABLE_TYPE"].ColumnName = "type";
                dt.Columns["REMARKS"].ColumnName = "remarks";
                return dt;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// �õ����ݿ��������ͼ��
        /// </summary>
        /// <param name="DbName">���ݿ�</param>
        /// <returns></returns>
        public DataTable GetVIEWs(string DbName)
        {
            //if (isdbosp)
            //{
            //    return GetVIEWsSP(DbName);
            //}
            //string strSql="select [name] from sysobjects where xtype='V' and [name]<>'syssegments' and [name]<>'sysconstraints' order by [name]";//order by id
            //return Query(DbName,strSql).Tables[0];
            return null;
        }
        /// <summary>
        /// �õ����ݿ��������ͼ��
        /// </summary>
        /// <param name="DbName">���ݿ�</param>
        /// <returns></returns>
        public DataTable GetVIEWsSP(string DbName)
        {
            SQLiteParameter[] parameters = {
					new SQLiteParameter("@table_name", System.Data.DbType.String,384),
					new SQLiteParameter("@table_owner",System.Data.DbType.String,384),
                    new SQLiteParameter("@table_qualifier", System.Data.DbType.String,384),
                    new SQLiteParameter("@table_type",System.Data.DbType.String,100)
            };
            parameters[0].Value = null;
            parameters[1].Value = null;
            parameters[2].Value = null;
            parameters[3].Value = "'VIEW'";

            DataSet ds = RunProcedure(DbName, "sp_tables", parameters, "ds");
            if (ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                dt.Columns["TABLE_QUALIFIER"].ColumnName = "db";
                dt.Columns["TABLE_OWNER"].ColumnName = "cuser";
                dt.Columns["TABLE_NAME"].ColumnName = "name";
                dt.Columns["TABLE_TYPE"].ColumnName = "type";
                dt.Columns["REMARKS"].ColumnName = "remarks";
                return dt;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// �õ����ݿ�����б����ͼ��
        /// </summary>
        /// <param name="DbName">���ݿ�</param>
        /// <returns></returns>
        public DataTable GetTabViews(string DbName)
        {
            string strSql = "select name from sqlite_master WHERE type IN ('table','view') AND name NOT LIKE 'sqlite_%' order by name";//order by id
            DataTable dt = Query(DbName, strSql).Tables[0];
            dt.Columns[0].ColumnName = "name";
            return dt;
        }
        /// <summary>
        /// �õ����ݿ�����б����ͼ��
        /// </summary>
        /// <param name="DbName">���ݿ�</param>
        /// <returns></returns>
        public DataTable GetTabViewsSP(string DbName)
        {
            return null;
        }

        /// <summary>
        /// �õ����ݿ�����д洢������
        /// </summary>
        /// <param name="DbName">���ݿ�</param>
        /// <returns></returns>
        public DataTable GetProcs(string DbName)
        {
            return null;
        }
        #endregion

        #region  �õ����ݿ�����б����ͼ ���б���Ϣ
        /// <summary>
        /// �õ����ݿ�����б����ϸ��Ϣ
        /// </summary>
        /// <param name="DbName">���ݿ�</param>
        /// <returns></returns>
        public List<TableInfo> GetTablesInfo(string DbName)
        {
            List<TableInfo> tablist = new List<TableInfo>();
            TableInfo tab;
            string strSql = "select * from sqlite_master where type='table' order by name";
            SQLiteDataReader reader = ExecuteReader(DbName, strSql);
            while (reader.Read())
            {

                tab = new TableInfo();
                tab.TabName = reader["Name"].ToString();
                if (tab.TabName != "sqlite_sequence")
                {
                    tab.TabType = "U";
                    tab.TabUser = "";
                    tablist.Add(tab);
                }

            }
            reader.Close();
            return tablist;

        }
        /// <summary>
        /// �õ����ݿ��������ͼ����ϸ��Ϣ
        /// </summary>
        /// <param name="DbName">���ݿ�</param>
        /// <returns></returns>
        public List<TableInfo> GetVIEWsInfo(string DbName)
        {
            return null;
        }
        /// <summary>
        /// �õ����ݿ�����б����ͼ����ϸ��Ϣ
        /// </summary>
        /// <param name="DbName">���ݿ�</param>
        /// <returns></returns>
        public List<TableInfo> GetTabViewsInfo(string DbName)
        {
            List<TableInfo> tablist = new List<TableInfo>();
            TableInfo tab;
            string strSql = "select * from sqlite_master WHERE type IN ('table','view') AND name NOT LIKE 'sqlite_%' order by name";
            SQLiteDataReader reader = ExecuteReader(DbName, strSql);
            while (reader.Read())
            {
                tab = new TableInfo();
                tab.TabName = reader["Name"].ToString();
                //try
                //{
                //    if (reader["Create_time"] != null)
                //    {
                //        tab.TabDate = reader["Create_time"].ToString();
                //    }
                //}
                //catch
                //{ }               
                tab.TabType = "U";
                tab.TabUser = "";
                tablist.Add(tab);
            }
            reader.Close();
            return tablist;
        }
        /// <summary>
        /// �õ����ݿ�����д洢���̵���ϸ��Ϣ
        /// </summary>
        /// <param name="DbName">���ݿ�</param>
        /// <returns></returns>
        public List<TableInfo> GetProcInfo(string DbName)
        {
            return null;
        }
        #endregion

        #region �õ����������
        /// <summary>
        /// �õ���ͼ��洢���̵Ķ������
        /// </summary>
        /// <param name="DbName">���ݿ�</param>
        /// <returns></returns>
        public string GetObjectInfo(string DbName, string objName)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select sql ");
            strSql.Append("from sqlite_master   ");
            strSql.Append("where name= '" + objName+"'");    
            return GetSingle(DbName, strSql.ToString()).ToString();
        }
        #endregion

        #region �õ�(����)���ݿ��������������� GetColumnList(string DbName,string TableName)

        /// <summary>
        /// �õ����ݿ�������ͼ������������
        /// </summary>
        /// <param name="DbName">��</param>
        /// <param name="TableName">��</param>
        /// <returns></returns>
        public List<ColumnInfo> GetColumnList(string DbName, string TableName)
        {
            return GetColumnInfoList(DbName, TableName);

        }
        public List<ColumnInfo> GetColumnListSP(string DbName, string TableName)
        {
            return GetColumnInfoList(DbName, TableName);
        }
        #endregion


        #region �õ�����е���ϸ��Ϣ GetColumnInfoList(string DbName,string TableName)
        /// <summary>
        /// �õ����ݿ�������ͼ���е���ϸ��Ϣ
        /// </summary>
        /// <param name="DbName">��</param>
        /// <param name="TableName">��</param>
        /// <returns></returns>
        public List<ColumnInfo> GetColumnInfoList(string DbName, string TableName)
        {
            try
            {
                //if (isdbosp)
                //{               
                //   return GetColumnInfoListSP(DbName, TableName);                
                //}           
                string strSql = "select * from sqlite_master where type = 'table' and name='" + TableName+"'";
                List<ColumnInfo> collist = new List<ColumnInfo>();
                SQLiteDataReader reader = ExecuteReader(DbName, strSql);
                int n = 1;
                while (reader.Read())
                {
                   
                    string CreateTableStr = reader.GetString(4);
                    int startIndex=CreateTableStr.IndexOf("(");
                    int endIndex=CreateTableStr.IndexOf(")");
                    string CoulmnsStr = CreateTableStr.Substring(startIndex+1, endIndex-startIndex-1);
                    string[] coulmns = CoulmnsStr.Split(',');
                    foreach(string str in coulmns)
                    {
                        ColumnInfo col; col = new ColumnInfo();
                        col.Colorder = n.ToString();
                        if (str.Contains("PRIMARY KEY"))
                        {
                            col.IsPK = true;
                        }
                        else
                        {
                            col.IsPK = false;
                        }
                        string[] coulmnNameAndType = str.Split(' ');
                        col.ColumnName = coulmnNameAndType[0].Trim().Replace("\"", "");
                        col.TypeName = coulmnNameAndType[2].Trim();
                        collist.Add(col);
                        n++;
                    }
                    
                }
                reader.Close();
                return collist;
            }
            catch (System.Exception ex)
            {
                throw new Exception("��ȡ������ʧ��" + ex.Message);
            }

        }

        //���������� ����
        private void TypeNameProcess(string strName, out string TypeName, out string Length, out string Preci, out string Scale)
        {
            TypeName = strName;
            int n = strName.IndexOf("(");
            Length = "";
            Preci = "";
            Scale = "";
            if (n > 0)
            {
                TypeName = strName.Substring(0, n);
                switch (TypeName.Trim().ToUpper())
                {
                    //ֻ�д�С(M)
                    case "TINYINT":
                    case "SMALLINT":
                    case "MEDIUMINT":
                    case "INT":
                    case "INTEGER":
                    case "BIGINT":
                    case "TIMESTAMP":
                    case "CHAR":
                    case "VARCHAR":
                        {
                            int m = strName.IndexOf(")");
                            Length = strName.Substring(n + 1, m - n - 1);
                        }
                        break;
                    case "FLOAT"://(M,D)
                    case "DOUBLE":
                    case "REAL":
                    case "DECIMAL":
                    case "DEC":
                    case "NUMERIC":
                        {
                            int m = strName.IndexOf(")");
                            string strlen = strName.Substring(n + 1, m - n - 1);
                            int i = strlen.IndexOf(",");
                            Length = strlen.Substring(0, i);
                            Scale = strlen.Substring(i + 1);
                        }
                        break;
                    case "ENUM"://(M1,M2,M3)
                    case "SET":
                        {
                        }
                        break;
                    default:
                        break;
                }
            }

        }

        public DataTable GetColumnInfoListSP(string DbName, string TableName)
        {
            return null;
        }

        #endregion


        #region �õ����ݿ��������� GetKeyName(string DbName,string TableName)

        //��������Ϣ��
        public DataTable CreateColumnTable()
        {
            DataTable table = new DataTable();
            DataColumn col;

            col = new DataColumn();
            col.DataType = Type.GetType("System.String");
            col.ColumnName = "colorder";
            table.Columns.Add(col);

            col = new DataColumn();
            col.DataType = Type.GetType("System.String");
            col.ColumnName = "ColumnName";
            table.Columns.Add(col);

            col = new DataColumn();
            col.DataType = Type.GetType("System.String");
            col.ColumnName = "TypeName";
            table.Columns.Add(col);

            col = new DataColumn();
            col.DataType = Type.GetType("System.String");
            col.ColumnName = "Length";
            table.Columns.Add(col);

            col = new DataColumn();
            col.DataType = Type.GetType("System.String");
            col.ColumnName = "Preci";
            table.Columns.Add(col);

            col = new DataColumn();
            col.DataType = Type.GetType("System.String");
            col.ColumnName = "Scale";
            table.Columns.Add(col);

            col = new DataColumn();
            col.DataType = Type.GetType("System.String");
            col.ColumnName = "IsIdentity";
            table.Columns.Add(col);

            col = new DataColumn();
            col.DataType = Type.GetType("System.String");
            col.ColumnName = "isPK";
            table.Columns.Add(col);

            col = new DataColumn();
            col.DataType = Type.GetType("System.String");
            col.ColumnName = "cisNull";
            table.Columns.Add(col);

            col = new DataColumn();
            col.DataType = Type.GetType("System.String");
            col.ColumnName = "defaultVal";
            table.Columns.Add(col);

            col = new DataColumn();
            col.DataType = Type.GetType("System.String");
            col.ColumnName = "deText";
            table.Columns.Add(col);

            return table;
        }

        /// <summary>
        /// �õ����ݿ���������
        /// </summary>
        /// <param name="DbName">��</param>
        /// <param name="TableName">��</param>
        /// <returns></returns>
        public DataTable GetKeyName(string DbName, string TableName)
        {
            DataTable dtkey = CreateColumnTable();
            List<ColumnInfo> collist = GetColumnInfoList(DbName, TableName);
            DataTable dt = CodeHelper.CodeCommon.GetColumnInfoDt(collist);
            DataRow[] rows = dt.Select(" isPK='��' or IsIdentity='��' ");
            foreach (DataRow row in rows)
            {
                DataRow nrow = dtkey.NewRow();
                nrow["colorder"] = row["colorder"];
                nrow["ColumnName"] = row["ColumnName"];
                nrow["TypeName"] = row["TypeName"];
                nrow["Length"] = row["Length"];
                nrow["Preci"] = row["Preci"];
                nrow["Scale"] = row["Scale"];
                nrow["IsIdentity"] = row["IsIdentity"];
                nrow["isPK"] = row["isPK"];
                nrow["cisNull"] = row["cisNull"];
                nrow["defaultVal"] = row["defaultVal"];
                nrow["deText"] = row["deText"];
                dtkey.Rows.Add(nrow);
            }
            return dtkey;


        }
        #endregion

        #region �õ����ݱ�������� GetTabData(string DbName,string TableName)

        /// <summary>
        /// �õ����ݱ��������
        /// </summary>
        /// <param name="DbName"></param>
        /// <param name="TableName"></param>
        /// <returns></returns>
        public DataTable GetTabData(string DbName, string TableName)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select * from " + TableName + "");
            return Query(DbName, strSql.ToString()).Tables[0];
        }
        /// <summary>
        /// ����SQL��ѯ�õ����ݱ��������
        /// </summary>
        /// <param name="DbName"></param>
        /// <param name="TableName"></param>
        /// <returns></returns>
        public DataTable GetTabDataBySQL(string DbName, string strSQL)
        {
            return Query(DbName, strSQL).Tables[0];
        }

        #endregion


        #region ���ݿ����Բ���

        /// <summary>
        /// �޸ı�����
        /// </summary>
        /// <param name="OldName"></param>
        /// <param name="NewName"></param>
        /// <returns></returns>
        public bool RenameTable(string DbName, string OldName, string NewName)
        {
            try
            {
                SQLiteCommand dbCommand = OpenDB(DbName);
                dbCommand.CommandText = "RENAME TABLE " + OldName + " TO " + NewName + "";
                dbCommand.ExecuteNonQuery();
                return true;
            }
            catch//(System.Exception ex)
            {
                //string str=ex.Message;	
                return false;
            }
        }

        /// <summary>
        /// ɾ����
        /// </summary>	
        public bool DeleteTable(string DbName, string TableName)
        {
            try
            {
                SQLiteCommand dbCommand = OpenDB(DbName);
                dbCommand.CommandText = "DROP TABLE " + TableName + "";
                dbCommand.ExecuteNonQuery();
                return true;
            }
            catch//(System.Exception ex)
            {
                //string str=ex.Message;	
                return false;
            }
        }

        /// <summary>
        /// �õ��汾��
        /// </summary>
        /// <returns></returns>
        public string GetVersion()
        {
            try
            {
                string strSql = "execute master..sp_msgetversion ";//select @@version
                return Query("master", strSql).Tables[0].Rows[0][0].ToString();
            }
            catch//(System.Exception ex)
            {
                //string str=ex.Message;	
                return "";
            }
        }


        /// <summary>
        /// �õ������� �Ľű�
        /// </summary>
        /// <returns></returns>
        public string GetTableScript(string DbName, string TableName)
        {
            string strScript = "";
            string strSql = "SHOW CREATE TABLE " + TableName;
            SQLiteDataReader reader = ExecuteReader(DbName, strSql);
            while (reader.Read())
            {
                strScript = reader.GetString(1);
            }
            reader.Close();
            return strScript;

        }
        #endregion



    }
}

using System;
using System.Data;
using System.Text;
using System.Collections.Generic;
using CodeHelper;
using System.Windows.Forms;
using System.Data.OleDb;
using System.IO;
using FileCreate;
namespace DbObjects.OleDb
{
    /// <summary>
    /// DbObject ��ժҪ˵����
    /// </summary>
    public class DbObject 
    {
        string datatypefile = Application.StartupPath + @"\datatype.ini";
        INIFile dtfile;

        #region  ����
        public string DbType
        {
            get { return "OleDb"; }
        }
        private string _dbconnectStr;
        public string DbConnectStr
        {
            set { _dbconnectStr = value; }
            get { return _dbconnectStr; }
        }
        OleDbConnection connect = new OleDbConnection();

        #endregion

        #region ���캯�������������Ϣ
        public DbObject()
        {
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
        public DbObject(bool SSPI, string server, string User, string Pass)
        {
            connect = new OleDbConnection();
            //			_dbconnectStr="Data Source="+server+"; user id="+User+";password="+Pass;	
            _dbconnectStr = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + server + ";Persist Security Info=False";
            connect.ConnectionString = _dbconnectStr;
        }


        #endregion

        #region ������OpenDB()
        /// <summary>
        /// �����ݿ�
        /// </summary>
        /// <param name="DbName">Ҫ�򿪵����ݿ�</param>
        /// <returns></returns>
        public void OpenDB()
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
                if (connect.State == System.Data.ConnectionState.Closed)
                {
                    connect.Open();
                }

            }
            catch//(System.Exception ex)
            {
                //string str=ex.Message;	
                //return null;
            }

        }
        #endregion

        #region ADO.NET ����

        public int ExecuteSql(string DbName, string SQLString)
        {
            OpenDB();
            OleDbCommand dbCommand = new OleDbCommand(SQLString, connect);
            dbCommand.CommandText = SQLString;
            int rows = dbCommand.ExecuteNonQuery();
            return rows;
        }

        public DataSet Query(string DbName, string SQLString)
        {
            DataSet ds = new DataSet();
            try
            {
                OpenDB();
                OleDbDataAdapter command = new OleDbDataAdapter(SQLString, connect);
                command.Fill(ds, "ds");
            }
            catch (System.Data.OleDb.OleDbException ex)
            {
                throw new Exception(ex.Message);
            }
            return ds;
        }

        public OleDbDataReader ExecuteReader(string strSQL)
        {
            try
            {
                OpenDB();
                OleDbCommand cmd = new OleDbCommand(strSQL, connect);
                OleDbDataReader myReader = cmd.ExecuteReader();
                return myReader;
            }
            catch (System.Data.OleDb.OleDbException ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public object GetSingle(string DbName, string SQLString)
        {
            try
            {
                OpenDB();
                OleDbCommand dbCommand = new OleDbCommand(SQLString, connect);
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
        #endregion

        
        #region �õ����ݿ�������б� GetDBList()
        /// <summary>
        /// �õ����ݿ�������б�
        /// </summary>
        /// <returns></returns>
        public List<string> GetDBList()
        {
            // TODO:  ��� DbObject.GetDBList ʵ��
            return null;
        }
        #endregion


        #region  �õ����ݿ�������û����� GetTables(string DbName)
        private DataTable Tab2Tab(DataTable sTable)
        {
            DataTable tTable = new DataTable();
            tTable.Columns.Add("name");
            tTable.Columns.Add("cuser");
            tTable.Columns.Add("type");
            tTable.Columns.Add("dates");
            foreach (DataRow row in sTable.Rows)
            {
                DataRow newRow = tTable.NewRow();
                newRow["name"] = row[2].ToString();
                newRow["cuser"] = "dbo";
                newRow["type"] = row[3].ToString();
                newRow["dates"] = row[6].ToString();
                tTable.Rows.Add(newRow);
            }
            return tTable;
        }


        /// <summary>
        /// �õ����ݿ�����б���
        /// </summary>
        /// <param name="DbName"></param>
        /// <returns></returns>
        public List<string> GetTables(string DbName)
        {
            OpenDB();
            DataTable schemaTable = connect.GetOleDbSchemaTable(OleDbSchemaGuid.Tables,
                new object[] { null, null, null, "TABLE" });           
            List<string> tabNames = new List<string>();            
            foreach (DataRow row in schemaTable.Rows)
            {
                tabNames.Add(row[2].ToString());
            }
            tabNames.Sort(CompareStrByOrder);
            return tabNames;
        }

        /// <summary>
        /// �����ַ�������
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

        public DataTable GetVIEWs(string DbName)
        {
            OpenDB();
            DataTable schemaTable = connect.GetOleDbSchemaTable(OleDbSchemaGuid.Tables,
                new object[] { null, null, null, "VIEW" });
            return Tab2Tab(schemaTable);
        }
        /// <summary>
        /// �õ����ݿ�����б����ͼ��
        /// </summary>
        /// <param name="DbName">���ݿ�</param>
        /// <returns></returns>
        public DataTable GetTabViews(string DbName)
        {
            OpenDB();
            DataTable schemaTable = connect.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
            return Tab2Tab(schemaTable);
        }
        /// <summary>
        /// �õ����ݿ�����д洢������
        /// </summary>
        /// <param name="DbName">���ݿ�</param>
        /// <returns></returns>
        public DataTable GetProcs(string DbName)
        {
            //string strSql = "select [name] from sysObjects where xtype='P'and [name]<>'dtproperties' order by [name]";//order by id
            //return Query(DbName, strSql).Tables[0];
            return null;
        }
        #endregion

        #region  �õ����ݿ�����б����ϸ��Ϣ GetTablesInfo(string DbName)

        /// <summary>
        /// �õ����ݿ�����б����ϸ��Ϣ
        /// </summary>
        /// <param name="DbName"></param>
        /// <returns></returns>
        public List<TableInfo> GetTablesInfo(string DbName)
        {
            OpenDB();
            DataTable schemaTable = connect.GetOleDbSchemaTable(OleDbSchemaGuid.Tables,
                new object[] { null, null, null, "TABLE" });
            List<TableInfo> tablist = new List<TableInfo>();
            TableInfo tab;
            foreach (DataRow row in schemaTable.Rows)
            {
                tab = new TableInfo();
                tab.TabName = row[2].ToString();
                tab.TabUser = "dbo";
                tab.TabType = row[3].ToString();
                tab.TabDate = row[6].ToString();
                tablist.Add(tab);
            }
            #region ����
            tablist.Sort(CompareTabByOrder);
            #endregion
            return tablist;
        }
        public List<TableInfo> GetVIEWsInfo(string DbName)
        {
            OpenDB();
            DataTable schemaTable = connect.GetOleDbSchemaTable(OleDbSchemaGuid.Tables,
                new object[] { null, null, null, "VIEW" });
            List<TableInfo> tablist = new List<TableInfo>();
            TableInfo tab;
            foreach (DataRow row in schemaTable.Rows)
            {
                tab = new TableInfo();
                tab.TabName = row[2].ToString();
                tab.TabUser = "dbo";
                tab.TabType = row[3].ToString();
                tab.TabDate = row[6].ToString();
                tablist.Add(tab);
            }
            #region ����
            tablist.Sort(CompareTabByOrder);
            #endregion
            return tablist;
        }

        /// <summary>
        /// �õ����ݿ�����б����ͼ����ϸ��Ϣ
        /// </summary>
        /// <param name="DbName">���ݿ�</param>
        /// <returns></returns>
        public List<TableInfo> GetTabViewsInfo(string DbName)
        {
            OpenDB();
            DataTable schemaTable = connect.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);            
            List<TableInfo> tablist = new List<TableInfo>();
            TableInfo tab;
            foreach (DataRow row in schemaTable.Rows)
            {
                tab = new TableInfo();
                tab.TabName = row[2].ToString();
                tab.TabUser = "dbo";
                tab.TabType = row[3].ToString();
                tab.TabDate = row[6].ToString();
                tablist.Add(tab);
            }
            
            #region ����
            tablist.Sort(CompareTabByOrder);
            #endregion

            return tablist;
        }

        /// <summary>
        /// �õ����ݿ�����д洢���̵���ϸ��Ϣ
        /// </summary>
        /// <param name="DbName">���ݿ�</param>
        /// <returns></returns>
        public List<TableInfo> GetProcInfo(string DbName)
        {
            //StringBuilder strSql = new StringBuilder();
            //strSql.Append("select sysObjects.[name] name,sysusers.name cuser,");
            //strSql.Append("sysObjects.xtype type,sysObjects.crdate dates ");
            //strSql.Append("from sysObjects,sysusers ");
            //strSql.Append("where sysusers.uid=sysObjects.uid ");
            //strSql.Append("and sysObjects.xtype='P' ");
            ////strSql.Append("and  sysObjects.[name]<>'dtproperties' ");
            //strSql.Append("order by sysObjects.id");//order by id			
            //return Query(DbName, strSql.ToString()).Tables[0];
            return null;
        }

        /// <summary>
        /// �����ַ�������
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>0������ȣ�-1����y����x��1����x����y</returns>
        private int CompareTabByOrder(TableInfo x, TableInfo y)
        {
            if (x == null)
            {
                if (y == null)
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
                if (y == null)  // ...and y is null, x is greater.
                {
                    return 1;
                }
                else
                {
                    int retval = x.TabName.CompareTo(y.TabName);
                    return retval;                   
                }
            }
        }

        #endregion

        #region �õ����������
        public string GetObjectInfo(string DbName, string objName)
        {
            return null;
        }
        #endregion

        #region �õ�(����)���ݿ��������������� GetColumnList(string DbName,string TableName)
        
        /// <summary>
        /// �õ��ֶ������ַ���
        /// </summary>
        /// <param name="typeNum"></param>
        /// <returns></returns>
        private string GetColumnType(string typeNum)
        {
            string type = typeNum;
            if (File.Exists(datatypefile))
            {
                dtfile = new INIFile(datatypefile);
                type = dtfile.IniReadValue("AccessDbTypeMap", typeNum);                
            }                       
            //switch (typeNum)
            //{
            //    case "3":
            //        type = "int";
            //        break;
            //    case "5":
            //        type = "float";
            //        break;
            //    case "6":
            //        type = "money";
            //        break;
            //    case "7":
            //        type = "datetime";
            //        break;
            //    case "11":
            //        type = "bool";
            //        break;
            //    case "130":
            //        type = "varchar";
            //        break;
            //}
            return type;
        }

        private DataTable Tab2Colum(DataTable sTable)
        {
            DataTable tTable = new DataTable();
            tTable.Columns.Add("colorder");
            tTable.Columns.Add("ColumnName");
            tTable.Columns.Add("TypeName");
            tTable.Columns.Add("Length");
            tTable.Columns.Add("Preci");
            tTable.Columns.Add("Scale");
            tTable.Columns.Add("IsIdentity");
            tTable.Columns.Add("isPK");
            tTable.Columns.Add("cisNull");
            tTable.Columns.Add("defaultVal");
            tTable.Columns.Add("deText");

            int n = 0;
            foreach (DataRow row in sTable.Select("", "ORDINAL_POSITION asc"))
            {
                DataRow newRow = tTable.NewRow();
                newRow["colorder"] = row[6].ToString();
                newRow["ColumnName"] = row[3].ToString();
                string type = GetColumnType(row[11].ToString());
                
                newRow["TypeName"] = type;
                newRow["Length"] = row[13].ToString();
                newRow["Preci"] = row[15].ToString();
                newRow["Scale"] = row[16].ToString();
                newRow["IsIdentity"] = "";
                newRow["isPK"] = "";
                if (row[10].ToString().ToLower() == "true")
                {
                    newRow["cisNull"] = "";
                }
                else
                {
                    newRow["cisNull"] = "��";
                }
                newRow["defaultVal"] = row[8].ToString();
                newRow["deText"] = "";
                tTable.Rows.Add(newRow);
                n++;
            }

            return tTable;
        }


        /// <summary>
        /// �õ����ݿ���������������
        /// </summary>
        /// <param name="DbName"></param>
        /// <param name="TableName"></param>
        /// <returns></returns>
        public List<ColumnInfo> GetColumnList(string DbName, string TableName)
        {
            return GetColumnInfoList(DbName, TableName);
        }

        #endregion

        #region �õ����ݿ������е���ϸ��Ϣ GetColumnInfoList(string DbName,string TableName)

        /// <summary>
        /// �õ����ݿ������е���ϸ��Ϣ
        /// </summary>
        /// <param name="DbName"></param>
        /// <param name="TableName"></param>
        /// <returns></returns>
        public List<ColumnInfo> GetColumnInfoList(string DbName, string TableName)
        {
            OpenDB();
            DataTable schemaTable = connect.GetOleDbSchemaTable(OleDbSchemaGuid.Columns,
                new Object[] { null, null, TableName, null });

            List<ColumnInfo> collist = new List<ColumnInfo>();
            ColumnInfo col;
            foreach (DataRow row in schemaTable.Rows)
            {
                col = new ColumnInfo();
                col.Colorder = row[6].ToString();
                col.ColumnName = row[3].ToString();
                string type = GetColumnType(row[11].ToString());
                
                col.TypeName = type;
                col.Length = row[13].ToString();
                col.Preci = row[15].ToString();
                col.Scale = row[16].ToString();
                if (row[10].ToString().ToLower() == "true")
                {
                    col.cisNull = false;
                }
                else
                {
                    col.cisNull = true;
                }
                col.DefaultVal = row[8].ToString();
                col.DeText = "";
                col.IsPK = false;
                col.IsIdentity = false;
                collist.Add(col);

            }

            #region ����

            collist.Sort(CompareDinosByintOrder);
            #endregion

            return collist;
        }

        /// <summary>
        /// �����ַ���תint����
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>0������ȣ�-1����y����x��1����x����y</returns>
        private int CompareDinosByintOrder(ColumnInfo x, ColumnInfo y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    // If x is null and y is null, they're equal. 
                    return 0;
                }
                else
                {
                    // If x is null and y is not null, y is greater. 
                    return -1;
                }
            }
            else
            {
                if (y == null)  // ...and y is null, x is greater.
                {
                    return 1;
                }
                else
                {
                    // ...and y is not null, compare the lengths of the two strings.
                    //int retval = x.Colorder.CompareTo(y.Colorder);
                    //return retval;
                    int n = 0;
                    int m = 0;
                    try
                    {
                        n = Convert.ToInt32(x.Colorder);
                    }
                    catch
                    {
                        return -1;
                    }
                    try
                    {
                        m = Convert.ToInt32(y.Colorder);
                    }
                    catch
                    {
                        return 1;
                    }

                    if (n < m)
                    {
                        return -1;
                    }
                    else
                        if (x == y)
                        {
                            return 0;
                        }
                        else
                        {
                            return 1;
                        }

                }
            }
        }
        
        /// <summary>
        /// �����ַ�������
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>0������ȣ�-1����y����x��1����x����y</returns>
        private int CompareDinosByOrder(ColumnInfo x, ColumnInfo y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    // If x is null and y is null, they're equal. 
                    return 0;
                }
                else
                {
                    // If x is null and y is not null, y is greater. 
                    return -1;
                }
            }
            else
            {
                if (y == null)  // ...and y is null, x is greater.
                {
                    return 1;
                }
                else
                {
                    // ...and y is not null, compare the lengths of the two strings.     

                    int retval = x.Colorder.CompareTo(y.Colorder);
                    return retval;

                    //if (retval != 0)
                    //{
                    //    // If the strings are not of equal length,
                    //    // the longer string is greater.
                    //    return retval;
                    //}
                    //else
                    //{
                    //    // If the strings are of equal length,
                    //    // sort them with ordinary string comparison.
                    //    return x.CompareTo(y);
                    //}
                }
            }
        }


        public DataTable GetColumnInfoListSP(string DbName, string ViewName)
        {
            //StringBuilder strSql = new StringBuilder();
            //strSql.Append("sp_columns " + ViewName);
            //strSql.Append("DROP TABLE t1");
            //strSql.Append("create table t1(TABLE_QUALIFIER sql_variant,TABLE_OWNER sql_variant,TABLE_NAME sql_variant,COLUMN_NAME sql_variant,DATA_TYPE sql_variant,TYPE_NAME sql_variant,[PRECISION] INT,LENGTH INT,SCALE sql_variant,RADIX sql_variant,NULLABLE sql_variant,REMARKS sql_variant,COLUMN_DEF sql_variant,SQL_DATA_TYPE sql_variant,SQL_DATETIME_SUB sql_variant,CHAR_OCTET_LENGTH sql_variant,ORDINAL_POSITION sql_variant,IS_NULLABLE sql_variant,SS_DATA_TYPE sql_variant)");
            //strSql.Append("insert into t1(TABLE_QUALIFIER,TABLE_OWNER,TABLE_NAME,COLUMN_NAME,DATA_TYPE,TYPE_NAME,[PRECISION],LENGTH,SCALE,RADIX,NULLABLE,REMARKS,COLUMN_DEF,SQL_DATA_TYPE,SQL_DATETIME_SUB,CHAR_OCTET_LENGTH,ORDINAL_POSITION,IS_NULLABLE,SS_DATA_TYPE )");
            //strSql.Append("exec sp_columns " + ViewName);
            //strSql.Append("select ORDINAL_POSITION ,COLUMN_NAME ,TYPE_NAME ,LENGTH ,[PRECISION],SCALE ,NULLABLE ,COLUMN_DEF from t1");
            //return Query(DbName, strSql.ToString()).Tables[0];
            return null;
        }

        #endregion


        #region �õ����ݿ��������� GetKeyName(string DbName,string TableName)

        private DataTable Key2Colum(DataTable sTable, string TableName)
        {
            DataTable tTable = new DataTable();
            tTable.Columns.Add("colorder");
            tTable.Columns.Add("ColumnName");
            tTable.Columns.Add("TypeName");
            tTable.Columns.Add("Length");
            tTable.Columns.Add("Preci");
            tTable.Columns.Add("Scale");
            tTable.Columns.Add("IsIdentity");
            tTable.Columns.Add("isPK");
            tTable.Columns.Add("cisNull");
            tTable.Columns.Add("defaultVal");
            tTable.Columns.Add("deText");

            int n = 0;
            foreach (DataRow row in sTable.Rows)
            {
                string strtab = row[5].ToString();
                if (strtab != TableName)
                {
                    continue;
                }
                string strkey = row[2].ToString();
                if (strkey != "PrimaryKey")
                {
                    continue;
                }
                string colname = row[6].ToString();

                DataRow newRow = tTable.NewRow();
                newRow["colorder"] = row[9].ToString();

                List<ColumnInfo> collist = GetColumnList(null, TableName);
                DataTable dt = CodeHelper.CodeCommon.GetColumnInfoDt(collist);

                foreach (DataRow rowk in dt.Select("ColumnName='" + colname + "'"))
                {
                    newRow["ColumnName"] = colname;
                    newRow["TypeName"] = rowk["TypeName"];
                    newRow["Length"] = rowk["Length"];
                    newRow["Preci"] = rowk["Preci"];
                    newRow["Scale"] = rowk["Scale"];
                    newRow["IsIdentity"] = rowk["IsIdentity"];
                    newRow["isPK"] = rowk["isPK"];
                    newRow["cisNull"] = rowk["cisNull"];
                    newRow["defaultVal"] = rowk["defaultVal"];
                    newRow["deText"] = rowk["deText"];
                }
                tTable.Rows.Add(newRow);
                n++;
            }
            return tTable;
        }
        public DataTable GetKeyName(string DbName, string TableName)
        {
            try
            {
                OpenDB();
                DataTable schemaTable = connect.GetOleDbSchemaTable(OleDbSchemaGuid.Key_Column_Usage,
                    new Object[] { null, null, null, null });
                return Key2Colum(schemaTable, TableName);

            }
            catch (System.Exception ex)
            {
                string str = ex.Message;
                return null;
            }
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
            strSql.Append("select * from [" + TableName + "]");
            return Query("", strSql.ToString()).Tables[0];
        }
        /// <summary>
        /// ����SQL��ѯ�õ����ݱ��������
        /// </summary>
        /// <param name="DbName"></param>
        /// <param name="TableName"></param>
        /// <returns></returns>
        public DataTable GetTabDataBySQL(string DbName, string strSQL)
        {
            return Query("", strSQL).Tables[0];
        }
        #endregion

        #region ���ݿ����Բ���

        public bool RenameTable(string DbName, string OldName, string NewName)
        {
            // TODO:  ��� DbObject.RenameTable ʵ��
            return false;
        }

        public bool DeleteTable(string DbName, string TableName)
        {
            try
            {
                string strsql = "DROP TABLE " + TableName + "";
                ExecuteSql(DbName, TableName);
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
            return "";
        }
        #endregion


    }
}

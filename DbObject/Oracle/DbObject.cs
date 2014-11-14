using System;
using System.Data;
using System.Text;
using System.Collections.Generic;
using CodeHelper;
using System.Data.OracleClient;
namespace DbObjects.Oracle
{
	/// <summary>
	/// DbObject ��ժҪ˵����
	/// </summary>
	public class DbObject
	{		
		
		#region  ����		
        public string DbType
        {
            get { return "Oracle"; }
        }
		private string _dbconnectStr;	
		public string DbConnectStr
		{
			set{_dbconnectStr=value;}
			get{return _dbconnectStr;}
		}
		OracleConnection connect = new OracleConnection();
		
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
			_dbconnectStr=DbConnectStr;
			connect.ConnectionString=DbConnectStr;
		}		
		/// <summary>
		/// ����һ�������ַ���
		/// </summary>
		/// <param name="SSPI">�Ƿ�windows������֤</param>
		/// <param name="Ip">������IP</param>
		/// <param name="User">�û���</param>
		/// <param name="Pass">����</param>
		public DbObject(bool SSPI,string server,string User,string Pass)
		{		
			connect = new OracleConnection();							
			_dbconnectStr="Data Source="+server+"; user id="+User+";password="+Pass;			
			connect.ConnectionString=_dbconnectStr;
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
				if(connect.ConnectionString=="")
				{
					connect.ConnectionString=_dbconnectStr;
				}
				if(connect.ConnectionString!=_dbconnectStr)
				{
					connect.Close();
					connect.ConnectionString=_dbconnectStr;
				}
				if(connect.State==System.Data.ConnectionState.Closed)
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

		public int ExecuteSql(string DbName,string SQLString)
		{
			OpenDB();
			OracleCommand dbCommand = new OracleCommand(SQLString,connect);
			dbCommand.CommandText=SQLString;
			int rows=dbCommand.ExecuteNonQuery();
			return rows;
		}
		
		public DataSet Query(string DbName,string SQLString)
		{			
			DataSet ds = new DataSet();
			try
			{		
				OpenDB();
				OracleDataAdapter command = new OracleDataAdapter(SQLString,connect);				
				command.Fill(ds,"ds");
			}
			catch(System.Data.OracleClient.OracleException ex)
			{				
				throw new Exception(ex.Message);
			}			
			return ds;				
		}	

		public OracleDataReader ExecuteReader(string strSQL)
		{				
			try
			{
				OpenDB();
				OracleCommand cmd = new OracleCommand(strSQL,connect);
				OracleDataReader myReader = cmd.ExecuteReader();
				return myReader;
			}
			catch(System.Data.OracleClient.OracleException ex)
			{								
				throw new Exception(ex.Message);
			}			
		}
        public object GetSingle(string DbName, string SQLString)
        {
            try
            {
                OpenDB();
                OracleCommand dbCommand = new OracleCommand(SQLString, connect);
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

		#region  �õ����ݿ�� �����û� ���� GetTables(string DbName)
		/// <summary>
		/// �õ����ݿ�����б���
		/// </summary>
		/// <param name="DbName"></param>
		/// <returns></returns>
        public List<string> GetTables(string DbName)
		{
			string strSql="select TNAME name from tab where TABTYPE='TABLE'";
            List<string> tabNames = new List<string>();
            OracleDataReader reader = ExecuteReader(strSql);
            while (reader.Read())
            {
                tabNames.Add(reader.GetString(0));
            }
            reader.Close();
            return tabNames;
		}
		public DataTable GetVIEWs(string DbName)
		{
			string strSql="select TNAME name from tab where TABTYPE='VIEW'";
			return Query("",strSql).Tables[0];
		}
		/// <summary>
		/// �õ����ݿ�����б����ͼ��
		/// </summary>
		/// <param name="DbName">���ݿ�</param>
		/// <returns></returns>
		public DataTable GetTabViews(string DbName)
		{
			string strSql="select TNAME name,TABTYPE type from tab ";
			return Query("",strSql).Tables[0];
		}
        /// <summary>
        /// �õ����ݿ�����д洢������
        /// </summary>
        /// <param name="DbName">���ݿ�</param>
        /// <returns></returns>
        public DataTable GetProcs(string DbName)
        {
            string strSql = "SELECT * FROM ALL_SOURCE  where TYPE='PROCEDURE'  ";
            return Query(DbName, strSql).Tables[0];
            //return null;
        }

		#endregion       

        #region  �õ����ݿ������ �����ϸ��Ϣ GetTablesInfo(string DbName)

        /// <summary>
		/// �õ����ݿ�����б����ϸ��Ϣ
		/// </summary>
		/// <param name="DbName"></param>
		/// <returns></returns>
        public List<TableInfo> GetTablesInfo(string DbName)
		{
			string strSql="select TNAME name,'dbo' cuser,TABTYPE type,'' dates from tab where TABTYPE='TABLE'";
            //return Query("",strSql).Tables[0];

            List<TableInfo> tablist = new List<TableInfo>();
            TableInfo tab;
            OracleDataReader reader = ExecuteReader(strSql);
            while (reader.Read())
            {
                tab = new TableInfo();
                tab.TabName = reader.GetString(0);
                tab.TabDate = reader.GetValue(3).ToString();
                tab.TabType = reader.GetString(2);
                tab.TabUser = reader.GetString(1);
                tablist.Add(tab);
            }
            reader.Close();
            return tablist;
		}
        public List<TableInfo> GetVIEWsInfo(string DbName)
		{
			string strSql="select TNAME name,'dbo' cuser,TABTYPE type,'' dates from tab where TABTYPE='VIEW'";
            //return Query("",strSql).Tables[0];

            List<TableInfo> tablist = new List<TableInfo>();
            TableInfo tab;
            OracleDataReader reader = ExecuteReader(strSql);
            while (reader.Read())
            {
                tab = new TableInfo();
                tab.TabName = reader.GetString(0);
                tab.TabDate = reader.GetValue(3).ToString();
                tab.TabType = reader.GetString(2);
                tab.TabUser = reader.GetString(1);
                tablist.Add(tab);
            }
            reader.Close();
            return tablist;
		}
		/// <summary>
		/// �õ����ݿ�����б����ͼ����ϸ��Ϣ
		/// </summary>
		/// <param name="DbName">���ݿ�</param>
		/// <returns></returns>
        public List<TableInfo> GetTabViewsInfo(string DbName)
		{
			string strSql="select TNAME name,'dbo' cuser,TABTYPE type,'' dates from tab ";
            //return Query("",strSql).Tables[0];

            List<TableInfo> tablist = new List<TableInfo>();
            TableInfo tab;
            OracleDataReader reader = ExecuteReader(strSql);
            while (reader.Read())
            {
                tab = new TableInfo();
                tab.TabName = reader.GetString(0);
                tab.TabDate = reader.GetValue(3).ToString();
                tab.TabType = reader.GetString(2);
                tab.TabUser = reader.GetString(1);
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
            string strSql = "SELECT * FROM ALL_SOURCE  where TYPE='PROCEDURE'  ";

            //return Query(DbName, strSql).Tables[0];
           
            List<TableInfo> tablist = new List<TableInfo>();
            TableInfo tab;
            OracleDataReader reader = ExecuteReader(strSql);
            while (reader.Read())
            {
                tab = new TableInfo();
                tab.TabName = reader.GetString(0);
                tab.TabDate = reader.GetValue(3).ToString();
                tab.TabType = reader.GetString(2);
                tab.TabUser = reader.GetString(1);
                tablist.Add(tab);
            }
            reader.Close();
            return tablist;

            //return null;
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
            //string strSql = "select DBMS_METADATA.GET_DDL('PROCEDURE',u.object_name) from user_objects u where object_type = 'PROCEDURE'";
            //string strSql = String.Format("select dbms_metadata.get_ddl('PROCEDURE','{0}','{1}') from dual;", objName,);            
            //return GetSingle(DbName, strSql.ToString()).ToString();
            return null;
        }
        #endregion

		#region �õ�(����)���ݿ��������������� GetColumnList(string DbName,string TableName)

		/// <summary>
		/// �õ����ݿ���������������
		/// </summary>
		/// <param name="DbName"></param>
		/// <param name="TableName"></param>
		/// <returns></returns>
        public List<ColumnInfo> GetColumnList(string DbName, string TableName)
		{
			StringBuilder strSql=new StringBuilder();
			strSql.Append("select ");
            strSql.Append("COLUMN_ID as colorder,");
			strSql.Append("COLUMN_NAME as ColumnName,");
			strSql.Append("DATA_TYPE as TypeName ");		
			strSql.Append(" from USER_TAB_COLUMNS ");
			strSql.Append(" where TABLE_NAME='"+TableName+"'");
			strSql.Append(" order by COLUMN_ID");					
            //return Query("",strSql.ToString()).Tables[0];

            List<ColumnInfo> collist = new List<ColumnInfo>();
            ColumnInfo col;
            OracleDataReader reader = ExecuteReader(strSql.ToString());
            while (reader.Read())
            {
                col = new ColumnInfo();
                col.Colorder = reader.GetValue(0).ToString();
                col.ColumnName = reader.GetString(1);
                col.TypeName = reader.GetString(2);
                col.Length = "";
                col.Preci = "";
                col.Scale = "";
                col.IsPK = false;
                col.cisNull = false;
                col.DefaultVal = "";
                col.IsIdentity = false                    ;
                col.DeText = "";
                collist.Add(col);
            }
            reader.Close();
            return collist;
		}

		#endregion

		#region �õ����ݿ����� �е���ϸ��Ϣ GetColumnInfoList(string DbName,string TableName)

		/// <summary>
		/// �õ����ݿ������е���ϸ��Ϣ
		/// </summary>
		/// <param name="DbName"></param>
		/// <param name="TableName"></param>
		/// <returns></returns>
        public List<ColumnInfo> GetColumnInfoList(string DbName, string TableName)
		{
            try
            {
                StringBuilder strSql = new StringBuilder();
                strSql.Append("select ");
                strSql.Append("COL.COLUMN_ID as colorder,");
                strSql.Append("COL.COLUMN_NAME as ColumnName,");
                strSql.Append("COL.DATA_TYPE as TypeName, ");
                //strSql.Append("COL.DATA_LENGTH as Length,");
                strSql.Append("DECODE(COL.DATA_TYPE, 'NUMBER',COL.DATA_PRECISION, COL.DATA_LENGTH) as Length,");
                strSql.Append("COL.DATA_PRECISION as Preci,");
                strSql.Append("COL.DATA_SCALE as Scale,");
                strSql.Append("'' as IsIdentity,");
                //strSql.Append("PKCOL.COLUMN_POSITION as isPK,");//�������������ж���������ͻ���1,2,3������������˳����ȥ
                strSql.Append("case when PKCOL.COLUMN_POSITION >0  then '��'else '' end as isPK,");
                //strSql.Append("COL.NULLABLE as cisNull,");
                strSql.Append("case when COL.NULLABLE='Y'  then '��'else '' end as cisNull, ");
                strSql.Append("COL.DATA_DEFAULT as defaultVal, ");
                strSql.Append("CCOM.COMMENTS as deText,");
                strSql.Append("COL.NUM_DISTINCT as NUM_DISTINCT ");
                strSql.Append(" FROM USER_TAB_COLUMNS COL,USER_COL_COMMENTS CCOM, ");
                strSql.Append(" ( SELECT AA.TABLE_NAME, AA.INDEX_NAME, AA.COLUMN_NAME, AA.COLUMN_POSITION");
                strSql.Append(" FROM USER_IND_COLUMNS AA, USER_CONSTRAINTS BB");
                strSql.Append(" WHERE BB.CONSTRAINT_TYPE = 'P'");
                strSql.Append(" AND AA.TABLE_NAME = BB.TABLE_NAME");
                strSql.Append(" AND AA.INDEX_NAME = BB.CONSTRAINT_NAME");
                strSql.Append(" AND AA.TABLE_NAME IN ('" + TableName + "')");
                strSql.Append(") PKCOL");
                strSql.Append(" WHERE COL.TABLE_NAME = CCOM.TABLE_NAME");
                strSql.Append(" AND COL.COLUMN_NAME = CCOM.COLUMN_NAME");
                strSql.Append(" AND COL.TABLE_NAME ='" + TableName + "'");
                strSql.Append(" AND COL.COLUMN_NAME = PKCOL.COLUMN_NAME(+)");
                strSql.Append(" AND COL.TABLE_NAME = PKCOL.TABLE_NAME(+)");
                strSql.Append(" ORDER BY COL.COLUMN_ID ");

                //return Query("",strSql.ToString()).Tables[0];

                List<ColumnInfo> collist = new List<ColumnInfo>();
                ColumnInfo col;
                OracleDataReader reader = ExecuteReader(strSql.ToString());
                while (reader.Read())
                {
                    col = new ColumnInfo();
                    col.Colorder = reader.GetValue(0).ToString();
                    col.ColumnName = reader.GetValue(1).ToString();
                    col.TypeName = reader.GetValue(2).ToString();
                    col.Length = reader.GetValue(3).ToString();
                    col.Preci = reader.GetValue(4).ToString();
                    col.Scale = reader.GetValue(5).ToString();
                    col.IsIdentity = (reader.GetValue(6).ToString() == "��") ? true : false;
                    col.IsPK = (reader.GetValue(7).ToString() == "��") ? true : false;
                    col.cisNull = (reader.GetValue(8).ToString() == "��") ? true : false;
                    col.DefaultVal = reader.GetValue(9).ToString();
                    col.DeText = reader.GetValue(10).ToString();
                    collist.Add(col);
                }
                reader.Close();
                return collist;

            }
            catch (System.Exception ex)
            {
                throw new Exception("��ȡ������ʧ��" + ex.Message);
            }

            #region
            //SELECT COL.TABLE_NAME as TNAME, 
            //TCOM.COMMENTS as TCMT,
            //COL.COLUMN_NAME as COL_NM, 
            //CCOM.COMMENTS as COL_CMT,
            //COL.COLUMN_ID ID,
            //PKCOL.COLUMN_POSITION AS PK,  --�������������ж���������ͻ���1,2,3������������˳����ȥ
            //COL.DATA_TYPE as TYPE_CD, 
            //DECODE(COL.DATA_TYPE, 'NUMBER',COL.DATA_PRECISION ||'.'||COL.DATA_SCALE, COL.DATA_LENGTH) as LENGTH,
            //COL.NULLABLE as NULL_YN,
            //COL.DATA_DEFAULT as D_DEFAULT,
            //COL.NUM_DISTINCT as NUM_DISTINCT
            //FROM USER_TAB_COLUMNS COL,
            //USER_TAB_COMMENTS TCOM,
            //USER_COL_COMMENTS CCOM,
            //( SELECT AA.TABLE_NAME, AA.INDEX_NAME, AA.COLUMN_NAME, AA.COLUMN_POSITION
            //FROM USER_IND_COLUMNS AA, USER_CONSTRAINTS BB
            //WHERE BB.CONSTRAINT_TYPE = 'P'
            //AND AA.TABLE_NAME = BB.TABLE_NAME
            //AND AA.INDEX_NAME = BB.CONSTRAINT_NAME
            //-- AND AA.TABLE_NAME IN ('ACCOUNTMONEY') 
            //) PKCOL
            //WHERE COL.TABLE_NAME = TCOM.TABLE_NAME 
            //AND COL.TABLE_NAME = CCOM.TABLE_NAME 
            //AND COL.COLUMN_NAME = CCOM.COLUMN_NAME
            //AND COL.TABLE_NAME = 'ACCOUNTMONEY' 
            //AND COL.COLUMN_NAME = PKCOL.COLUMN_NAME(+)
            //AND COL.TABLE_NAME = PKCOL.TABLE_NAME(+)
            //ORDER BY COL.TABLE_NAME ,
            //COL.COLUMN_ID 
            #endregion

        }

		#endregion

        
		#region �õ����ݿ��������� GetKeyName(string DbName,string TableName)

		public DataTable GetKeyName(string DbName, string TableName)
		{
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select ");
            strSql.Append("COL.COLUMN_ID as colorder,");
            strSql.Append("COL.COLUMN_NAME as ColumnName,");
            strSql.Append("COL.DATA_TYPE as TypeName, ");
            strSql.Append("COL.DATA_LENGTH as Length,");
            strSql.Append("COL.DATA_PRECISION as Preci,");
            strSql.Append("COL.DATA_SCALE as Scale,");
            strSql.Append("'' as IsIdentity,");
            //strSql.Append("PKCOL.COLUMN_POSITION as isPK,");//�������������ж���������ͻ���1,2,3������������˳����ȥ
            strSql.Append("case when PKCOL.COLUMN_POSITION >0  then '��'else '' end as isPK,");

            //strSql.Append("COL.NULLABLE as cisNull,");
            strSql.Append("case when COL.NULLABLE='Y'  then '��'else '' end as cisNull, ");

            strSql.Append("COL.DATA_DEFAULT as defaultVal, ");
            strSql.Append("CCOM.COMMENTS as deText,");
            strSql.Append("COL.NUM_DISTINCT as NUM_DISTINCT ");
            strSql.Append(" FROM USER_TAB_COLUMNS COL,USER_COL_COMMENTS CCOM, ");
            strSql.Append(" ( SELECT AA.TABLE_NAME, AA.INDEX_NAME, AA.COLUMN_NAME, AA.COLUMN_POSITION");
            strSql.Append(" FROM USER_IND_COLUMNS AA, USER_CONSTRAINTS BB");
            strSql.Append(" WHERE BB.CONSTRAINT_TYPE = 'P'");
            strSql.Append(" AND AA.TABLE_NAME = BB.TABLE_NAME");
            strSql.Append(" AND AA.INDEX_NAME = BB.CONSTRAINT_NAME");
            strSql.Append(" AND AA.TABLE_NAME IN ('" + TableName + "')");
            strSql.Append(") PKCOL");
            strSql.Append(" WHERE COL.TABLE_NAME = CCOM.TABLE_NAME");
            strSql.Append(" AND PKCOL.COLUMN_POSITION >0");
            strSql.Append(" AND COL.COLUMN_NAME = CCOM.COLUMN_NAME");
            strSql.Append(" AND COL.TABLE_NAME ='" + TableName + "'");
            strSql.Append(" AND COL.COLUMN_NAME = PKCOL.COLUMN_NAME(+)");
            strSql.Append(" AND COL.TABLE_NAME = PKCOL.TABLE_NAME(+)");
            strSql.Append(" ORDER BY COL.COLUMN_ID ");	

            return Query("", strSql.ToString()).Tables[0];
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
			StringBuilder strSql=new StringBuilder();
            strSql.Append("select * from " + TableName + "");
			return Query("",strSql.ToString()).Tables[0];
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
            try
            {
                string strsql = "RENAME " + OldName + " TO " + NewName + "";
                ExecuteSql(DbName, strsql);
                return true;
            }
            catch//(System.Exception ex)
            {
                //string str=ex.Message;	
                return false;
            }		
		}

		public bool DeleteTable(string DbName, string TableName)
		{
			try
			{				
				string strsql="DROP TABLE "+TableName+"";
                ExecuteSql(DbName, strsql);
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

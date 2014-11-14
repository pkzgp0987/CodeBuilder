using System;
using System.Data;
using System.Text;
using System.Data.Odbc;
using LTP.IDBO;
namespace LTP.DbObjects.Odbc
{
	/// <summary>
	/// DbObject ��ժҪ˵����
	/// </summary>
	public class DbObject:IDbObject
	{		
		
		#region  ����	
        public string DbType
        {
            get { return "Odbc"; }
        }
		private string _dbconnectStr;	
		public string DbConnectStr
		{
			set{_dbconnectStr=value;}
			get{return _dbconnectStr;}
		}
        
		OdbcConnection connect = new OdbcConnection();
		
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
			connect = new OdbcConnection();

            //DRIVER={Sybase System 11};SRVR=jyyd;Uid=sa;Pwd=;database=jyyd
            _dbconnectStr = "DRIVER={Sybase System 11};SRVR=" + server + ";Uid=" + User + ";Pwd=" + Pass + ";database=jyyd";	
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
			OdbcCommand dbCommand = new OdbcCommand(SQLString,connect);
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
				OdbcDataAdapter command = new OdbcDataAdapter(SQLString,connect);				
				command.Fill(ds,"ds");
			}
			catch(System.Data.Odbc.OdbcException ex)
			{				
				throw new Exception(ex.Message);
			}			
			return ds;				
		}	

		public OdbcDataReader ExecuteReader(string strSQL)
		{				
			try
			{
				OpenDB();
				OdbcCommand cmd = new OdbcCommand(strSQL,connect);
				OdbcDataReader myReader = cmd.ExecuteReader();
				return myReader;
			}
			catch(System.Data.Odbc.OdbcException ex)
			{								
				throw new Exception(ex.Message);
			}			
		}
        public object GetSingle(string DbName, string SQLString)
        {
            try
            {
                OpenDB();
                OdbcCommand dbCommand = new OdbcCommand(SQLString, connect);
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
		public DataTable GetDBList()
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

			foreach(DataRow row in sTable.Rows)
			{
				DataRow newRow = tTable.NewRow();
				newRow["name"]=row[2].ToString();
				newRow["cuser"]="dbo";
				newRow["type"]=row[3].ToString();
				newRow["dates"]=row[6].ToString();
				tTable.Rows.Add(newRow);
			}			
			return tTable;					
		}
		

		/// <summary>
		/// �õ����ݿ�����б���
		/// </summary>
		/// <param name="DbName"></param>
		/// <returns></returns>
		public DataTable GetTables(string DbName)
		{
			OpenDB();
			DataTable schemaTable = connect.GetSchema(OdbcSchemaGuid.Tables,
				new object[] {null, null, null, "TABLE"});			
			return Tab2Tab(schemaTable);
		}
		public DataTable GetVIEWs(string DbName)
		{
			OpenDB();
            DataTable schemaTable = connect.GetSchema(OdbcSchemaGuid.Tables,
				new object[] {null, null, null, "VIEW"});			
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
            DataTable schemaTable = connect.GetSchema(OdbcSchemaGuid.Tables, null);	
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
		public DataTable GetTablesInfo(string DbName)
		{
			OpenDB();
            DataTable schemaTable = connect.GetSchema(OleDbSchemaGuid.Tables,
				new object[] {null, null, null, "TABLE"});
			//connect.Close();
			return Tab2Tab(schemaTable);
		}
		public DataTable GetVIEWsInfo(string DbName)
		{
			OpenDB();
            DataTable schemaTable = connect.GetSchema(OleDbSchemaGuid.Tables,
				new object[] {null, null, null, "VIEW"});			
			return Tab2Tab(schemaTable);
		}
		/// <summary>
		/// �õ����ݿ�����б����ͼ����ϸ��Ϣ
		/// </summary>
		/// <param name="DbName">���ݿ�</param>
		/// <returns></returns>
		public DataTable GetTabViewsInfo(string DbName)
		{
			OpenDB();
            DataTable schemaTable = connect.GetSchema(OleDbSchemaGuid.Tables, null);		
			return Tab2Tab(schemaTable);
		}
        /// <summary>
        /// �õ����ݿ�����д洢���̵���ϸ��Ϣ
        /// </summary>
        /// <param name="DbName">���ݿ�</param>
        /// <returns></returns>
        public DataTable GetProcInfo(string DbName)
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
		#endregion

        #region �õ����������      
        public string GetObjectInfo(string DbName, string objName)
        {
            return null;
        }
        #endregion

		#region �õ�(����)���ݿ��������������� GetColumnList(string DbName,string TableName)

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

			int n=0;
			foreach(DataRow row in sTable.Select("","ORDINAL_POSITION asc"))
			{
				DataRow newRow = tTable.NewRow();
				newRow["colorder"]=row[6].ToString();
				newRow["ColumnName"]=row[3].ToString();
				string type=row[11].ToString();
				switch(row[11].ToString())
				{
					case "3":
						type="int";
						break;
					case "5":
						type="float";
						break;
					case "6":
						type="money";
						break;
					case "7":
						type="datetime";
						break;
					case "11":
						type="bool";
						break;
					case "130":
						type="varchar";
						break;
				}
				newRow["TypeName"]=type;
				newRow["Length"]=row[13].ToString();
				newRow["Preci"]=row[15].ToString();
				newRow["Scale"]=row[16].ToString();
				newRow["IsIdentity"]="";
				newRow["isPK"]="";
				if(row[10].ToString().ToLower()=="true")
				{
					newRow["cisNull"]="";
				}
				else
				{
					newRow["cisNull"]="��";
				}
				newRow["defaultVal"]=row[8].ToString();
				newRow["deText"]="";		
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
		public DataTable GetColumnList(string DbName, string TableName)
		{
			OpenDB();
			DataTable schemaTable = connect.GetOleDbSchemaTable(OleDbSchemaGuid.Columns,
				new Object[] {null, null, TableName, null});			
			return Tab2Colum(schemaTable);

//			string strSql="select * from "+TableName+" where 1=2";
//			OleDbDataReader dr=ExecuteReader(strSql);
//			DataTable dt=dr.GetSchemaTable();
//			dr.Close();
//			return dt;
		}

		#endregion

		#region �õ����ݿ������е���ϸ��Ϣ GetColumnInfoList(string DbName,string TableName)

		/// <summary>
		/// �õ����ݿ������е���ϸ��Ϣ
		/// </summary>
		/// <param name="DbName"></param>
		/// <param name="TableName"></param>
		/// <returns></returns>
		public DataTable GetColumnInfoList(string DbName, string TableName)
		{
//			StringBuilder strSql=new StringBuilder();
//			strSql.Append("select ");
//			strSql.Append("COLUMN_ID as colorder,");
//			strSql.Append("COLUMN_NAME as ColumnName,");
//			strSql.Append("DATA_TYPE as TypeName,");
//			strSql.Append("DATA_LENGTH as Length,");
//			strSql.Append("DATA_PRECISION as Preci,");
//			strSql.Append("DATA_SCALE as Scale,");
//			strSql.Append("'' as IsIdentity,");
//			strSql.Append("'' as isPK,");
//			strSql.Append("NULLABLE as cisNull ,");
//			strSql.Append("DATA_DEFAULT as defaultVal, ");
//			strSql.Append("'' as deText ");
//			strSql.Append(" from USER_TAB_COLUMNS ");
//			strSql.Append(" where TABLE_NAME='"+TableName+"'");
//			strSql.Append(" order by COLUMN_ID");					
//			return Query("",strSql.ToString()).Tables[0];

			OpenDB();
			DataTable schemaTable = connect.GetOleDbSchemaTable(OleDbSchemaGuid.Columns,
				new Object[] {null, null, TableName, null});			
			return Tab2Colum(schemaTable);
		}

		#endregion

		#region �õ����ݿ��������� GetKeyName(string DbName,string TableName)

		private DataTable Key2Colum(DataTable sTable,string TableName)
		{
			DataTable tTable = new DataTable();
			tTable.Columns.Add("colorder");
			tTable.Columns.Add("ColumnName");
			tTable.Columns.Add("TypeName");
			
			int n=0;
			foreach(DataRow row in sTable.Rows)
			{
				string strtab=row[5].ToString();
				if(strtab!=TableName)
				{
					continue;
				}
				string strkey=row[2].ToString();
				if(strkey!="PrimaryKey")
				{
					continue;
				}
				DataRow newRow = tTable.NewRow();
				newRow["colorder"]=row[9].ToString();
				string colname=row[6].ToString();
				newRow["ColumnName"]=colname;
				foreach(DataRow rowk in GetColumnList(null,TableName).Select("ColumnName='"+colname+"'"))
				{
					string type=rowk["TypeName"].ToString();
					newRow["TypeName"]=type;
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
					new Object[] {null, null, null, null});	
				return Key2Colum(schemaTable,TableName);
			}
			catch(System.Exception ex)
			{
				string str=ex.Message;
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
			StringBuilder strSql=new StringBuilder();
			strSql.Append("select * from "+TableName);				
			return Query("",strSql.ToString()).Tables[0];
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
				string strsql="DROP TABLE "+TableName+"";
				ExecuteSql(DbName,TableName);
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

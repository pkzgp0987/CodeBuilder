using System;
using System.Data;
using System.Text;
using System.IO;
using System.Collections;
using LTP.Utility;
using LTP.IDBO;
namespace LTP.DbObjects.Odbc
{
	/// <summary>
	/// ���ݿ�ű������ࡣScript
	/// </summary>
	public class DbScriptBuilder:IDbScriptBuilder
	{

		#region ����
		private string _dbconnectStr;
		private string _dbname;
		private string _tablename;		
		private string _id="ID";//�����ֶ�		
		private string _idType="int";//��������
        private string _procprefix;
        private string _projectname;

		public string DbConnectStr
		{
			set{_dbconnectStr=value;}
			get{return _dbconnectStr;}
		}
		public string DbName
		{
			set{ _dbname=value;}
			get{return _dbname;}
		}
		public string TableName
		{
			set{ _tablename=value;}
			get{return _tablename;}
		}
		public string ID
		{
			set{ _id=value;}
			get{return _id;}
		}
		public string IDType
		{
			set{ _idType=value;}
			get{return _idType;}
		}
        /// <summary>
        /// �洢����ǰ׺ 
        /// </summary>       
        public string ProcPrefix
        {
            set { _procprefix = value; }
            get { return _procprefix; }
        }
        /// <summary>
        /// ��Ŀ���� 
        /// </summary>        
        public string ProjectName
        {
            set { _projectname = value; }
            get { return _projectname; }
        }
		#endregion

		DbObject dbobj=new DbObject();

		public DbScriptBuilder()
		{			
		}
		
		#region �����������Ƿ�ӵ�����
		/// <summary>
		/// �����������Ƿ�ӵ�����
		/// </summary>
		/// <param name="columnType">���ݿ�����</param>
		/// <returns></returns>
		private bool IsAddMark(string columnType)
		{			
			bool isadd=false;
			switch(columnType.Trim())
			{
				case "nvarchar":					
				case "nchar":					
				case "ntext":
				case "varchar":
				case "char":
				case "text":
				case "datetime":
				case "smalldatetime":
				case "string":
					isadd=true;
					break;
			}			
			return isadd;
		}
		#endregion

		#region byte������ת16����

		char[] hexDigits = {'0', '1', '2', '3', '4', '5', '6', '7',
							   '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};
 
		//���ֽ�����ת��Ϊʮ������ֵ�ַ���
		//		public string ToHexString(byte[] bytes) 
		//		{
		//			char[] chars = new char[bytes.Length * 2];
		//			for (int i = 0; i < bytes.Length; i++) 
		//			{
		//				int b = bytes[i];
		//				chars[i * 2] = hexDigits[b >> 4];
		//				chars[i * 2 + 1] = hexDigits[b & 0xF];
		//			}
		//			return new string(chars);
		//		}	
		public string ToHexString(byte[] bytes) 
		{
			char[] chars = new char[bytes.Length * 2];
			for (int i = 0; i < bytes.Length; i++) 
			{
				int b = bytes[i];
				chars[i * 2] = hexDigits[b >> 4];
				chars[i * 2 + 1] = hexDigits[b & 0xF];
			}			
			string str=new string(chars);
			return "0x"+str.Substring(0,bytes.Length);
		}	

		#endregion

		#region �������ݿ�����ű�

		/// <summary>
		/// �������ݿ�����ű�
		/// </summary>
		/// <returns></returns>
		public string CreateTabScript(string dbname,string tablename)
		{
			dbobj.DbConnectStr=_dbconnectStr;
			DataTable dt=dbobj.GetColumnInfoList(dbname,tablename);
			StringPlus strclass=new StringPlus();
//			strclass.AppendLine("if exists (select * from sysobjects where id = OBJECT_ID('["+tablename+"]') and OBJECTPROPERTY(id, 'IsUserTable') = 1) ");
//			strclass.AppendLine("DROP TABLE ["+tablename+"]");
			
			string PKfild="";//�����ֶ�
			StringPlus ColdefaVal=new StringPlus();//�ֶε�Ĭ��ֵ�б�			
			
			Hashtable FildtabList=new Hashtable();//�ֶ��б�
			StringPlus FildList=new StringPlus();//�ֶ��б�
			//��ʼ������
			strclass.AppendLine("");
			strclass.AppendLine("CREATE TABLE ["+tablename+"] (");
			if(dt!=null)
			{				
				foreach(DataRow row in dt.Rows)
				{					
					string columnName=row["ColumnName"].ToString();	
					string columnType=row["TypeName"].ToString();				
					string Length=row["Length"].ToString();
					string Preci=row["Preci"].ToString();
					string Scale=row["Scale"].ToString();
					string ispk=row["isPK"].ToString();	
					string isnull=row["cisNull"].ToString();
					string defaultVal=row["defaultVal"].ToString();

					strclass.Append("["+columnName+"] ["+columnType+"] ");
					switch(columnType.Trim())
					{	
						case "CHAR":					
						case "VARCHAR2":
						case "NCHAR":
						case "NVARCHAR2":						
							strclass.Append(" ("+Length+")");
							break;
						case "NUMBER":		
							strclass.Append(" ("+Preci+","+Scale+")");
							break;						
					}				
					if(isnull=="��")
					{
						strclass.Append(" NULL");
					}
					else
					{
						strclass.Append(" NOT NULL");
					}
					if(defaultVal!="")
					{
						strclass.Append(" DEFAULT "+defaultVal);
					}
					strclass.AppendLine(",");

					FildtabList.Add(columnName,columnType);
					FildList.Append("["+columnName+"],");
					
//					if(defaultVal!="")
//					{
//						ColdefaVal.Append("CONSTRAINT [DF_"+tablename+"_"+columnName+"] DEFAULT "+defaultVal+" FOR ["+columnName+"],");
//					}

					if(PKfild=="")
					{						
						PKfild=columnName;//�õ�����
					}
				}
			}
			strclass.DelLastComma();
			FildList.DelLastComma();
			strclass.AppendLine(")");
			strclass.AppendLine("");

			//��ȡ����
			DataTable dtdata=dbobj.GetTabData(dbname,tablename);
			if(dtdata!=null)
			{				
				foreach(DataRow row in dtdata.Rows)//ѭ��������
				{	
					StringPlus strfild=new StringPlus();
					StringPlus strdata=new StringPlus();
					string [] split= FildList.Value.Split(new Char [] { ','});

					foreach(string fild in split)//ѭ��һ�����ݵĸ����ֶ�
					{
						string colname=fild.Substring(1,fild.Length-2);
						string coltype="";
						foreach (DictionaryEntry myDE in FildtabList)
						{
							if(myDE.Key.ToString()==colname)
							{
								coltype=myDE.Value.ToString();
							}
						}	
						string strval="";
						switch(coltype)
						{
							case "BLOB":
							{
								byte[] bys=(byte[])row[colname];
								strval=ToHexString(bys);
							}
								break;
							case "bit":
							{
								strval=(row[colname].ToString().ToLower()=="true")?"1":"0";
							}
								break;
							default:
								strval=row[colname].ToString().Trim();
								break;
						}
						if(strval!="")
						{
							if(IsAddMark(coltype))
							{
								strdata.Append("'"+strval+"',");
							}
							else
							{
								strdata.Append(strval+",");
							}	
							strfild.Append("["+colname+"],");
						}

					}					
					strfild.DelLastComma();
					strdata.DelLastComma();
					//��������INSERT���
					strclass.Append("INSERT ["+tablename+"] (");
					strclass.Append(strfild.Value);
					strclass.Append(") VALUES ( ");
					strclass.Append(strdata.Value);//����ֵ
					strclass.AppendLine(")");
				}
			}


			return strclass.Value;

		}

	
		/// <summary>
		/// �������ݿ�����ű����ļ�
		/// </summary>
		/// <returns></returns>
		public void CreateTabScript(string dbname,string tablename,string filename,System.Windows.Forms.ProgressBar progressBar)
		{
			StreamWriter sw=new StreamWriter(filename,true,Encoding.Default);//,false);

			dbobj.DbConnectStr=_dbconnectStr;
			DataTable dt=dbobj.GetColumnInfoList(dbname,tablename);
			StringPlus strclass=new StringPlus();
//			strclass.AppendLine("if exists (select * from sysobjects where id = OBJECT_ID('["+tablename+"]') and OBJECTPROPERTY(id, 'IsUserTable') = 1) ");
//			strclass.AppendLine("DROP TABLE ["+tablename+"]");
			
			string PKfild="";//�����ֶ�
			StringPlus ColdefaVal=new StringPlus();//�ֶε�Ĭ��ֵ�б�
			
			
			Hashtable FildtabList=new Hashtable();//�ֶ��б�
			StringPlus FildList=new StringPlus();//�ֶ��б�
			
			#region �����Ľű�

			//��ʼ������
			strclass.AppendLine("");
			strclass.AppendLine("CREATE TABLE ["+tablename+"] (");
			if(dt!=null)
			{				
				foreach(DataRow row in dt.Rows)
				{					
					string columnName=row["ColumnName"].ToString();	
					string columnType=row["TypeName"].ToString();					
					string Length=row["Length"].ToString();
					string Preci=row["Preci"].ToString();
					string Scale=row["Scale"].ToString();
					string ispk=row["isPK"].ToString();	
					string isnull=row["cisNull"].ToString();
					string defaultVal=row["defaultVal"].ToString();

					strclass.Append("["+columnName+"] ["+columnType+"] ");					
					switch(columnType.Trim())
					{	
						case "char":					
						case "varchar":
						case "nchar":
						case "nvarchar":						
							strclass.Append(" ("+Length+")");
							break;
						case "float":		
							strclass.Append(" ("+Preci+","+Scale+")");
							break;							
					}				
					if(isnull=="��")
					{
						strclass.Append(" NULL");
					}
					else
					{
						strclass.Append(" NOT NULL");
					}
					if(defaultVal!="")
					{
						strclass.Append(" DEFAULT "+defaultVal);
					}
					strclass.AppendLine(",");

					FildtabList.Add(columnName,columnType);
					FildList.Append("["+columnName+"],");
					
//					if(defaultVal!="")
//					{
//						ColdefaVal.Append("CONSTRAINT [DF_"+tablename+"_"+columnName+"] DEFAULT "+defaultVal+" FOR ["+columnName+"],");
//					}

					if(PKfild=="")
					{						
						PKfild=columnName;//�õ�����
					}
				}
			}
			strclass.DelLastComma();
			FildList.DelLastComma();
			strclass.AppendLine(")");
			strclass.AppendLine("");
			
			if(PKfild!="")
			{
				strclass.Append("ALTER TABLE ["+tablename+"] WITH NOCHECK ADD  CONSTRAINT [PK_"+tablename+"] PRIMARY KEY  NONCLUSTERED ( ["+PKfild+"] )");
			}

			#endregion			

			#region
			//��������
			//			if((PKfild!="")||(ColdefaVal.Value!=""))
			//			{				
			//				strclass.AppendLine("ALTER TABLE ["+tablename+"] WITH NOCHECK ADD ");
			//				if(ColdefaVal.Value!="")
			//				{
			//					strclass.Append(ColdefaVal.Value);
			//				}
			//				if(PKfild!="")
			//				{
			//					strclass.Append(" CONSTRAINT [PK_"+tablename+"] PRIMARY KEY  NONCLUSTERED ( ["+PKfild+"] )");
			//				}
			//				else
			//				{
			//					strclass.DelLastComma();
			//				}
			//			}
			#endregion

			sw.Write(strclass.Value);

			#region �������ݽű�

			//��ȡ����
			DataTable dtdata=dbobj.GetTabData(dbname,tablename);
			if(dtdata!=null)
			{		
				int i=0;				
				progressBar.Maximum=dtdata.Rows.Count;				
				foreach(DataRow row in dtdata.Rows)//ѭ��������
				{						
					progressBar.Value=i;
					i++;
					StringPlus rowdata=new StringPlus();

					StringPlus strfild=new StringPlus();
					StringPlus strdata=new StringPlus();
					string [] split= FildList.Value.Split(new Char [] { ','});

					foreach(string fild in split)//ѭ��һ�����ݵĸ����ֶ�
					{
						string colname=fild.Substring(1,fild.Length-2);
						string coltype="";
						foreach (DictionaryEntry myDE in FildtabList)
						{
							if(myDE.Key.ToString()==colname)
							{
								coltype=myDE.Value.ToString();
							}
						}	
						string strval="";
						switch(coltype)
						{
							case "binary":
							{
								byte[] bys=(byte[])row[colname];
								strval=ToHexString(bys);
							}
								break;
							case "bit":
							case "bool":
							{
								strval=(row[colname].ToString().ToLower()=="true")?"1":"0";
							}
								break;
							default:
								strval=row[colname].ToString().Trim();
								break;
						}
						if(strval!="")
						{
							if(IsAddMark(coltype))
							{
								strdata.Append("'"+strval+"',");
							}
							else
							{
								strdata.Append(strval+",");
							}	
							strfild.Append("["+colname+"],");
						}

					}					
					strfild.DelLastComma();
					strdata.DelLastComma();

					//��������INSERT���
					rowdata.Append("INSERT ["+tablename+"] (");
					rowdata.Append(strfild.Value);
					rowdata.Append(") VALUES ( ");
					rowdata.Append(strdata.Value);//����ֵ
					rowdata.AppendLine(")");

					sw.Write(rowdata.Value);
				}
			}

			#endregion

			sw.Flush();
			sw.Close();

		}


		/// <summary>
		/// �������ݿ����б�Ĵ����ű�
		/// </summary>
		/// <returns></returns>
		public string CreateDBTabScript(string dbname)
		{			
			dbobj.DbConnectStr=this.DbConnectStr;
			DataTable dt=dbobj.GetTables(dbname);
			StringPlus strclass=new StringPlus();
			if(dt!=null)
			{
				foreach(DataRow row in dt.Rows)
				{
					string tabname=row["name"].ToString();
					
					strclass.AppendLine(CreateTabScript(dbname,tabname));
				}
			}
			return strclass.Value;
		}

		#endregion

		#region �����洢����

		public string CreatPROCGetMaxID()
		{
			StringPlus strclass=new StringPlus();			
			strclass.AppendLine("------------------------------------");	
			strclass.AppendLine("--��;���õ����ID ");
            strclass.AppendLine("--��Ŀ���ƣ�" + ProjectName);
			strclass.AppendLine("--˵����");
			strclass.AppendLine("--ʱ�䣺"+DateTime.Now.ToString());
			strclass.AppendLine("------------------------------------");		
			strclass.AppendLine("CREATE PROCEDURE " + ProcPrefix + ""+_tablename+"_GetMaxId");
			strclass.AppendLine("AS");
			strclass.AppendSpaceLine(1,"DECLARE @TempID int");
			strclass.AppendSpaceLine(1,"SELECT @TempID = max(["+ID+"])+1 FROM "+_tablename);
			strclass.AppendSpaceLine(1,"IF @TempID IS NULL");
			strclass.AppendSpaceLine(2,"RETURN 1");		
			strclass.AppendSpaceLine(1,"ELSE");		
			strclass.AppendSpaceLine(2,"RETURN @TempID");	
			strclass.AppendLine("");	
			strclass.AppendLine("GO");		
			return strclass.ToString();
		}
		public string CreatPROCIsHas()
		{						
			StringPlus strclass=new StringPlus();			
			strclass.AppendLine("------------------------------------");	
			strclass.AppendLine("--��;���Ƿ��Ѿ����� ");
            strclass.AppendLine("--��Ŀ���ƣ�" + ProjectName);
			strclass.AppendLine("--˵����");
			strclass.AppendLine("--ʱ�䣺"+DateTime.Now.ToString());
			strclass.AppendLine("------------------------------------");	
			strclass.AppendLine("CREATE PROCEDURE " + ProcPrefix + ""+_tablename+"_Exists");
			strclass.AppendLine("@"+ID+" "+IDType);
			strclass.AppendLine("AS");
			strclass.AppendSpaceLine(1,"DECLARE @TempID int");
			strclass.AppendSpaceLine(1,"SELECT @TempID = count(1) FROM "+_tablename+" WHERE ["+ID+"] = @"+ID);
			strclass.AppendSpaceLine(1,"IF @TempID = 0");
			strclass.AppendSpaceLine(2,"RETURN 0");		
			strclass.AppendSpaceLine(1,"ELSE");		
			strclass.AppendSpaceLine(2,"RETURN 1");		
			strclass.AppendLine("");	
			strclass.AppendLine("GO");		
			return strclass.Value;
		}
		public string CreatPROCADD(DataTable dtColumn,bool IsHasMaxId)
		{						
			StringPlus strclass=new StringPlus();
			StringPlus strclass1=new StringPlus();
			StringPlus strclass2=new StringPlus();			
			strclass.AppendLine("------------------------------------");	
			strclass.AppendLine("--��;������һ����¼ ");
            strclass.AppendLine("--��Ŀ���ƣ�" + ProjectName);
			strclass.AppendLine("--˵����");
			strclass.AppendLine("--ʱ�䣺"+DateTime.Now.ToString());
			strclass.AppendLine("------------------------------------");
			strclass.AppendLine("CREATE PROCEDURE " + ProcPrefix + ""+_tablename+"_ADD");			
			foreach(DataRow row in dtColumn.Rows)
			{
				string columnName=row["ColumnName"].ToString();
				string columnType=row["TypeName"].ToString();
				string Length=row["Length"].ToString();				
				string Preci=row["Preci"].ToString();		
				string Scale=row["Scale"].ToString();					
				switch(columnType.ToLower())
				{
					case "decimal":
					case "numeric":
						strclass.Append("@"+columnName+" "+columnType+"("+Preci+","+Scale+")");
						break;
					case "varchar":
					case "nvarchar":
					case "char":
					case "nchar":					
						strclass.Append("@"+columnName+" "+columnType+"("+Length+")");
						break;
					default:
						strclass.Append("@"+columnName+" "+columnType);
						break;
				}
				if(ID==columnName)
				{
					if(!IsHasMaxId)					
					{
						strclass.AppendLine(" output,");
						continue;
					}
				}			
				strclass.AppendLine(" ,");
				strclass1.Append("["+columnName+"],");
				strclass2.Append("@"+columnName+",");

			}			
			strclass.DelLastComma();
			strclass1.DelLastComma();
			strclass2.DelLastComma();
			strclass.AppendLine("");
			strclass.AppendLine(" AS ");
			strclass.AppendSpaceLine(1,"INSERT INTO "+_tablename+"(");
			strclass.AppendSpaceLine(1,strclass1.Value);
			strclass.AppendSpaceLine(1,")VALUES(");
			strclass.AppendSpaceLine(1,strclass2.Value);		
			strclass.AppendSpaceLine(1,")");
			if(!IsHasMaxId)	
			{
				strclass.AppendSpaceLine(1,"SET @"+ID+" = @@IDENTITY");		
			}
			strclass.AppendLine("");	
			strclass.AppendLine("GO");		
			return strclass.Value;
		}
		public string CreatPROCUpdate(DataTable dtColumn)
		{						
			StringPlus strclass=new StringPlus();
			StringPlus strclass1=new StringPlus();						
			strclass.AppendLine("------------------------------------");	
			strclass.AppendLine("--��;���޸�һ����¼ ");
            strclass.AppendLine("--��Ŀ���ƣ�" + ProjectName);
			strclass.AppendLine("--˵����");
			strclass.AppendLine("--ʱ�䣺"+DateTime.Now.ToString());
			strclass.AppendLine("------------------------------------");		
			strclass.AppendLine("CREATE PROCEDURE " + ProcPrefix + ""+_tablename+"_Update");			
			foreach(DataRow row in dtColumn.Rows)
			{
				string columnName=row["ColumnName"].ToString();
				string columnType=row["TypeName"].ToString();
				string Length=row["Length"].ToString();				
				string Preci=row["Preci"].ToString();		
				string Scale=row["Scale"].ToString();				
				switch(columnType.ToLower())
				{
					case "decimal":
					case "numeric":
						strclass.AppendLine("@"+columnName+" "+columnType+"("+Preci+","+Scale+"),");
						break;
					case "varchar":
					case "nvarchar":
					case "char":
					case "nchar":	
						strclass.AppendLine("@"+columnName+" "+columnType+"("+Length+"),");						
						break;
					default:
						strclass.AppendLine("@"+columnName+" "+columnType+",");
						break;
				}
				if(ID==columnName)
				{					
					continue;					
				}					
				strclass1.Append("["+columnName+"] = @"+columnName+",");
			}			
			strclass.DelLastComma();
			strclass1.DelLastComma();
			strclass.AppendLine("");
			strclass.AppendLine(" AS ");
			strclass.AppendSpaceLine(1,"UPDATE "+_tablename+" SET ");
			strclass.AppendSpaceLine(1,strclass1.Value);
			strclass.AppendSpaceLine(1,"WHERE ["+ID+"] = @"+ID);	
			strclass.AppendLine("");	
			strclass.AppendLine("GO");		
			return strclass.Value;
		}
		public string CreatPROCDelete()
		{					
			StringPlus strclass=new StringPlus();
			StringPlus strclass1=new StringPlus();						
			strclass.AppendLine("------------------------------------");	
			strclass.AppendLine("--��;��ɾ��һ����¼ ");
            strclass.AppendLine("--��Ŀ���ƣ�" + ProjectName);
			strclass.AppendLine("--˵����");
			strclass.AppendLine("--ʱ�䣺"+DateTime.Now.ToString());
			strclass.AppendLine("------------------------------------");
			strclass.AppendLine("CREATE PROCEDURE " + ProcPrefix + ""+_tablename+"_Delete");			
			strclass.AppendLine("@"+ID+" "+this.IDType);
			strclass.AppendLine(" AS ");
			strclass.AppendSpaceLine(1,"DELETE "+_tablename);			
			strclass.AppendSpaceLine(1," WHERE ["+ID+"] = @"+ID);	
			strclass.AppendLine("");	
			strclass.AppendLine("GO");		
			return strclass.Value;
			
		}
        public string CreatPROCGetModel(DataTable dtColumn)
		{		
			StringPlus strclass=new StringPlus();
			StringPlus strclass1=new StringPlus();						
			strclass.AppendLine("------------------------------------");	
			strclass.AppendLine("--��;���õ�ʵ��������ϸ��Ϣ ");
            strclass.AppendLine("--��Ŀ���ƣ�" + ProjectName);
			strclass.AppendLine("--˵����");
			strclass.AppendLine("--ʱ�䣺"+DateTime.Now.ToString());
			strclass.AppendLine("------------------------------------");
            foreach (DataRow row in dtColumn.Rows)
            {
                string columnName = row["ColumnName"].ToString();
                strclass1.Append("[" + columnName + "],");
            }
            strclass1.DelLastComma();
			strclass.AppendLine("CREATE PROCEDURE " + ProcPrefix + ""+_tablename+"_GetModel");			
			strclass.AppendLine("@"+ID+" "+this.IDType);
			strclass.AppendLine(" AS ");
            strclass.AppendSpaceLine(1, "SELECT ");
            strclass.AppendSpaceLine(1, strclass1.Value);
            strclass.AppendSpaceLine(1, " FROM " + _tablename);						
			strclass.AppendSpaceLine(1," WHERE ["+ID+"] = @"+ID);	
			strclass.AppendLine("");	
			strclass.AppendLine("GO");		
			return strclass.Value;
			
		}
		public string CreatPROCGetList(DataTable dtColumn)
		{						
			StringPlus strclass=new StringPlus();
			StringPlus strclass1=new StringPlus();			
			strclass.AppendLine("------------------------------------");	
			strclass.AppendLine("--��;����ѯ��¼��Ϣ ");
            strclass.AppendLine("--��Ŀ���ƣ�" + ProjectName);
			strclass.AppendLine("--˵����");
			strclass.AppendLine("--ʱ�䣺"+DateTime.Now.ToString());
			strclass.AppendLine("------------------------------------");	
			foreach(DataRow row in dtColumn.Rows)
			{
				string columnName=row["ColumnName"].ToString();				
				strclass1.Append("["+columnName+"],");
			}
			strclass1.DelLastComma();
			strclass.AppendLine("CREATE PROCEDURE " + ProcPrefix + ""+_tablename+"_GetList");						
			strclass.AppendLine(" AS ");
			strclass.AppendSpaceLine(1,"SELECT ");			
			strclass.AppendSpaceLine(1,strclass1.Value);			
			strclass.AppendSpaceLine(1," FROM "+_tablename);			
			//strclass.AppendSpaceLine(1," WHERE ");
			strclass.AppendLine("");	
			strclass.AppendLine("GO");		
			return strclass.Value;
			
		}

		
		/// <summary>
		/// �õ�ĳ����Ĵ洢���̣�ѡ�����ɵķ�����
		/// </summary>
		/// <param name="Maxid"></param>
		/// <param name="Ishas"></param>
		/// <param name="Add"></param>
		/// <param name="Update"></param>
		/// <param name="Delete"></param>
		/// <param name="GetModel"></param>
		/// <param name="List"></param>
		/// <param name="dtColumn">�����������Ϣ</param>
		/// <returns></returns>
		public string GetPROCCode(bool Maxid,bool Ishas,bool Add,bool Update,bool Delete,bool GetModel,bool List,DataTable dtColumn)
		{
			StringPlus strclass=new StringPlus();			
			strclass.AppendLine("/******************************************************************");
			strclass.AppendLine("* ������"+_tablename);
			strclass.AppendLine("* ʱ�䣺"+DateTime.Now.ToString());
			strclass.AppendLine("******************************************************************/");
			strclass.AppendLine("");
			#region  ��������
			if(Maxid)
			{
				strclass.AppendLine(CreatPROCGetMaxID());
			}
			if(Ishas)
			{
				strclass.AppendLine(CreatPROCIsHas());
			}
			if(Add)
			{
				strclass.AppendLine(CreatPROCADD(dtColumn,Maxid));
			}
			if(Update)
			{
				strclass.AppendLine(CreatPROCUpdate(dtColumn));
			}
			if(Delete)
			{
				strclass.AppendLine(CreatPROCDelete());
			}
			if(GetModel)
			{
                strclass.AppendLine(CreatPROCGetModel(dtColumn));
			}
			if(List)
			{
				strclass.AppendLine(CreatPROCGetList(dtColumn));
			}
			
			
			#endregion
			return strclass.Value;
		}

		/// <summary>
		/// �õ�ĳ����Ĵ洢����
		/// </summary>
		/// <param name="dbname">����</param>
		/// <param name="tablename">����</param>
		/// <returns></returns>
		public string GetPROCCode(string dbname,string tablename)
		{
			dbobj.DbConnectStr=_dbconnectStr;
			DataTable dt=dbobj.GetColumnInfoList(dbname,tablename);
			this.DbName=dbname;
			this.TableName=tablename;
			if(dt!=null)
			{
				foreach(DataRow row in dt.Rows)
				{
					string columnName=row["ColumnName"].ToString();	
					string columnType=row["TypeName"].ToString();				
					string IsIdentity=row["IsIdentity"].ToString();	
					string ispk=row["isPK"].ToString();	
					string isnull=row["cisNull"].ToString();
					if(((ispk=="��")&&(isnull.Trim()==""))||(IsIdentity=="��"))
					{
						this.ID=columnName;
						this.IDType=columnType;
						break;
					}
				}
			}
			return GetPROCCode(true,true,true,true,true,true,true,dt);
		}
		/// <summary>
		/// �õ�һ���������б�Ĵ洢����
		/// </summary>
		/// <param name="DbName"></param>
		/// <returns></returns>
		public string GetPROCCode(string dbname)
		{
			dbobj.DbConnectStr=_dbconnectStr;
			DataTable dt=dbobj.GetTables(dbname);
			StringPlus strclass=new StringPlus();
			if(dt!=null)
			{
				foreach(DataRow row in dt.Rows)
				{
					string tabname=row["name"].ToString();
					
					strclass.AppendLine(GetPROCCode(dbname,tabname));
				}
			}
			return strclass.Value;
		}
		#endregion


        #region ����SQL��ѯ���

        /// <summary>
        /// ����Select��ѯ���
        /// </summary>
        /// <param name="dbname">����</param>
        /// <param name="tablename">����</param>
        /// <returns></returns>
        public string GetSQLSelect(string dbname, string tablename)
        {
            dbobj.DbConnectStr = _dbconnectStr;
            DataTable dt = dbobj.GetColumnList(dbname, tablename);
            this.DbName = dbname;
            this.TableName = tablename;
            StringPlus strsql = new StringPlus();
            strsql.Append("select ");
            if (dt != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    string columnName = row["ColumnName"].ToString();
                    strsql.Append("[" + columnName + "],");
                }
                strsql.DelLastComma();
            }
            strsql.Append(" from " + tablename);
            return strsql.Value;
        }

        /// <summary>
        /// ����update��ѯ���
        /// </summary>
        /// <param name="dbname">����</param>
        /// <param name="tablename">����</param>
        /// <returns></returns>
        public string GetSQLUpdate(string dbname, string tablename)
        {
            dbobj.DbConnectStr = _dbconnectStr;
            DataTable dt = dbobj.GetColumnList(dbname, tablename);
            this.DbName = dbname;
            this.TableName = tablename;
            StringPlus strsql = new StringPlus();
            strsql.AppendLine("update " + tablename + " set ");
            if (dt != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    string columnName = row["ColumnName"].ToString();
                    strsql.AppendLine("[" + columnName + "] = <" + columnName + ">,");
                }
                strsql.DelLastComma();
            }
            strsql.Append(" where <��������>");
            return strsql.Value;
        }
        /// <summary>
        /// ����update��ѯ���
        /// </summary>
        /// <param name="dbname">����</param>
        /// <param name="tablename">����</param>
        /// <returns></returns>
        public string GetSQLDelete(string dbname, string tablename)
        {
            dbobj.DbConnectStr = _dbconnectStr;
            DataTable dt = dbobj.GetColumnList(dbname, tablename);
            this.DbName = dbname;
            this.TableName = tablename;
            StringPlus strsql = new StringPlus();
            strsql.AppendLine("delete from " + tablename);
            strsql.Append(" where  <��������>");
            return strsql.Value;
        }
        /// <summary>
        /// ����INSERT��ѯ���
        /// </summary>
        /// <param name="dbname">����</param>
        /// <param name="tablename">����</param>
        /// <returns></returns>
        public string GetSQLInsert(string dbname, string tablename)
        {
            dbobj.DbConnectStr = _dbconnectStr;
            DataTable dt = dbobj.GetColumnList(dbname, tablename);
            this.DbName = dbname;
            this.TableName = tablename;
            StringPlus strsql = new StringPlus();
            StringPlus strsql2 = new StringPlus();
            strsql.AppendLine("INSERT INTO " + tablename + " ( ");

            if (dt != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    string columnName = row["ColumnName"].ToString();
                    string columnType = row["TypeName"].ToString();
                    strsql.AppendLine("[" + columnName + "] ,");
                    if (IsAddMark(columnType))
                    {
                        strsql2.Append("'" + columnName + "',");
                    }
                    else
                    {
                        strsql2.Append(columnName + ",");
                    }

                }
                strsql.DelLastComma();
                strsql2.DelLastComma();
            }
            strsql.Append(") VALUES (" + strsql2.Value + ")");
            return strsql.Value;
        }
        #endregion
	}
}

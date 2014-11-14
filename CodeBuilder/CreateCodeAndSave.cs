using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;
using System.Windows.Forms;
using CodeHelper;
using DbObjects.SQLite;
using TempCreate;
using FileCreate;

namespace CodeBuilder
{
    class CreateCodeAndSave
    {
        public CreateCodeAndSave()
        {
            WFile = new WriteFile();
        }
        public string DBPath;
        public string ResultPath;

        private string constr;

        private List<TableInfo> TableNames;

        DbObject Dbobj;

        string ModelHfile="";

        WriteFile WFile;

        //测试数据库是否可以连接
        public bool isDBCanConnection()
        {
            try
            {
                constr ="Data Source=" + DBPath;
                //测试连接
                SQLiteConnection myCn = new SQLiteConnection(constr);
                try
                {
                    myCn.Open();
                }
                catch
                {
                    MessageBox.Show("连接数据库失败！请检查数据库地址或密码是否正确！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                myCn.Close();
                return true;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        //开始
        public void start()
        {
            if (DBPath == "" || ResultPath == "")
            {
                MessageBox.Show("数据库和输出路径不能为空！");
                return;
            }
            if (isDBCanConnection())
            {
                OpenDBAndReadInfo();
            }
            MessageBox.Show("生成完成！");
        }

        //打开数据库文件并解析表结构
        public void OpenDBAndReadInfo()
        {
            Dbobj = new DbObject();
            Dbobj.DbConnectStr = constr;
            TableNames = Dbobj.GetTablesInfo(constr);
            foreach(TableInfo table in TableNames)
            {
                List<ColumnInfo> columns = Dbobj.GetColumnList(constr, table.TabName);
                Create_ModelAndDal(columns, table.TabName + "Model", table.TabName);
                Add_Model(table.TabName + "Model");
            }
            writeAllModel();
            Create_Copy();
        }

        //生成并写入Model和DAL
        public void Create_ModelAndDal(List<ColumnInfo> columns,string ModelName,string tableName)
        {
            TempCreate.TempManager tc=new TempManager(columns,tableName,ModelName);
            WFile.WriteAllFile(@"" + ResultPath + "/Model/" + ModelName + ".h", tc.CreateC_Model());
            WFile.WriteAllFile(@"" + ResultPath + "/DAL/" + ModelName + "Query.h", tc.CreateC_H());
            WFile.WriteAllFile(@"" + ResultPath + "/DAL/" + ModelName + "Query.cpp", tc.CreateC_CPP());
        }

        //添加头文件
        public void Add_Model(string ModelName)
        {
            if (ModelHfile=="")
            {
                ModelHfile = ReadFile.ReadAllFile("TempFile/AllAttribute.h");
            }
            int index = ModelHfile.IndexOf("#include#");
            StringControl str=new StringControl();
            str.Append("#include \""+ModelName+".h\"");
            str.AppendLine();
            ModelHfile= ModelHfile.Insert(index - 1, str.Value);
        }

        //写入AllModel头文件
        public void writeAllModel()
        {
            ModelHfile = ModelHfile.Replace("#include#", "");
            WFile.WriteAllFile(@"" + ResultPath + "/Model/AllAttribute.h", ModelHfile);
        }

        //写入必要的文件（copy）
        public void Create_Copy()
        {
            WFile.CopyFile("TempFile/BaseQuery.h", @"" + ResultPath + "/DAL/BaseQuery.h");
            WFile.CopyFile("TempFile/BaseQuery.cpp", @"" + ResultPath + "/DAL/BaseQuery.cpp");
            WFile.CopyFile("TempFile/GameDefine.h", @"" + ResultPath + "/GameDefine.h");
            WFile.CopyFile("TempFile/DBConnection.h", @"" + ResultPath + "/DAL/DBConnection.h");
            WFile.CopyFile("TempFile/DBConnection.cpp", @"" + ResultPath + "/DAL/DBConnection.cpp");
        }

    }
}

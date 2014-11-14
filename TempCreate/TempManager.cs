using System;
using System.Collections.Generic;
using System.Text;
using FileCreate;
using CodeHelper;

namespace TempCreate
{
    public class TempManager:Temp
    {
        public TempManager(List<ColumnInfo> m_TableColumn, string M_TableName, string m_ModelName)
        {
            TableColumn = m_TableColumn;
            TableName = M_TableName;
            ModelName = m_ModelName;
        }
        //生成C++头文件
        public string CreateC_Model()
        {
            ReadTempFile("TempFile/ModelFiel.h");
            return GetResult();
        }
        //生成C++头文件
        public string CreateC_H()
        {
            ReadTempFile("TempFile/_ModelName_Query.h");
            return GetResult();
        }
        //生成c++ cpp文件
        public string CreateC_CPP()
        {
            ReadTempFile("TempFile/_ModelName_Query.cpp");
            return GetResult();
        }
    }
}

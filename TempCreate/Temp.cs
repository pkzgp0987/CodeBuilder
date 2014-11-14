using System;
using System.Collections.Generic;
using System.Text;
using FileCreate;
using CodeHelper;

namespace TempCreate
{
    /// <summary>
    /// 模版解析核心类
    /// </summary>
    public class Temp
    {
        //模版文件数据
        private String temptext;

        //所有需要替换的key
        private Dictionary<string, string> allReplaceKey;

        //模版文件需要连接的表类型和字段名
        public List<ColumnInfo> TableColumn;

        //模版表的名字
        public string TableName="";

        //模版表模型的名字
        public string ModelName="";

        public Temp()
        {
            allReplaceKey = new Dictionary<string, string>();
        }

        public Temp(List<ColumnInfo> m_TableColumn, string M_TableName, string m_ModelName)
        {
            allReplaceKey = new Dictionary<string, string>();
            TableColumn = m_TableColumn;
            TableName = M_TableName;
            ModelName = m_ModelName;
        }

        public string GetResult()
        {
            return temptext;
        }
        //读取模版文件，并解析替换规则(调用该方法前必须给所有属性赋值)
        public void ReadTempFile(string filename)
        {
            temptext=ReadFile.ReadAllFile(filename);
            allReplaceKey.Clear();
            string[] ch = new string[] { "<#","#>"};
            string[] ResultArray = temptext.Split(ch, StringSplitOptions.None);
            ReplaceRule(ResultArray);
            foreach (KeyValuePair<string, string> key in allReplaceKey)
            {
               temptext=temptext.Replace(key.Key,key.Value);
            }
        }
        //替换规则
        //_time_        注：系统时间
        //_TableName_    注：替换为表的名称
        //_ModelName_    注：替换为模型名称
        //_PID_ 主键
        //_columnName_    注：替换为当前的列的名称
        //_i_    注：替换为当前的列的序号
        //<#foreach[parm]case格式#>    注：遍历 parm：遍历的对象，主要有table(表不带主键)，table0(表带主键)
        //case格式:case[Type1,Type2,...]:[replace1,replace2,...](x)   注：替换的格式(Type若为0则不分类型) (x为列的分割符，一般为符号如：,;:,若换行则为n,T为缩进,p为空格)
        //<#PIDcase[Type1,Type2,...]:[replace1,replace2,...]#> 注：主键类型需要判断
        //
        //将需要解析的文字段放入解析队列
        public void ReplaceRule(string[] ResultArray)
        {
            foreach (string str in ResultArray)
            {
                //找出所有的解析函数为替换规则
                if (str.StartsWith("foreach[") || str.StartsWith("PIDcase["))
                {
                    if (!allReplaceKey.ContainsKey(str))
                    {
                        string result = AnalyticMethod(str);
                        allReplaceKey.Add(str, result);
                    }
                    
                }
            }
            //添加其他的替换规则
            allReplaceKey.Add("<#", "");
            allReplaceKey.Add("#>", "");
            allReplaceKey.Add("_TableName_", TableName);
            allReplaceKey.Add("_ModelName_", ModelName);
            allReplaceKey.Add("_time_", DateTime.Now.ToShortDateString());
        }

        //解析方法
        private string AnalyticMethod(string str)
        {
            if(str.StartsWith("foreach["))
            {
                //判断foreach的循环范围
                string s_tabel = getMid(str, "[", "]");
                if (s_tabel == "table0")
                {
                    //截取case
                    string result = AnalyticForeach(str.Substring(str.IndexOf("]")+1), 0);
                    return result;
                }
                else if (s_tabel == "table")
                {
                    //截取case
                    string result=AnalyticForeach(str.Substring(str.IndexOf("]")+1), 1);
                    return result;
                }
               
            }
            else if (str.StartsWith("PIDcase["))
            {
                foreach (ColumnInfo c_info in TableColumn)
                {
                    if (c_info.IsPK)
                    {
                        //截取case
                        return AnalyticCase(str, -1, c_info.ColumnName, c_info.TypeName);
                    }
                }
            }
            return null;
            
        }
        //解析Foreach方法
        private string AnalyticForeach(string str, int startNum)
        {
            StringControl m_strcon = new StringControl();
            //截取case后的分隔符
            string m_delimiter = AnalyticDelimiter(getMid(str.Substring(str.LastIndexOf("]")+1), "(", ")"));
            string m_case = str.Substring(0,str.LastIndexOf("("));
            int i = 0;
            foreach (ColumnInfo c_info in TableColumn)
            {
                if (i >= startNum)
                {
                    if (i >= startNum+1)
                    {
                        m_strcon.Append(m_delimiter);
                    }
                    m_strcon.Append(AnalyticCase(m_case,i,c_info.ColumnName,c_info.TypeName));
                }
                i++;
            }
            return m_strcon.Value;
        }
        //解析分隔符
        private string AnalyticDelimiter(string m_delimiter)
        {
            StringControl m_strcon = new StringControl();
            int size = m_delimiter.Length;
            for (int i = 0; i < size; i++)
            {
                string str=m_delimiter.Substring(i,1);
                if (str=="n")
                {
                    m_strcon.AppendLine();
                }
                else if (str == "T")
                {
                    m_strcon.AppendSpace(1,"");
                }
                else if (str == "p")
                {
                    m_strcon.AppendSpace(1, "");
                }
                else
                {
                    m_strcon.Append(str);
                }
            }
            return m_strcon.Value;
        }
        //解析Case方法(num为-1时表示没有序号标识)
        private string AnalyticCase(string str, int num, string columnName,string columnType)
        {
            StringBuilder m_strcon = new StringBuilder();
            //获取条件和替换对象的字串
            string m_Conditions = getMid(str,"[","]",Mid_type.start_last_type);
            string[] s = new string[]{"]:[" };
            string[] ReplaceRule = m_Conditions.Split(s, StringSplitOptions.None);
            //获取条件和替换结果
            string[] m_type = ReplaceRule[0].Split(',');
            string[] m_Result = ReplaceRule[1].Split(';');
            //判断是否为无类型判断
            if (m_type[0] == "0")
            {
                m_strcon.Append(m_Result[0]);
            }
            else
            {
                for (int i = 0; i < m_type.Length; i++)
                {
                    if (m_type[i] == columnType)
                    {
                        m_strcon.Append(m_Result[i]);
                    }
                }
            }
            string str_num = num.ToString();
            //替换结果中的列名和序号
            m_strcon.Replace("_i_", str_num);
            m_strcon.Replace("_columnName_", columnName);
            m_strcon.Replace("_PID_", columnName);
            return m_strcon.ToString();
        }
        //获取目标字段中的字段
        public string getMid(string target, string start, string end, Mid_type type = Mid_type.start_type)
        {
            int m_s = target.IndexOf(start);
            int m_e;
            if (type == Mid_type.start_last_type)
            {
                m_e = target.LastIndexOf(end);
            }
            else
            {
                m_e = target.IndexOf(end);
            }
            return target.Substring(m_s+1,m_e-m_s-1);
        }

        //自定义替换规则
        public void UserReplaceRule()
        {

        }

        public enum Mid_type
        {
            //最前面的两个字符串之间
            start_type,
            //最前和最后的两个字符之间
            start_last_type
        }
    }
}

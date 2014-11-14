using System;
using System.Collections.Generic;
using System.Text;

namespace CodeHelper
{
    /// <summary>
    /// 命名规则处理
    /// </summary>
    public class NameRule
    {
        CmConfig.ModuleSettings settings;
        public NameRule()
        {
            settings = CmConfig.ModuleConfig.GetSettings();
        }
        /// <summary>
        /// 得到Model类名
        /// </summary>
        /// <param name="TabName">表名</param>
        /// <returns></returns>
        public string GetModelClass(string TabName)
        {
            return settings.ModelPrefix + TabNameRuled(TabName) + settings.ModelSuffix;
        }
        /// <summary>
        /// 得到BLL类名
        /// </summary>
        /// <param name="TabName"></param>
        /// <returns></returns>
        public string GetBLLClass(string TabName)
        {          
            return settings.BLLPrefix + TabNameRuled(TabName) + settings.BLLSuffix;
        }
       
        /// <summary>
        /// 得到DAL类名
        /// </summary>
        /// <param name="TabName"></param>
        /// <returns></returns>
        public string GetDALClass(string TabName)
        {
            return settings.DALPrefix + TabNameRuled(TabName) + settings.DALSuffix;
        }

        private string TabNameRuled(string TabName)
        {
            string newTabName = TabName;
            switch (settings.TabNameRule.ToLower())
            {                
                case "lower":
                    newTabName = TabName.ToLower();
                    break;
                case "upper":
                    newTabName = TabName.ToUpper();
                    break;
                case "firstupper":
                    {
                        string strfir = TabName.Substring(0,1).ToUpper();
                        newTabName = strfir + TabName.Substring(1);
                    }
                    break;
                case "same":
                    break;
                default:
                    break;
            }
            return newTabName;
        }
    }
}

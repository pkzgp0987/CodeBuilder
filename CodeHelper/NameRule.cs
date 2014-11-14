using System;
using System.Collections.Generic;
using System.Text;

namespace CodeHelper
{
    /// <summary>
    /// ����������
    /// </summary>
    public class NameRule
    {
        CmConfig.ModuleSettings settings;
        public NameRule()
        {
            settings = CmConfig.ModuleConfig.GetSettings();
        }
        /// <summary>
        /// �õ�Model����
        /// </summary>
        /// <param name="TabName">����</param>
        /// <returns></returns>
        public string GetModelClass(string TabName)
        {
            return settings.ModelPrefix + TabNameRuled(TabName) + settings.ModelSuffix;
        }
        /// <summary>
        /// �õ�BLL����
        /// </summary>
        /// <param name="TabName"></param>
        /// <returns></returns>
        public string GetBLLClass(string TabName)
        {          
            return settings.BLLPrefix + TabNameRuled(TabName) + settings.BLLSuffix;
        }
       
        /// <summary>
        /// �õ�DAL����
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

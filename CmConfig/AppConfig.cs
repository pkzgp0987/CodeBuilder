using System;
using System.IO;
using System.Web;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.Windows.Forms;
namespace CmConfig
{
    //����������

    #region ���ö���ģ���� ModuleSettings

    /// <summary>
    /// ���õ�modul�ࣨע��������Ե�������[XmlElement]��
    /// use:AppSettings settings=AppConfig.GetSettings();
    /// </summary>
    public class AppSettings
    {
        private string _appstart;
        private string _startuppage;
        private string _homepage;
        private string _templatefolder = "Template";
        private bool _setup=false;
        
        /// <summary>
        /// Ӧ�ó�������ʱ startuppage   blank   homepage
        /// </summary>
        [XmlElement]
        public string AppStart
        {
            set { _appstart = value; }
            get { return _appstart; }
        }
        /// <summary>
        /// ��ʼҳrss��ַ
        /// </summary>
        [XmlElement]
        public string StartUpPage
        {
            set { _startuppage = value; }
            get { return _startuppage; }
        }
        /// <summary>
        /// ��ҳurl��ַ
        /// </summary>
        [XmlElement]
        public string HomePage
        {
            set { _homepage = value; }
            get { return _homepage; }
        } 
        /// <summary>
        /// ����ģ����Ŀ¼
        /// </summary>
        [XmlElement]
        public string TemplateFolder
        {
            set { _templatefolder = value; }
            get { return _templatefolder; }
        }
        /// <summary>
        /// �Ƿ��Ͱ�װ��Ϣ
        /// </summary>
        [XmlElement]
        public bool Setup
        {
            set { _setup = value; }
            get { return _setup; }
        }
    }
    #endregion


    #region  ���õĲ����� ModuleConfig

    /// <summary>
    /// ���õĲ�����ModuleConfig��
    /// </summary>
    public class AppConfig
    {
        public static AppSettings GetSettings()
        {
            AppSettings data = null;
            XmlSerializer serializer = new XmlSerializer(typeof(AppSettings));
            try
            {
                string apppath = Application.StartupPath;
                string fileName = apppath + "\\appconfig.config";
                FileStream fs = new FileStream(fileName, FileMode.Open);
                data = (AppSettings)serializer.Deserialize(fs);
                fs.Close();
            }
            catch
            {
                data = new AppSettings();
            }
            return data;
        }

        public static void SaveSettings(AppSettings data)
        {
            string apppath = Application.StartupPath;
            string fileName = apppath + "\\appconfig.config";
            XmlSerializer serializer = new XmlSerializer(typeof(AppSettings));

            // serialize the object
            FileStream fs = new FileStream(fileName, FileMode.Create);
            serializer.Serialize(fs, data);
            fs.Close();
        }
    }

    #endregion



}

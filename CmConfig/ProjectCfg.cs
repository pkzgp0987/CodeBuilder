using System;
using System.IO;
using System.Web;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
namespace CmConfig
{
	//��Ŀ��������

	#region ���ö���ģ���� ProSettings

	/// <summary>
	/// ��Ŀ��������
	/// </summary>
	public class ProSettings
	{		
		private string _mode="";
		private string _fileext="";
		private string _fileextdel="";
		private string _sourceDirectory;
		private string _targetDirectory;
				

		/// <summary>
		/// ������ʽ
		/// </summary>
		[XmlElement]
		public string Mode
		{
			set{ _mode=value; }
			get{ return _mode; }
		}
		/// <summary>
		/// ɸѡ��
		/// </summary>
		[XmlElement]
		public string FileExt
		{
			set{ _fileext=value; }
			get{ return _fileext; }
		}
		/// <summary>
		/// ���˷�
		/// </summary>
		[XmlElement]
		public string FileExtDel
		{
			set{ _fileextdel=value; }
			get{ return _fileextdel; }
		}
		/// <summary>
		/// �ϴε�Դ·��
		/// </summary>
		[XmlElement]
		public string SourceDirectory
		{
			set{ _sourceDirectory=value; }
			get{ return _sourceDirectory; }
		}
		/// <summary>
		/// �ϴε�Ŀ��·��
		/// </summary>
		[XmlElement]
		public string TargetDirectory
		{
			set{ _targetDirectory=value; }
			get{ return _targetDirectory; }
		}

	}
	#endregion 


	#region  ���õĲ����� ProConfig
	/// <summary>
	/// ���õĲ�����ModuleConfig��
	/// </summary>
	public class ProConfig
	{

		public static ProSettings GetSettings()
		{			
			ProSettings data = null;
			XmlSerializer serializer = new XmlSerializer(typeof(ProSettings));
			try
			{
				string fileName = "ProConfig.config";				
				FileStream fs = new FileStream(fileName, FileMode.Open);					
				data = (ProSettings)serializer.Deserialize(fs);
				fs.Close();				
			}
			catch
			{	
				data = new ProSettings();
			}
		
			
			return data;
		}


		public static void SaveSettings(ProSettings data)
		{
			string fileName = "ProConfig.config";
			XmlSerializer serializer = new XmlSerializer (typeof(ProSettings));
        
			// serialize the object
			FileStream fs = new FileStream(fileName, FileMode.Create);
			serializer.Serialize(fs, data);
			fs.Close();
		}

		}
	
	#endregion


}

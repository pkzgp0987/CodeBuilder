using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace FileCreate
{
    public class WriteFile
    {        
        //判断文件是否存在
        static public bool isFileExist(string fullfilename)
        {
            return File.Exists(fullfilename);
        }

        //把代码写入指定文件
        public void WriteAllFile(string Filename, string strCode)
        {
            FolderCheck(Filename.Remove(Filename.LastIndexOf("/")));
            StreamWriter sw = new StreamWriter(Filename, false, Encoding.Default);//,false);
            sw.Write(strCode);
            sw.Flush();
            sw.Close();
        }
        //复制文件
        public void CopyFile(string Filename, string TargetFilename)
        {
            File.Copy(Filename, TargetFilename, true);
        }
        //判断文件路径是否存在
        public void FolderCheck(string Folder)
        {
            DirectoryInfo target = new DirectoryInfo(Folder);
            if (!target.Exists)
            {
                target.Create();
            }
        }
    }
}

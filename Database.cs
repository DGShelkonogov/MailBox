using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Mail
{
    class Database
    {

        public const string FILE_USER_INFO = "user_info";
        public const string HTML_BUFFER = "html_trash.html";
        public const string DRAFT_MAILS_FILE = "draft_mails";
        public static string getData(string path)
        {
            string str = "";
            using (BinaryReader binaryReader = new BinaryReader(
                File.Open(path, FileMode.OpenOrCreate)))
            {
                try
                {
                    while (true)
                    {
                        str += binaryReader.ReadString();
                    }
                   
                }catch (Exception e2) { }
                  
            }
            return str;
        }

        public static void writeInFile(string path, string str, FileMode mode)
        {
            using (BinaryWriter binaryWriter = new BinaryWriter(
            File.Open(path, mode)))
            {
               binaryWriter.Write(str);
            }
        }

        public static void DeleteFile(string path)
        {
            File.Delete(path);
        }
    }
}

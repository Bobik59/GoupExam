using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace GoupExam
{

    public class DirectoryManager
    {
        public static string p;
        public static void CreateDirectory(string path,string userName)
        {
            p = path;
            path += userName;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
          
        }

        public static void CreateFile( string type, string response, double weight, double height, string gender,int age)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(p);
                FileStream fs = fileInfo.Create(); if (type == "anketa")
                {
                    fs.Write(Encoding.UTF8.GetBytes($"{weight}, {height}, {gender},{age}"));
                }
                if (type == "product")
                {
                    fs.Write(Encoding.UTF8.GetBytes($"{response}"));
                }
                fs.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}

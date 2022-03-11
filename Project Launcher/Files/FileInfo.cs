using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Launcher.Files
{
    internal class FileInfo
    {
        protected virtual string BaseURL => "https://drive.google.com/uc?export=download&id=";
        internal virtual string SharedLink { get => $"https:////drive.google.com//file/d//{Id}//view?usp=sharing"; }
        internal virtual string DownloadURL => System.IO.Path.Combine(BaseURL, Id);

        internal string Name => GetName(FileName);
        internal string FileName { get; private set; }
        internal object File => GetFile();

        internal string Id { get; private set; }
        internal string SubDirectory { get; private set; }
        internal string Path => System.IO.Path.Combine(Directory, FileName);
        internal string Directory => System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), SubDirectory);

        internal FileInfo(string _fileName, string _id = "", string _subPath = "")
        {
            FileName = _fileName;
            Id = _id;
            SubDirectory = _subPath;
        }

        protected static string GetName(string _fileName)
        {
            string[] s = _fileName.Split('.');
            return s[0];
        }

        protected object GetFile()
        {
            using (FileStream stream = new FileStream(Path, FileMode.Open))
            {
                return stream;
            }
        }
    }
}

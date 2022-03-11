using System;
using System.Collections.Generic;
using System.IO;

namespace Project_Launcher.Files
{
    internal static class FileManager
    {
        private static string AuthorizationFolderName = "AuthorizationFiles";
        private static string authorizationSubDirectory = "Authority";
        private static string BiosphereFolderName = "BiosphereGameFiles";
        private static string biosphereSubDirectory = "Bioshpere";

        private static Dictionary<string, FileFolder> Folder = new Dictionary<string, FileFolder>();

        public static void LoadFileInfo()
        {
            FileManager.Folder.Add(AuthorizationFolderName, new FileFolder());
            FileManager.Folder[AuthorizationFolderName].AddFileInfo(_name: "Credentials.json", _subPath: authorizationSubDirectory);

            FileManager.Folder.Add(BiosphereFolderName, new FileFolder());
            FileManager.Folder[BiosphereFolderName].AddFileInfo(_name: "Version.txt", _id: "1eo_Yj2srTwbziX8eGg4Ergd1ot34600C", _subPath: biosphereSubDirectory);
            FileManager.Folder[BiosphereFolderName].AddFileInfo(_name: "Biosphere.zip", _id: "1HxDQiT_XiGPukoitu1ybNe2ZHtZyV-L8", _subPath: biosphereSubDirectory);
            FileManager.Folder[BiosphereFolderName].AddFileInfo(_name: "Biosphere.exe", _subPath: Path.Combine(biosphereSubDirectory, "Biosphere"));
        }

        public static FileFolder GetFolder(string _folderName) => Folder[_folderName];
    }
}

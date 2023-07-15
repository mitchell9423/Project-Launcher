using System;
using System.Collections.Generic;
using System.IO;

namespace Project_Launcher.Files
{
    internal static class FileManager
    {
        private static Dictionary<EFile, string> _ID = new Dictionary<EFile, string>()
        {
            { EFile.Credentials, ""},
            { EFile.Version, "1eo_Yj2srTwbziX8eGg4Ergd1ot34600C"},
            { EFile.Biosphere, "1HxDQiT_XiGPukoitu1ybNe2ZHtZyV-L8"}
        };

        private static Dictionary<EFolder, FileFolder> Folder = new Dictionary<EFolder, FileFolder>();

        public static void LoadFileInfo()
        {
            FileFolder folder = new FileFolder();
            folder.AddFileInfo(_name: EFile.Credentials, ext: EExt.json, _subFolder: EFolder.Authority);
            Folder.Add(EFolder.AuthorizationFiles, folder);

            folder = new FileFolder();
            folder.AddFileInfo(_name: EFile.Version, ext: EExt.txt, _id: _ID[EFile.Version], _subFolder: EFolder.Bioshpere);
            folder.AddFileInfo(_name: EFile.Biosphere, ext: EExt.zip, _id: _ID[EFile.Biosphere], _subFolder: EFolder.Bioshpere);
            folder.AddFileInfo(_name: EFile.Biosphere, ext: EExt.exe, _subFolder: EFolder.Bioshpere);
            Folder.Add(EFolder.BiosphereGameFiles, folder);
        }

        public static FileFolder GetFolder(EFolder _folderName) => Folder[_folderName];
    }
}

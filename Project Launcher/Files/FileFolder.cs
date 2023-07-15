using System.Collections.Generic;
using System.IO;

namespace Project_Launcher.Files
{
    internal class FileFolder
    {
        private List<FileInfo> files = new List<FileInfo>();

        public void AddFileInfo(EFile _name, EExt ext = EExt.None, string _id = "", EFolder _subFolder = EFolder.None)
        {
            if (ext == EExt.exe)
            {
                files.Add(new FileInfo($"{_name}.{ext}", _id, $"{_subFolder}/{_name}"));
            }
			else
            {
                files.Add(new FileInfo($"{_name}.{ext}", _id, _subFolder.ToString()));
            }
        }

        public FileInfo GetFileInfo(EFile _name, EExt ext = EExt.None)
        {
            if (ext == EExt.None)
            {
                return files.Find((file) => file.Name == _name.ToString());
            }
			else
            {
                return files.Find((file) => file.FileName == $"{_name}.{ext}");
            }
        }
        public object GetFile(EFile _name) => GetFileInfo(_name).File;

        //// File Path Components
        //private const string VERSIONFILENAME = "Version.txt";
        //private const string ZIPPEDGAMEFILENAME = "Biosphere.zip";
        //internal static string CredentialsFileName { get => "Credentials.json"; }
        //private const string GAMEFOLDER = "Biosphere";
        //private const string GAMEEXENAME = "Biosphere.exe";

        //// URL Components
        //private const string GOOGLEDOMAIN = "https://drive.google.com";
        //private const string ACTIONURL = "/uc?export=download&id=";
        //internal static string GameFileId { get => "1HxDQiT_XiGPukoitu1ybNe2ZHtZyV-L8"; }
        //internal static string VersionFileId { get => "1eo_Yj2srTwbziX8eGg4Ergd1ot34600C"; }

        //// File Path Composite
        //internal static string CredentialsFilePath { get => Path.Combine(ApplicationRoot, CredentialsFileName); }
        //internal static string ApplicationRoot { get => Path.Combine(Directory.GetCurrentDirectory(), "Download"); }
        //internal static string VersionFilePath { get => Path.Combine(ApplicationRoot, VERSIONFILENAME); }
        //internal static string ZippedGamePath { get => Path.Combine(ApplicationRoot, ZIPPEDGAMEFILENAME); }
        //internal static string GameDirectory { get => Path.Combine(ApplicationRoot, GAMEFOLDER); }
        //internal static string GameExePath { get => Path.Combine(GameDirectory, GAMEEXENAME); }

        //// URL Composite
        //internal static string DirectGoogleLink { get => $"{GOOGLEDOMAIN}{ACTIONURL}"; }
        //internal static string LinkToOnlineVersionFile { get => $"{GOOGLEDOMAIN}{ACTIONURL}{VersionFileId}"; }
        //internal static string LinkToGameDownLoad { get => $"{GOOGLEDOMAIN}{ACTIONURL}{GameFileId}"; }
        //internal static string SharedGameLink { get => "https://drive.google.com/file/d/1HxDQiT_XiGPukoitu1ybNe2ZHtZyV-L8/view?usp=sharing"; }
    }
}

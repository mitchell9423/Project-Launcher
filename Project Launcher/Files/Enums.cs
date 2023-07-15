
namespace Project_Launcher
{
	enum LauncherStatus
	{
		Ready,
		Failed,
		Updating,
		Downloading
	}

    enum LauncherState
	{
        Main,
        Biosphere
	}
}


namespace Project_Launcher.Files
{
    public enum EFile
    {
        Biosphere,
        Credentials,
        Version
    }

    public enum EExt
    {
        json,
        txt,
        zip,
        exe,
        None
    }

    public enum EFolder
    {
        AuthorizationFiles,
        Authority,
        BiosphereGameFiles,
        Bioshpere,
        None
    }
}
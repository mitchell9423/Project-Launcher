
namespace Project_Launcher
{
	struct Version
	{
		internal static Version zero = new Version(0, 0, 0);

		private short major;
		private short minor;
		private short subMinor;

		internal Version(short _major, short _minor, short _subMinor)
		{
			major = _major;
			minor = _minor;
			subMinor = _subMinor;
		}

		internal Version(string _version)
		{
			string[] _versionString = _version.Split('.');
			if (_versionString.Length != 3)
			{
				major = 0;
				minor = 0;
				subMinor = 0;
				return;
			}

			major = short.Parse(_versionString[0]);
			minor = short.Parse(_versionString[1]);
			subMinor = short.Parse(_versionString[2]);
		}

		internal bool IsDifferentVersion(Version _otherVersion)
		{
			return major != _otherVersion.major ? minor != _otherVersion.minor ? subMinor != _otherVersion.subMinor : false : false;
		}

		public override string ToString()
		{
			return $"{major}.{minor}.{subMinor}";
		}
	}
}
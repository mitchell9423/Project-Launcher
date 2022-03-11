using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using System.Windows;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Download;
using Google.Apis.Util.Store;
using System.Threading.Tasks;
using Project_Launcher.Files;
using Project_Launcher.GoogleDrive;

namespace Project_Launcher
{
	enum LauncherStatus
	{
		Ready,
		Failed,
		Downloading
	}

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		//defined scope.
		private static string[] Scopes = { DriveService.Scope.DriveReadonly };

		internal LauncherStatus Status { get; private set; } = LauncherStatus.Downloading;

		private string playButtonText;
		private string versionText;

		private Files.FileInfo credentials;
		private Files.FileInfo versionFile;
		private Files.FileInfo gameZip;
		private Files.FileInfo gameExe;

		private void UpdateUIText()
		{
			do
			{
				PlayButton.Content = "Downloading...";
			} while (Status != LauncherStatus.Ready);

			PlayButton.Content = "Play Game";
			VersionText.Text = versionText;
		}

		public MainWindow()
		{
			InitializeComponent();
			LoadFileInfo();
		}

		private void LoadFileInfo()
		{
			FileManager.LoadFileInfo();

			FileFolder fileFolder = FileManager.GetFolder("AuthorizationFiles");
			credentials = fileFolder.GetFileInfo("Credentials");

			fileFolder = FileManager.GetFolder("BiosphereGameFiles");
			versionFile = fileFolder.GetFileInfo("VersionFile");
			gameZip = fileFolder.GetFileInfo("Biosphere.zip");
			gameExe = fileFolder.GetFileInfo("Biosphere.exe");
		}

		private void Window_ContentRendered(object sender, EventArgs e)
		{
			CheckForUpdates();
			UpdateUIText();
		}

		private void CheckForUpdates()
		{
			WebClient webClient = new WebClient();

			Version onlineVersion = new Version(webClient.DownloadString(versionFile.DownloadURL));

			if (File.Exists(versionFile.Path))
			{
				Version localVersion = new Version(File.ReadAllText(versionFile.Path));
				VersionText.Text = localVersion.ToString();

				try
				{
					if (onlineVersion.IsDifferentVersion(localVersion))
					{
						DownLoadGameFiles(gameZip.Id, true, onlineVersion);
					}
					else
					{
						Status = LauncherStatus.Ready;
					}
				}
				catch (Exception message)
				{
					Status = LauncherStatus.Failed;
					MessageBox.Show($"Check For Update Failed: {message}");
				}
			}
			else
			{
				DownLoadGameFiles(gameZip.Id, false, onlineVersion);
			}
		}

		private void DownLoadGameFiles(string fileId, bool isUpdate, Version _onlineVersion)
		{
			try
			{
				if (!isUpdate)
				{
					using (WebClient webClient = new WebClient())
					{
						_onlineVersion = new Version(webClient.DownloadString(versionFile.DownloadURL));
					}
				}

				FilesResource.GetRequest request = GetService().Files.Get(fileId);

				FileStream stream1 = new FileStream(gameZip.Path, FileMode.Create, FileAccess.ReadWrite);

				request.DownloadAsync(stream1);

				request.MediaDownloader.ProgressChanged += (IDownloadProgress progress) =>
				{
					switch (progress.Status)
					{
						case DownloadStatus.Completed:
							{
								DownloadCompletedCallback(_onlineVersion.ToString(), stream1);
								break;
							}
					}
				};


			}
			catch (Exception message)
			{
				Status = LauncherStatus.Failed;
				MessageBox.Show($"Install Failed: {message}");
			}
		}

		private async void DownloadCompletedCallback(string _onlineVersion, Stream stream1)
		{
			await Task.Run(() => stream1.Close());

			try
			{
				ZipFile.ExtractToDirectory(gameZip.Path, gameZip.Path);
				File.Delete(gameZip.Path);

				File.WriteAllText(versionFile.Path, _onlineVersion);
				Status = LauncherStatus.Ready;
				versionText = _onlineVersion;
			}
			catch (Exception message)
			{
				Status = LauncherStatus.Failed;
				MessageBox.Show($"Download Failed: {message}");
			}
		}

		//create Drive API service.
		public DriveService GetService()
		{
			//get Credentials from client_secret.json file 
			UserCredential credential;
			GetCredentials(out credential);

			//create Drive API service.
			DriveService service = new DriveService(new BaseClientService.Initializer()
			{
				HttpClientInitializer = credential,
				ApplicationName = "Project Launcher",
			});

			return service;
		}

		private void GetCredentials(out UserCredential credential)
		{
			using (var stream = new FileStream(credentials.Path, FileMode.Open, FileAccess.Read))
			{
				// The file token.json stores the user's access and refresh tokens, and is created
				// automatically when the authorization flow completes for the first time.
				string credPath = Path.Combine(credentials.Directory, "token.json");

				credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
					GoogleClientSecrets.FromStream(stream).Secrets,
					Scopes,
					"user",
					CancellationToken.None,
					new FileDataStore(credentials.Path, true)).Result;
				Console.WriteLine("Credential file saved to: " + credentials.Path);
			}
		}

		class MyWebClient : WebClient
		{
			CookieContainer c = new CookieContainer();

			protected override WebRequest GetWebRequest(Uri u)
			{
				var r = (HttpWebRequest)base.GetWebRequest(u);
				r.CookieContainer = c;
				return r;
			}
		}

		private void PlayButton_Click(object sender, RoutedEventArgs e)
		{
			if (File.Exists(gameExe.Path) && Status == LauncherStatus.Ready)
			{
				ProcessStartInfo startInfo = new ProcessStartInfo(gameExe.Path);
				startInfo.WorkingDirectory = gameExe.Directory;
				Process.Start(startInfo);
				Close();
			}
			else if (Status == LauncherStatus.Failed)
			{
				CheckForUpdates();
			}
		}
	}

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

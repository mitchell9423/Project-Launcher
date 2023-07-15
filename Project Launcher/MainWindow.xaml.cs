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
using System.Net.Http;
using System.ComponentModel;
using System.Windows.Threading;

namespace Project_Launcher
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private Files.FileInfo credentials;
		private Files.FileInfo versionFile;
		private Files.FileInfo gameZip;
		private Files.FileInfo gameExe;

		//defined scope.
		private static string[] Scopes = { DriveService.Scope.DriveReadonly };

		private LauncherState state = LauncherState.Main;
		internal LauncherState State
		{
			get { return state; }
			set
			{
				if (state != value)
				{
					state = value;
					ChangeState(value);
				}
			}
		}

		private LauncherStatus status = LauncherStatus.Updating;
		internal LauncherStatus Status
		{
			get { return status; }
			set
			{
				if (status != value)
				{
					status = value;
					UpdatePlayButtonText(value);
				}
			}
		}

		private string version = "0.0.0";
		internal string Version
		{
			get { return version; }
			set
			{
				if (version != value)
				{
					version = value;
					UpdateVersionText(value);
				}
			}
		}

		public MainWindow()
		{
			InitializeComponent();
			LoadFileInfo();
		}

		private void ChangeState(LauncherState _state)
		{
			Dispatcher.Invoke(() => {

				PlayButton.Visibility = _state == LauncherState.Main ? Visibility.Hidden : Visibility.Visible;
				ViewGames.Visibility = _state == LauncherState.Main ? Visibility.Hidden : Visibility.Visible;
				Biosphere.Visibility = _state == LauncherState.Main ? Visibility.Visible : Visibility.Hidden;

				switch (_state)
				{
					case LauncherState.Main:
						break;
					case LauncherState.Biosphere:
						GetFileInfo(EFolder.BiosphereGameFiles, EFile.Biosphere);
						Task.Run(() => CheckForUpdates());
						break;
					default:
						break;
				}

			});
		}

		private void UpdateVersionText(string _version)
		{
			Dispatcher.Invoke(() => {
				VersionText.Text = _version;
			});
		}

		private void UpdatePlayButtonText(LauncherStatus _status)
		{
			Dispatcher.Invoke(() => {
				switch (_status)
				{
					case LauncherStatus.Ready:
						PlayButton.Content = "Play Game";
						break;
					case LauncherStatus.Failed:
						break;
					case LauncherStatus.Downloading:
						PlayButton.Content = "Downloading...";
						break;
					default:
						break;
				}
			});
		}

		private void LoadFileInfo()
		{
			FileManager.LoadFileInfo();

			FileFolder fileFolder = FileManager.GetFolder(EFolder.AuthorizationFiles);
			credentials = fileFolder.GetFileInfo(EFile.Credentials);
		}

		private void GetFileInfo(EFolder folder, EFile file)
		{
			FileFolder fileFolder = FileManager.GetFolder(folder);
			versionFile = fileFolder.GetFileInfo(EFile.Version);
			gameZip = fileFolder.GetFileInfo(file, EExt.zip);
			gameExe = fileFolder.GetFileInfo(file, EExt.exe);
		}

		private void Window_ContentRendered(object sender, EventArgs e)
		{
			ChangeState(LauncherState.Main);
		}

		private async void CheckForUpdates()
		{
			HttpClient webClient  = new HttpClient();

			try
			{
				// Send HTTP request
				HttpResponseMessage response = await webClient.GetAsync(versionFile.DownloadURL);

				// Ensure we get a successful status code back
				response.EnsureSuccessStatusCode();

				byte[] fileContents = await response.Content.ReadAsByteArrayAsync();

				Console.WriteLine($"File downloaded successfully. Size: {fileContents.Length} bytes");

				Version onlineVersion = new Version(System.Text.Encoding.UTF8.GetString(fileContents));

				if (File.Exists(versionFile.Path))
				{
					Version localVersion = new Version(File.ReadAllText(versionFile.Path));

					Version = localVersion.ToString();

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
			catch (HttpRequestException e)
			{
				Console.WriteLine("\nException Caught!");
				Console.WriteLine("Message :{0} ", e.Message);
			}
		}

		private async void DownLoadGameFiles(string fileId, bool isUpdate, Version _onlineVersion)
		{
			try
			{
				if (!isUpdate)
				{
					using (HttpClient webClient = new HttpClient())
					{
						Status = LauncherStatus.Downloading;

						// Send HTTP request
						HttpResponseMessage response = await webClient.GetAsync(versionFile.DownloadURL);

						// Ensure we get a successful status code back
						response.EnsureSuccessStatusCode();

						byte[] fileContents = await response.Content.ReadAsByteArrayAsync();

						Console.WriteLine($"File downloaded successfully. Size: {fileContents.Length} bytes");

						_onlineVersion = new Version(System.Text.Encoding.UTF8.GetString(fileContents));
						//_onlineVersion = new Version(fileContents.ToString());
					}
				}

				FilesResource.GetRequest request = GetService().Files.Get(fileId);

				string directory = Path.GetDirectoryName(gameZip.Path);

				// Check if directory exists
				if (Directory.Exists(directory))
				{
					// Delete the directory and all of its contents
					Directory.Delete(directory, true);
				}

				// Create the directory if it doesn't exist
				Directory.CreateDirectory(directory);

				FileStream stream1 = new FileStream(gameZip.Path, FileMode.CreateNew, FileAccess.ReadWrite);

				request.MediaDownloader.ProgressChanged += (IDownloadProgress progress) =>
				{
					switch (progress.Status)
					{
						case DownloadStatus.Downloading:
							{
								Console.WriteLine($"Downloaded {progress.BytesDownloaded} bytes so far...");
							}
							break;
						case DownloadStatus.Completed:
							{
								Console.WriteLine("Download completed.");
								DownloadCompletedCallback(_onlineVersion.ToString(), stream1);
							}
							break;
					}
				};

				await request.DownloadAsync(stream1);

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
				ZipFile.ExtractToDirectory(gameZip.Path, Path.GetDirectoryName(gameZip.Path));
				File.Delete(gameZip.Path);

				File.WriteAllText(versionFile.Path, _onlineVersion);

				Status = LauncherStatus.Ready;

				Version = _onlineVersion;
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
					new FileDataStore(credPath, true)).Result;
				Console.WriteLine("Credential file saved to: " + credPath);
			}
			//new FileDataStore(credentials.Path, true)).Result;
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

		private void MenuButton_Click(object sender, RoutedEventArgs e)
		{
			if (e.Source is System.Windows.Controls.Button button)
			{

				Console.WriteLine(button.Name.ToString());

				switch (button.Name.ToString())
				{
					case "Biosphere":
						State = LauncherState.Biosphere;
						break;
					case "ViewGames":
						State = LauncherState.Main;
						break;
					default:
						break;
				}
			}
		}
	}
}

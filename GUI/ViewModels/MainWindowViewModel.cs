using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Threading;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using UT4UU.Installer.Common;

namespace UT4UU.Installer.GUI.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
	{
		private Options installOptions;
		private int selectedPageIndex;
		private ObservableCollection<LogMessage> logMessages;
		private string errorMessage;
		private OperationInstallation? operation;
		private double progress;
		private bool canExit;

		private bool cacheIsValidInstallLocation;
		private string? cacheUT4UUVersionText;

		public string InstallLocationWatermark
		{
			get
			{
				PlatformTarget platformTarget = (Environment.OSVersion.Platform == PlatformID.Unix) ? PlatformTarget.Linux : PlatformTarget.Win64;
				return System.IO.Path.Combine("<InstallLocation>", "Engine", "Binaries", platformTarget.ToString(),
					Helper.GetGameExecutableName(platformTarget, BuildConfiguration.Shipping));
			}
		}

		public string ActionButtonText
		{
			get
			{
				if (cacheUT4UUVersionText != null)
					return "Uninstall";
				return "Install";
			}
		}

		public string InstallLocation
		{
			get => installOptions.InstallLocation;
			set
			{
				SetInstallLocation(value);
			}
		}

		public bool IsValidInstallLocation
		{
			get => cacheIsValidInstallLocation;
		}

		public bool CanCustomizeAction
		{
			get => cacheIsValidInstallLocation && cacheUT4UUVersionText == null;
		}

		public bool CanCustomizeSuffix
		{
			get => CanCustomizeAction && (RefreshingExperience || UpgradeEngineModules);
		}

		public bool IsDryRun
		{
			get => installOptions.IsDryRun;
			set
			{
				installOptions.IsDryRun = value;
				this.RaisePropertyChanged();
			}
		}

		public bool UpgradeEngineModules
		{
			get => installOptions.UpgradeEngineModules;
			set
			{
				installOptions.UpgradeEngineModules = value;
				this.RaisePropertyChanged();
				this.RaisePropertyChanged("CanCustomizeSuffix");
			}
		}

		public bool RefreshingExperience
		{
			get => installOptions.RefreshingExperience;
			set
			{
				installOptions.RefreshingExperience = value;
				this.RaisePropertyChanged();
				this.RaisePropertyChanged("CanCustomizeSuffix");
			}
		}

		public bool TryToInstallInLocalGameServer
		{
			get => installOptions.TryToInstallInLocalGameServer;
			set
			{
				installOptions.TryToInstallInLocalGameServer = value;
				this.RaisePropertyChanged();
			}
		}

		public string ReplacementSuffix
		{
			get => installOptions.ReplacementSuffix;
			set
			{
				installOptions.ReplacementSuffix = value;
				this.RaisePropertyChanged();
			}
		}

		public bool CreateDesktopShortcut
		{
			get => installOptions.CreateShortcut;
			set
			{
				installOptions.CreateShortcut = value;
				this.RaisePropertyChanged();
			}
		}

		public int SelectedPageIndex
		{
			get => selectedPageIndex;
			set
			{
				this.RaiseAndSetIfChanged(ref selectedPageIndex, value);
				this.RaisePropertyChanged("IsOperationInProgress");
			}
		}

		public string BuildTypeText
		{
			get
			{
				if (!IsValidInstallLocation)
					return "-";

				// create a user-friendly description
				string ret = string.Empty;
				switch (installOptions.PlatformTarget)
				{
					case PlatformTarget.Win64:
						ret += "Windows";
						break;
					default:
						ret += installOptions.PlatformTarget.ToString();
						break;
				}
				ret += " ";
				switch (installOptions.BuildConfiguration)
				{
					case BuildConfiguration.Shipping:
						ret += "Game";
						break;
					case BuildConfiguration.ShippingServer:
						ret += "Server";
						break;
					case BuildConfiguration.DevelopmentEditor:
						ret += "Editor";
						break;
					default:
						ret += installOptions.BuildConfiguration.ToString();
						break;
				}
				return ret;
			}
		}

		public string UT4UUVersionText
		{
			get
			{
				if (cacheUT4UUVersionText != null)
					return cacheUT4UUVersionText;
				return "-";
			}
		}

		public string ErrorMessage
		{
			get => errorMessage;
			set => this.RaiseAndSetIfChanged(ref errorMessage, value);
		}

		public bool IsOperationInProgress
		{
			get => selectedPageIndex == 2;
		}

		public double Progress
		{
			get
			{
				return progress;
			}
		}

		public ObservableCollection<LogMessage> LogMessages
		{
			get => logMessages;
		}

		public bool CanExit
		{
			get => canExit;
			set => this.RaiseAndSetIfChanged(ref canExit, value);
		}

		private bool isInstallationSuccessful;

		public string AfterInstallTitle
		{
			get
			{
				return $"{(cacheUT4UUVersionText == null ? "I" : "Uni")}nstallation {(isInstallationSuccessful ? "successful" : "failed")}!";
			}
		}

		public string AfterInstallParagraph1
		{
			get
			{
				if (isInstallationSuccessful)
				{
					if (cacheUT4UUVersionText == null)
					{
						return $"If you are going to uninstall Unreal Tournament 4, please make sure to first uninstall UT4UU with this installer.";
					}
					else
					{
						return $"We are sad to see you leave. Unless... you are just updating.";
					}
				}
				else
				{
					return $"Something went wrong while {(cacheUT4UUVersionText == null ? "" : "un")}installing.";
				}
			}
		}

		public string AfterInstallParagraph2
		{
			get
			{
				if (isInstallationSuccessful)
				{
					if (cacheUT4UUVersionText == null)
					{
						return $"If you don't do that, some files might remain on your machine without your knowledge after {BuildTypeText} uninstallation.";
					}
					else
					{
						return $"Either way, thank you for using UT4UU {cacheUT4UUVersionText}.";
					}
				}
				else
				{
					return $"You may open log file which should contain information of what went wrong.";
				}
			}
		}

		public string AfterInstallParagraph3
		{
			get
			{
				if (isInstallationSuccessful)
				{
					if (cacheUT4UUVersionText == null)
					{
						return $"To uninstall, run this installer again and enter the location where UT4UU was previously installed.";
					}
					else
					{
						return $"Goodbye o/";
					}
				}
				else
				{
					return $"If you cannot figure it out, feel free to ask for help in our discord server.";
				}
			}
		}

		public void SwitchToPage(string index)
		{
			SelectedPageIndex = int.Parse(index);
			if (SelectedPageIndex == 2)
			{
				Dispatcher.UIThread.Post(async () =>
				{
					await System.Threading.Tasks.Task.Run(StartProperOperation);
				}
				, DispatcherPriority.Background);
			}
			else if (SelectedPageIndex == 3)
			{
				this.RaisePropertyChanged("AfterInstallTitle");
				this.RaisePropertyChanged("AfterInstallParagraph1");
				this.RaisePropertyChanged("AfterInstallParagraph2");
				this.RaisePropertyChanged("AfterInstallParagraph3");
			}
		}

		public static bool OpenFile(string filepath)
		{
			return RunProcess("file:///" + (System.IO.Path.IsPathRooted(filepath) ? filepath : System.IO.Path.Combine(Environment.CurrentDirectory, filepath)));
		}

		public static bool RunProcess(string uri)
		{
			Process? p;
			try
			{
				p = Process.Start(uri);
			}
			catch (Exception ex)
			{
				Trace.WriteLine(ex);

				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				{
					uri = uri.Replace("&", "^&");
					p = Process.Start(new ProcessStartInfo("cmd", $"/c \"start {uri}\"") { CreateNoWindow = true });
				}
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
				{
					// untested
					p = Process.Start("xdg-open", uri);
				}
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				{
					// untested
					p = Process.Start("open", uri);
				}
				else
				{
					return false;
				}
			}

			p?.Dispose();
			return true;
		}

		private StreamWriter? logFileStream;

		private class PerOperationDepthInfo
		{
			public int TaskIndex { get; set; }
			public int TaskCount { get; set; }

		}
		private List<PerOperationDepthInfo> progressPerOperationDepth;

		public MainWindowViewModel()
		{
			isInstallationSuccessful = false;
			errorMessage = string.Empty;
			byte[] buffer = new byte[1024 * 1024];
			logMessages = new ObservableCollection<LogMessage>();
			progressPerOperationDepth = new();
			CanExit = true;

			installOptions = new Options();
			installOptions.UpgradeEngineModules = true;
			installOptions.CreateShortcut = true;
			installOptions.RefreshingExperience = true;
			installOptions.TryToInstallInLocalGameServer = true;
			installOptions.Logger = (object? sender, Options.LogEventArgs e) =>
			{
				// remove exccess depth
				while (progressPerOperationDepth.Count > e.OperationDepth + 1)
				{
					progressPerOperationDepth.RemoveAt(progressPerOperationDepth.Count - 1);
				}
				// add multi depth steps
				while (progressPerOperationDepth.Count < e.OperationDepth)
				{
					progressPerOperationDepth.Add(new() { TaskIndex = 0, TaskCount = 1 });
				}
				// add current depth
				if (progressPerOperationDepth.Count == e.OperationDepth)
				{
					progressPerOperationDepth.Add(new());
				}
				// update current depth info
				progressPerOperationDepth[e.OperationDepth].TaskIndex = e.TaskIndex;
				progressPerOperationDepth[e.OperationDepth].TaskCount = e.TaskCount;

				// calculate current progress
				progress = 0;
				for (int i = 0; i < progressPerOperationDepth.Count; i++)
				{
					double depthProgress = progressPerOperationDepth[i].TaskIndex / (double)progressPerOperationDepth[i].TaskCount;
					if (i > 0)
						depthProgress *= 1.0 / progressPerOperationDepth[i - 1].TaskCount;
					progress += depthProgress;
				}

				// poor man's error message detection
				string messageLower = e.Message.ToLower();
				bool isError =
					messageLower.Contains("fail") ||
					messageLower.Contains("warning") ||
					messageLower.Contains("error") ||
					messageLower.Contains("abort") ||
					messageLower.Contains("cannot") ||
					messageLower.Contains("can't") ||
					messageLower.Contains("could not");


				for (int i = 0; i < e.OperationDepth; i++)
				{
					e.Message = "    " + e.Message;
				}

				if (isError)
				{
					logFileStream?.Write($"[{DateTime.UtcNow}] Err: ");
					logMessages.Add(new LogMessageError(e.Message));
					logFileStream?.WriteLine(e.Message);
				}
				else
				{
					logFileStream?.Write($"[{DateTime.UtcNow}] Log: ");
					logMessages.Add(new LogMessageInfo(e.Message));
					logFileStream?.WriteLine(e.Message);
				}

				this.RaisePropertyChanged("Progress");
				this.RaisePropertyChanged("LogMessages");
			};


			var assembly = Assembly.GetEntryAssembly();
			if (assembly != null)
				installOptions.SourceLocation = System.IO.Path.GetDirectoryName(assembly.Location) ?? string.Empty;
			installOptions.SourceLocation = System.IO.Path.Combine(installOptions.SourceLocation, "Files");
			SetInstallLocation(Helper.TryFindInstallationLocation() ?? string.Empty);

#if DEBUG
			installOptions.SourceLocation = "E:\\UT4Source\\UT4UU\\Source\\Programs\\Installer\\Files";
			SetInstallLocation("E:\\UT4Source\\TestGameDir");
#endif
		}

		private void SetInstallLocation(string location)
		{
			// clear any error message
			ErrorMessage = string.Empty;

			// change install location
			installOptions.InstallLocation = location;
			PlatformTarget platform = PlatformTarget.Unknown;
			BuildConfiguration config = BuildConfiguration.Unknown;
			if (Helper.DetectInstallationType(installOptions.InstallLocation, ref platform, ref config))
			{
				installOptions.PlatformTarget = platform;
				installOptions.BuildConfiguration = config;
			}

			cacheIsValidInstallLocation = Helper.IsUTDirectory(
				installOptions.InstallLocation,
				installOptions.PlatformTarget,
				installOptions.BuildConfiguration
			);
			cacheUT4UUVersionText = Helper.GetUT4UUInstalledVersionName(installOptions.InstallLocation);

			this.RaisePropertyChanged("InstallLocation");
			this.RaisePropertyChanged("IsValidInstallLocation");
			this.RaisePropertyChanged("CanCustomizeAction");
			this.RaisePropertyChanged("CanCustomizeSuffix");
			this.RaisePropertyChanged("ActionButtonText");
			this.RaisePropertyChanged("BuildTypeText");
			this.RaisePropertyChanged("UT4UUVersionText");
		}

		private void StartProperOperation()
		{
			if (!IsValidInstallLocation)
			{
				ErrorMessage = $"Invalid install location";
				SelectedPageIndex = 0;
				return;
			}

			CanExit = false;
			logFileStream = new StreamWriter(new FileStream("LastInstallation.log", FileMode.Append, FileAccess.Write, FileShare.Read));
			logFileStream.WriteLine("-------------------- START OF LOG --------------------");
			installOptions.WriteOptions(logFileStream);

			if (Helper.IsUT4UUInstalled(installOptions.InstallLocation))
			{
				bool isHandlableUT4UUInstalled = Helper.IsExpectedUT4UUVersionInstalled(
					installOptions.InstallLocation,
					installOptions.SourceLocation
				);
				if (isHandlableUT4UUInstalled)
				{
					operation = new OperationUninstall(installOptions);
					try
					{
						operation.Do();
						isInstallationSuccessful = true;
					}
					catch
					{
						isInstallationSuccessful = false;
					}
				}
				else
				{
					string? installedVersion = Helper.GetUT4UUInstalledVersionName(installOptions.InstallLocation);
					string? sourceVersion = Helper.GetUT4UUInstalledVersionName(installOptions.SourceLocation);
					if (installedVersion == null)
						installedVersion = "<Unknown>";
					if (sourceVersion == null)
						sourceVersion = "<Unknown>";

					ErrorMessage = $"This installer can only uninstall version '{sourceVersion}' of UT4UU";
					SelectedPageIndex = 0;
				}
			}
			else
			{
				bool isUTDir = Helper.IsUTDirectory(
					installOptions.InstallLocation,
					installOptions.PlatformTarget,
					installOptions.BuildConfiguration
				);
				if (isUTDir)
				{
					// begin to install
					operation = new OperationInstall(installOptions);
					try
					{
						operation.Do();
						isInstallationSuccessful = true;
					}
					catch
					{
						isInstallationSuccessful = false;
					}
				}
				else
				{
					ErrorMessage = $"Tried to install into non-UT4 related directory";
					SelectedPageIndex = 0;
				}
			}

			logFileStream.Dispose();
			logFileStream = null;

			operation = null;
			CanExit = true;
		}
	}
}

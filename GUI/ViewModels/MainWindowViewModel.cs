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
using UT4UU.Installer.Common;
using UT4UU.Installer.GUI.Models;

namespace UT4UU.Installer.GUI.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
	{
		private Options installOptions;
		private int selectedPageIndex;
		private ObservableCollection<string> logMessages;
		private string errorMessage;
		private OperationInstallation? operation;
		private double progress;

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
				if (Helper.IsUT4UUInstalled(installOptions.InstallLocation))
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
				return installOptions.PlatformTarget.ToString() + ", " + installOptions.BuildConfiguration.ToString();
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

		public ObservableCollection<string> LogMessages
		{
			get => logMessages;
		}

		public bool IsProgressTextShown
		{
			get => true;
		}

		public void SwitchToPage(string index)
		{
			SelectedPageIndex = int.Parse(index);
			if (SelectedPageIndex == 2)
			{
				Dispatcher.UIThread.Post(() => StartProperOperation(), DispatcherPriority.Background);
			}
		}


		public MainWindowViewModel()
		{
			errorMessage = string.Empty;
			byte[] buffer = new byte[1024 * 1024];
			logMessages = new ObservableCollection<string>();

			installOptions = new Options();
			installOptions.UpgradeEngineModules = true;
			installOptions.CreateShortcut = true;
			installOptions.Logger = (string message, int taskIndex, int taskCount) =>
			{
				progress = taskIndex / (double)taskCount;
				logMessages.Add(message);

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
			this.RaisePropertyChanged("ActionButtonText");
			this.RaisePropertyChanged("BuildTypeText");
			this.RaisePropertyChanged("UT4UUVersionText");
		}

		private async void StartProperOperation()
		{
			if (!IsValidInstallLocation)
			{
				ErrorMessage = $"Invalid install location";
				SelectedPageIndex = 0;
				return;
			}

			if (Helper.IsUT4UUInstalled(installOptions.InstallLocation))
			{
				bool isHandlableUT4UUInstalled = Helper.IsExpectedUT4UUVersionInstalled(
					installOptions.InstallLocation,
					installOptions.SourceLocation
				);
				if (installOptions.IsDryRun || isHandlableUT4UUInstalled)
				{
					await Dispatcher.UIThread.InvokeAsync(() => PerformAction(false), DispatcherPriority.Background);
				}
				else
				{
					string? installedVersion = Helper.GetUT4UUInstalledVersionName(installOptions.InstallLocation);
					string? sourceVersion = Helper.GetUT4UUInstalledVersionName(installOptions.SourceLocation);
					if (installedVersion != null)
						installedVersion = "<Unknown>";
					if (sourceVersion != null)
						sourceVersion = "<Unknown>";

					ErrorMessage = $"This installer can only uninstall UT4UU {sourceVersion}";
					SelectedPageIndex = 0;
					return;
				}
			}
			else
			{
				bool isUTDir = Helper.IsUTDirectory(
					installOptions.InstallLocation,
					installOptions.PlatformTarget,
					installOptions.BuildConfiguration
				);
				if (installOptions.IsDryRun || isUTDir)
				{
					await Dispatcher.UIThread.InvokeAsync(() => PerformAction(true), DispatcherPriority.Background);
				}
				else
				{
					ErrorMessage = $"";
					SelectedPageIndex = 0;
					return;
				}
			}
		}

		private void PerformAction(bool isInstallation)
		{
			operation = new OperationInstallation(installOptions, isInstallation);
			operation.Do();
		}
	}
}

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using ReactiveUI;
using System;
using System.Reflection;
using UT4UU.Installer.Common;

namespace UT4UU.Installer.GUI.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
	{
		private Options installOptions;
		private int selectedPageIndex;

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
				installOptions.InstallLocation = value;
				this.RaisePropertyChanged();
			}
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


		public int SelectedPageIndex
		{
			get => selectedPageIndex;
			set => this.RaiseAndSetIfChanged(ref selectedPageIndex, value);
		}


		public MainWindowViewModel()
		{
			installOptions = new Options();
			installOptions.UpgradeEngineModules = true;

			var assembly = Assembly.GetEntryAssembly();
			if (assembly != null)
				installOptions.SourceLocation = System.IO.Path.GetDirectoryName(assembly.Location) ?? string.Empty;
			SetInstallLocation(Helper.TryFindInstallationLocation());
		}

		private void SetInstallLocation(string? location)
		{
			if (string.IsNullOrWhiteSpace(location))
				return;

			installOptions.InstallLocation = location;
			PlatformTarget platform = PlatformTarget.Unknown;
			BuildConfiguration config = BuildConfiguration.Unknown;
			if (Helper.DetectInstallationType(installOptions.InstallLocation, ref platform, ref config))
			{
				installOptions.PlatformTarget = platform;
				installOptions.BuildConfiguration = config;
			}
		}

		public void SwitchToPage(string index)
		{
			SelectedPageIndex = int.Parse(index);
		}
	}
}

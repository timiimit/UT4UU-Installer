using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using ReactiveUI;
using System;
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

		public string InstallLocation { get; set; } = Helper.TryFindInstallationLocation() ?? string.Empty;

		public bool IsDryRun
		{
			get => installOptions.IsDryRun;
			set => installOptions.IsDryRun = value;
		}

		public bool UpgradeEngineModules
		{
			get => installOptions.UpgradeEngineModules;
			set => installOptions.UpgradeEngineModules = value;
		}

		public string ReplacementSuffix
		{
			get => installOptions.ReplacementSuffix;
			set => installOptions.ReplacementSuffix = value;
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
		}

		public void SwitchToPage(string index)
		{
			SelectedPageIndex = int.Parse(index);
		}
	}
}

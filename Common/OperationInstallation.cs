using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UT4UU.Installer.Common
{
	public class OperationInstallation : Operation
	{
		public OperationInstallation(Options options, bool isInstalling, int depth = 0) : base("Install UT4UU", "Uninstall UT4UU", depth, options, isInstalling)
		{
			string srcEngine = Path.Combine(options.SourceLocation, "Engine", "Binaries", options.PlatformTarget.ToString());
			string srcPluginRoot = Path.Combine(options.SourceLocation, "UnrealTournament", "Plugins", "UT4UU");
			string srcPlugin = Path.Combine(srcPluginRoot, "Binaries", options.PlatformTarget.ToString());
			string srcContent = Path.Combine(options.SourceLocation, "UnrealTournament", "Content");
			string srcMovies = Path.Combine(srcContent, "Movies");
			string srcSplash = Path.Combine(srcContent, "Splash");

			string dstEngine = Path.Combine(options.InstallLocation, "Engine", "Binaries", options.PlatformTarget.ToString());
			string dstPluginRoot = Path.Combine(options.InstallLocation, "UnrealTournament", "Plugins", "UT4UU");
			string dstPlugin = Path.Combine(dstPluginRoot, "Binaries", options.PlatformTarget.ToString());
			string dstContent = Path.Combine(options.InstallLocation, "UnrealTournament", "Content");
			string dstMovies = Path.Combine(dstContent, "Movies");
			string dstSplash = Path.Combine(dstContent, "Splash");

			tasks = new List<Task>();

			// create needed tree structure
			tasks.Add(new TaskCreateDirectory(dstPluginRoot));
			tasks.Add(new TaskCreateDirectory(Path.Combine(dstPluginRoot, "Binaries")));
			tasks.Add(new TaskCreateDirectory(dstPlugin));

			// copy plugin files
			AddCopyTask(srcPluginRoot, dstPluginRoot, "UT4UU.uplugin");
			AddCopyTask(srcPlugin, dstPlugin, GetModuleName("UT4UU"));
			AddCopyTask(srcPlugin, dstPlugin, GetModuleName("UT4UUHelper"));
			AddCopyTask(srcPlugin, dstPlugin, GetModuleName("Funchook"));

			if (options.UpgradeEngineModules)
			{
				AddReplaceModuleTask(srcEngine, dstEngine, "SSL");
				AddReplaceModuleTask(srcEngine, dstEngine, "HTTP");
				AddReplaceModuleTask(srcEngine, dstEngine, "XMPP");
				AddReplaceModuleTask(srcEngine, dstEngine, "HttpNetworkReplayStreaming");
			}

			if (options.CreateShortcut)
			{
				string exeName = Helper.GetGameExecutableName(options.PlatformTarget, options.BuildConfiguration);

				// defaults
				string name = "Unreal Tournament 4 UU";
				string arguments = "UnrealTournament";
				string description = "Run Unreal Tournament 4";

				// specialized per build configuration
				switch (options.BuildConfiguration)
				{
					case BuildConfiguration.Shipping: // standard game arguments
						name = "Unreal Tournament 4 UU";
						arguments = "UnrealTournament -epicapp=UnrealTournamentDev -epicenv=Prod -EpicPortal";
						description = "Run Unreal Tournament 4 as if it was ran from Epic Games Launcher";
						break;

					case BuildConfiguration.ShippingServer: // standard hub arguments
						name = "Unreal Tournament 4 UU Hub";
						arguments = "UnrealTournament UT-Entry?Game=Lobby -Log";
						description = "Run Unreal Tournament 4 Hub";
						break;

					case BuildConfiguration.DevelopmentEditor:
						name = "Unreal Tournament 4 UU Editor";
						break;
				}

				string fullpath = Path.Combine(dstEngine, exeName);
				tasks.Add(new TaskCreateDesktopShortcut(fullpath, arguments,name,description) { CanFail = true });
			}

			if (options.RefreshingExperience && options.BuildConfiguration == BuildConfiguration.Shipping)
			{
				tasks.Add(new TaskRenameFile(
					Path.Combine(dstMovies, "engine_startup.mp4"),
					"engine_startup.mp4" + Options.ReplacementSuffix
				));
				tasks.Add(new TaskRenameFile(
					Path.Combine(dstMovies, "intro_full.mp4"),
					"engine_startup.mp4"
				));
				tasks.Add(new TaskRenameFile(
					Path.Combine(dstSplash, "Splash.bmp"),
					"Splash.bmp" + options.ReplacementSuffix
				));
				AddCopyTask(srcSplash, dstSplash, "Splash.bmp");
			}

			if (options.TryToInstallInLocalGameServer && options.BuildConfiguration == BuildConfiguration.Shipping)
			{
				// TODO: heavy debugging needed
				Options subOptions = options;
				subOptions.TryToInstallInLocalGameServer = false;
				subOptions.CreateShortcut = false;

				if (options.PlatformTarget == PlatformTarget.Win64)
					subOptions.InstallLocation = Path.Combine(options.InstallLocation, "WindowsServer");
				else if (options.PlatformTarget == PlatformTarget.Linux)
					subOptions.InstallLocation = Path.Combine(options.InstallLocation, "LinuxServer");

				PlatformTarget pt = PlatformTarget.Unknown;
				BuildConfiguration bc = BuildConfiguration.Unknown;
				if (Helper.DetectInstallationType(subOptions.InstallLocation, ref pt, ref bc) && bc == BuildConfiguration.ShippingServer)
				{
					subOptions.PlatformTarget = pt;
					subOptions.BuildConfiguration = BuildConfiguration.ShippingServer;
					var subInstallation = new OperationInstallation(subOptions, true, OperationDepth + 1) { CanFail = true };
					tasks.Add(subInstallation);

					// don't think we can unwrap the sub installation due to this whole operation being optional.
					// meaning, we don't want to abort installation if we fail to install into local server.

					//subInstallation.DescriptionDo = "Install UT4UU into local game server";
					//// we want to unwrap tasks out, because this way caller
					//// can know the whole number of tasks.
					//for (int i = 0; i < subInstallation.tasks.Count; i++)
					//{
					//	tasks.Add(subInstallation.tasks[i]);
					//}
				}
			}

			tasks.Add(new TaskCreateInstallInfoFile(options));
		}

		private string GetModuleName(string moduleName)
		{
			return Helper.GetActualModuleName(moduleName, Options.PlatformTarget, Options.BuildConfiguration);
		}

		private void AddReplaceModuleTask(string srcDir, string targetDir, string moduleName)
		{
			if (string.IsNullOrWhiteSpace(Options.ReplacementSuffix))
				throw new ArgumentOutOfRangeException(nameof(Options.ReplacementSuffix));

			string filename = GetModuleName(moduleName);
			tasks.Add(new TaskRenameFile(
				Path.Combine(targetDir, filename),
				filename + Options.ReplacementSuffix
			));
			AddCopyTask(srcDir, targetDir, filename);
		}

		private void AddCopyTask(string srcDir, string dstDir, string filename)
		{
			if (Options.CreateSymbolicLinks)
			{
				tasks.Add(new TaskCreateSymbolicLink(Path.Combine(dstDir, filename), Path.Combine(srcDir, filename)));
			}
			else
			{
				tasks.Add(new TaskCopyFile(Path.Combine(srcDir, filename), Path.Combine(dstDir, filename)));
			}
		}
	}
}

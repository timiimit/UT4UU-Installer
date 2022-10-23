using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UT4UU.Installer.Common
{
	public class OperationInstallation : Operation
	{
		public OperationInstallation(Options options, bool isInstalling) : base("Install UT4UU", "Uninstall UT4UU", options, isInstalling)
		{
			string srcEngine = Path.Combine(options.SourceLocation, "Engine", "Binaries", options.PlatformTarget.ToString());
			string srcPluginRoot = Path.Combine(options.SourceLocation, "UnrealTournament", "Plugins", "UT4UU");
			string srcPlugin = Path.Combine(srcPluginRoot, "Binaries", options.PlatformTarget.ToString());

			string dstEngine = Path.Combine(options.InstallLocation, "Engine", "Binaries", options.PlatformTarget.ToString());
			string dstPluginRoot = Path.Combine(options.InstallLocation, "UnrealTournament", "Plugins", "UT4UU");
			string dstPlugin = Path.Combine(dstPluginRoot, "Binaries", options.PlatformTarget.ToString());

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
				// TODO: Environment.SpecialFolder.Desktop might not get correct value each time
				string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop, Environment.SpecialFolderOption.None);
				if (!string.IsNullOrEmpty(desktop))
				{
					string exeName = Helper.GetGameExecutableName(options.PlatformTarget, options.BuildConfiguration);
					tasks.Add(new TaskCreateDesktopShortcut(
						Path.Combine(dstEngine, exeName),
						"UnrealTournament -epicapp=UnrealTournamentDev -epicenv=Prod -EpicPortal",
						"Unreal Tournament 4 UU",
						"Run Unreal Tournament 4 as if it was ran from Epic Games Launcher"
					) { CanFail = true });
				}
			}

			AddCopyTask(srcPluginRoot, dstPluginRoot, "InstallInfo.bin");
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
			tasks.Add(new TaskMoveFile(
				Path.Combine(targetDir, filename),
				Path.Combine(targetDir, filename + Options.ReplacementSuffix)
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

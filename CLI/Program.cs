

using System.Reflection;
using UT4UU.Installer.Common;

namespace UT4UU.Installer.CLI
{
	internal class Program
	{
		static void PrintHelp()
		{
			Console.WriteLine($@"Syntax:
	UT4UU.Installer.CLI Install
	UT4UU.Installer.CLI Uninstall [UTInstallPath]
");
		}

		private static int ParseArguments(string[] args, ref Options options)
		{
			//options.InstallLocation = Helper.TryFindInstallationLocation() ?? string.Empty;
			options.SourceLocation = Assembly.GetEntryAssembly()?.Location ?? string.Empty;
			options.Logger = new StreamWriter(Console.OpenStandardOutput());

#if DEBUG
			options.IsDryRun = true; // for debugging
			options.InstallLocation = "E:/UT4Source/TestGameDir";
			options.SourceLocation = "E:/UT4Source/UT4UU/Source/Programs/Installer/Files";
#endif

			for (int i = 0; i < args.Length; i++)
			{
				string arg = args[i].ToLower();
				if (arg == "-d" || arg == "--dry-run")
					options.IsDryRun = true;
				if (arg == "-s" || arg == "--symbolic-links")
					options.CreateSymbolicLinks = true;
				if (arg == "-u" || arg == "--upgrade-engine-modules")
					options.UpgradeEngineModules = true;
				if (arg == "-i" || arg == "--install-path")
					options.InstallLocation = args[++i];
				if (arg == "--source-path")
					options.SourceLocation = args[++i];
				if (arg == "--replacement-suffix")
					options.ReplacementSuffix = args[++i];
				if (arg == "--platform-target")
					options.PlatformTarget = (PlatformTarget)Enum.Parse(typeof(PlatformTarget), args[++i]);
				if (arg == "--build-configuration")
					options.BuildConfiguration = (BuildConfiguration)Enum.Parse(typeof(BuildConfiguration), args[++i]);
			}

			string detectedInstallLocation = Helper.TryFindInstallationLocation() ?? string.Empty;
			if (string.IsNullOrWhiteSpace(options.InstallLocation))
			{
				if (string.IsNullOrWhiteSpace(detectedInstallLocation))
				{
					options.Logger.WriteLine("Failed to find install location. Please specify it manually.");
					return 1;
				}
				options.InstallLocation = detectedInstallLocation;
			}
			//else
			//{
			//	if (!string.IsNullOrWhiteSpace(detectedInstallLocation))
			//	{
			//		if (detectedInstallLocation != options.InstallLocation)
			//		{
			//			options.Logger.WriteLine("Detected install location differs from specified one.");
			//		}
			//	}
			//}


			if (options.PlatformTarget == PlatformTarget.Unknown && options.BuildConfiguration != BuildConfiguration.Unknown)
			{
				options.Logger.WriteLine("PlatformTarget is unknown.");
				return 1;
			}
			if (options.PlatformTarget != PlatformTarget.Unknown && options.BuildConfiguration == BuildConfiguration.Unknown)
			{
				options.Logger.WriteLine("BuildConfiguration is unknown.");
				return 1;
			}
			if (options.PlatformTarget == PlatformTarget.Unknown && options.BuildConfiguration == BuildConfiguration.Unknown)
			{
				PlatformTarget pt = PlatformTarget.Unknown;
				BuildConfiguration bc = BuildConfiguration.Unknown;
				if (Helper.DetectInstallationType(options.InstallLocation, ref pt, ref bc))
				{
					options.PlatformTarget = pt;
					options.BuildConfiguration = bc;
				}
				else
				{
					options.Logger.WriteLine("Failed to detect PlatformTarget and BuildConfiguration.");
					return 1;
				}
			}
			if (options.PlatformTarget != PlatformTarget.Unknown && options.BuildConfiguration != BuildConfiguration.Unknown)
			{
				PlatformTarget pt = PlatformTarget.Unknown;
				BuildConfiguration bc = BuildConfiguration.Unknown;
				if (Helper.DetectInstallationType(options.InstallLocation, ref pt, ref bc))
				{
					if (pt != options.PlatformTarget)
					{
						options.Logger.WriteLine($"WARNING: Detected PlatformTarget ({pt}) does not match the specified one");
					}
					if (bc != options.BuildConfiguration)
					{
						options.Logger.WriteLine($"WARNING: Detected BuildConfiguration ({bc}) does not match the specified one");
					}
				}
				else
				{
					options.Logger.WriteLine("WARNING: Failed to detect PlatformTarget and BuildConfiguration.");
				}
			}

			return 0;
		}

		private static string GetInstallInfoFile(string installLocation)
		{
			return Path.Combine(installLocation, "UnrealTournament", "Plugins", "UT4UU", "InstallInfo.bin");
		}

		static int Main(string[] args)
		{
			if (args[0].ToLower() == "install")
			{
				Options options = new Options();

				string[] optionArguments = new string[args.Length - 1];
				Array.Copy(args, 1, optionArguments, 0, optionArguments.Length);

				int result = ParseArguments(optionArguments, ref options);
				if (result != 0)
					return result;

				// create install info file which will be copied to installed plugin directory
				if (!options.IsDryRun)
					options.Save(GetInstallInfoFile(options.SourceLocation));

				OperationInstall op = new OperationInstall(options);
				op.Do();


			}
			else if (args[0].ToLower() == "uninstall")
			{
				string installLocation;
				if (args.Length <= 1)
				{
					string? detectedInstallLocation = Helper.TryFindInstallationLocation();
					if (detectedInstallLocation == null)
					{
						Console.WriteLine("Failed to find install location. Please specify it manually.");
						return 1;
					}
					installLocation = detectedInstallLocation;
				}
				else
				{
					installLocation = args[1];
				}


				var options = Options.Load(GetInstallInfoFile(installLocation));

				OperationUninstall op = new OperationUninstall(options);
				op.Do();
			}
			else
			{
				PrintHelp();
				return 1;
			}

			


			//options.Logger.WriteLine($"Using PlatformTarget = {options.PlatformTarget} and BuildConfiguration = {options.BuildConfiguration}");



			return 0;
		}
	}
}


using System.Reflection;
using UT4UU.Installer.Common;

namespace UT4UU.Installer.CLI
{
	internal class Program
	{
		static void PrintHelp()
		{
			// simulate empty arguments to get default values
			var o = new Options();
			ParseArguments(new string[0], ref o);
			Console.WriteLine($@"USAGE:
	UT4UU.Installer.CLI <Install|Uninstall> [OPTIONS]

DESCRIPTION:
Install UT4UU into existing Unreal Tournament installation or
uninstall previously installed UT4UU.

Can be used for any executable associated with the game.
This includes:
    - windows game
    - windows server
    - windows editor
    - linux game
    - linux server

Only latest released builds of the binaries are supported!
This means that if you are using windows binaries from before
2021 November update then you won't be able to install UT4UU.

Not all options are taken into account when uninstalling.
At installation InstallInfo.bin file is created which stores
all information required for proper uninstallation.


OPTIONS:
    -d or --dry-run                   Only logs what is supposed to be happening. Does not actually
                                      preform an installation. This is useful for debugging or just
                                      seeing what will happen at installation.
                                      (default: {o.IsDryRun})

    -s or --symbolic-links            Create symbolic links instead of copying files
                                      (default: {o.CreateSymbolicLinks})

    -u or --upgrade-engine-modules    Upgrade engine's modules. This fixes friend list in the game.
                                      (default: {o.UpgradeEngineModules})

    -i or --install-path <path>       Specify installation directory. Should be either root directory
                                      of the game, server or editor. If this is not specified then
                                      installer will try to locate this directory on its own.
                                      (default: {o.InstallLocation})

	--source-path <path>              Specify the location of directory tree structure containing
                                      installation files.
                                      (default: {o.SourceLocation})

	--replacement-suffix <suffix>     Specify the suffix for when original files are backed up.
                                      Useful only in combination with -u.
                                      (default: {o.ReplacementSuffix})

	--platform-target <platform>      Overwrite automatically detected platform target of installation
                                      directory. Not recommended for use.
                                      Possible values: Win64, Linux
                                      (default: {o.PlatformTarget})

	--build-configuration <comnfig>   Overwrite automatically detected build configuration of
                                      installation directory. Not recommended for use.
                                      Possible values: Shipping, ShippingServer, DevelopmentEditor
                                      (default: {o.BuildConfiguration})
");
		}

		private static int ParseArguments(string[] args, ref Options options)
		{
			//options.InstallLocation = Helper.TryFindInstallationLocation() ?? string.Empty;
			options.SourceLocation = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? Environment.CurrentDirectory, "Files");
			options.Logger = new StreamWriter(Console.OpenStandardOutput());
			options.Logger.AutoFlush = true;

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
				else if (arg == "-s" || arg == "--symbolic-links")
					options.CreateSymbolicLinks = true;
				else if (arg == "-u" || arg == "--upgrade-engine-modules")
					options.UpgradeEngineModules = true;
				else if (arg == "-i" || arg == "--install-path")
					options.InstallLocation = args[++i];
				else if (arg == "--source-path")
					options.SourceLocation = args[++i];
				else if (arg == "--replacement-suffix")
					options.ReplacementSuffix = args[++i];
				else if (arg == "--platform-target")
					options.PlatformTarget = (PlatformTarget)Enum.Parse(typeof(PlatformTarget), args[++i]);
				else if (arg == "--build-configuration")
					options.BuildConfiguration = (BuildConfiguration)Enum.Parse(typeof(BuildConfiguration), args[++i]);
				else
				{
					return 1;
				}
			}

			string? detectedInstallLocation = Helper.TryFindInstallationLocation();
			if (string.IsNullOrWhiteSpace(options.InstallLocation))
			{
				if (detectedInstallLocation == null)
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
			else if (options.PlatformTarget != PlatformTarget.Unknown && options.BuildConfiguration == BuildConfiguration.Unknown)
			{
				options.Logger.WriteLine("BuildConfiguration is unknown.");
				return 1;
			}
			else if (options.PlatformTarget == PlatformTarget.Unknown && options.BuildConfiguration == BuildConfiguration.Unknown)
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
			else if (options.PlatformTarget != PlatformTarget.Unknown && options.BuildConfiguration != BuildConfiguration.Unknown)
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

			// normalize paths
			options.SourceLocation = new DirectoryInfo(options.SourceLocation).FullName;
			options.InstallLocation = new DirectoryInfo(options.InstallLocation).FullName;

			return 0;
		}

		private static string GetInstallInfoFile(string installLocation)
		{
			return Path.Combine(installLocation, "UnrealTournament", "Plugins", "UT4UU", "InstallInfo.bin");
		}

		static int Main(string[] args)
		{
			if (args.Length > 0)
			{
				Options options = new Options();
				string command = args[0].ToLower();
				string[] optionArguments = new string[args.Length - 1];
				Array.Copy(args, 1, optionArguments, 0, optionArguments.Length);
				int result = ParseArguments(optionArguments, ref options);

				options.Logger = new StreamWriter(Console.OpenStandardOutput());
				options.Logger.AutoFlush = true;

				if (result == 0)
				{
					if (command == "install")
					{
						// create install info file which will be copied to installed plugin directory
						if (!options.IsDryRun)
							options.Save(GetInstallInfoFile(options.SourceLocation));

						OperationInstall op = new OperationInstall(options);
						op.Do();
						return 0;
					}
					else if (command == "uninstall")
					{
						FileInfo fi = new FileInfo(GetInstallInfoFile(options.InstallLocation));
						if (fi.Exists)
						{
							// read stored install info
							var installInfo = Options.Load(fi.FullName);
							installInfo.SourceLocation = options.SourceLocation;
							installInfo.IsDryRun = options.IsDryRun;
							installInfo.Logger = options.Logger;
							options = installInfo;
						}

						if (fi.Exists || options.IsDryRun)
						{
							OperationUninstall op = new OperationUninstall(options);
							op.Do();
							return 0;
						}
						else
						{
							Console.WriteLine($"Could not find installation info file in '{fi.FullName}'");
						}
					}
				}
			}

			PrintHelp();
			return 1;
		}
	}
}
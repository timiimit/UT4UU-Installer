

using System.Reflection;
using System.Threading.Tasks;
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
	UT4UU.Installer.CLI <install|uninstall> [OPTIONS]

DESCRIPTION:
Install UT4UU into existing Unreal Tournament installation or
uninstall previously installed UT4UU.

This tool is not designed to handle upgrading UT4UU.
Please uninstall any previously installed version before
attempting to install using this tool.

For windows users: This will not preform an actual system 
installation that can be then found in control panel or applist.
It will only move files to proper locations.

Can be used for any executable associated with the game.
This includes:
    - windows game
    - windows server
    - windows editor
    - linux game
    - linux server

Only latest released builds of the binaries are supported!
This means that if you are using windows binaries from before
2021 November update, then you won't be able to (and shouldn't
try to) install UT4UU.

Not all options are taken into account when uninstalling.
At installation InstallInfo.bin file is created which stores
all information required for proper uninstallation.


OPTIONS:

    -i or --install-path <path>       Specify installation directory. Should be either root directory
                                      of the game, server or editor. If this is not specified then
                                      installer will try to locate this directory on its own.
                                      (default: {o.InstallLocation})

    -s or --shortcut                  Create a shortcut on desktop.

    -u or --upgrade-engine-modules    Upgrade engine's modules. This fixes friend list in the game.
                                      (default: {o.UpgradeEngineModules})

    --install-local-server            Try to install in game's local server.
                                      (default: {o.TryToInstallInLocalGameServer})

    --refreshing-experience           Install new startup movie and a new splash screen image.
                                      (default: {o.RefreshingExperience})

    -d or --dry-run                   Only logs what is supposed to be happening. Does not actually
                                      preform an installation. This is useful for debugging or just
                                      seeing what will happen at installation.
                                      (default: {o.IsDryRun})

    --replacement-suffix <suffix>     Specify the suffix for when original files are backed up.
                                      Useful only in combination with -u.
                                      (default: {o.ReplacementSuffix})

    -l or --symbolic-links            Create symbolic links instead of copying files
                                      (default: {o.CreateSymbolicLinks})

    --source-path <path>              Specify the location of directory tree structure containing
                                      installation files.
                                      (default: {o.SourceLocation})

    --platform-target <platform>      Overwrite automatically detected platform target of installation
                                      directory. Not recommended for use.
                                      Possible values: Win64, Linux
                                      (default: {o.PlatformTarget})

    --build-configuration <config>    Overwrite automatically detected build configuration of
                                      installation directory. Not recommended for use.
                                      Possible values: Shipping, ShippingServer, DevelopmentEditor
                                      (default: {o.BuildConfiguration})
");
		}

		private static int ParseArguments(string[] args, ref Options options)
		{
			//options.InstallLocation = Helper.TryFindInstallationLocation() ?? string.Empty;
			options.SourceLocation = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? Environment.CurrentDirectory, "Files");
			options.Logger = (object? sender, Options.LogEventArgs e) =>
			{
				for (int i = 0; i < e.OperationDepth; i++)
				{
					Console.Write("    ");
				}
				Console.WriteLine(e.Message);
			};

#if DEBUG
			options.InstallLocation = "E:/UT4Source/TestGameDir";
			options.SourceLocation = "E:/UT4Source/UT4UU/Source/Programs/Installer/Files";
#endif

			for (int i = 0; i < args.Length; i++)
			{
				string arg = args[i].ToLower();
				if (arg == "-d" || arg == "--dry-run")
					options.IsDryRun = true;
				else if (arg == "-s" || arg == "--shortcut")
					options.CreateShortcut = true;
				else if (arg == "-l" || arg == "--symbolic-links")
					options.CreateSymbolicLinks = true;
				else if (arg == "--install-local-server")
					options.TryToInstallInLocalGameServer = true;
				else if (arg == "--refreshing-experience")
					options.RefreshingExperience = true;
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
					options.Logger.Invoke(null, new("Failed to find install location. Please specify it manually.", 0, 0, 0));
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
				options.Logger.Invoke(null, new("PlatformTarget is unknown.", 0, 0, 0));
				return 1;
			}
			else if (options.PlatformTarget != PlatformTarget.Unknown && options.BuildConfiguration == BuildConfiguration.Unknown)
			{
				options.Logger.Invoke(null, new("BuildConfiguration is unknown.", 0, 0, 0));
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
					options.Logger.Invoke(null, new("Failed to detect PlatformTarget and BuildConfiguration.", 0, 0, 0));
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
						options.Logger.Invoke(null, new($"WARNING: Detected PlatformTarget ({pt}) does not match the specified one", 0, 0, 0));
					}
					if (bc != options.BuildConfiguration)
					{
						options.Logger.Invoke(null, new($"WARNING: Detected BuildConfiguration ({bc}) does not match the specified one", 0, 0, 0));
					}
				}
				else
				{
					options.Logger.Invoke(null, new("WARNING: Failed to detect PlatformTarget and BuildConfiguration.", 0, 0, 0));
				}
			}

			// normalize paths
			options.SourceLocation = new DirectoryInfo(options.SourceLocation).FullName;
			options.InstallLocation = new DirectoryInfo(options.InstallLocation).FullName;

			return 0;
		}

		static int Main(string[] args)
		{
			int result;
			if (args.Length > 0)
			{
				Options options = new Options();
				string command = args[0].ToLower();
				string[] optionArguments = new string[args.Length - 1];
				Array.Copy(args, 1, optionArguments, 0, optionArguments.Length);
				result = ParseArguments(optionArguments, ref options);
				using (var stdout = new StreamWriter(Console.OpenStandardOutput()))
				{
					options.WriteOptions(stdout);
				}

				if (result == 0)
				{
					if (command == "install")
					{
						OperationInstall op = new OperationInstall(options);
						op.Do();
					}
					else if (command == "uninstall")
					{
						FileInfo fi = new FileInfo(Helper.GetInstallInfoFile(options.InstallLocation));
						if (fi.Exists)
						{
							// read stored install info
							var installInfo = Options.Load(fi.FullName);

							// copy over some uninstall customizable options
							installInfo.SourceLocation = options.SourceLocation;
							installInfo.IsDryRun = options.IsDryRun;
							installInfo.Logger = options.Logger;
							options = installInfo;
						}

						if (fi.Exists || options.IsDryRun)
						{
							OperationUninstall op = new OperationUninstall(options);
							op.Do();
						}
						else
						{
							options.Logger?.Invoke(null, new($"Could not find installation info file in '{fi.FullName}'", 0, 0, 0));
						}
					}
				}
			}
			else
			{
				result = 1;
			}

			if (result != 0)
				PrintHelp();
			return result;
		}
	}
}
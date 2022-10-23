using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UT4UU.Installer.Common
{
	public enum PlatformTarget : byte
	{
		Unknown,
		Win64,
		Linux,
	}

	public enum BuildConfiguration : byte
	{
		Unknown,
		Shipping,
		ShippingServer,
		DevelopmentEditor,
	}
	public static class Helper
	{
		internal static PlatformTarget[] GetPlatformTargets()
		{
			return new PlatformTarget[]
				{
					PlatformTarget.Win64,
					PlatformTarget.Linux
				};
		}
		internal static BuildConfiguration[] GetBuildConfigurations()
		{
			return new BuildConfiguration[]
				{
					BuildConfiguration.Shipping,
					BuildConfiguration.ShippingServer,
					BuildConfiguration.DevelopmentEditor
				};
		}

		internal static string GetModulePrefix(PlatformTarget platformTarget, BuildConfiguration config)
		{
			string prefix = string.Empty;
			switch (platformTarget)
			{
				case PlatformTarget.Linux:
					prefix = "lib";
					break;
			}
			switch (config)
			{
				case BuildConfiguration.Shipping:
					prefix += "UE4-";
					break;
				case BuildConfiguration.ShippingServer:
					prefix += "UE4Server-";
					break;
				case BuildConfiguration.DevelopmentEditor:
					prefix += "UE4Editor-";
					break;
			}

			return prefix;
		}

		internal static string GetModuleSuffix(PlatformTarget platformTarget, BuildConfiguration config)
		{
			string suffix = string.Empty;
			switch (config)
			{
				case BuildConfiguration.Shipping:
				case BuildConfiguration.ShippingServer:
					suffix += "-" + platformTarget.ToString() + "-Shipping";
					break;
			}

			switch (platformTarget)
			{
				case PlatformTarget.Win64:
					suffix += ".dll";
					break;
				case PlatformTarget.Linux:
					suffix += ".so";
					break;
			}

			return suffix;
		}

		internal static string GetFileChecksum(string filepath)
		{
			byte[] bytes = File.ReadAllBytes(filepath);
			byte[] hash = System.Security.Cryptography.SHA256.HashData(bytes);
			return Convert.ToHexString(hash);
		}

		internal static string GetGameExecutableExpectedChecksum(PlatformTarget platformTarget, BuildConfiguration config)
		{
			switch (platformTarget)
			{
				case PlatformTarget.Win64:
					switch (config)
					{
						case BuildConfiguration.Shipping:
							return "7DD13B2D18E8C1583A5F61F48638B705A29F5F61A61EF86065DF94E4A050C4A7";
						case BuildConfiguration.ShippingServer:
							return "12E3184BC9116FB6663C12C9DA9AA210026EC781F1796BEB0C7182BC37E6B5D6";
						case BuildConfiguration.DevelopmentEditor:
							return "30E94DE631E3F4E58A490171E045CDACEC76843AABAD045E64D0602A508840A3";
					}
					break;
				case PlatformTarget.Linux:
					switch (config)
					{
						case BuildConfiguration.Shipping:
							throw new NotImplementedException();
						//return "";
						case BuildConfiguration.ShippingServer:
							throw new NotImplementedException();
							//return "";
							//case BuildConfiguration.DevelopmentEditor:
							//	return "";
					}
					break;
			}

			return string.Empty;
		}



		private struct QueueElement
		{
			public string Folder;
			public int Depth;
		}

		private static string? SearchForDirectory(string folder, int depth, Regex pattern, Func<string, bool> matchValidator)
		{
			Queue q = new Queue();
			q.Enqueue(new QueueElement() { Folder = folder, Depth = 0 });
			while (q.Count > 0)
			{
				var eObj = q.Dequeue();
				if (eObj == null)
					continue;

				QueueElement e = (QueueElement)eObj;

				folder = e.Folder;
				if (pattern.IsMatch(folder))
				{
					if (matchValidator(folder))
					{
						return folder;
					}
				}

				if (e.Depth < depth)
				{
					try
					{
						var enumerator = Directory.EnumerateDirectories(folder);
						foreach (string subdir in enumerator)
						{
							q.Enqueue(new QueueElement() { Folder = subdir, Depth = e.Depth + 1 });
						}
					}
					catch
					{
						// ignore failed to enumerate
					}
				}
			}

			return null;
		}

		/////////////////////////////////////////////////////////////////////////////////////////
		/////////////////////////////////////////////////////////////////////////////////////////
		/////////////////////////////                    ////////////////////////////////////////
		/////////////////////////////                    ////////////////////////////////////////
		/////////////////////////////   PUBLIC METHODS   ////////////////////////////////////////
		/////////////////////////////                    ////////////////////////////////////////
		/////////////////////////////                    ////////////////////////////////////////
		/////////////////////////////////////////////////////////////////////////////////////////
		/////////////////////////////////////////////////////////////////////////////////////////

		public static string GetUTExecutableFromInstallationDirectory(
			string installDirectory, PlatformTarget platformTarget, BuildConfiguration buildConfiguration)
		{
			return Path.Combine(
				installDirectory,
				"Engine",
				"Binaries",
				platformTarget.ToString(),
				GetGameExecutableName(platformTarget, buildConfiguration)
			);
		}

		public static bool IsUTDirectory(string directory, PlatformTarget platformTarget, BuildConfiguration buildConfiguration)
		{
			return IsUTExecutable(
				GetUTExecutableFromInstallationDirectory(directory, platformTarget, buildConfiguration),
				platformTarget, buildConfiguration
			);
		}

		public static bool IsUTExecutable(string file, PlatformTarget platformTarget, BuildConfiguration buildConfiguration)
		{
			if (File.Exists(file))
			{
				if (GetGameExecutableExpectedChecksum(platformTarget, buildConfiguration) == GetFileChecksum(file))
				{
					return true;
				}
			}
			return false;
		}

		public static string GetActualModuleName(string nameOnly, PlatformTarget platformTarget, BuildConfiguration config)
		{
			return GetModulePrefix(platformTarget, config) + nameOnly + GetModuleSuffix(platformTarget, config);
		}

		public static string GetGameExecutableName(PlatformTarget platformTarget, BuildConfiguration config)
		{
			string ret = string.Empty;
			switch (config)
			{
				case BuildConfiguration.Shipping:
					ret += "UE4-" + platformTarget.ToString() + "-Shipping";
					break;
				case BuildConfiguration.ShippingServer:
					ret += "UE4Server-" + platformTarget.ToString() + "-Shipping";
					break;
				case BuildConfiguration.DevelopmentEditor:
					ret += "UE4Editor";
					break;
			}

			if (platformTarget == PlatformTarget.Win64)
			{
				ret += ".exe";
			}

			return ret;
		}


		public static string? TryFindInstallationLocation()
		{
			var platforms = GetPlatformTargets();
			for (int i = 0; i < platforms.Length; i++)
			{
				var configs = GetBuildConfigurations();
				for (int j = 0; j < configs.Length; j++)
				{
					string? installLocation = TryFindInstallationLocationForPlatform(platforms[i], configs[j]);
					if (installLocation != null)
						return installLocation;
				}
			}
			return null;
		}

		public static string? TryFindInstallationLocationForPlatform(PlatformTarget platformTarget, BuildConfiguration buildConfiguration)
		{
			Regex regex = new Regex("unreal|tournament|ut|ue4", RegexOptions.IgnoreCase);

			string[] searchDirs;
			string? rootDir = Path.GetPathRoot(Environment.SystemDirectory);

			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				rootDir ??= "C:\\";

				// SpecialFolder.ProgramFilesX86 and SpecialFolder.ProgramFiles return the same path
				// for some reason, so use ExpandEnvironmentVariables instead
				searchDirs = new string[]
				{
					Environment.ExpandEnvironmentVariables(@"%ProgramW6432%\Epic Games"),
					Environment.ExpandEnvironmentVariables(@"%ProgramFiles(x86)%\Epic Games"),
					rootDir,
					Environment.ExpandEnvironmentVariables(@"%ProgramW6432%"),
					Environment.ExpandEnvironmentVariables(@"%ProgramFiles(x86)%"),
					Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
					Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
					Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
				};
			}
			else if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				// set some possible installation directories
				rootDir ??= "/";
				searchDirs = new string[]
				{
					rootDir,
					"/opt",
					"~",
				};
			}
			else
			{
				// we have no clue how to do anything
				return null;
			}

			// search for install directory
			Func<string, bool> funcValidator = (string s) => { return IsUTDirectory(s, platformTarget, buildConfiguration); };
			string? utDirectory;
			for (int i = 0; i < searchDirs.Length; i++)
			{
				utDirectory = searchDirs[i];
				if (Directory.Exists(utDirectory))
				{
					utDirectory = SearchForDirectory(utDirectory, 1, regex, funcValidator);
					if (utDirectory != null)
						return utDirectory;
				}
			}

			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				// search all root drive locations
				var drives = DriveInfo.GetDrives();
				for (int i = 0; i < drives.Length; i++)
				{
					DriveInfo drive = drives[i];
					if (drive.DriveType == DriveType.Ram || drive.DriveType == DriveType.Fixed)
					{
						DirectoryInfo di = drive.RootDirectory;
						if (di.FullName == rootDir)
							continue; // already checked

						utDirectory = SearchForDirectory(di.FullName, 1, regex, funcValidator);
						if (utDirectory != null)
							return utDirectory;
					}
				}
			}
			else if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				// search in commonly installed locations
				searchDirs = new string[]
				{
					"/usr/bin",
					"/usr/local/bin"
				};

				string executableName = GetGameExecutableName(platformTarget, buildConfiguration);

				// look for specific executable names
				foreach (var searchDir in searchDirs)
				{
					if (!Directory.Exists(searchDir))
						continue;

					string executablePath = Path.Combine(searchDir, executableName);
					if (IsUTExecutable(executablePath, platformTarget, buildConfiguration))
						return searchDir;
				}

				// enumerate all files and look for possible game executable
				foreach (var searchDir in searchDirs)
				{
					foreach (var filepath in Directory.EnumerateFiles(searchDir))
					{
						// we already checked this file
						if (Path.GetFileName(filepath) == executableName)
							continue;

						// if is not really what we expect, skip
						if (!regex.IsMatch(filepath))
							continue;

						if (IsUTExecutable(filepath, platformTarget, buildConfiguration))
						{
							var fi = new FileInfo(filepath);
							if (fi.LinkTarget != null)
							{
								// continue following symlinks until we find an actual file.
								// not sure if it is possible to make recursive symlinks, but we handle them anyway lol
								List<string> symlinkPaths = new List<string>();
								while (fi.LinkTarget != null)
								{
									if (symlinkPaths.Contains(fi.LinkTarget))
									{
										// recursive symlink found
										break;
									}

									string prevTarget = fi.LinkTarget;
									symlinkPaths.Add(prevTarget);

									fi = new FileInfo(prevTarget);
								}
								if (fi.LinkTarget != null)
								{
									// ignore recursive symlinks
									if (symlinkPaths.Contains(fi.LinkTarget))
										continue;
								}

								string? dir = Path.GetDirectoryName(fi.FullName);
								if (dir == null)
									continue;

								DirectoryInfo di = new DirectoryInfo(Path.Combine(dir, "..", "..", ".."));
								if (di.Exists)
								{
									fi = new FileInfo(Path.Combine(dir, "Engine", "Binaries", platformTarget.ToString(), executableName));
									if (fi.Exists && IsUTExecutable(fi.FullName, platformTarget, buildConfiguration))
									{
										return dir;
									}
								}

								// file was in an unexpected directory tree structure
								// ignore this symlink
							}
							else
							{
								// this is strange...
								// executable is directly placed in the bin directory.
								// something must be wrong here, or the system admin is stupid.
							}
						}
					}
				}
			}

			return null;
		}

		public static bool DetectInstallationType(
			string installLocation,
			ref PlatformTarget platformTarget,
			ref BuildConfiguration buildConfiguration)
		{
			string engineBinaries = Path.Combine(installLocation, "Engine", "Binaries");
			var platforms = GetPlatformTargets();
			var configs = GetBuildConfigurations();

			for (int i = 0; i < platforms.Length; i++)
			{
				string binaryDir = Path.Combine(engineBinaries, platforms[i].ToString());

				if (!Directory.Exists(binaryDir))
					continue;

				for (int j = 0; j < configs.Length; j++)
				{
					string filepath = Path.Combine(binaryDir, GetGameExecutableName(platforms[i], configs[j]));
					if (!File.Exists(filepath))
						continue;

					if (IsUTExecutable(filepath, platforms[i], configs[j]))
					{
						platformTarget = platforms[i];
						buildConfiguration = configs[j];
						return true;
					}
				}
			}

			return false;
		}




	}
}

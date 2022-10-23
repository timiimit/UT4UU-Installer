using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

		internal static string GetModulePrefix(BuildConfiguration config, PlatformTarget platformTarget)
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

		internal static string GetModuleSuffix(BuildConfiguration config, PlatformTarget platformTarget)
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

		internal static string GetGameExecutableExpectedChecksum(BuildConfiguration config, PlatformTarget platformTarget)
		{
			switch (platformTarget)
			{
				case PlatformTarget.Win64:
					switch (config)
					{
						case BuildConfiguration.Shipping:
							return "";
						case BuildConfiguration.ShippingServer:
							return "";
						case BuildConfiguration.DevelopmentEditor:
							return "";
					}
					break;
				case PlatformTarget.Linux:
					switch (config)
					{
						case BuildConfiguration.Shipping:
							return "";
						case BuildConfiguration.ShippingServer:
							return "";
							//case BuildConfiguration.DevelopmentEditor:
							//	return "";
					}
					break;
			}

			return string.Empty;
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



		public static string GetActualModuleName(string nameOnly, BuildConfiguration config, PlatformTarget platformTarget)
		{
			return GetModulePrefix(config, platformTarget) + nameOnly + GetModuleSuffix(config, platformTarget);
		}

		public static string GetGameExecutableName(BuildConfiguration config, PlatformTarget platformTarget)
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
			return null;
		}

		public static bool DetectInstallationType(string installLocation, ref PlatformTarget platformTarget, ref BuildConfiguration buildConfiguration)
		{
			string engineBinaries = Path.Combine(installLocation, "Engine", "Binaries");
			var platforms = GetPlatformTargets();
			for (int i = 0; i < platforms.Length; i++)
			{
				string binaryDir = Path.Combine(engineBinaries, platforms[i].ToString());

				if (Directory.Exists(binaryDir))
				{
					var configs = GetBuildConfigurations();
					for (int j = 0; j < configs.Length; j++)
					{
						string filepath = Path.Combine(binaryDir, GetGameExecutableName(configs[j], platforms[i]));
						if (File.Exists(filepath))
						{
							string expectedHash = GetGameExecutableExpectedChecksum(configs[j], platforms[i]);
							if (string.IsNullOrWhiteSpace(expectedHash))
								continue;

							string actualHash = GetFileChecksum(filepath);
							if (expectedHash == actualHash)
							{
								platformTarget = platforms[i];
								buildConfiguration = configs[j];
								return true;
							}
						}
					}
				}
			}

			return false;
		}




	}
}

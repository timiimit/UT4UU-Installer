using System.Security.Cryptography;

namespace UT4UU.Installer.Common
{
	public class Common
	{




		public static bool Install(string installLocation)
		{
			PlatformTarget platformTarget = PlatformTarget.Win64;
			BuildConfiguration buildConfiguration = BuildConfiguration.Shipping;
			if (!DetectInstallationType(installLocation, ref platformTarget, ref buildConfiguration))
				return false;

			string engineBinaries = Path.Combine(installLocation, "Engine", "Binaries", platformTarget.ToString());
			string[] modules = new string[]
			{
				"SSL", "HTTP", "HttpNetworkStreaming",
				"XMPP"
			};
			for (int i = 0; i < modules.Length; i++)
			{
				string modulePath = Path.Combine(engineBinaries, GetActualModuleName(modules[i]));
			}

			return true;
		}
	}
}
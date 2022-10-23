using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UT4UU.Installer.Common
{
	public struct Options
	{
		public bool IsDryRun { get; set; }
		public bool CreateSymbolicLinks { get; set; }
		public bool UpgradeEngineModules { get; set; }
		public string SourceLocation { get; set; }
		public string InstallLocation { get; set; }
		public string SuffixOfReplacedEngineModules { get; set; }
		public PlatformTarget PlatformTarget { get; set; }
		public BuildConfiguration BuildConfiguration { get; set; }
		public StreamWriter? Logger { get; private set; }


		public Options()
		{
			IsDryRun = true;
			CreateSymbolicLinks = false;
			UpgradeEngineModules = true;
			SourceLocation = string.Empty;
			InstallLocation = string.Empty;
			SuffixOfReplacedEngineModules = ".bak";
			PlatformTarget = PlatformTarget.Win64;
			BuildConfiguration = BuildConfiguration.Shipping;
			Logger = null;
		}
	}
}

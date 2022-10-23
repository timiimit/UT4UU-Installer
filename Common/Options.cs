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
		public string ReplacementSuffix { get; set; }
		public PlatformTarget PlatformTarget { get; set; }
		public BuildConfiguration BuildConfiguration { get; set; }
		public StreamWriter? Logger { get; set; }


		public Options()
		{
			IsDryRun = true;
			CreateSymbolicLinks = false;
			UpgradeEngineModules = true;
			SourceLocation = string.Empty;
			InstallLocation = string.Empty;
			ReplacementSuffix = ".uu.bak";
			PlatformTarget = PlatformTarget.Unknown;
			BuildConfiguration = BuildConfiguration.Unknown;
			Logger = null;
		}

		public void Save(string filepath)
		{
			using (var w = new BinaryWriter(new FileStream(filepath, FileMode.CreateNew, FileAccess.Write, FileShare.Read)))
			{
				w.Write(IsDryRun);
				w.Write(CreateSymbolicLinks);
				w.Write(UpgradeEngineModules);
				w.Write(SourceLocation);
				w.Write(InstallLocation);
				w.Write(ReplacementSuffix);
				w.Write((byte)PlatformTarget);
				w.Write((byte)BuildConfiguration);
			}
		}

		public static Options Load(string filepath)
		{
			var o = new Options();
			using (var r = new BinaryReader(new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read)))
			{
				o.IsDryRun = r.ReadBoolean();
				o.CreateSymbolicLinks = r.ReadBoolean();
				o.UpgradeEngineModules = r.ReadBoolean();
				o.SourceLocation = r.ReadString();
				o.InstallLocation = r.ReadString();
				o.ReplacementSuffix = r.ReadString();
				o.PlatformTarget = (PlatformTarget)r.ReadByte();
				o.BuildConfiguration = (BuildConfiguration)r.ReadByte();
			}
			return o;
		}
	}
}

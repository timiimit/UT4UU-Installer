using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UT4UU.Installer.Common
{
	public struct Options
	{
		public bool CreateShortcut { get; set; }
		public bool IsDryRun { get; set; }
		public bool CreateSymbolicLinks { get; set; }
		public bool UpgradeEngineModules { get; set; }
		public bool RefreshingExperience { get; set; }
		public bool TryToInstallInLocalGameServer { get; set; }
		public string SourceLocation { get; set; }
		public string InstallLocation { get; set; }
		public string ReplacementSuffix { get; set; }
		public PlatformTarget PlatformTarget { get; set; }
		public BuildConfiguration BuildConfiguration { get; set; }

		public class LogEventArgs
		{
			public string Message { get; set; }
			public int TaskIndex { get; set; }
			public int TaskCount { get; set; }
			public int OperationDepth { get; set; }

			public LogEventArgs(string message, int taskIndex, int taskCount, int operationDepth)
			{
				Message = message;
				TaskIndex = taskIndex;
				TaskCount = taskCount;
				OperationDepth = operationDepth;
			}
		}
		public Action<object?, LogEventArgs>? Logger { get; set; }


		public Options()
		{
			CreateShortcut = false;
#if DEBUG
			IsDryRun = true;
#else
			IsDryRun = false;
#endif
			CreateSymbolicLinks = false;
			UpgradeEngineModules = false;
			RefreshingExperience = false;
			TryToInstallInLocalGameServer = false;
			SourceLocation = string.Empty;
			InstallLocation = string.Empty;
			ReplacementSuffix = ".uu.bak";
			PlatformTarget = PlatformTarget.Unknown;
			BuildConfiguration = BuildConfiguration.Unknown;
			Logger = null;
		}

		public void Save(string filepath)
		{
			using (var w = new BinaryWriter(new FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.Read)))
			{
				w.Write(CreateShortcut);
				w.Write(IsDryRun);
				w.Write(CreateSymbolicLinks);
				w.Write(UpgradeEngineModules);
				w.Write(RefreshingExperience);
				w.Write(TryToInstallInLocalGameServer);
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
				o.CreateShortcut = r.ReadBoolean();
				o.IsDryRun = r.ReadBoolean();
				o.CreateSymbolicLinks = r.ReadBoolean();
				o.UpgradeEngineModules = r.ReadBoolean();
				o.RefreshingExperience = r.ReadBoolean();
				o.TryToInstallInLocalGameServer = r.ReadBoolean();
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UT4UU.Installer.Common
{
	public class TaskCreateInstallInfoFile : Task
	{
		Options options;

		public TaskCreateInstallInfoFile(Options options) :
			base(
				$"Create InstallInfo.bin in {Path.GetDirectoryName(Helper.GetInstallInfoFile(options.InstallLocation))}",
				$"Delete InstallInfo.bin in {Path.GetDirectoryName(Helper.GetInstallInfoFile(options.InstallLocation))}"
			)
		{
			this.options = options;
		}

		public override void Do()
		{
			//throw new NotImplementedException();
			options.Save(Helper.GetInstallInfoFile(options.InstallLocation));
		}

		public override void Undo()
		{
			File.Delete(Helper.GetInstallInfoFile(options.InstallLocation));
		}

		public override void FinishDo()
		{

		}

		public override void FinishUndo()
		{

		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UT4UU.Installer.Common
{
	public class TaskCreateDirectory : Task
	{
		private string directory;

		public TaskCreateDirectory(string directory) : base($"Create directory '{directory}'")
		{
			this.directory = directory;
		}

		public override void Do()
		{
			Directory.CreateDirectory(directory);
		}

		public override void Undo()
		{
			Directory.Delete(directory);
		}

		public override void FinishDo()
		{

		}

		public override void FinishUndo()
		{

		}
	}
}

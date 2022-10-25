using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UT4UU.Installer.Common
{
	public class TaskRenameFile : Task
	{
		private string fromFullpath;
		private string toFullpath;

		public TaskRenameFile(string fromFullpath, string toFilename) : base(
			$"Rename '{fromFullpath}' to '{toFilename}'",
			$"Rename '{Path.Combine(Path.GetDirectoryName(fromFullpath) ?? string.Empty, toFilename)}' to '{Path.GetFileName(fromFullpath)}'")
		{
			this.fromFullpath = fromFullpath;
			this.toFullpath = Path.Combine(Path.GetDirectoryName(fromFullpath) ?? string.Empty, toFilename);
		}
		public override void Do()
		{
			File.Move(fromFullpath, toFullpath);
		}

		public override void Undo()
		{
			File.Move(toFullpath, fromFullpath);
		}

		public override void FinishDo()
		{
		}

		public override void FinishUndo()
		{
		}
	}
}

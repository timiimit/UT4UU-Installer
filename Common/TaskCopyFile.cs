using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UT4UU.Installer.Common
{
	public class TaskCopyFile : Task
	{
		private string from;
		private string to;

		public TaskCopyFile(string from, string to) : base($"Copy '{Path.GetFileName(from)}' to '{Path.GetDirectoryName(to)}'", $"Delete '{to}'")
		{
			this.from = from;
			this.to = to;
		}

		public override void Do()
		{
			File.Copy(from, to);
		}

		public override void Undo()
		{
			File.Delete(to);
		}

		public override void FinishDo()
		{

		}

		public override void FinishUndo()
		{

		}
	}
}

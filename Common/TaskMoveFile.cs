using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UT4UU.Installer.Common
{
	public class TaskMoveFile : Task
	{
		private string from;
		private string to;

		public TaskMoveFile(string from, string to) : base($"Move '{from}' to '{to}'", $"Move '{to}' to '{from}'")
		{
			this.from = from;
			this.to = to;
		}
		public override void Do()
		{
			File.Move(from, to);
		}

		public override void Undo()
		{
			File.Move(to, from);
		}

		public override void FinishDo()
		{
		}

		public override void FinishUndo()
		{
		}
	}
}

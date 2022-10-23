using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UT4UU.Installer.Common
{
	public class TaskCreateSymbolicLink : Task
	{
		string path;
		string pointsTo;

		public TaskCreateSymbolicLink(string path, string pointsTo) :
			base(
				$"Create symbolic link at '{path}' pointing at '{pointsTo}'",
				$"Delete symbolic link '{path}'"
			)
		{
			this.path = path;
			this.pointsTo = pointsTo;
		}

		public override void Do()
		{
			var fi = new FileInfo(path);
			fi.CreateAsSymbolicLink(pointsTo);
		}

		public override void Undo()
		{
			File.Delete(path);
		}

		public override void FinishDo()
		{

		}

		public override void FinishUndo()
		{

		}
	}
}

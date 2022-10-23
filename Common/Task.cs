using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UT4UU.Installer.Common
{
	public abstract class Task
	{
		public string DescriptionDo { get; }
		public string DescriptionUndo { get; }

		public Task(string descriptionDo, string descriptionUndo)
		{
			DescriptionDo = descriptionDo;
			DescriptionUndo = descriptionUndo;
		}


		/// <summary>
		/// Executes the task
		/// </summary>
		public abstract void Do();

		/// <summary>
		/// Undos the executed task, putting things into the state from before Do() was called
		/// </summary>
		public abstract void Undo();

		/// <summary>
		/// Called when all tasks have successfully finished
		/// </summary>
		public abstract void FinishDo();

		/// <summary>
		/// Called when all tasks have been put into original state
		/// </summary>
		public abstract void FinishUndo();
	}
}

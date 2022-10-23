using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UT4UU.Installer.Common
{
	public class Operation : Task
	{
		public Options Options { get; private set; }


		protected List<Task> tasks;
		private bool doDirection;

		public Operation(string description, Options options, bool direction) : base(description)
		{
			this.Options = options;
			this.tasks = new List<Task>();
			doDirection = direction;
		}

		private void InternalDo(bool targetDo)
		{
			List<int> touchedTasks = new List<int>();
			bool isDoing = targetDo;
			int taskIndex = targetDo ? 0 : tasks.Count - 1;
			while (true)
			{
				if (isDoing == targetDo)
					Options.Logger?.WriteLine($"Do {tasks[taskIndex].Description}");
				else
					Options.Logger?.WriteLine($"Undo {tasks[taskIndex].Description}");

				if (!Options.IsDryRun)
				{
					try
					{
						if (isDoing)
						{
							tasks[taskIndex].Do();
						}
						else
						{
							tasks[taskIndex].Undo();
						}
					}
					catch (Exception ex)
					{
						Options.Logger?.WriteLine($"Caught exception: {ex}");
						if (isDoing)
						{
							Options.Logger?.WriteLine($"Undoing tasks...");

							// do we want to Undo() currently failed task?
							//continue;
						}
						else
						{
							Options.Logger?.WriteLine($"Ignoring exception, continuing to undo tasks...");
						}
					}

					if (isDoing == targetDo)
						touchedTasks.Add(taskIndex);
				}

				if (isDoing)
				{
					taskIndex++;
					if (taskIndex >= tasks.Count)
						break;
				}
				else
				{
					taskIndex--;
					if (taskIndex < 0)
						break;
				}
			}

			for (int i = 0; i < touchedTasks.Count; i++)
			{
				if (isDoing)
					tasks[touchedTasks[i]].FinishDo();
				else
					tasks[touchedTasks[i]].FinishUndo();
			}
		}

		public override void Do()
		{
			InternalDo(doDirection);
		}

		public override void Undo()
		{
			InternalDo(!doDirection);
		}

		public override void FinishDo()
		{
			throw new NotImplementedException();
		}

		public override void FinishUndo()
		{
			throw new NotImplementedException();
		}
	}
}

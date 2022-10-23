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

		public Operation(string descriptionDo, string descriptionUndo, Options options, bool direction) : base(descriptionDo, descriptionUndo)
		{
			Options = options;
			tasks = new List<Task>();
			doDirection = direction;
		}

		private void InternalDo(bool targetDo)
		{
			List<int> touchedTasks = new List<int>();
			bool isDoing = targetDo;
			int taskIndex = targetDo ? 0 : tasks.Count - 1;
			while (true)
			{
				if (isDoing)
					Options.Logger?.WriteLine(tasks[taskIndex].DescriptionDo);
				else
					Options.Logger?.WriteLine(tasks[taskIndex].DescriptionUndo);

				if (!Options.IsDryRun)
				{
					try
					{
						if (isDoing)
							tasks[taskIndex].Do();
						else
							tasks[taskIndex].Undo();
					}
					catch (Exception ex)
					{
						Options.Logger?.WriteLine($"Caught exception: {ex}");
						if (isDoing == targetDo)
						{
							if (tasks[taskIndex].CanFail)
							{
								Options.Logger?.WriteLine($"Continuing with tasks...");
							}
							else
							{
								Options.Logger?.WriteLine($"Undoing tasks...");
								isDoing = !targetDo;
							}
						}
						else
						{
							Options.Logger?.WriteLine($"Ignoring exception, continuing to undo tasks...");
						}
					}
				}

#if false //DEBUG
				if (taskIndex == 10)
				{
					// simulate a fail
					Options.Logger?.WriteLine($"Undoing tasks...");
					isDoing = !targetDo;
				}
#endif

				if (isDoing == targetDo)
					touchedTasks.Add(taskIndex);

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

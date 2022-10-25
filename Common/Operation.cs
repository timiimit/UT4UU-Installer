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

		public int TaskCount { get => tasks.Count; }


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
					Options.Logger?.Invoke(tasks[taskIndex].DescriptionDo, taskIndex, TaskCount);
				else
					Options.Logger?.Invoke(tasks[taskIndex].DescriptionUndo, taskIndex, TaskCount);

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
						Options.Logger?.Invoke($"Task {taskIndex} failed: {ex.Message}", taskIndex, TaskCount);
						if (isDoing == targetDo)
						{
							if (tasks[taskIndex].CanFail)
							{
								Options.Logger?.Invoke($"Continuing with tasks...", taskIndex, TaskCount);
							}
							else
							{
								Options.Logger?.Invoke($"Undoing tasks...", taskIndex, TaskCount);
								isDoing = !targetDo;
							}
						}
						else
						{
							Options.Logger?.Invoke($"Ignoring exception, continuing to undo tasks...", taskIndex, TaskCount);
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

				// make it so that progress actually means something lol
				Thread.Sleep(100);
			}

			for (int i = 0; i < touchedTasks.Count; i++)
			{
				if (isDoing)
					tasks[touchedTasks[i]].FinishDo();
				else
					tasks[touchedTasks[i]].FinishUndo();
			}

			string whatHappened = isDoing == targetDo ? "Successfully completed" : "Aborted";
			string taskDescripion = targetDo ? DescriptionDo : DescriptionUndo;
			Options.Logger?.Invoke($"{whatHappened} action: {taskDescripion}", taskIndex, TaskCount);
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

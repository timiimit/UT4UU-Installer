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
		//private int outerTaskIndex;

		//public int TaskCount
		//{
		//	get
		//	{
		//		int counter = 0;
		//		for (int i = 0; i < tasks.Count; i++)
		//		{
		//			if (tasks[i] is Operation op)
		//			{
		//				counter += op.TaskCount;
		//			}
		//			counter++;
		//		}
		//		return counter;
		//	}
		//}


		protected List<Task> tasks;
		private bool doDirection;

		public Operation(string descriptionDo, string descriptionUndo, /*int outerTaskIndex,*/ Options options, bool direction) : base(descriptionDo, descriptionUndo)
		{
			//this.outerTaskIndex = outerTaskIndex;
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

				try
				{
					if (!Options.IsDryRun || tasks[taskIndex] is Operation)
					{
						if (isDoing)
							tasks[taskIndex].Do();
						else
							tasks[taskIndex].Undo();
					}
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
#if DEBUG
				Thread.Sleep(100);
#else
				Thread.Sleep(50);
#endif
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

			if (isDoing != targetDo)
			{
				// we failed to do this task
				throw new OperationCanceledException();
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

		}

		public override void FinishUndo()
		{

		}
	}
}

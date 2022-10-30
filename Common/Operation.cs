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

		private int operationDepth;
		public int OperationDepth { get => operationDepth; }
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

		public Operation(string descriptionDo, string descriptionUndo, int depth, /*int outerTaskIndex,*/ Options options, bool direction) : base(descriptionDo, descriptionUndo)
		{
			operationDepth = depth;
			//this.outerTaskIndex = outerTaskIndex;
			Options = options;
			tasks = new();
			doDirection = direction;
		}

		private void InternalDo(bool targetDo)
		{
			List<int> touchedTasks = new();
			bool isDoing = targetDo;
			int taskIndex = targetDo ? 0 : tasks.Count - 1;
			while (true)
			{
				if (isDoing)
					Options.Logger?.Invoke(this, new(tasks[taskIndex].DescriptionDo, taskIndex, TaskCount, OperationDepth));
				else
					Options.Logger?.Invoke(this, new(tasks[taskIndex].DescriptionUndo, taskIndex, TaskCount, OperationDepth));

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
					Options.Logger?.Invoke(this, new($"Task {taskIndex} failed: {ex.Message}", taskIndex, TaskCount, OperationDepth));
					if (isDoing == targetDo)
					{
						if (tasks[taskIndex].CanFail)
						{
							Options.Logger?.Invoke(this, new($"Continuing with tasks...", taskIndex, TaskCount, OperationDepth));
						}
						else
						{
							Options.Logger?.Invoke(this, new($"Undoing tasks...", taskIndex, TaskCount, OperationDepth));
							isDoing = !targetDo;
						}
					}
					else
					{
						Options.Logger?.Invoke(this, new($"Ignoring exception, continuing to undo tasks...", taskIndex, TaskCount, OperationDepth));
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
			Options.Logger?.Invoke(this, new($"{whatHappened} action: {taskDescripion}", taskIndex, TaskCount, OperationDepth));

			if (isDoing != targetDo)
			{
				// we failed to do this task
				throw new OperationCanceledException();
			}
		}

		protected void AddCopyTask(string srcDir, string dstDir, string filename)
		{
			if (Options.CreateSymbolicLinks)
			{
				tasks.Add(new TaskCreateSymbolicLink(Path.Combine(dstDir, filename), Path.Combine(srcDir, filename)));
			}
			else
			{
				tasks.Add(new TaskCopyFile(Path.Combine(srcDir, filename), Path.Combine(dstDir, filename)));
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

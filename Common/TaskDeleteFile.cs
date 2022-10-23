using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UT4UU.Installer.Common
{
	//public class TaskDeleteFile : Task
	//{
	//	private string filepath;
	//	private string temporaryFilepath;


	//	public TaskDeleteFile(string filepath, string temporaryFilepath) : base($"Delete '{filepath}'")
	//	{
	//		this.filepath = filepath;
	//		this.temporaryFilepath = temporaryFilepath;
	//	}

	//	public override void Do()
	//	{
	//		File.Move(filepath, temporaryFilepath);
	//	}

	//	public override void Undo()
	//	{
	//		File.Move(temporaryFilepath, filepath);
	//	}

	//	public override void FinishDo()
	//	{
	//		File.Delete(temporaryFilepath);
	//	}

	//	public override void FinishUndo()
	//	{
	//	}
	//}
}

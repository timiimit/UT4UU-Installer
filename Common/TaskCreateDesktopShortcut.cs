using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UT4UU.Installer.Common
{
	public class TaskCreateDesktopShortcut : Task
	{
		string pointsTo;
		string arguments;
		string name;
		string description;

		public string DesktopDirectory
		{
			get
			{
				if (Environment.OSVersion.Platform == PlatformID.Unix)
					return "/usr/local/share/applications";
				//else if (Environment.OSVersion.Platform == PlatformID.Win32NT)
				return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory, Environment.SpecialFolderOption.None);
			}
		}

		public string FullPath
		{
			get
			{
				return Path.Combine(DesktopDirectory, Filename);
			}
		}

		public string Filename
		{
			get
			{
				if (Environment.OSVersion.Platform == PlatformID.Win32NT)
					return name + ".lnk";
				else if (Environment.OSVersion.Platform == PlatformID.Unix)
					return name + ".desktop";

				return name;
			}
		}

		public TaskCreateDesktopShortcut(string pointsTo, string arguments, string name, string description) :
			base(
				$"Create desktop shortcut '{name}'\npointing at '{pointsTo}'\nwith arguments '{arguments}'",
				$"Delete desktop shortcut '{name}'"
			)
		{
			this.pointsTo = pointsTo;
			this.arguments = arguments;
			this.name = name;
			this.description = description;
		}

		public override void Do()
		{
			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				WshShell shell = new();

				IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(FullPath);
				shortcut.TargetPath = pointsTo;
				shortcut.WorkingDirectory = Path.GetDirectoryName(pointsTo);
				shortcut.Arguments = arguments;
				shortcut.IconLocation = pointsTo + ", 0";
				shortcut.Description = description;
				shortcut.Hotkey = "Ctrl+Shift+U";
				shortcut.Save();
			}
			else if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				string content = $@"[Desktop Entry]
Type=Application
Version=1.0
Name={name}
Comment={description}
Path={Path.GetDirectoryName(pointsTo)}
Exec={Filename} {arguments}
Icon={Filename}
Terminal=false
Categories=Games;Entertainment;";

				System.IO.File.WriteAllText(FullPath, content);
			}
			else
			{
				System.IO.File.WriteAllText(FullPath,
					"Hi! I'm sorry, I wasn't sure how to make a shortcut for your system and frankly," +
					"I'm not even sure why you would need this tool on your machine, so I just made this file instead :)");
			}
		}

		public override void Undo()
		{
			System.IO.File.Delete(FullPath);
		}

		public override void FinishDo()
		{

		}

		public override void FinishUndo()
		{

		}
	}
}

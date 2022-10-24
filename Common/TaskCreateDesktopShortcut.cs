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
				$"Create desktop shortcut '{name}' pointing at '{pointsTo}' with arguments '{arguments}'",
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
				// create shortcut file manually
				File.WriteAllBytes(FullPath, new byte[0]);

				// modify the shortcut file
				var shell = new Shell32.Shell();
				var folder = shell.NameSpace(DesktopDirectory);
				var item = folder.Items().Item(Filename);
				var shortcut = (Shell32.ShellLinkObject)item.GetLink;

				shortcut.Path = pointsTo;
				shortcut.Description = description;
				shortcut.Arguments = arguments;
				shortcut.SetIconLocation(pointsTo, 0);
				shortcut.WorkingDirectory = Path.GetDirectoryName(pointsTo);

#if false
				StringBuilder sb = new StringBuilder();
				sb.AppendLine($"Path: {shortcutPath}");
				sb.AppendLine($"TargetPath: {shortcut.Path}");
				sb.AppendLine($"Arguments: {shortcut.Arguments}");
				sb.AppendLine($"IconLocation: {iconLocation}");
				sb.AppendLine($"WorkingDirectory: {shortcut.WorkingDirectory}");
				MessageBox.Show(sb.ToString());
#endif

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

				File.WriteAllText(FullPath, content);
			}
			else
			{
				File.WriteAllText(FullPath,
					"Hi! I'm sorry, I wasn't sure how to make a shortcut for your system and frankly," +
					"I'm not even sure why you would need this tool on your machine, so I just made this file instead :)");
			}
		}

		public override void Undo()
		{
			File.Delete(FullPath);
		}

		public override void FinishDo()
		{

		}

		public override void FinishUndo()
		{

		}
	}
}

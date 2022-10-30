using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UT4UU.Installer.Common
{
	public class OperationRefreshingExperience : Operation
	{
		public OperationRefreshingExperience(int depth, Options options, bool direction) : base("Install Refreshing Experience", "Uninstall Refreshing Experience", depth, options, direction)
		{
			string srcContent = Path.Combine(options.SourceLocation, "UnrealTournament", "Content");
			string srcMovies = Path.Combine(srcContent, "Movies");
			string srcSplash = Path.Combine(srcContent, "Splash");

			string dstContent = Path.Combine(options.InstallLocation, "UnrealTournament", "Content");
			string dstMovies = Path.Combine(dstContent, "Movies");
			string dstSplash = Path.Combine(dstContent, "Splash");

			tasks.Add(new TaskRenameFile(
				Path.Combine(dstMovies, "engine_startup.mp4"),
				"engine_startup.mp4" + Options.ReplacementSuffix
			));
			tasks.Add(new TaskRenameFile(
				Path.Combine(dstMovies, "intro_full.mp4"),
				"engine_startup.mp4"
			));
			tasks.Add(new TaskRenameFile(
				Path.Combine(dstSplash, "Splash.bmp"),
				"Splash.bmp" + options.ReplacementSuffix
			));
			AddCopyTask(srcSplash, dstSplash, "Splash.bmp");
		}
	}
}

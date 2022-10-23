using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UT4UU.Installer.Common
{
	public class OperationUninstall : OperationInstallation
	{
		public OperationUninstall(Options options) : base(options, false)
		{
		}
	}
}

using Avalonia.Controls;
using Avalonia.Interactivity;
using System.ComponentModel;
using UT4UU.Installer.GUI.ViewModels;

namespace UT4UU.Installer.GUI.Views
{
	public partial class MainWindow : Window
	{
		bool canClose;

		public MainWindow()
		{
			InitializeComponent();
			canClose = false;
		}

		public async void BrowseForInstallDirectory(object? sender, RoutedEventArgs e)
		{
			OpenFolderDialog dialog = new OpenFolderDialog();
			dialog.Title = "Installation Directory";
			string? selection = await dialog.ShowAsync(this);
			if (DataContext is MainWindowViewModel dc)
			{
				if (selection != null)
					dc.InstallLocation = selection;
			}
		}

		public void CloseInstaller(object? sender, RoutedEventArgs e)
		{
			canClose = true;
			Close();
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			// we want to prevent the window from closing unless we allow the user to do so
			if (!canClose)
				e.Cancel = true;

			base.OnClosing(e);
		}
	}
}
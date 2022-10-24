using Avalonia.Controls;
using Avalonia.Interactivity;
using UT4UU.Installer.GUI.ViewModels;

namespace UT4UU.Installer.GUI.Views
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
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
	}
}
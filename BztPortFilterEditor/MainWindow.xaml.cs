using System.Windows;

namespace BztPortFilterEditor
{
	/// <summary>
	/// Logique d'interaction pour Window1.xaml
	/// </summary>
	public partial class MainWindow : Window
	{		
		public MainWindow() {
			InitializeComponent();
			this.DataContext = new MainViewModel();
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Gafa.Dokan;


namespace Gafa
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			//MountsList.Instance.Add(new CustomFilesystem(@"U:\", @"C:\Temp"));
			MountsList.Instance.Add(new GitFilesystem(@"Y:\", @"Z:\Development\Gafa"));
		}

		private void Mount_Click(object sender, RoutedEventArgs e)
		{
			MountsList.Mount();
		}

		private void Unmount_Click(object sender, RoutedEventArgs e)
		{
			MountsList.Unmount();
		}
	}
}

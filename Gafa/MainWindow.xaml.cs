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
using Gafa.Patterns;
using Gafa.FileSystem;

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


			//Singleton<MountsList>.Instance.Add(new CustomFilesystem(@"U:\", @"C:\Temp"));
			var treeHandler = new TreeHandler("*");
			var repositoryPath = @"Z:\Development\Gafa";
			Singleton<MountsList>.Instance.m_FileSystems.Add(
				new GitFilesystem(
					@"Y:\",
					@"Z:\Development\Gafa",
					new RootHandler("", repositoryPath, new List<SubFolderHandler>()
					{
						new BranchesHandler("branches", new BranchHandler("*", treeHandler)),
						new TagHandler("tags", treeHandler)
					})));
		}

		private void Mount_Click(object sender, RoutedEventArgs e)
		{
			Singleton<MountsList>.Instance.Mount();
		}

		private void Unmount_Click(object sender, RoutedEventArgs e)
		{
			Singleton<MountsList>.Instance.Unmount();
		}
	}
}

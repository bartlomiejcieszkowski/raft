using System.Collections.Generic;
using System.Windows;
using Gafa.Dokan;
using Gafa.Patterns;
using Gafa.FileSystem;
using Gafa.Helper;
using NLog.Targets.Wrappers;
using NLog;
using NLog.Config;

namespace Gafa
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			Dispatcher.Invoke(() =>
			{
				var target = new WpfRichTextBoxTarget
				{
					Name = "RichText",
					Layout =
						"[${time:useUTC=false}] ${level:uppercase=true} ${callsite}::${message} ${exception:innerFormat=tostring:maxInnerExceptionLevel=10:separator=,:format=tostring}",
					ControlName = logRichTextBox.Name,
					FormName = GetType().Name,
					AutoScroll = true,
					MaxLines = 100000,
					UseDefaultRowColoringRules = true,
				};
				var asyncWrapper = new AsyncTargetWrapper { Name = "RichTextAsync", WrappedTarget = target };

				LogManager.Configuration.AddTarget(asyncWrapper.Name, asyncWrapper);
				LogManager.Configuration.LoggingRules.Insert(0, new LoggingRule("*", LogLevel.Trace, asyncWrapper));
				LogManager.ReconfigExistingLoggers();

			});
		}

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

		private void MainWindow_Closed(object sender, System.EventArgs e)
		{
			Singleton<MountsList>.Instance.Unmount();
		}
	}
}

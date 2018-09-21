using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Inflectra.SpiraTest.AddOns.JamaContourAdapter.ControlForm
{
	/// <summary>
	/// Interaction logic for ProcedureDialog.xaml
	/// </summary>
	public partial class ProcedureDialog : Window
	{
		#region Internal Variables
		private IProcedureComponent introductionComponent = null;
		private List<IProcedureComponent> configurationComponents = null;
		private IProcedureComponent confirmationComponent = null;
		private IProcedureProcessComponent progressComponent = null;
		private IProcedureComponent resultsComponent = null;

		private IProcedureComponent currentComponent = null;

		private bool procedureComplete = false;
		private bool processing = false;

		private bool closeConfirmed = false;
		public bool ignoreMin = false;

		private delegate void RefreshDelegate();
		#endregion

		/// <summary>Is thrown when navigation is changed. Sender is an int: 1 - Next; -1 - Back; 0 - Random. Change the .Handled on the RoutedEventArgs to specify whether it is okay to navigate or not.</summary>
		public event RoutedEventHandler NavigationChange;
		public event EventHandler AskedForClose;

		#region initialization
		public ProcedureDialog(String title, IProcedureComponent introductionComponent, List<IProcedureComponent> configurationComponents,
			IProcedureComponent confirmationComponent, IProcedureProcessComponent progressComponent, IProcedureComponent resultsComponent)
		{
			InitializeComponent();
			this.Title = title;

			#region store procedure components
			if (introductionComponent == null || confirmationComponent == null ||
				progressComponent == null || resultsComponent == null ||
				configurationComponents == null || configurationComponents.Count == 0)
			{
				throw new Exception("NULL IProcedureComponent or empty configuration components list passed to initialization of ProcedureDialog");
			}

			this.introductionComponent = introductionComponent;
			this.confirmationComponent = confirmationComponent;
			this.progressComponent = progressComponent;
			this.resultsComponent = resultsComponent;

			this.configurationComponents = new List<IProcedureComponent>();
			for (int i = 0; i < configurationComponents.Count; i++)
			{
				this.configurationComponents.Add(configurationComponents[i]);
			}
			#endregion

			#region populate key
			introductionLabel.Content = introductionComponent.KeyText;
			confirmationLabel.Content = confirmationComponent.KeyText;
			progressLabel.Content = progressComponent.KeyText;
			resultsLabel.Content = resultsComponent.KeyText;

			StackPanel sP = new StackPanel();
			sP.Orientation = Orientation.Vertical;
			configurationExpander.Content = sP;

			for (int i = 0; i < configurationComponents.Count; i++)
			{
				Label configurationLabel = new Label();
				configurationLabel.Content = configurationComponents[i].KeyText;
				configurationLabel.Margin = new Thickness(30, 0, 0, 0);
				configurationLabel.Height = 25;
				configurationLabel.Tag = configurationComponents[i];
				configurationLabel.MouseUp += new MouseButtonEventHandler(configurationLabel_MouseUp);
				sP.Children.Add(configurationLabel);
			}
			#endregion

			#region load introduction
			contentGrid.Children.Add((UserControl)introductionComponent);
			currentComponent = introductionComponent;
			#endregion

			#region hook finish process
			progressComponent.ProcedureCompleteDelegates.Add(new ProcedureCompleteDelegate(finishProcess));
			#endregion

			updateKeyLinks();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				//If we're not in Vista/Win7, throw a dummy exception.
				if (System.Environment.OSVersion.Version.Major < 6)
					throw new Exception("Under windows Vista.");

				// Obtain the window handle for WPF application
				IntPtr mainWindowPtr = new WindowInteropHelper(this).Handle;
				HwndSource mainWindowSrc = HwndSource.FromHwnd(mainWindowPtr);
				mainWindowSrc.CompositionTarget.BackgroundColor = Color.FromArgb(0, 255, 255, 255);

				// Get System Dpi
				System.Drawing.Graphics desktop = System.Drawing.Graphics.FromHwnd(mainWindowPtr);
				desktop.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
				float DesktopDpiX = desktop.DpiX;
				float DesktopDpiY = desktop.DpiY;

				//// Set Margins
				NonClientRegionAPI.MARGINS margins = new NonClientRegionAPI.MARGINS();

				//// Extend glass frame into client area
				//// Note that the default desktop Dpi is 96dpi. The  margins are
				//// adjusted for the system Dpi.
				margins.cxLeftWidth = Convert.ToInt32(5 * (DesktopDpiX / 96));
				margins.cxRightWidth = Convert.ToInt32(5 * (DesktopDpiX / 96));
				margins.cyTopHeight = Convert.ToInt32(((int)75 + 5) * (DesktopDpiX / 96));
				margins.cyBottomHeight = Convert.ToInt32((5 - 10) * (DesktopDpiX / 96));

				int hr = NonClientRegionAPI.DwmExtendFrameIntoClientArea(mainWindowSrc.Handle, ref margins);
				////
				if (hr < 0)
				{
					//DwmExtendFrameIntoClientArea Failed
				}


				headerGrid.Background = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255));  // White, 50% transparent
				buttonBorder.Background = new SolidColorBrush(Color.FromArgb(64, 255, 128, 64)); // Orange, 50% transparent.
				//buttonBorder.Background = new SolidColorBrush(Color.FromArgb(128, 138, 215, 255)); // Light Blue, 50% transparent.
				buttonBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255)); // White, 100% transparent.
				contentBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255)); // White, 100% transparent.
				contentBorder.Background = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)); // White, 10% transparent.
				//keyGrid
				//keyBorder.Background = new LinearGradientBrush(Color.FromArgb(128, 255, 255, 255), Color.FromArgb(128, 138, 215, 255), 90.0); // White, 50% transparent; Light Blue, 50% transparent.
				keyBorder.Background = new LinearGradientBrush(Color.FromArgb(128, 255, 255, 255), Color.FromArgb(64, 255, 128, 64), 90.0); // White, 50% transparent; Orange, 50% transparent.
				keyBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255)); // White, 100% transparent.
			}
			// If not Vista, paint background white.
			catch (Exception)
			{
				Application.Current.MainWindow.Background = System.Windows.SystemColors.ControlBrush;
				headerGrid.Background = System.Windows.SystemColors.ControlBrush;
				buttonBorder.Background = System.Windows.SystemColors.ControlBrush;
				buttonBorder.BorderBrush = System.Windows.SystemColors.ControlLightLightBrush;
				keyBorder.Background = System.Windows.SystemColors.ControlBrush;
				keyBorder.BorderBrush = System.Windows.SystemColors.ControlLightLightBrush;
				contentBorder.Background = System.Windows.SystemColors.ControlBrush;
				contentBorder.BorderBrush = System.Windows.SystemColors.ControlLightLightBrush;
			}
		}
		#endregion

		#region key handlers
		private void introductionLabel_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (currentComponent != introductionComponent && currentComponent != progressComponent &&
				currentComponent != resultsComponent)
			{
				this.NavigationChange((int)-1, e);

				if (e.Handled)
				{

					currentComponent = introductionComponent;
					contentGrid.Children.Clear();
					contentGrid.Children.Add((UserControl)currentComponent);

					updateButtons();
				}
			}
		}

		private void configurationLabel_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (currentComponent != (IProcedureComponent)((Label)sender).Tag && currentComponent != progressComponent &&
				currentComponent != resultsComponent)
			{
				this.NavigationChange((int)-1, e);

				if (e.Handled)
				{
					currentComponent = (IProcedureComponent)((Label)sender).Tag;
					contentGrid.Children.Clear();
					contentGrid.Children.Add((UserControl)currentComponent);

					updateButtons();
				}
			}
		}

		private void confirmationLabel_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (currentComponent != confirmationExpander && currentComponent != progressComponent &&
				currentComponent != resultsComponent)
			{
				this.NavigationChange((int)-1, e);

				if (e.Handled)
				{
					currentComponent = confirmationComponent;
					contentGrid.Children.Clear();
					contentGrid.Children.Add((UserControl)currentComponent);

					updateButtons();
				}
			}
		}

		private void progressLabel_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (currentComponent == resultsComponent)
			{
				this.NavigationChange((int)-1, e);

				if (e.Handled)
				{
					currentComponent = progressComponent;
					contentGrid.Children.Clear();
					contentGrid.Children.Add((UserControl)currentComponent);

					updateButtons();
				}
			}
		}

		private void resultsLabel_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (procedureComplete && currentComponent == progressComponent)
			{
				this.NavigationChange((int)-1, e);

				if (e.Handled)
				{
					currentComponent = resultsComponent;
					contentGrid.Children.Clear();
					contentGrid.Children.Add((UserControl)currentComponent);

					updateButtons();
				}
			}
		}
		#endregion

		#region back / next / finish handlers
		private void backButton_Click(object sender, RoutedEventArgs e)
		{
			this.NavigationChange((int)-1, e);

			if (e.Handled)
			{
				currentComponent = getBack(currentComponent);
				contentGrid.Children.Clear();
				contentGrid.Children.Add((UserControl)currentComponent);

				updateButtons();
			}
		}

		private void nextButton_Click(object sender, RoutedEventArgs e)
		{
			this.NavigationChange((int)1, e);

			if (e.Handled)
			{
				currentComponent = getNext(currentComponent);
				contentGrid.Children.Clear();
				contentGrid.Children.Add((UserControl)currentComponent);

				updateButtons();
			}
		}

		private void finishButton_Click(object sender, RoutedEventArgs e)
		{
			if (currentComponent == confirmationComponent)
			{
				#region confirm action
				if (ShowProcessWarning)
				{
					if (MessageBox.Show(this, "Are you sure you wish to continue?\r\n\r\nPerforming this operation may not be reversable or cancellable, and may take considerable time.", this.Title, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.Cancel)
					{
						return;
					}
				}
				#endregion

				currentComponent = getNext(currentComponent);
				contentGrid.Children.Clear();
				contentGrid.Children.Add((UserControl)currentComponent);

				updateButtons();

				processing = true;

				progressComponent.startProcedure();
			}
			else if (currentComponent == progressComponent)
			{
				currentComponent = getNext(currentComponent);
				contentGrid.Children.Clear();
				contentGrid.Children.Add((UserControl)currentComponent);
				updateButtons();
			}
			else if (currentComponent == resultsComponent)
			{
				procedureComplete = true;
				if (this.AskedForClose != null)
					this.AskedForClose(this, new EventArgs());
				else
					this.Close();
			}
		}

		private void updateButtons()
		{
			if (currentComponent == confirmationComponent)
			{
				backButton.IsEnabled = true;
				nextButton.IsEnabled = false;
				finishButton.IsEnabled = true;
				finishButton.Content = "Import";
			}
			else if (currentComponent == introductionComponent)
			{
				backButton.IsEnabled = false;
				nextButton.IsEnabled = true;
				finishButton.IsEnabled = false;
				finishButton.Content = "Import";
			}
			else if (currentComponent == progressComponent)
			{
				backButton.IsEnabled = false;
				nextButton.IsEnabled = false;
				finishButton.IsEnabled = procedureComplete;
				cancelButton.IsEnabled = !procedureComplete;
			}
			else if (currentComponent == resultsComponent)
			{
				backButton.IsEnabled = true;
				nextButton.IsEnabled = false;
				finishButton.IsEnabled = true;
				finishButton.Content = "Finish";
				cancelButton.IsEnabled = false;
			}
			else
			{
				backButton.IsEnabled = true;
				nextButton.IsEnabled = true;
				finishButton.IsEnabled = false;
				finishButton.Content = "Import";
			}

			updateKeyLinks();
		}

		private void updateKeyLinks()
		{
			if (currentComponent == introductionComponent || currentComponent == confirmationComponent)
			{
				introductionLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
				UIElementCollection col = ((StackPanel)(configurationExpander.Content)).Children;
				for (int i = 0; i < col.Count; i++)
				{
					((Label)col[i]).Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
				}
				confirmationLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
				progressLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
				resultsLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
			}
			else if (currentComponent == progressComponent)
			{
				if (procedureComplete)
				{
					introductionLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
					UIElementCollection col = ((StackPanel)(configurationExpander.Content)).Children;
					for (int i = 0; i < col.Count; i++)
					{
						((Label)col[i]).Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
					}
					confirmationLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
					progressLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
					resultsLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
				}
				else
				{
					introductionLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
					UIElementCollection col = ((StackPanel)(configurationExpander.Content)).Children;
					for (int i = 0; i < col.Count; i++)
					{
						((Label)col[i]).Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
					}
					confirmationLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
					progressLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
					resultsLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
				}
			}
			else if (currentComponent == resultsComponent)
			{
				introductionLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
				UIElementCollection col = ((StackPanel)(configurationExpander.Content)).Children;
				for (int i = 0; i < col.Count; i++)
				{
					((Label)col[i]).Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
				}
				confirmationLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
				progressLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
				resultsLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
			}
			else
			{
				introductionLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
				UIElementCollection col = ((StackPanel)(configurationExpander.Content)).Children;
				for (int i = 0; i < col.Count; i++)
				{
					((Label)col[i]).Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
				}
				confirmationLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
				progressLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
				resultsLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
			}

			#region update key expanders
			introductionExpander.IsExpanded = false;
			configurationExpander.IsExpanded = false;
			confirmationExpander.IsExpanded = false;
			progressExpander.IsExpanded = false;
			resultsExpander.IsExpanded = false;

			if (currentComponent == introductionComponent)
				introductionExpander.IsExpanded = true;
			else if (configurationComponents.Contains(currentComponent))
				configurationExpander.IsExpanded = true;
			else if (currentComponent == confirmationComponent)
				confirmationExpander.IsExpanded = true;
			else if (currentComponent == progressComponent)
				progressExpander.IsExpanded = true;
			else
				resultsExpander.IsExpanded = true;
			#endregion

		}

		private IProcedureComponent getNext(IProcedureComponent current)
		{
			if (currentComponent == introductionComponent)
			{
				return configurationComponents[0];
			}
			else if (currentComponent == confirmationComponent)
			{
				return progressComponent;
			}
			else if (currentComponent == progressComponent)
			{
				return resultsComponent;
			}
			else if (currentComponent == resultsComponent)
			{
				return null;
			}
			else
			{
				for (int i = 0; i < configurationComponents.Count - 1; i++)
				{
					if (currentComponent == configurationComponents[i])
					{
						return configurationComponents[i + 1];
					}
				}

				return confirmationComponent;
			}
		}

		private IProcedureComponent getBack(IProcedureComponent current)
		{
			if (currentComponent == introductionComponent)
			{
				return null;
			}
			else if (currentComponent == confirmationComponent)
			{
				return configurationComponents[configurationComponents.Count - 1];
			}
			else if (currentComponent == progressComponent)
			{
				return null;
			}
			else if (currentComponent == resultsComponent)
			{
				return progressComponent;
			}
			else
			{
				for (int i = 1; i < configurationComponents.Count; i++)
				{
					if (currentComponent == configurationComponents[i])
					{
						return configurationComponents[i - 1];
					}
				}

				return introductionComponent;
			}
		}
		#endregion

		#region cancel / close handlers
		private void cancelButton_Click(object sender, RoutedEventArgs e)
		{
			if (processing)
			{
				if (MessageBox.Show(this, "Are you sure you want to cancel processing?", this.Title, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
				{
					Thread t = new Thread(new ThreadStart(waitForCancel));
					t.Name = "Wait for cancel thread";
					t.Priority = ThreadPriority.BelowNormal;
					t.Start();

					this.ignoreMin = true;
				}
			}
			else if (procedureComplete)
			{
				closeConfirmed = true;
				this.Close();
			}
			else
			{
				if (MessageBox.Show(this, "Are you sure you want to cancel this wizard?", this.Title, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
				{
					closeConfirmed = true;
					this.Close();
					this.ignoreMin = true;
				}
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (!closeConfirmed)
			{
				if (processing)
				{
					e.Cancel = true;
					if (MessageBox.Show(this, "Are you sure you want to cancel processing?", this.Title, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
					{
						e.Cancel = true;

						Thread t = new Thread(new ThreadStart(waitForCancel));
						t.Name = "Wait for cancel thread";
						t.Priority = ThreadPriority.BelowNormal;
						t.Start();
					}
				}
				else if (procedureComplete)
				{
					e.Cancel = false;
				}
				else
				{
					if (MessageBox.Show(this, "Are you sure you want to cancel this wizard?", this.Title, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.No)
					{
						e.Cancel = true;
					}
					else
					{
						e.Cancel = false;
						this.ignoreMin = true;
					}
				}
			}
			else
			{
				e.Cancel = false;
			}
		}

		private void waitForCancel()
		{
			bool result = progressComponent.cancelProcedure();
			//Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new RefreshDelegate(cancelDialog.Hide));

			if (result)
			{
				closeConfirmed = true;
				//Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new RefreshDelegate(this.Close));
			}
			else
			{
				Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new RefreshDelegate(this.waitForCancelFail));
			}
		}

		private void waitForCancelFail()
		{
			MessageBox.Show(this, "Error cancelling procedure...\r\n\r\nEither the operation cannot be cancelled or the process cannot be terminated at this time; check the user documentation for more information.", this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
		}
		#endregion

		#region processing handlers
		private void finishProcess()
		{
			finishButton.Content = "Finish";
			finishButton.IsEnabled = true;
			cancelButton.IsEnabled = false;
			procedureComplete = true;
			processing = false;

			Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new RefreshDelegate(this.updateKeyLinks));
		}
		#endregion

		#region accessors
		public IProcedureComponent IntroductionComponent
		{
			get
			{
				return introductionComponent;
			}
		}

		public List<IProcedureComponent> ConfigurationComponents
		{
			get
			{
				List<IProcedureComponent> cc = new List<IProcedureComponent>();
				for (int i = 0; i < configurationComponents.Count; i++)
				{
					cc.Add(configurationComponents[i]);
				}
				return cc;
			}
		}

		public IProcedureComponent ConfirmationComponent
		{
			get
			{
				return confirmationComponent;
			}
		}

		public IProcedureProcessComponent ProgressComponent
		{
			get
			{
				return progressComponent;
			}
		}

		public IProcedureComponent ResultsComponent
		{
			get
			{
				return resultsComponent;
			}
		}

		public String HeaderTitle
		{
			get
			{
				return headingLabel.Content.ToString();
			}
			set
			{
				headingLabel.Content = value;
			}
		}

		public String HeaderDescription
		{
			get
			{
				return summaryLabel.Content.ToString();
			}
			set
			{
				summaryLabel.Content = value;
			}
		}

		public bool ShowProcessWarning
		{
			get;
			set;
		}
		#endregion

		/// <summary>Will return or set the # of the screen the user is on, -1 for introduction, -2 for progress, and -3 for Finished.</summary>
		public IProcedureComponent currentScreen
		{
			get
			{
				return this.currentComponent;
			}
			set
			{
				currentComponent = value;

				contentGrid.Children.Clear();
				contentGrid.Children.Add((UserControl)currentComponent);

				updateButtons();
			}
		}
	}
}

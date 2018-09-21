using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Inflectra.SpiraTest.AddOns.JamaContourAdapter.UI
{
	/// <summary>
	/// Interaction logic for ItemProgress.xaml
	/// </summary>
	public partial class ItemProgress : UserControl
	{
		private ProcessStatusEnum _CurProcess = ProcessStatusEnum.None;

		public ItemProgress()
		{
			InitializeComponent();
		}

		/// <summary>The status image of the item.</summary>
		public enum ProcessStatusEnum
		{
			/// <summary>None - No image, leaves it as a 'to do'.</summary>
			None = 0,
			/// <summary>Processing - gives the item a highlightd blue arrow.</summary>
			Processing = 1,
			/// <summary>Error - An error ocured during processing. Gives the item a red X.</summary>
			Error = 2,
			/// <summary>Success - Successful processing, move on to the next item.</summary>
			Success = 3
		}
		
		/// <summary>Sets the action text.</summary>
		public string SetActionName
		{
			get
			{
				return this.txtName.Text;
			}
			set
			{
				this.txtName.Text = value;
			}
		}

		/// <summary>Sets the error message, if needed,</summary>
		public string SetErrorString
		{
			get
			{
				return this.txtError.Text;
			}
			set
			{
				this.txtError.Text = value;
				this.txtError.Visibility = ((string.IsNullOrEmpty(value)) ? Visibility.Collapsed : Visibility.Visible);
			}
		}

		/// <summary>Sets the satatus of the item.</summary>
		public ProcessStatusEnum SetActionStatus
		{
			get
			{
				return this._CurProcess;
			}
			set
			{
				this._CurProcess = value;
				this.imgItem.Visibility = ((value == ProcessStatusEnum.None) ? Visibility.Hidden : Visibility.Visible);
				switch (value)
				{
				    case ProcessStatusEnum.Error:
				        this.imgItem.Source = new BitmapImage(new Uri("pack://application:,,,/JamaContourAdapter;component/_Resources/Error.png"));
						break;
				    case ProcessStatusEnum.Processing:
                        this.imgItem.Source = new BitmapImage(new Uri("pack://application:,,,/JamaContourAdapter;component/_Resources/Processing.png"));
						break;
					case ProcessStatusEnum.Success:
                        this.imgItem.Source = new BitmapImage(new Uri("pack://application:,,,/JamaContourAdapter;component/_Resources/Success.png"));
						break;
				}
			}
		}
	}
}

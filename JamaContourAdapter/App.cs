using System.Data.OleDb;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Threading;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

namespace Inflectra.SpiraTest.AddOns.JamaContourAdapter
{
	partial class App
	{
		public static bool IsDebug;
		public const string CANCELSTRING = "Process Canceled";

		/// <summary>Initial launch, creates the actual Application class.</summary>
		[STAThread]
		public static void Main()
		{
			//Set our thread name for tracing.
            Thread.CurrentThread.Name = Properties.Resources.Global_ApplicationName;

			//Generate application.
			System.Windows.Application thisApp = new System.Windows.Application();
			thisApp.DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);

			//Get our settings so we can see if debug is on or off.
            App.IsDebug = Properties.Settings.Default.DebugEnabled;

			thisApp.Run(new UI.wpfMasterForm());
		}

		/// <summary>Hit whenever an uncaught exception occurs. Writes to the eventlog, displays a message, and terminates the program.</summary>
		/// <param name="sender">App</param>
		/// <param name="e">EventArgs</param>
		internal static void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			//Create event logging source if needed.

            if (!EventLog.SourceExists(Properties.Resources.Global_ApplicationName))
                EventLog.CreateEventSource(Properties.Resources.Global_ApplicationName, "Application");

			//Get error messages.
			Exception ex = e.Exception;
			string errMsg = ex.Message;
			while (ex.InnerException != null)
			{
				errMsg += Environment.NewLine + ex.InnerException.Message;
				ex = ex.InnerException;
			}
			//Add stack trace.
			errMsg += Environment.NewLine + Environment.NewLine + "Stack Trace:" + Environment.NewLine;
			errMsg += e.Exception.StackTrace;

            EventLog.WriteEntry(Properties.Resources.Global_ApplicationName, errMsg, EventLogEntryType.Error);

			System.Windows.MessageBox.Show("An error occurred that was unrecoverable." + Environment.NewLine + "The full error details were saved to the error log. Contact support if the error happens again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			Environment.Exit(-1);
		}
	}
}

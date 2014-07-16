#region Namesapces

using NLog;
using System;
using System.Windows.Forms;

#endregion

namespace Panoptes
{
    static class Program
	{
		#region Variables

		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		#endregion

		#region Entry Point

		/// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
			AppDomain.CurrentDomain.UnhandledException += UnhandledException;

			Application.ThreadException += ThreadException;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

		#endregion

		#region Exception Events

		/// <summary>
		/// 쓰레드에서 예외가 발생한 경우
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		static void ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
		{
			if (e.Exception == null) return;

			logger.Fatal(e.Exception.Message, e);
		}

		/// <summary>
		/// 처리되지 않은 예외가 발생한 경우
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			if (e.ExceptionObject == null || !(e.ExceptionObject is Exception)) return;

			Exception error = e.ExceptionObject as Exception;

			logger.Fatal(error.Message, error);
		}

		#endregion
	}
}
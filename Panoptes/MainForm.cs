#region Namespaces

using Argos.Panoptes.Rfb.Socket;
using System.Windows.Forms;

#endregion

namespace Panoptes
{
	public partial class MainForm : Form
	{
		#region Constructor

		public MainForm()
		{
			InitializeComponent();

			var rfbServer = new RfbServer();
		}

		#endregion
	}
}
#region Namespaces

using Argos.Panoptes.Rfb;
using SuperWebSocket;

#endregion

namespace Argos.Panoptes.Rfb.Socket
{
	/// <summary>
	/// RFB 클라이언트 정보
	/// </summary>
	class RfbClient
	{
		#region Properties

		/// <summary>
		/// 웹소켓 세션
		/// </summary>
		internal WebSocketSession Session
		{
			get;
			private set;
		}

		/// <summary>
		/// 다른 클라이언트와 서버를 공유할지 여부
		/// </summary>
		internal bool Shared
		{
			get;
			private set;
		}

		/// <summary>
		/// 픽셀 포맷
		/// </summary>
		internal PixelFormat PixelFormat
		{
			get;
			set;
		}

		#endregion

		#region Constructor

		public RfbClient(WebSocketSession session, bool shared, PixelFormat pixelFormat)
		{
			this.Session = session;
			this.Shared = shared;
			this.PixelFormat = pixelFormat;
		}

		#endregion
	}
}
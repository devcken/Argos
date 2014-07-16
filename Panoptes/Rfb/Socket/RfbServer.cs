#region Namespaces

using Argos.Panoptes.Graphics;
using Argos.Panoptes.Rfb.Protocol;
using NLog;
using SuperWebSocket;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

#endregion

namespace Argos.Panoptes.Rfb.Socket
{
	/// <summary>
	/// RFB 서버
	/// </summary>
	class RfbServer
	{
		#region Constants

		/// <summary>
		/// RFB 기본 포트 번호
		/// </summary>
		private static readonly int DEFAULT_PORT = 5900;

		/// <summary>
		/// RFB 서버 프로코롤 버전
		/// 현재 버전은 3.8을 지원한다.
		/// </summary>
		private static readonly ProtocolVersion SERVER_PROTOCOL_VERSION = new ProtocolVersion(3, 8);

		#endregion

		#region Variables

		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// RFB 웹소켓 서버
		/// </summary>
		private WebSocketServer server;

		/// <summary>
		/// 기본 ServerInit
		/// </summary>
		private ServerInit serverInit = ServerInit.MakeDefault();

		/// <summary>
		/// 각 클라이언트의 핸드쉐이크 단계
		/// </summary>
		private ConcurrentDictionary<string, HandshakingPhase> clientsPhase = new ConcurrentDictionary<string,HandshakingPhase>();

		/// <summary>
		/// 클라이언트로부터 전달되는 프로토콜 메시지 핸들러
		/// </summary>
		private MessageHandler messageHandler;

		/// <summary>
		/// 
		/// </summary>
		private FrameBitmapGrabber screenGrabber;

		#endregion

		#region Properties

		/// <summary>
		/// 인증된 클라이언트 목록
		/// </summary>
		public ConcurrentDictionary<string, RfbClient> Clients
		{
			get;
			private set;
		}

		#endregion

		#region Constructor

		public RfbServer(int port = 0) {
			this.serverInit = ServerInit.MakeDefault();

			this.Clients = new ConcurrentDictionary<string, RfbClient>();

			Microsoft.Win32.SystemEvents.DisplaySettingsChanged += DisplaySettingsChanged;

			HandshakingHandler.ClientAuthenticated += ClientAuthenticated;

			this.messageHandler = new MessageHandler(); 

			this.screenGrabber = new FrameBitmapGrabber();

			this.screenGrabber.Grabbed += Grabbed;

			this.server = new WebSocketServer();

			this.server.NewSessionConnected += NewSessionConnected;
			this.server.SessionClosed += SessionClosed;
			this.server.NewDataReceived += NewDataReceived;

			this.server.Setup(port == 0 ? DEFAULT_PORT : port);

			this.server.Start();
		}

		#endregion

		#region System Events

		/// <summary>
		/// 윈도우의 디스플레이 설정이 변경된 경우
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void DisplaySettingsChanged(object sender, EventArgs e)
		{
			// 스크린 해상도를 업데이트한다.
			this.serverInit.ChangeFramebufferResolution(SystemInformation.VirtualScreen.Width, SystemInformation.VirtualScreen.Height);
		}

		#endregion

		#region WebSocket Events

		/// <summary>
		/// 세션으로부터 바이너리 데이터를 전달받은 경우
		/// </summary>
		/// <param name="session">웹소켓 세션</param>
		/// <param name="value">바이너리 데이터</param>
		private void NewDataReceived(WebSocketSession session, byte[] value)
		{
			if (!this.clientsPhase.ContainsKey(session.SessionID))
			{
				session.Close();

				return;
			}

			// 해당 세션이 핸드쉐이크된 상태인지 아닌지를 판단하여 처리한다.
			if (this.clientsPhase[session.SessionID] == HandshakingPhase.Initialized)
			{
				this.messageHandler.Handle(session, value);
			}
			else
			{
				HandshakingHandler.Handle(session, value, this.clientsPhase[session.SessionID]);

				this.clientsPhase[session.SessionID]++;
			}
		}

		/// <summary>
		/// 새로운 세션이 접속한 경우
		/// </summary>
		/// <param name="session">웹소켓 세션</param>
		private void NewSessionConnected(WebSocketSession session)
		{
			logger.Debug("New session connected @ {0}", session.SessionID);

			this.clientsPhase.TryAdd(session.SessionID, HandshakingPhase.NotYet);

			// 서버 프로토콜 버전을 전달하여 핸드쉐이크를 시작한다.
			session.Send(SERVER_PROTOCOL_VERSION.ToBinary());
		}

		/// <summary>
		/// 세션이 종료된 경우
		/// </summary>
		/// <param name="session">웹소켓 세션</param>
		/// <param name="value">세션이 닫힌 이유</param>
		private void SessionClosed(WebSocketSession session, SuperSocket.SocketBase.CloseReason value)
		{
			if (this.clientsPhase.ContainsKey(session.SessionID))
			{
				if (this.clientsPhase[session.SessionID] == HandshakingPhase.Initialized &&
					this.Clients.ContainsKey(session.SessionID))
				{
					RfbClient client;

					this.Clients.TryRemove(session.SessionID, out client);
				}

				HandshakingPhase phase = HandshakingPhase.NotYet;

				this.clientsPhase.TryRemove(session.SessionID, out phase);
			}

			if (this.Clients.Count == 0)
			{
				this.screenGrabber.Stop();
			}

			logger.Debug("Session closed @ {0}", session.SessionID);
		}

		#endregion

		#region Handshaking Handler Events

		/// <summary>
		/// 클라이언트가 인증된 경우
		/// </summary>
		/// <param name="sesssion">인증된 클라이언트의 웹소켓 세션</param>
		/// <param name="shared">다른 클라이언트와 서버를 공유할지 여부</param>
		void ClientAuthenticated(WebSocketSession session, bool shared)
		{
			this.Clients.TryAdd(session.SessionID, new RfbClient(session, shared, this.serverInit.PixelFormat));

			session.Send(this.serverInit.ToBinary());

			this.screenGrabber.Start();
		}

		#endregion

		#region ScreenGrabber Events

		/// <summary>
		/// Framebuffer가 수집된 경우
		/// </summary>
		/// <param name="bounds">비트맵의 바운드</param>
		/// <param name="image">비트맵 데이터</param>
		//void Grabbed(Rectangle bounds, System.Drawing.Bitmap image)
		void Grabbed(List<BoundBitmap> boundBitmaps)
		{
			foreach (BoundBitmap boundBitmap in boundBitmaps)
			{
				var messageBinary = FramebufferUpdate.ToMessage(boundBitmap.Bound, boundBitmap.Bitmap);

				// 접속 중인 클라이언트들로 Framebuffer 정보를 전달한다.
				foreach (var item in this.Clients.Values)
				{
					item.Session.Send(messageBinary);
				}
			}
		}

		#endregion
	}
}
#region Namespaces

using Argos.Panoptes.Rfb.Authentication;
using NLog;
using SuperWebSocket;
using System;
using System.Collections.Concurrent;

#endregion

namespace Argos.Panoptes.Rfb.Protocol
{
	/// <summary>
	/// 핸드쉐이크 프로토콜 메시지 처리자
	/// </summary>
	static class HandshakingHandler
	{
		#region Variables

		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// 클라이언트별 인증 처리자 목록
		/// </summary>
		private static ConcurrentDictionary<string, Authenticator> authenticator = new ConcurrentDictionary<string, Authenticator>();

		#endregion

		#region Events and Delegates

		/// <summary>
		/// ClientAuthenticated 이벤트 핸들러
		/// </summary>
		/// <param name="session">웹소켓 세션</param>
		/// <param name="shared">서버 공유 여부</param>
		public delegate void ClientAuthenticatedEventHandler(WebSocketSession session, bool shared);

		/// <summary>
		/// 클라이언트가 인증됐을 때 발생한다.
		/// </summary>
		public static event ClientAuthenticatedEventHandler ClientAuthenticated;

		#endregion

		#region Methods

		/// <summary>
		/// 핸드쉐이크 메시지를 처리한다.
		/// </summary>
		/// <param name="session">웹소켓 세션</param>
		/// <param name="data">메시지 데이터</param>
		/// <param name="phase">핸드쉐이크 단계</param>
		public static void Handle(WebSocketSession session, byte[] data, HandshakingPhase phase)
		{
			// 핸드쉐이크 단계에 따라 진행한다.
			switch (phase)
			{
				// 1 단계: 핸드쉐이크를 시작한다.
				// 클라이언트 측으로부터 프로토콜 버전을 받은 후 지원하는 암호화 유형의 목록을 전달한다.
				case HandshakingPhase.NotYet:
					ProtocolVersion clientVersion = new ProtocolVersion(data);

					if (logger.IsDebugEnabled)
					{
						logger.Debug("Session @ {0}, Protocol version: {1}", session.SessionID, clientVersion.ToString());
					}

					session.Send(new SecurityTypeList().ToBinary());

					break;

				// 2 단계: 클라이언트가 사용할 암호화 타입을 받고, 인증 단계로 넘어간다.
				case HandshakingPhase.SecurityTypeEnumerated:
					DecidedSecurityType securityType = new DecidedSecurityType(data);
					
					HandshakingHandler.authenticator.TryAdd(session.SessionID, securityType.Authenticator);

					// 인증 단계를 시작하기 위해, challenge 정보(평문)를 클라이언트로 전달한다.
					session.Send(new ArraySegment<byte>(securityType.Authenticator.Initialize("12345678")));

					break;

				// 3 단계: 클라이언트의 암호문을 받고 원본 암호문과 비교한다.
				// 인증 결과는 클라이언트로 전달된다.
				case HandshakingPhase.SecurityTypeDecided:
					if (!HandshakingHandler.authenticator.ContainsKey(session.SessionID))
					{
						session.Close();
					}

					Authenticator authenticator = HandshakingHandler.authenticator[session.SessionID];

					// 클라이언트의 암호문과 원본 암호문을 비교한다.
					bool authenticated = authenticator.Authenticate(data);

					SecurityResult securityResult = new SecurityResult(authenticated, string.Empty);

					session.Send(securityResult.ToBinary());

					HandshakingHandler.authenticator.TryRemove(session.SessionID, out authenticator);

					break;

				// 4 단계: 인증이 성공했을 경우, 클라이언트로부터 ClientInit 메시지가 전달된다.
				case HandshakingPhase.Authenticated:
					ClientInit clientInit = new ClientInit(data);

					// 클라이언트 인증 성공을 알린다.
					// ServerInit 메시지가 전달되어야 하나, 원활한 처리를 위해 RfbServer 측에서 직접 처리한다.
					ClientAuthenticated(session, clientInit.Shared);

					break;
			}
		}

		#endregion
	}
}
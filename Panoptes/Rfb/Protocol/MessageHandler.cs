#region Namespaces

using Argos.Panoptes.Interfaces.Key;
using Argos.Panoptes.Interfaces.Pointer;
using SuperWebSocket;
using System;
using System.IO;

#endregion

namespace Argos.Panoptes.Rfb.Protocol
{
	/// <summary>
	/// RFB 클라이언트로부터 전달되는 프로토콜 메시지 처리자
	/// </summary>
	class MessageHandler
	{
		#region Constructor

		public MessageHandler()
		{

		}

		#endregion

		#region Methods

		/// <summary>
		/// 메시지 타입을 분류하여 각각에 타입에 맞는 핸들러로 처리한다.
		/// </summary>
		/// <param name="session">웹소켓 세션</param>
		/// <param name="value">프로토콜 메시지 데이터</param>
		public void Handle(WebSocketSession session, byte[] value)
		{
			using (MemoryStream stream = new MemoryStream(value))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					// 첫번째 바이트를 읽어 메시지 타입을 구별해낸다.
					ClientToServerMessage messageType = (ClientToServerMessage)Enum.ToObject(typeof(ClientToServerMessage), reader.ReadByte());

					switch (messageType)
					{
						// 키보드 이벤트
						case ClientToServerMessage.KeyEvent:

							TypeWriter.Write(reader);

							break;

						// 포인터 이벤트(마우스, 디지타이트 등)
						case ClientToServerMessage.PointerEvent:

							PointMapper.Map(reader);

							break;
					}
				}
			}
		}

		#endregion
	}
}
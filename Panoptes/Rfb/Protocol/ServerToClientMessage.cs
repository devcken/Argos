namespace Argos.Panoptes.Rfb
{
	/// <summary>
	/// 서버로부터 클라이언트로 전달된 프로토콜 메시지의 열거자
	/// </summary>
	enum ServerToClientMessage : byte
	{
		FramebufferUpdate = 0x00,
		SetColourMapEntries = 0x01,
		Bell = 0x02,
		ServerCutText = 0x03
	}
}
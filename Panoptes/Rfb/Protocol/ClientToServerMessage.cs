namespace Argos.Panoptes.Rfb
{
	/// <summary>
	/// 클라이언트로부터 서버로 전달되는 메시지 타입의 열거자
	/// </summary>
	enum ClientToServerMessage : byte
	{
		SetPixelFormat = 0x00,
		SetEncodings = 0x02,
		FramebufferUpdateRequest = 0x03,
		KeyEvent = 0x04,
		PointerEvent = 0x05,
		ClientCutText = 0x06
	}
}
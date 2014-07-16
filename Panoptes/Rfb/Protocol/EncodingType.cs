namespace Argos.Panoptes.Rfb
{
	/// <summary>
	/// Framebuffer에 대한 인코딩 타입 열거자
	/// 현재는 Raw 타입만 사용한다.
	/// </summary>
	enum EncodingType : int
	{
		Raw = 0,
		CopyRect = 1,
		RRE = 2,
		Hextile = 5,
		ZRLE = 16,
		Cursor_pseudo_encoding = -239,
		DesktopSize_pseudo_encoding = -223
	}
}
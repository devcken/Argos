namespace Argos.Panoptes.Rfb
{
	/// <summary>
	/// 지원되는 암호화 타입 열거자
	/// </summary>
	enum SecurityTypes : byte
	{
		Invalid = 0x00,
		None = 0x01,
		VNCAuthentication = 0x02,
		//RA2 = 5,
		//RA2ne = 6,
		//Tight = 16,
		//Ultra = 17,
		//TLS = 18,
		//VenCrypt = 19,
		//GTK_VNC_SASL = 20,
		//MD5_hash_authenticaiton = 21,
		//Colin_Dean_xvp = 22
	}
}
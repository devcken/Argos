namespace Argos.Panoptes.Rfb.Protocol
{
	/// <summary>
	/// 핸드쉐이크 단계 열거자
	/// </summary>
	enum HandshakingPhase : int
	{
		NotYet = 0,
		SecurityTypeEnumerated = 1,
		SecurityTypeDecided = 2,
		Authenticated = 3,
		Initialized = 4
	}
}
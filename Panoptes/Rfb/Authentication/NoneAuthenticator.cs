namespace Argos.Panoptes.Rfb.Authentication
{
	/// <summary>
	/// 비인증 처리자
	/// RFB 서버가 클라이언트를 인증하지 않을 때 이 구현이 사용된다.
	/// </summary>
	class NoneAuthenticator : Authenticator
	{
		#region Overriding Methods

		public override byte[] Initialize(string key)
		{
			return null;
		}

		public override bool Authenticate(byte[] target)
		{
			// 항상 인증 성공
			return true;
		}

		#endregion
	}
}
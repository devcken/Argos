namespace Argos.Panoptes.Rfb.Authentication
{
	/// <summary>
	/// 인증 핸들링 프로토타입
	/// </summary>
	abstract class Authenticator
	{
		#region Variables

		/// <summary>
		/// 사전에 암호화된 원본 데이터
		/// </summary>
		protected byte[] ciphertext;

		#endregion

		#region Abstract Methods

		/// <summary>
		/// 인증을 준비한다.
		/// </summary>
		/// <param name="key">인증 키</param>
		/// <returns>무작위로 만들어진 평문</returns>
		public abstract byte[] Initialize(string key);

		/// <summary>
		/// 인증을 처리한다.
		/// </summary>
		/// <param name="userCiphertext">사용자가 입력한 평문을 암호화한 데이터</param>
		/// <returns>true인 경우, 인증 성공</returns>
		public abstract bool Authenticate(byte[] userCiphertext);

		#endregion
	}
}
#region Namespaces

using Panoptes.Rfb.Cryptography;
using System;
using System.Linq;

#endregion

namespace Argos.Panoptes.Rfb.Authentication
{
	/// <summary>
	/// VNC 기본 인증 처리자
	/// </summary>
	class VncAuthenticator : Authenticator
	{
		#region Overriding Methods

		public override byte[] Initialize(string key)
		{
			byte[] plaintext = new byte[16];

			// 16 바이트 크기의 평문을 무작위로 생성한다.
			new Random(DateTime.Now.Millisecond).NextBytes(plaintext);

			// DES 기법을 이용해 인증 키로 평문을 암호화한다.
			base.ciphertext = DesEcbZeroPaddingCipher.Encrypt(plaintext, key);

			return plaintext;
		}

		public override bool Authenticate(byte[] userCiphertext)
		{
			// 사용자가 입력한 암호문을 비교한다.
			return base.ciphertext.SequenceEqual(userCiphertext);
		}

		#endregion
	}
}
#region Namespaces

using System.Security.Cryptography;
using System.Text;

#endregion

namespace Panoptes.Rfb.Cryptography
{
	/// <summary>
	/// VNC 인증을 위한 DES 암복호 처리자
	/// </summary>
	class DesEcbZeroPaddingCipher
	{
		#region Methods

		/// <summary>
		/// Encrypt data(binary) using key for VNC Authorization
		/// VNC 인증을 위해 주어진 키를 이용해 평문을 암호화한다.
		/// </summary>
		/// <param name="plaintext">암호화하려는 평문(challenge)</param>
		/// <param name="key">인증 키(VNC 암호)</param>
		/// <returns>인증 키에 의해 암호화된 암호문</returns>
		public static byte[] Encrypt(byte[] plaintext, string key)
		{
			DES des = new DESCryptoServiceProvider();

			// VNC 인증은 ECB와 No padding 조건으로 DES를 이용한다.
			des.Mode = CipherMode.ECB;
			des.Padding = PaddingMode.None;

			byte[] keyBinary = new byte[8];

			Encoding.ASCII.GetBytes(key, 0, key.Length >= 8 ? 8 : key.Length, keyBinary, 0);

			ICryptoTransform enc = des.CreateEncryptor(keyBinary, null);

			byte[] encrypted = new byte[16];

			enc.TransformBlock(plaintext, 0, plaintext.Length, encrypted, 0);

			return encrypted;
		}

		#endregion
	}
}
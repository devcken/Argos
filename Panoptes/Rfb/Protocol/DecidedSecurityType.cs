#region Namespaces

using Argos.Panoptes.Rfb.Authentication;
using System;

#endregion

namespace Argos.Panoptes.Rfb.Protocol
{
	/// <summary>
	/// 인증에 사용하기로 결정된 암호화 유형과 인증 처리자
	/// </summary>
	class DecidedSecurityType : ProtocolMessage
	{
		#region Constants

		/// <summary>
		/// 프로토콜 메시지의 고정 길이
		/// </summary>
		private static readonly byte DECIDED_SECURITY_TYPE_MESSAGE_LENGTH = 1;

		#endregion

		#region Properties

		/// <summary>
		/// 인증에 사용하기로 결정된 암호화 유형
		/// </summary>
		public SecurityTypes SecurityType
		{
			get;
			private set;
		}

		/// <summary>
		/// 암호화 유형에 따른 인증 처리자
		/// </summary>
		public Authenticator Authenticator
		{
			get;
			private set;
		}

		#endregion

		#region Constructors

		public DecidedSecurityType(byte[] binary)
			: base(DECIDED_SECURITY_TYPE_MESSAGE_LENGTH, binary)
		{
			this.Read();
		}

		#endregion

		#region Overriding Methods

		public override void Read()
		{
			this.SecurityType = (SecurityTypes)Enum.ToObject(typeof(SecurityTypes), base.Reader.ReadByte());

			// 인증 처리자를 암호화 유형에 따라 생성한다.
			switch (this.SecurityType)
			{
				case SecurityTypes.None:
					this.Authenticator = new NoneAuthenticator();

					break;

				case SecurityTypes.VNCAuthentication:
					this.Authenticator = new VncAuthenticator();

					break;

				default:
				case SecurityTypes.Invalid:
					this.Authenticator = null;

					break;
			}
		}

		/// <summary>
		/// 구현되지 않음
		/// </summary>
		public override void Write()
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
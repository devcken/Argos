#region Namespaces

using System;
using System.Linq;

#endregion

namespace Argos.Panoptes.Rfb.Protocol
{
	/// <summary>
	/// 지원되는 암호화 타입 목록 프로토콜 메시지
	/// </summary>
	class SecurityTypeList : ProtocolMessage
	{
		#region Properties

		/// <summary>
		/// 지원되는 암호화 타입의 개수
		/// </summary>
		public byte NumberOfSecurityTypes
		{
			get;
			private set;
		}

		/// <summary>
		/// 지원되는 암호화 타입 목록
		/// </summary>
		public byte[] SecurityTypes
		{
			get;
			private set;
		}

		#endregion

		#region Constructors

		public SecurityTypeList()
			: base(-1) // We don't know legnth of message in this time.
		{
			Array securityTypeValues = Enum.GetValues(typeof(SecurityTypes));

			this.NumberOfSecurityTypes = System.Convert.ToByte(securityTypeValues.Length);
			this.SecurityTypes = securityTypeValues.OfType<byte>().ToArray<byte>();
		}

		#endregion

		#region Overriding Methods

		/// <summary>
		/// 구현되지 않음
		/// </summary>
		public override void Read()
		{
			throw new NotImplementedException();
		}

		public override void Write()
		{
			base.Writer.Write(this.NumberOfSecurityTypes);
			base.Writer.Write(this.SecurityTypes, 0, this.SecurityTypes.Length);
		}

		#endregion
	}
}
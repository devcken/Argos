#region Namespaces

using System;
using System.Text;

#endregion

namespace Argos.Panoptes.Rfb.Protocol
{
	/// <summary>
	/// 인증 결과 프로토콜 메시지
	/// </summary>
	class SecurityResult : ProtocolMessage
	{
		#region Constants

		/// <summary>
		/// 인증 결과 성공 플래그 값
		/// </summary>
		private static readonly uint RESULT_TRUE_FLAG = 0;

		#endregion

		#region Properties

		/// <summary>
		/// 인증 결과
		/// </summary>
		public bool Result
		{
			get;
			private set;
		}

		/// <summary>
		/// 인증 실패 원인 문자열
		/// </summary>
		public string ReasonString
		{
			get;
			private set;
		}

		#endregion

		#region Constructors

		public SecurityResult(bool result, string reasonString)
			: base(-1) // We don't know length of message in this time.
		{
			this.Result = result;
			this.ReasonString = reasonString;

			this.Write();
		}

		#endregion

		#region Overriding Methods

		public override void Read()
		{
			throw new NotImplementedException();
		}

		public override void Write()
		{
			base.Writer.Write(this.Result ? RESULT_TRUE_FLAG : RESULT_TRUE_FLAG + 1);

			byte[] reasonStringBinary = Encoding.ASCII.GetBytes(this.ReasonString);

			base.Writer.Write(Convert.ToByte(reasonStringBinary.Length));
			base.Writer.Write(reasonStringBinary);
		}

		#endregion
	}
}
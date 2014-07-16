namespace Argos.Panoptes.Rfb.Protocol
{
	/// <summary>
	/// ClientInit 프로토콜 메시지
	/// </summary>
	class ClientInit : ProtocolMessage
	{
		#region Constants

		/// <summary>
		/// ClientInit 메세지의 고정 길이
		/// </summary>
		private static readonly int CLIENT_INIT_MESSAGE_LENGTH = 1;

		/// <summary>
		/// 'Shared' 플래그의 참값
		/// </summary>
		private static readonly byte SHARED_TRUE_FLAG = 0x00;

		#endregion

		#region Properties

		/// <summary>
		/// 클라이언트가 서버를 공유할 지 여부
		/// </summary>
		public bool Shared
		{
			get;
			private set;
		}

		#endregion

		#region Constructors

		public ClientInit(byte[] binary)
			: base(CLIENT_INIT_MESSAGE_LENGTH, binary)
		{
			this.Read();
		}

		#endregion

		#region Overriding Methods

		public override void Read()
		{
			// 'shared' 플래그를 읽는다.
			this.Shared = base.Reader.ReadByte().Equals(SHARED_TRUE_FLAG);
		}

		/// <summary>
		/// 구현되지 않음
		/// </summary>
		public override void Write()
		{
			throw new System.NotImplementedException();
		}

		#endregion
	}
}
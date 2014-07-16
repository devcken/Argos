#region Namespaces

using System;
using System.IO;
using System.Text;

#endregion

namespace Argos.Panoptes.Rfb.Protocol
{
	/// <summary>
	/// 서버와 클라이언트 사이에 오가는 프로토콜 메시지의 프로토타입
	/// </summary>
	abstract class ProtocolMessage : IDisposable
	{
		#region Variables

		/// <summary>
		/// 프로토콜 메시지의 길이(고정 길이일 때에만 유효)
		/// </summary>
		protected readonly int messageLength;

		/// <summary>
		/// 프로토콜 메시지 데이터에 대한 읽기 혹은 쓰기 가능한 스트림
		/// </summary>
		protected MemoryStream stream;

		#endregion

		#region Properties

		/// <summary>
		/// 스트림으로부터 프로토콜 메시지를 읽기 위한 처리자
		/// </summary>
		protected BinaryReader Reader
		{
			get;
			private set;
		}

		/// <summary>
		/// 스트림에 프로토콜 메시지를 쓰기 위한 처리자
		/// </summary>
		protected BinaryWriter Writer
		{
			get;
			private set;
		}

		/// <summary>
		/// 해당 프로토콜 메시지가 클라이언트로부터 전달된 것이면 참
		/// </summary>
		public bool Readable
		{
			get
			{
				return this.Reader != null;
			}
		}

		/// <summary>
		/// 해당 프로토콜 메시지가 클라이언트로 전달되는 것이면 참
		/// </summary>
		public bool Writable
		{
			get
			{
				return this.Writer != null;
			}
		}

		#endregion

		#region Constructors

		public ProtocolMessage(int messageLength, byte[] binary)
		{
			if (messageLength != binary.Length) throw new Exception("Protocol message length is not correct.");

			this.messageLength = messageLength;

			this.stream = new MemoryStream(binary);

			this.Reader = new BinaryReader(stream, Encoding.ASCII);
		}

		public ProtocolMessage(int messageLength)
		{
			this.messageLength = messageLength;

			this.stream = new MemoryStream();

			this.Writer = new BinaryWriter(this.stream);
		}

		#endregion

		#region Abstract Methods

		/// <summary>
		/// 프로토콜 메시지를 읽어들인다.
		/// </summary>
		public abstract void Read();

		/// <summary>
		/// 프로토콜 메시지를 쓴다.
		/// </summary>
		public abstract void Write();

		#endregion

		#region Methods

		/// <summary>
		/// 프로토콜 메시지를 바이너리 데이터로 변환한다.
		/// </summary>
		/// <returns>프로토콜 메시지에 대한 바이너리 데이터</returns>
		public ArraySegment<byte> ToBinary()
		{
			this.stream.Position = 0;

			return new ArraySegment<byte>(this.stream.ToArray());
		}

		#endregion

		#region IDisposable Implementations

		/// <summary>
		/// 'Reader'와 'Writer' 자원을 해제한다.
		/// </summary>
		public void Dispose()
		{
			if (this.Reader != null) this.Reader.Close();
			if (this.Writer != null) this.Writer.Close();
		}

		#endregion
	}
}
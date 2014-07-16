#region Namespaces

using System;
using System.Text;

#endregion

namespace Argos.Panoptes.Rfb.Protocol
{
	/// <summary>
	/// 'ProtocolVersion' 프로토콜 메시지
	/// </summary>
	class ProtocolVersion : ProtocolMessage
	{
		#region Constants

		/// <summary>
		/// 프로토콜 버전 메시지의 고정 길이
		/// </summary>
		private static readonly int PROTOCOL_VERSION_BINARY_LENGTH = 12;

		/// <summary>
		/// Space의 ASCII 코드
		/// </summary>
		private static readonly byte SPACE = 0x20;

		/// <summary>
		/// Period의 ASCII 코드
		/// </summary>
		private static readonly byte PERIOD = 0x2e;

		/// <summary>
		/// NewLine의 ASCII 코드
		/// </summary>
		private static readonly byte NEW_LINE = 0x0a;

		/// <summary>
		/// ProtocolVersion 메시지의 접두사
		/// </summary>
		private static readonly byte[] PROTOCOL_VERSION_PREFIX = { 0x52, 0x46, 0x42 };

		#endregion

		#region Properties

		/// <summary>
		/// 상위 버전
		/// </summary>
		public short Major
		{
			get;
			private set;
		}

		/// <summary>
		/// 하위 버전
		/// </summary>
		public short Minor
		{
			get;
			private set;
		}

		/// <summary>
		/// 상하위를 함께 표기한 버전
		/// </summary>
		public float Version
		{
			get
			{
				return this.Major * 1.0f + this.Minor * 0.1f;
			}
		}

		#endregion

		#region Constructors

		public ProtocolVersion(byte[] binary)
			: base(PROTOCOL_VERSION_BINARY_LENGTH, binary)
		{
			this.Read();
		}

		public ProtocolVersion(short major, short minor)
			: base(PROTOCOL_VERSION_BINARY_LENGTH)
		{
			this.Major = major;
			this.Minor = minor;

			this.Write();
		}

		#endregion

		#region Overriding Methods

		public override void Read()
		{
			byte[] prefixBinary = base.Reader.ReadBytes(3);

			if (prefixBinary.BinarySearch(PROTOCOL_VERSION_PREFIX) != 0) throw new Exception("Protocol version prefix is not found.");

			// 1바이트 크기의 padding을 읽는다.
			base.Reader.ReadByte();

			byte[] majorBinary = base.Reader.ReadBytes(3);

			this.Major = Convert.ToInt16(majorBinary[0] * 100 + majorBinary[1] * 10 + majorBinary[2]);

			// Period를 읽는다.
			base.Reader.ReadByte();

			byte[] minorBinary = base.Reader.ReadBytes(3);

			this.Minor = Convert.ToInt16(minorBinary[0] * 100 + minorBinary[1] * 10 + minorBinary[2]);

			// new line을 읽는다.
			base.Reader.ReadByte();
		}

		public override void Write()
		{
			base.Writer.Write(PROTOCOL_VERSION_PREFIX);
			base.Writer.Write(SPACE);

			byte[] majorBinary = new byte[3] { Convert.ToByte(this.Major / 100), Convert.ToByte(this.Major % 100 / 10), Convert.ToByte(this.Major % 1000) };
			base.Writer.Write(majorBinary);

			base.Writer.Write(PERIOD);

			byte[] minorBinary = new byte[3] { Convert.ToByte(this.Minor / 100), Convert.ToByte(this.Minor % 100 / 10), Convert.ToByte(this.Minor % 1000) };
			base.Writer.Write(minorBinary);

			base.Writer.Write(NEW_LINE);
		}

		public override string ToString()
		{
			string.Format("RFB {0}{1}{2}.{3}{4}{5}\\n",
				this.Major / 100, this.Major % 100 / 10, this.Major % 1000,
				this.Minor / 100, this.Minor % 100 / 10, this.Minor % 1000);

			return Encoding.ASCII.GetString(this.ToBinary().Array);
		}

		#endregion

		#region Operators Overriding

		public override bool Equals(object obj)
		{
			return this == (ProtocolVersion)obj;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public static bool operator ==(ProtocolVersion a, ProtocolVersion b)
		{
			return a.Version == b.Version;
		}

		public static bool operator !=(ProtocolVersion a, ProtocolVersion b)
		{
			return a.Version != b.Version;
		}

		public static bool operator >(ProtocolVersion a, ProtocolVersion b)
		{
			return a.Version > b.Version;
		}

		public static bool operator >=(ProtocolVersion a, ProtocolVersion b)
		{
			return a.Version >= b.Version;
		}

		public static bool operator <(ProtocolVersion a, ProtocolVersion b)
		{
			return a.Version < b.Version;
		}

		public static bool operator <=(ProtocolVersion a, ProtocolVersion b)
		{
			return a.Version <= b.Version;
		}

		#endregion
	}
}
#region Namespaces

using Argos.Panoptes.Graphics;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

#endregion

namespace Argos.Panoptes.Rfb.Protocol
{
	/// <summary>
	/// 'ServerInit' 프로토콜 메시지
	/// </summary>
	class ServerInit : ProtocolMessage
	{
		#region Properties

		/// <summary>
		/// Framebuffer의 전체 너비
		/// </summary>
		public ushort FramebufferWidth
		{
			get;
			private set;
		}

		/// <summary>
		/// Framebuffer의 전체 높이
		/// </summary>
		public ushort FramebufferHeight
		{
			get;
			private set;
		}

		/// <summary>
		/// 기본 픽셀 포맷
		/// </summary>
		public PixelFormat PixelFormat
		{
			get;
			private set;
		}

		/// <summary>
		/// 컴퓨터 이름의 길이
		/// </summary>
		public uint NameLength
		{
			get;
			private set;
		}

		/// <summary>
		/// 컴퓨터 이름
		/// </summary>
		public string NameString
		{
			get;
			private set;
		}

		#endregion

		#region Constructors

		private ServerInit()
			: base(-1)
		{

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
			base.Writer.Write(this.FramebufferWidth);
			base.Writer.Write(this.FramebufferHeight);

			int pixelFormatSize = Marshal.SizeOf(typeof(PixelFormat));
			
			IntPtr pointer = Marshal.AllocHGlobal(pixelFormatSize);
			byte[] pixelFormatBinary = new byte[pixelFormatSize];

			Marshal.StructureToPtr(this.PixelFormat, pointer, true);
			Marshal.Copy(pointer, pixelFormatBinary, 0, pixelFormatSize);
			Marshal.FreeHGlobal(pointer);

			base.Writer.Write(pixelFormatBinary);
			base.Writer.Write(this.NameLength);
			base.Writer.Write(Encoding.ASCII.GetBytes(this.NameString));
		}

		#endregion

		#region Methods

		/// <summary>
		/// Framebuffer의 너비와 높이를 업데이트한다.
		/// </summary>
		/// <param name="width">너비</param>
		/// <param name="height">높이</param>
		public void ChangeFramebufferResolution(int width, int height)
		{
			this.FramebufferWidth = Convert.ToUInt16(width);
			this.FramebufferHeight = Convert.ToUInt16(height);
		}

		/// <summary>
		/// 기본 ServerInit 메시지를 만든다.
		/// </summary>
		/// <returns></returns>
		public static ServerInit MakeDefault()
		{
			int bitsPerPixel = 32;

			// 모든 디스플레이 중 BPP 수치가 가장 낮은 것으로 설정된다.
			Screen.AllScreens.All((screen) =>
			{
				if (screen.BitsPerPixel < bitsPerPixel)
				{
					bitsPerPixel = screen.BitsPerPixel;
				}

				return true;
			});

			int depth = Convert.ToInt32(Convert.ChangeType(ColorDepth.Depth24Bit, typeof(ColorDepth)));

			// Color depth는 BPP보다 한단계 작은 것으로 결정된다.
			Enum.GetValues(typeof(ColorDepth)).Cast<int>().ToLookup(colorDepth =>
			{
				if (bitsPerPixel < colorDepth)
				{
					return false;
				}

				if (colorDepth < depth)
				{
					return false;
				}
				else
				{
					depth = colorDepth;

					return true;
				}
			});

			int redMax = 0;
			int greenMax = 0;
			int blueMax = 0;
			int redShift = 0;
			int greenShift = 0;
			int blueShift = 0;

			// RGB의 최대값과 비트 이동 값을 구해온다.
			RgbColorCalibrator.GetRgbMaxValues(depth,
				out redMax, out greenMax, out blueMax,
				out redShift, out greenShift, out blueShift);

			var serverInit = new ServerInit()
			{
				FramebufferWidth = Convert.ToUInt16(SystemInformation.VirtualScreen.Width),
				FramebufferHeight = Convert.ToUInt16(SystemInformation.VirtualScreen.Height),
				PixelFormat = new Argos.Panoptes.Rfb.PixelFormat(Convert.ToByte(bitsPerPixel), Convert.ToByte(depth), Convert.ToByte(BitConverter.IsLittleEndian ? 0 : 1),
					Convert.ToUInt16(redMax), Convert.ToUInt16(greenMax), Convert.ToUInt16(blueMax),
					Convert.ToByte(redShift), Convert.ToByte(greenShift), Convert.ToByte(blueShift)),
				NameString = System.Environment.MachineName,
				NameLength = Convert.ToUInt32(Encoding.ASCII.GetBytes(System.Environment.MachineName).Length),
			};

			serverInit.Write();

			return serverInit;
		}

		#endregion
	}
}
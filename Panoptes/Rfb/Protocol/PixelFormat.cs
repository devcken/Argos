#region Namespaces

using System;
using System.Runtime.InteropServices;

#endregion

namespace Argos.Panoptes.Rfb
{
	/// <summary>
	/// Framebuffer 비트맵 데이터의 Pixel 유형
	/// </summary>
	struct PixelFormat
	{
		[MarshalAs(UnmanagedType.U1)]
		internal byte BitsPerPixel;
		[MarshalAs(UnmanagedType.U1)]
		internal byte Depth;
		[MarshalAs(UnmanagedType.U1)]
		internal byte BigEndianFlag;
		[MarshalAs(UnmanagedType.U1)]
		internal byte TrueColourFlag;
		[MarshalAs(UnmanagedType.U2)]
		internal ushort RedMax;
		[MarshalAs(UnmanagedType.U2)]
		internal ushort GreenMax;
		[MarshalAs(UnmanagedType.U2)]
		internal ushort BlueMax;
		[MarshalAs(UnmanagedType.U1)]
		internal byte RedShift;
		[MarshalAs(UnmanagedType.U1)]
		internal byte GreenShift;
		[MarshalAs(UnmanagedType.U1)]
		internal byte BlueShift;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst=3)]
		internal byte[] Padding;

		public PixelFormat(byte bitsPerPixel, byte depth, byte bigEndianFlag, ushort redMax, ushort greenMax, ushort blueMax, byte redShift, byte greenShift, byte blueShift)
		{
			this.BitsPerPixel = bitsPerPixel;
			this.Depth = depth;
			this.BigEndianFlag = bigEndianFlag;
			this.TrueColourFlag = Convert.ToByte(this.Depth >= 24 ? 1 : 0);
			this.RedMax = redMax;
			this.GreenMax = greenMax;
			this.BlueMax = blueMax;
			this.RedShift = redShift;
			this.GreenShift = greenShift;
			this.BlueShift = blueShift;

			Padding = new byte[3];
		}
	}
}
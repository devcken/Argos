#region Namespaces

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

#endregion

namespace Argos.Panoptes.Graphics
{
	/// <summary>
	/// 스크린 캡쳐 메서드를 제공한다.
	/// </summary>
	class CaptureScreen
	{
		#region Methods

		/// <summary>
		/// 윈도우의 특정 영역을 비트맵으로 가져온다.
		/// </summary>
		/// <param name="hWnd">윈도우 핸들</param>
		/// <param name="x">x 좌표</param>
		/// <param name="y">y 좌표</param>
		/// <param name="width">너비</param>
		/// <param name="height">높이</param>
		/// <returns>윈도우의 특정 영역에 대한 비트맵</returns>
		public static Bitmap CaptureWindow(IntPtr hWnd, int x, int y, int width, int height)
		{
			IntPtr hDc = GetDC(hWnd);

			IntPtr hCDc = CreateCompatibleDC(hDc);
			IntPtr hCBitmap = CreateCompatibleBitmap(hDc, width, height);

			Bitmap bitmap = null;

			if (!hCBitmap.Equals(IntPtr.Zero))
			{
				IntPtr hPrev = (IntPtr)SelectObject(hCDc, hCBitmap);

				BitBlt(hCDc, 0, 0, width, height, hDc, x, y, SRCCOPY);

				SelectObject(hCDc, hPrev);

				bitmap = Image.FromHbitmap(hCBitmap);
			}

			ReleaseDC(hWnd, hDc);
  			DeleteDC(hCDc);
			DeleteObject(hCBitmap);

			GC.Collect();
			
			return bitmap;
		}

		/// <summary>
		/// 데스크톱 비트맵을 가져온다.
		/// </summary>
		/// <param name="withCursor">커서를 포함할 지 여부</param>
		/// <returns>데스크톱 비트맵</returns>
		public static Bitmap CaptureDesktop(bool withCursor = false)
		{
			Size desktopSize = SystemInformation.VirtualScreen.Size;

			Bitmap windowBitmap = CaptureWindow(GetDesktopWindow(), 0, 0, desktopSize.Width, desktopSize.Height);

			if (withCursor)
			{
				CURSORINFO pci;

				pci.cbSize = Marshal.SizeOf(typeof(CURSORINFO));

				if (GetCursorInfo(out pci))
				{
					if (pci.flags == CURSOR_SHOWING)
					{
						var g = System.Drawing.Graphics.FromImage(windowBitmap);

						DrawIcon(g.GetHdc(), pci.ptScreenPos.x, pci.ptScreenPos.y, pci.hCursor);

						g.ReleaseHdc();
					}
				}
			}

			return windowBitmap;
		}

		#endregion

		#region User32 API

		public const Int32 CURSOR_SHOWING = 0x00000001;

		[StructLayout(LayoutKind.Sequential)]
		public struct ICONINFO
		{
			public bool fIcon;         // Specifies whether this structure defines an icon or a cursor. A value of TRUE specifies 
			public Int32 xHotspot;     // Specifies the x-coordinate of a cursor's hot spot. If this structure defines an icon, the hot 
			public Int32 yHotspot;     // Specifies the y-coordinate of the cursor's hot spot. If this structure defines an icon, the hot 
			public IntPtr hbmMask;     // (HBITMAP) Specifies the icon bitmask bitmap. If this structure defines a black and white icon, 
			public IntPtr hbmColor;    // (HBITMAP) Handle to the icon color bitmap. This member can be optional if this 
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct POINT
		{
			public Int32 x;
			public Int32 y;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct CURSORINFO
		{
			public Int32 cbSize;        // Specifies the size, in bytes, of the structure. 
			public Int32 flags;         // Specifies the cursor state. This parameter can be one of the following values:
			public IntPtr hCursor;          // Handle to the cursor. 
			public POINT ptScreenPos;       // A POINT structure that receives the screen coordinates of the cursor. 
		}

		[DllImport("user32.dll", EntryPoint = "GetDesktopWindow")]
		static extern IntPtr GetDesktopWindow();

		[DllImport("user32.dll", EntryPoint = "GetDC")]
		static extern IntPtr GetDC(IntPtr ptr);

		[DllImport("user32.dll", EntryPoint = "ReleaseDC")]
		static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDc);

		[DllImport("user32.dll", EntryPoint = "GetCursorInfo")]
		static extern bool GetCursorInfo(out CURSORINFO pci);

		[DllImport("user32.dll")]
		static extern bool DrawIcon(IntPtr hDC, int X, int Y, IntPtr hIcon);

		#endregion

		#region GDI32 API

		public const int SRCCOPY = 13369376;

		[DllImport("gdi32.dll", EntryPoint = "DeleteDC")]
		public static extern IntPtr DeleteDC(IntPtr hDc);

		[DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
		public static extern IntPtr DeleteObject(IntPtr hDc);

		[DllImport("gdi32.dll", EntryPoint = "BitBlt")]
		public static extern bool BitBlt(IntPtr hdcDest, int xDest,
										 int yDest, int wDest,
										 int hDest, IntPtr hdcSource,
										 int xSrc, int ySrc, int RasterOp);

		[DllImport("gdi32.dll", EntryPoint = "CreateCompatibleBitmap")]
		public static extern IntPtr CreateCompatibleBitmap
									(IntPtr hdc, int nWidth, int nHeight);

		[DllImport("gdi32.dll", EntryPoint = "CreateCompatibleDC")]
		public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

		[DllImport("gdi32.dll", EntryPoint = "SelectObject")]
		public static extern IntPtr SelectObject(IntPtr hdc, IntPtr bmp);

		#endregion
	}
}

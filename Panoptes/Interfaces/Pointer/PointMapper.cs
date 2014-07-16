#region Namespaces

using NLog;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

#endregion

namespace Argos.Panoptes.Interfaces.Pointer
{
	/// <summary>
	/// 포인터 포인트 매퍼
	/// </summary>
	class PointMapper
	{
		#region Variables

		private static Logger logger = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// 마우스가 눌린 상태인지 아닌지 여부
		/// </summary>
		private static bool mousePressed = false;

		#endregion

		#region Methods

		/// <summary>
		/// 포인터 이벤트를 매핑한다.
		/// </summary>
		/// <param name="reader">포인터 이벤트 데이터 리더</param>
		public static void Map(BinaryReader reader)
		{
			// 버튼 마스크를 읽어온다.
			byte buttonMask = reader.ReadByte();
			
			// x, y 좌표를 읽는다.
			ushort x = reader.ReadUInt16();
			ushort y = reader.ReadUInt16();

			short dwData = 0;
			
			MouseEventFlags flag = MouseEventFlags.LEFTDOWN;

			// 버튼 마스크로부터 버튼을 유형을 알아낸다.
			PointerButtonMask mask = FindMaskPosition(buttonMask);

			// 포인터가 이동된 경우
			if (mask == PointerButtonMask.Move)
			{
				SetCursorPos(x, y);
			}
			else
			{
				switch (mask)
				{
					// 왼쪽 버튼
					case PointerButtonMask.Left:
						mousePressed = !mousePressed;

						if (mousePressed) flag = MouseEventFlags.LEFTDOWN;
						else flag = MouseEventFlags.LEFTUP;

						break;

					// 가운데 버튼
					case PointerButtonMask.Middle:
						mousePressed = !mousePressed;

						if (mousePressed) flag = MouseEventFlags.MIDDLEDOWN;
						else flag = MouseEventFlags.MIDDLEUP;

						break;

					// 오른쪽 버튼
					case PointerButtonMask.Right:
						mousePressed = !mousePressed;

						if (mousePressed) flag = MouseEventFlags.RIGHTDOWN;
						else flag = MouseEventFlags.RIGHTUP;

						break;

					// 휠 업/다운
					case PointerButtonMask.WheelUp:
					case PointerButtonMask.WheelDown:

						flag = MouseEventFlags.WHEEL;

						dwData = (short)x;

						x = 0;

						break;
				}

				//if (logger.IsDebugEnabled)
				//{
				//	logger.Debug("flag: {0}, x: {1}, y: {2}, dwData: {3}", flag, x, y, dwData);
				//}

				mouse_event((uint)flag, x, y, dwData, 0);
			}
		}

		/// <summary>
		/// 버튼 마스크로부터 마스크 위치를 찾아낸다.
		/// </summary>
		/// <param name="buttonMask">버튼 마스크</param>
		/// <returns>버튼 마스크 위치에 따른 버튼 유형</returns>
		private static PointerButtonMask FindMaskPosition(byte buttonMask)
		{
			// 버튼 마스크를 integer 형으로 변환한다.
			int mask = Convert.ToInt32(buttonMask);
			int count = 0;

			// 마스크된 부분까지 오른쪽으로 비트 쉬프트하여 비트 쉬프트된 회수를 샌다.
			for (; mask != 0; count++)
			{
				mask = mask >> 1;
			}

			return (PointerButtonMask)count;
		}

		#endregion

		#region Windows API

		/// <summary>
		/// mouse_event Windows API
		/// </summary>
		[DllImport("user32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
		static extern void mouse_event(uint dwFlags, uint dx, uint dy, int dwData, int dwExtraInfo);

		/// <summary>
		/// Mouse event flags for mouse_event API
		/// </summary>
		[Flags]
		public enum MouseEventFlags
		{
			LEFTDOWN = 0x00000002,
			LEFTUP = 0x00000004,
			MIDDLEDOWN = 0x00000020,
			MIDDLEUP = 0x00000040,
			MOVE = 0x00000001,
			ABSOLUTE = 0x00008000,
			RIGHTDOWN = 0x00000008,
			RIGHTUP = 0x00000010,
			WHEEL = 0x00000800,
			XDOWN = 0x00000080,
			XUP = 0x00000100
		}

		/// <summary>
		/// SetCursorPos Windows API
		/// </summary>
		[DllImport("user32.dll")]
		static extern bool SetCursorPos(int X, int Y);

		#endregion
	}

	/// <summary>
	/// 버튼 마스크의 열거자
	/// </summary>
	enum PointerButtonMask : int
	{
		Move = 0,
		Left = 1,
		Middle = 2,
		Right = 3,
		WheelUp = 4,
		WheelDown = 5
	}
}
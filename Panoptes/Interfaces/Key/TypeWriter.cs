#region Namespaces

using NLog;
using System;
using System.IO;
using System.Runtime.InteropServices;

#endregion

namespace Argos.Panoptes.Interfaces.Key
{
	/// <summary>
	/// 키보드 이벤트 핸들러
	/// </summary>
	class TypeWriter
	{
		#region Variables

		private static Logger logger = LogManager.GetCurrentClassLogger();

		#endregion

		#region Methods

		/// <summary>
		/// 클라이언트의 키보드 이벤트에 대응하는 키보드 이벤트를 발생시킨다.
		/// </summary>
		/// <param name="reader">클라이언트 키보드 이벤트 데이터 리더</param>
		public static void Write(BinaryReader reader)
		{
			// 키가 눌렸는지 떼어졌는지 여부를 읽어온다.
			uint dwFlags = Convert.ToBoolean(reader.ReadByte()) ? 0 : KEYEVENTF_KEYUP;

			// 2 바이트 크기의 padding을 읽는다.
			reader.ReadUInt16();

			// RFB 프로토콜은 키보드 이벤트에서 범용성을 위해 keysym을 이용하기 때문에 4바이트 크기의 unsigned integer를 사용하도록 되어 있다.
			// 그러나, .NET Fx의 경우 keysym을 이용하지 않고도 충분히 모든 키를 사용할 수 있으므로 keysym을 구현하지 않고 keybd_event API를 사용한다.
			uint key = reader.ReadUInt32();

			if (logger.IsDebugEnabled)
			{
				logger.Debug("flag: {0}, key: {1}", dwFlags, key);
			}
			
			keybd_event((byte)key, (byte)MapVirtualKey(key, 0), dwFlags, UIntPtr.Zero);
		}

		#endregion

		#region Windows API

		/// <summary>
		/// keybd_event Windows API
		/// </summary>
		[DllImport("user32.dll", SetLastError = true)]
		private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

		/// <summary>
		/// MapVirtualKey Windows API
		/// </summary>
		[DllImport("user32.dll")]
		static extern uint MapVirtualKey(uint uCode, uint uMapType);

		// KEYEVENTF_EXTENDEDKEY 상수는 Shift 키와 충돌을 일으킨다. 대신 0x00값을 사용한다.
		//private const uint KEYEVENTF_EXTENDEDKEY = 0x01;
		private const uint KEYEVENTF_KEYUP = 0x02;

		#endregion
	}
}
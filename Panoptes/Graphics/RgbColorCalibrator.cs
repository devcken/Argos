#region Namespaces

using System;

#endregion

namespace Argos.Panoptes.Graphics
{
	/// <summary>
	/// RGB 컬러 
	/// </summary>
	class RgbColorCalibrator
	{
		#region Constants

		/// <summary>
		/// 8-depth RGB 비트 카운트
		/// </summary>
		private static readonly int[] DEPTH8_RGB_BIT_COUNT = { 3, 3, 2 };

		/// <summary>
		/// 16-depth RGB 비트 카운트
		/// </summary>
		private static readonly int[] DEPTH16_RGB_BIT_COUNT = { 5, 6, 5 };

		/// <summary>
		/// 24-depth RGB 비트 카운트
		/// </summary>
		private static readonly int[] DEPTH24_RGB_BIT_COUNT = { 8, 8, 8 };

		#endregion

		#region Methods

		/// <summary>
		/// Color depth에 대한 RGB 최대 값과 비트 쉬프트를 구해온다.
		/// </summary>
		/// <param name="colorDepth">RGB color depth</param>
		/// <param name="redMax">Max value of red</param>
		/// <param name="greenMax">Max value of green</param>
		/// <param name="blueMax">Max value of blue</param>
		/// <param name="redShift">Bit shift of red</param>
		/// <param name="greenShift">Bit shift of green</param>
		/// <param name="blueShift">Bit shift of blue</param>
		public static void GetRgbMaxValues(int colorDepth,
			out int redMax, out int greenMax, out int blueMax,
			out int redShift, out int greenShift, out int blueShift)
		{
			int[] channelBitCounts;

			switch (colorDepth)
			{
				case 8:
					channelBitCounts = DEPTH8_RGB_BIT_COUNT;

					break;

				case 16:
					channelBitCounts = DEPTH16_RGB_BIT_COUNT;


					break;

				case 24:
				case 32:
					channelBitCounts = DEPTH24_RGB_BIT_COUNT;

					break;
					
				default:
					throw new Exception("This color depth is not supported.");
			}

			redMax = (int)Math.Pow(2, channelBitCounts[0]);
			greenMax = (int)Math.Pow(2, channelBitCounts[1]);
			blueMax = (int)Math.Pow(2, channelBitCounts[2]);

			redShift = 0;
			greenShift = redShift + channelBitCounts[0];
			blueShift = greenShift + channelBitCounts[1];
		}

		#endregion
	}
}
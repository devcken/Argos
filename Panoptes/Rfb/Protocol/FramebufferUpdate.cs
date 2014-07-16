#region Namespaces

using System;
using System.Drawing;
using System.IO;
using System.Text;

#endregion

namespace Argos.Panoptes.Rfb.Protocol
{
	/// <summary>
	/// Framebuffer의 업데이트를 위한 프로토콜 메시지(FramebufferUpdate)
	/// 이 메시지는 서버에서 클라이언트로 전달된다.
	/// </summary>
	class FramebufferUpdate
	{
		#region Variables

		/// <summary>
		/// 'FramebufferUpdate' 메시지 번호
		/// </summary>
		private static readonly byte MESSAGE_TYPE = 0x00;

		#endregion

		#region Methods

		/// <summary>
		/// 비트맵 데이터를 바이너리 데이터로 변환한다.
		/// </summary>
		/// <param name="bounds">비트맵 데이터에 대한 바운드 정보</param>
		/// <param name="image">비트맵 데이터</param>
		/// <returns>Framebuffer를 업데이트하기 위한 바운드 및 비트맵 데이터</returns>
		public static ArraySegment<byte> ToMessage(Rectangle bounds, Bitmap image)
		{
			byte[] base64ImageBinary;

			using (MemoryStream stream = new MemoryStream())
			{
				// 비트맵 데이터를 PNG 형식으로 스트림에 저장한다.
				image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
				
				byte[] rawBinary = stream.ToArray();

				// 비트맵 바이너리를 Base64 인코딩한다.
				base64ImageBinary = Encoding.ASCII.GetBytes(Convert.ToBase64String(rawBinary));
			}

			ArraySegment<byte> binary;

			using (MemoryStream stream = new MemoryStream())
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					writer.Write(MESSAGE_TYPE);
					writer.Write(Convert.ToByte(0));
					writer.Write(Convert.ToUInt16(1));
					writer.Write(Convert.ToUInt16(bounds.X));
					writer.Write(Convert.ToUInt16(bounds.Y));
					writer.Write(Convert.ToUInt16(bounds.Width));
					writer.Write(Convert.ToUInt16(bounds.Height));
					writer.Write(0);

					writer.Write(base64ImageBinary);
				}

				binary = new ArraySegment<byte>(stream.ToArray());
			}

			return binary;
		}

		#endregion
	}
}
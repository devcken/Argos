#region Namespaces

using System.Drawing;

#endregion

namespace Argos.Panoptes.Graphics
{
	/// <summary>
	/// Bound와 그에 대한 비트맵 정보를 담는다.
	/// </summary>
	class BoundBitmap
	{
		#region Properties

		/// <summary>
		/// 비트맵에 대한 바운드 정보
		/// </summary>
		public Rectangle Bound
		{
			get;
			private set;
		}

		/// <summary>
		/// 스크린 이미지에서 변경된 부분의 비트맵
		/// </summary>
		public Bitmap Bitmap
		{
			get;
			private set;
		}

		#endregion

		#region Constructors

		public BoundBitmap(Rectangle bound, Bitmap bitmap)
		{
			this.Bound = bound;
			this.Bitmap = bitmap;
		}

		#endregion
	}
}
#region Namespaces

using AForge.Imaging.Filters;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

#endregion

namespace Argos.Panoptes.Graphics
{
	/// <summary>
	/// 스크린에 대한 프레임 비트맵을 수집하여 전달한다.
	/// </summary>
	class FrameBitmapGrabber
	{
		#region Variables

		/// <summary>
		/// 이전에 캡쳐된 스크린 비트맵
		/// </summary>
		private Bitmap prevBitmap = null;

		#endregion

		#region Properties

		/// <summary>
		/// 시작 여부
		/// </summary>
		public bool Started
		{
			get;
			private set;
		}

		/// <summary>
		/// 동작 여부
		/// </summary>
		public bool Working
		{
			get;
			private set;
		}

		#endregion

		#region Events

		/// <summary>
		/// Grabbed 이벤트 핸들러
		/// </summary>
		/// <param name="boundBitmaps"></param>
		public delegate void GrabEventHandler(List<BoundBitmap> boundBitmaps);

		/// <summary>
		/// 프레임 비트맵들이 수집된 경우 발생한다.
		/// </summary>
		public event GrabEventHandler Grabbed;

		#endregion

		#region Constructors

		public FrameBitmapGrabber()
		{
		}

		#endregion

		#region Methods

		/// <summary>
		/// 프레임 비트맵 수집을 시작한다.
		/// </summary>
		public void Start()
		{
			if (this.Working) return;

			this.Started = true;

			ThreadPool.QueueUserWorkItem(Work);
		}

		/// <summary>
		/// 프레임 비트맵 수집은 스레드 상에서 이루어진다.
		/// </summary>
		/// <param name="argument"></param>
		private void Work(object argument)
		{
			this.Working = true;

			List<BoundBitmap> boundBitmaps = GetDifferenceBitmaps();

			if (boundBitmaps != null && boundBitmaps.Count > 0)
			{
				this.Grabbed(boundBitmaps);
			}

			if (Started)
			{
				Thread.Sleep(200);

				ThreadPool.QueueUserWorkItem(Work);
			}
		}

		/// <summary>
		/// 스크린의 이전 비트맵과 현재 비트뱁을 비교하여 변경된 부분의 비트맵 목록을 가져온다.
		/// </summary>
		/// <returns>변경된 부분의 비트맵 목록</returns>
		private List<BoundBitmap> GetDifferenceBitmaps()
		{
			Bitmap currentBitmap = CaptureScreen.CaptureDesktop();

			// 이전 비트맵이 존재하지 않을 경우, 전체 스크린에 대한 비트맵을 반환한다.
			if (this.prevBitmap == null)
			{
				return new List<BoundBitmap> { new BoundBitmap(new Rectangle(0, 0, currentBitmap.Width, currentBitmap.Height), currentBitmap) };
			}

			// 변경된 부분의 바운드 목록을 가져온다.
			Rectangle[] differenceBounds = this.GetDiffereceBounds(currentBitmap);

			List<BoundBitmap> differenceBitmaps = new List<BoundBitmap>();

			// 변경된 부분의 비트맵을 구해 목록에 저장한다.
			foreach (Rectangle bound in differenceBounds)
			{
				Bitmap boundBitmap = new Bitmap(bound.Width, bound.Height, currentBitmap.PixelFormat);

				using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(boundBitmap))
				{
					g.DrawImage(currentBitmap, bound);
					g.Flush();
				}

				differenceBitmaps.Add(new BoundBitmap(bound, boundBitmap));
			}

			this.prevBitmap = currentBitmap;

			return differenceBitmaps;
		}

		/// <summary>
		/// 이전과 현재의 비트맵에서 변경된 부분의 바운드 목록을 구한다.
		/// </summary>
		/// <param name="bitmap">현재 비트맵</param>
		/// <returns>변경된 부분의 바운드 목록</returns>
		private Rectangle[] GetDiffereceBounds(Bitmap bitmap)
		{
			// 두 비트맵의 차를 구한다.
			Bitmap subtractedBitmap = new Subtract(this.prevBitmap).Apply(bitmap);

			ConnectedComponentsLabeling labelingFilter = new ConnectedComponentsLabeling();

			// 라벨링 필터를 적용하여 어떤 부분이 두 비트맵의 차인지 알 수 있다.
			Bitmap labeledBitmap = labelingFilter.Apply(subtractedBitmap);

			List<Rectangle> rectangles = new List<Rectangle>();

			// 서로 겹치는 바운드들을 모두 통합한다.
			foreach (Rectangle objRect in labelingFilter.BlobCounter.GetObjectsRectangles())
			{
				Rectangle tempRect1 = Rectangle.Empty;
				Rectangle tempRect2 = Rectangle.Empty;

				foreach (Rectangle rect in rectangles)
				{
					if (objRect.IntersectsWith(rect))
					{
						tempRect1 = Rectangle.Union(objRect, rect);
						tempRect2 = rect;

						break;
					}
				}

				if (tempRect1.IsEmpty)
				{
					tempRect1 = objRect;
				}

				rectangles.Add(tempRect1);

				if (tempRect2.IsEmpty)
				{
					rectangles.Remove(tempRect2);
				}
			}

			return rectangles.ToArray();
		}

		/// <summary>
		/// 수집을 중지한다.
		/// </summary>
		public void Stop()
		{
			this.Started = false;
			this.Working = false;
		}

		#endregion
	}
}
#region Namespaces

using System;
using System.Linq;

#endregion

namespace Argos.Panoptes
{
	/// <summary>
	/// Method Extension for System.Array
	/// </summary>
	public static class ArrayExtension
	{
		/// <summary>
		/// Search some array in original.
		/// </summary>
		/// <typeparam name="T">Composition of array</typeparam>
		/// <param name="origin">Original array</param>
		/// <param name="search">Pattern array</param>
		/// <returns>Index position of pattern array in original</returns>
		public static int BinarySearch<T>(this T[] origin, T[] search)
		{
			if (origin.Length < search.Length) return -1;

			int startPointCount = origin.Length - search.Length + 1;

			int position = -1;

			for (int index = 0; index < startPointCount; index++)
			{
				T[] temp = new T[search.Length];

				Array.Copy(origin, index, temp, 0, temp.Length);

				if (temp.SequenceEqual(search))
				{
					position = index;

					break;
				}
			}

			return position;
		}
	}
}
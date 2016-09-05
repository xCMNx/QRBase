using System;
using System.Text.RegularExpressions;
using Core;
using QRCoder;

namespace QRBase
{
	public static class LHelpers
	{
		[System.Runtime.InteropServices.DllImport("gdi32.dll")]
		[return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
		internal static extern bool DeleteObject(IntPtr hObject);

		/// <summary>
		/// Converts a <see cref="System.Drawing.Bitmap"/> into a WPF <see cref="BitmapSource"/>.
		/// </summary>
		/// <remarks>Uses GDI to do the conversion. Hence the call to the marshalled DeleteObject.
		/// </remarks>
		/// <param name="source">The source bitmap.</param>
		/// <returns>A BitmapSource</returns>
		public static System.Windows.Media.Imaging.BitmapSource ToBitmapSource(this System.Drawing.Bitmap source)
		{
			System.Windows.Media.Imaging.BitmapSource bitSrc = null;

			var hBitmap = source.GetHbitmap();

			try
			{
				bitSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
					hBitmap,
					IntPtr.Zero,
					System.Windows.Int32Rect.Empty,
					System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
			}
			catch (System.ComponentModel.Win32Exception)
			{
				bitSrc = null;
			}
			finally
			{
				DeleteObject(hBitmap);
			}

			return bitSrc;
		}

		static Regex geRegex = new Regex(@"{\$(.*?)}");
		public static string GenInfo(this QRData data, string format)
		{
			try
			{
				return geRegex.Replace(format, m =>
					{
						object o;
						if (data.SimpleTryGetValue(m.Groups[1].Value, out o, true))
							return o?.ToString();
						return string.Empty;
					}
				);
			}
			catch
			{
				return string.Empty;
			}
		}

		static QRCodeGenerator qrGenerator = new QRCodeGenerator();
		public static System.Windows.Media.Imaging.BitmapSource ToQRImageSource(this string source, int pixelsPerModule = 20) => new QRCode(qrGenerator.CreateQrCode(source, QRCodeGenerator.ECCLevel.Q)).GetGraphic(pixelsPerModule).ToBitmapSource();
	}
}

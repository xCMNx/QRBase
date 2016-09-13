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
			if (string.IsNullOrWhiteSpace(format))
				return string.Empty;
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

		public static string GenObjInfo(this object data, string format)
		{
			try
			{
				if (data is QRData)
					return GenInfo(data as QRData, format);
				if (data == null)
					return string.Empty;
				var t = data.GetType();
				return geRegex.Replace(format, m => t.GetProperty(m.Groups[1].Value, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Default)?.GetValue(data)?.ToString());
			}
			catch
			{
				return string.Empty;
			}
		}

		static QRCodeGenerator qrGenerator = new QRCodeGenerator();
		public static System.Windows.Media.Imaging.BitmapSource ToQRImageSource(this string source, int pixelsPerModule = 20, string foreground = "#000000", string background = "#FFFFFF", bool full = true) => ToQRCode(source).GetGraphic(pixelsPerModule, foreground, background, !full).ToBitmapSource();
		public static QRCode ToQRCode(this string source) => new QRCode(qrGenerator.CreateQrCode(source, QRCodeGenerator.ECCLevel.H));

		public static System.Drawing.Bitmap CreateBitmapImage(string sImageText, System.Drawing.Color fore, System.Drawing.Color back)
		{
			var objBmpImage = new System.Drawing.Bitmap(1, 1);

			// Create the Font object for the image text drawing.
			var objFont = new System.Drawing.Font("Arial", 72, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
			sImageText = sImageText.Trim();
			var size = System.Windows.Forms.TextRenderer.MeasureText(sImageText, objFont);

			// Create the bmpImage again with the correct size for the text and font.
			objBmpImage = new System.Drawing.Bitmap(size.Width, size.Height);

			// Add the colors to the new bitmap.
			var objGraphics = System.Drawing.Graphics.FromImage(objBmpImage);

			// Set Background color
			objGraphics.Clear(back);
			objGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
			objGraphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
			objGraphics.DrawString(sImageText, objFont, new System.Drawing.SolidBrush(fore), 0, 0);
			objGraphics.Flush();
			return (objBmpImage);
		}

		public static System.Drawing.Bitmap MakeBmp(this QRData data, int ppm, string qrFormat, string qrFore, string qrBack, bool full, bool showText, string textFormat, string textFore, string textBack, int borderWidth, int textPercent)
		{
			return MakeBmp(data, ppm, qrFormat, qrFore.ToSDColor(), qrBack.ToSDColor(), full, showText, textFormat, textFore.ToSDColor(), textBack.ToSDColor(), borderWidth, textPercent);
		}

		public static System.Drawing.Bitmap MakeBmp(this QRData data, int ppm, string qrFormat, System.Drawing.Color qrFore, System.Drawing.Color qrBack, bool full, bool showText, string textFormat, System.Drawing.Color textFore, System.Drawing.Color textBack, int borderWidth, int textPercent)
		{
			string qrText = data.GenInfo(qrFormat);
			if (string.IsNullOrWhiteSpace(qrText))
				return null;
			var text = showText ? data.GenInfo(textFormat) : null;
			if (string.IsNullOrWhiteSpace(text))
				return qrText.ToQRCode().GetGraphic(ppm, qrFore, qrBack, !full);
			else
				return qrText.ToQRCode().GetGraphic(ppm, qrFore, qrBack, CreateBitmapImage(text, textFore, textBack), textPercent, borderWidth, !full);
		}

	}
}

using System.Windows;
using Core;

namespace QRBase.Controls
{
	public partial class QRItem
	{
		public QRItem()
		{
			InitializeComponent();
		}

		public static readonly DependencyProperty QRDataSourceProperty = DependencyProperty.Register(
			nameof(QRDataSource),
			typeof(QRData),
			typeof(QRItem),
			new PropertyMetadata((d, e) => (d as QRItem)?.Update())
		);

		public QRData QRDataSource
		{
			get { return (QRData)GetValue(QRDataSourceProperty); }
			set { SetValue(QRDataSourceProperty, value); }
		}

		public static readonly DependencyProperty QRFormatProperty = DependencyProperty.Register(
			nameof(QRFormat),
			typeof(string),
			typeof(QRItem),
			new PropertyMetadata((d, e) => (d as QRItem)?.Update())
		);

		public string QRFormat
		{
			get { return (string)GetValue(QRFormatProperty); }
			set { SetValue(QRFormatProperty, value); }
		}

		public static readonly DependencyProperty TextFormatProperty = DependencyProperty.Register(
			nameof(TextFormat),
			typeof(string),
			typeof(QRItem),
			new PropertyMetadata((d, e) => (d as QRItem)?.Update())
		);

		public string TextFormat
		{
			get { return (string)GetValue(TextFormatProperty); }
			set { SetValue(TextFormatProperty, value); }
		}

		public static readonly DependencyProperty ShowTextProperty = DependencyProperty.Register(
			nameof(ShowText),
			typeof(bool),
			typeof(QRItem),
			new PropertyMetadata(false, (d, e) => (d as QRItem)?.Update())
		);

		public bool ShowText
		{
			get { return (bool)GetValue(ShowTextProperty); }
			set { SetValue(ShowTextProperty, value); }
		}

		public static readonly DependencyProperty QRBackgroundProperty = DependencyProperty.Register(
			nameof(QRBackground),
			typeof(string),
			typeof(QRItem),
			new PropertyMetadata("#FFFFFF", (d, e) => (d as QRItem)?.Update())
		);

		public string QRBackground
		{
			get { return (string)GetValue(QRBackgroundProperty); }
			set { SetValue(QRBackgroundProperty, value); }
		}

		public static readonly DependencyProperty QRForegroundProperty = DependencyProperty.Register(
			nameof(QRForeground),
			typeof(string),
			typeof(QRItem),
			new PropertyMetadata("#000000", (d, e) => (d as QRItem)?.Update())
		);

		public string QRForeground
		{
			get { return (string)GetValue(QRForegroundProperty); }
			set { SetValue(QRForegroundProperty, value); }
		}

		public static readonly DependencyProperty QRSizeProperty = DependencyProperty.Register(
			nameof(QRSize),
			typeof(int),
			typeof(QRItem),
			new PropertyMetadata(20, (d, e) => (d as QRItem)?.Update())
		);

		public int QRSize
		{
			get { return (int)GetValue(QRSizeProperty); }
			set { SetValue(QRSizeProperty, value); }
		}

		public static readonly DependencyProperty QRFullProperty = DependencyProperty.Register(
			nameof(QRFull),
			typeof(bool),
			typeof(QRItem),
			new PropertyMetadata(true, (d, e) => (d as QRItem)?.Update())
		);

		public bool QRFull
		{
			get { return (bool)GetValue(QRFullProperty); }
			set { SetValue(QRFullProperty, value); }
		}

		public static readonly DependencyProperty TextBackgroundProperty = DependencyProperty.Register(
			nameof(TextBackground),
			typeof(string),
			typeof(QRItem),
			new PropertyMetadata("#000000", (d, e) => (d as QRItem)?.Update())
		);

		public string TextBackground
		{
			get { return (string)GetValue(TextBackgroundProperty); }
			set { SetValue(TextBackgroundProperty, value); }
		}

		public static readonly DependencyProperty TextForegroundProperty = DependencyProperty.Register(
			nameof(TextForeground),
			typeof(string),
			typeof(QRItem),
			new PropertyMetadata("#FFFFFF", (d, e) => (d as QRItem)?.Update())
		);

		public string TextForeground
		{
			get { return (string)GetValue(TextForegroundProperty); }
			set { SetValue(TextForegroundProperty, value); }
		}

		public static readonly DependencyProperty TextBorderWidthProperty = DependencyProperty.Register(
			nameof(TextBorderWidth),
			typeof(int),
			typeof(QRItem),
			new PropertyMetadata(2, (d, e) => (d as QRItem)?.Update())
		);

		public int TextBorderWidth
		{
			get { return (int)GetValue(TextBorderWidthProperty); }
			set { SetValue(TextBorderWidthProperty, value); }
		}

		public static readonly DependencyProperty TextPercentProperty = DependencyProperty.Register(
			nameof(TextPercent),
			typeof(int),
			typeof(QRItem),
			new PropertyMetadata(50, (d, e) => (d as QRItem)?.Update())
		);

		public int TextPercent
		{
			get { return (int)GetValue(TextPercentProperty); }
			set { SetValue(TextPercentProperty, value); }
		}

		public string Info => QRDataSource?.GenInfo(QRFormat);

		public void Update()
		{
			QRImageContainer.ToolTip = Info;
			QRImageContainer.Source = QRDataSource?.MakeBmp(QRSize, QRFormat, QRForeground, QRBackground, QRFull, ShowText, TextFormat, TextForeground, TextBackground, TextBorderWidth, TextPercent)?.ToBitmapSource();
		}
	}
}

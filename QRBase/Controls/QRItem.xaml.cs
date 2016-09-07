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

		public string Info => QRDataSource?.GenInfo(QRFormat);

		public void Update()
		{
			var info = Info;
			QRImageContainer.ToolTip = info;
			QRImageContainer.Source = info?.ToQRImageSource(QRSize, QRForeground, QRBackground);
		}
	}
}

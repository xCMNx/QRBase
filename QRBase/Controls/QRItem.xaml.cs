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

		public string Info => QRDataSource?.GenInfo(QRFormat);

		public void Update()
		{
			var info = Info;
			QRImageContainer.ToolTip = info;
			QRImageContainer.Source = info?.ToQRImageSource();
		}
	}
}

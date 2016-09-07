using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QRBase.Controls
{
	/// <summary>
	/// Логика взаимодействия для QRBar.xaml
	/// </summary>
	public partial class QRBar
	{
		public QRBar()
		{
			InitializeComponent();
		}

		public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
			nameof(Text),
			typeof(string),
			typeof(QRBar),
			new PropertyMetadata((d, e) => (d as QRBar)?.Update())
		);

		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
			nameof(Size),
			typeof(int),
			typeof(QRBar),
			new PropertyMetadata(20, (d, e) => (d as QRBar)?.Update())
		);

		public int Size
		{
			get { return (int)GetValue(SizeProperty); }
			set { SetValue(SizeProperty, value); }
		}

		public void Update()
		{
			QRImageContainer.ToolTip = Text;
			QRImageContainer.Source = Text?.ToQRImageSource(Size);
		}
	}
}

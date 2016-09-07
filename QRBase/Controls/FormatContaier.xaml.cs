using System;
using System.ComponentModel;
using System.Windows;

namespace QRBase.Controls
{
	/// <summary>
	/// Логика взаимодействия для FormatContaier.xaml
	/// </summary>
	public partial class FormatContaier : INotifyPropertyChanged
	{
		public FormatContaier()
		{
			InitializeComponent();
		}

		public static readonly DependencyProperty DataSourceProperty = DependencyProperty.Register(
			nameof(DataSource),
			typeof(object),
			typeof(FormatContaier),
			new PropertyMetadata((d, e) => (d as FormatContaier)?.Update())
		);

		public object DataSource
		{
			get { return GetValue(DataSourceProperty); }
			set { SetValue(DataSourceProperty, value); }
		}

		public static readonly DependencyProperty FormatProperty = DependencyProperty.Register(
			nameof(Format),
			typeof(string),
			typeof(FormatContaier),
			new PropertyMetadata((d, e) => (d as FormatContaier)?.Update())
		);

		public string Format
		{
			get { return (string)GetValue(FormatProperty); }
			set { SetValue(FormatProperty, value); }
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public virtual void NotifyPropertyChanged(String propertyName = "")
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public string Text => DataSource.GenObjInfo(Format);

		public void Update()
		{
			NotifyPropertyChanged(nameof(Text));
		}
	}
}

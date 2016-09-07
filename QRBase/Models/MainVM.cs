using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Core;
using QRCoder;

namespace QRBase.Models
{
	public class MainVM: CommandSink, INotifyPropertyChanged
	{
		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;

		public void NotifyPropertyChanged(string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		public void Route(object sender, PropertyChangedEventArgs e) => PropertyChanged?.Invoke(sender, e);

		public void NotifyPropertiesChanged(params string[] propertyNames)
		{
			if (PropertyChanged != null)
			{
				foreach (var prop in propertyNames)
					PropertyChanged(this, new PropertyChangedEventArgs(prop));
			}
		}
		#endregion

		static MainVM _self;
		public static MainVM Instance => _self ?? (_self = new MainVM());
		public IDataProvider DataProvider = new xlsx.xlsxProvider();

		QuestionBlock _Question = new QuestionBlock();
		public QuestionBlock Question => _Question;

		ToastBlock _Toast = new ToastBlock();
		public ToastBlock Toast => _Toast;

		ICollection<QRData> _SelectedQRList;
		public ICollection<QRData> SelectedQRList
		{
			get { return _SelectedQRList; }
			set
			{
				_SelectedQRList = value;
				NotifyPropertyChanged(nameof(SelectedQRList));
			}
		}
		public IEnumerable<QRData> QRList { get; private set; }
		const string QRFORMAT = "QRFORMAT";
		string _QRFormat;
		public string QRFormat
		{
			get { return _QRFormat; }
			set
			{
				if(_QRFormat != value)
				{
					_QRFormat = value;
					Helpers.ConfigWrite(QRFORMAT, value);
					NotifyPropertyChanged(nameof(QRFormat));
				}
			}
		}

		const string TITLEFORMAT = "TITLEFORMAT";
		string _TitleFormat;
		public string TitleFormat
		{
			get { return _TitleFormat; }
			set
			{
				if (_TitleFormat != value)
				{
					_TitleFormat = value;
					Helpers.ConfigWrite(TITLEFORMAT, value);
					NotifyPropertyChanged(nameof(TitleFormat));
				}
			}
		}

		const string EXPORTPATH = "EXPORTPATH";
		string _ExportPath;
		public string ExportPath
		{
			get { return _ExportPath; }
			set
			{
				if (_ExportPath != value)
				{
					_ExportPath = value;
					Helpers.ConfigWrite(EXPORTPATH, value);
					NotifyPropertyChanged(nameof(ExportPath));
				}
			}
		}

		const string QRSIZE = "QRSIZE";
		int _QRSize;
		public int QRSize
		{
			get { return _QRSize; }
			set
			{
				if (_QRSize != value)
				{
					_QRSize = value;
					Helpers.ConfigWrite(QRSIZE, value);
					NotifyPropertyChanged(nameof(QRSize));
				}
			}
		}

		const string QRFOREGROUND = "QRFOREGROUND";
		string _QRForeground;
		public string QRForeground
		{
			get { return _QRForeground; }
			set
			{
				if (_QRForeground != value)
				{
					_QRForeground = value;
					Helpers.ConfigWrite(QRFOREGROUND, value);
					NotifyPropertyChanged(nameof(QRForeground));
				}
			}
		}

		const string QRBACKGROUND = "QRBACKGROUND";
		string _QRBackground;
		public string QRBackground
		{
			get { return _QRBackground; }
			set
			{
				if (_QRBackground != value)
				{
					_QRBackground = value;
					Helpers.ConfigWrite(QRBACKGROUND, value);
					NotifyPropertyChanged(nameof(QRBackground));
				}
			}
		}

		void InitSettings()
		{
			_QRFormat = Helpers.ConfigRead(QRFORMAT, string.Empty, true);
			_TitleFormat = Helpers.ConfigRead(TITLEFORMAT, string.Empty, true);
			_QRSize = Helpers.ConfigRead(QRSIZE, 20, true);
			_QRForeground = Helpers.ConfigRead(QRFOREGROUND, "#FF000000", true);
			_QRBackground = Helpers.ConfigRead(QRBACKGROUND, "#FFFFFFFF", true);
			_ExportPath = Helpers.ConfigRead(EXPORTPATH, string.Empty, true);
			NotifyPropertiesChanged(nameof(QRFormat), nameof(TitleFormat), nameof(QRSize), nameof(QRForeground), nameof(QRBackground), nameof(QRList));
		}

		public static readonly RoutedCommand NewQRCommand = new RoutedCommand();
		public static readonly RoutedCommand NewFieldCommand = new RoutedCommand();
		public static readonly RoutedCommand SettingsCommand = new RoutedCommand();
		public static readonly RoutedCommand ExportCommand = new RoutedCommand();
		public MainVM()
		{
			_self = this;
			Helpers.mainCTX = System.Threading.SynchronizationContext.Current;
			RegisterCommand(
				NewQRCommand,
				param => true,
				NewQRCommandExecute
			);
			RegisterCommand(
				NewFieldCommand,
				param => true,
				NewFieldCommandExecute
			);
			RegisterCommand(
				SettingsCommand,
				param => true,
				SettingsCommandExecute
			);
			RegisterCommand(
				ExportCommand,
				param => true,
				ExportCommandExecute
			);
			Application.Current.Exit += Current_Exit;
			init();
		}

		private async void ExportCommandExecute(object parameter)
		{
			if (!(SelectedQRList != null && SelectedQRList.Count > 0))
				return;
			var dict = new IParametersRequestItem[] {
				new ParametersRequestItem(){ Title = "Путь", Value = new PathValueItem(ExportPath) },
			};

			if (await Question.ShowAsync(dict, "Экспорт"))
				try
				{
					ExportPath = Path.GetFullPath(dict[0].Value.Value);
					Directory.CreateDirectory(ExportPath);
					foreach(var d in SelectedQRList)
					{
						var q = d.GenInfo(QRFormat).ToQRCode().GetGraphic(QRSize, QRForeground, QRBackground);
						var fn = Path.Combine(ExportPath, d.GenInfo(TitleFormat).GetValidFileName());
						q.Save($"{fn}.png", ImageFormat.Png);
					}
				}
				catch (Exception e)
				{
					Toast.ShowAsync(e.Message);
					Helpers.ConsoleWrite(e.Message, ConsoleColor.Yellow);
				}
		}

		private async void SettingsCommandExecute(object parameter)
		{
			var dict = new IParametersRequestItem[] {
				new ParametersRequestItem(){ Title = "Шаблон QR", Value = new StringValueItem(QRFormat) }
				,new ParametersRequestItem(){ Title = "Шаблон заголовка", Value = new StringValueItem(TitleFormat) }
				,new ParametersRequestItem(){ Title = "Размер", Value = new NumericValueItem(QRSize, 10, 1024) }
				,new ParametersRequestItem(){ Title = "Цвет фона", Value = new ColorValueItem(QRBackground) }
				,new ParametersRequestItem(){ Title = "Основной вет", Value = new ColorValueItem(QRForeground) }
			};

			if (await Question.ShowAsync(dict, "Настройки"))
				try
				{
					QRFormat = dict[0].Value.Value;
					TitleFormat = dict[1].Value.Value;
					QRSize = dict[2].Value.Int;
					QRBackground = dict[3].Value.ColorHex;
					QRForeground = dict[4].Value.ColorHex;
				}
				catch (Exception e)
				{
					Toast.ShowAsync(e.Message);
					Helpers.ConsoleWrite(e.Message, ConsoleColor.Yellow);
				}
		}

		private void Current_Exit(object sender, ExitEventArgs e)
		{
			DataProvider?.Finalize();
		}

		async void NewQRCommandExecute(object parameter)
		{
			await DataProvider?.AddData(Question.ShowAsync, Toast.ShowAsync);
			QRList = await DataProvider?.GetData(Question.ShowAsync, Toast.ShowAsync);
			NotifyPropertyChanged(nameof(QRList));
		}

		async void NewFieldCommandExecute(object parameter)
		{
			await DataProvider?.AddField(Question.ShowAsync, Toast.ShowAsync);
		}

		async Task<bool> CheckSettingsPass()
		{
			var dict = new IParametersRequestItem[] {
				new ParametersRequestItem(){ Title = "Password", Value = new PasswordValueItem(string.Empty) }
			};

			while (await Question.ShowAsync(dict))
				if (Helpers.SetEncryptionKey((dict[0].Value as StringValueItem).String))
					return true;
			return false;
		}

		void init()
		{
			Task.Factory.StartNew(async () =>
			{
				if (!await CheckSettingsPass() || !await DataProvider.Init(Question.ShowAsync, Toast.ShowAsync))
					Helpers.Post(Application.Current.Shutdown);
				else
				{
					QRList = await DataProvider.GetData(Question.ShowAsync, Toast.ShowAsync);
					InitSettings();
				}
			});
		}

	}
}

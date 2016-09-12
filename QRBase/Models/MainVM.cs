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

		#region Settings params
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
		string _TextFormat;
		public string TextFormat
		{
			get { return _TextFormat; }
			set
			{
				if (_TextFormat != value)
				{
					_TextFormat = value;
					Helpers.ConfigWrite(TITLEFORMAT, value);
					NotifyPropertyChanged(nameof(TextFormat));
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

		const string QRFULL = "QRFULL";
		bool _QRFull;
		public bool QRFull
		{
			get { return _QRFull; }
			set
			{
				if (_QRFull != value)
				{
					_QRFull = value;
					Helpers.ConfigWrite(QRFULL, value);
					NotifyPropertyChanged(nameof(QRFull));
				}
			}
		}

		const string PREVIEW_SIZE = "PREVIEW_SIZE";
		int _QRPreviewSize;
		public int QRPreviewSize
		{
			get { return _QRPreviewSize; }
			set
			{
				if (_QRPreviewSize != value)
				{
					_QRPreviewSize = value;
					Helpers.ConfigWrite(PREVIEW_SIZE, value);
					NotifyPropertyChanged(nameof(QRPreviewSize));
				}
			}
		}

		const string TEXT_FOREGROUND = "TEXT_FOREGROUND";
		string _TextForeground;
		public string TextForeground
		{
			get { return _TextForeground; }
			set
			{
				if (_TextForeground != value)
				{
					_TextForeground = value;
					Helpers.ConfigWrite(TEXT_FOREGROUND, value);
					NotifyPropertyChanged(nameof(TextForeground));
				}
			}
		}

		const string TEXT_BACKGROUND = "TEXT_BACKGROUND";
		string _TextBackground;
		public string TextBackground
		{
			get { return _TextBackground; }
			set
			{
				if (_TextBackground != value)
				{
					_TextBackground = value;
					Helpers.ConfigWrite(TEXT_BACKGROUND, value);
					NotifyPropertyChanged(nameof(TextBackground));
				}
			}
		}

		const string TEXT_PERCENT = "TEXT_PERCENT";
		int _TextPercent;
		public int TextPercent
		{
			get { return _TextPercent; }
			set
			{
				if (_TextPercent != value)
				{
					_TextPercent = value;
					Helpers.ConfigWrite(TEXT_PERCENT, value);
					NotifyPropertyChanged(nameof(TextPercent));
				}
			}
		}

		const string TEXT_SHOW = "TEXT_SHOW";
		bool _ShowText;
		public bool ShowText
		{
			get { return _ShowText; }
			set
			{
				if (_ShowText != value)
				{
					_ShowText = value;
					Helpers.ConfigWrite(TEXT_SHOW, value);
					NotifyPropertyChanged(nameof(ShowText));
				}
			}
		}

		const string TEXT_BORDER_WIDTH = "TEXT_BORDER_WIDTH";
		int _TextBorderWidth;
		public int TextBorderWidth
		{
			get { return _TextBorderWidth; }
			set
			{
				if (_TextBorderWidth != value)
				{
					_TextBorderWidth = value;
					Helpers.ConfigWrite(TEXT_BORDER_WIDTH, value);
					NotifyPropertyChanged(nameof(TextBorderWidth));
				}
			}
		}

		void InitSettings()
		{
			QRFormat = Helpers.ConfigRead(QRFORMAT, string.Empty, true);
			TextFormat = Helpers.ConfigRead(TITLEFORMAT, string.Empty, true);
			QRSize = Helpers.ConfigRead(QRSIZE, 20, true);
			QRForeground = Helpers.ConfigRead(QRFOREGROUND, "#FF000000", true);
			QRBackground = Helpers.ConfigRead(QRBACKGROUND, "#FFFFFFFF", true);
			QRFull = Helpers.ConfigRead(QRFULL, true, true);
			QRPreviewSize = Helpers.ConfigRead(PREVIEW_SIZE, 150, true);
			TextForeground = Helpers.ConfigRead(TEXT_FOREGROUND, "#FF000000", true);
			TextBackground = Helpers.ConfigRead(TEXT_BACKGROUND, "#FFFFFFFF", true);
			TextBorderWidth = Helpers.ConfigRead(TEXT_BORDER_WIDTH, 2, true);
			TextPercent = Helpers.ConfigRead(TEXT_PERCENT, 50, true);
			ShowText = Helpers.ConfigRead(TEXT_SHOW, false, true);
		}
		#endregion

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

		#region Export
		#region Params
		const string EXPORTPATH = "EXPORTPATH";
		public string ExportPath
		{
			get { return Helpers.ConfigRead(EXPORTPATH, string.Empty, true); }
			set { Helpers.ConfigWrite(EXPORTPATH, value); }
		}

		const string PAGES_EXPORT = "PAGES_EXPORT";
		public bool PagesExport
		{
			get { return Helpers.ConfigRead(PAGES_EXPORT, true, true); }
			set { Helpers.ConfigWrite(PAGES_EXPORT, value); }
		}

		const string PAGE_COLUMNS = "PAGE_COLUMNS";
		public int PageColumns
		{
			get { return Helpers.ConfigRead(PAGE_COLUMNS, 4, true); }
			set { Helpers.ConfigWrite(PAGE_COLUMNS, value); }
		}

		const string PAGE_ROWS = "PAGE_ROWS";
		public int PageRows
		{
			get { return Helpers.ConfigRead(PAGE_ROWS, 4, true); }
			set { Helpers.ConfigWrite(PAGE_ROWS, value); }
		}

		const string PAGE_QRSIZE = "PAGE_QRSIZE";
		public int PageQRSize
		{
			get { return Helpers.ConfigRead(PAGE_QRSIZE, 250, true); }
			set { Helpers.ConfigWrite(PAGE_QRSIZE, value); }
		}

		const string PAGE_INTERVAL = "PAGE_INTERVAL";
		public int PageInterval
		{
			get { return Helpers.ConfigRead(PAGE_INTERVAL, 20, true); }
			set { Helpers.ConfigWrite(PAGE_INTERVAL, value); }
		}

		const string PAGE_BORDER_WIDTH = "PAGE_BORDER_WIDTH";
		public int PageBorderWidth
		{
			get { return Helpers.ConfigRead(PAGE_BORDER_WIDTH, 20, true); }
			set { Helpers.ConfigWrite(PAGE_BORDER_WIDTH, value); }
		}
		#endregion

		private async void ExportCommandExecute(object parameter)
		{
			if (!(SelectedQRList != null && SelectedQRList.Count > 0))
				return;
			var dict = new IParametersRequestItem[] {
				new ParametersRequestItem(){ Title = "Путь", Value = new PathValueItem(ExportPath) }
				,new ParametersRequestItem(){ Title = "Листы", Value = new BoolValueItem(PagesExport) }
				,new ParametersRequestItem(){ Title = "Столбцов на лист", Value = new NumericValueItem(PageColumns, 1, 1000) }
				,new ParametersRequestItem(){ Title = "Строк на лист", Value = new NumericValueItem(PageRows, 1, 1000) }
				,new ParametersRequestItem(){ Title = "Размер QR", Value = new NumericValueItem(PageQRSize, 21, 740) }
				,new ParametersRequestItem(){ Title = "Интервал", Value = new NumericValueItem(PageInterval, 0, 100) }
				,new ParametersRequestItem(){ Title = "Ширина рамки", Value = new NumericValueItem(PageBorderWidth, 0, 10) }
			};

			if (await Question.ShowAsync(dict, "Экспорт"))
				try
				{
					var qrFore = _QRForeground.ToSDColor();
					var qrBack = _QRBackground.ToSDColor();
					var textFore = _TextForeground.ToSDColor();
					var textBack = _TextBackground.ToSDColor();
					Func<QRData, System.Drawing.Bitmap> makeBmp = d => d.MakeBmp(QRSize, QRFormat, qrFore, qrBack, QRFull, ShowText, TextFormat, textFore, textBack, TextBorderWidth, TextPercent);
					var exportPath = ExportPath = Path.GetFullPath(dict[0].Value.Value);
					Directory.CreateDirectory(exportPath);
					if(!(PagesExport = dict[1].Value.Value))
						foreach(var d in SelectedQRList)
							makeBmp(d)?.Save($"{Path.Combine(exportPath, d.GenInfo(TextFormat).GetValidFileName())}.png", ImageFormat.Png);
					else
					{
						var cellSize = PageQRSize = dict[4].Value.Int;
						var cols = PageColumns = dict[2].Value.Int;
						var rows = PageRows = dict[3].Value.Int;
						var border = PageBorderWidth = dict[6].Value.Int;
						var interval = PageInterval = dict[5].Value.Int;
						var fullinterval = interval + border;
						var step = cellSize + fullinterval;
						var i = 0;
						var page = 1;
						while (i < SelectedQRList.Count)
						{
							using (var bmp = new System.Drawing.Bitmap(step * cols, step * rows))
							{
								var p = border > 0 ? new System.Drawing.Pen(System.Drawing.Color.Black, border) : null;
								using (var graph = System.Drawing.Graphics.FromImage(bmp))
								{
									var x = fullinterval / 2;
									var y = fullinterval / 2;
									for (var r = 0; r < cols && i < SelectedQRList.Count; r++, i++)
									{
										for (var c = 0; c < cols && i < SelectedQRList.Count; c++, i++)
										{
											if (p != null)
												graph.DrawRectangle(p, x - fullinterval / 2, y - fullinterval / 2, cellSize + fullinterval, cellSize + fullinterval);
											var qBmp = makeBmp(SelectedQRList.ElementAt(i));
											graph.DrawImage(qBmp, new System.Drawing.Rectangle(x, y, cellSize, cellSize));
											x += step;
										}
										y += step;
									}
								}
								bmp.Save($"{exportPath}\\{page++}.png", ImageFormat.Png);
							}
						}
					}
				}
				catch (Exception e)
				{
					Toast.ShowAsync(e.Message);
					Helpers.ConsoleWrite(e.Message, ConsoleColor.Yellow);
				}
		}
		#endregion

		private async void SettingsCommandExecute(object parameter)
		{
			var dict = new IParametersRequestItem[] {
				new ParametersRequestItem(){ Title = "Шаблон QR", Value = new MemoValueItem(QRFormat) }
				,new ParametersRequestItem(){ Title = "Шаблон подписи", Value = new MemoValueItem(TextFormat) }
				,new ParametersRequestItem(){ Title = "PPM", Hint="Пикселей на модуль", Value = new NumericValueItem(QRSize, 1, 100) }
				,new ParametersRequestItem(){ Title = "Без отступа", Value = new BoolValueItem(QRFull) }
				,new ParametersRequestItem(){ Title = "Цвет фона QR", Value = new ColorValueItem(QRBackground) }
				,new ParametersRequestItem(){ Title = "Цвет QR", Value = new ColorValueItem(QRForeground) }
				,new ParametersRequestItem(){ Title = "Цвет фона подписи", Value = new ColorValueItem(TextBackground) }
				,new ParametersRequestItem(){ Title = "Цвет подписи", Value = new ColorValueItem(TextForeground) }
				,new ParametersRequestItem(){ Title = "Подпись внутри", Value = new BoolValueItem(ShowText) }
				,new ParametersRequestItem(){ Title = "Шарина рамки подписи", Hint = "При тексте внутри QR", Value = new NumericValueItem(TextBorderWidth, 1, 100) }
				,new ParametersRequestItem(){ Title = "Подпись %", Hint = "Занимаемая подписью область QR", Value = new NumericValueItem(TextPercent, 1, 100) }
			};

			if (await Question.ShowAsync(dict, "Настройки"))
				try
				{
					int i = 0;
					QRFormat = dict[i++].Value.Value;
					TextFormat = dict[i++].Value.Value;
					QRSize = dict[i++].Value.Int;
					QRFull = dict[i++].Value.Value;
					QRBackground = dict[i++].Value.ColorHex;
					QRForeground = dict[i++].Value.ColorHex;
					TextBackground = dict[i++].Value.ColorHex;
					TextForeground = dict[i++].Value.ColorHex;
					ShowText = dict[i++].Value.Value;
					TextBorderWidth = dict[i++].Value.Int;
					TextPercent = dict[i++].Value.Int;
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
					InitSettings();
					QRList = await DataProvider.GetData(Question.ShowAsync, Toast.ShowAsync);
					NotifyPropertiesChanged(nameof(QRList));
				}
			});
		}

	}
}

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Core;

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

		public static readonly RoutedCommand NewQRCommand = new RoutedCommand();
		public static readonly RoutedCommand NewFieldCommand = new RoutedCommand();
		public MainVM()
		{
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
			Application.Current.Exit += Current_Exit;
			init();
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
					_QRFormat = Helpers.ConfigRead(QRFORMAT, string.Empty, true);
					QRList = await DataProvider.GetData(Question.ShowAsync, Toast.ShowAsync);
					NotifyPropertiesChanged(nameof(QRFormat), nameof(QRList));
				}
			});
		}

	}
}

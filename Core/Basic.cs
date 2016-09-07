using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Core
{
	public interface IParametersValueItem: INotifyPropertyChanged
	{
		dynamic Value { get; set; }
	}

	public interface IParametersRequestItem : INotifyPropertyChanged
	{
		string Title { get; }
		dynamic Value { get; set; }
		string Hint { get; }
	}

	public class ParametersRequestItem : BindableBase, IParametersRequestItem
	{
		public string Title { get; set; }
		public dynamic Value { get; set; }
		public string Hint { get; set; }
	}

	public class HeaderRequestItem : BindableBase, IParametersRequestItem
	{
		public string Title { get; set; }
		public dynamic Value { get { throw new InvalidOperationException(); } set { throw new InvalidOperationException(); } }
		public string Hint { get; set; }
	}

	public class TextRequestItem : BindableBase, IParametersRequestItem
	{
		public string Title { get; set; }
		public dynamic Value { get { throw new InvalidOperationException(); } set { throw new InvalidOperationException(); } }
		public string Hint { get { throw new InvalidOperationException(); } set { throw new InvalidOperationException(); } }
	}

	public class BoolRequestItem : BindableBase, IParametersRequestItem
	{
		public string Title { get; set; }
		public dynamic Value { get; set; }
		public string Hint { get; set; }
	}

	public class MemoRequestItem : BindableBase, IParametersRequestItem
	{
		public string Title { get; set; }
		public dynamic Value { get; set; }
		public string Hint { get; set; }
	}

	public class CheckValueItem : BindableBase, IParametersValueItem
	{
		public string Title { get; set; }
		public bool IsChecked { get; set; }
		public dynamic Value
		{
			get { return IsChecked; }
			set { IsChecked = (bool)value; }
		}
		public string Hint { get; set; }
		public CheckValueItem(bool isChecked, string title, string hint = null)
		{
			IsChecked = isChecked;
			Title = title;
			Hint = hint;
		}
	}

	public class ListValueItem : BindableBase, IParametersValueItem
	{
		public IList<IParametersValueItem> List { get; set; }
		public dynamic Value
		{
			get { return List; }
			set { List = (IList<IParametersValueItem>)value; }
		}
		public ListValueItem(IList<IParametersValueItem> list)
		{
			List = list;
		}
	}

	public class ParametersValueItem : BindableBase, IParametersValueItem
	{
		public dynamic Value { get; set; }
		public ParametersValueItem(object value)
		{
			Value = value;
		}
	}

	public class ComboListValueItem : ParametersValueItem
	{
		public IList<object> List { get; set; }
		public ComboListValueItem(IList<object> list, object value) : base(value)
		{
			List = list;
		}
	}

	public class StringValueItem : BindableBase, IParametersValueItem
	{
		public string String => Value;
		public dynamic Value { get; set; }
		public StringValueItem(string val)
		{
			Value = val;
		}
	}

	public class PasswordValueItem : StringValueItem
	{
		public PasswordValueItem(string val) : base(val) { }
	}

	public class BoolValueItem : BindableBase, IParametersValueItem
	{
		public bool Bool => Value;
		public dynamic Value { get; set; }
		public BoolValueItem(bool val)
		{
			Value = val;
		}
	}

	public class NumericValueItem : BindableBase, IParametersValueItem
	{
		public int Int => Convert.ToInt32(Value);
		public dynamic Value { get; set; }
		public decimal MinValue { get; private set; } = decimal.MinValue;
		public decimal MaxValue { get; private set; } = decimal.MaxValue;
		public string NumericFormat { get; private set; }
		public NumericValueItem(decimal val, decimal? min = null, decimal? max = null, string numericFormat = "N2")
		{
			Value = val;
			if (min.HasValue)
				MinValue = min.Value;
			if (max.HasValue)
				MaxValue = max.Value;
			NumericFormat = numericFormat;
		}
		public NumericValueItem(int val, int? min = null, int? max = null) 
		{
			Value = Convert.ToDecimal(val);
			MinValue = Convert.ToDecimal(min.HasValue ? min.Value : int.MinValue);
			MaxValue = Convert.ToDecimal(max.HasValue ? max.Value : int.MaxValue);
			MaxValue = max.Value;
			NumericFormat = "0";
		}
	}

	public class PathValueItem : StringValueItem
	{
		public BasicCommand Exec { get; private set; }
		public PathValueItem(string path) : base(path)
		{
			Exec = new BasicCommand(_Exec);
		}
		void _Exec(object prop)
		{
			using (var od = new System.Windows.Forms.FolderBrowserDialog())
			{
				od.SelectedPath = Value;
				if (od.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					Value = od.SelectedPath;
					NotifyPropertyChanged(nameof(Value));
				}
			}
		}
	}

	public class OpenFileValueItem : StringValueItem
	{
		public BasicCommand Exec { get; private set; }
		string _filter;
		bool _exists;
		public OpenFileValueItem(string path, string filter, bool exists = true) : base(path)
		{
			_filter = filter;
			_exists = exists;
			Exec = new BasicCommand(_Exec);
		}

		void _Exec(object prop)
		{
			var dlg = new Microsoft.Win32.OpenFileDialog();
			dlg.FileName = Value;
			dlg.Filter = _filter;
			dlg.AddExtension = true;
			dlg.CheckFileExists = _exists;
			if (dlg.ShowDialog() == true)
			{
				Value = dlg.FileName;
				NotifyPropertyChanged(nameof(Value));
			}
		}
	}

	public class ColorValueItem : BindableBase, IParametersValueItem
	{
		public static System.Windows.Media.Color ToSWMColor(System.Drawing.Color color) => System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
		public static System.Drawing.Color ToSDColor(System.Windows.Media.Color color) => System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
		public System.Drawing.Color DColor => ToSDColor(Value);
		public System.Drawing.Color MColor => Value;
		public string ColorHex => System.Drawing.ColorTranslator.ToHtml(DColor);
		public dynamic Value { get; set; }
		public BasicCommand Exec { get; private set; }
		public ColorValueItem(System.Windows.Media.Color color)
		{
			init(color);
		}
		void init(System.Windows.Media.Color color)
		{
			Value = color;
			Exec = new BasicCommand(_Exec);
		}
		public ColorValueItem(string color)
		{
			init(ToSWMColor(System.Drawing.ColorTranslator.FromHtml(color)));
		}
		public ColorValueItem(System.Drawing.Color color)
		{
			init(ToSWMColor(color));
		}
		void _Exec(object prop)
		{
			using (var cd = new System.Windows.Forms.ColorDialog())
			{
				cd.Color = DColor;
				if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					Value = ToSWMColor(cd.Color);
					NotifyPropertyChanged(nameof(Value));
				}
			}
		}
	}

	public class BasicCommand : System.Windows.Input.ICommand
	{
		public event EventHandler CanExecuteChanged;
		Action<object> _action;
		public bool CanExecute(object parameter) => true;
		public void Execute(object parameter) => _action(parameter);
		public BasicCommand(Action<object> action)
		{
			_action = action;
		}
	}

	public delegate Task<bool> ParametersRequest(IList<IParametersRequestItem> requestFields, string title = null, params string[] messages);
	public delegate Task ShowText(string text, int delay = -1);

}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;
using OfficeOpenXml;

namespace xlsx
{
	public class xlsxProvider : IDataProvider
	{
		protected const string PREFIX = "OPENXML_";
		protected const string FIRST_IS_HEAD = PREFIX + "FIRST_IS_HEAD[{0}]";
		protected const string FIRST_COL = PREFIX + "FIRST_COL[{0}]";
		protected const string FIRST_ROW = PREFIX + "FIRST_ROW[{0}]";
		protected const string FILENAME = PREFIX + "FILENAME[{0}]";

		public string ConfigName { get; set; }

		public int FirstRow
		{
			get { return Helpers.ConfigRead(string.Format(FIRST_ROW, ConfigName), 1, true); }
			set { Helpers.ConfigWrite(string.Format(FIRST_ROW, ConfigName), value); }
		}

		public int FirstCol
		{
			get { return Helpers.ConfigRead(string.Format(FIRST_COL, ConfigName), 1, true); }
			set { Helpers.ConfigWrite(string.Format(FIRST_COL, ConfigName), value); }
		}

		public bool FirstIsHead
		{
			get { return Helpers.ConfigRead(string.Format(FIRST_IS_HEAD, ConfigName), false, true); }
			set { Helpers.ConfigWrite(string.Format(FIRST_IS_HEAD, ConfigName), value); }
		}

		public string Filename
		{
			get { return Helpers.ConfigRead(string.Format(FILENAME, ConfigName), "storage.xlsx", true); }
			set { Helpers.ConfigWrite(string.Format(FILENAME, ConfigName), value); }
		}

		ExcelPackage _Package;

		Dictionary<string, string> _Fields = new Dictionary<string, string>();

		public ICollection<string> Fields => _Fields.Keys;

		public async Task<bool> Ask(string message, string caption, ParametersRequest parametersRequest)
		{
			var dict = new IParametersRequestItem[] { new HeaderRequestItem() { Title = message } };
			return await parametersRequest(dict, caption);
		}

		public async Task<bool> Finalize()
		{
			_Package?.Save();
			await Task.Yield();
			return true;
		}

		public async Task<IEnumerable<QRData>> GetData(ParametersRequest parametersRequest, ShowText showText)
		{
			var lst = new List<QRData>();
			try
			{
				var sheet = _Package.Workbook.Worksheets[1];
				if (sheet.Dimension == null)
					return lst;
				var last = sheet.Dimension.End.Row;
				for (int i = FirstRow + (FirstIsHead ? 1 : 0); i <= last; i++)
				{
					var data = new QRData();
					foreach (var f in _Fields)
						data.SimpleSetValue(f.Key, sheet.Cells[$"{f.Value}{i}"].Value);
					lst.Add(data);
				}
				await Task.Yield();
			}
			catch (Exception e)
			{
				showText(e.Message);
				Helpers.ConsoleWrite(e.Message, ConsoleColor.Yellow);
			}
			return lst;
		}

		public async Task<bool> AddField(ParametersRequest parametersRequest, ShowText showText)
		{
			try
			{
				var sheet = _Package.Workbook.Worksheets[1];
				var c = (sheet.Dimension == null ? 1 : sheet.Dimension.End.Column + 1).GetExcelColumnName();
				string name = c;
				var dict = new IParametersRequestItem[] {
					new ParametersRequestItem(){ Title = "Field name", Value = new StringValueItem(name) }
				};

				if (await parametersRequest(dict, "OpenXml: Добавление поля"))
				{
					name = (dict[0].Value as StringValueItem).Value;
					if (_Fields.Keys.Contains(name))
					{
						showText($"Fild \"{name}\" already exists.");
						return false;
					}
					if (FirstIsHead)
						sheet.Cells[$"{c}{FirstRow}"].Value = name;
					_Fields[name] = c;
					return true;
				}
			}
			catch (Exception e)
			{
				showText(e.Message);
				Helpers.ConsoleWrite(e.Message, ConsoleColor.Yellow);
			}
			return false;
		}

		public async Task<QRData> AddData(ParametersRequest parametersRequest, ShowText showText)
		{
			try
			{
				if (_Fields.Count == 0)
				{
					showText($"There is no fields.");
					return null;
				}
				var dict = new List<IParametersRequestItem>();
				foreach (var f in _Fields.Keys)
					dict.Add(new ParametersRequestItem() { Title = f, Value = new StringValueItem(null) });
				if (await parametersRequest(dict, "OpenXml: Добавление объекта"))
				{
					var data = new QRData();
					var sheet = _Package.Workbook.Worksheets[1];
					var r = sheet.Dimension.End.Row + 1;
					foreach (var prop in dict)
					{
						data.SimpleSetValue(prop.Title, prop.Value.Value);
						sheet.Cells[$"{_Fields[prop.Title]}{r}"].Value = prop.Value.Value;
					}
					return data;
				}
			}
			catch (Exception e)
			{
				showText(e.Message);
				Helpers.ConsoleWrite(e.Message, ConsoleColor.Yellow);
			}
			return null;
		}

		public async Task<bool> GetOptions(ParametersRequest parametersRequest, string message)
		{
			var dict = new IParametersRequestItem[] {
				new ParametersRequestItem(){ Title = "Filename", Value = new OpenFileValueItem(Filename, "OpenXml|*.xlsx", false) }
				,new ParametersRequestItem(){ Title = "First row", Value = new NumericValueItem(FirstRow, 1, 65535) }
				,new ParametersRequestItem(){ Title = "First column", Value = new NumericValueItem(FirstCol, 1, 65535) }
				,new ParametersRequestItem(){ Title = "First row is title", Value = new BoolValueItem(FirstIsHead) }
			};

			if (await parametersRequest(dict, "OpenXml: Настройки файла", message))
			{
				Filename = dict[0].Value.Value;
				FirstRow = dict[1].Value.Int;
				FirstCol = dict[2].Value.Int;
				FirstIsHead = dict[3].Value.Value;
				return true;
			}
			return false;
		}

		public async Task<ExcelPackage> Open(ParametersRequest parametersRequest, ShowText showText)
		{
			string message = null;
			while (await GetOptions(parametersRequest, message))
			{
				ExcelPackage pkg = null;
				try
				{
					if (File.Exists(Filename))
					{
						pkg = new ExcelPackage(new FileInfo(Filename));
						var sheet = pkg.Workbook.Worksheets[1];
						if (sheet.Dimension != null)
						{
							var last = sheet.Dimension.End.Column;
							for (int c = FirstCol; c <= last; c++)
							{
								var cn = c.GetExcelColumnName();
								_Fields[FirstIsHead ? sheet.Cells[$"{cn}{FirstRow}"].Value.ToString() : cn] = cn;
							}
						}
					}
					else
					{
						pkg = new ExcelPackage(new FileStream(Filename, FileMode.CreateNew));
						pkg.Workbook.Worksheets.Add("data");
						FirstRow = 1;
						FirstIsHead = true;
					}
					return pkg;
				}
				catch (Exception e)
				{
					pkg?.Dispose();
					message = e.Message;
					Helpers.ConsoleWrite(message, ConsoleColor.Red);
				}
			}
			return null;
		}

		public async Task<bool> Init(ParametersRequest parametersRequest, ShowText showText)
		{
			_Package = await Open(parametersRequest, showText);
			return _Package != null;
		}
	}
}

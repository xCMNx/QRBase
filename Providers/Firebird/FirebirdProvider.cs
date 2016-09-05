using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Core;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Isql;

namespace Firebird
{
	public class FirebirdProvider : IDataProvider
	{
		public const string DEFAULT_USER = "SYSDBA";
		public const string DEFAULT_PASS = "masterkey";
		public const string DEFAULT_CHARSET = "UTF8";
		public const string DEFAULT_SERVER = "localhost";
		public const string DEFAULT_PATH = "db.fdb";
		public const string DEFAULT_CONSTR = "character set=UTF8;user id=SYSDBA;password=masterkey;dialect=3;initial catalog=db.fdb;server type=Default;data source=localhost";
		protected const string PREFIX = "FIREBIRD_";
		protected const string USER = PREFIX + "USER[{0}]";
		protected const string PASS = PREFIX + "PASS[{0}]";
		protected const string SRV = PREFIX + "SRV[{0}]";
		protected const string PATH = PREFIX + "PATH[{0}]";

		public string ConfigName { get; set; }

		public string User
		{
			get { return Helpers.ConfigRead(string.Format(USER, ConfigName), DEFAULT_USER, true); }
			set { Helpers.ConfigWrite(string.Format(USER, ConfigName), value); }
		}

		public string Pass
		{
			get { return Helpers.ConfigRead(string.Format(PASS, ConfigName), DEFAULT_PASS, true); }
			set { Helpers.ConfigWrite(string.Format(PASS, ConfigName), value); }
		}

		public string Server
		{
			get { return Helpers.ConfigRead(string.Format(SRV, ConfigName), DEFAULT_SERVER, true); }
			set { Helpers.ConfigWrite(string.Format(SRV, ConfigName), value); }
		}

		public string Path
		{
			get { return Helpers.ConfigRead(string.Format(PATH, ConfigName), DEFAULT_PATH, true); }
			set { Helpers.ConfigWrite(string.Format(PATH, ConfigName), value); }
		}

		bool isEmbedded => string.IsNullOrWhiteSpace(Server);

		public string ConnectionString
		{
			get
			{
				var sb = new FbConnectionStringBuilder();
				sb.UserID = User;
				sb.Password = Pass;
				sb.DataSource = isEmbedded ? null : Server.Trim();
				sb.Database = Path;
				sb.ServerType = isEmbedded ? FbServerType.Embedded : FbServerType.Default;
				return sb.ToString();
			}
		}

		List<QRData> lst = new List<QRData>();
		public async Task<IEnumerable<QRData>> GetData(ParametersRequest parametersRequest, ShowText showText)
		{
			await Task.Yield();
			return lst.ToArray();
		}

		FbConnection _con;

		public async Task<bool> GetConnectInfo(ParametersRequest parametersRequest, string message)
		{
			var dict = new IParametersRequestItem[] {
				new ParametersRequestItem(){ Title = "Server", Value = new StringValueItem(Server) }
				,new ParametersRequestItem(){ Title = "Path", Value = new StringValueItem(Path) }
				,new ParametersRequestItem(){ Title = "User", Value = new StringValueItem(User) }
				,new ParametersRequestItem(){ Title = "Password", Value = new PasswordValueItem(Pass) }
			};

			if (await parametersRequest(dict, "Firebird: Настройки соединения", message))
			{
				Server = (dict[0].Value as StringValueItem).String;
				Path = (dict[1].Value as StringValueItem).String;
				User = (dict[2].Value as StringValueItem).String;
				Pass = (dict[3].Value as StringValueItem).String;
				return true;
			}
			return false;
		}

		public async Task<bool> Ask(string message, string caption, ParametersRequest parametersRequest)
		{
			var dict = new IParametersRequestItem[] { new HeaderRequestItem() { Title = message } };
			return await parametersRequest(dict, caption);
		}

		void CreateDb(FbConnection connection)
		{
			var assembly = Assembly.GetExecutingAssembly();
			var stream = assembly.GetManifestResourceStream(typeof(FirebirdProvider).Namespace + ".struct.sql");
			string script = new StreamReader(stream, System.Text.Encoding.ASCII).ReadToEnd();
			FbScript fbs = new FbScript(script);
			fbs.Parse();
			FbConnection.CreateDatabase(connection.ConnectionString);
			using (var c = new FbConnection(connection.ConnectionString))
			{
				c.Open();
				try
				{
					using (FbTransaction myTransaction = c.BeginTransaction())
					{
						FbBatchExecution fbe = new FbBatchExecution(c, fbs);
						fbe.CommandExecuting += (sender, args) => args.SqlCommand.Transaction = myTransaction;
						fbe.Execute(true);
					}
				}
				finally
				{
					c.Close();
				}
			}
		}

		public async Task<FbConnection> Connect(ParametersRequest parametersRequest, ShowText showText)
		{
			string message = null;
			while (await GetConnectInfo(parametersRequest, message))
			{
				try
				{
					var conn = new FbConnection(ConnectionString);
					try
					{
						conn.Open();
						await GetTables(conn);
						return conn;
					}
					catch (FbException e)
					{
						if (e.ErrorCode == 335544344)
						{
							if (!await Ask("File not exists" + (isEmbedded ? " or database is opened" : string.Empty) + ".\r\nTry to create file?", "Firebird: Error", parametersRequest))
								throw;
							CreateDb(conn);
							conn.Open();
							return conn;
						}
						else
							throw;
					}
				}
				catch (Exception e)
				{
					message = e.Message;
					Helpers.ConsoleWrite(message, ConsoleColor.Red);
				}
			}
			return null;
		}

		public async Task<IEnumerable<string>> ListOfX(FbConnection connection, string sqlName, bool noSys = true)
		{
			var lst = new List<string>();
			using (var c = connection.CreateCommand())
			using (c.Transaction = connection.BeginTransaction())
				try
				{
					c.CommandText = $"SELECT DISTINCT R.RDB${sqlName}_NAME FROM RDB${sqlName}S R";
					if (noSys)
						c.CommandText += " where (RDB$SYSTEM_FLAG = 0)";
					using (var reader = await c.ExecuteReaderAsync())
						while (reader.Read())
							lst.Add(reader.GetString(0));
				}
				finally
				{
					c.Transaction.Rollback();
				}
			return lst;
		}

		public async Task<IEnumerable<string>> GetFields(ParametersRequest parametersRequest, ShowText showText) => await GetTables(_con);

		public async Task<IEnumerable<string>> GetTables(FbConnection connection) => await ListOfX(connection, "RELATION");

		public async Task<bool> Init(ParametersRequest parametersRequest, ShowText showText)
		{
			_con?.Close();
			_con = await Connect(parametersRequest, showText);
			return _con != null;
		}

		public async Task<bool> Finalize()
		{
			_con?.Close();
			await Task.Yield();
			return true;
		}
	}
}

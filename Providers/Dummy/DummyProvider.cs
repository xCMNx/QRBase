using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core;

namespace Dummy
{
	public class DummyProvider : IDataProvider
	{
		List<QRData> lst = new List<QRData>();
		public IEnumerable<QRData> GetData() => lst.ToArray();
		QRData getNew()
		{
			var d = new QRData() as dynamic;
			d.Value = "123";
			d.Value2 = "321";
			return d;
		}

		public DummyProvider()
		{
			lst.Add(getNew());
			lst.Add(getNew());
			lst.Add(getNew());
			lst.Add(getNew());
			lst.Add(getNew());
			lst.Add(getNew());
		}

		public async Task<bool> Init(ParametersRequest parametersRequest, ShowText showText)
		{
			await Task.Yield();
			return true;
		}

		public async Task<IEnumerable<QRData>> GetData(ParametersRequest parametersRequest, ShowText showText)
		{
			await Task.Yield();
			throw new NotImplementedException();
		}

		public async  Task<bool> Finalize()
		{
			await Task.Yield();
			throw new NotImplementedException();
		}
	}
}

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core
{
	public interface IDataProvider
	{
		ICollection<string> Fields { get; }
		Task<IEnumerable<QRData>> GetData(ParametersRequest parametersRequest, ShowText showText);
		Task<bool> Init(ParametersRequest parametersRequest, ShowText showText);
		Task<bool> AddField(ParametersRequest parametersRequest, ShowText showText);
		Task<QRData> AddData(ParametersRequest parametersRequest, ShowText showText);
		Task<bool> Finalize();
	}
}

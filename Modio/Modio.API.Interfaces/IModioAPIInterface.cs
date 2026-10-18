using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Modio.API.Interfaces;

public interface IModioAPIInterface : IDisposable
{
	void SetBasePath(string value);

	void SetDefaultHeader(string name, string value);

	void AddDefaultPathParameter(string key, string value);

	void RemoveDefaultPathParameter(string key);

	void RemoveDefaultHeader(string name);

	void AddDefaultParameter(string value);

	void RemoveDefaultParameter(string value);

	void ResetConfiguration();

	Task<(Error, Stream)> DownloadFile(string url, CancellationToken token = default(CancellationToken));

	Task<(Error error, T? result)> GetJson<T>(ModioAPIRequest request) where T : struct;

	Task<(Error error, JToken)> GetJson(ModioAPIRequest request);
}

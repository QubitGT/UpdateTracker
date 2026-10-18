using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace NativeWebSocket;

public class WaitForBackgroundThread
{
	public ConfiguredTaskAwaitable.ConfiguredTaskAwaiter GetAwaiter()
	{
		return Task.Run(delegate
		{
		}).ConfigureAwait(continueOnCapturedContext: false).GetAwaiter();
	}
}

namespace Cysharp.Threading.Tasks.Linq;

public interface IAsyncWriter<T>
{
	UniTask YieldAsync(T value);
}

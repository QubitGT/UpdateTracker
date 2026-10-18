using System.Collections;

namespace Fusion;

public interface INetworkArray : IEnumerable
{
	object this[int index] { get; set; }
}

using System.Collections;

namespace Valve.Newtonsoft.Json.Utilities;

internal interface IWrappedCollection : IList, ICollection, IEnumerable
{
	object UnderlyingCollection { get; }
}

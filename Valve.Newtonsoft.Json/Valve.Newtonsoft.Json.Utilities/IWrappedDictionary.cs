using System.Collections;

namespace Valve.Newtonsoft.Json.Utilities;

internal interface IWrappedDictionary : IDictionary, ICollection, IEnumerable
{
	object UnderlyingDictionary { get; }
}

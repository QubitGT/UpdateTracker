using System;
using System.Collections.Generic;

namespace Meta.Conduit;

internal interface IInstanceResolver
{
	IEnumerable<object> GetObjectsOfType(Type type);
}

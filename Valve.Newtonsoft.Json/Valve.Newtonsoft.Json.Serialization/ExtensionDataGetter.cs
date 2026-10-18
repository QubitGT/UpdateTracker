using System.Collections.Generic;

namespace Valve.Newtonsoft.Json.Serialization;

public delegate IEnumerable<KeyValuePair<object, object>> ExtensionDataGetter(object o);

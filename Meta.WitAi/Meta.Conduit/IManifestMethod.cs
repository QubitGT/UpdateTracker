using System.Collections.Generic;

namespace Meta.Conduit;

internal interface IManifestMethod
{
	string ID { get; set; }

	List<ManifestParameter> Parameters { get; set; }

	string Assembly { get; set; }
}

using System.IO;

namespace Backtrace.Unity.Model.Breadcrumbs.Storage;

internal interface IBreadcrumbFile
{
	bool Exists();

	void Delete();

	Stream GetCreateStream();

	Stream GetIOStream();

	Stream GetWriteStream();
}

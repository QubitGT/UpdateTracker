using System.Text;

namespace Fusion;

public interface ILogDumpable
{
	void Dump(StringBuilder builder);
}

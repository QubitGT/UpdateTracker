using System.Diagnostics;
using Pathfinding.Ionic.Zip;

namespace Pathfinding.Ionic;

internal abstract class SelectionCriterion
{
	internal virtual bool Verbose { get; set; }

	internal abstract bool Evaluate(string filename);

	[Conditional("SelectorTrace")]
	protected static void CriterionTrace(string format, params object[] args)
	{
	}

	internal abstract bool Evaluate(ZipEntry entry);
}

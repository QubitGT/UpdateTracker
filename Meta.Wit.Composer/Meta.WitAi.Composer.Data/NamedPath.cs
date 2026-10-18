using JetBrains.Annotations;
using Meta.WitAi.Composer.Integrations;
using UnityEngine.Scripting;

namespace Meta.WitAi.Composer.Data;

[UsedImplicitly]
public class NamedPath : ReservedContextPath
{
	protected override string ReservedPath => WitComposerConstants.CONTEXT_MAP_RESERVED_PATH;

	[Preserve]
	public NamedPath()
	{
	}

	public override string ToString()
	{
		return GetValue();
	}
}

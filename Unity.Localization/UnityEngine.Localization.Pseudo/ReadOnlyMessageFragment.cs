using System.Diagnostics;
using UnityEngine.Pool;

namespace UnityEngine.Localization.Pseudo;

[DebuggerDisplay("ReadOnly: {Text}")]
public class ReadOnlyMessageFragment : MessageFragment
{
	internal static readonly ObjectPool<ReadOnlyMessageFragment> Pool = new ObjectPool<ReadOnlyMessageFragment>(() => new ReadOnlyMessageFragment(), null, null, null, collectionCheck: false);

	public string Text => ToString();
}

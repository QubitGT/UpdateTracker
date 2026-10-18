using System.Diagnostics;
using UnityEngine.Pool;

namespace UnityEngine.Localization.Pseudo;

[DebuggerDisplay("Writable: {Text}")]
public class WritableMessageFragment : MessageFragment
{
	internal static readonly ObjectPool<WritableMessageFragment> Pool = new ObjectPool<WritableMessageFragment>(() => new WritableMessageFragment(), null, null, null, collectionCheck: false);

	public string Text
	{
		get
		{
			return ToString();
		}
		set
		{
			Initialize(base.Message, value);
		}
	}
}

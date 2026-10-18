using System;

namespace Fusion;

[Serializable]
public abstract class FusionMppmCommand
{
	public virtual bool NeedsAck => false;

	public virtual string PersistentKey => null;

	public abstract void Execute();
}

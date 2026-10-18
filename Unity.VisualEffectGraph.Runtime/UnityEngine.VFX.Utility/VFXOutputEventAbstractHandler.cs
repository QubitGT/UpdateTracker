using System;

namespace UnityEngine.VFX.Utility;

[ExecuteAlways]
[RequireComponent(typeof(VisualEffect))]
public abstract class VFXOutputEventAbstractHandler : MonoBehaviour
{
	public bool executeInEditor = true;

	public ExposedProperty outputEvent = "On Received Event";

	public abstract bool canExecuteInEditor { get; }

	protected VisualEffect m_VisualEffect { get; private set; }

	protected virtual void OnEnable()
	{
		m_VisualEffect = GetComponent<VisualEffect>();
		if (m_VisualEffect != null)
		{
			VisualEffect visualEffect = m_VisualEffect;
			visualEffect.outputEventReceived = (Action<VFXOutputEventArgs>)Delegate.Combine(visualEffect.outputEventReceived, new Action<VFXOutputEventArgs>(OnOutputEventRecieved));
		}
	}

	protected virtual void OnDisable()
	{
		if (m_VisualEffect != null)
		{
			VisualEffect visualEffect = m_VisualEffect;
			visualEffect.outputEventReceived = (Action<VFXOutputEventArgs>)Delegate.Remove(visualEffect.outputEventReceived, new Action<VFXOutputEventArgs>(OnOutputEventRecieved));
		}
	}

	private void OnOutputEventRecieved(VFXOutputEventArgs args)
	{
		if ((Application.isPlaying || (executeInEditor && canExecuteInEditor)) && args.nameId == (int)outputEvent)
		{
			OnVFXOutputEvent(args.eventAttribute);
		}
	}

	public abstract void OnVFXOutputEvent(VFXEventAttribute eventAttribute);
}

using System;
using UnityEngine;

namespace Oculus.Interaction.Locomotion;

public class LocomotionActionsBroadcaster : MonoBehaviour, ILocomotionEventBroadcaster
{
	public enum LocomotionAction
	{
		Crouch,
		StandUp,
		ToggleCrouch,
		Run,
		Walk,
		ToggleRun,
		Jump
	}

	private class Decorator : ValueToValueDecorator<ulong, LocomotionAction>
	{
		private Decorator()
		{
		}

		public static Decorator GetFromContext(Context context = null)
		{
			if (context == null)
			{
				context = Context.Global.GetInstance();
			}
			return context.GetOrCreateSingleton(() => new Decorator());
		}
	}

	[SerializeField]
	[Optional]
	private Context _context;

	private UniqueIdentifier _identifier;

	public int Identifier => _identifier.ID;

	public event Action<LocomotionEvent> WhenLocomotionPerformed = delegate
	{
	};

	protected virtual void Awake()
	{
		_identifier = UniqueIdentifier.Generate((_context != null) ? _context : Context.Global.GetInstance(), this);
	}

	public void SendLocomotionAction(LocomotionAction action)
	{
		LocomotionEvent locomotionEvent = CreateLocomotionEventAction(Identifier, action, Pose.identity, _context);
		this.WhenLocomotionPerformed(locomotionEvent);
		DisposeLocomotionAction(locomotionEvent);
	}

	public void Crouch()
	{
		SendLocomotionAction(LocomotionAction.Crouch);
	}

	public void StandUp()
	{
		SendLocomotionAction(LocomotionAction.StandUp);
	}

	public void ToggleCrouch()
	{
		SendLocomotionAction(LocomotionAction.ToggleCrouch);
	}

	public void Run()
	{
		SendLocomotionAction(LocomotionAction.Run);
	}

	public void Walk()
	{
		SendLocomotionAction(LocomotionAction.Walk);
	}

	public void ToggleRun()
	{
		SendLocomotionAction(LocomotionAction.ToggleRun);
	}

	public void Jump()
	{
		SendLocomotionAction(LocomotionAction.Jump);
	}

	public void InjectOptionalContext(Context context)
	{
		_context = context;
	}

	public static LocomotionEvent CreateLocomotionEventAction(int identifier, LocomotionAction action, Pose pose = default(Pose), Context context = null)
	{
		LocomotionEvent result = new LocomotionEvent(identifier, pose, LocomotionEvent.TranslationType.None, LocomotionEvent.RotationType.None);
		Decorator.GetFromContext(context).AddDecoration(result.EventId, action);
		return result;
	}

	public static bool TryGetLocomotionActions(LocomotionEvent locomotionEvent, out LocomotionAction action, Context context = null)
	{
		if (Decorator.GetFromContext(context).TryGetDecoration(locomotionEvent.EventId, out var decoration))
		{
			action = decoration;
			return true;
		}
		action = LocomotionAction.Crouch;
		return false;
	}

	public static void DisposeLocomotionAction(LocomotionEvent locomotionEvent, Context context = null)
	{
		Decorator.GetFromContext(context).RemoveDecoration(locomotionEvent.EventId);
	}
}

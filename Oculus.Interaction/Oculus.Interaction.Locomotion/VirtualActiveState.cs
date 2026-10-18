using UnityEngine;

namespace Oculus.Interaction.Locomotion;

public class VirtualActiveState : MonoBehaviour, IActiveState
{
	[SerializeField]
	private bool _active;

	public bool Active
	{
		get
		{
			return _active;
		}
		set
		{
			_active = value;
		}
	}
}

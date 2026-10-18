using UnityEngine;

namespace Oculus.Interaction;

public class GameObjectActiveState : MonoBehaviour, IActiveState
{
	[SerializeField]
	private GameObject _sourceGameObject;

	[SerializeField]
	private bool _sourceActiveSelf;

	public bool SourceActiveSelf
	{
		get
		{
			return _sourceActiveSelf;
		}
		set
		{
			_sourceActiveSelf = value;
		}
	}

	public bool Active
	{
		get
		{
			if (!_sourceActiveSelf)
			{
				return _sourceGameObject.activeInHierarchy;
			}
			return _sourceGameObject.activeSelf;
		}
	}

	protected virtual void Start()
	{
	}

	public void InjectAllGameObjectActiveState(GameObject sourceGameObject)
	{
		InjectSourceGameObject(sourceGameObject);
	}

	public void InjectSourceGameObject(GameObject sourceGameObject)
	{
		_sourceGameObject = sourceGameObject;
	}
}

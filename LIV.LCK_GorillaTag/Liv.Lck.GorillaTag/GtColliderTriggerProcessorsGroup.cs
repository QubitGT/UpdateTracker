using UnityEngine;

namespace Liv.Lck.GorillaTag;

public class GtColliderTriggerProcessorsGroup : MonoBehaviour
{
	private GtColliderTriggerProcessor _currentTriggerProcessor;

	public void SetCurrentTriggerProcessor(GtColliderTriggerProcessor triggerProcessor)
	{
		_currentTriggerProcessor = triggerProcessor;
	}

	public GtColliderTriggerProcessor GetCurrentTriggerProcessor()
	{
		return _currentTriggerProcessor;
	}

	public void ClearAllTriggers()
	{
		_currentTriggerProcessor = null;
	}
}

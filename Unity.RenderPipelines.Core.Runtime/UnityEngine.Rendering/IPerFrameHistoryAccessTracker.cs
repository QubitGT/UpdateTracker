namespace UnityEngine.Rendering;

public interface IPerFrameHistoryAccessTracker
{
	void RequestAccess<Type>() where Type : ContextItem;
}

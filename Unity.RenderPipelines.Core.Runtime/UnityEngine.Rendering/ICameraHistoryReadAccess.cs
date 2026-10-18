namespace UnityEngine.Rendering;

public interface ICameraHistoryReadAccess
{
	public delegate void HistoryRequestDelegate(IPerFrameHistoryAccessTracker historyAccess);

	event HistoryRequestDelegate OnGatherHistoryRequests;

	Type GetHistoryForRead<Type>() where Type : ContextItem;
}

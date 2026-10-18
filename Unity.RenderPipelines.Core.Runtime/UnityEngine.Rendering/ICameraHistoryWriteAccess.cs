namespace UnityEngine.Rendering;

public interface ICameraHistoryWriteAccess
{
	bool IsAccessRequested<Type>() where Type : ContextItem;

	Type GetHistoryForWrite<Type>() where Type : ContextItem, new();

	bool IsWritten<Type>() where Type : ContextItem;
}

using System;

namespace Fusion;

public sealed class NetworkObjectSpawnException : Exception
{
	public NetworkObjectTypeId? TypeId { get; }

	public NetworkSpawnStatus Status { get; }

	public override string Message
	{
		get
		{
			string text = ((Status != NetworkSpawnStatus.FailedToLoadPrefabSynchronously) ? $"Failed to spawn: {Status}" : "Failed to load prefab synchronously. Use async spawn instead or enable EnqueueIncompleteSynchronousSpawns");
			if (TypeId.HasValue)
			{
				text += $" (prefab: {TypeId})";
			}
			return text;
		}
	}

	public NetworkObjectSpawnException(NetworkSpawnStatus status, NetworkObjectTypeId? id = null)
	{
		TypeId = id;
		Status = status;
	}
}

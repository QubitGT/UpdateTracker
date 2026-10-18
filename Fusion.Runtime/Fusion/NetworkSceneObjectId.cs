using System;

namespace Fusion;

public struct NetworkSceneObjectId : IEquatable<NetworkSceneObjectId>
{
	public SceneRef Scene;

	public int ObjectId;

	public NetworkSceneLoadId LoadId;

	[Obsolete("Use LoadId instead.")]
	public int SceneLoadId => LoadId.Value;

	public bool IsValid => Scene.IsValid;

	public NetworkSceneObjectId(SceneRef scene, int objectId, NetworkSceneLoadId loadId = default(NetworkSceneLoadId))
	{
		Scene = scene;
		ObjectId = objectId;
		LoadId = loadId;
	}

	public override string ToString()
	{
		return $"[Scene: {Scene.ToString(brackets: false, prefix: false)}, ObjectId: {ObjectId}, SceneLoadId: {LoadId.Value}]";
	}

	public bool Equals(NetworkSceneObjectId other)
	{
		return Scene.Equals(other.Scene) && ObjectId == other.ObjectId && LoadId == other.LoadId;
	}

	public override bool Equals(object obj)
	{
		return obj is NetworkSceneObjectId other && Equals(other);
	}

	public override int GetHashCode()
	{
		int hashCode = Scene.GetHashCode();
		hashCode = (hashCode * 397) ^ ObjectId;
		return (hashCode * 397) ^ LoadId.Value;
	}
}

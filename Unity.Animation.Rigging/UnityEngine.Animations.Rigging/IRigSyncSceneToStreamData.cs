namespace UnityEngine.Animations.Rigging;

internal interface IRigSyncSceneToStreamData
{
	Transform[] syncableTransforms { get; }

	SyncableProperties[] syncableProperties { get; }

	bool[] rigStates { get; }
}

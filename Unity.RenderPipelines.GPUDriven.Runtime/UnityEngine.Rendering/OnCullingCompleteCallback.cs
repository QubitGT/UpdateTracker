using Unity.Jobs;

namespace UnityEngine.Rendering;

internal delegate void OnCullingCompleteCallback(JobHandle jobHandle, in BatchCullingContext cullingContext, in BatchCullingOutput cullingOutput);

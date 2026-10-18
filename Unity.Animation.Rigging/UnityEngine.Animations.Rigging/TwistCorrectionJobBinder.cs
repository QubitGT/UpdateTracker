using Unity.Collections;

namespace UnityEngine.Animations.Rigging;

public class TwistCorrectionJobBinder<T> : AnimationJobBinder<TwistCorrectionJob, T> where T : struct, IAnimationJobData, ITwistCorrectionData
{
	public override TwistCorrectionJob Create(Animator animator, ref T data, Component component)
	{
		TwistCorrectionJob result = default(TwistCorrectionJob);
		result.source = ReadOnlyTransformHandle.Bind(animator, data.source);
		result.sourceInverseBindRotation = Quaternion.Inverse(data.source.localRotation);
		result.axisMask = data.twistAxis;
		WeightedTransformArray twistNodes = data.twistNodes;
		WeightedTransformArrayBinder.BindReadWriteTransforms(animator, component, twistNodes, out result.twistTransforms);
		WeightedTransformArrayBinder.BindWeights(animator, component, twistNodes, data.twistNodesProperty, out result.twistWeights);
		result.weightBuffer = new NativeArray<float>(twistNodes.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		result.twistBindRotations = new NativeArray<Quaternion>(twistNodes.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		for (int i = 0; i < twistNodes.Count; i++)
		{
			Transform transform = twistNodes[i].transform;
			result.twistBindRotations[i] = transform.localRotation;
		}
		return result;
	}

	public override void Destroy(TwistCorrectionJob job)
	{
		job.twistTransforms.Dispose();
		job.twistWeights.Dispose();
		job.twistBindRotations.Dispose();
		job.weightBuffer.Dispose();
	}
}

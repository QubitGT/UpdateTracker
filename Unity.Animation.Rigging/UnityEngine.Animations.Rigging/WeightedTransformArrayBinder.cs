using Unity.Collections;

namespace UnityEngine.Animations.Rigging;

public class WeightedTransformArrayBinder
{
	public static void BindReadOnlyTransforms(Animator animator, Component component, WeightedTransformArray weightedTransformArray, out NativeArray<ReadOnlyTransformHandle> transforms)
	{
		transforms = new NativeArray<ReadOnlyTransformHandle>(weightedTransformArray.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		for (int i = 0; i < weightedTransformArray.Count; i++)
		{
			transforms[i] = ReadOnlyTransformHandle.Bind(animator, weightedTransformArray[i].transform);
		}
	}

	public static void BindReadWriteTransforms(Animator animator, Component component, WeightedTransformArray weightedTransformArray, out NativeArray<ReadWriteTransformHandle> transforms)
	{
		transforms = new NativeArray<ReadWriteTransformHandle>(weightedTransformArray.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		for (int i = 0; i < weightedTransformArray.Count; i++)
		{
			transforms[i] = ReadWriteTransformHandle.Bind(animator, weightedTransformArray[i].transform);
		}
	}

	public static void BindWeights(Animator animator, Component component, WeightedTransformArray weightedTransformArray, string name, out NativeArray<PropertyStreamHandle> weights)
	{
		weights = new NativeArray<PropertyStreamHandle>(weightedTransformArray.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		for (int i = 0; i < weightedTransformArray.Count; i++)
		{
			weights[i] = animator.BindStreamProperty(component.transform, component.GetType(), name + ".m_Item" + i + ".weight");
		}
	}
}

using System;
using Unity.Burst;
using Unity.Collections;

namespace UnityEngine.Animations.Rigging;

[BurstCompile]
internal struct RigSyncSceneToStreamJob : IAnimationJob
{
	public struct TransformSyncer : IDisposable
	{
		public NativeArray<TransformSceneHandle> sceneHandles;

		public NativeArray<TransformStreamHandle> streamHandles;

		public static TransformSyncer Create(int size)
		{
			TransformSyncer result = default(TransformSyncer);
			result.sceneHandles = new NativeArray<TransformSceneHandle>(size, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			result.streamHandles = new NativeArray<TransformStreamHandle>(size, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			return result;
		}

		public void Dispose()
		{
			if (sceneHandles.IsCreated)
			{
				sceneHandles.Dispose();
			}
			if (streamHandles.IsCreated)
			{
				streamHandles.Dispose();
			}
		}

		public void BindAt(int index, Animator animator, Transform transform)
		{
			sceneHandles[index] = animator.BindSceneTransform(transform);
			streamHandles[index] = animator.BindStreamTransform(transform);
		}

		public void Sync(ref AnimationStream stream)
		{
			int i = 0;
			for (int length = sceneHandles.Length; i < length; i++)
			{
				TransformSceneHandle value = sceneHandles[i];
				if (value.IsValid(stream))
				{
					TransformStreamHandle value2 = streamHandles[i];
					value.GetLocalTRS(stream, out var position, out var rotation, out var scale);
					value2.SetLocalTRS(stream, position, rotation, scale, useMask: true);
					streamHandles[i] = value2;
					sceneHandles[i] = value;
				}
			}
		}
	}

	internal struct PropertySyncer : IDisposable
	{
		public NativeArray<PropertySceneHandle> sceneHandles;

		public NativeArray<PropertyStreamHandle> streamHandles;

		public NativeArray<float> buffer;

		public static PropertySyncer Create(int size)
		{
			PropertySyncer result = default(PropertySyncer);
			result.sceneHandles = new NativeArray<PropertySceneHandle>(size, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			result.streamHandles = new NativeArray<PropertyStreamHandle>(size, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			result.buffer = new NativeArray<float>(size, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			return result;
		}

		public void Dispose()
		{
			if (sceneHandles.IsCreated)
			{
				sceneHandles.Dispose();
			}
			if (streamHandles.IsCreated)
			{
				streamHandles.Dispose();
			}
			if (buffer.IsCreated)
			{
				buffer.Dispose();
			}
		}

		public void BindAt(int index, Animator animator, Component component, string property)
		{
			sceneHandles[index] = animator.BindSceneProperty(component.transform, component.GetType(), property);
			streamHandles[index] = animator.BindStreamProperty(component.transform, component.GetType(), property);
		}

		public void Sync(ref AnimationStream stream)
		{
			AnimationSceneHandleUtility.ReadFloats(stream, sceneHandles, buffer);
			AnimationStreamHandleUtility.WriteFloats(stream, streamHandles, buffer, useMask: true);
		}

		public NativeArray<float> StreamValues(ref AnimationStream stream)
		{
			AnimationStreamHandleUtility.ReadFloats(stream, streamHandles, buffer);
			return buffer;
		}
	}

	public TransformSyncer transformSyncer;

	public PropertySyncer propertySyncer;

	public PropertySyncer rigWeightSyncer;

	public PropertySyncer constraintWeightSyncer;

	public NativeArray<float> rigStates;

	public NativeArray<int> rigConstraintEndIdx;

	public NativeArray<PropertyStreamHandle> modulatedConstraintWeights;

	public void ProcessRootMotion(AnimationStream stream)
	{
	}

	public void ProcessAnimation(AnimationStream stream)
	{
		transformSyncer.Sync(ref stream);
		propertySyncer.Sync(ref stream);
		rigWeightSyncer.Sync(ref stream);
		constraintWeightSyncer.Sync(ref stream);
		NativeArray<float> nativeArray = rigWeightSyncer.StreamValues(ref stream);
		NativeArray<float> buffer = constraintWeightSyncer.StreamValues(ref stream);
		int num = 0;
		int i = 0;
		for (int length = buffer.Length; i < length; i++)
		{
			if (i >= rigConstraintEndIdx[num])
			{
				num++;
			}
			buffer[i] *= nativeArray[num] * rigStates[num];
		}
		AnimationStreamHandleUtility.WriteFloats(stream, modulatedConstraintWeights, buffer, useMask: false);
	}
}

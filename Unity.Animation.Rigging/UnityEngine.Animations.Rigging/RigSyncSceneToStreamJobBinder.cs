using Unity.Collections;

namespace UnityEngine.Animations.Rigging;

internal class RigSyncSceneToStreamJobBinder<T> : AnimationJobBinder<RigSyncSceneToStreamJob, T> where T : struct, IAnimationJobData, IRigSyncSceneToStreamData
{
	internal static string[] s_PropertyElementNames = new string[4] { ".x", ".y", ".z", ".w" };

	public override RigSyncSceneToStreamJob Create(Animator animator, ref T data, Component component)
	{
		RigSyncSceneToStreamJob result = default(RigSyncSceneToStreamJob);
		Transform[] syncableTransforms = data.syncableTransforms;
		if (syncableTransforms != null)
		{
			result.transformSyncer = RigSyncSceneToStreamJob.TransformSyncer.Create(syncableTransforms.Length);
			for (int i = 0; i < syncableTransforms.Length; i++)
			{
				result.transformSyncer.BindAt(i, animator, syncableTransforms[i]);
			}
		}
		SyncableProperties[] syncableProperties = data.syncableProperties;
		if (syncableProperties != null)
		{
			int num = syncableProperties.Length;
			int num2 = 0;
			int num3 = 0;
			for (int j = 0; j < syncableProperties.Length; j++)
			{
				num2 += syncableProperties[j].constraints.Length;
				for (int k = 0; k < syncableProperties[j].constraints.Length; k++)
				{
					for (int l = 0; l < syncableProperties[j].constraints[k].properties.Length; l++)
					{
						num3 += syncableProperties[j].constraints[k].properties[l].descriptor.size;
					}
				}
			}
			result.propertySyncer = RigSyncSceneToStreamJob.PropertySyncer.Create(num3);
			result.rigWeightSyncer = RigSyncSceneToStreamJob.PropertySyncer.Create(num);
			result.constraintWeightSyncer = RigSyncSceneToStreamJob.PropertySyncer.Create(num2);
			result.rigStates = new NativeArray<float>(num, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			result.rigConstraintEndIdx = new NativeArray<int>(num, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			result.modulatedConstraintWeights = new NativeArray<PropertyStreamHandle>(num2, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			int num4 = 0;
			int num5 = 0;
			for (int m = 0; m < syncableProperties.Length; m++)
			{
				result.rigWeightSyncer.BindAt(m, animator, syncableProperties[m].rig.component, RigProperties.s_Weight);
				result.rigStates[m] = (data.rigStates[m] ? 1f : 0f);
				ConstraintProperties[] constraints = syncableProperties[m].constraints;
				for (int n = 0; n < constraints.Length; n++)
				{
					ref ConstraintProperties reference = ref constraints[n];
					result.constraintWeightSyncer.BindAt(num4, animator, reference.component, ConstraintProperties.s_Weight);
					result.modulatedConstraintWeights[num4++] = animator.BindCustomStreamProperty(ConstraintsUtils.ConstructCustomPropertyName(reference.component, ConstraintProperties.s_Weight), CustomStreamPropertyType.Float);
					for (int num6 = 0; num6 < reference.properties.Length; num6++)
					{
						ref Property reference2 = ref reference.properties[num6];
						if (reference2.descriptor.size == 1)
						{
							result.propertySyncer.BindAt(num5++, animator, reference.component, reference2.name);
							continue;
						}
						for (int num7 = 0; num7 < reference2.descriptor.size; num7++)
						{
							result.propertySyncer.BindAt(num5++, animator, reference.component, reference2.name + s_PropertyElementNames[num7]);
						}
					}
				}
				result.rigConstraintEndIdx[m] = num4;
			}
		}
		return result;
	}

	public override void Destroy(RigSyncSceneToStreamJob job)
	{
		job.transformSyncer.Dispose();
		job.propertySyncer.Dispose();
		job.rigWeightSyncer.Dispose();
		job.constraintWeightSyncer.Dispose();
		if (job.rigStates.IsCreated)
		{
			job.rigStates.Dispose();
		}
		if (job.rigConstraintEndIdx.IsCreated)
		{
			job.rigConstraintEndIdx.Dispose();
		}
		if (job.modulatedConstraintWeights.IsCreated)
		{
			job.modulatedConstraintWeights.Dispose();
		}
	}

	public override void Update(RigSyncSceneToStreamJob job, ref T data)
	{
		int num = Mathf.Min(job.rigStates.Length, data.rigStates.Length);
		for (int i = 0; i < num; i++)
		{
			job.rigStates[i] = (data.rigStates[i] ? 1f : 0f);
		}
	}
}

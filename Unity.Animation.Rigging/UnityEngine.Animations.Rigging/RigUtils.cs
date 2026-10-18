using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UnityEngine.Animations.Rigging;

internal static class RigUtils
{
	private struct RigSyncSceneToStreamData : IAnimationJobData, IRigSyncSceneToStreamData
	{
		private readonly bool m_IsValid;

		public Transform[] syncableTransforms { get; private set; }

		public SyncableProperties[] syncableProperties { get; private set; }

		public bool[] rigStates { get; set; }

		public RigSyncSceneToStreamData(Transform[] transforms, SyncableProperties[] properties, int rigCount)
		{
			if (transforms != null && transforms.Length != 0)
			{
				int[] array = UniqueTransformIndices(transforms);
				if (array.Length != transforms.Length)
				{
					syncableTransforms = new Transform[array.Length];
					for (int i = 0; i < array.Length; i++)
					{
						syncableTransforms[i] = transforms[array[i]];
					}
				}
				else
				{
					syncableTransforms = transforms;
				}
			}
			else
			{
				syncableTransforms = null;
			}
			syncableProperties = properties;
			rigStates = ((rigCount > 0) ? new bool[rigCount] : null);
			m_IsValid = (syncableTransforms != null && syncableTransforms.Length != 0) || (syncableProperties != null && syncableProperties.Length != 0) || rigStates != null;
		}

		private static int[] UniqueTransformIndices(Transform[] transforms)
		{
			if (transforms == null || transforms.Length == 0)
			{
				return null;
			}
			HashSet<int> hashSet = new HashSet<int>();
			List<int> list = new List<int>(transforms.Length);
			for (int i = 0; i < transforms.Length; i++)
			{
				if (hashSet.Add(transforms[i].GetInstanceID()))
				{
					list.Add(i);
				}
			}
			return list.ToArray();
		}

		bool IAnimationJobData.IsValid()
		{
			return m_IsValid;
		}

		void IAnimationJobData.SetDefaultValues()
		{
			syncableTransforms = null;
			syncableProperties = null;
			rigStates = null;
		}
	}

	internal static readonly Dictionary<Type, PropertyDescriptor> s_SupportedPropertyTypeToDescriptor = new Dictionary<Type, PropertyDescriptor>
	{
		{
			typeof(float),
			new PropertyDescriptor
			{
				size = 1,
				type = PropertyType.Float
			}
		},
		{
			typeof(int),
			new PropertyDescriptor
			{
				size = 1,
				type = PropertyType.Int
			}
		},
		{
			typeof(bool),
			new PropertyDescriptor
			{
				size = 1,
				type = PropertyType.Bool
			}
		},
		{
			typeof(Vector2),
			new PropertyDescriptor
			{
				size = 2,
				type = PropertyType.Float
			}
		},
		{
			typeof(Vector3),
			new PropertyDescriptor
			{
				size = 3,
				type = PropertyType.Float
			}
		},
		{
			typeof(Vector4),
			new PropertyDescriptor
			{
				size = 4,
				type = PropertyType.Float
			}
		},
		{
			typeof(Quaternion),
			new PropertyDescriptor
			{
				size = 4,
				type = PropertyType.Float
			}
		},
		{
			typeof(Vector3Int),
			new PropertyDescriptor
			{
				size = 3,
				type = PropertyType.Int
			}
		},
		{
			typeof(Vector3Bool),
			new PropertyDescriptor
			{
				size = 3,
				type = PropertyType.Bool
			}
		}
	};

	public static IAnimationJobBinder syncSceneToStreamBinder { get; } = new RigSyncSceneToStreamJobBinder<RigSyncSceneToStreamData>();


	public static IRigConstraint[] GetConstraints(Rig rig)
	{
		IRigConstraint[] componentsInChildren = rig.GetComponentsInChildren<IRigConstraint>();
		if (componentsInChildren.Length == 0)
		{
			return null;
		}
		List<IRigConstraint> list = new List<IRigConstraint>(componentsInChildren.Length);
		IRigConstraint[] array = componentsInChildren;
		foreach (IRigConstraint rigConstraint in array)
		{
			if (rigConstraint.IsValid())
			{
				list.Add(rigConstraint);
			}
		}
		if (list.Count != 0)
		{
			return list.ToArray();
		}
		return null;
	}

	private static Transform[] GetSyncableRigTransforms(Animator animator)
	{
		RigTransform[] componentsInChildren = animator.GetComponentsInChildren<RigTransform>();
		if (componentsInChildren.Length == 0)
		{
			return null;
		}
		Transform[] array = new Transform[componentsInChildren.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = componentsInChildren[i].transform;
		}
		return array;
	}

	private static bool ExtractTransformType(Animator animator, FieldInfo field, object data, List<Transform> syncableTransforms)
	{
		bool result = true;
		Type fieldType = field.FieldType;
		if (fieldType == typeof(Transform))
		{
			Transform transform = (Transform)field.GetValue(data);
			if (transform != null && transform.IsChildOf(animator.avatarRoot))
			{
				syncableTransforms.Add(transform);
			}
		}
		else if (fieldType == typeof(Transform[]) || fieldType == typeof(List<Transform>))
		{
			foreach (Transform item in (IEnumerable<Transform>)field.GetValue(data))
			{
				if (item != null && item.IsChildOf(animator.avatarRoot))
				{
					syncableTransforms.Add(item);
				}
			}
		}
		else
		{
			result = false;
		}
		return result;
	}

	private static bool ExtractPropertyType(FieldInfo field, object data, List<Property> syncableProperties, string namePrefix = "")
	{
		if (!s_SupportedPropertyTypeToDescriptor.TryGetValue(field.FieldType, out var value))
		{
			return false;
		}
		syncableProperties.Add(new Property
		{
			name = ConstraintsUtils.ConstructConstraintDataPropertyName(namePrefix + field.Name),
			descriptor = value
		});
		return true;
	}

	private static bool ExtractWeightedTransforms(Animator animator, FieldInfo field, object data, List<Transform> syncableTransforms, List<Property> syncableProperties)
	{
		bool result = true;
		Type fieldType = field.FieldType;
		if (fieldType == typeof(WeightedTransform))
		{
			Transform transform = ((WeightedTransform)field.GetValue(data)).transform;
			if (transform != null && transform.IsChildOf(animator.avatarRoot))
			{
				syncableTransforms.Add(transform);
			}
			syncableProperties.Add(new Property
			{
				name = ConstraintsUtils.ConstructConstraintDataPropertyName(field.Name + ".weight"),
				descriptor = s_SupportedPropertyTypeToDescriptor[typeof(float)]
			});
		}
		else if (fieldType == typeof(WeightedTransformArray))
		{
			IEnumerable<WeightedTransform> obj = (IEnumerable<WeightedTransform>)field.GetValue(data);
			int num = 0;
			foreach (WeightedTransform item in obj)
			{
				if (item.transform != null && item.transform.IsChildOf(animator.avatarRoot))
				{
					syncableTransforms.Add(item.transform);
				}
				syncableProperties.Add(new Property
				{
					name = ConstraintsUtils.ConstructConstraintDataPropertyName(field.Name + ".m_Item" + num + ".weight"),
					descriptor = s_SupportedPropertyTypeToDescriptor[typeof(float)]
				});
				num++;
			}
		}
		else
		{
			result = false;
		}
		return result;
	}

	private static bool ExtractNestedPropertyType(Animator animator, FieldInfo field, object data, List<Transform> syncableTransforms, List<Property> syncableProperties, string namePrefix = "")
	{
		Type fieldType = field.FieldType;
		object value = field.GetValue(data);
		string namePrefix2 = namePrefix + field.Name + ".";
		if (!fieldType.IsValueType || fieldType.IsPrimitive)
		{
			return false;
		}
		foreach (FieldInfo item in from info in fieldType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			where info.GetCustomAttribute<SyncSceneToStreamAttribute>() != null
			select info)
		{
			if (!ExtractTransformType(animator, item, value, syncableTransforms) && !ExtractPropertyType(item, value, syncableProperties, namePrefix2) && !ExtractNestedPropertyType(animator, item, value, syncableTransforms, syncableProperties, namePrefix2))
			{
				throw new NotSupportedException("Field type [" + field.FieldType?.ToString() + "] is not a supported syncable property type.");
			}
		}
		return true;
	}

	private static void ExtractAllSyncableData(Animator animator, IList<IRigLayer> layers, out List<Transform> syncableTransforms, out List<SyncableProperties> syncableProperties)
	{
		syncableTransforms = new List<Transform>();
		syncableProperties = new List<SyncableProperties>(layers.Count);
		Dictionary<Type, FieldInfo[]> dictionary = new Dictionary<Type, FieldInfo[]>();
		foreach (IRigLayer layer in layers)
		{
			if (!layer.IsValid())
			{
				continue;
			}
			IRigConstraint[] constraints = layer.constraints;
			List<ConstraintProperties> list = new List<ConstraintProperties>(constraints.Length);
			IRigConstraint[] array = constraints;
			foreach (IRigConstraint rigConstraint in array)
			{
				IAnimationJobData data = rigConstraint.data;
				Type type = rigConstraint.data.GetType();
				FieldInfo[] array2;
				if (!dictionary.TryGetValue(type, out var value))
				{
					FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					List<FieldInfo> list2 = new List<FieldInfo>(fields.Length);
					array2 = fields;
					foreach (FieldInfo fieldInfo in array2)
					{
						if (fieldInfo.GetCustomAttribute<SyncSceneToStreamAttribute>() != null)
						{
							list2.Add(fieldInfo);
						}
					}
					value = (dictionary[type] = list2.ToArray());
				}
				List<Property> list3 = new List<Property>(value.Length);
				array2 = value;
				foreach (FieldInfo fieldInfo2 in array2)
				{
					if (!ExtractWeightedTransforms(animator, fieldInfo2, data, syncableTransforms, list3) && !ExtractTransformType(animator, fieldInfo2, data, syncableTransforms) && !ExtractPropertyType(fieldInfo2, data, list3) && !ExtractNestedPropertyType(animator, fieldInfo2, data, syncableTransforms, list3))
					{
						throw new NotSupportedException("Field type [" + fieldInfo2.FieldType?.ToString() + "] is not a supported syncable property type.");
					}
				}
				list.Add(new ConstraintProperties
				{
					component = rigConstraint.component,
					properties = list3.ToArray()
				});
			}
			syncableProperties.Add(new SyncableProperties
			{
				rig = new RigProperties
				{
					component = layer.rig
				},
				constraints = list.ToArray()
			});
		}
		Transform[] syncableRigTransforms = GetSyncableRigTransforms(animator);
		if (syncableRigTransforms != null)
		{
			syncableTransforms.AddRange(syncableRigTransforms);
		}
	}

	public static IAnimationJob[] CreateAnimationJobs(Animator animator, IRigConstraint[] constraints)
	{
		if (constraints == null || constraints.Length == 0)
		{
			return null;
		}
		IAnimationJob[] array = new IAnimationJob[constraints.Length];
		for (int i = 0; i < constraints.Length; i++)
		{
			array[i] = constraints[i].CreateJob(animator);
		}
		return array;
	}

	public static void DestroyAnimationJobs(IRigConstraint[] constraints, IAnimationJob[] jobs)
	{
		if (jobs != null && jobs.Length == constraints.Length)
		{
			for (int i = 0; i < constraints.Length; i++)
			{
				constraints[i].DestroyJob(jobs[i]);
			}
		}
	}

	internal static IAnimationJobData CreateSyncSceneToStreamData(Animator animator, IList<IRigLayer> layers)
	{
		ExtractAllSyncableData(animator, layers, out var syncableTransforms, out var syncableProperties);
		return new RigSyncSceneToStreamData(syncableTransforms.ToArray(), syncableProperties.ToArray(), layers.Count);
	}
}

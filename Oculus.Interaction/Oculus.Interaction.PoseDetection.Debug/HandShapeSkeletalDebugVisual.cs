using System.Collections.Generic;
using System.Linq;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.PoseDetection.Debug;

public class HandShapeSkeletalDebugVisual : MonoBehaviour
{
	[SerializeField]
	private ShapeRecognizerActiveState _shapeRecognizerActiveState;

	[SerializeField]
	private GameObject _fingerFeatureDebugVisualPrefab;

	protected virtual void Awake()
	{
	}

	protected virtual void Start()
	{
		foreach (var item in from s in AllFeatureStates()
			group s by s.Item1 into @group
			select new
			{
				HandFinger = @group.Key,
				FingerFeatures = @group.SelectMany(((HandFinger, IReadOnlyList<ShapeRecognizer.FingerFeatureConfig>) item) => item.Item2)
			})
		{
			foreach (ShapeRecognizer.FingerFeatureConfig fingerFeature in item.FingerFeatures)
			{
				GameObject obj = Object.Instantiate(_fingerFeatureDebugVisualPrefab);
				obj.GetComponent<FingerFeatureSkeletalDebugVisual>().Initialize(_shapeRecognizerActiveState.Hand, item.HandFinger, fingerFeature);
				Transform obj2 = obj.transform;
				obj2.parent = base.transform;
				obj2.localScale = Vector3.one;
				obj2.localRotation = Quaternion.identity;
				obj2.localPosition = Vector3.zero;
			}
		}
	}

	private IEnumerable<(HandFinger, IReadOnlyList<ShapeRecognizer.FingerFeatureConfig>)> AllFeatureStates()
	{
		foreach (ShapeRecognizer shape in _shapeRecognizerActiveState.Shapes)
		{
			foreach (var fingerFeatureConfig in shape.GetFingerFeatureConfigs())
			{
				yield return fingerFeatureConfig;
			}
		}
	}
}

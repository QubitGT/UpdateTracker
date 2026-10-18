using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Internal;

namespace UnityEngine.XR.Interaction.Toolkit.Locomotion;

[AddComponentMenu("XR/Locomotion/XR Body Transformer", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.XRBodyTransformer.html")]
[DefaultExecutionOrder(-205)]
public class XRBodyTransformer : MonoBehaviour
{
	private struct OrderedTransformation
	{
		public IXRBodyTransformation transformation;

		public int priority;
	}

	[SerializeField]
	[Tooltip("The XR Origin to transform (will find one if None).")]
	private XROrigin m_XROrigin;

	[SerializeField]
	[RequireInterface(typeof(IXRBodyPositionEvaluator))]
	[Tooltip("Object that determines the position of the user's body. If set to None, this behavior will estimate the position to be the camera position projected onto the XZ plane of the XR Origin.")]
	private Object m_BodyPositionEvaluatorObject;

	private IXRBodyPositionEvaluator m_BodyPositionEvaluator;

	[SerializeField]
	[RequireInterface(typeof(IConstrainedXRBodyManipulator))]
	[Tooltip("Object used to perform movement that is constrained by collision (optional, may be None).")]
	private Object m_ConstrainedBodyManipulatorObject;

	private IConstrainedXRBodyManipulator m_ConstrainedBodyManipulator;

	[SerializeField]
	[Tooltip("When enabled and if a Constrained Manipulator is not already assigned, this behavior will use the XR Origin's Character Controller to perform constrained movement, if one exists on the XR Origin's base GameObject.")]
	private bool m_UseCharacterControllerIfExists = true;

	private bool m_UsingDynamicBodyPositionEvaluator;

	private bool m_UsingDynamicConstrainedBodyManipulator;

	private XRMovableBody m_MovableBody;

	private readonly LinkedList<OrderedTransformation> m_TransformationsQueue = new LinkedList<OrderedTransformation>();

	private readonly ApplyBodyTransformationsEventArgs m_ApplyTransformationsEventArgs = new ApplyBodyTransformationsEventArgs();

	public XROrigin xrOrigin
	{
		get
		{
			return m_XROrigin;
		}
		set
		{
			m_XROrigin = value;
			if (Application.isPlaying)
			{
				InitializeMovableBody();
			}
		}
	}

	public IXRBodyPositionEvaluator bodyPositionEvaluator
	{
		get
		{
			return m_BodyPositionEvaluator;
		}
		set
		{
			m_BodyPositionEvaluator = value;
			if (Application.isPlaying)
			{
				InitializeMovableBody();
			}
		}
	}

	public IConstrainedXRBodyManipulator constrainedBodyManipulator
	{
		get
		{
			return m_ConstrainedBodyManipulator;
		}
		set
		{
			m_ConstrainedBodyManipulator = value;
			if (m_MovableBody != null)
			{
				m_MovableBody.UnlinkConstrainedManipulator();
				if (m_ConstrainedBodyManipulator != null)
				{
					m_MovableBody.LinkConstrainedManipulator(m_ConstrainedBodyManipulator);
				}
			}
		}
	}

	public bool useCharacterControllerIfExists
	{
		get
		{
			return m_UseCharacterControllerIfExists;
		}
		set
		{
			m_UseCharacterControllerIfExists = value;
		}
	}

	public event Action<XRBodyTransformer> beforeApplyTransformations;

	public event Action<ApplyBodyTransformationsEventArgs> afterApplyTransformations;

	protected virtual void Reset()
	{
		m_XROrigin = ComponentLocatorUtility<XROrigin>.FindComponent();
	}

	protected virtual void OnEnable()
	{
		if (m_XROrigin == null && !ComponentLocatorUtility<XROrigin>.TryFindComponent(out m_XROrigin))
		{
			Debug.LogError("XR Body Transformer requires an XR Origin in the scene.", this);
			base.enabled = false;
			return;
		}
		m_BodyPositionEvaluator = m_BodyPositionEvaluatorObject as IXRBodyPositionEvaluator;
		if (m_BodyPositionEvaluator == null)
		{
			m_UsingDynamicBodyPositionEvaluator = true;
			m_BodyPositionEvaluator = ScriptableSingletonCache<UnderCameraBodyPositionEvaluator>.GetInstance(this);
		}
		m_ConstrainedBodyManipulator = m_ConstrainedBodyManipulatorObject as IConstrainedXRBodyManipulator;
		if (m_ConstrainedBodyManipulator == null && m_UseCharacterControllerIfExists && m_XROrigin.Origin.TryGetComponent<CharacterController>(out var _))
		{
			m_UsingDynamicConstrainedBodyManipulator = true;
			m_ConstrainedBodyManipulator = ScriptableSingletonCache<CharacterControllerBodyManipulator>.GetInstance(this);
		}
		InitializeMovableBody();
	}

	protected virtual void OnDisable()
	{
		m_MovableBody?.UnlinkConstrainedManipulator();
		m_MovableBody = null;
		if (m_UsingDynamicBodyPositionEvaluator)
		{
			ScriptableSingletonCache<UnderCameraBodyPositionEvaluator>.ReleaseInstance(this);
			m_UsingDynamicBodyPositionEvaluator = false;
		}
		if (m_UsingDynamicConstrainedBodyManipulator)
		{
			ScriptableSingletonCache<CharacterControllerBodyManipulator>.ReleaseInstance(this);
			m_UsingDynamicConstrainedBodyManipulator = false;
		}
	}

	protected virtual void Update()
	{
		if (m_TransformationsQueue.Count != 0)
		{
			this.beforeApplyTransformations?.Invoke(this);
			while (m_TransformationsQueue.Count > 0)
			{
				m_TransformationsQueue.First.Value.transformation.Apply(m_MovableBody);
				m_TransformationsQueue.RemoveFirst();
			}
			m_ApplyTransformationsEventArgs.bodyTransformer = this;
			this.afterApplyTransformations?.Invoke(m_ApplyTransformationsEventArgs);
		}
	}

	private void InitializeMovableBody()
	{
		m_MovableBody = new XRMovableBody(m_XROrigin, m_BodyPositionEvaluator);
		if (m_ConstrainedBodyManipulator != null)
		{
			m_MovableBody.LinkConstrainedManipulator(m_ConstrainedBodyManipulator);
		}
	}

	public void QueueTransformation(IXRBodyTransformation transformation, int priority = 0)
	{
		OrderedTransformation orderedTransformation = default(OrderedTransformation);
		orderedTransformation.transformation = transformation;
		orderedTransformation.priority = priority;
		OrderedTransformation value = orderedTransformation;
		LinkedListNode<OrderedTransformation> linkedListNode = m_TransformationsQueue.First;
		if (linkedListNode == null || linkedListNode.Value.priority > priority)
		{
			m_TransformationsQueue.AddFirst(value);
			return;
		}
		while (linkedListNode.Next != null && linkedListNode.Next.Value.priority <= priority)
		{
			linkedListNode = linkedListNode.Next;
		}
		m_TransformationsQueue.AddAfter(linkedListNode, value);
	}

	protected virtual void OnDrawGizmosSelected()
	{
		if (m_UseCharacterControllerIfExists && m_ConstrainedBodyManipulator != null && m_ConstrainedBodyManipulator is CharacterControllerBodyManipulator characterControllerBodyManipulator && characterControllerBodyManipulator.characterController != null)
		{
			CharacterController characterController = characterControllerBodyManipulator.characterController;
			Vector3 center = characterController.center + characterController.transform.position + Vector3.up * ((characterController.stepOffset - characterController.skinWidth) * 0.5f);
			float height = characterController.height + characterController.stepOffset + characterController.skinWidth;
			float radius = characterController.radius + characterController.skinWidth;
			GizmoHelpers.DrawCapsule(center, height, radius, Vector3.up, new Color(1f, 0.92f, 0.016f, 0.5f));
		}
	}
}

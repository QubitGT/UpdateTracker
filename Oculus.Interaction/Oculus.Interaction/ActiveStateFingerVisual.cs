using System;
using System.Collections;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public class ActiveStateFingerVisual : MonoBehaviour
{
	[SerializeField]
	[Interface(typeof(IActiveState), new Type[] { })]
	private UnityEngine.Object _activeState;

	private IActiveState ActiveState;

	[SerializeField]
	private HandFingerFlags _fingersMask;

	[SerializeField]
	private MaterialPropertyBlockEditor _handMaterialPropertyBlockEditor;

	[SerializeField]
	private float _glowLerpSpeed = 2f;

	[SerializeField]
	private Color _fingerGlowColor;

	private readonly int[] _handShaderGlowPropertyIds = new int[5]
	{
		Shader.PropertyToID("_ThumbGlowValue"),
		Shader.PropertyToID("_IndexGlowValue"),
		Shader.PropertyToID("_MiddleGlowValue"),
		Shader.PropertyToID("_RingGlowValue"),
		Shader.PropertyToID("_PinkyGlowValue")
	};

	private readonly int _fingerGlowColorPropertyId = Shader.PropertyToID("_FingerGlowColor");

	private bool _prevActive;

	protected bool _started;

	public HandFingerFlags FingersMask
	{
		get
		{
			return _fingersMask;
		}
		set
		{
			_fingersMask = value;
		}
	}

	public float GlowLerpSpeed
	{
		get
		{
			return _glowLerpSpeed;
		}
		set
		{
			_glowLerpSpeed = value;
		}
	}

	public Color FingerGlowColor
	{
		get
		{
			return _fingerGlowColor;
		}
		set
		{
			_fingerGlowColor = value;
		}
	}

	protected virtual void Awake()
	{
		ActiveState = _activeState as IActiveState;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	private void Update()
	{
		if (_prevActive == ActiveState.Active)
		{
			return;
		}
		StopAllCoroutines();
		_prevActive = ActiveState.Active;
		_handMaterialPropertyBlockEditor.MaterialPropertyBlock.SetColor(_fingerGlowColorPropertyId, _fingerGlowColor);
		float targetGlow = (ActiveState.Active ? 1f : 0f);
		for (int i = 0; i < 5; i++)
		{
			if (_fingersMask.HasFlag((HandFingerFlags)(1 << i)))
			{
				StartCoroutine(UpdateGlowValue(i, targetGlow));
			}
		}
		_handMaterialPropertyBlockEditor.UpdateMaterialPropertyBlock();
	}

	private IEnumerator UpdateGlowValue(int fingerIndex, float targetGlow)
	{
		float startGlow = _handMaterialPropertyBlockEditor.MaterialPropertyBlock.GetFloat(_handShaderGlowPropertyIds[fingerIndex]);
		float startTime = Time.time;
		float currentGlow;
		do
		{
			currentGlow = Mathf.MoveTowards(startGlow, targetGlow, _glowLerpSpeed * (Time.time - startTime));
			_handMaterialPropertyBlockEditor.MaterialPropertyBlock.SetFloat(_handShaderGlowPropertyIds[fingerIndex], currentGlow);
			yield return null;
		}
		while (currentGlow != targetGlow);
	}

	public void InjectAllActiveStateFingerVisual(IActiveState activeState, MaterialPropertyBlockEditor handMaterialPropertyBlockEditor)
	{
		InjectActiveState(activeState);
		InjectHandMaterialPropertyBlockEditor(handMaterialPropertyBlockEditor);
	}

	public void InjectActiveState(IActiveState activeState)
	{
		ActiveState = activeState;
		_activeState = activeState as UnityEngine.Object;
	}

	public void InjectHandMaterialPropertyBlockEditor(MaterialPropertyBlockEditor handMaterialPropertyBlockEditor)
	{
		_handMaterialPropertyBlockEditor = handMaterialPropertyBlockEditor;
	}
}

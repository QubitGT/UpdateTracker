using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Meta.XR.Util;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

[DisallowMultipleComponent]
[HelpURL("https://developer.oculus.com/documentation/unity/VK-unity-IntegratePrefab/")]
[Feature(Feature.VirtualKeyboard)]
public class OVRVirtualKeyboard : MonoBehaviour
{
	public enum KeyboardPosition
	{
		Far = 0,
		Near = 1,
		[Obsolete]
		Direct = 1,
		Custom = 2
	}

	public interface ITextHandler
	{
		Action<string> OnTextChanged { get; set; }

		string Text { get; }

		bool SubmitOnEnter { get; }

		bool IsFocused { get; }

		void Submit();

		void AppendText(string s);

		void ApplyBackspace();

		void MoveTextEnd();
	}

	public abstract class AbstractTextHandler : MonoBehaviour, ITextHandler
	{
		public abstract Action<string> OnTextChanged { get; set; }

		public abstract string Text { get; }

		public abstract bool SubmitOnEnter { get; }

		public abstract bool IsFocused { get; }

		public abstract void Submit();

		public abstract void AppendText(string s);

		public abstract void ApplyBackspace();

		public abstract void MoveTextEnd();
	}

	private class TextHandlerScope : ITextHandler, IDisposable
	{
		private readonly ITextHandler _textHandler;

		private readonly Action<string> _textChangeHandler;

		public Action<string> OnTextChanged
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public string Text => _textHandler.Text;

		public bool SubmitOnEnter => _textHandler.SubmitOnEnter;

		public bool IsFocused => _textHandler.IsFocused;

		public TextHandlerScope(ITextHandler textHandler, Action<string> textChangeHandler)
		{
			_textHandler = textHandler;
			_textChangeHandler = textChangeHandler;
			ITextHandler textHandler2 = _textHandler;
			textHandler2.OnTextChanged = (Action<string>)Delegate.Remove(textHandler2.OnTextChanged, _textChangeHandler);
		}

		public void Dispose()
		{
			ITextHandler textHandler = _textHandler;
			textHandler.OnTextChanged = (Action<string>)Delegate.Combine(textHandler.OnTextChanged, _textChangeHandler);
		}

		public void Submit()
		{
			_textHandler?.Submit();
		}

		public void AppendText(string s)
		{
			if (_textHandler != null)
			{
				_textHandler.AppendText(s);
				if (_textHandler.IsFocused)
				{
					_textHandler.MoveTextEnd();
				}
			}
		}

		public void ApplyBackspace()
		{
			if (_textHandler != null)
			{
				_textHandler.ApplyBackspace();
				if (_textHandler.IsFocused)
				{
					_textHandler.MoveTextEnd();
				}
			}
		}

		public void MoveTextEnd()
		{
			_textHandler?.MoveTextEnd();
		}
	}

	public class WaitUntilKeyboardVisible : CustomYieldInstruction
	{
		private readonly OVRVirtualKeyboard _keyboard;

		public override bool keepWaiting
		{
			get
			{
				if (_keyboard.modelAvailable_)
				{
					return !_keyboard.keyboardVisible_;
				}
				return true;
			}
		}

		public WaitUntilKeyboardVisible(OVRVirtualKeyboard keyboard)
		{
			_keyboard = keyboard;
		}
	}

	public class InteractorRootTransformOverride
	{
		private struct InteractorRootOverrideData
		{
			public Transform root;

			public OVRPose originalPose;

			public OVRPose targetPose;
		}

		private Queue<InteractorRootOverrideData> applyQueue = new Queue<InteractorRootOverrideData>();

		private Queue<InteractorRootOverrideData> revertQueue = new Queue<InteractorRootOverrideData>();

		public void Enqueue(Transform interactorRootTransform, OVRPlugin.Posef interactorRootPose)
		{
			if (interactorRootTransform == null)
			{
				throw new Exception("Transform is undefined");
			}
			applyQueue.Enqueue(new InteractorRootOverrideData
			{
				root = interactorRootTransform,
				originalPose = interactorRootTransform.ToOVRPose(),
				targetPose = interactorRootPose.ToOVRPose()
			});
		}

		public void LateApply(MonoBehaviour coroutineRunner)
		{
			while (applyQueue.Count > 0)
			{
				InteractorRootOverrideData interactorRootOverrideData = applyQueue.Dequeue();
				OVRPose targetPose = interactorRootOverrideData.root.ToOVRPose();
				if (ApplyOverride(interactorRootOverrideData))
				{
					interactorRootOverrideData.originalPose = interactorRootOverrideData.root.ToOVRPose();
					interactorRootOverrideData.targetPose = targetPose;
					revertQueue.Enqueue(interactorRootOverrideData);
				}
			}
			if (revertQueue.Count > 0 && coroutineRunner != null)
			{
				coroutineRunner.StartCoroutine(RevertInteractorOverrides());
			}
		}

		public void Reset()
		{
			while (revertQueue.Count > 0)
			{
				ApplyOverride(revertQueue.Dequeue());
			}
		}

		private IEnumerator RevertInteractorOverrides()
		{
			yield return new WaitForEndOfFrame();
			Reset();
		}

		private static bool ApplyOverride(InteractorRootOverrideData interactorOverride)
		{
			if (interactorOverride.root.position != interactorOverride.originalPose.position || interactorOverride.root.rotation != interactorOverride.originalPose.orientation)
			{
				return false;
			}
			interactorOverride.root.position = interactorOverride.targetPose.position;
			interactorOverride.root.rotation = interactorOverride.targetPose.orientation;
			return true;
		}
	}

	public enum InputSource
	{
		ControllerLeft,
		ControllerRight,
		HandLeft,
		HandRight
	}

	private interface IInputSource : IDisposable
	{
		void Update();
	}

	private abstract class BaseInputSource : IInputSource, IDisposable
	{
		protected readonly bool _operatingWithoutOVRCameraRig;

		private readonly OVRCameraRig _rig;

		private bool _disposed;

		protected BaseInputSource()
		{
			_rig = UnityEngine.Object.FindAnyObjectByType<OVRCameraRig>();
			if (!(_rig == null))
			{
				_rig.UpdatedAnchors += OnUpdatedAnchors;
				_operatingWithoutOVRCameraRig = false;
			}
		}

		private void OnUpdatedAnchors(OVRCameraRig obj)
		{
			if (_disposed)
			{
				throw new Exception("Virtual Keyboard Input Source Disposed");
			}
			UpdateInput();
		}

		public void Update()
		{
			if (_operatingWithoutOVRCameraRig && !_disposed)
			{
				UpdateInput();
			}
		}

		protected abstract void UpdateInput();

		public void Dispose()
		{
			_disposed = true;
			if (_rig != null)
			{
				_rig.UpdatedAnchors -= OnUpdatedAnchors;
			}
		}
	}

	private class ControllerInputSource : BaseInputSource
	{
		private readonly Transform _rootTransform;

		private readonly Transform _directTransform;

		private readonly InputSource _inputSource;

		private readonly OVRInput.Controller _controllerType;

		private readonly OVRVirtualKeyboard _keyboard;

		private int _lastFrameCount;

		private bool TriggerIsPressed => OVRInput.Get((_controllerType == OVRInput.Controller.LTouch) ? (OVRInput.RawButton.X | OVRInput.RawButton.LIndexTrigger) : (OVRInput.RawButton.A | OVRInput.RawButton.RIndexTrigger));

		public ControllerInputSource(OVRVirtualKeyboard keyboard, InputSource inputSource, OVRInput.Controller controllerType, Transform rootTransform, Transform directTransform)
		{
			_keyboard = keyboard;
			_inputSource = inputSource;
			_controllerType = controllerType;
			_rootTransform = rootTransform;
			_directTransform = directTransform;
		}

		protected override void UpdateInput()
		{
			if (_keyboard.InputEnabled && OVRInput.GetControllerPositionValid(_controllerType) && (bool)_rootTransform && Time.frameCount != _lastFrameCount)
			{
				_lastFrameCount = Time.frameCount;
				if (_keyboard.controllerRayInteraction)
				{
					_keyboard.SendVirtualKeyboardRayInput(_directTransform, _inputSource, TriggerIsPressed);
				}
				if (_keyboard.controllerDirectInteraction)
				{
					_keyboard.SendVirtualKeyboardDirectInput(_directTransform.position, _inputSource, TriggerIsPressed, _rootTransform);
				}
			}
		}
	}

	private class HandInputSource : BaseInputSource
	{
		private readonly OVRHand _hand;

		private readonly InputSource _inputSource;

		private readonly OVRVirtualKeyboard _keyboard;

		private readonly OVRSkeleton _skeleton;

		private int _lastFrameCount;

		public HandInputSource(OVRVirtualKeyboard keyboard, InputSource inputSource, OVRHand hand)
		{
			if (!keyboard)
			{
				throw new ArgumentNullException("keyboard");
			}
			_keyboard = keyboard;
			if (!hand)
			{
				throw new ArgumentNullException("hand");
			}
			_hand = hand;
			_skeleton = _hand.GetComponent<OVRSkeleton>();
			if (!_skeleton && _keyboard.handDirectInteraction)
			{
				Debug.LogWarning("Hand Direct Interaction requires an OVRSkeleton on the OVRHand");
			}
			_inputSource = inputSource;
		}

		protected override void UpdateInput()
		{
			if (!_keyboard.InputEnabled || !_hand || Time.frameCount == _lastFrameCount)
			{
				return;
			}
			_lastFrameCount = Time.frameCount;
			if (_keyboard.handRayInteraction && _hand.IsPointerPoseValid)
			{
				_keyboard.SendVirtualKeyboardRayInput(_hand.PointerPose, _inputSource, _hand.GetFingerIsPinching(OVRHand.HandFinger.Index));
			}
			if (_keyboard.handDirectInteraction && (bool)_skeleton && _skeleton.IsDataValid)
			{
				OVRSkeleton.BoneId indexTipJoint = ((_skeleton.GetSkeletonType() == OVRSkeleton.SkeletonType.XRHandLeft || _skeleton.GetSkeletonType() == OVRSkeleton.SkeletonType.XRHandRight) ? OVRSkeleton.BoneId.Hand_Middle2 : OVRSkeleton.BoneId.Hand_IndexTip);
				OVRSkeleton.BoneId wristRootJoint = ((_skeleton.GetSkeletonType() == OVRSkeleton.SkeletonType.XRHandLeft || _skeleton.GetSkeletonType() == OVRSkeleton.SkeletonType.XRHandRight) ? OVRSkeleton.BoneId.Hand_ForearmStub : OVRSkeleton.BoneId.Hand_Start);
				OVRBone oVRBone = _skeleton.Bones.First((OVRBone b) => b.Id == indexTipJoint);
				OVRBone oVRBone2 = _skeleton.Bones.First((OVRBone b) => b.Id == wristRootJoint);
				_keyboard.SendVirtualKeyboardDirectInput(oVRBone.Transform.position, _inputSource, _hand.GetFingerIsPinching(OVRHand.HandFinger.Index), oVRBone2.Transform);
			}
		}
	}

	private class KeyboardEventListener : OVRManager.EventListener
	{
		private readonly OVRVirtualKeyboard keyboard_;

		public KeyboardEventListener(OVRVirtualKeyboard keyboard)
		{
			keyboard_ = keyboard;
		}

		public void OnEvent(OVRPlugin.EventDataBuffer eventDataBuffer)
		{
			switch (eventDataBuffer.EventType)
			{
			case OVRPlugin.EventType.VirtualKeyboardCommitText:
				if (keyboard_.CommitText != null || keyboard_.CommitText != null)
				{
					string text = Encoding.UTF8.GetString(eventDataBuffer.EventData).Replace("\0", "");
					keyboard_.CommitText?.Invoke(text);
					keyboard_.CommitText?.Invoke(text);
				}
				break;
			case OVRPlugin.EventType.VirtualKeyboardBackspace:
				keyboard_.Backspace?.Invoke();
				keyboard_.Backspace?.Invoke();
				break;
			case OVRPlugin.EventType.VirtualKeyboardEnter:
				keyboard_.Enter?.Invoke();
				keyboard_.Enter?.Invoke();
				break;
			case OVRPlugin.EventType.VirtualKeyboardShown:
				keyboard_.KeyboardShown?.Invoke();
				keyboard_.KeyboardShown?.Invoke();
				break;
			case OVRPlugin.EventType.VirtualKeyboardHidden:
				keyboard_.KeyboardHidden?.Invoke();
				keyboard_.KeyboardHidden?.Invoke();
				break;
			}
		}
	}

	private struct VirtualKeyboardTextureInfo
	{
		public IntPtr buffer;

		public uint bufferLength;

		public Texture2D texture;

		public bool hasTexture;

		public List<Material> materials;
	}

	[Serializable]
	public class CommitTextUnityEvent : UnityEvent<string>
	{
	}

	private static readonly string _defaultShaderName = "Unlit/Color";

	private static readonly string _defaultAlphaBlendShaderName = "Unlit/Transparent";

	private static OVRVirtualKeyboard singleton_;

	[SerializeField]
	private KeyboardPosition InitialPosition = KeyboardPosition.Custom;

	[SerializeField]
	[FormerlySerializedAs("TextCommitField")]
	[Obsolete]
	[HideInInspector]
	private InputField textCommitField;

	[SerializeField]
	private AbstractTextHandler textHandler;

	private ITextHandler _runtimeTextHandler;

	[Header("Controller Input")]
	[FormerlySerializedAs("leftControllerInputTransform")]
	public Transform leftControllerRootTransform;

	public Transform leftControllerDirectTransform;

	[FormerlySerializedAs("rightControllerInputTransform")]
	public Transform rightControllerRootTransform;

	public Transform rightControllerDirectTransform;

	public bool controllerDirectInteraction = true;

	public bool controllerRayInteraction = true;

	public OVRPhysicsRaycaster controllerRaycaster;

	[Header("Hand Input")]
	public OVRHand handLeft;

	public OVRHand handRight;

	public bool handDirectInteraction = true;

	public bool handRayInteraction = true;

	public OVRPhysicsRaycaster handRaycaster;

	[Header("Graphics")]
	public Shader keyboardModelShader;

	public Shader keyboardModelAlphaBlendShader;

	[NonSerialized]
	public bool InputEnabled = true;

	[Header("Event Handling")]
	public CommitTextUnityEvent CommitTextEvent;

	public UnityEvent BackspaceEvent;

	public UnityEvent EnterEvent;

	public UnityEvent KeyboardShownEvent;

	public UnityEvent KeyboardHiddenEvent;

	private bool isKeyboardCreated_;

	private ulong keyboardSpace_;

	private Dictionary<ulong, VirtualKeyboardTextureInfo> virtualKeyboardTextures_ = new Dictionary<ulong, VirtualKeyboardTextureInfo>();

	private OVRGLTFScene virtualKeyboardScene_;

	private ulong virtualKeyboardModelKey_;

	private bool modelInitialized_;

	private bool modelAvailable_;

	private bool keyboardVisible_;

	private InteractorRootTransformOverride _interactorRootTransformOverride = new InteractorRootTransformOverride();

	private List<IInputSource> _inputSources;

	private KeyboardEventListener keyboardEventListener_;

	private Coroutine gltfModelCoroutine_;

	private OVRGLTFLoader _gltfLoader;

	private int _animationStateCount;

	private int _animationStateBufferLength;

	private IntPtr _animationStateBuffer;

	public Collider Collider { get; private set; }

	[Obsolete("TextCommitField has been replaced with TextHandler for more flexibility.")]
	public InputField TextCommitField
	{
		get
		{
			Debug.LogWarning("Migrate to TextHandler for better performance.");
			return (textHandler as OVRVirtualKeyboardInputFieldTextHandler)?.InputField;
		}
		set
		{
			Debug.LogWarning("Migrate to TextHandler for better performance.");
			if (TextHandler is OVRVirtualKeyboardInputFieldTextHandler oVRVirtualKeyboardInputFieldTextHandler)
			{
				oVRVirtualKeyboardInputFieldTextHandler.InputField = value;
			}
		}
	}

	public ITextHandler TextHandler
	{
		get
		{
			return _runtimeTextHandler;
		}
		set
		{
			if (_runtimeTextHandler != value)
			{
				if (_runtimeTextHandler != null)
				{
					ITextHandler runtimeTextHandler = _runtimeTextHandler;
					runtimeTextHandler.OnTextChanged = (Action<string>)Delegate.Remove(runtimeTextHandler.OnTextChanged, new Action<string>(OnTextHandlerChange));
				}
				_runtimeTextHandler = value;
				if (_runtimeTextHandler != null)
				{
					ITextHandler runtimeTextHandler2 = _runtimeTextHandler;
					runtimeTextHandler2.OnTextChanged = (Action<string>)Delegate.Combine(runtimeTextHandler2.OnTextChanged, new Action<string>(OnTextHandlerChange));
					ChangeTextContextInternal(_runtimeTextHandler.Text);
				}
			}
		}
	}

	[Obsolete("Use CommitTextEvent", false)]
	public event Action<string> CommitText = new CommitTextUnityEvent();

	[Obsolete("Use BackspaceEvent", false)]
	public event Action Backspace = new UnityEvent();

	[Obsolete("Use EnterEvent", false)]
	public event Action Enter = new UnityEvent();

	[Obsolete("Use KeyboardShownEvent", false)]
	public event Action KeyboardShown = new UnityEvent();

	[Obsolete("Use KeyboardHiddenEvent", false)]
	public event Action KeyboardHidden = new UnityEvent();

	private void Awake()
	{
		if (keyboardModelShader == null)
		{
			Debug.LogWarning("keyboardModelShader not specified; falling back to " + _defaultShaderName);
			keyboardModelShader = Shader.Find(_defaultShaderName);
		}
		if (keyboardModelAlphaBlendShader == null)
		{
			Debug.LogWarning("keyboardModelAlphaBlendShader not specified; falling back to " + _defaultAlphaBlendShaderName);
			keyboardModelAlphaBlendShader = Shader.Find(_defaultAlphaBlendShaderName);
		}
		if (singleton_ != null)
		{
			UnityEngine.Object.Destroy(this);
			throw new Exception("OVRVirtualKeyboard only supports a single instance");
		}
		if (leftControllerDirectTransform == null && leftControllerRootTransform != null)
		{
			if (controllerDirectInteraction)
			{
				Debug.LogWarning("Missing left controller direct transform for virtual keyboard input; falling back to the root!");
			}
			leftControllerDirectTransform = leftControllerRootTransform;
		}
		if (rightControllerDirectTransform == null && rightControllerRootTransform != null)
		{
			if (controllerDirectInteraction)
			{
				Debug.LogWarning("Missing right controller direct transform for virtual keyboard input; falling back to the root!");
			}
			rightControllerDirectTransform = rightControllerRootTransform;
		}
		singleton_ = this;
		if ((bool)OVRManager.instance)
		{
			keyboardEventListener_ = new KeyboardEventListener(this);
			OVRManager.instance.RegisterEventListener(keyboardEventListener_);
		}
		TextHandler = textHandler;
		CommitText.AddListener(OnCommitText);
		Backspace.AddListener(OnBackspace);
		Enter.AddListener(OnEnter);
		KeyboardShown.AddListener(OnKeyboardShown);
		KeyboardHidden.AddListener(OnKeyboardHidden);
	}

	private void OnDestroy()
	{
		if (!OVRPlugin.initialized)
		{
			return;
		}
		CommitText.RemoveListener(OnCommitText);
		Backspace.RemoveListener(OnBackspace);
		Enter.RemoveListener(OnEnter);
		KeyboardShown.RemoveListener(OnKeyboardShown);
		KeyboardHidden.RemoveListener(OnKeyboardHidden);
		foreach (VirtualKeyboardTextureInfo value in virtualKeyboardTextures_.Values)
		{
			if (value.buffer != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(value.buffer);
			}
		}
		virtualKeyboardTextures_.Clear();
		TextHandler = null;
		if (singleton_ == this)
		{
			if (OVRManager.instance != null)
			{
				OVRManager.instance.DeregisterEventListener(keyboardEventListener_);
			}
			singleton_ = null;
		}
		keyboardEventListener_ = null;
		DestroyKeyboard();
	}

	private void OnEnable()
	{
		ShowKeyboard();
	}

	private void OnDisable()
	{
		if (OVRPlugin.initialized)
		{
			HideKeyboard();
		}
	}

	private void Reset()
	{
		keyboardModelShader = Shader.Find(_defaultShaderName);
		keyboardModelAlphaBlendShader = Shader.Find(_defaultAlphaBlendShaderName);
	}

	public void UseSuggestedLocation(KeyboardPosition position)
	{
		OVRPlugin.VirtualKeyboardLocationInfo locationInfo = default(OVRPlugin.VirtualKeyboardLocationInfo);
		switch (position)
		{
		case KeyboardPosition.Near:
			locationInfo.locationType = OVRPlugin.VirtualKeyboardLocationType.Direct;
			break;
		case KeyboardPosition.Far:
			locationInfo.locationType = OVRPlugin.VirtualKeyboardLocationType.Far;
			break;
		case KeyboardPosition.Custom:
			locationInfo = ComputeLocation(base.transform);
			break;
		default:
			Debug.LogError("Unknown KeyboardInputMode: " + position);
			return;
		}
		InitialPosition = position;
		if (keyboardSpace_ != 0L)
		{
			locationInfo.trackingOriginType = OVRPlugin.GetTrackingOriginType();
			OVRPlugin.Result result = OVRPlugin.SuggestVirtualKeyboardLocation(locationInfo);
			if (result != 0)
			{
				Debug.LogError("SuggestVirtualKeyboardLocation failed: " + result);
				return;
			}
			base.transform.hasChanged = false;
			SyncKeyboardLocation();
		}
	}

	public void SendVirtualKeyboardRayInput(Transform inputTransform, InputSource source, bool isPressed, bool useRaycastMask = true)
	{
		OVRPlugin.VirtualKeyboardInputSource inputSource = source switch
		{
			InputSource.ControllerLeft => OVRPlugin.VirtualKeyboardInputSource.ControllerRayLeft, 
			InputSource.ControllerRight => OVRPlugin.VirtualKeyboardInputSource.ControllerRayRight, 
			InputSource.HandLeft => OVRPlugin.VirtualKeyboardInputSource.HandRayLeft, 
			InputSource.HandRight => OVRPlugin.VirtualKeyboardInputSource.HandRayRight, 
			_ => throw new Exception("Unknown input source: " + source), 
		};
		OVRPhysicsRaycaster oVRPhysicsRaycaster = ((source == InputSource.ControllerLeft || source == InputSource.ControllerRight) ? controllerRaycaster : handRaycaster);
		if ((bool)oVRPhysicsRaycaster)
		{
			OVRPointerEventData eventData = new OVRPointerEventData(EventSystem.current)
			{
				worldSpaceRay = new Ray(inputTransform.position, inputTransform.forward)
			};
			List<RaycastResult> list = new List<RaycastResult>();
			oVRPhysicsRaycaster.Raycast(eventData, list);
			if (list.Count <= 0 || list[0].gameObject != Collider.gameObject)
			{
				return;
			}
		}
		SendVirtualKeyboardInput(inputSource, inputTransform.ToOVRPose(), isPressed);
	}

	public void SendVirtualKeyboardDirectInput(Vector3 position, InputSource source, bool isPressed, Transform interactorRootTransform = null)
	{
		SendVirtualKeyboardInput(source switch
		{
			InputSource.ControllerLeft => OVRPlugin.VirtualKeyboardInputSource.ControllerDirectLeft, 
			InputSource.ControllerRight => OVRPlugin.VirtualKeyboardInputSource.ControllerDirectRight, 
			InputSource.HandLeft => OVRPlugin.VirtualKeyboardInputSource.HandDirectIndexTipLeft, 
			InputSource.HandRight => OVRPlugin.VirtualKeyboardInputSource.HandDirectIndexTipRight, 
			_ => throw new Exception("Unknown input source: " + source), 
		}, new OVRPose
		{
			position = position
		}, isPressed, interactorRootTransform);
	}

	public void ChangeTextContext(string textContext)
	{
		if (TextHandler != null && TextHandler.Text != textContext)
		{
			Debug.LogWarning("TextHandler text out of sync with Keyboard text context");
		}
		ChangeTextContextInternal(textContext);
	}

	private void LoadRuntimeVirtualKeyboardMesh()
	{
		modelAvailable_ = false;
		gltfModelCoroutine_ = StartCoroutine(InitializeGlTFModel());
	}

	private IEnumerator InitializeGlTFModel()
	{
		Func<MemoryStream> deferredStream = delegate
		{
			Debug.Log("LoadRuntimeVirtualKeyboardMesh");
			string text = OVRPlugin.GetRenderModelPaths()?.FirstOrDefault((string p) => p.Equals("/model_fb/virtual_keyboard") || p.Equals("/model_meta/keyboard/virtual"));
			if (string.IsNullOrEmpty(text))
			{
				Debug.LogError("Failed to find keyboard model.  Check Render Model support.");
				return (MemoryStream)null;
			}
			OVRPlugin.RenderModelProperties modelProperties = default(OVRPlugin.RenderModelProperties);
			if (!OVRPlugin.GetRenderModelProperties(text, ref modelProperties))
			{
				Debug.LogError("Failed to find keyboard model properties.  Check Render Model support.");
				return (MemoryStream)null;
			}
			if (modelProperties.ModelKey == 0L)
			{
				Debug.LogError("Failed to find keyboard model key.  Check Render Model support.");
				return (MemoryStream)null;
			}
			virtualKeyboardModelKey_ = modelProperties.ModelKey;
			return new MemoryStream(OVRPlugin.LoadRenderModel(modelProperties.ModelKey));
		};
		_gltfLoader = new OVRGLTFLoader(deferredStream);
		_gltfLoader.textureUriHandler = delegate(string rawUri, Material mat)
		{
			Uri uri = new Uri(rawUri);
			if (!uri.Scheme.Equals("metaVirtualKeyboard", StringComparison.OrdinalIgnoreCase) || uri.Host != "texture")
			{
				return (Texture2D)null;
			}
			ulong key = ulong.Parse(uri.LocalPath.Substring(1));
			if (!virtualKeyboardTextures_.TryGetValue(key, out var value))
			{
				value.materials = new List<Material>();
			}
			value.materials.Add(mat);
			virtualKeyboardTextures_[key] = value;
			return (Texture2D)null;
		};
		_gltfLoader.SetModelShader(keyboardModelShader);
		_gltfLoader.SetModelAlphaBlendShader(keyboardModelAlphaBlendShader);
		IEnumerator loadGlbCoroutine = _gltfLoader.LoadGLBCoroutine(supportAnimation: true);
		while (loadGlbCoroutine.MoveNext())
		{
			yield return loadGlbCoroutine.Current;
		}
		virtualKeyboardScene_ = _gltfLoader.scene;
		_gltfLoader = null;
		gltfModelCoroutine_ = null;
		modelAvailable_ = virtualKeyboardScene_.root != null;
		if (modelAvailable_)
		{
			virtualKeyboardScene_.root.transform.SetParent(base.transform, worldPositionStays: false);
			virtualKeyboardScene_.root.gameObject.name = "OVRVirtualKeyboardModel";
			ApplyHideFlags(virtualKeyboardScene_.root.transform);
			SetKeyboardVisibility(visible: true);
			UseSuggestedLocation(InitialPosition);
			UpdateAnimationState();
			PopulateCollision();
		}
	}

	private static void ApplyHideFlags(Transform t)
	{
		t.gameObject.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
		for (int i = 0; i < t.childCount; i++)
		{
			ApplyHideFlags(t.GetChild(i));
		}
	}

	private void PopulateCollision()
	{
		if (!modelAvailable_)
		{
			throw new Exception("Keyboard Model Unavailable");
		}
		MeshFilter meshFilter = (from mesh in virtualKeyboardScene_.root.GetComponentsInChildren<MeshFilter>()
			where mesh.gameObject.name == "collision"
			select mesh).FirstOrDefault();
		if (meshFilter != null)
		{
			MeshCollider meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();
			meshCollider.convex = true;
			Collider = meshCollider;
			MeshRenderer component = meshFilter.gameObject.GetComponent<MeshRenderer>();
			if (component != null)
			{
				component.enabled = false;
			}
		}
	}

	private void ShowKeyboard()
	{
		if (!isKeyboardCreated_)
		{
			OVRPlugin.Result result = OVRPlugin.CreateVirtualKeyboard(default(OVRPlugin.VirtualKeyboardCreateInfo));
			if (result != 0)
			{
				Debug.LogError("Create failed: '" + result.ToString() + "'. Check for Virtual Keyboard Support.");
				return;
			}
			isKeyboardCreated_ = true;
		}
		if (!modelInitialized_)
		{
			modelInitialized_ = true;
			LoadRuntimeVirtualKeyboardMesh();
		}
		else
		{
			SetKeyboardVisibility(visible: true);
		}
		if (TextHandler != null)
		{
			ChangeTextContextInternal(TextHandler.Text);
		}
	}

	private void SetKeyboardVisibility(bool visible)
	{
		if (modelAvailable_)
		{
			OVRPlugin.VirtualKeyboardModelVisibility visibility = default(OVRPlugin.VirtualKeyboardModelVisibility);
			visibility.Visible = visible;
			OVRPlugin.Result result = OVRPlugin.SetVirtualKeyboardModelVisibility(ref visibility);
			if (result != 0)
			{
				Debug.LogError("SetVirtualKeyboardModelVisibility failed: " + result);
			}
		}
	}

	private void HideKeyboard()
	{
		if (modelInitialized_ && !modelAvailable_)
		{
			UnloadModel();
		}
		SetKeyboardVisibility(visible: false);
	}

	private void UnloadModel()
	{
		if (gltfModelCoroutine_ != null)
		{
			StopCoroutine(gltfModelCoroutine_);
			gltfModelCoroutine_ = null;
		}
		if (_gltfLoader != null && _gltfLoader.scene.root != null)
		{
			UnityEngine.Object.Destroy(_gltfLoader.scene.root);
			_gltfLoader = null;
		}
		if (modelAvailable_)
		{
			UnityEngine.Object.Destroy(virtualKeyboardScene_.root);
			modelAvailable_ = false;
		}
		modelInitialized_ = false;
	}

	private void DestroyKeyboard()
	{
		UnloadModel();
		InputEnabled = false;
		if (isKeyboardCreated_)
		{
			if (OVRPlugin.DestroyVirtualKeyboard() != 0)
			{
				Debug.LogError("Destroy failed");
			}
			else
			{
				Debug.Log("Destroy success");
			}
			isKeyboardCreated_ = false;
		}
		if (_inputSources != null)
		{
			foreach (IInputSource inputSource in _inputSources)
			{
				inputSource.Dispose();
			}
		}
		_inputSources = null;
	}

	private float MaxElement(Vector3 vec)
	{
		return Mathf.Max(Mathf.Max(vec.x, vec.y), vec.z);
	}

	private OVRPlugin.VirtualKeyboardLocationInfo ComputeLocation(Transform transform)
	{
		OVRPlugin.VirtualKeyboardLocationInfo result = default(OVRPlugin.VirtualKeyboardLocationInfo);
		result.locationType = OVRPlugin.VirtualKeyboardLocationType.Custom;
		result.pose.Position = transform.position.ToFlippedZVector3f();
		result.pose.Orientation = transform.rotation.ToFlippedZQuatf();
		result.scale = MaxElement(transform.localScale);
		return result;
	}

	private void Update()
	{
		if (OVRPlugin.initialized)
		{
			if (modelAvailable_)
			{
				UpdateInputs();
			}
			if (isKeyboardCreated_)
			{
				SyncKeyboardLocation();
			}
			if (modelAvailable_)
			{
				UpdateAnimationState();
			}
		}
	}

	private void LateUpdate()
	{
		_interactorRootTransformOverride.LateApply(this);
	}

	private void SendVirtualKeyboardInput(OVRPlugin.VirtualKeyboardInputSource inputSource, OVRPose pose, bool isPressed, Transform interactorRootTransform = null)
	{
		OVRPlugin.VirtualKeyboardInputInfo inputInfo = default(OVRPlugin.VirtualKeyboardInputInfo);
		inputInfo.inputTrackingOriginType = (OVRPlugin.TrackingOrigin)OVRManager.instance.trackingOriginType;
		inputInfo.inputSource = inputSource;
		inputInfo.inputPose = pose.ToPosef();
		inputInfo.inputState = (OVRPlugin.VirtualKeyboardInputStateFlags)(isPressed ? 1 : 0);
		OVRPlugin.Posef interactorRootPose = ((!(interactorRootTransform != null)) ? pose.ToPosef() : interactorRootTransform.ToOVRPose().ToPosef());
		if (OVRPlugin.SendVirtualKeyboardInput(inputInfo, ref interactorRootPose) == OVRPlugin.Result.Success && interactorRootTransform != null)
		{
			_interactorRootTransformOverride.Enqueue(interactorRootTransform, interactorRootPose);
		}
	}

	private void UpdateInputs()
	{
		if (!InputEnabled || !modelAvailable_)
		{
			return;
		}
		if (_inputSources == null)
		{
			_inputSources = new List<IInputSource>();
			if ((bool)leftControllerRootTransform)
			{
				_inputSources.Add(new ControllerInputSource(this, InputSource.ControllerLeft, OVRInput.Controller.LTouch, leftControllerRootTransform, leftControllerDirectTransform));
			}
			if ((bool)rightControllerRootTransform)
			{
				_inputSources.Add(new ControllerInputSource(this, InputSource.ControllerRight, OVRInput.Controller.RTouch, rightControllerRootTransform, rightControllerDirectTransform));
			}
			if ((bool)handLeft)
			{
				_inputSources.Add(new HandInputSource(this, InputSource.HandLeft, handLeft));
			}
			if ((bool)handRight)
			{
				_inputSources.Add(new HandInputSource(this, InputSource.HandRight, handRight));
			}
		}
		foreach (IInputSource inputSource in _inputSources)
		{
			inputSource.Update();
		}
	}

	private ulong GetKeyboardSpace()
	{
		if (keyboardSpace_ != 0L)
		{
			return keyboardSpace_;
		}
		OVRPlugin.VirtualKeyboardSpaceCreateInfo createInfo = default(OVRPlugin.VirtualKeyboardSpaceCreateInfo);
		OVRPlugin.VirtualKeyboardLocationInfo virtualKeyboardLocationInfo = ComputeLocation(base.transform);
		createInfo.locationType = virtualKeyboardLocationInfo.locationType;
		createInfo.trackingOriginType = OVRPlugin.GetTrackingOriginType();
		createInfo.pose = virtualKeyboardLocationInfo.pose;
		OVRPlugin.Result result = OVRPlugin.CreateVirtualKeyboardSpace(createInfo, out keyboardSpace_);
		if (result != 0)
		{
			Debug.LogError("Create failed to create keyboard space: " + result);
			DestroyKeyboard();
		}
		UseSuggestedLocation(InitialPosition);
		return keyboardSpace_;
	}

	private void SyncKeyboardLocation()
	{
		if (keyboardSpace_ != 0L && base.transform.hasChanged)
		{
			float num = MaxElement(base.transform.localScale);
			Vector3 localScale = Vector3.one * num;
			base.transform.localScale = localScale;
			UseSuggestedLocation(KeyboardPosition.Custom);
		}
		if (!OVRPlugin.TryLocateSpace(GetKeyboardSpace(), OVRPlugin.GetTrackingOriginType(), out var pose))
		{
			Debug.LogError("Failed to locate the virtual keyboard space.");
			return;
		}
		if (OVRPlugin.GetVirtualKeyboardScale(out var scale) != 0)
		{
			Debug.LogError("Failed to get virtual keyboard scale.");
			return;
		}
		Transform obj = base.transform;
		obj.SetPositionAndRotation(pose.Position.FromFlippedZVector3f(), pose.Orientation.FromFlippedZQuatf());
		obj.localScale = Vector3.one * scale;
		obj.hasChanged = false;
	}

	private void UpdateAnimationState()
	{
		if (!modelAvailable_)
		{
			return;
		}
		OVRPlugin.GetVirtualKeyboardDirtyTextures(out var textureIds);
		ulong[] textureIds2 = textureIds.TextureIds;
		foreach (ulong num in textureIds2)
		{
			if (!virtualKeyboardTextures_.TryGetValue(num, out var value))
			{
				continue;
			}
			OVRPlugin.VirtualKeyboardTextureData textureData = default(OVRPlugin.VirtualKeyboardTextureData);
			OVRPlugin.GetVirtualKeyboardTextureData(num, ref textureData);
			if (textureData.BufferCountOutput == 0)
			{
				continue;
			}
			if (value.bufferLength < textureData.BufferCountOutput && value.buffer != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(value.buffer);
				value.buffer = IntPtr.Zero;
			}
			if (value.buffer == IntPtr.Zero)
			{
				value.bufferLength = textureData.BufferCountOutput;
				value.buffer = Marshal.AllocHGlobal((int)textureData.BufferCountOutput);
			}
			textureData.Buffer = value.buffer;
			textureData.BufferCapacityInput = value.bufferLength;
			OVRPlugin.GetVirtualKeyboardTextureData(num, ref textureData);
			if (value.hasTexture && (value.texture.width != textureData.TextureWidth || value.texture.height != textureData.TextureHeight))
			{
				value.hasTexture = false;
			}
			if (!value.hasTexture)
			{
				value.texture = new Texture2D((int)textureData.TextureWidth, (int)textureData.TextureHeight, TextureFormat.RGBA32, mipChain: false);
				value.texture.filterMode = FilterMode.Trilinear;
				value.hasTexture = true;
			}
			value.texture.LoadRawTextureData(value.buffer, (int)value.bufferLength);
			value.texture.Apply(updateMipmaps: true, makeNoLongerReadable: false);
			virtualKeyboardTextures_[num] = value;
			foreach (Material material in value.materials)
			{
				material.mainTexture = value.texture;
			}
		}
		_animationStateCount = 0;
		OVRPlugin.GetVirtualKeyboardModelAnimationStates(AnimationStatesBufferProvider, AnimationStateHandler);
		if (_animationStateCount <= 0)
		{
			return;
		}
		foreach (OVRGLTFAnimationNodeMorphTargetHandler morphTargetHandler in virtualKeyboardScene_.morphTargetHandlers)
		{
			morphTargetHandler.Update();
		}
	}

	private IntPtr AnimationStatesBufferProvider(int bufferLength, int count)
	{
		if (_animationStateBufferLength < bufferLength)
		{
			Marshal.FreeHGlobal(_animationStateBuffer);
			_animationStateBufferLength = bufferLength;
			_animationStateBuffer = Marshal.AllocHGlobal(_animationStateBufferLength);
		}
		_animationStateCount = count;
		return _animationStateBuffer;
	}

	private void AnimationStateHandler(ref OVRPlugin.VirtualKeyboardModelAnimationState state)
	{
		if (state.AnimationIndex >= virtualKeyboardScene_.animationNodeLookup.Count)
		{
			Debug.LogWarning($"Unknown Animation State Index {state.AnimationIndex}");
			return;
		}
		OVRGLTFAnimatinonNode[] array = virtualKeyboardScene_.animationNodeLookup[state.AnimationIndex];
		for (int i = 0; i < array.Length; i++)
		{
			array[i].UpdatePose(state.Fraction, applyDeadZone: false);
		}
	}

	private void OnCommitText(string text)
	{
		if (TextHandler == null)
		{
			return;
		}
		using TextHandlerScope textHandlerScope = new TextHandlerScope(TextHandler, OnTextHandlerChange);
		textHandlerScope.AppendText(text);
	}

	private void OnTextHandlerChange(string textContext)
	{
		ChangeTextContextInternal(textContext);
	}

	private void ChangeTextContextInternal(string textContext)
	{
		if (isKeyboardCreated_ && OVRPlugin.ChangeVirtualKeyboardTextContext(textContext) != 0)
		{
			Debug.LogError("Failed to set keyboard text context");
		}
	}

	private void OnBackspace()
	{
		if (TextHandler == null)
		{
			return;
		}
		using TextHandlerScope textHandlerScope = new TextHandlerScope(TextHandler, OnTextHandlerChange);
		textHandlerScope.ApplyBackspace();
	}

	private void OnEnter()
	{
		if (TextHandler == null)
		{
			return;
		}
		using TextHandlerScope textHandlerScope = new TextHandlerScope(TextHandler, OnTextHandlerChange);
		if (textHandlerScope.SubmitOnEnter)
		{
			textHandlerScope.Submit();
		}
		else
		{
			OnCommitText("\n");
		}
	}

	private void OnKeyboardShown()
	{
		if (!keyboardVisible_)
		{
			keyboardVisible_ = true;
			UpdateVisibleState();
		}
	}

	private void OnKeyboardHidden()
	{
		if (keyboardVisible_)
		{
			keyboardVisible_ = false;
			UpdateVisibleState();
		}
	}

	private void UpdateVisibleState()
	{
		base.gameObject.SetActive(keyboardVisible_);
		if (modelAvailable_)
		{
			virtualKeyboardScene_.root.gameObject.SetActive(keyboardVisible_);
		}
	}

	[ContextMenu("Autofill Input Roots")]
	public void AutoPopulate()
	{
		OVRCameraRig oVRCameraRig = UnityEngine.Object.FindAnyObjectByType<OVRCameraRig>();
		if (oVRCameraRig == null)
		{
			Debug.LogWarning("Couldn't auto fill input transforms as we didn't have an OVRCameraRig.");
			return;
		}
		if (handRight == null || handLeft == null)
		{
			OVRHand[] componentsInChildren = oVRCameraRig.GetComponentsInChildren<OVRHand>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (componentsInChildren[i].HandType == OVRHand.Hand.HandLeft && handLeft == null)
				{
					handLeft = componentsInChildren[i];
				}
				if (componentsInChildren[i].HandType == OVRHand.Hand.HandRight && handRight == null)
				{
					handRight = componentsInChildren[i];
				}
				if ((bool)handRight && (bool)handLeft)
				{
					break;
				}
			}
		}
		if (!(leftControllerRootTransform == null) && !(rightControllerRootTransform == null))
		{
			return;
		}
		OVRControllerHelper[] componentsInChildren2 = oVRCameraRig.GetComponentsInChildren<OVRControllerHelper>();
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			if (leftControllerRootTransform == null && componentsInChildren2[j].m_controller == OVRInput.Controller.LTouch)
			{
				leftControllerRootTransform = componentsInChildren2[j].transform;
			}
			if (rightControllerRootTransform == null && componentsInChildren2[j].m_controller == OVRInput.Controller.RTouch)
			{
				rightControllerRootTransform = componentsInChildren2[j].transform;
			}
			if ((bool)leftControllerRootTransform && (bool)rightControllerRootTransform)
			{
				break;
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Rendering.UI;

namespace UnityEngine.Rendering;

public sealed class DebugManager
{
	public enum UIMode
	{
		EditorMode,
		RuntimeMode
	}

	private class UIState
	{
		public UIMode mode;

		[SerializeField]
		private bool m_Open;

		public bool open
		{
			get
			{
				return m_Open;
			}
			set
			{
				if (m_Open != value)
				{
					m_Open = value;
					DebugManager.windowStateChanged?.Invoke(mode, m_Open);
				}
			}
		}
	}

	private const string kEnableDebugBtn1 = "Enable Debug Button 1";

	private const string kEnableDebugBtn2 = "Enable Debug Button 2";

	private const string kDebugPreviousBtn = "Debug Previous";

	private const string kDebugNextBtn = "Debug Next";

	private const string kValidateBtn = "Debug Validate";

	private const string kPersistentBtn = "Debug Persistent";

	private const string kDPadVertical = "Debug Vertical";

	private const string kDPadHorizontal = "Debug Horizontal";

	private const string kMultiplierBtn = "Debug Multiplier";

	private const string kResetBtn = "Debug Reset";

	private const string kEnableDebug = "Enable Debug";

	private DebugActionDesc[] m_DebugActions;

	private DebugActionState[] m_DebugActionStates;

	private InputActionMap debugActionMap = new InputActionMap("Debug Menu");

	private static readonly Lazy<DebugManager> s_Instance = new Lazy<DebugManager>(() => new DebugManager());

	private ReadOnlyCollection<DebugUI.Panel> m_ReadOnlyPanels;

	private readonly List<DebugUI.Panel> m_Panels = new List<DebugUI.Panel>();

	public bool refreshEditorRequested;

	private int? m_RequestedPanelIndex;

	private GameObject m_Root;

	private DebugUIHandlerCanvas m_RootUICanvas;

	private GameObject m_PersistentRoot;

	private DebugUIHandlerPersistentCanvas m_RootUIPersistentCanvas;

	private UIState editorUIState = new UIState
	{
		mode = UIMode.EditorMode
	};

	private bool m_EnableRuntimeUI = true;

	private UIState runtimeUIState = new UIState
	{
		mode = UIMode.RuntimeMode
	};

	public static DebugManager instance => s_Instance.Value;

	public ReadOnlyCollection<DebugUI.Panel> panels
	{
		get
		{
			if (m_ReadOnlyPanels == null)
			{
				UpdateReadOnlyCollection();
			}
			return m_ReadOnlyPanels;
		}
	}

	public bool isAnyDebugUIActive
	{
		get
		{
			if (!displayRuntimeUI)
			{
				return displayPersistentRuntimeUI;
			}
			return true;
		}
	}

	public bool displayEditorUI
	{
		get
		{
			return editorUIState.open;
		}
		set
		{
			editorUIState.open = value;
		}
	}

	public bool enableRuntimeUI
	{
		get
		{
			return m_EnableRuntimeUI;
		}
		set
		{
			if (value != m_EnableRuntimeUI)
			{
				m_EnableRuntimeUI = value;
				DebugUpdater.SetEnabled(value);
			}
		}
	}

	public bool displayRuntimeUI
	{
		get
		{
			if (m_Root != null)
			{
				return m_Root.activeInHierarchy;
			}
			return false;
		}
		set
		{
			if (value)
			{
				m_Root = Object.Instantiate(Resources.Load<Transform>("DebugUICanvas")).gameObject;
				m_Root.name = "[Debug Canvas]";
				m_Root.transform.localPosition = Vector3.zero;
				m_RootUICanvas = m_Root.GetComponent<DebugUIHandlerCanvas>();
				m_Root.SetActive(value: true);
			}
			else
			{
				CoreUtils.Destroy(m_Root);
				m_Root = null;
				m_RootUICanvas = null;
			}
			this.onDisplayRuntimeUIChanged(value);
			DebugUpdater.HandleInternalEventSystemComponents(value);
			runtimeUIState.open = m_Root != null && m_Root.activeInHierarchy;
		}
	}

	public bool displayPersistentRuntimeUI
	{
		get
		{
			if (m_RootUIPersistentCanvas != null)
			{
				return m_PersistentRoot.activeInHierarchy;
			}
			return false;
		}
		set
		{
			if (value)
			{
				EnsurePersistentCanvas();
				return;
			}
			CoreUtils.Destroy(m_PersistentRoot);
			m_PersistentRoot = null;
			m_RootUIPersistentCanvas = null;
		}
	}

	public event Action<bool> onDisplayRuntimeUIChanged = delegate
	{
	};

	public event Action onSetDirty = delegate
	{
	};

	private event Action resetData;

	public static event Action<UIMode, bool> windowStateChanged;

	private void RegisterActions()
	{
		m_DebugActions = new DebugActionDesc[9];
		m_DebugActionStates = new DebugActionState[9];
		DebugActionDesc debugActionDesc = new DebugActionDesc();
		debugActionDesc.buttonAction = debugActionMap.FindAction("Enable Debug");
		debugActionDesc.repeatMode = DebugActionRepeatMode.Never;
		AddAction(DebugAction.EnableDebugMenu, debugActionDesc);
		DebugActionDesc debugActionDesc2 = new DebugActionDesc();
		debugActionDesc2.buttonAction = debugActionMap.FindAction("Debug Reset");
		debugActionDesc2.repeatMode = DebugActionRepeatMode.Never;
		AddAction(DebugAction.ResetAll, debugActionDesc2);
		DebugActionDesc debugActionDesc3 = new DebugActionDesc();
		debugActionDesc3.buttonAction = debugActionMap.FindAction("Debug Next");
		debugActionDesc3.repeatMode = DebugActionRepeatMode.Never;
		AddAction(DebugAction.NextDebugPanel, debugActionDesc3);
		DebugActionDesc debugActionDesc4 = new DebugActionDesc();
		debugActionDesc4.buttonAction = debugActionMap.FindAction("Debug Previous");
		debugActionDesc4.repeatMode = DebugActionRepeatMode.Never;
		AddAction(DebugAction.PreviousDebugPanel, debugActionDesc4);
		DebugActionDesc debugActionDesc5 = new DebugActionDesc();
		debugActionDesc5.buttonAction = debugActionMap.FindAction("Debug Validate");
		debugActionDesc5.repeatMode = DebugActionRepeatMode.Never;
		AddAction(DebugAction.Action, debugActionDesc5);
		DebugActionDesc debugActionDesc6 = new DebugActionDesc();
		debugActionDesc6.buttonAction = debugActionMap.FindAction("Debug Persistent");
		debugActionDesc6.repeatMode = DebugActionRepeatMode.Never;
		AddAction(DebugAction.MakePersistent, debugActionDesc6);
		DebugActionDesc debugActionDesc7 = new DebugActionDesc();
		debugActionDesc7.buttonAction = debugActionMap.FindAction("Debug Multiplier");
		debugActionDesc7.repeatMode = DebugActionRepeatMode.Delay;
		debugActionDesc5.repeatDelay = 0f;
		AddAction(DebugAction.Multiplier, debugActionDesc7);
		DebugActionDesc debugActionDesc8 = new DebugActionDesc();
		debugActionDesc8.buttonAction = debugActionMap.FindAction("Debug Vertical");
		debugActionDesc8.repeatMode = DebugActionRepeatMode.Delay;
		debugActionDesc8.repeatDelay = 0.16f;
		AddAction(DebugAction.MoveVertical, debugActionDesc8);
		DebugActionDesc debugActionDesc9 = new DebugActionDesc();
		debugActionDesc9.buttonAction = debugActionMap.FindAction("Debug Horizontal");
		debugActionDesc9.repeatMode = DebugActionRepeatMode.Delay;
		debugActionDesc9.repeatDelay = 0.16f;
		AddAction(DebugAction.MoveHorizontal, debugActionDesc9);
	}

	internal void EnableInputActions()
	{
		foreach (InputAction item in debugActionMap)
		{
			item.Enable();
		}
	}

	private void AddAction(DebugAction action, DebugActionDesc desc)
	{
		m_DebugActions[(int)action] = desc;
		m_DebugActionStates[(int)action] = new DebugActionState();
	}

	private void SampleAction(int actionIndex)
	{
		DebugActionDesc debugActionDesc = m_DebugActions[actionIndex];
		DebugActionState debugActionState = m_DebugActionStates[actionIndex];
		if (!debugActionState.runningAction && debugActionDesc.buttonAction != null)
		{
			float num = debugActionDesc.buttonAction.ReadValue<float>();
			if (!Mathf.Approximately(num, 0f))
			{
				debugActionState.TriggerWithButton(debugActionDesc.buttonAction, num);
			}
		}
	}

	private void UpdateAction(int actionIndex)
	{
		DebugActionDesc desc = m_DebugActions[actionIndex];
		DebugActionState debugActionState = m_DebugActionStates[actionIndex];
		if (debugActionState.runningAction)
		{
			debugActionState.Update(desc);
		}
	}

	internal void UpdateActions()
	{
		for (int i = 0; i < m_DebugActions.Length; i++)
		{
			UpdateAction(i);
			SampleAction(i);
		}
	}

	internal float GetAction(DebugAction action)
	{
		return m_DebugActionStates[(int)action].actionState;
	}

	internal bool GetActionToggleDebugMenuWithTouch()
	{
		if (!EnhancedTouchSupport.enabled)
		{
			return false;
		}
		ReadOnlyArray<Touch> activeTouches = Touch.activeTouches;
		int count = activeTouches.Count;
		TouchPhase? touchPhase = null;
		if (count == 3)
		{
			foreach (Touch item in activeTouches)
			{
				if ((!touchPhase.HasValue || item.phase == touchPhase.Value) && item.tapCount == 2)
				{
					return true;
				}
			}
		}
		return false;
	}

	internal bool GetActionReleaseScrollTarget()
	{
		bool num = Mouse.current != null && Mouse.current.scroll.ReadValue() != Vector2.zero;
		bool flag = Touchscreen.current != null;
		return num || flag;
	}

	private void RegisterInputs()
	{
		debugActionMap.AddAction("Enable Debug", InputActionType.Button).AddCompositeBinding("ButtonWithOneModifier").With("Modifier", "<Gamepad>/rightStickPress")
			.With("Button", "<Gamepad>/leftStickPress")
			.With("Modifier", "<Keyboard>/leftCtrl")
			.With("Button", "<Keyboard>/backspace");
		debugActionMap.AddAction("Debug Reset", InputActionType.Button).AddCompositeBinding("ButtonWithOneModifier").With("Modifier", "<Gamepad>/rightStickPress")
			.With("Button", "<Gamepad>/b")
			.With("Modifier", "<Keyboard>/leftAlt")
			.With("Button", "<Keyboard>/backspace");
		InputAction action = debugActionMap.AddAction("Debug Next", InputActionType.Button);
		action.AddBinding("<Keyboard>/pageDown");
		action.AddBinding("<Gamepad>/rightShoulder");
		InputAction action2 = debugActionMap.AddAction("Debug Previous", InputActionType.Button);
		action2.AddBinding("<Keyboard>/pageUp");
		action2.AddBinding("<Gamepad>/leftShoulder");
		InputAction action3 = debugActionMap.AddAction("Debug Validate", InputActionType.Button);
		action3.AddBinding("<Keyboard>/enter");
		action3.AddBinding("<Gamepad>/a");
		InputAction action4 = debugActionMap.AddAction("Debug Persistent", InputActionType.Button);
		action4.AddBinding("<Keyboard>/rightShift");
		action4.AddBinding("<Gamepad>/x");
		InputAction action5 = debugActionMap.AddAction("Debug Multiplier");
		action5.AddBinding("<Keyboard>/leftShift");
		action5.AddBinding("<Gamepad>/y");
		debugActionMap.AddAction("Debug Vertical").AddCompositeBinding("1DAxis").With("Positive", "<Gamepad>/dpad/up")
			.With("Negative", "<Gamepad>/dpad/down")
			.With("Positive", "<Keyboard>/upArrow")
			.With("Negative", "<Keyboard>/downArrow");
		debugActionMap.AddAction("Debug Horizontal").AddCompositeBinding("1DAxis").With("Positive", "<Gamepad>/dpad/right")
			.With("Negative", "<Gamepad>/dpad/left")
			.With("Positive", "<Keyboard>/rightArrow")
			.With("Negative", "<Keyboard>/leftArrow");
	}

	private void UpdateReadOnlyCollection()
	{
		m_Panels.Sort();
		m_ReadOnlyPanels = m_Panels.AsReadOnly();
	}

	private DebugManager()
	{
	}

	public void RefreshEditor()
	{
		refreshEditorRequested = true;
	}

	public void Reset()
	{
		this.resetData?.Invoke();
		ReDrawOnScreenDebug();
	}

	public void ReDrawOnScreenDebug()
	{
		if (displayRuntimeUI)
		{
			m_RootUICanvas?.RequestHierarchyReset();
		}
	}

	public void RegisterData(IDebugData data)
	{
		resetData += data.GetReset();
	}

	public void UnregisterData(IDebugData data)
	{
		resetData -= data.GetReset();
	}

	public int GetState()
	{
		int num = 17;
		foreach (DebugUI.Panel panel in m_Panels)
		{
			num = num * 23 + panel.GetHashCode();
		}
		return num;
	}

	internal void RegisterRootCanvas(DebugUIHandlerCanvas root)
	{
		m_Root = root.gameObject;
		m_RootUICanvas = root;
	}

	internal void ChangeSelection(DebugUIHandlerWidget widget, bool fromNext)
	{
		m_RootUICanvas.ChangeSelection(widget, fromNext);
	}

	internal void SetScrollTarget(DebugUIHandlerWidget widget)
	{
		if (m_RootUICanvas != null)
		{
			m_RootUICanvas.SetScrollTarget(widget);
		}
	}

	private void EnsurePersistentCanvas()
	{
		if (m_RootUIPersistentCanvas == null)
		{
			DebugUIHandlerPersistentCanvas debugUIHandlerPersistentCanvas = Object.FindFirstObjectByType<DebugUIHandlerPersistentCanvas>();
			if (debugUIHandlerPersistentCanvas == null)
			{
				m_PersistentRoot = Object.Instantiate(Resources.Load<Transform>("DebugUIPersistentCanvas")).gameObject;
				m_PersistentRoot.name = "[Debug Canvas - Persistent]";
				m_PersistentRoot.transform.localPosition = Vector3.zero;
			}
			else
			{
				m_PersistentRoot = debugUIHandlerPersistentCanvas.gameObject;
			}
			m_RootUIPersistentCanvas = m_PersistentRoot.GetComponent<DebugUIHandlerPersistentCanvas>();
		}
	}

	internal void TogglePersistent(DebugUI.Widget widget, int? forceTupleIndex = null)
	{
		if (widget == null)
		{
			return;
		}
		EnsurePersistentCanvas();
		if (!(widget is DebugUI.Value widget2))
		{
			if (!(widget is DebugUI.ValueTuple widget3))
			{
				if (widget is DebugUI.Container container)
				{
					int value = container.children.Max((DebugUI.Widget w) => (w as DebugUI.ValueTuple)?.pinnedElementIndex ?? (-1));
					{
						foreach (DebugUI.Widget child in container.children)
						{
							if (child is DebugUI.Value || child is DebugUI.ValueTuple)
							{
								TogglePersistent(child, value);
							}
						}
						return;
					}
				}
				Debug.Log("Only readonly items can be made persistent.");
			}
			else
			{
				m_RootUIPersistentCanvas.Toggle(widget3, forceTupleIndex);
			}
		}
		else
		{
			m_RootUIPersistentCanvas.Toggle(widget2);
		}
	}

	private void OnPanelDirty(DebugUI.Panel panel)
	{
		this.onSetDirty();
	}

	public int PanelIndex([DisallowNull] string displayName)
	{
		if (displayName == null)
		{
			displayName = string.Empty;
		}
		for (int i = 0; i < m_Panels.Count; i++)
		{
			if (displayName.Equals(m_Panels[i].displayName, StringComparison.InvariantCultureIgnoreCase))
			{
				return i;
			}
		}
		return -1;
	}

	public string PanelDiplayName([DisallowNull] int panelIndex)
	{
		if (panelIndex < 0 || panelIndex > m_Panels.Count - 1)
		{
			return string.Empty;
		}
		return m_Panels[panelIndex].displayName;
	}

	public void RequestEditorWindowPanelIndex(int index)
	{
		m_RequestedPanelIndex = index;
	}

	internal int? GetRequestedEditorWindowPanelIndex()
	{
		int? requestedPanelIndex = m_RequestedPanelIndex;
		m_RequestedPanelIndex = null;
		return requestedPanelIndex;
	}

	public DebugUI.Panel GetPanel(string displayName, bool createIfNull = false, int groupIndex = 0, bool overrideIfExist = false)
	{
		int num = PanelIndex(displayName);
		DebugUI.Panel panel = ((num >= 0) ? m_Panels[num] : null);
		if (panel != null)
		{
			if (!overrideIfExist)
			{
				return panel;
			}
			panel.onSetDirty -= OnPanelDirty;
			RemovePanel(panel);
			panel = null;
		}
		if (createIfNull)
		{
			panel = new DebugUI.Panel
			{
				displayName = displayName,
				groupIndex = groupIndex
			};
			panel.onSetDirty += OnPanelDirty;
			m_Panels.Add(panel);
			UpdateReadOnlyCollection();
		}
		return panel;
	}

	public int FindPanelIndex(string displayName)
	{
		return m_Panels.FindIndex((DebugUI.Panel p) => p.displayName == displayName);
	}

	public void RemovePanel(string displayName)
	{
		DebugUI.Panel panel = null;
		foreach (DebugUI.Panel panel2 in m_Panels)
		{
			if (panel2.displayName == displayName)
			{
				panel2.onSetDirty -= OnPanelDirty;
				panel = panel2;
				break;
			}
		}
		RemovePanel(panel);
	}

	public void RemovePanel(DebugUI.Panel panel)
	{
		if (panel != null)
		{
			m_Panels.Remove(panel);
			UpdateReadOnlyCollection();
		}
	}

	public DebugUI.Widget[] GetItems(DebugUI.Flags flags)
	{
		List<DebugUI.Widget> value;
		using (ListPool<DebugUI.Widget>.Get(out value))
		{
			foreach (DebugUI.Panel panel in m_Panels)
			{
				DebugUI.Widget[] itemsFromContainer = GetItemsFromContainer(flags, panel);
				value.AddRange(itemsFromContainer);
			}
			return value.ToArray();
		}
	}

	internal DebugUI.Widget[] GetItemsFromContainer(DebugUI.Flags flags, DebugUI.IContainer container)
	{
		List<DebugUI.Widget> value;
		using (ListPool<DebugUI.Widget>.Get(out value))
		{
			foreach (DebugUI.Widget child in container.children)
			{
				if (child.flags.HasFlag(flags))
				{
					value.Add(child);
				}
				else if (child is DebugUI.IContainer container2)
				{
					value.AddRange(GetItemsFromContainer(flags, container2));
				}
			}
			return value.ToArray();
		}
	}

	public DebugUI.Widget GetItem(string queryPath)
	{
		foreach (DebugUI.Panel panel in m_Panels)
		{
			DebugUI.Widget item = GetItem(queryPath, panel);
			if (item != null)
			{
				return item;
			}
		}
		return null;
	}

	private DebugUI.Widget GetItem(string queryPath, DebugUI.IContainer container)
	{
		foreach (DebugUI.Widget child in container.children)
		{
			if (child.queryPath == queryPath)
			{
				return child;
			}
			if (child is DebugUI.IContainer container2)
			{
				DebugUI.Widget item = GetItem(queryPath, container2);
				if (item != null)
				{
					return item;
				}
			}
		}
		return null;
	}

	[Obsolete("Use DebugManager.instance.displayEditorUI property instead. #from(23.1)")]
	public void ToggleEditorUI(bool open)
	{
		editorUIState.open = open;
	}
}

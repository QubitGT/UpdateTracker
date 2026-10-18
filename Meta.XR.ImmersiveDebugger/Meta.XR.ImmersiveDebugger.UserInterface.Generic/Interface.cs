using UnityEngine;

namespace Meta.XR.ImmersiveDebugger.UserInterface.Generic;

public class Interface : Controller
{
	private ProxyInputModule _proxyInputModule;

	private ProxyCameraRig _proxyCameraRig;

	private bool _positionHasBeenInitialized;

	internal Cursor Cursor { get; private set; }

	internal Camera Camera => _proxyCameraRig.Camera;

	protected virtual bool FollowOverride { get; set; }

	protected virtual bool RotateOverride { get; set; }

	internal virtual void Awake()
	{
		Setup(null);
		GameObject gameObject = new GameObject("cursor");
		gameObject.transform.SetParent(base.Transform);
		Cursor = gameObject.AddComponent<Cursor>();
		_proxyCameraRig = new ProxyCameraRig();
		_proxyInputModule = new ProxyInputModule(base.GameObject, Cursor);
	}

	private void UpdateTransform(bool updatePosition, bool updateRotation)
	{
		if (updatePosition)
		{
			Vector3 position = _proxyCameraRig.CameraTransform.position;
			if (position != Vector3.zero)
			{
				_positionHasBeenInitialized = true;
			}
			base.Transform.position = position;
		}
		if (updateRotation)
		{
			Vector3 eulerAngles = _proxyCameraRig.CameraTransform.eulerAngles;
			eulerAngles.x = 0f;
			eulerAngles.z = 0f;
			base.Transform.rotation = Quaternion.Euler(eulerAngles);
		}
	}

	protected override void OnVisibilityChanged()
	{
		base.OnVisibilityChanged();
		if (base.Visibility && _proxyCameraRig.Refresh())
		{
			UpdateTransform(updatePosition: true, updateRotation: true);
		}
	}

	private void UpdateCulling()
	{
		RuntimeSettings instance = RuntimeSettings.Instance;
		if (instance.AutomaticLayerCullingUpdate)
		{
			int cullingMask = Camera.cullingMask;
			int num = SetBits(cullingMask, instance.PanelLayer, instance.MeshRendererLayer, !RuntimeSettings.Instance.ShouldUseOverlay);
			if (num != cullingMask)
			{
				Camera.cullingMask = num;
			}
		}
	}

	private static int SetBits(int cullingMask, int bitPosition1, int bitPosition2, bool state)
	{
		if (state)
		{
			cullingMask |= 1 << bitPosition1;
			cullingMask |= 1 << bitPosition2;
		}
		else
		{
			cullingMask &= ~(1 << bitPosition1);
			cullingMask &= ~(1 << bitPosition2);
		}
		return cullingMask;
	}

	internal virtual void LateUpdate()
	{
		UpdateRefreshLayout(force: false);
		if (_proxyCameraRig.Refresh())
		{
			UpdateTransform(FollowOverride || !_positionHasBeenInitialized, RotateOverride || !_positionHasBeenInitialized);
			UpdateCulling();
			_proxyInputModule.Refresh();
		}
	}

	protected override void RefreshLayoutPreChildren()
	{
	}
}

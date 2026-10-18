using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Meta.XR.Util;
using UnityEngine;
using UnityEngine.Events;

[HelpURL("https://developer.oculus.com/documentation/unity/unity-passthrough-gs/")]
[Feature(Feature.Passthrough)]
public class OVRPassthroughLayer : MonoBehaviour
{
	public enum ProjectionSurfaceType
	{
		Reconstructed,
		UserDefined
	}

	public enum ColorMapEditorType
	{
		None = 0,
		GrayscaleToColor = 1,
		Controls = 1,
		Custom = 2,
		Grayscale = 3,
		ColorAdjustment = 4,
		ColorLut = 5,
		InterpolatedColorLut = 6
	}

	private struct Settings
	{
		public Texture2D colorLutTargetTexture;

		public Texture2D colorLutSourceTexture;

		public float saturation;

		public float posterize;

		public float brightness;

		public float contrast;

		public Gradient gradient;

		public float lutWeight;

		public bool flipLutY;

		public Settings(Texture2D colorLutTargetTexture, Texture2D colorLutSourceTexture, float saturation, float posterize, float brightness, float contrast, Gradient gradient, float lutWeight, bool flipLutY)
		{
			this.colorLutTargetTexture = colorLutTargetTexture;
			this.colorLutSourceTexture = colorLutSourceTexture;
			this.saturation = saturation;
			this.posterize = posterize;
			this.brightness = brightness;
			this.contrast = contrast;
			this.gradient = gradient;
			this.lutWeight = lutWeight;
			this.flipLutY = flipLutY;
		}
	}

	private struct PassthroughMeshInstance
	{
		public ulong meshHandle;

		public ulong instanceHandle;

		public bool updateTransform;

		public Matrix4x4 localToWorld;
	}

	[Serializable]
	internal struct SerializedSurfaceGeometry
	{
		public MeshFilter meshFilter;

		public bool updateTransform;
	}

	private struct DeferredPassthroughMeshAddition
	{
		public GameObject gameObject;

		public bool updateTransform;
	}

	private interface IStyleHandler
	{
		bool IsValid { get; }

		void ApplyStyleSettings(ref OVRPlugin.InsightPassthroughStyle2 style);

		void Update(Settings settings);

		void Clear();
	}

	private class StylesHandler
	{
		private NoneStyleHandler _noneHandler;

		private ColorLutHandler _lutHandler;

		private InterpolatedColorLutHandler _interpolatedLutHandler;

		private MonoToRgbaStyleHandler _monoToRgbaHandler;

		private MonoToMonoStyleHandler _monoToMonoHandler;

		private BCSStyleHandler _bcsHandler;

		private GCHandle _colorMapDataHandle;

		private byte[] _colorMapData;

		public IStyleHandler CurrentStyleHandler;

		public StylesHandler()
		{
			_noneHandler = new NoneStyleHandler();
			_lutHandler = new ColorLutHandler();
			_interpolatedLutHandler = new InterpolatedColorLutHandler();
			_monoToMonoHandler = new MonoToMonoStyleHandler(ref _colorMapDataHandle, _colorMapData);
			_monoToRgbaHandler = new MonoToRgbaStyleHandler(ref _colorMapDataHandle, _colorMapData);
			_bcsHandler = new BCSStyleHandler(ref _colorMapDataHandle, _colorMapData);
		}

		public void SetStyleHandler(OVRPlugin.InsightPassthroughColorMapType type)
		{
			IStyleHandler styleHandler = GetStyleHandler(type);
			if (styleHandler != CurrentStyleHandler)
			{
				if (CurrentStyleHandler != null)
				{
					CurrentStyleHandler.Clear();
				}
				CurrentStyleHandler = styleHandler;
			}
		}

		private IStyleHandler GetStyleHandler(OVRPlugin.InsightPassthroughColorMapType type)
		{
			return type switch
			{
				OVRPlugin.InsightPassthroughColorMapType.None => _noneHandler, 
				OVRPlugin.InsightPassthroughColorMapType.MonoToRgba => _monoToRgbaHandler, 
				OVRPlugin.InsightPassthroughColorMapType.MonoToMono => _monoToMonoHandler, 
				OVRPlugin.InsightPassthroughColorMapType.BrightnessContrastSaturation => _bcsHandler, 
				OVRPlugin.InsightPassthroughColorMapType.ColorLut => _lutHandler, 
				OVRPlugin.InsightPassthroughColorMapType.InterpolatedColorLut => _interpolatedLutHandler, 
				_ => throw new ArgumentException($"Unrecognized color map type {type}."), 
			};
		}

		public void SetColorLutHandler(OVRPassthroughColorLut lut, float weight)
		{
			SetStyleHandler(OVRPlugin.InsightPassthroughColorMapType.ColorLut);
			_lutHandler.Update(lut, weight);
		}

		internal void SetInterpolatedColorLutHandler(OVRPassthroughColorLut lutSource, OVRPassthroughColorLut lutTarget, float weight)
		{
			SetStyleHandler(OVRPlugin.InsightPassthroughColorMapType.InterpolatedColorLut);
			_interpolatedLutHandler.Update(lutSource, lutTarget, weight);
		}

		internal void SetMonoToRgbaHandler(Color[] values)
		{
			SetStyleHandler(OVRPlugin.InsightPassthroughColorMapType.MonoToRgba);
			_monoToRgbaHandler.Update(values);
		}

		internal void SetMonoToMonoHandler(byte[] values)
		{
			SetStyleHandler(OVRPlugin.InsightPassthroughColorMapType.MonoToMono);
			_monoToMonoHandler.Update(values);
		}
	}

	private class NoneStyleHandler : IStyleHandler
	{
		public bool IsValid => true;

		public void ApplyStyleSettings(ref OVRPlugin.InsightPassthroughStyle2 style)
		{
		}

		public void Update(Settings settings)
		{
		}

		public void Clear()
		{
		}
	}

	private abstract class BaseGeneratedStyleHandler : IStyleHandler
	{
		private GCHandle _colorMapDataHandle;

		protected byte[] _colorMapData;

		protected abstract uint MapSize { get; }

		public bool IsValid => true;

		public BaseGeneratedStyleHandler(ref GCHandle colorMapDataHandler, byte[] colorMapData)
		{
			_colorMapDataHandle = colorMapDataHandler;
			_colorMapData = colorMapData;
		}

		public virtual void Update(Settings settings)
		{
		}

		public virtual void ApplyStyleSettings(ref OVRPlugin.InsightPassthroughStyle2 style)
		{
			style.TextureColorMapData = _colorMapDataHandle.AddrOfPinnedObject();
			style.TextureColorMapDataSize = MapSize;
			style.TextureColorMapData = _colorMapDataHandle.AddrOfPinnedObject();
			style.TextureColorMapDataSize = MapSize;
		}

		public void Clear()
		{
			DeallocateColorMapData();
		}

		protected virtual void AllocateColorMapData(uint size = 4096u)
		{
			if (_colorMapData != null && size != _colorMapData.Length)
			{
				DeallocateColorMapData();
			}
			if (_colorMapData == null)
			{
				_colorMapData = new byte[size];
				_colorMapDataHandle = GCHandle.Alloc(_colorMapData, GCHandleType.Pinned);
			}
		}

		protected virtual void DeallocateColorMapData()
		{
			if (_colorMapData != null)
			{
				_colorMapDataHandle.Free();
				_colorMapData = null;
			}
		}

		protected void WriteColorToColorMap(int colorIndex, ref Color color)
		{
			for (int i = 0; i < 4; i++)
			{
				Buffer.BlockCopy(BitConverter.GetBytes(color[i]), 0, _colorMapData, colorIndex * 16 + i * 4, 4);
			}
		}

		protected void WriteFloatToColorMap(int index, float value)
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, _colorMapData, index * 4, 4);
		}

		protected static void ComputeBrightnessContrastPosterizeMap(byte[] result, float brightness, float contrast, float posterize)
		{
			for (int i = 0; i < 256; i++)
			{
				float num = (float)i / 255f;
				float num2 = contrast + 1f;
				num = (num - 0.5f) * num2 + 0.5f + brightness;
				if (posterize > 0f)
				{
					float num3 = (Mathf.Pow(50f, posterize) - 1f) / 49f;
					num = Mathf.Round(num / num3) * num3;
				}
				result[i] = (byte)(Mathf.Min(Mathf.Max(num, 0f), 1f) * 255f);
			}
		}
	}

	private class MonoToRgbaStyleHandler : BaseGeneratedStyleHandler
	{
		protected byte[] _tmpColorMapData;

		protected override uint MapSize => 4096u;

		public MonoToRgbaStyleHandler(ref GCHandle colorMapDataHandler, byte[] colorMapData)
			: base(ref colorMapDataHandler, colorMapData)
		{
		}

		public override void Update(Settings settings)
		{
			AllocateColorMapData();
			BaseGeneratedStyleHandler.ComputeBrightnessContrastPosterizeMap(_tmpColorMapData, settings.brightness, settings.contrast, settings.posterize);
			for (int i = 0; i < 256; i++)
			{
				Color color = settings.gradient.Evaluate((float)(int)_tmpColorMapData[i] / 255f);
				WriteColorToColorMap(i, ref color);
			}
		}

		public void Update(Color[] values)
		{
			AllocateColorMapData();
			for (int i = 0; i < 256; i++)
			{
				WriteColorToColorMap(i, ref values[i]);
			}
		}

		protected override void AllocateColorMapData(uint size = 4096u)
		{
			base.AllocateColorMapData(size);
			_tmpColorMapData = new byte[256];
		}

		protected override void DeallocateColorMapData()
		{
			base.DeallocateColorMapData();
			_tmpColorMapData = null;
		}
	}

	private class MonoToMonoStyleHandler : BaseGeneratedStyleHandler
	{
		protected override uint MapSize => 256u;

		public MonoToMonoStyleHandler(ref GCHandle colorMapDataHandler, byte[] colorMapData)
			: base(ref colorMapDataHandler, colorMapData)
		{
		}

		public override void Update(Settings settings)
		{
			AllocateColorMapData();
			BaseGeneratedStyleHandler.ComputeBrightnessContrastPosterizeMap(_colorMapData, settings.brightness, settings.contrast, settings.posterize);
		}

		public void Update(byte[] values)
		{
			AllocateColorMapData();
			Buffer.BlockCopy(values, 0, _colorMapData, 0, 256);
		}
	}

	private class BCSStyleHandler : BaseGeneratedStyleHandler
	{
		protected override uint MapSize => 12u;

		public BCSStyleHandler(ref GCHandle colorMapDataHandler, byte[] colorMapData)
			: base(ref colorMapDataHandler, colorMapData)
		{
		}

		public override void Update(Settings settings)
		{
			AllocateColorMapData();
			WriteFloatToColorMap(0, settings.brightness * 100f);
			WriteFloatToColorMap(1, settings.contrast + 1f);
			WriteFloatToColorMap(2, settings.saturation + 1f);
		}
	}

	private class ColorLutHandler : IStyleHandler
	{
		protected bool _currentFlipLutY;

		protected Texture2D _currentColorLutSourceTexture;

		public OVRPassthroughColorLut Lut { get; set; }

		public float Weight { get; set; }

		public bool IsValid { get; protected set; }

		public virtual void ApplyStyleSettings(ref OVRPlugin.InsightPassthroughStyle2 style)
		{
			style.LutSource = Lut._colorLutHandle;
			style.LutWeight = Weight;
		}

		public virtual void Update(Settings settings)
		{
			Update(GetColorLutForTexture(settings.colorLutSourceTexture, Lut, ref _currentColorLutSourceTexture, settings.flipLutY), settings.lutWeight);
		}

		protected OVRPassthroughColorLut GetColorLutForTexture(Texture2D newTexture, OVRPassthroughColorLut lut, ref Texture2D lastTexture, bool flipY)
		{
			if (newTexture == null)
			{
				Debug.LogError("Trying to update style with null texture.");
				return null;
			}
			if (lastTexture != newTexture || _currentFlipLutY != flipY)
			{
				lut?.Dispose();
				lastTexture = newTexture;
				_currentFlipLutY = flipY;
				return new OVRPassthroughColorLut(newTexture, _currentFlipLutY);
			}
			return lut;
		}

		internal void Update(OVRPassthroughColorLut lut, float weight)
		{
			if (lut == null)
			{
				IsValid = false;
				return;
			}
			IsValid = true;
			Lut = lut;
			Weight = weight;
		}

		public virtual void Clear()
		{
			Lut = null;
			_currentColorLutSourceTexture = null;
		}
	}

	private class InterpolatedColorLutHandler : ColorLutHandler
	{
		private Texture2D _currentColorLutTargetTexture;

		public OVRPassthroughColorLut LutTarget { get; set; }

		public override void ApplyStyleSettings(ref OVRPlugin.InsightPassthroughStyle2 style)
		{
			base.ApplyStyleSettings(ref style);
			style.LutTarget = LutTarget._colorLutHandle;
		}

		public override void Update(Settings settings)
		{
			Update(GetColorLutForTexture(settings.colorLutSourceTexture, base.Lut, ref _currentColorLutSourceTexture, settings.flipLutY), GetColorLutForTexture(settings.colorLutTargetTexture, LutTarget, ref _currentColorLutTargetTexture, settings.flipLutY), settings.lutWeight);
		}

		public void Update(OVRPassthroughColorLut lutSource, OVRPassthroughColorLut lutTarget, float weight)
		{
			if (lutSource == null || lutTarget == null)
			{
				base.IsValid = false;
				return;
			}
			base.IsValid = true;
			base.Lut = lutSource;
			LutTarget = lutTarget;
			base.Weight = weight;
		}

		public override void Clear()
		{
			base.Clear();
			LutTarget = null;
			_currentColorLutTargetTexture = null;
		}
	}

	public ProjectionSurfaceType projectionSurfaceType;

	public OVROverlay.OverlayType overlayType = OVROverlay.OverlayType.Overlay;

	public int compositionDepth;

	public bool hidden;

	public bool overridePerLayerColorScaleAndOffset;

	public Vector4 colorScale = Vector4.one;

	public Vector4 colorOffset = Vector4.zero;

	public UnityEvent<OVRPassthroughLayer> passthroughLayerResumed = new UnityEvent<OVRPassthroughLayer>();

	[SerializeField]
	internal ColorMapEditorType colorMapEditorType_;

	private static Dictionary<ColorMapEditorType, OVRPlugin.InsightPassthroughColorMapType> _editorToColorMapType = new Dictionary<ColorMapEditorType, OVRPlugin.InsightPassthroughColorMapType>
	{
		{
			ColorMapEditorType.None,
			OVRPlugin.InsightPassthroughColorMapType.None
		},
		{
			ColorMapEditorType.Grayscale,
			OVRPlugin.InsightPassthroughColorMapType.MonoToMono
		},
		{
			ColorMapEditorType.GrayscaleToColor,
			OVRPlugin.InsightPassthroughColorMapType.MonoToRgba
		},
		{
			ColorMapEditorType.ColorAdjustment,
			OVRPlugin.InsightPassthroughColorMapType.BrightnessContrastSaturation
		},
		{
			ColorMapEditorType.ColorLut,
			OVRPlugin.InsightPassthroughColorMapType.ColorLut
		},
		{
			ColorMapEditorType.InterpolatedColorLut,
			OVRPlugin.InsightPassthroughColorMapType.InterpolatedColorLut
		}
	};

	public Gradient colorMapEditorGradient = CreateNeutralColorMapGradient();

	[Range(-1f, 1f)]
	public float colorMapEditorContrast;

	[Range(-1f, 1f)]
	public float colorMapEditorBrightness;

	[Range(0f, 1f)]
	public float colorMapEditorPosterize;

	[Range(-1f, 1f)]
	public float colorMapEditorSaturation;

	[SerializeField]
	internal Texture2D _colorLutSourceTexture;

	[SerializeField]
	internal Texture2D _colorLutTargetTexture;

	[SerializeField]
	[Range(0f, 1f)]
	internal float _lutWeight = 1f;

	[SerializeField]
	internal bool _flipLutY = true;

	private Settings _settings = new Settings(null, null, 0f, 0f, 0f, 0f, new Gradient(), 1f, flipLutY: true);

	private OVRCameraRig cameraRig;

	private bool cameraRigInitialized;

	private GameObject auxGameObject;

	private OVROverlay passthroughOverlay;

	private Dictionary<GameObject, PassthroughMeshInstance> surfaceGameObjects = new Dictionary<GameObject, PassthroughMeshInstance>();

	private List<DeferredPassthroughMeshAddition> deferredSurfaceGameObjects = new List<DeferredPassthroughMeshAddition>();

	[SerializeField]
	[HideInInspector]
	internal List<SerializedSurfaceGeometry> serializedSurfaceGeometry = new List<SerializedSurfaceGeometry>();

	[SerializeField]
	[Range(0f, 1f)]
	internal float textureOpacity_ = 1f;

	[SerializeField]
	internal bool edgeRenderingEnabled_;

	[SerializeField]
	internal Color edgeColor_ = new Color(1f, 1f, 1f, 1f);

	[SerializeField]
	private OVRPlugin.InsightPassthroughColorMapType colorMapType;

	private bool styleDirty = true;

	private StylesHandler _stylesHandler = new StylesHandler();

	private static readonly Gradient colorMapNeutralGradient = CreateNeutralColorMapGradient();

	public float textureOpacity
	{
		get
		{
			return textureOpacity_;
		}
		set
		{
			value = Mathf.Clamp01(value);
			if (value != textureOpacity_)
			{
				textureOpacity_ = value;
				styleDirty = true;
			}
		}
	}

	public bool edgeRenderingEnabled
	{
		get
		{
			return edgeRenderingEnabled_;
		}
		set
		{
			if (value != edgeRenderingEnabled_)
			{
				edgeRenderingEnabled_ = value;
				styleDirty = true;
			}
		}
	}

	public Color edgeColor
	{
		get
		{
			return edgeColor_;
		}
		set
		{
			if (value != edgeColor_)
			{
				edgeColor_ = value;
				styleDirty = true;
			}
		}
	}

	public ColorMapEditorType colorMapEditorType
	{
		get
		{
			return colorMapEditorType_;
		}
		set
		{
			if (value == colorMapEditorType_)
			{
				return;
			}
			colorMapEditorType_ = value;
			if (value != ColorMapEditorType.Custom)
			{
				colorMapType = _editorToColorMapType[value];
				_stylesHandler.SetStyleHandler(colorMapType);
				if (value == ColorMapEditorType.None)
				{
					styleDirty = true;
				}
				else
				{
					UpdateColorMapFromControls(forceUpdate: true);
				}
			}
		}
	}

	private OVROverlay.OverlayShape overlayShape
	{
		get
		{
			if (projectionSurfaceType != ProjectionSurfaceType.UserDefined)
			{
				return OVROverlay.OverlayShape.ReconstructionPassthrough;
			}
			return OVROverlay.OverlayShape.SurfaceProjectedPassthrough;
		}
	}

	[Obsolete("This event is deprecated, use passthroughLayerResumed UnityEvent instead", false)]
	public event Action PassthroughLayerResumed;

	public void AddSurfaceGeometry(GameObject obj, bool updateTransform = false)
	{
		if (projectionSurfaceType != ProjectionSurfaceType.UserDefined)
		{
			Debug.LogError("Passthrough layer is not configured for surface projected passthrough.");
			return;
		}
		if (surfaceGameObjects.ContainsKey(obj))
		{
			Debug.LogError("Specified GameObject has already been added as passthrough surface.");
			return;
		}
		if (obj.GetComponent<MeshFilter>() == null)
		{
			Debug.LogError("Specified GameObject does not have a mesh component.");
			return;
		}
		deferredSurfaceGameObjects.Add(new DeferredPassthroughMeshAddition
		{
			gameObject = obj,
			updateTransform = updateTransform
		});
	}

	public void RemoveSurfaceGeometry(GameObject obj)
	{
		if (surfaceGameObjects.TryGetValue(obj, out var value))
		{
			if (OVRPlugin.DestroyInsightPassthroughGeometryInstance(value.instanceHandle) && OVRPlugin.DestroyInsightTriangleMesh(value.meshHandle))
			{
				surfaceGameObjects.Remove(obj);
			}
			else
			{
				Debug.LogError("GameObject could not be removed from passthrough surface.");
			}
		}
		else if (deferredSurfaceGameObjects.RemoveAll((DeferredPassthroughMeshAddition x) => x.gameObject == obj) == 0)
		{
			Debug.LogError("Specified GameObject has not been added as passthrough surface.");
		}
	}

	public bool IsSurfaceGeometry(GameObject obj)
	{
		if (!surfaceGameObjects.ContainsKey(obj))
		{
			return deferredSurfaceGameObjects.Exists((DeferredPassthroughMeshAddition x) => x.gameObject == obj);
		}
		return true;
	}

	public void SetColorMap(Color[] values)
	{
		if (values.Length != 256)
		{
			throw new ArgumentException("Must provide exactly 256 colors");
		}
		colorMapType = OVRPlugin.InsightPassthroughColorMapType.MonoToRgba;
		colorMapEditorType = ColorMapEditorType.Custom;
		_stylesHandler.SetMonoToRgbaHandler(values);
		styleDirty = true;
	}

	public void SetColorLut(OVRPassthroughColorLut lut, float weight = 1f)
	{
		if (lut != null && lut.IsValid)
		{
			weight = ClampWeight(weight);
			colorMapType = OVRPlugin.InsightPassthroughColorMapType.ColorLut;
			colorMapEditorType = ColorMapEditorType.Custom;
			_stylesHandler.SetColorLutHandler(lut, weight);
			styleDirty = true;
		}
		else
		{
			Debug.LogError("Trying to set an invalid Color LUT for Passthrough");
		}
	}

	public void SetColorLut(OVRPassthroughColorLut lutSource, OVRPassthroughColorLut lutTarget, float weight)
	{
		if (lutSource != null && lutSource.IsValid && lutTarget != null && lutTarget.IsValid)
		{
			weight = ClampWeight(weight);
			colorMapType = OVRPlugin.InsightPassthroughColorMapType.InterpolatedColorLut;
			colorMapEditorType = ColorMapEditorType.Custom;
			_stylesHandler.SetInterpolatedColorLutHandler(lutSource, lutTarget, weight);
			styleDirty = true;
		}
		else
		{
			Debug.LogError("Trying to set an invalid Color LUT for Passthrough");
		}
	}

	public void SetColorMapControls(float contrast, float brightness = 0f, float posterize = 0f, Gradient gradient = null, ColorMapEditorType colorMapType = ColorMapEditorType.GrayscaleToColor)
	{
		if (colorMapType != ColorMapEditorType.Grayscale && colorMapType != ColorMapEditorType.GrayscaleToColor)
		{
			Debug.LogError("Unsupported color map type specified");
			return;
		}
		colorMapEditorType = colorMapType;
		colorMapEditorContrast = contrast;
		colorMapEditorBrightness = brightness;
		colorMapEditorPosterize = posterize;
		if (colorMapType == ColorMapEditorType.GrayscaleToColor)
		{
			if (gradient != null)
			{
				colorMapEditorGradient = gradient;
			}
			else if (!colorMapEditorGradient.Equals(colorMapNeutralGradient))
			{
				colorMapEditorGradient = CreateNeutralColorMapGradient();
			}
		}
		else if (gradient != null)
		{
			Debug.LogWarning("Gradient parameter is ignored for color map types other than GrayscaleToColor");
		}
	}

	public void SetColorMapMonochromatic(byte[] values)
	{
		if (values.Length != 256)
		{
			throw new ArgumentException("Must provide exactly 256 values");
		}
		colorMapType = OVRPlugin.InsightPassthroughColorMapType.MonoToMono;
		colorMapEditorType = ColorMapEditorType.Custom;
		_stylesHandler.SetMonoToMonoHandler(values);
		styleDirty = true;
	}

	public void SetBrightnessContrastSaturation(float brightness = 0f, float contrast = 0f, float saturation = 0f)
	{
		colorMapType = OVRPlugin.InsightPassthroughColorMapType.BrightnessContrastSaturation;
		colorMapEditorType = ColorMapEditorType.ColorAdjustment;
		colorMapEditorBrightness = brightness;
		colorMapEditorContrast = contrast;
		colorMapEditorSaturation = saturation;
		UpdateColorMapFromControls();
	}

	public void DisableColorMap()
	{
		colorMapEditorType = ColorMapEditorType.None;
	}

	public void SetStyleDirty()
	{
		styleDirty = true;
	}

	private void AddDeferredSurfaceGeometries()
	{
		for (int i = 0; i < deferredSurfaceGameObjects.Count; i++)
		{
			DeferredPassthroughMeshAddition deferredPassthroughMeshAddition = deferredSurfaceGameObjects[i];
			bool flag = false;
			if ((bool)deferredPassthroughMeshAddition.gameObject)
			{
				ulong meshHandle;
				ulong instanceHandle;
				Matrix4x4 localToWorld;
				if (surfaceGameObjects.ContainsKey(deferredPassthroughMeshAddition.gameObject))
				{
					flag = true;
				}
				else if (CreateAndAddMesh(deferredPassthroughMeshAddition.gameObject, out meshHandle, out instanceHandle, out localToWorld))
				{
					surfaceGameObjects.Add(deferredPassthroughMeshAddition.gameObject, new PassthroughMeshInstance
					{
						meshHandle = meshHandle,
						instanceHandle = instanceHandle,
						updateTransform = deferredPassthroughMeshAddition.updateTransform,
						localToWorld = localToWorld
					});
					flag = true;
				}
				else
				{
					Debug.LogWarning("Failed to create internal resources for GameObject added to passthrough surface.");
				}
			}
			if (flag)
			{
				deferredSurfaceGameObjects.RemoveAt(i--);
			}
		}
	}

	private Matrix4x4 GetTransformMatrixForPassthroughSurfaceObject(Matrix4x4 worldFromObj)
	{
		using (new OVRProfilerScope("GetTransformMatrixForPassthroughSurfaceObject"))
		{
			if (!cameraRigInitialized)
			{
				cameraRig = OVRManager.instance.GetComponentInParent<OVRCameraRig>();
				cameraRigInitialized = true;
			}
			Matrix4x4 matrix4x = ((cameraRig != null) ? cameraRig.trackingSpace.worldToLocalMatrix : Matrix4x4.identity);
			return Matrix4x4.Scale(new Vector3(1f, 1f, -1f)) * matrix4x * worldFromObj;
		}
	}

	private bool CreateAndAddMesh(GameObject obj, out ulong meshHandle, out ulong instanceHandle, out Matrix4x4 localToWorld)
	{
		meshHandle = 0uL;
		instanceHandle = 0uL;
		localToWorld = obj.transform.localToWorldMatrix;
		MeshFilter component = obj.GetComponent<MeshFilter>();
		if (component == null)
		{
			Debug.LogError("Passthrough surface GameObject does not have a mesh component.");
			return false;
		}
		Mesh sharedMesh = component.sharedMesh;
		Vector3[] vertices = sharedMesh.vertices;
		int[] triangles = sharedMesh.triangles;
		Matrix4x4 transformMatrixForPassthroughSurfaceObject = GetTransformMatrixForPassthroughSurfaceObject(localToWorld);
		if (!OVRPlugin.CreateInsightTriangleMesh(passthroughOverlay.layerId, vertices, triangles, out meshHandle))
		{
			Debug.LogWarning("Failed to create triangle mesh handle.");
			return false;
		}
		if (!OVRPlugin.AddInsightPassthroughSurfaceGeometry(passthroughOverlay.layerId, meshHandle, transformMatrixForPassthroughSurfaceObject, out instanceHandle))
		{
			Debug.LogWarning("Failed to add mesh to passthrough surface.");
			return false;
		}
		return true;
	}

	private void DestroySurfaceGeometries(bool addBackToDeferredQueue = false)
	{
		foreach (KeyValuePair<GameObject, PassthroughMeshInstance> surfaceGameObject in surfaceGameObjects)
		{
			if (surfaceGameObject.Value.meshHandle != 0L)
			{
				OVRPlugin.DestroyInsightPassthroughGeometryInstance(surfaceGameObject.Value.instanceHandle);
				OVRPlugin.DestroyInsightTriangleMesh(surfaceGameObject.Value.meshHandle);
				if (addBackToDeferredQueue)
				{
					deferredSurfaceGameObjects.Add(new DeferredPassthroughMeshAddition
					{
						gameObject = surfaceGameObject.Key,
						updateTransform = surfaceGameObject.Value.updateTransform
					});
				}
			}
		}
		surfaceGameObjects.Clear();
	}

	private void UpdateSurfaceGeometryTransforms()
	{
		using (new OVRProfilerScope("UpdateSurfaceGeometryTransforms"))
		{
			List<GameObject> list;
			using (new OVRObjectPool.ListScope<GameObject>(out list))
			{
				foreach (KeyValuePair<GameObject, PassthroughMeshInstance> surfaceGameObject in surfaceGameObjects)
				{
					if (surfaceGameObject.Key == null)
					{
						list.Add(surfaceGameObject.Key);
						continue;
					}
					ulong instanceHandle = surfaceGameObject.Value.instanceHandle;
					if (instanceHandle != 0L)
					{
						Matrix4x4 localToWorld = (surfaceGameObject.Value.updateTransform ? surfaceGameObject.Key.transform.localToWorldMatrix : surfaceGameObject.Value.localToWorld);
						UpdateSurfaceGeometryTransform(instanceHandle, localToWorld);
					}
				}
				foreach (GameObject item in list)
				{
					RemoveSurfaceGeometry(item);
				}
			}
		}
	}

	private void UpdateSurfaceGeometryTransform(ulong instanceHandle, Matrix4x4 localToWorld)
	{
		Matrix4x4 transformMatrixForPassthroughSurfaceObject = GetTransformMatrixForPassthroughSurfaceObject(localToWorld);
		using (new OVRProfilerScope("UpdateInsightPassthroughGeometryTransform"))
		{
			if (!OVRPlugin.UpdateInsightPassthroughGeometryTransform(instanceHandle, transformMatrixForPassthroughSurfaceObject))
			{
				Debug.LogWarning("Failed to update a transform of a passthrough surface");
			}
		}
	}

	internal static Gradient CreateNeutralColorMapGradient()
	{
		Gradient gradient = new Gradient();
		gradient.colorKeys = new GradientColorKey[2]
		{
			new GradientColorKey(new Color(0f, 0f, 0f), 0f),
			new GradientColorKey(new Color(1f, 1f, 1f), 1f)
		};
		gradient.alphaKeys = new GradientAlphaKey[2]
		{
			new GradientAlphaKey(1f, 0f),
			new GradientAlphaKey(1f, 1f)
		};
		return gradient;
	}

	private bool HasControlsBasedColorMap()
	{
		if (colorMapEditorType != ColorMapEditorType.Grayscale && colorMapEditorType != ColorMapEditorType.ColorAdjustment && colorMapEditorType != ColorMapEditorType.ColorLut && colorMapEditorType != ColorMapEditorType.InterpolatedColorLut)
		{
			return colorMapEditorType == ColorMapEditorType.GrayscaleToColor;
		}
		return true;
	}

	private void UpdateColorMapFromControls(bool forceUpdate = false)
	{
		bool flag = _settings.brightness != colorMapEditorBrightness || _settings.contrast != colorMapEditorContrast || _settings.posterize != colorMapEditorPosterize || _settings.colorLutSourceTexture != _colorLutSourceTexture || _settings.colorLutTargetTexture != _colorLutTargetTexture || _settings.lutWeight != _lutWeight || _settings.saturation != colorMapEditorSaturation || _settings.flipLutY != _flipLutY;
		bool flag2 = colorMapEditorType == ColorMapEditorType.GrayscaleToColor && !colorMapEditorGradient.Equals(_settings.gradient);
		if ((HasControlsBasedColorMap() && flag) || flag2 || forceUpdate)
		{
			_settings.gradient.CopyFrom(colorMapEditorGradient);
			_settings.brightness = colorMapEditorBrightness;
			_settings.contrast = colorMapEditorContrast;
			_settings.posterize = colorMapEditorPosterize;
			_settings.saturation = colorMapEditorSaturation;
			_settings.lutWeight = _lutWeight;
			_settings.flipLutY = _flipLutY;
			_settings.colorLutSourceTexture = _colorLutSourceTexture;
			_settings.colorLutTargetTexture = _colorLutTargetTexture;
			if (Application.isPlaying)
			{
				_stylesHandler.CurrentStyleHandler.Update(_settings);
				styleDirty = true;
			}
		}
	}

	private void SyncToOverlay()
	{
		passthroughOverlay.currentOverlayType = overlayType;
		passthroughOverlay.compositionDepth = compositionDepth;
		passthroughOverlay.hidden = hidden || IsUserDefinedAndDoesNotContainSurfaceGeometry();
		passthroughOverlay.overridePerLayerColorScaleAndOffset = overridePerLayerColorScaleAndOffset;
		passthroughOverlay.colorScale = colorScale;
		passthroughOverlay.colorOffset = colorOffset;
		if (passthroughOverlay.currentOverlayShape != overlayShape)
		{
			if (passthroughOverlay.layerId > 0)
			{
				Debug.LogWarning("Change to projectionSurfaceType won't take effect until the layer goes through a disable/enable cycle. ");
			}
			if (projectionSurfaceType == ProjectionSurfaceType.Reconstructed)
			{
				Debug.Log("Removing user defined surface geometries");
				DestroySurfaceGeometries();
			}
			passthroughOverlay.currentOverlayShape = overlayShape;
		}
		bool num = passthroughOverlay.enabled;
		passthroughOverlay.enabled = OVRManager.instance != null && OVRManager.instance.isInsightPassthroughEnabled && OVRManager.IsInsightPassthroughInitialized();
		if (num != passthroughOverlay.enabled)
		{
			if (passthroughOverlay.enabled)
			{
				styleDirty = true;
			}
			else
			{
				DestroySurfaceGeometries(addBackToDeferredQueue: true);
			}
		}
	}

	private bool IsUserDefinedAndDoesNotContainSurfaceGeometry()
	{
		if (projectionSurfaceType == ProjectionSurfaceType.UserDefined && deferredSurfaceGameObjects.Count == 0)
		{
			return surfaceGameObjects.Count == 0;
		}
		return false;
	}

	private static float ClampWeight(float weight)
	{
		if (weight < 0f || weight > 1f)
		{
			Debug.LogWarning("Color lut weight should be between in [0, 1] range. Setting it to closest value.");
			weight = Mathf.Clamp01(weight);
		}
		return weight;
	}

	private void Awake()
	{
		foreach (SerializedSurfaceGeometry item in serializedSurfaceGeometry)
		{
			if (!(item.meshFilter == null))
			{
				deferredSurfaceGameObjects.Add(new DeferredPassthroughMeshAddition
				{
					gameObject = item.meshFilter.gameObject,
					updateTransform = item.updateTransform
				});
			}
		}
	}

	private void Update()
	{
		SyncToOverlay();
	}

	private void LateUpdate()
	{
		if (hidden || passthroughOverlay.layerId <= 0)
		{
			return;
		}
		if (projectionSurfaceType == ProjectionSurfaceType.UserDefined)
		{
			UpdateSurfaceGeometryTransforms();
			AddDeferredSurfaceGeometries();
		}
		UpdateColorMapFromControls();
		if (styleDirty)
		{
			if (_stylesHandler.CurrentStyleHandler.IsValid)
			{
				OVRPlugin.SetInsightPassthroughStyle(passthroughOverlay.layerId, CreateOvrPluginStyleObject());
			}
			styleDirty = false;
		}
	}

	private OVRPlugin.InsightPassthroughStyle2 CreateOvrPluginStyleObject()
	{
		OVRPlugin.InsightPassthroughStyle2 style = default(OVRPlugin.InsightPassthroughStyle2);
		style.Flags = (OVRPlugin.InsightPassthroughStyleFlags)7;
		style.TextureOpacityFactor = textureOpacity;
		style.EdgeColor = (edgeRenderingEnabled ? edgeColor.ToColorf() : new OVRPlugin.Colorf
		{
			r = 0f,
			g = 0f,
			b = 0f,
			a = 0f
		});
		style.TextureColorMapType = colorMapType;
		style.TextureColorMapData = IntPtr.Zero;
		style.TextureColorMapDataSize = 0u;
		_stylesHandler.CurrentStyleHandler.ApplyStyleSettings(ref style);
		return style;
	}

	private void OnEnable()
	{
		auxGameObject = new GameObject("OVRPassthroughLayer auxiliary GameObject");
		auxGameObject.transform.parent = base.transform;
		passthroughOverlay = auxGameObject.AddComponent<OVROverlay>();
		passthroughOverlay.currentOverlayShape = overlayShape;
		OVRManager.PassthroughLayerResumed += OnPassthroughLayerResumed;
		SyncToOverlay();
		if (colorMapEditorType != ColorMapEditorType.Custom)
		{
			_stylesHandler.SetStyleHandler(_editorToColorMapType[colorMapEditorType]);
		}
		if (HasControlsBasedColorMap())
		{
			UpdateColorMapFromControls(forceUpdate: true);
		}
		styleDirty = true;
	}

	private void OnDisable()
	{
		OVRManager.PassthroughLayerResumed -= OnPassthroughLayerResumed;
		if (OVRManager.loadedXRDevice == OVRManager.XRDevice.Oculus)
		{
			DestroySurfaceGeometries(addBackToDeferredQueue: true);
		}
		if (auxGameObject != null)
		{
			UnityEngine.Object.Destroy(auxGameObject);
			auxGameObject = null;
			passthroughOverlay = null;
		}
	}

	private void OnDestroy()
	{
		DestroySurfaceGeometries();
	}

	private void OnPassthroughLayerResumed(int layerId)
	{
		if (passthroughOverlay != null && passthroughOverlay.layerId == layerId)
		{
			if (this.PassthroughLayerResumed != null)
			{
				this.PassthroughLayerResumed();
			}
			passthroughLayerResumed?.Invoke(this);
		}
	}
}

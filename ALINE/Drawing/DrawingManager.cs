using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Drawing;

[ExecuteAlways]
[AddComponentMenu("")]
public class DrawingManager : MonoBehaviour
{
	public DrawingData gizmos;

	private static List<IDrawGizmos> gizmoDrawers = new List<IDrawGizmos>();

	private static Dictionary<Type, bool> gizmoDrawerTypes = new Dictionary<Type, bool>();

	private static DrawingManager _instance;

	private bool framePassed;

	private int lastFrameCount = int.MinValue;

	private float lastFrameTime = float.PositiveInfinity;

	private int lastFilterFrame;

	[SerializeField]
	private bool actuallyEnabled;

	private RedrawScope previousFrameRedrawScope;

	public static bool allowRenderToRenderTextures = false;

	public static bool drawToAllCameras = false;

	public static float lineWidthMultiplier = 1f;

	private CommandBuffer commandBuffer;

	[NonSerialized]
	private DetectedRenderPipeline detectedRenderPipeline;

	private HashSet<ScriptableRenderer> scriptableRenderersWithPass = new HashSet<ScriptableRenderer>();

	private AlineURPRenderPassFeature renderPassFeature;

	private static readonly ProfilerMarker MarkerALINE = new ProfilerMarker("ALINE");

	private static readonly ProfilerMarker MarkerCommandBuffer = new ProfilerMarker("Executing command buffer");

	private static readonly ProfilerMarker MarkerFrameTick = new ProfilerMarker("Frame Tick");

	private static readonly ProfilerMarker MarkerFilterDestroyedObjects = new ProfilerMarker("Filter destroyed objects");

	internal static readonly ProfilerMarker MarkerRefreshSelectionCache = new ProfilerMarker("Refresh Selection Cache");

	private static readonly ProfilerMarker MarkerGizmosAllowed = new ProfilerMarker("GizmosAllowed");

	private static readonly ProfilerMarker MarkerDrawGizmos = new ProfilerMarker("DrawGizmos");

	private static readonly ProfilerMarker MarkerSubmitGizmos = new ProfilerMarker("Submit Gizmos");

	private const float NO_DRAWING_TIMEOUT_SECS = 10f;

	private readonly Dictionary<Type, bool> typeToGizmosEnabled = new Dictionary<Type, bool>();

	public static DrawingManager instance
	{
		get
		{
			if (_instance == null)
			{
				Init();
			}
			return _instance;
		}
	}

	public static void Init()
	{
		if (!(_instance != null))
		{
			GameObject gameObject = new GameObject("RetainedGizmos")
			{
				hideFlags = (HideFlags.HideAndDontSave | HideFlags.HideInInspector)
			};
			_instance = gameObject.AddComponent<DrawingManager>();
			if (Application.isPlaying)
			{
				UnityEngine.Object.DontDestroyOnLoad(gameObject);
			}
		}
	}

	private void RefreshRenderPipelineMode()
	{
		if (((RenderPipelineManager.currentPipeline != null) ? RenderPipelineManager.currentPipeline.GetType() : null) == typeof(UniversalRenderPipeline))
		{
			detectedRenderPipeline = DetectedRenderPipeline.URP;
		}
		else
		{
			detectedRenderPipeline = DetectedRenderPipeline.BuiltInOrCustom;
		}
	}

	private void OnEnable()
	{
		if (_instance == null)
		{
			_instance = this;
		}
		if (!(_instance != this))
		{
			actuallyEnabled = true;
			if (gizmos == null)
			{
				gizmos = new DrawingData();
			}
			gizmos.frameRedrawScope = new RedrawScope(gizmos);
			Draw.builder = gizmos.GetBuiltInBuilder();
			Draw.ingame_builder = gizmos.GetBuiltInBuilder(renderInGame: true);
			commandBuffer = new CommandBuffer();
			commandBuffer.name = "ALINE Gizmos";
			Camera.onPostRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPostRender, new Camera.CameraCallback(PostRender));
			RenderPipelineManager.beginContextRendering += BeginContextRendering;
			RenderPipelineManager.beginCameraRendering += BeginCameraRendering;
			RenderPipelineManager.endCameraRendering += EndCameraRendering;
		}
	}

	private void BeginContextRendering(ScriptableRenderContext context, List<Camera> cameras)
	{
		RefreshRenderPipelineMode();
	}

	private void BeginFrameRendering(ScriptableRenderContext context, Camera[] cameras)
	{
		RefreshRenderPipelineMode();
	}

	private void BeginCameraRendering(ScriptableRenderContext context, Camera camera)
	{
		if (detectedRenderPipeline != DetectedRenderPipeline.URP)
		{
			return;
		}
		UniversalAdditionalCameraData universalAdditionalCameraData = camera.GetUniversalAdditionalCameraData();
		if (universalAdditionalCameraData != null)
		{
			ScriptableRenderer scriptableRenderer = universalAdditionalCameraData.scriptableRenderer;
			if (renderPassFeature == null)
			{
				renderPassFeature = ScriptableObject.CreateInstance<AlineURPRenderPassFeature>();
			}
			renderPassFeature.AddRenderPasses(scriptableRenderer);
		}
	}

	private void OnDisable()
	{
		if (actuallyEnabled)
		{
			actuallyEnabled = false;
			commandBuffer.Dispose();
			commandBuffer = null;
			Camera.onPostRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPostRender, new Camera.CameraCallback(PostRender));
			RenderPipelineManager.beginContextRendering -= BeginContextRendering;
			RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;
			RenderPipelineManager.endCameraRendering -= EndCameraRendering;
			if (gizmos != null)
			{
				Draw.builder.DiscardAndDisposeInternal();
				Draw.ingame_builder.DiscardAndDisposeInternal();
				gizmos.ClearData();
			}
			if (renderPassFeature != null)
			{
				UnityEngine.Object.DestroyImmediate(renderPassFeature);
				renderPassFeature = null;
			}
		}
	}

	private void OnEditorUpdate()
	{
		framePassed = true;
		CleanupIfNoCameraRendered();
	}

	private void Update()
	{
		if (actuallyEnabled)
		{
			CleanupIfNoCameraRendered();
		}
	}

	private void CleanupIfNoCameraRendered()
	{
		if (Time.frameCount > lastFrameCount + 1)
		{
			CheckFrameTicking();
			gizmos.PostRenderCleanup();
		}
		if (Time.realtimeSinceStartup - lastFrameTime > 10f)
		{
			Draw.builder.DiscardAndDisposeInternal();
			Draw.ingame_builder.DiscardAndDisposeInternal();
			Draw.builder = gizmos.GetBuiltInBuilder();
			Draw.ingame_builder = gizmos.GetBuiltInBuilder(renderInGame: true);
			lastFrameTime = Time.realtimeSinceStartup;
			RemoveDestroyedGizmoDrawers();
		}
		if (lastFilterFrame - Time.frameCount > 5)
		{
			lastFilterFrame = Time.frameCount;
			RemoveDestroyedGizmoDrawers();
		}
	}

	internal void ExecuteCustomRenderPass(ScriptableRenderContext context, Camera camera)
	{
		commandBuffer.Clear();
		SubmitFrame(camera, new DrawingData.CommandBufferWrapper
		{
			cmd = commandBuffer
		}, usingRenderPipeline: true);
		context.ExecuteCommandBuffer(commandBuffer);
	}

	internal void ExecuteCustomRenderGraphPass(DrawingData.CommandBufferWrapper cmd, Camera camera)
	{
		SubmitFrame(camera, cmd, usingRenderPipeline: true);
	}

	private void EndCameraRendering(ScriptableRenderContext context, Camera camera)
	{
		if (detectedRenderPipeline == DetectedRenderPipeline.BuiltInOrCustom)
		{
			ExecuteCustomRenderPass(context, camera);
		}
	}

	private void PostRender(Camera camera)
	{
		commandBuffer.Clear();
		SubmitFrame(camera, new DrawingData.CommandBufferWrapper
		{
			cmd = commandBuffer
		}, usingRenderPipeline: false);
		Graphics.ExecuteCommandBuffer(commandBuffer);
	}

	private void CheckFrameTicking()
	{
		if (Time.frameCount != lastFrameCount)
		{
			framePassed = true;
			lastFrameCount = Time.frameCount;
			lastFrameTime = Time.realtimeSinceStartup;
			previousFrameRedrawScope = gizmos.frameRedrawScope;
			gizmos.frameRedrawScope = new RedrawScope(gizmos);
			Draw.builder.DisposeInternal();
			Draw.ingame_builder.DisposeInternal();
			Draw.builder = gizmos.GetBuiltInBuilder();
			Draw.ingame_builder = gizmos.GetBuiltInBuilder(renderInGame: true);
		}
		else if (framePassed && Application.isPlaying)
		{
			previousFrameRedrawScope.Draw();
		}
		if (framePassed)
		{
			gizmos.TickFramePreRender();
			framePassed = false;
		}
	}

	internal void SubmitFrame(Camera camera, DrawingData.CommandBufferWrapper cmd, bool usingRenderPipeline)
	{
		bool flag = false;
		bool allowCameraDefault = allowRenderToRenderTextures || drawToAllCameras || camera.targetTexture == null || flag;
		CheckFrameTicking();
		Submit(camera, cmd, usingRenderPipeline, allowCameraDefault);
		gizmos.PostRenderCleanup();
	}

	private bool ShouldDrawGizmos(UnityEngine.Object obj)
	{
		return true;
	}

	private static void RemoveDestroyedGizmoDrawers()
	{
		int num = 0;
		for (int i = 0; i < gizmoDrawers.Count; i++)
		{
			IDrawGizmos drawGizmos = gizmoDrawers[i];
			if ((bool)(drawGizmos as MonoBehaviour))
			{
				gizmoDrawers[num] = drawGizmos;
				num++;
			}
		}
		gizmoDrawers.RemoveRange(num, gizmoDrawers.Count - num);
	}

	private void Submit(Camera camera, DrawingData.CommandBufferWrapper cmd, bool usingRenderPipeline, bool allowCameraDefault)
	{
		bool allowGizmos = false;
		Draw.builder.DisposeInternal();
		Draw.ingame_builder.DisposeInternal();
		gizmos.Render(camera, allowGizmos, cmd, allowCameraDefault);
		Draw.builder = gizmos.GetBuiltInBuilder();
		Draw.ingame_builder = gizmos.GetBuiltInBuilder(renderInGame: true);
	}

	public static void Register(IDrawGizmos item)
	{
		Type type = item.GetType();
		if (!gizmoDrawerTypes.TryGetValue(type, out var value))
		{
			BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			MethodInfo obj = type.GetMethod("DrawGizmos", bindingAttr) ?? type.GetMethod("Pathfinding.Drawing.IDrawGizmos.DrawGizmos", bindingAttr) ?? type.GetMethod("Drawing.IDrawGizmos.DrawGizmos", bindingAttr);
			if (obj == null)
			{
				throw new Exception("Could not find the DrawGizmos method in type " + type.Name);
			}
			value = obj.DeclaringType != typeof(MonoBehaviourGizmos);
			gizmoDrawerTypes[type] = value;
		}
		if (value)
		{
			gizmoDrawers.Add(item);
		}
	}

	public static CommandBuilder GetBuilder(bool renderInGame = false)
	{
		return instance.gizmos.GetBuilder(renderInGame);
	}

	public static CommandBuilder GetBuilder(RedrawScope redrawScope, bool renderInGame = false)
	{
		return instance.gizmos.GetBuilder(redrawScope, renderInGame);
	}

	public static CommandBuilder GetBuilder(DrawingData.Hasher hasher, RedrawScope redrawScope = default(RedrawScope), bool renderInGame = false)
	{
		return instance.gizmos.GetBuilder(hasher, redrawScope, renderInGame);
	}

	public static RedrawScope GetRedrawScope()
	{
		RedrawScope result = new RedrawScope(instance.gizmos);
		result.DrawUntilDispose();
		return result;
	}
}

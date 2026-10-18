using System;
using System.Collections.Generic;
using CustomMapSupport;
using UnityEngine;
using UnityEngine.AI;

namespace GT_CustomMapSupportRuntime;

public class Constants
{
	public enum CMSGameModeType
	{
		Casual,
		Infection,
		HuntDown,
		Paintbrawl,
		Ambush,
		FreezeTag,
		Ghost,
		Custom,
		Count
	}

	public static readonly int customMapSupportVersion = 5;

	public static readonly int minRopeLength = 3;

	public static readonly int maxRopeLength = 31;

	public static readonly int storeATMLimit = 1;

	public static readonly int atmCreatorCodeSizeLimit = 10;

	public static readonly int storeDisplayStandLimit = 12;

	public static readonly int storeCheckoutCounterLimit = 2;

	public static readonly int storeTryOnConsoleLimit = 2;

	public static readonly int storeTryOnAreaLimit = 2;

	public static readonly float storeTryOnAreaVolumeLimit = 64f;

	public static readonly float minTeleportDistFromStorePlaceholder = 2f;

	public static readonly int aiAgentLimit = 100;

	public static readonly int leafGliderLimit = 16;

	public static readonly Vector3 AccessDoorWorldPosition = new Vector3(0f, -11.098f, 2.9295f);

	public static readonly List<Type> componentAllowList = new List<Type>
	{
		typeof(MeshRenderer),
		typeof(Transform),
		typeof(MeshFilter),
		typeof(MeshRenderer),
		typeof(Collider),
		typeof(BoxCollider),
		typeof(SphereCollider),
		typeof(CapsuleCollider),
		typeof(MeshCollider),
		typeof(Light),
		typeof(ReflectionProbe),
		typeof(AudioSource),
		typeof(Animator),
		typeof(SkinnedMeshRenderer),
		typeof(TextMesh),
		typeof(ParticleSystem),
		typeof(ParticleSystemRenderer),
		typeof(RectTransform),
		typeof(SpriteRenderer),
		typeof(BillboardRenderer),
		typeof(Canvas),
		typeof(CanvasRenderer),
		typeof(Rigidbody),
		typeof(TrailRenderer),
		typeof(LineRenderer),
		typeof(Camera),
		typeof(NavMesh),
		typeof(NavMeshAgent),
		typeof(NavMeshObstacle),
		typeof(HingeJoint),
		typeof(ConstantForce),
		typeof(LODGroup),
		typeof(MapDescriptor),
		typeof(AccessDoorPlaceholder),
		typeof(MapOrientationPoint),
		typeof(SurfaceOverrideSettings),
		typeof(TeleporterSettings),
		typeof(TagZoneSettings),
		typeof(LuauTriggerSettings),
		typeof(MapBoundarySettings),
		typeof(ObjectActivationTriggerSettings),
		typeof(LoadZoneSettings),
		typeof(GTObjectPlaceholder),
		typeof(CMSZoneShaderSettings),
		typeof(ZoneShaderTriggerSettings),
		typeof(MultiPartFire),
		typeof(HandHoldSettings),
		typeof(CustomMapEjectButtonSettings),
		typeof(BezierSpline),
		typeof(UberShaderDynamicLight),
		typeof(MapEntity),
		typeof(GrabbableEntity),
		typeof(AIAgent),
		typeof(AISpawnManager),
		typeof(MapSpawnPoint),
		typeof(MapSpawnManager),
		typeof(AISpawnPoint),
		typeof(RopeSwingSegment),
		typeof(ZiplineSegment),
		typeof(PlayAnimationTriggerSettings),
		typeof(SurfaceMoverSettings),
		typeof(MovingSurfaceSettings),
		typeof(CustomMapReviveStation),
		typeof(BasicGravityZoneSettings),
		typeof(ConsensusGravityZoneSettings),
		typeof(TorusZoneSettings),
		typeof(PlanetZoneSettings),
		typeof(CubicPlanetZoneSettings),
		typeof(SizeChangerSettings),
		typeof(MonkeGravityControllerSettings)
	};

	public static readonly List<string> componentTypeStringAllowList = new List<string>
	{
		"UniversalAdditionalLightData", "BakerySkyLight", "BakeryDirectLight", "BakeryPointLight", "ftLightmapsStorage", "BakeryAlwaysRender", "BakeryLightMesh", "BakeryLightmapGroupSelector", "BakeryPackAsSingleSquare", "BakerySector",
		"BakeryVolume", "BakeryLightmapGroup", "ProBuilderMesh", "TMP_Text", "TMPro.TextMeshPro", "TMPro.TextMeshProUGUI", "UnityEngine.UI.CanvasScaler", "UnityEngine.UI.GraphicRaycaster", "UnityEngine.Halo", "UnityEngine.Rendering.LensFlareComponentSRP",
		"UnityEngine.Rendering.Universal.UniversalAdditionalCameraData", "Unity.AI.Navigation.NavMeshModifier", "Unity.AI.Navigation.NavMeshSurface", "Unity.AI.Navigation.NavMeshSurfaceVolume", "Unity.AI.Navigation.NavMeshLink"
	};

	public static readonly List<string> componentTypeStringsToStripPreExport = new List<string> { "ProBuilderShape", "PolyShape", "InspectorNote" };
}

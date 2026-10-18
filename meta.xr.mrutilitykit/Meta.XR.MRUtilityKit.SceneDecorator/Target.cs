using System;

namespace Meta.XR.MRUtilityKit.SceneDecorator;

[Flags]
public enum Target
{
	GLOBAL_MESH = 1,
	RESERVED_MESH = 2,
	PHYSICS_LAYERS = 4,
	CUSTOM_COLLIDERS = 8,
	CUSTOM_TAGS = 0x10,
	SCENE_ANCHORS = 0x20
}

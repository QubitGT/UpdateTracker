using UnityEngine;

namespace GT_CustomMapSupportRuntime;

public class MapEntity : MonoBehaviour
{
	[Tooltip("If \"IsTemplate\" is enabled, this Map Entity will be used by the MapSpawnManager to create duplicate Map Entities of the same \"entityTypeId\". Template Map Entities will not be created when the map loads.")]
	public bool isTemplate;

	[Tooltip("\"EntityTypeID\" is used to distinguish each Map Entity that the MapSpawnManager can create. Make sure each Map Entity with \"IsTemplate\" set to TRUE has a unique \"EntityTypeID\".")]
	public byte entityTypeId;

	[Tooltip("The \"LuaEntityID\" can be used in Luau scripts with the \"findPrePlacedAIAgentByID\" and\"findPrePlacedGrabbableByID\" functions to find your pre-placed Map Entities after the map is loaded.")]
	public short lua_EntityID;

	public virtual long GetPackedCreateData()
	{
		return 0L;
	}
}

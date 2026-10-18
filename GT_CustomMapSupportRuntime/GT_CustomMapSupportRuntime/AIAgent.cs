using System;
using System.Collections.Generic;
using UnityEngine;

namespace GT_CustomMapSupportRuntime;

public class AIAgent : MapEntity
{
	[HideInInspector]
	[Obsolete("Use MapEntity.entityTypeId instead")]
	public byte enemyTypeId;

	[Tooltip("\"NavAgentType\" determines how the NavAgent will interact with the NavMesh.\nCheck the settings for each Nav Agent type in the \"Window > AI > Navigation\" window on the \"Agents\" tab to determine which agent type best fits your Agent.\nEnsure your Scene contains a baked navmesh for the corresponding agent type.\n\nPlease note that any changes to the values for the agent types as well as the addition of any new agent types will be ignored once your map is loaded in-game")]
	public NavAgentType navAgentType;

	[Tooltip("\"MovementSpeed\" determines the max movement speed of the agent.")]
	public float movementSpeed;

	[Tooltip("\"Acceleration\" determines how quickly the agent can get to max speed.")]
	public float acceleration;

	[Tooltip("\"TurnSpeed\" determines how quickly the agent can turn.")]
	public float turnSpeed;

	[Tooltip("If checked, this agent can target players who have already been tagged in non-custom game modes. If unchecked, this agent will NOT target tagged players in non-custom game modes.")]
	public bool allowTargetingTaggedPlayers;

	[Tooltip("\"SightOffset\" determines from what point raycasts will begin. It is the offset from the position of the AIAgent's transform.")]
	public Vector3 sightOffset;

	[Tooltip("\"SightFOV\" is the field of view for the agent when searching for players. 360 max")]
	[Range(0f, 360f)]
	public float sightFOV;

	[Tooltip("\"SightDist\" determines how close a player must be to the agent before it will consider them as a target.")]
	public float sightDist;

	[Tooltip("\"LoseSightDist\" sets the distance at which an agent will lose sight of a player and stop targeting them.")]
	public float loseSightDist;

	[Tooltip("If checked, this agent will continue moving to their chase target's last known position.\nIf unchecked, this agent will stop moving as soon as it loses sight of their chase target.")]
	public bool rememberLoseSightPosition = true;

	[Tooltip("\"StopDist\" sets how close an agent will come to a player when chasing.\nSetting this too small may result in players getting stuck inside agent colliders.\"")]
	public float stopDist;

	[Tooltip("\"AttackType\" Determines how a hit from this Agent will be handled.\nTag - Agent won't deal damage to players when attacking and will Tag them instead. In the Custom Game Mode this will send the \"taggedByAI\" event to your Luau script.\nUseGT - Will deal damage to players using GT's built-in systems. Allows you to make use of GT's health, death, and revive systems.\nUseLuau - Sends the \"playerHit\" event to Luau with the damage amount. Allows you to determine what happens with damage, but you won't be able to use GT's built-in systems")]
	public AttackType attackType;

	[Tooltip("\"AttackDist\" is the distance at which an agent will start attacking a player.")]
	public float attackDist;

	[Tooltip("If checked, the Attack behavior will be driven by Trigger colliders attached to this Agent.\n The design intention for these colliders is that they are small and attached to the \"damaging\" parts of your agent like their hands or weapon. If they are too big, attacking may not work as expected.\n\n If unchecked, the Attack behavior will be driven solely by the target being within the specified Attack Distance.")]
	public bool useColliders;

	[Tooltip("If checked, this Agent will immediately stop moving when starting an attack. If unchecked, the Agent will finish it's active move request while it starts attacking.")]
	public bool stopMovingToAttack;

	[Tooltip("\"DamageAmount\" is how much damage the agent does per attack")]
	public float damageAmount;

	[Tooltip("\"TimeBetweenAttacks\" is how much time (in seconds) should there be between attacks.")]
	public float timeBetweenAttacks;

	[Tooltip("\"AttackAnimName\" is the name of the Attack state in the Animation Controller(s) that will be activated when the agent attacks.")]
	public string attackAnimName = "Attack";

	[Tooltip("\"AnimBlendTime\" is how much time (in seconds) it should take to blend into the Attack animation.\nSetting this to 0 means the Animation Controller(s) will immediately switch to the Attackanimation state\"")]
	public float animBlendTime;

	[Tooltip("\"DamageDelayAfterPlayingAnim\" is how much time (in seconds) to delay the attack damage event after starting the attack animation. This is only used if \"UseColliders\" is unchecked. This is useful when your attack animation contains a long windup.")]
	public float damageDelayAfterPlayingAnim;

	[Tooltip("The \"AgentBehaviours\" list determines what behaviours an agent will use and what priority each behaviour is be given.\n\nPriority is based of index in the list. If the first behaviour in the list can execute, all behaviours after it will be skipped.\n\nOnly one instance of a behaviour will be used, any duplicates will be ignored.\n\nLeaving this empty will result in the agent doing nothing unless specifically told by a LUAU script.\"")]
	public List<AgentBehaviours> agentBehaviours = new List<AgentBehaviours>();

	[Obsolete("Use MapEntity.lua_EntityID instead")]
	public short lua_AgentID;

	private static List<AgentBehaviours> validateBehaviors = new List<AgentBehaviours>();

	private static List<int> invalidEntries = new List<int>();

	public void OnValidate()
	{
		if (agentBehaviours.Count > 3)
		{
			agentBehaviours.RemoveRange(3, agentBehaviours.Count - 3);
		}
		try
		{
			validateBehaviors.Clear();
			invalidEntries.Clear();
			if (agentBehaviours.Count == 0)
			{
				return;
			}
			for (int i = 0; i < agentBehaviours.Count; i++)
			{
				if (!validateBehaviors.Contains(agentBehaviours[i]))
				{
					validateBehaviors.Add(agentBehaviours[i]);
				}
				else
				{
					invalidEntries.Add(i);
				}
			}
			foreach (int invalidEntry in invalidEntries)
			{
				for (int j = 0; j < 3; j++)
				{
					if (!validateBehaviors.Contains((AgentBehaviours)j))
					{
						agentBehaviours[invalidEntry] = (AgentBehaviours)j;
						validateBehaviors.Add(agentBehaviours[invalidEntry]);
						break;
					}
				}
			}
		}
		catch (Exception message)
		{
			Debug.Log(message);
		}
	}

	public override long GetPackedCreateData()
	{
		bool hasInstance = AISpawnManager.HasInstance;
		return (long)(hasInstance ? enemyTypeId : entityTypeId) + (long)((hasInstance ? lua_AgentID : lua_EntityID) << 8);
	}

	public static void UnpackCreateData(long data, out byte entityTypeID, out short luaAgentID)
	{
		entityTypeID = (byte)(data & 0xFF);
		luaAgentID = (short)((data >> 8) & 0xFFFF);
	}
}

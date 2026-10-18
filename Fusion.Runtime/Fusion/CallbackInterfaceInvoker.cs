using System;

namespace Fusion;

internal static class CallbackInterfaceInvoker
{
	public static void IBeforeCopyPreviousState(SimulationBehaviourUpdater updater)
	{
		try
		{
			int callbackCount = updater.GetCallbackCount(typeof(IBeforeCopyPreviousState));
			for (int i = 0; i < callbackCount; i++)
			{
				SimulationBehaviour head;
				using (updater.GetCallbackHead(typeof(IBeforeCopyPreviousState), i, out head))
				{
					while (BehaviourUtils.IsNotNull(head))
					{
						SimulationBehaviour next = head.Next;
						if (head.CanReceiveSimulationCallback)
						{
							((IBeforeCopyPreviousState)head).BeforeCopyPreviousState();
						}
						head = next;
					}
				}
			}
		}
		catch (Exception error)
		{
			InternalLogStreams.LogException?.Log(error);
		}
	}

	public static void IBeforeClientPredictionReset(SimulationBehaviourUpdater updater)
	{
		try
		{
			int callbackCount = updater.GetCallbackCount(typeof(IBeforeClientPredictionReset));
			for (int i = 0; i < callbackCount; i++)
			{
				SimulationBehaviour head;
				using (updater.GetCallbackHead(typeof(IBeforeClientPredictionReset), i, out head))
				{
					while (BehaviourUtils.IsNotNull(head))
					{
						SimulationBehaviour next = head.Next;
						if (head.CanReceiveSimulationCallback)
						{
							((IBeforeClientPredictionReset)head).BeforeClientPredictionReset();
						}
						head = next;
					}
				}
			}
		}
		catch (Exception error)
		{
			InternalLogStreams.LogException?.Log(error);
		}
	}

	public static void IAfterClientPredictionReset(SimulationBehaviourUpdater updater)
	{
		try
		{
			int callbackCount = updater.GetCallbackCount(typeof(IAfterClientPredictionReset));
			for (int i = 0; i < callbackCount; i++)
			{
				SimulationBehaviour head;
				using (updater.GetCallbackHead(typeof(IAfterClientPredictionReset), i, out head))
				{
					while (BehaviourUtils.IsNotNull(head))
					{
						SimulationBehaviour next = head.Next;
						if (head.CanReceiveSimulationCallback)
						{
							((IAfterClientPredictionReset)head).AfterClientPredictionReset();
						}
						head = next;
					}
				}
			}
		}
		catch (Exception error)
		{
			InternalLogStreams.LogException?.Log(error);
		}
	}

	public static void IBeforeUpdateRemotePrefabs(SimulationBehaviourUpdater updater)
	{
		try
		{
			int callbackCount = updater.GetCallbackCount(typeof(IBeforeUpdateRemotePrefabs));
			for (int i = 0; i < callbackCount; i++)
			{
				SimulationBehaviour head;
				using (updater.GetCallbackHead(typeof(IBeforeUpdateRemotePrefabs), i, out head))
				{
					while (BehaviourUtils.IsNotNull(head))
					{
						SimulationBehaviour next = head.Next;
						if (head.CanReceiveSimulationCallback)
						{
							((IBeforeUpdateRemotePrefabs)head).BeforeUpdateRemotePrefabs();
						}
						head = next;
					}
				}
			}
		}
		catch (Exception error)
		{
			InternalLogStreams.LogException?.Log(error);
		}
	}

	public static void IAfterUpdateRemotePrefabs(SimulationBehaviourUpdater updater)
	{
		try
		{
			int callbackCount = updater.GetCallbackCount(typeof(IAfterUpdateRemotePrefabs));
			for (int i = 0; i < callbackCount; i++)
			{
				SimulationBehaviour head;
				using (updater.GetCallbackHead(typeof(IAfterUpdateRemotePrefabs), i, out head))
				{
					while (BehaviourUtils.IsNotNull(head))
					{
						SimulationBehaviour next = head.Next;
						if (head.CanReceiveSimulationCallback)
						{
							((IAfterUpdateRemotePrefabs)head).AfterUpdateRemotePrefabs();
						}
						head = next;
					}
				}
			}
		}
		catch (Exception error)
		{
			InternalLogStreams.LogException?.Log(error);
		}
	}

	public static void IBeforeTick(SimulationBehaviourUpdater updater)
	{
		try
		{
			int callbackCount = updater.GetCallbackCount(typeof(IBeforeTick));
			for (int i = 0; i < callbackCount; i++)
			{
				SimulationBehaviour head;
				using (updater.GetCallbackHead(typeof(IBeforeTick), i, out head))
				{
					while (BehaviourUtils.IsNotNull(head))
					{
						SimulationBehaviour next = head.Next;
						if (head.CanReceiveSimulationCallback)
						{
							((IBeforeTick)head).BeforeTick();
						}
						head = next;
					}
				}
			}
		}
		catch (Exception error)
		{
			InternalLogStreams.LogException?.Log(error);
		}
	}

	public static void IAfterTick(SimulationBehaviourUpdater updater)
	{
		try
		{
			int callbackCount = updater.GetCallbackCount(typeof(IAfterTick));
			for (int i = 0; i < callbackCount; i++)
			{
				SimulationBehaviour head;
				using (updater.GetCallbackHead(typeof(IAfterTick), i, out head))
				{
					while (BehaviourUtils.IsNotNull(head))
					{
						SimulationBehaviour next = head.Next;
						if (head.CanReceiveSimulationCallback)
						{
							((IAfterTick)head).AfterTick();
						}
						head = next;
					}
				}
			}
		}
		catch (Exception error)
		{
			InternalLogStreams.LogException?.Log(error);
		}
	}

	public static void IBeforeAllTicks(SimulationBehaviourUpdater updater, bool resimulation, int tickCount)
	{
		try
		{
			int callbackCount = updater.GetCallbackCount(typeof(IBeforeAllTicks));
			for (int i = 0; i < callbackCount; i++)
			{
				SimulationBehaviour head;
				using (updater.GetCallbackHead(typeof(IBeforeAllTicks), i, out head))
				{
					while (BehaviourUtils.IsNotNull(head))
					{
						SimulationBehaviour next = head.Next;
						if (head.CanReceiveSimulationCallback)
						{
							((IBeforeAllTicks)head).BeforeAllTicks(resimulation, tickCount);
						}
						head = next;
					}
				}
			}
		}
		catch (Exception error)
		{
			InternalLogStreams.LogException?.Log(error);
		}
	}

	public static void IAfterAllTicks(SimulationBehaviourUpdater updater, bool resimulation, int tickCount)
	{
		try
		{
			int callbackCount = updater.GetCallbackCount(typeof(IAfterAllTicks));
			for (int i = 0; i < callbackCount; i++)
			{
				SimulationBehaviour head;
				using (updater.GetCallbackHead(typeof(IAfterAllTicks), i, out head))
				{
					while (BehaviourUtils.IsNotNull(head))
					{
						SimulationBehaviour next = head.Next;
						if (head.CanReceiveSimulationCallback)
						{
							((IAfterAllTicks)head).AfterAllTicks(resimulation, tickCount);
						}
						head = next;
					}
				}
			}
		}
		catch (Exception error)
		{
			InternalLogStreams.LogException?.Log(error);
		}
	}

	public static void IBeforeSimulation(SimulationBehaviourUpdater updater, int forwardTickCount)
	{
		try
		{
			int callbackCount = updater.GetCallbackCount(typeof(IBeforeSimulation));
			for (int i = 0; i < callbackCount; i++)
			{
				SimulationBehaviour head;
				using (updater.GetCallbackHead(typeof(IBeforeSimulation), i, out head))
				{
					while (BehaviourUtils.IsNotNull(head))
					{
						SimulationBehaviour next = head.Next;
						if (head.CanReceiveSimulationCallback)
						{
							((IBeforeSimulation)head).BeforeSimulation(forwardTickCount);
						}
						head = next;
					}
				}
			}
		}
		catch (Exception error)
		{
			InternalLogStreams.LogException?.Log(error);
		}
	}

	public static void IPlayerJoined(SimulationBehaviourUpdater updater, PlayerRef player)
	{
		try
		{
			int callbackCount = updater.GetCallbackCount(typeof(IPlayerJoined));
			for (int i = 0; i < callbackCount; i++)
			{
				SimulationBehaviour head;
				using (updater.GetCallbackHead(typeof(IPlayerJoined), i, out head))
				{
					while (BehaviourUtils.IsNotNull(head))
					{
						SimulationBehaviour next = head.Next;
						if (head.CanReceiveSimulationCallback)
						{
							((IPlayerJoined)head).PlayerJoined(player);
						}
						head = next;
					}
				}
			}
		}
		catch (Exception error)
		{
			InternalLogStreams.LogException?.Log(error);
		}
	}

	public static void IPlayerLeft(SimulationBehaviourUpdater updater, PlayerRef player)
	{
		try
		{
			int callbackCount = updater.GetCallbackCount(typeof(IPlayerLeft));
			for (int i = 0; i < callbackCount; i++)
			{
				SimulationBehaviour head;
				using (updater.GetCallbackHead(typeof(IPlayerLeft), i, out head))
				{
					while (BehaviourUtils.IsNotNull(head))
					{
						SimulationBehaviour next = head.Next;
						if (head.CanReceiveSimulationCallback)
						{
							((IPlayerLeft)head).PlayerLeft(player);
						}
						head = next;
					}
				}
			}
		}
		catch (Exception error)
		{
			InternalLogStreams.LogException?.Log(error);
		}
	}

	public static void IBeforeHitboxRegistration(SimulationBehaviourUpdater updater)
	{
		try
		{
			int callbackCount = updater.GetCallbackCount(typeof(IBeforeHitboxRegistration));
			for (int i = 0; i < callbackCount; i++)
			{
				SimulationBehaviour head;
				using (updater.GetCallbackHead(typeof(IBeforeHitboxRegistration), i, out head))
				{
					while (BehaviourUtils.IsNotNull(head))
					{
						SimulationBehaviour next = head.Next;
						if (head.CanReceiveSimulationCallback)
						{
							((IBeforeHitboxRegistration)head).BeforeHitboxRegistration();
						}
						head = next;
					}
				}
			}
		}
		catch (Exception error)
		{
			InternalLogStreams.LogException?.Log(error);
		}
	}

	public static void IAfterUpdate(SimulationBehaviourUpdater updater)
	{
		try
		{
			int callbackCount = updater.GetCallbackCount(typeof(IAfterUpdate));
			for (int i = 0; i < callbackCount; i++)
			{
				SimulationBehaviour head;
				using (updater.GetCallbackHead(typeof(IAfterUpdate), i, out head))
				{
					while (BehaviourUtils.IsNotNull(head))
					{
						SimulationBehaviour next = head.Next;
						if (head.CanReceiveSimulationCallback)
						{
							((IAfterUpdate)head).AfterUpdate();
						}
						head = next;
					}
				}
			}
		}
		catch (Exception error)
		{
			InternalLogStreams.LogException?.Log(error);
		}
	}

	public static void IBeforeUpdate(SimulationBehaviourUpdater updater)
	{
		try
		{
			int callbackCount = updater.GetCallbackCount(typeof(IBeforeUpdate));
			for (int i = 0; i < callbackCount; i++)
			{
				SimulationBehaviour head;
				using (updater.GetCallbackHead(typeof(IBeforeUpdate), i, out head))
				{
					while (BehaviourUtils.IsNotNull(head))
					{
						SimulationBehaviour next = head.Next;
						if (head.CanReceiveSimulationCallback)
						{
							((IBeforeUpdate)head).BeforeUpdate();
						}
						head = next;
					}
				}
			}
		}
		catch (Exception error)
		{
			InternalLogStreams.LogException?.Log(error);
		}
	}

	public static void IAfterRender(SimulationBehaviourUpdater updater)
	{
		try
		{
			int callbackCount = updater.GetCallbackCount(typeof(IAfterRender));
			for (int i = 0; i < callbackCount; i++)
			{
				SimulationBehaviour head;
				using (updater.GetCallbackHead(typeof(IAfterRender), i, out head))
				{
					while (BehaviourUtils.IsNotNull(head))
					{
						SimulationBehaviour next = head.Next;
						if (head.CanReceiveRenderCallback)
						{
							((IAfterRender)head).AfterRender();
						}
						head = next;
					}
				}
			}
		}
		catch (Exception error)
		{
			InternalLogStreams.LogException?.Log(error);
		}
	}

	public static void ISceneLoadDone(SimulationBehaviourUpdater updater, in SceneLoadDoneArgs sceneLoadDoneArgs)
	{
		try
		{
			int callbackCount = updater.GetCallbackCount(typeof(ISceneLoadDone));
			for (int i = 0; i < callbackCount; i++)
			{
				SimulationBehaviour head;
				using (updater.GetCallbackHead(typeof(ISceneLoadDone), i, out head))
				{
					while (BehaviourUtils.IsNotNull(head))
					{
						SimulationBehaviour next = head.Next;
						if (head.CanReceiveSimulationCallback)
						{
							((ISceneLoadDone)head).SceneLoadDone(in sceneLoadDoneArgs);
						}
						head = next;
					}
				}
			}
		}
		catch (Exception error)
		{
			InternalLogStreams.LogException?.Log(error);
		}
	}

	public static void ISceneLoadStart(SimulationBehaviourUpdater updater, SceneRef sceneRef)
	{
		try
		{
			int callbackCount = updater.GetCallbackCount(typeof(ISceneLoadStart));
			for (int i = 0; i < callbackCount; i++)
			{
				SimulationBehaviour head;
				using (updater.GetCallbackHead(typeof(ISceneLoadStart), i, out head))
				{
					while (BehaviourUtils.IsNotNull(head))
					{
						SimulationBehaviour next = head.Next;
						if (head.CanReceiveSimulationCallback)
						{
							((ISceneLoadStart)head).SceneLoadStart(sceneRef);
						}
						head = next;
					}
				}
			}
		}
		catch (Exception error)
		{
			InternalLogStreams.LogException?.Log(error);
		}
	}

	public static void IAfterHostMigration(SimulationBehaviourUpdater updater)
	{
		try
		{
			int callbackCount = updater.GetCallbackCount(typeof(IAfterHostMigration));
			for (int i = 0; i < callbackCount; i++)
			{
				SimulationBehaviour head;
				using (updater.GetCallbackHead(typeof(IAfterHostMigration), i, out head))
				{
					while (BehaviourUtils.IsNotNull(head))
					{
						SimulationBehaviour next = head.Next;
						if (head.CanReceiveSimulationCallback)
						{
							((IAfterHostMigration)head).AfterHostMigration();
						}
						head = next;
					}
				}
			}
		}
		catch (Exception error)
		{
			InternalLogStreams.LogException?.Log(error);
		}
	}
}

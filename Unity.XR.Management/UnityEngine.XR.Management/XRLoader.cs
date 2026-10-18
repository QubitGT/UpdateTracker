using System.Collections.Generic;
using UnityEngine.Rendering;

namespace UnityEngine.XR.Management;

public abstract class XRLoader : ScriptableObject
{
	public virtual bool Initialize()
	{
		return true;
	}

	public virtual bool Start()
	{
		return true;
	}

	public virtual bool Stop()
	{
		return true;
	}

	public virtual bool Deinitialize()
	{
		return true;
	}

	public abstract T GetLoadedSubsystem<T>() where T : class, ISubsystem;

	public virtual List<GraphicsDeviceType> GetSupportedGraphicsDeviceTypes(bool buildingPlayer)
	{
		return new List<GraphicsDeviceType>();
	}
}

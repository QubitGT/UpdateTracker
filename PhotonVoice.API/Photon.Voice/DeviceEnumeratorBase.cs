using System;
using System.Collections;
using System.Collections.Generic;

namespace Photon.Voice;

public abstract class DeviceEnumeratorBase : IDeviceEnumerator, IDisposable, IEnumerable<DeviceInfo>, IEnumerable
{
	protected List<DeviceInfo> devices = new List<DeviceInfo>();

	protected ILogger logger;

	public virtual bool IsSupported => true;

	public virtual string Error { get; protected set; }

	public DeviceEnumeratorBase(ILogger logger)
	{
		this.logger = logger;
	}

	public IEnumerator<DeviceInfo> GetEnumerator()
	{
		return devices.GetEnumerator();
	}

	public abstract void Refresh();

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public abstract void Dispose();
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Oculus.Interaction.PoseDetection.Debug;

public abstract class ActiveStateModel<TActiveState> : IActiveStateModel where TActiveState : class, IActiveState
{
	public async Task<IEnumerable<IActiveState>> GetChildrenAsync(IActiveState activeState)
	{
		if (activeState is TActiveState instance)
		{
			return await GetChildrenAsync(instance);
		}
		return Enumerable.Empty<IActiveState>();
	}

	protected abstract Task<IEnumerable<IActiveState>> GetChildrenAsync(TActiveState instance);

	[Obsolete("Use async version of this method", true)]
	public IEnumerable<IActiveState> GetChildren(IActiveState activeState)
	{
		throw new NotImplementedException();
	}

	[Obsolete("Use async version of this method", true)]
	protected virtual IEnumerable<IActiveState> GetChildren(TActiveState activeState)
	{
		throw new NotImplementedException();
	}
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oculus.Interaction.PoseDetection.Debug;

public interface IActiveStateModel
{
	[Obsolete("Use async version of this method", true)]
	IEnumerable<IActiveState> GetChildren(IActiveState activeState)
	{
		throw new NotImplementedException();
	}

	Task<IEnumerable<IActiveState>> GetChildrenAsync(IActiveState activeState);
}

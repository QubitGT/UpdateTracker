using System;
using UnityEngine;

namespace Oculus.Interaction.Throw;

[Obsolete("Use Grabbable instead")]
public interface IThrowVelocityCalculator
{
	ReleaseVelocityInformation CalculateThrowVelocity(Transform objectThrown);
}

using System;
using System.Collections.Generic;

namespace Oculus.Interaction.Throw;

[Obsolete("Use IThrowVelocityCalculator directly instead")]
public interface IVelocityCalculator : IThrowVelocityCalculator
{
	float UpdateFrequency { get; }

	event Action<List<ReleaseVelocityInformation>> WhenThrowVelocitiesChanged;

	event Action<ReleaseVelocityInformation> WhenNewSampleAvailable;

	IReadOnlyList<ReleaseVelocityInformation> LastThrowVelocities();

	void SetUpdateFrequency(float frequency);
}

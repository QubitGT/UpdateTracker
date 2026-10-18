using System;
using Fusion.Sockets;
using UnityEngine;

namespace Fusion;

[Serializable]
public class NetworkSimulationConfiguration
{
	[InlineHelp]
	public bool Enabled;

	[InlineHelp]
	[DrawIf("Enabled", Hide = true)]
	public NetConfigSimulationOscillator.WaveShape DelayShape = NetConfigSimulationOscillator.WaveShape.Noise;

	[InlineHelp]
	[DrawIf("Enabled", Hide = true)]
	[Unit(Units.Seconds)]
	[RangeEx(0.0, 0.5)]
	public double DelayMin = 0.15;

	[InlineHelp]
	[DrawIf("Enabled", Hide = true)]
	[Unit(Units.Seconds)]
	[RangeEx(0.0, 0.5)]
	public double DelayMax = 0.15;

	[InlineHelp]
	[DrawIf("Enabled", Hide = true)]
	[Unit(Units.Seconds)]
	[RangeEx(0.0, 10.0)]
	public double DelayPeriod = 0.0;

	[InlineHelp]
	[DrawIf("Enabled", Hide = true)]
	[Unit(Units.Seconds)]
	[RangeEx(0.0, 1.0)]
	public double DelayThreshold = 0.0;

	[InlineHelp]
	[DrawIf("Enabled", Hide = true)]
	[Unit(Units.Seconds)]
	[RangeEx(0.0, 1.0)]
	public double AdditionalJitter = 0.05;

	[InlineHelp]
	[DrawIf("Enabled", Hide = true)]
	[Space]
	public NetConfigSimulationOscillator.WaveShape LossChanceShape = NetConfigSimulationOscillator.WaveShape.Noise;

	[InlineHelp]
	[DrawIf("Enabled", Hide = true)]
	[Unit(Units.NormalizedPercentage)]
	[RangeEx(0.0, 1.0)]
	public double LossChanceMin = 0.05;

	[InlineHelp]
	[DrawIf("Enabled", Hide = true)]
	[Unit(Units.NormalizedPercentage)]
	[RangeEx(0.0, 1.0)]
	public double LossChanceMax = 0.05;

	[InlineHelp]
	[DrawIf("Enabled", Hide = true)]
	[Unit(Units.NormalizedPercentage)]
	[RangeEx(0.0, 1.0)]
	public double LossChanceThreshold = 0.0;

	[InlineHelp]
	[DrawIf("Enabled", Hide = true)]
	[Unit(Units.Seconds)]
	[RangeEx(0.0, 10.0)]
	public double LossChancePeriod = 0.0;

	[InlineHelp]
	[DrawIf("Enabled", Hide = true)]
	[Unit(Units.NormalizedPercentage)]
	[RangeEx(0.0, 1.0)]
	public double AdditionalLoss = 0.0;

	public NetworkSimulationConfiguration Clone()
	{
		return (NetworkSimulationConfiguration)MemberwiseClone();
	}

	public NetConfigSimulation Create()
	{
		NetConfigSimulation defaults = NetConfigSimulation.Defaults;
		if (Enabled)
		{
			if (DelayMin == 0.0 && DelayMax == 0.0)
			{
				defaults.DelayOscillator.Min = 0.0;
				defaults.DelayOscillator.Max = 0.0;
			}
			else if (DelayMin > DelayMax)
			{
				defaults.DelayOscillator.Min = Math.Max(9.999999747378752E-05, DelayMax * 0.5);
				defaults.DelayOscillator.Max = Math.Max(9.999999747378752E-05, DelayMin * 0.5);
			}
			else
			{
				defaults.DelayOscillator.Min = Math.Max(9.999999747378752E-05, DelayMin * 0.5);
				defaults.DelayOscillator.Max = Math.Max(9.999999747378752E-05, DelayMax * 0.5);
			}
			defaults.DelayOscillator.Period = DelayPeriod;
			defaults.DelayOscillator.Shape = DelayShape;
			defaults.DelayOscillator.Threshold = DelayThreshold;
			defaults.DelayOscillator.Additional = AdditionalJitter * 0.5;
			if (LossChanceMin == 0.0 && LossChanceMax == 0.0)
			{
				defaults.LossOscillator.Min = 0.0;
				defaults.LossOscillator.Max = 0.0;
			}
			else if (LossChanceMin > LossChanceMax)
			{
				defaults.LossOscillator.Min = Math.Max(9.999999747378752E-05, LossChanceMax * 0.5);
				defaults.LossOscillator.Max = Math.Max(9.999999747378752E-05, LossChanceMin * 0.5);
			}
			else
			{
				defaults.LossOscillator.Min = Math.Max(9.999999747378752E-05, LossChanceMin * 0.5);
				defaults.LossOscillator.Max = Math.Max(9.999999747378752E-05, LossChanceMax * 0.5);
			}
			defaults.LossOscillator.Period = LossChancePeriod;
			defaults.LossOscillator.Shape = LossChanceShape;
			defaults.LossOscillator.Threshold = LossChanceThreshold;
			defaults.LossOscillator.Additional = AdditionalLoss;
		}
		return defaults;
	}
}

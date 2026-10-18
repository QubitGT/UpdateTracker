using System;
using UnityEngine;

namespace Fusion;

[Serializable]
public class InterpolatedErrorCorrectionSettings
{
	[InlineHelp]
	public float MinRate = 3.3f;

	[InlineHelp]
	public float MaxRate = 10f;

	[InlineHelp]
	[Header("Position Error")]
	public float PosBlendStart = 0.25f;

	[InlineHelp]
	public float PosBlendEnd = 1f;

	[InlineHelp]
	public float PosMinCorrection = 0.025f;

	[InlineHelp]
	public float PosTeleportDistance = 2f;

	[InlineHelp]
	[Header("Rotation Error")]
	public float RotBlendStart = 0.1f;

	[InlineHelp]
	public float RotBlendEnd = 0.5f;

	[InlineHelp]
	public float RotTeleportRadians = 1.5f;
}

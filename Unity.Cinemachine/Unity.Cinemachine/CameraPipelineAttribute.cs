using System;

namespace Unity.Cinemachine;

public sealed class CameraPipelineAttribute : Attribute
{
	public CinemachineCore.Stage Stage { get; private set; }

	public CameraPipelineAttribute(CinemachineCore.Stage stage)
	{
		Stage = stage;
	}
}

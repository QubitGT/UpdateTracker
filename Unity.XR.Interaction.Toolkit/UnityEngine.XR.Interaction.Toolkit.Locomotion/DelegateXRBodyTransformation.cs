using System;

namespace UnityEngine.XR.Interaction.Toolkit.Locomotion;

public class DelegateXRBodyTransformation : IXRBodyTransformation
{
	public event Action<XRMovableBody> transformation;

	public DelegateXRBodyTransformation()
	{
	}

	public DelegateXRBodyTransformation(Action<XRMovableBody> transformation)
	{
		this.transformation = transformation;
	}

	public void Apply(XRMovableBody body)
	{
		this.transformation?.Invoke(body);
	}
}

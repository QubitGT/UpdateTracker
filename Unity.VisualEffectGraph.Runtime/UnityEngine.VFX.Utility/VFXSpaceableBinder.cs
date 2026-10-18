namespace UnityEngine.VFX.Utility;

internal abstract class VFXSpaceableBinder : VFXBinderBase
{
	public enum BinderSpace
	{
		Automatic,
		World,
		Local
	}

	[SerializeField]
	public BinderSpace Space;

	private VFXSpace GetTargetSpace(VisualEffect component, ExposedProperty targetProperty)
	{
		VFXSpace result = VFXSpace.None;
		switch (Space)
		{
		case BinderSpace.Automatic:
			result = component.visualEffectAsset.GetExposedSpace(targetProperty);
			break;
		case BinderSpace.World:
			result = VFXSpace.World;
			break;
		case BinderSpace.Local:
			result = VFXSpace.Local;
			break;
		}
		return result;
	}

	protected void ApplySpacePositionNormal(VisualEffect component, ExposedProperty targetProperty, Transform sourceTransform, out Vector3 position, out Vector3 normal)
	{
		if (GetTargetSpace(component, targetProperty) == VFXSpace.Local)
		{
			Matrix4x4 matrix4x = component.transform.worldToLocalMatrix * sourceTransform.localToWorldMatrix;
			position = matrix4x.GetPosition();
			normal = matrix4x.MultiplyVector(Vector3.up);
		}
		else
		{
			position = sourceTransform.position;
			normal = sourceTransform.up;
		}
	}

	protected void ApplySpaceTS(VisualEffect component, ExposedProperty targetProperty, Transform sourceTransform, out Vector3 position, out Vector3 scale)
	{
		if (GetTargetSpace(component, targetProperty) == VFXSpace.Local)
		{
			Matrix4x4 matrix4x = component.transform.worldToLocalMatrix * sourceTransform.localToWorldMatrix;
			position = matrix4x.GetPosition();
			scale = matrix4x.lossyScale;
		}
		else
		{
			position = sourceTransform.position;
			scale = sourceTransform.lossyScale;
		}
	}

	protected void ApplySpaceTRS(VisualEffect component, ExposedProperty targetProperty, Transform sourceTransform, out Vector3 position, out Vector3 eulerAngles, out Vector3 scale)
	{
		if (GetTargetSpace(component, targetProperty) == VFXSpace.Local)
		{
			Matrix4x4 matrix4x = component.transform.worldToLocalMatrix * sourceTransform.localToWorldMatrix;
			position = matrix4x.GetPosition();
			eulerAngles = matrix4x.rotation.eulerAngles;
			scale = matrix4x.lossyScale;
		}
		else
		{
			position = sourceTransform.position;
			eulerAngles = sourceTransform.eulerAngles;
			scale = sourceTransform.lossyScale;
		}
	}

	protected Vector3 ApplySpacePosition(VisualEffect component, ExposedProperty targetProperty, Vector3 sourceWorldPosition)
	{
		if (GetTargetSpace(component, targetProperty) == VFXSpace.Local)
		{
			return component.transform.worldToLocalMatrix.MultiplyPoint(sourceWorldPosition);
		}
		return sourceWorldPosition;
	}
}

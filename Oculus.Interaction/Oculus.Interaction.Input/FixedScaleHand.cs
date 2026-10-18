using UnityEngine;

namespace Oculus.Interaction.Input;

public class FixedScaleHand : Hand
{
	[SerializeField]
	private float _scale = 1f;

	protected override void Apply(HandDataAsset data)
	{
		Pose b = PoseUtils.Delta(in data.Root, in data.PointerPose);
		b.position = b.position / data.HandScale * _scale;
		PoseUtils.Multiply(in data.Root, in b, ref data.PointerPose);
		data.HandScale = _scale;
	}

	public void InjectAllFixedScaleDataModifier(UpdateModeFlags updateMode, IDataSource updateAfter, DataModifier<HandDataAsset> modifyDataFromSource, bool applyModifier, float scale)
	{
		InjectAllHand(updateMode, updateAfter, modifyDataFromSource, applyModifier);
		InjectScale(scale);
	}

	public void InjectScale(float scale)
	{
		_scale = scale;
	}
}

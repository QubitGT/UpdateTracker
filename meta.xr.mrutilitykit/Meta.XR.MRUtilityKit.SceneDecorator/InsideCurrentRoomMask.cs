using Meta.XR.Util;

namespace Meta.XR.MRUtilityKit.SceneDecorator;

[Feature(Feature.Scene)]
public class InsideCurrentRoomMask : Mask
{
	public override float SampleMask(Candidate candidate)
	{
		return 0f;
	}

	public override bool Check(Candidate c)
	{
		return MRUK.Instance.GetCurrentRoom().IsPositionInRoom(c.hit.point);
	}
}

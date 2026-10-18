namespace Oculus.Interaction.Input.Compatibility.OVR;

public interface ICopyFrom<in TSelfType>
{
	void CopyFrom(TSelfType source);
}

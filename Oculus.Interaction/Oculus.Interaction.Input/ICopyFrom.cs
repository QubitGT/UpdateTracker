namespace Oculus.Interaction.Input;

public interface ICopyFrom<in TSelfType>
{
	void CopyFrom(TSelfType source);
}

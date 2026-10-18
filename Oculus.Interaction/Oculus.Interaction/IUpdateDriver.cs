namespace Oculus.Interaction;

public interface IUpdateDriver
{
	bool IsRootDriver { get; set; }

	void Drive();
}

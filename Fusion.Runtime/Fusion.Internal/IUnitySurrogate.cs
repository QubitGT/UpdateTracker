namespace Fusion.Internal;

public interface IUnitySurrogate
{
	unsafe void Read(int* data, int capacity);

	unsafe void Write(int* data, int capacity);
}

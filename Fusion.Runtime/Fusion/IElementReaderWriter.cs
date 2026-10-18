namespace Fusion;

public interface IElementReaderWriter<T>
{
	unsafe T Read(byte* data, int index);

	unsafe void Write(byte* data, int index, T element);

	int GetElementWordCount();

	unsafe ref T ReadRef(byte* data, int index);

	int GetElementHashCode(T element);
}

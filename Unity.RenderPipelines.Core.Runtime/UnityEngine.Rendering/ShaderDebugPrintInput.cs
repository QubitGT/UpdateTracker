namespace UnityEngine.Rendering;

public struct ShaderDebugPrintInput
{
	public Vector2 pos { get; set; }

	public bool leftDown { get; set; }

	public bool rightDown { get; set; }

	public bool middleDown { get; set; }

	public string String()
	{
		return $"Mouse: {pos.x}x{pos.y}  Btns: Left:{leftDown} Right:{rightDown} Middle:{middleDown} ";
	}
}

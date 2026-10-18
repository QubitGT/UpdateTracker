using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Runtime.CompilerServices;

[CompilerGenerated]
[EditorBrowsable(EditorBrowsableState.Never)]
[GeneratedCode("Unity.MonoScriptGenerator.MonoScriptInfoGenerator", null)]
internal class UnitySourceGeneratedAssemblyMonoScriptTypes_v1
{
	private struct MonoScriptData
	{
		public byte[] FilePathsData;

		public byte[] TypesData;

		public int TotalTypes;

		public int TotalFiles;

		public bool IsEditorOnly;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static MonoScriptData Get()
	{
		MonoScriptData result = default(MonoScriptData);
		result.FilePathsData = new byte[62]
		{
			0, 0, 0, 1, 0, 0, 0, 54, 92, 80,
			97, 99, 107, 97, 103, 101, 115, 92, 99, 111,
			109, 46, 117, 110, 105, 116, 121, 46, 114, 101,
			99, 111, 114, 100, 101, 114, 92, 82, 117, 110,
			116, 105, 109, 101, 92, 66, 97, 115, 101, 92,
			82, 101, 102, 72, 97, 110, 100, 108, 101, 46,
			99, 115
		};
		result.TypesData = new byte[26]
		{
			0, 0, 0, 0, 21, 85, 110, 105, 116, 121,
			46, 77, 101, 100, 105, 97, 124, 82, 101, 102,
			72, 97, 110, 100, 108, 101
		};
		result.TotalFiles = 1;
		result.TotalTypes = 1;
		result.IsEditorOnly = false;
		return result;
	}
}

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
		result.FilePathsData = new byte[111]
		{
			0, 0, 0, 1, 0, 0, 0, 47, 92, 65,
			115, 115, 101, 116, 115, 92, 68, 101, 98, 117,
			103, 68, 114, 97, 119, 105, 110, 103, 69, 120,
			116, 101, 110, 115, 105, 111, 110, 92, 68, 101,
			98, 117, 103, 69, 120, 116, 101, 110, 115, 105,
			111, 110, 46, 99, 115, 0, 0, 0, 1, 0,
			0, 0, 48, 92, 65, 115, 115, 101, 116, 115,
			92, 68, 101, 98, 117, 103, 68, 114, 97, 119,
			105, 110, 103, 69, 120, 116, 101, 110, 115, 105,
			111, 110, 92, 69, 120, 97, 109, 112, 108, 101,
			92, 69, 120, 97, 109, 112, 108, 101, 46, 99,
			115
		};
		result.TypesData = new byte[33]
		{
			0, 0, 0, 0, 15, 124, 68, 101, 98, 117,
			103, 69, 120, 116, 101, 110, 115, 105, 111, 110,
			0, 0, 0, 0, 8, 124, 69, 120, 97, 109,
			112, 108, 101
		};
		result.TotalFiles = 2;
		result.TotalTypes = 2;
		result.IsEditorOnly = false;
		return result;
	}
}

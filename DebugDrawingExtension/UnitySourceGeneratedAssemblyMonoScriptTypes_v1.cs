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
		result.FilePathsData = new byte[55]
		{
			0, 0, 0, 1, 0, 0, 0, 47, 92, 65,
			115, 115, 101, 116, 115, 92, 68, 101, 98, 117,
			103, 68, 114, 97, 119, 105, 110, 103, 69, 120,
			116, 101, 110, 115, 105, 111, 110, 92, 68, 101,
			98, 117, 103, 69, 120, 116, 101, 110, 115, 105,
			111, 110, 46, 99, 115
		};
		result.TypesData = new byte[20]
		{
			0, 0, 0, 0, 15, 124, 68, 101, 98, 117,
			103, 69, 120, 116, 101, 110, 115, 105, 111, 110
		};
		result.TotalFiles = 1;
		result.TotalTypes = 1;
		result.IsEditorOnly = false;
		return result;
	}
}

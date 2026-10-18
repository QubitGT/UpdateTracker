using System.Text;
using DigitalOpus.MB.Core;
using UnityEngine;

public class MB3_MultiMeshBaker : MB3_MeshBakerCommon
{
	[SerializeField]
	protected MB3_MultiMeshCombiner _meshCombiner = new MB3_MultiMeshCombiner();

	public override MB3_MeshCombiner meshCombiner => _meshCombiner;

	public void PrintTimings()
	{
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		double num4 = 0.0;
		double num5 = 0.0;
		double num6 = 0.0;
		double num7 = 0.0;
		double num8 = 0.0;
		double num9 = 0.0;
		double num10 = 0.0;
		for (int i = 0; i < _meshCombiner.meshCombiners.Count; i++)
		{
			MB3_MeshCombinerSingle combinedMesh = _meshCombiner.meshCombiners[i].combinedMesh;
			num += combinedMesh.db_showHideGameObjects.Elapsed.TotalSeconds;
			num2 += combinedMesh.db_addDeleteGameObjects.Elapsed.TotalSeconds;
			num7 += combinedMesh.db_addDeleteGameObjects_CollectMeshData.Elapsed.TotalSeconds;
			num3 += combinedMesh.db_addDeleteGameObjects_InitFromMeshCombiner.Elapsed.TotalSeconds;
			num4 += combinedMesh.db_addDeleteGameObjects_Init.Elapsed.TotalSeconds;
			num5 += combinedMesh.db_addDeleteGameObjects_CopyArraysFromPreviousBakeBuffersToNewBuffers.Elapsed.TotalSeconds;
			num6 += combinedMesh.db_addDeleteGameObjects_CopyFromDGOMeshToBuffers.Elapsed.TotalSeconds;
			num8 += combinedMesh.db_apply.Elapsed.TotalSeconds;
			num9 += combinedMesh.db_applyShowHide.Elapsed.TotalSeconds;
			num10 += combinedMesh.db_updateGameObjects.Elapsed.TotalSeconds;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Timings  " + ((_meshCombiner.settings.meshAPI == MB_MeshCombineAPIType.betaNativeArrayAPI) ? "  newMeshAPI " : " oldMeshAPI"));
		stringBuilder.AppendLine("db_showHideGameObjects\t" + num);
		stringBuilder.AppendLine("db_addDeleteGameObjects\t" + num2);
		stringBuilder.AppendLine("\t\tdb_addDeleteGameObjects_CollectMeshData\t" + num7);
		stringBuilder.AppendLine("\t\tdb_addDeleteGameObjects_InitFromMeshCombiner\t" + num3);
		stringBuilder.AppendLine("\t\tdb_addDeleteGameObjects_Init\t" + num4);
		stringBuilder.AppendLine("\t\tdb_addDeleteGameObjects_CopyArraysFromPreviousBakeBuffersToNewBuffers\t" + num5);
		stringBuilder.AppendLine("\t\tdb_addDeleteGameObjects_CopyFromDGOMeshToBuffers\t" + num6);
		stringBuilder.AppendLine("\t\tdb_addDeleteGameObjects_CollectMeshData  tdb_addDeleteGameObjects_CollectMeshData ");
		stringBuilder.AppendLine("db_apply\t" + num8);
		stringBuilder.AppendLine("db_applyShowHide\t" + num9);
		stringBuilder.AppendLine("db_updateGameObjects\t" + num10);
		Debug.Log(stringBuilder.ToString());
	}

	public override bool AddDeleteGameObjects(GameObject[] gos, GameObject[] deleteGOs, bool disableRendererInSource)
	{
		UpgradeToCurrentVersionIfNecessary();
		if (_meshCombiner.resultSceneObject == null)
		{
			_meshCombiner.resultSceneObject = new GameObject("CombinedMesh-" + base.name);
		}
		meshCombiner.name = base.name + "-mesh";
		return _meshCombiner.AddDeleteGameObjects(gos, deleteGOs, disableRendererInSource);
	}

	public override bool AddDeleteGameObjectsByID(GameObject[] gos, int[] deleteGOs, bool disableRendererInSource)
	{
		UpgradeToCurrentVersionIfNecessary();
		if (_meshCombiner.resultSceneObject == null)
		{
			_meshCombiner.resultSceneObject = new GameObject("CombinedMesh-" + base.name);
		}
		meshCombiner.name = base.name + "-mesh";
		return _meshCombiner.AddDeleteGameObjectsByID(gos, deleteGOs, disableRendererInSource);
	}

	public void OnDestroy()
	{
		if (_meshCombiner != null)
		{
			_meshCombiner.Dispose();
		}
	}
}

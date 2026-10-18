using System.Text;
using DigitalOpus.MB.Core;
using UnityEngine;

public class MB3_MeshBaker : MB3_MeshBakerCommon
{
	[SerializeField]
	protected MB3_MeshCombinerSingle _meshCombiner = new MB3_MeshCombinerSingle();

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
		double num11 = 0.0;
		double num12 = 0.0;
		double num13 = 0.0;
		MB3_MeshCombinerSingle mB3_MeshCombinerSingle = _meshCombiner;
		num += mB3_MeshCombinerSingle.db_showHideGameObjects.Elapsed.TotalSeconds;
		num2 += mB3_MeshCombinerSingle.db_addDeleteGameObjects.Elapsed.TotalSeconds;
		num7 += mB3_MeshCombinerSingle.db_addDeleteGameObjects_CollectMeshData.Elapsed.TotalSeconds;
		num8 += mB3_MeshCombinerSingle.db_addDeleteGameObjects_CollectMeshData_a.Elapsed.TotalSeconds;
		num9 += mB3_MeshCombinerSingle.db_addDeleteGameObjects_CollectMeshData_b.Elapsed.TotalSeconds;
		num10 += mB3_MeshCombinerSingle.db_addDeleteGameObjects_CollectMeshData_c.Elapsed.TotalSeconds;
		num3 += mB3_MeshCombinerSingle.db_addDeleteGameObjects_InitFromMeshCombiner.Elapsed.TotalSeconds;
		num4 += mB3_MeshCombinerSingle.db_addDeleteGameObjects_Init.Elapsed.TotalSeconds;
		num5 += mB3_MeshCombinerSingle.db_addDeleteGameObjects_CopyArraysFromPreviousBakeBuffersToNewBuffers.Elapsed.TotalSeconds;
		num6 += mB3_MeshCombinerSingle.db_addDeleteGameObjects_CopyFromDGOMeshToBuffers.Elapsed.TotalSeconds;
		num11 += mB3_MeshCombinerSingle.db_apply.Elapsed.TotalSeconds;
		num12 += mB3_MeshCombinerSingle.db_applyShowHide.Elapsed.TotalSeconds;
		num13 += mB3_MeshCombinerSingle.db_updateGameObjects.Elapsed.TotalSeconds;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Timings  " + ((_meshCombiner.settings.meshAPI == MB_MeshCombineAPIType.betaNativeArrayAPI) ? "  newMeshAPI " : " oldMeshAPI"));
		stringBuilder.AppendLine("db_showHideGameObjects\t" + num);
		stringBuilder.AppendLine("db_addDeleteGameObjects\t" + num2);
		stringBuilder.AppendLine("\t\tdb_addDeleteGameObjects_CollectMeshData\t" + num7);
		stringBuilder.AppendLine("\t\tdb_addDeleteGameObjects_CollectMeshDataA\t\t" + num8);
		stringBuilder.AppendLine("\t\tdb_addDeleteGameObjects_CollectMeshDataB\t\t" + num9);
		stringBuilder.AppendLine("\t\tdb_addDeleteGameObjects_CollectMeshDataC\t\t" + num10);
		stringBuilder.AppendLine("\t\tdb_addDeleteGameObjects_InitFromMeshCombiner\t" + num3);
		stringBuilder.AppendLine("\t\tdb_addDeleteGameObjects_Init\t" + num4);
		stringBuilder.AppendLine("\t\tdb_addDeleteGameObjects_CopyArraysFromPreviousBakeBuffersToNewBuffers\t" + num5);
		stringBuilder.AppendLine("\t\tdb_addDeleteGameObjects_CopyFromDGOMeshToBuffers\t" + num6);
		stringBuilder.AppendLine("\t\tdb_addDeleteGameObjects_CollectMeshData  tdb_addDeleteGameObjects_CollectMeshData ");
		stringBuilder.AppendLine("db_apply\t" + num11);
		stringBuilder.AppendLine("db_applyShowHide\t" + num12);
		stringBuilder.AppendLine("db_updateGameObjects\t" + num13);
		Debug.Log(stringBuilder.ToString());
	}

	public void BuildSceneMeshObject()
	{
		_meshCombiner.BuildSceneMeshObject();
	}

	public virtual bool ShowHide(GameObject[] gos, GameObject[] deleteGOs)
	{
		return _meshCombiner.ShowHideGameObjects(gos, deleteGOs);
	}

	public virtual void ApplyShowHide()
	{
		_meshCombiner.ApplyShowHide();
	}

	public override bool AddDeleteGameObjects(GameObject[] gos, GameObject[] deleteGOs, bool disableRendererInSource)
	{
		UpgradeToCurrentVersionIfNecessary();
		_meshCombiner.name = base.name + "-mesh";
		return _meshCombiner.AddDeleteGameObjects(gos, deleteGOs, disableRendererInSource);
	}

	public override bool AddDeleteGameObjectsByID(GameObject[] gos, int[] deleteGOinstanceIDs, bool disableRendererInSource)
	{
		UpgradeToCurrentVersionIfNecessary();
		_meshCombiner.name = base.name + "-mesh";
		return _meshCombiner.AddDeleteGameObjectsByID(gos, deleteGOinstanceIDs, disableRendererInSource);
	}

	public void OnDestroy()
	{
		if (meshCombiner != null)
		{
			meshCombiner.Dispose();
		}
	}
}

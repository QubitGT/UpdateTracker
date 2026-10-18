using Unity.Collections;

namespace GorillaTagScripts;

public class BuilderTableJobs
{
	public static void BuildTestPieceListForJob(BuilderPiece testPiece, NativeList<BuilderPieceData> testPieceList, NativeList<BuilderGridPlaneData> testGridPlaneList)
	{
		if (!(testPiece == null))
		{
			int length = testPieceList.Length;
			BuilderPieceData value = new BuilderPieceData(testPiece);
			testPieceList.Add(in value);
			for (int i = 0; i < testPiece.gridPlanes.Count; i++)
			{
				BuilderGridPlaneData value2 = new BuilderGridPlaneData(testPiece.gridPlanes[i], length);
				testGridPlaneList.Add(in value2);
			}
			BuilderPiece builderPiece = testPiece.firstChildPiece;
			while (builderPiece != null)
			{
				BuildTestPieceListForJob(builderPiece, testPieceList, testGridPlaneList);
				builderPiece = builderPiece.nextSiblingPiece;
			}
		}
	}

	public static void BuildTestPieceListForJob(BuilderPiece testPiece, NativeList<BuilderGridPlaneData> testGridPlaneList)
	{
		if (!(testPiece == null))
		{
			for (int i = 0; i < testPiece.gridPlanes.Count; i++)
			{
				BuilderGridPlaneData value = new BuilderGridPlaneData(testPiece.gridPlanes[i], -1);
				testGridPlaneList.Add(in value);
			}
			BuilderPiece builderPiece = testPiece.firstChildPiece;
			while (builderPiece != null)
			{
				BuildTestPieceListForJob(builderPiece, testGridPlaneList);
				builderPiece = builderPiece.nextSiblingPiece;
			}
		}
	}
}

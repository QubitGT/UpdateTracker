using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DigitalOpus.MB.Core;

public class MB3_TextureCombinerMerging
{
	public static bool DO_INTEGRITY_CHECKS;

	private bool _HasBeenInitialized;

	private bool _considerNonTextureProperties;

	private MB3_TextureCombinerNonTextureProperties resultMaterialTextureBlender;

	private bool fixOutOfBoundsUVs = true;

	public MB2_LogLevel LOG_LEVEL = MB2_LogLevel.info;

	private static bool LOG_LEVEL_TRACE_MERGE_MAT_SUBRECTS;

	public static Rect BuildTransformMeshUV2AtlasRect(bool considerMeshUVs, Rect _atlasRect, Rect _obUVRect, Rect _sourceMaterialTiling, Rect _encapsulatingRect)
	{
		DRect r = new DRect(_atlasRect);
		DRect t = ((!considerMeshUVs) ? new DRect(0.0, 0.0, 1.0, 1.0) : new DRect(_obUVRect));
		DRect r2 = new DRect(_sourceMaterialTiling);
		DRect t2 = new DRect(_encapsulatingRect);
		DRect r3 = MB3_UVTransformUtility.InverseTransform(ref t2);
		DRect r4 = MB3_UVTransformUtility.InverseTransform(ref t);
		DRect B = MB3_UVTransformUtility.CombineTransforms(ref t, ref r2);
		DRect r5 = MB3_UVTransformUtility.GetShiftTransformToFitBinA(ref t2, ref B);
		B = MB3_UVTransformUtility.CombineTransforms(ref B, ref r5);
		DRect r6 = MB3_UVTransformUtility.CombineTransforms(ref B, ref r3);
		DRect r7 = MB3_UVTransformUtility.CombineTransforms(ref r4, ref r6);
		return MB3_UVTransformUtility.CombineTransforms(ref r7, ref r).GetRect();
	}

	public MB3_TextureCombinerMerging(bool considerNonTextureProps, MB3_TextureCombinerNonTextureProperties resultMaterialTexBlender, bool fixObUVs, MB2_LogLevel logLevel)
	{
		LOG_LEVEL = logLevel;
		_considerNonTextureProperties = considerNonTextureProps;
		resultMaterialTextureBlender = resultMaterialTexBlender;
		fixOutOfBoundsUVs = fixObUVs;
	}

	public void MergeOverlappingDistinctMaterialTexturesAndCalcMaterialSubrects(List<MB_TexSet> distinctMaterialTextures)
	{
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			Debug.Log("MergeOverlappingDistinctMaterialTexturesAndCalcMaterialSubrects num atlas rects" + distinctMaterialTextures.Count);
		}
		int num = 0;
		for (int i = 0; i < distinctMaterialTextures.Count; i++)
		{
			MB_TexSet mB_TexSet = distinctMaterialTextures[i];
			int num2 = -1;
			bool flag = true;
			DRect dRect = default(DRect);
			for (int j = 0; j < mB_TexSet.ts.Length; j++)
			{
				if (num2 != -1)
				{
					if (!mB_TexSet.ts[j].isNull && dRect != mB_TexSet.ts[j].matTilingRect)
					{
						flag = false;
					}
				}
				else if (!mB_TexSet.ts[j].isNull)
				{
					num2 = j;
					dRect = mB_TexSet.ts[j].matTilingRect;
				}
			}
			if (LOG_LEVEL >= MB2_LogLevel.debug || LOG_LEVEL_TRACE_MERGE_MAT_SUBRECTS)
			{
				if (flag)
				{
					Debug.LogFormat("TextureSet {0} allTexturesUseSameMatTiling = {1}", i, flag);
				}
				else
				{
					Debug.Log($"Textures in material(s) do not all use the same material tiling. This set of textures will not be considered for merge: {mB_TexSet.GetDescription()} ");
				}
			}
			if (flag)
			{
				mB_TexSet.SetAllTexturesUseSameMatTilingTrue();
			}
		}
		for (int k = 0; k < distinctMaterialTextures.Count; k++)
		{
			MB_TexSet mB_TexSet2 = distinctMaterialTextures[k];
			for (int l = 0; l < mB_TexSet2.matsAndGOs.mats.Count; l++)
			{
				if (mB_TexSet2.matsAndGOs.gos.Count > 0)
				{
					mB_TexSet2.matsAndGOs.mats[l].objName = mB_TexSet2.matsAndGOs.gos[0].name;
				}
				else if (mB_TexSet2.ts[0] != null)
				{
					mB_TexSet2.matsAndGOs.mats[l].objName = $"[objWithTx:{mB_TexSet2.ts[0].GetTexName()} atlasBlock:{k} matIdx{l}]";
				}
				else
				{
					mB_TexSet2.matsAndGOs.mats[l].objName = string.Format("[objWithTx:{0} atlasBlock:{1} matIdx{2}]", "Unknown", k, l);
				}
			}
			mB_TexSet2.CalcInitialFullSamplingRects(fixOutOfBoundsUVs);
			mB_TexSet2.CalcMatAndUVSamplingRects();
		}
		_HasBeenInitialized = true;
		List<int> list = new List<int>();
		for (int m = 0; m < distinctMaterialTextures.Count; m++)
		{
			MB_TexSet mB_TexSet3 = distinctMaterialTextures[m];
			for (int n = m + 1; n < distinctMaterialTextures.Count; n++)
			{
				MB_TexSet mB_TexSet4 = distinctMaterialTextures[n];
				if (!mB_TexSet4.AllTexturesAreSameForMerge(mB_TexSet3, _considerNonTextureProperties, resultMaterialTextureBlender))
				{
					continue;
				}
				double num3 = 0.0;
				double num4 = 0.0;
				DRect dRect2 = default(DRect);
				int num5 = -1;
				for (int num6 = 0; num6 < mB_TexSet3.ts.Length; num6++)
				{
					if (!mB_TexSet3.ts[num6].isNull && num5 == -1)
					{
						num5 = num6;
					}
				}
				DRect uvRect = default(DRect);
				DRect uvRect2 = default(DRect);
				if (num5 != -1)
				{
					uvRect = mB_TexSet4.matsAndGOs.mats[0].samplingRectMatAndUVTiling;
					for (int num7 = 1; num7 < mB_TexSet4.matsAndGOs.mats.Count; num7++)
					{
						DRect willBeIn = mB_TexSet4.matsAndGOs.mats[num7].samplingRectMatAndUVTiling;
						uvRect = MB3_UVTransformUtility.GetEncapsulatingRectShifted(ref uvRect, ref willBeIn);
					}
					uvRect2 = mB_TexSet3.matsAndGOs.mats[0].samplingRectMatAndUVTiling;
					for (int num8 = 1; num8 < mB_TexSet3.matsAndGOs.mats.Count; num8++)
					{
						DRect willBeIn2 = mB_TexSet3.matsAndGOs.mats[num8].samplingRectMatAndUVTiling;
						uvRect2 = MB3_UVTransformUtility.GetEncapsulatingRectShifted(ref uvRect2, ref willBeIn2);
					}
					dRect2 = MB3_UVTransformUtility.GetEncapsulatingRectShifted(ref uvRect, ref uvRect2);
					num3 += dRect2.width * dRect2.height;
					num4 += uvRect.width * uvRect.height + uvRect2.width * uvRect2.height;
				}
				else
				{
					dRect2 = new DRect(0f, 0f, 1f, 1f);
				}
				if (num3 < num4)
				{
					num++;
					StringBuilder stringBuilder = null;
					if (LOG_LEVEL >= MB2_LogLevel.info)
					{
						stringBuilder = new StringBuilder();
						stringBuilder.AppendFormat("About To Merge:\n   TextureSet1 {0}\n   TextureSet2 {1}\n", mB_TexSet4.GetDescription(), mB_TexSet3.GetDescription());
						if (LOG_LEVEL >= MB2_LogLevel.trace)
						{
							for (int num9 = 0; num9 < mB_TexSet4.matsAndGOs.mats.Count; num9++)
							{
								stringBuilder.AppendFormat("tx1 Mat {0} matAndMeshUVRect {1} fullSamplingRect {2}\n", mB_TexSet4.matsAndGOs.mats[num9].mat, mB_TexSet4.matsAndGOs.mats[num9].samplingRectMatAndUVTiling, mB_TexSet4.ts[0].GetEncapsulatingSamplingRect());
							}
							for (int num10 = 0; num10 < mB_TexSet3.matsAndGOs.mats.Count; num10++)
							{
								stringBuilder.AppendFormat("tx2 Mat {0} matAndMeshUVRect {1} fullSamplingRect {2}\n", mB_TexSet3.matsAndGOs.mats[num10].mat, mB_TexSet3.matsAndGOs.mats[num10].samplingRectMatAndUVTiling, mB_TexSet3.ts[0].GetEncapsulatingSamplingRect());
							}
						}
					}
					for (int num11 = 0; num11 < mB_TexSet3.matsAndGOs.gos.Count; num11++)
					{
						if (!mB_TexSet4.matsAndGOs.gos.Contains(mB_TexSet3.matsAndGOs.gos[num11]))
						{
							mB_TexSet4.matsAndGOs.gos.Add(mB_TexSet3.matsAndGOs.gos[num11]);
						}
					}
					for (int num12 = 0; num12 < mB_TexSet3.matsAndGOs.mats.Count; num12++)
					{
						mB_TexSet4.matsAndGOs.mats.Add(mB_TexSet3.matsAndGOs.mats[num12]);
					}
					mB_TexSet4.SetEncapsulatingSamplingRectWhenMergingTexSets(dRect2);
					if (!list.Contains(m))
					{
						list.Add(m);
					}
					if (LOG_LEVEL < MB2_LogLevel.debug)
					{
						break;
					}
					if (LOG_LEVEL >= MB2_LogLevel.trace)
					{
						stringBuilder.AppendFormat("=== After Merge TextureSet {0}\n", mB_TexSet4.GetDescription());
						for (int num13 = 0; num13 < mB_TexSet4.matsAndGOs.mats.Count; num13++)
						{
							stringBuilder.AppendFormat("tx1 Mat {0} matAndMeshUVRect {1} fullSamplingRect {2}\n", mB_TexSet4.matsAndGOs.mats[num13].mat, mB_TexSet4.matsAndGOs.mats[num13].samplingRectMatAndUVTiling, mB_TexSet4.ts[0].GetEncapsulatingSamplingRect());
						}
						if (DO_INTEGRITY_CHECKS && DO_INTEGRITY_CHECKS)
						{
							DoIntegrityCheckMergedEncapsulatingSamplingRects(distinctMaterialTextures);
						}
					}
					Debug.Log(stringBuilder.ToString());
					break;
				}
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					string description = mB_TexSet4.GetDescription();
					DRect dRect3 = uvRect;
					string arg = description + dRect3.ToString();
					string description2 = mB_TexSet3.GetDescription();
					dRect3 = uvRect2;
					Debug.Log($"Considered merging {arg} and {description2 + dRect3.ToString()} but there was not enough overlap. It is more efficient to bake these to separate rectangles.");
				}
			}
		}
		for (int num14 = list.Count - 1; num14 >= 0; num14--)
		{
			distinctMaterialTextures.RemoveAt(list[num14]);
		}
		list.Clear();
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			Debug.Log($"MergeOverlappingDistinctMaterialTexturesAndCalcMaterialSubrects complete merged {num} now have {distinctMaterialTextures.Count}");
		}
		if (DO_INTEGRITY_CHECKS)
		{
			DoIntegrityCheckMergedEncapsulatingSamplingRects(distinctMaterialTextures);
		}
	}

	public void MergeDistinctMaterialTexturesThatWouldExceedMaxAtlasSizeAndCalcMaterialSubrects(List<MB_TexSet> distinctMaterialTextures, int maxAtlasSize)
	{
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			Debug.Log("MergeDistinctMaterialTexturesThatWouldExceedMaxAtlasSizeAndCalcMaterialSubrects num atlas rects" + distinctMaterialTextures.Count);
		}
		int num = 0;
		List<int> list = new List<int>();
		for (int i = 0; i < distinctMaterialTextures.Count; i++)
		{
			MB_TexSet mB_TexSet = distinctMaterialTextures[i];
			for (int j = i + 1; j < distinctMaterialTextures.Count; j++)
			{
				MB_TexSet mB_TexSet2 = distinctMaterialTextures[j];
				if (!mB_TexSet2.AllTexturesAreSameForMerge(mB_TexSet, _considerNonTextureProperties, resultMaterialTextureBlender))
				{
					continue;
				}
				DRect dRect = default(DRect);
				int num2 = -1;
				for (int k = 0; k < mB_TexSet.ts.Length; k++)
				{
					if (!mB_TexSet.ts[k].isNull && num2 == -1)
					{
						num2 = k;
					}
				}
				DRect uvRect = default(DRect);
				DRect uvRect2 = default(DRect);
				if (num2 != -1)
				{
					uvRect = mB_TexSet2.matsAndGOs.mats[0].samplingRectMatAndUVTiling;
					for (int l = 1; l < mB_TexSet2.matsAndGOs.mats.Count; l++)
					{
						DRect willBeIn = mB_TexSet2.matsAndGOs.mats[l].samplingRectMatAndUVTiling;
						uvRect = MB3_UVTransformUtility.GetEncapsulatingRectShifted(ref uvRect, ref willBeIn);
					}
					uvRect2 = mB_TexSet.matsAndGOs.mats[0].samplingRectMatAndUVTiling;
					for (int m = 1; m < mB_TexSet.matsAndGOs.mats.Count; m++)
					{
						DRect willBeIn2 = mB_TexSet.matsAndGOs.mats[m].samplingRectMatAndUVTiling;
						uvRect2 = MB3_UVTransformUtility.GetEncapsulatingRectShifted(ref uvRect2, ref willBeIn2);
					}
					dRect = MB3_UVTransformUtility.GetEncapsulatingRectShifted(ref uvRect, ref uvRect2);
				}
				else
				{
					dRect = new DRect(0f, 0f, 1f, 1f);
				}
				Vector2 maxRawTextureHeightWidth = mB_TexSet2.GetMaxRawTextureHeightWidth();
				if (dRect.width * (double)maxRawTextureHeightWidth.x > (double)maxAtlasSize || dRect.height * (double)maxRawTextureHeightWidth.y > (double)maxAtlasSize)
				{
					num++;
					StringBuilder stringBuilder = null;
					if (LOG_LEVEL >= MB2_LogLevel.info)
					{
						stringBuilder = new StringBuilder();
						stringBuilder.AppendFormat("About To Merge:\n   TextureSet1 {0}\n   TextureSet2 {1}\n", mB_TexSet2.GetDescription(), mB_TexSet.GetDescription());
						if (LOG_LEVEL >= MB2_LogLevel.trace)
						{
							for (int n = 0; n < mB_TexSet2.matsAndGOs.mats.Count; n++)
							{
								stringBuilder.AppendFormat("tx1 Mat {0} matAndMeshUVRect {1} fullSamplingRect {2}\n", mB_TexSet2.matsAndGOs.mats[n].mat, mB_TexSet2.matsAndGOs.mats[n].samplingRectMatAndUVTiling, mB_TexSet2.ts[0].GetEncapsulatingSamplingRect());
							}
							for (int num3 = 0; num3 < mB_TexSet.matsAndGOs.mats.Count; num3++)
							{
								stringBuilder.AppendFormat("tx2 Mat {0} matAndMeshUVRect {1} fullSamplingRect {2}\n", mB_TexSet.matsAndGOs.mats[num3].mat, mB_TexSet.matsAndGOs.mats[num3].samplingRectMatAndUVTiling, mB_TexSet.ts[0].GetEncapsulatingSamplingRect());
							}
						}
					}
					for (int num4 = 0; num4 < mB_TexSet.matsAndGOs.gos.Count; num4++)
					{
						if (!mB_TexSet2.matsAndGOs.gos.Contains(mB_TexSet.matsAndGOs.gos[num4]))
						{
							mB_TexSet2.matsAndGOs.gos.Add(mB_TexSet.matsAndGOs.gos[num4]);
						}
					}
					for (int num5 = 0; num5 < mB_TexSet.matsAndGOs.mats.Count; num5++)
					{
						mB_TexSet2.matsAndGOs.mats.Add(mB_TexSet.matsAndGOs.mats[num5]);
					}
					mB_TexSet2.SetEncapsulatingSamplingRectWhenMergingTexSets(dRect);
					if (!list.Contains(i))
					{
						list.Add(i);
					}
					if (LOG_LEVEL < MB2_LogLevel.debug)
					{
						break;
					}
					if (LOG_LEVEL >= MB2_LogLevel.trace)
					{
						stringBuilder.AppendFormat("=== After Merge TextureSet {0}\n", mB_TexSet2.GetDescription());
						for (int num6 = 0; num6 < mB_TexSet2.matsAndGOs.mats.Count; num6++)
						{
							stringBuilder.AppendFormat("tx1 Mat {0} matAndMeshUVRect {1} fullSamplingRect {2}\n", mB_TexSet2.matsAndGOs.mats[num6].mat, mB_TexSet2.matsAndGOs.mats[num6].samplingRectMatAndUVTiling, mB_TexSet2.ts[0].GetEncapsulatingSamplingRect());
						}
						if (DO_INTEGRITY_CHECKS && DO_INTEGRITY_CHECKS)
						{
							DoIntegrityCheckMergedEncapsulatingSamplingRects(distinctMaterialTextures);
						}
					}
					Debug.Log(stringBuilder.ToString());
					break;
				}
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					string description = mB_TexSet2.GetDescription();
					DRect dRect2 = uvRect;
					string arg = description + dRect2.ToString();
					string description2 = mB_TexSet.GetDescription();
					dRect2 = uvRect2;
					Debug.Log($"Considered merging {arg} and {description2 + dRect2.ToString()} but there was not enough overlap. It is more efficient to bake these to separate rectangles.");
				}
			}
		}
		for (int num7 = list.Count - 1; num7 >= 0; num7--)
		{
			distinctMaterialTextures.RemoveAt(list[num7]);
		}
		list.Clear();
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			Debug.Log($"MergeDistinctMaterialTexturesThatWouldExceedMaxAtlasSizeAndCalcMaterialSubrects complete merged {num} now have {distinctMaterialTextures.Count}");
		}
		if (DO_INTEGRITY_CHECKS)
		{
			DoIntegrityCheckMergedEncapsulatingSamplingRects(distinctMaterialTextures);
		}
	}

	public void DoIntegrityCheckMergedEncapsulatingSamplingRects(List<MB_TexSet> distinctMaterialTextures)
	{
		if (!DO_INTEGRITY_CHECKS)
		{
			return;
		}
		for (int i = 0; i < distinctMaterialTextures.Count; i++)
		{
			MB_TexSet mB_TexSet = distinctMaterialTextures[i];
			if (!mB_TexSet.allTexturesUseSameMatTiling)
			{
				continue;
			}
			for (int j = 0; j < mB_TexSet.matsAndGOs.mats.Count; j++)
			{
				MatAndTransformToMerged matAndTransformToMerged = mB_TexSet.matsAndGOs.mats[j];
				DRect obUVRectIfTilingSame = matAndTransformToMerged.obUVRectIfTilingSame;
				DRect materialTiling = matAndTransformToMerged.materialTiling;
				if (!MB2_TextureBakeResults.IsMeshAndMaterialRectEnclosedByAtlasRect(mB_TexSet.tilingTreatment, obUVRectIfTilingSame.GetRect(), materialTiling.GetRect(), mB_TexSet.ts[0].GetEncapsulatingSamplingRect().GetRect(), MB2_LogLevel.info))
				{
					string[] obj = new string[11]
					{
						"mesh ",
						mB_TexSet.matsAndGOs.mats[j].objName,
						"\n uv=",
						null,
						null,
						null,
						null,
						null,
						null,
						null,
						null
					};
					DRect dRect = obUVRectIfTilingSame;
					obj[3] = dRect.ToString();
					obj[4] = "\n mat=";
					obj[5] = materialTiling.GetRect().ToString("f5");
					obj[6] = "\n samplingRect=";
					obj[7] = mB_TexSet.matsAndGOs.mats[j].samplingRectMatAndUVTiling.GetRect().ToString("f4");
					obj[8] = "\n encapsulatingRect ";
					obj[9] = mB_TexSet.ts[0].GetEncapsulatingSamplingRect().GetRect().ToString("f4");
					obj[10] = "\n";
					Debug.LogErrorFormat(string.Concat(obj));
					Debug.LogErrorFormat(string.Format("Integrity check failed. " + mB_TexSet.matsAndGOs.mats[j].objName + " Encapsulating sampling rect failed to contain potentialRect\n"));
					MB2_TextureBakeResults.IsMeshAndMaterialRectEnclosedByAtlasRect(mB_TexSet.tilingTreatment, obUVRectIfTilingSame.GetRect(), materialTiling.GetRect(), mB_TexSet.ts[0].GetEncapsulatingSamplingRect().GetRect(), MB2_LogLevel.trace);
				}
			}
		}
	}
}

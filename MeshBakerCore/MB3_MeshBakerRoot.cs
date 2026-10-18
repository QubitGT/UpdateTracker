using System.Collections.Generic;
using DigitalOpus.MB.Core;
using UnityEngine;

public abstract class MB3_MeshBakerRoot : MonoBehaviour
{
	public class ZSortObjects
	{
		public class Item
		{
			public GameObject go;

			public Vector3 point;
		}

		public class ItemComparer : IComparer<Item>
		{
			public int Compare(Item a, Item b)
			{
				return (int)Mathf.Sign(b.point.z - a.point.z);
			}
		}

		public Vector3 sortAxis;

		public void SortByDistanceAlongAxis(List<GameObject> gos)
		{
			if (sortAxis == Vector3.zero)
			{
				Debug.LogError("The sort axis cannot be the zero vector.");
				return;
			}
			Debug.Log("Z sorting meshes along axis numObjs=" + gos.Count);
			List<Item> list = new List<Item>();
			Quaternion quaternion = Quaternion.FromToRotation(sortAxis, Vector3.forward);
			for (int i = 0; i < gos.Count; i++)
			{
				if (gos[i] != null)
				{
					Item item = new Item();
					item.point = gos[i].transform.position;
					item.go = gos[i];
					item.point = quaternion * item.point;
					list.Add(item);
				}
			}
			list.Sort(new ItemComparer());
			for (int j = 0; j < gos.Count; j++)
			{
				gos[j] = list[j].go;
			}
		}
	}

	public Vector3 sortAxis;

	[HideInInspector]
	public abstract MB2_TextureBakeResults textureBakeResults { get; set; }

	public virtual List<GameObject> GetObjectsToCombine()
	{
		return null;
	}

	public virtual void PurgeNullsFromObjectsToCombine()
	{
	}

	public static bool DoCombinedValidate(MB3_MeshBakerRoot mom, MB_ObjsToCombineTypes objToCombineType, MB2_EditorMethodsInterface editorMethods, MB2_ValidationLevel validationLevel)
	{
		if (mom.textureBakeResults == null)
		{
			Debug.LogError("Need to set Texture Bake Result on " + mom);
			return false;
		}
		if (mom is MB3_MeshBakerCommon)
		{
			MB3_TextureBaker textureBaker = ((MB3_MeshBakerCommon)mom).GetTextureBaker();
			if (textureBaker != null && textureBaker.textureBakeResults != mom.textureBakeResults)
			{
				Debug.LogWarning("Texture Bake Result on this component is not the same as the Texture Bake Result on the MB3_TextureBaker.");
			}
		}
		List<GameObject> objectsToCombine = mom.GetObjectsToCombine();
		if (!ValidateTextureBakerGameObjects(mom, objectsToCombine, validationLevel))
		{
			return false;
		}
		if (mom is MB3_MeshBaker)
		{
			List<GameObject> objectsToCombine2 = mom.GetObjectsToCombine();
			if (objectsToCombine2 == null || objectsToCombine2.Count == 0)
			{
				Debug.LogError("No meshes to combine. Please assign some meshes to combine.");
				return false;
			}
			if (mom is MB3_MeshBaker && ((MB3_MeshBaker)mom).meshCombiner.settings.renderType == MB_RenderType.skinnedMeshRenderer && !editorMethods.ValidateSkinnedMeshes(objectsToCombine2))
			{
				return false;
			}
		}
		editorMethods?.CheckPrefabTypes(objToCombineType, objectsToCombine);
		return true;
	}

	public static bool ValidateTextureBakerGameObjects(MB3_MeshBakerRoot mom, List<GameObject> objsToMesh, MB2_ValidationLevel validationLevel)
	{
		Dictionary<int, MB_Utility.MeshAnalysisResult> dictionary = null;
		if (validationLevel == MB2_ValidationLevel.robust)
		{
			dictionary = new Dictionary<int, MB_Utility.MeshAnalysisResult>();
		}
		Dictionary<string, Material> dictionary2 = new Dictionary<string, Material>();
		for (int i = 0; i < objsToMesh.Count; i++)
		{
			GameObject gameObject = objsToMesh[i];
			if (gameObject == null)
			{
				Debug.LogError($"The list of objects to combine contains a null at position {i}. Select and use [shift + delete] to remove the object, or purge all null objects from the context menu.");
				return false;
			}
			for (int j = i + 1; j < objsToMesh.Count; j++)
			{
				if (objsToMesh[i] == objsToMesh[j])
				{
					Debug.LogError("The list of objects to combine contains duplicates at " + i + " and " + j);
					return false;
				}
			}
			Material[] gOMaterials = MB_Utility.GetGOMaterials(gameObject);
			if (gOMaterials.Length == 0)
			{
				Debug.LogError("Object " + gameObject?.ToString() + " in the list of objects to be combined does not have a material");
				return false;
			}
			Mesh mesh = MB_Utility.GetMesh(gameObject);
			if (mesh == null)
			{
				Debug.LogError("Object " + gameObject?.ToString() + " in the list of objects to be combined does not have a mesh");
				return false;
			}
			if (mesh != null && mom.textureBakeResults != null && Application.isEditor && !Application.isPlaying && mom.textureBakeResults.doMultiMaterial && validationLevel >= MB2_ValidationLevel.robust)
			{
				if (!dictionary.TryGetValue(mesh.GetInstanceID(), out var value))
				{
					MB_Utility.doSubmeshesShareVertsOrTris(mesh, ref value);
					dictionary.Add(mesh.GetInstanceID(), value);
				}
				if (value.hasOverlappingSubmeshVerts)
				{
					Debug.LogWarning("Object " + objsToMesh[i]?.ToString() + " in the list of objects to combine has overlapping submeshes (submeshes share vertices). If the UVs associated with the shared vertices are important then this bake may not work. If you are using multiple materials then this object can only be combined with objects that use the exact same set of textures (each atlas contains one texture). There may be other undesirable side affects as well. Mesh Master, available in the asset store can fix overlapping submeshes.");
				}
			}
			if (!MBVersion.IsUsingAddressables())
			{
				continue;
			}
			HashSet<string> hashSet = new HashSet<string>();
			for (int k = 0; k < gOMaterials.Length; k++)
			{
				if (!(gOMaterials[k] != null))
				{
					continue;
				}
				if (dictionary2.ContainsKey(gOMaterials[k].name))
				{
					if (gOMaterials[k] != dictionary2[gOMaterials[k].name])
					{
						hashSet.Add(gOMaterials[k].name);
					}
				}
				else
				{
					dictionary2.Add(gOMaterials[k].name, gOMaterials[k]);
				}
			}
			if (hashSet.Count > 0)
			{
				string[] array = new string[hashSet.Count];
				hashSet.CopyTo(array);
				string text = string.Join(",", array);
				Debug.LogError("The source objects use different materials that have the same name (" + text + "). If using addressables, materials with the same name are considered to be the same material when baking meshes at runtime. If you want to use this Material Bake Result at runtime then all source materials must have distinct names. Baking in edit-mode will still work.");
			}
		}
		return true;
	}
}

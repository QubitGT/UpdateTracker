using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Modio.API;
using Modio.API.SchemaDefinitions;
using Modio.Caching;
using Modio.Errors;
using Modio.Extensions;

namespace Modio.Mods;

public class ModDependencies
{
	private const int MAX_DEPTH = 5;

	private TaskCompletionSource<Error> _isFetchingDependencies;

	private List<Mod>[] _depthMap;

	private readonly Mod _dependent;

	private List<Mod> _flattenedMods;

	public int Count
	{
		get
		{
			if (!HasDependencies)
			{
				return 0;
			}
			List<Mod> flattenedMods = _flattenedMods;
			int num;
			if (flattenedMods == null)
			{
				List<Mod>[] depthMap = _depthMap;
				if (depthMap == null)
				{
					num = 0;
					goto IL_004f;
				}
				num = depthMap.Sum((List<Mod> list) => list.Count);
			}
			else
			{
				num = flattenedMods.Count;
			}
			if (num == 0)
			{
				goto IL_004f;
			}
			goto IL_0062;
			IL_004f:
			if (_isFetchingDependencies == null)
			{
				FetchDependencies().ForgetTaskSafely();
			}
			goto IL_0062;
			IL_0062:
			return num;
		}
	}

	public bool HasDependencies { get; }

	public bool IsMapped => _depthMap != null;

	internal ModDependencies(Mod dependent, bool hasDependencies)
	{
		HasDependencies = hasDependencies;
		_dependent = dependent;
	}

	public async Task<(Error error, IReadOnlyList<Mod> results)> GetAllDependencies()
	{
		if (!IsMapped)
		{
			Error error = await FetchDependencies();
			if ((bool)error)
			{
				return (error: error, results: Array.Empty<Mod>());
			}
		}
		return (error: Error.None, results: _flattenedMods);
	}

	private async Task<Error> FetchDependencies()
	{
		if (!HasDependencies)
		{
			ModioLog.Warning?.Log("Attempting to get dependencies when none exist for this mod!");
			return new Error(ErrorCode.BAD_PARAMETER);
		}
		if (_isFetchingDependencies != null)
		{
			await _isFetchingDependencies.Task;
		}
		if (IsMapped)
		{
			return Error.None;
		}
		List<Mod>[] newMap = new List<Mod>[6];
		for (int i = 0; i < newMap.Length; i++)
		{
			newMap[i] = new List<Mod>();
		}
		ModioAPI.Dependencies.GetModDependenciesFilter filter = ModioAPI.Dependencies.FilterGetModDependencies().Recursive(recursive: true);
		_isFetchingDependencies = new TaskCompletionSource<Error>();
		while (true)
		{
			var (error, pagination) = await ModioAPI.Dependencies.GetModDependencies(_dependent.Id, filter);
			if ((bool)error)
			{
				if (!error.IsSilent)
				{
					ModioLog.Error?.Log($"Error getting dependencies for mod {_dependent}: {error.GetMessage()}");
				}
				_isFetchingDependencies.SetResult(error);
				_isFetchingDependencies = null;
				return error;
			}
			ModDependenciesObject[] data = pagination.Value.Data;
			for (int j = 0; j < data.Length; j++)
			{
				ModDependenciesObject dependency = data[j];
				if (dependency.Visible == 0L)
				{
					ModioLog.Error?.Log($"Mod {_dependent} has incompatible dependency {dependency.Id}!");
					return new Error(ErrorCode.INCOMPATIBLE_DEPENDENCIES);
				}
				newMap[(int)dependency.DependencyDepth].Add(ModCache.GetMod(ConstructModObject(dependency)));
			}
			if (pagination.Value.ResultOffset + pagination.Value.ResultCount >= pagination.Value.ResultTotal)
			{
				break;
			}
			filter.PageIndex++;
		}
		_depthMap = newMap;
		_flattenedMods = new List<Mod>();
		for (int num = 5; num >= 0; num--)
		{
			_flattenedMods.AddRange(_depthMap[num]);
		}
		_flattenedMods = _flattenedMods.Distinct().ToList();
		_isFetchingDependencies.SetResult(Error.None);
		_isFetchingDependencies = null;
		_dependent.InvokeModUpdated(ModChangeType.Dependencies);
		return Error.None;
	}

	private static ModObject ConstructModObject(ModDependenciesObject dependency)
	{
		return new ModObject(dependency.Id, dependency.GameId, dependency.Status, dependency.Visible, dependency.SubmittedBy, dependency.DateAdded, dependency.DateUpdated, dependency.DateLive, dependency.MaturityOption, dependency.CommunityOptions, dependency.MonetizationOptions, 0L, dependency.Stock, dependency.Price, dependency.Tax, dependency.Logo, dependency.HomepageUrl, dependency.Name, dependency.NameId, dependency.Summary, dependency.Description, dependency.DescriptionPlaintext, dependency.MetadataBlob, dependency.ProfileUrl, dependency.Media, dependency.Modfile, dependency.Dependencies, dependency.Platforms, dependency.MetadataKvp, dependency.Tags, dependency.Stats);
	}
}

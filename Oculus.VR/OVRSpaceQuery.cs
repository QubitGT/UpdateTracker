using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

internal static class OVRSpaceQuery
{
	[StructLayout(LayoutKind.Explicit)]
	private struct QueryInfoUnion
	{
		[FieldOffset(0)]
		public OVRPlugin.SpaceQueryInfo V1;

		[FieldOffset(0)]
		public OVRPlugin.SpaceQueryInfo2 V2;
	}

	[Obsolete("This helper is for obsolete usages of xrQuerySpacesFB. See OVRAnchor.FetchAnchorsAsync.")]
	public struct Options
	{
		public const int MaxUuidCount = 1024;

		private OVRPlugin.SpaceComponentType _componentType;

		private IEnumerable<Guid> _uuidFilter;

		private Guid? _groupFilter;

		public int MaxResults { get; set; }

		public double Timeout { get; set; }

		public OVRSpace.StorageLocation Location { get; set; }

		public OVRPlugin.SpaceQueryType QueryType { get; set; }

		public OVRPlugin.SpaceQueryActionType ActionType { get; set; }

		public OVRPlugin.SpaceComponentType ComponentFilter
		{
			get
			{
				return _componentType;
			}
			set
			{
				ValidateSingleFilter(_uuidFilter, value, _groupFilter);
				_componentType = value;
			}
		}

		public IEnumerable<Guid> UuidFilter
		{
			get
			{
				return _uuidFilter;
			}
			set
			{
				ValidateSingleFilter(value, _componentType, _groupFilter);
				if (value is IReadOnlyCollection<Guid> { Count: >1024 } readOnlyCollection)
				{
					throw new ArgumentException(string.Format("There must not be more than {0} UUIDs specified by the {1} (new value contains {2} UUIDs).", 1024, "UuidFilter", readOnlyCollection.Count), "value");
				}
				_uuidFilter = value;
			}
		}

		public Guid? GroupFilter
		{
			get
			{
				return _groupFilter;
			}
			set
			{
				ValidateSingleFilter(_uuidFilter, _componentType, value);
				_groupFilter = value;
			}
		}

		public OVRPlugin.SpaceQueryInfo ToQueryInfo()
		{
			OVRPlugin.Result result;
			string message;
			OVRPlugin.SpaceQueryInfo2 query;
			if (_uuidFilter == null)
			{
				(result, message) = ForComponent(_componentType, out query);
			}
			else
			{
				(result, message) = ForAnchors(_uuidFilter, out query);
			}
			if (result.IsSuccess())
			{
				return query.ToV1();
			}
			if (result == OVRPlugin.Result.Failure_InvalidParameter)
			{
				throw new InvalidOperationException(string.Format("{0} must not contain more than {1} UUIDs.", "UuidFilter", 1024));
			}
			throw new InvalidOperationException(message);
		}

		public OVRPlugin.SpaceQueryInfo2 ToQueryInfo2()
		{
			OVRPlugin.Result result;
			string message;
			OVRPlugin.SpaceQueryInfo2 query;
			if (_groupFilter.HasValue)
			{
				(result, message) = ForGroup(_groupFilter.Value, out query, _uuidFilter);
			}
			else if (_uuidFilter == null)
			{
				(result, message) = ForComponent(_componentType, out query);
			}
			else
			{
				(result, message) = ForAnchors(_uuidFilter, out query);
			}
			if (result.IsSuccess())
			{
				return query;
			}
			if (result == OVRPlugin.Result.Failure_InvalidParameter)
			{
				throw new InvalidOperationException(string.Format("{0} must not contain more than {1} UUIDs.", "UuidFilter", 1024));
			}
			throw new InvalidOperationException(message);
		}

		public bool TryQuerySpaces(out ulong requestId)
		{
			return OVRPlugin.QuerySpaces(ToQueryInfo(), out requestId);
		}

		private static void ValidateSingleFilter(IEnumerable<Guid> uuidFilter, OVRPlugin.SpaceComponentType componentFilter, Guid? groupFilter)
		{
			int num = 0;
			if (uuidFilter != null)
			{
				num++;
			}
			if (groupFilter.HasValue)
			{
				num++;
			}
			if (componentFilter != 0)
			{
				num++;
			}
			if (num > 1)
			{
				throw new InvalidOperationException("You may only query by one of UUID, Group, or component type.");
			}
		}
	}

	public const int MaxResultsForAnchors = 1024;

	public const int MaxResultsForGroup = 1024;

	public const OVRPlugin.SpaceStorageLocation DefaultStorageLocation = OVRPlugin.SpaceStorageLocation.Cloud;

	public const double DefaultTimeout = 0.0;

	private static readonly Guid[] s_Ids = new Guid[1024];

	private static readonly OVRPlugin.SpaceComponentType[] s_ComponentTypes = new OVRPlugin.SpaceComponentType[16];

	private static readonly OVRPlugin.SpaceQueryInfo2 s_TemplateQuery = new OVRPlugin.SpaceQueryInfo2
	{
		QueryType = OVRPlugin.SpaceQueryType.Action,
		ActionType = OVRPlugin.SpaceQueryActionType.Load,
		Location = OVRPlugin.SpaceStorageLocation.Cloud,
		Timeout = 0.0,
		IdInfo = new OVRPlugin.SpaceFilterInfoIds
		{
			Ids = s_Ids
		},
		ComponentsInfo = new OVRPlugin.SpaceFilterInfoComponents
		{
			Components = s_ComponentTypes
		}
	};

	public static (OVRPlugin.Result result, string why) ForAnchors([CanBeNull] IEnumerable<Guid> anchorIds, out OVRPlugin.SpaceQueryInfo2 query)
	{
		query = s_TemplateQuery;
		query.FilterType = OVRPlugin.SpaceQueryFilterType.Ids;
		query.MaxQuerySpaces = 1024;
		return AppendAnchors(ref query, anchorIds);
	}

	internal static OVRPlugin.SpaceQueryInfo2 ForAnchorsUnchecked(OVREnumerable<Guid> anchorIds)
	{
		OVRPlugin.SpaceQueryInfo2 query = s_TemplateQuery;
		query.FilterType = OVRPlugin.SpaceQueryFilterType.Ids;
		query.MaxQuerySpaces = 1024;
		foreach (Guid item in anchorIds)
		{
			query.IdInfo.Ids[query.IdInfo.NumIds++] = item;
		}
		PostProcessQuery(ref query, OVRPlugin.Result.Success, in string.Empty);
		return query;
	}

	internal static OVRPlugin.SpaceQueryInfo2 ForAnchorsThrow([NotNull] IEnumerable<Guid> anchorIds, string argName = null)
	{
		OVRPlugin.Result result;
		string arg;
		(result, arg) = ForAnchors(anchorIds, out var query);
		if (result.IsSuccess())
		{
			return query;
		}
		arg = $"{arg} ({(int)result} {result})";
		if (result == OVRPlugin.Result.Failure_HandleInvalid || result == OVRPlugin.Result.Failure_InvalidParameter)
		{
			throw new ArgumentException(arg, argName);
		}
		throw new InvalidOperationException(arg);
	}

	public static (OVRPlugin.Result result, string why) ForComponent(OVRPlugin.SpaceComponentType type, out OVRPlugin.SpaceQueryInfo2 query)
	{
		query = s_TemplateQuery;
		query.FilterType = OVRPlugin.SpaceQueryFilterType.Components;
		query.Location = OVRPlugin.SpaceStorageLocation.Local;
		query.MaxQuerySpaces = 1024;
		query.ComponentsInfo.Components[0] = type;
		query.ComponentsInfo.NumComponents = 1;
		return PostProcessQuery(ref query, OVRPlugin.Result.Success, in string.Empty);
	}

	internal static OVRPlugin.SpaceQueryInfo2 ForComponentUnchecked(OVRPlugin.SpaceComponentType type)
	{
		OVRPlugin.SpaceQueryInfo2 query = s_TemplateQuery;
		query.FilterType = OVRPlugin.SpaceQueryFilterType.Components;
		query.Location = OVRPlugin.SpaceStorageLocation.Local;
		query.MaxQuerySpaces = 1024;
		query.ComponentsInfo.Components[0] = type;
		query.ComponentsInfo.NumComponents = 1;
		PostProcessQuery(ref query, OVRPlugin.Result.Success, in string.Empty);
		return query;
	}

	internal static OVRPlugin.SpaceQueryInfo2 ForComponentThrow(OVRPlugin.SpaceComponentType type, string argName = null)
	{
		OVRPlugin.Result result;
		string arg;
		(result, arg) = ForComponent(type, out var query);
		if (result.IsSuccess())
		{
			return query;
		}
		arg = $"{arg} ({(int)result} {result})";
		if (result == OVRPlugin.Result.Failure_InvalidParameter)
		{
			throw new ArgumentException(arg, argName);
		}
		throw new InvalidOperationException(arg);
	}

	public static (OVRPlugin.Result result, string why) ForGroup(Guid groupUuid, out OVRPlugin.SpaceQueryInfo2 query, IEnumerable<Guid> anchorIds = null)
	{
		query = s_TemplateQuery;
		query.FilterType = OVRPlugin.SpaceQueryFilterType.Group;
		query.MaxQuerySpaces = 1024;
		query.GroupUuidInfo = groupUuid;
		OVRPlugin.Result result = OVRPlugin.Result.Success;
		string why = string.Empty;
		if (groupUuid == Guid.Empty)
		{
			result = OVRPlugin.Result.Failure_InvalidParameter;
		}
		else if (anchorIds != null)
		{
			return AppendAnchors(ref query, anchorIds);
		}
		return PostProcessQuery(ref query, result, in why);
	}

	internal static OVRPlugin.SpaceQueryInfo2 ForGroupUnchecked(Guid groupUuid, OVREnumerable<Guid> anchorIds = default(OVREnumerable<Guid>))
	{
		OVRPlugin.SpaceQueryInfo2 query = s_TemplateQuery;
		query.FilterType = OVRPlugin.SpaceQueryFilterType.Group;
		query.MaxQuerySpaces = 1024;
		query.GroupUuidInfo = groupUuid;
		foreach (Guid item in anchorIds)
		{
			query.IdInfo.Ids[query.IdInfo.NumIds++] = item;
		}
		PostProcessQuery(ref query, OVRPlugin.Result.Success, in string.Empty);
		return query;
	}

	internal static OVRPlugin.SpaceQueryInfo2 ForGroupThrow(Guid groupUuid, string argName = null, IEnumerable<Guid> anchorIds = null)
	{
		OVRPlugin.Result result;
		string arg;
		(result, arg) = ForGroup(groupUuid, out var query, anchorIds);
		if (result.IsSuccess())
		{
			return query;
		}
		arg = $"{arg} ({(int)result} {result})";
		if (result == OVRPlugin.Result.Failure_HandleInvalid || result == OVRPlugin.Result.Failure_InvalidParameter)
		{
			throw new ArgumentException(arg, argName);
		}
		throw new InvalidOperationException(arg);
	}

	public static OVRPlugin.SpaceQueryInfo ToV1(this in OVRPlugin.SpaceQueryInfo2 query2)
	{
		QueryInfoUnion queryInfoUnion = default(QueryInfoUnion);
		queryInfoUnion.V2 = query2;
		return queryInfoUnion.V1;
	}

	public static OVRPlugin.SpaceQueryInfo2 ToV2(this in OVRPlugin.SpaceQueryInfo query1)
	{
		QueryInfoUnion queryInfoUnion = default(QueryInfoUnion);
		queryInfoUnion.V1 = query1;
		return queryInfoUnion.V2;
	}

	private static (OVRPlugin.Result result, string why) AppendAnchors(ref OVRPlugin.SpaceQueryInfo2 query, IEnumerable<Guid> anchorIds)
	{
		OVRPlugin.Result result = OVRPlugin.Result.Success;
		string why = string.Empty;
		if (query.FilterType != OVRPlugin.SpaceQueryFilterType.Ids && query.FilterType != OVRPlugin.SpaceQueryFilterType.Group)
		{
			result = OVRPlugin.Result.Failure_InvalidOperation;
			return PostProcessQuery(ref query, result, in why);
		}
		foreach (Guid item in anchorIds.ToNonAlloc())
		{
			if (query.IdInfo.NumIds >= query.MaxQuerySpaces)
			{
				result = OVRPlugin.Result.Failure_InvalidParameter;
				return PostProcessQuery(ref query, result, in why);
			}
			query.IdInfo.Ids[query.IdInfo.NumIds++] = item;
		}
		return PostProcessQuery(ref query, result, in why);
	}

	private static (OVRPlugin.Result result, string why) PostProcessQuery(ref OVRPlugin.SpaceQueryInfo2 query, OVRPlugin.Result result, in string why)
	{
		if (result.IsSuccess())
		{
			if (query.MaxQuerySpaces > query.IdInfo.NumIds && query.IdInfo.NumIds > 0)
			{
				query.MaxQuerySpaces = query.IdInfo.NumIds;
			}
		}
		else
		{
			query.MaxQuerySpaces = 0;
			query.IdInfo.NumIds = 0;
		}
		return (result: result, why: why);
	}
}

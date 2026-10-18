using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Modio.API.Interfaces;
using Modio.API.SchemaDefinitions;
using Modio.Authentication;
using Modio.Errors;
using Newtonsoft.Json.Linq;

namespace Modio.API;

public static class ModioAPI
{
	public static class Media
	{
		internal static async Task<(Error error, JToken updateGameMediaResponse)> AddGameMediaAsJToken(AddGameMediaRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), updateGameMediaResponse: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/games/{game-id}/media", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.MultipartFormData);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, UpdateGameMediaResponse? updateGameMediaResponse)> AddGameMedia(AddGameMediaRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), updateGameMediaResponse: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/games/{game-id}/media", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.MultipartFormData);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<UpdateGameMediaResponse>(request);
		}

		internal static async Task<(Error error, JToken updateModMediaResponse)> AddModMediaAsJToken(long modId, AddModMediaRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), updateModMediaResponse: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/media", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.MultipartFormData);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, UpdateModMediaResponse? updateModMediaResponse)> AddModMedia(long modId, AddModMediaRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), updateModMediaResponse: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/media", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.MultipartFormData);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<UpdateModMediaResponse>(request);
		}

		internal static async Task<(Error error, JToken response204)> DeleteModMediaAsJToken(long modId, DeleteModMediaRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), response204: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/media", ModioAPIRequestMethod.Delete, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, Response204? response204)> DeleteModMedia(long modId, DeleteModMediaRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), response204: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/media", ModioAPIRequestMethod.Delete, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<Response204>(request);
		}

		internal static async Task<(Error error, JToken response204)> ReorderModMediaAsJToken(long modId, DeleteModMediaRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), response204: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/media/reorder", ModioAPIRequestMethod.Put, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, Response204? response204)> ReorderModMedia(long modId, DeleteModMediaRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), response204: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/media/reorder", ModioAPIRequestMethod.Put, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<Response204>(request);
		}
	}

	public static class Mods
	{
		public class GetModsFilter : SearchFilter<GetModsFilter>
		{
			internal GetModsFilter(int pageIndex, int pageSize)
				: base(pageIndex, pageSize)
			{
			}

			public GetModsFilter Id(long id, Filtering condition = Filtering.None)
			{
				Parameters["id" + condition.ClearText()] = id;
				return this;
			}

			public GetModsFilter Id(ICollection<long> id, Filtering condition = Filtering.None)
			{
				Parameters["id" + condition.ClearText()] = id;
				return this;
			}

			public GetModsFilter GameId(long gameId, Filtering condition = Filtering.None)
			{
				Parameters["game_id" + condition.ClearText()] = gameId;
				return this;
			}

			public GetModsFilter GameId(ICollection<long> gameId, Filtering condition = Filtering.None)
			{
				Parameters["game_id" + condition.ClearText()] = gameId;
				return this;
			}

			public GetModsFilter Status(long status, Filtering condition = Filtering.None)
			{
				Parameters["status" + condition.ClearText()] = status;
				return this;
			}

			public GetModsFilter Status(ICollection<long> status, Filtering condition = Filtering.None)
			{
				Parameters["status" + condition.ClearText()] = status;
				return this;
			}

			public GetModsFilter Visible(long visible, Filtering condition = Filtering.None)
			{
				Parameters["visible" + condition.ClearText()] = visible;
				return this;
			}

			public GetModsFilter Visible(ICollection<long> visible, Filtering condition = Filtering.None)
			{
				Parameters["visible" + condition.ClearText()] = visible;
				return this;
			}

			public GetModsFilter SubmittedBy(long submittedBy, Filtering condition = Filtering.None)
			{
				Parameters["submitted_by" + condition.ClearText()] = submittedBy;
				return this;
			}

			public GetModsFilter SubmittedBy(ICollection<long> submittedBy, Filtering condition = Filtering.None)
			{
				Parameters["submitted_by" + condition.ClearText()] = submittedBy;
				return this;
			}

			public GetModsFilter SubmittedByDisplayName(string submittedByDisplayName, Filtering condition = Filtering.None)
			{
				Parameters["submitted_by_display_name" + condition.ClearText()] = submittedByDisplayName;
				return this;
			}

			public GetModsFilter SubmittedByDisplayName(ICollection<string> submittedByDisplayName, Filtering condition = Filtering.None)
			{
				Parameters["submitted_by_display_name" + condition.ClearText()] = submittedByDisplayName;
				return this;
			}

			public GetModsFilter DateAdded(long dateAdded, Filtering condition = Filtering.None)
			{
				Parameters["date_added" + condition.ClearText()] = dateAdded;
				return this;
			}

			public GetModsFilter DateAdded(ICollection<long> dateAdded, Filtering condition = Filtering.None)
			{
				Parameters["date_added" + condition.ClearText()] = dateAdded;
				return this;
			}

			public GetModsFilter DateUpdated(long dateUpdated, Filtering condition = Filtering.None)
			{
				Parameters["date_updated" + condition.ClearText()] = dateUpdated;
				return this;
			}

			public GetModsFilter DateUpdated(ICollection<long> dateUpdated, Filtering condition = Filtering.None)
			{
				Parameters["date_updated" + condition.ClearText()] = dateUpdated;
				return this;
			}

			public GetModsFilter DateLive(long dateLive, Filtering condition = Filtering.None)
			{
				Parameters["date_live" + condition.ClearText()] = dateLive;
				return this;
			}

			public GetModsFilter DateLive(ICollection<long> dateLive, Filtering condition = Filtering.None)
			{
				Parameters["date_live" + condition.ClearText()] = dateLive;
				return this;
			}

			public GetModsFilter CommunityOptions(long communityOptions, Filtering condition = Filtering.None)
			{
				Parameters["community_options" + condition.ClearText()] = communityOptions;
				return this;
			}

			public GetModsFilter CommunityOptions(ICollection<long> communityOptions, Filtering condition = Filtering.None)
			{
				Parameters["community_options" + condition.ClearText()] = communityOptions;
				return this;
			}

			public GetModsFilter MaturityOption(long maturityOption, Filtering condition = Filtering.None)
			{
				Parameters["maturity_option" + condition.ClearText()] = maturityOption;
				return this;
			}

			public GetModsFilter MaturityOption(ICollection<long> maturityOption, Filtering condition = Filtering.None)
			{
				Parameters["maturity_option" + condition.ClearText()] = maturityOption;
				return this;
			}

			public GetModsFilter MonetizationOptions(long monetizationOptions, Filtering condition = Filtering.None)
			{
				Parameters["monetization_options" + condition.ClearText()] = monetizationOptions;
				return this;
			}

			public GetModsFilter MonetizationOptions(ICollection<long> monetizationOptions, Filtering condition = Filtering.None)
			{
				Parameters["monetization_options" + condition.ClearText()] = monetizationOptions;
				return this;
			}

			public GetModsFilter Name(string name, Filtering condition = Filtering.None)
			{
				Parameters["name" + condition.ClearText()] = name;
				return this;
			}

			public GetModsFilter Name(ICollection<string> name, Filtering condition = Filtering.None)
			{
				Parameters["name" + condition.ClearText()] = name;
				return this;
			}

			public GetModsFilter NameId(string nameId, Filtering condition = Filtering.None)
			{
				Parameters["name_id" + condition.ClearText()] = nameId;
				return this;
			}

			public GetModsFilter NameId(ICollection<string> nameId, Filtering condition = Filtering.None)
			{
				Parameters["name_id" + condition.ClearText()] = nameId;
				return this;
			}

			public GetModsFilter Modfile(long modfile, Filtering condition = Filtering.None)
			{
				Parameters["modfile" + condition.ClearText()] = modfile;
				return this;
			}

			public GetModsFilter Modfile(ICollection<long> modfile, Filtering condition = Filtering.None)
			{
				Parameters["modfile" + condition.ClearText()] = modfile;
				return this;
			}

			public GetModsFilter MetadataBlob(string metadataBlob, Filtering condition = Filtering.None)
			{
				Parameters["metadata_blob" + condition.ClearText()] = metadataBlob;
				return this;
			}

			public GetModsFilter MetadataBlob(ICollection<string> metadataBlob, Filtering condition = Filtering.None)
			{
				Parameters["metadata_blob" + condition.ClearText()] = metadataBlob;
				return this;
			}

			public GetModsFilter MetadataKvp(string metadataKvp, Filtering condition = Filtering.None)
			{
				Parameters["metadata_kvp" + condition.ClearText()] = metadataKvp;
				return this;
			}

			public GetModsFilter MetadataKvp(ICollection<string> metadataKvp, Filtering condition = Filtering.None)
			{
				Parameters["metadata_kvp" + condition.ClearText()] = metadataKvp;
				return this;
			}

			public GetModsFilter Tags(string tags, Filtering condition = Filtering.None)
			{
				Parameters["tags" + condition.ClearText()] = tags;
				return this;
			}

			public GetModsFilter Tags(ICollection<string> tags, Filtering condition = Filtering.None)
			{
				Parameters["tags" + condition.ClearText()] = tags;
				return this;
			}

			public GetModsFilter PlatformStatus(string platformStatus, Filtering condition = Filtering.None)
			{
				Parameters["platform_status" + condition.ClearText()] = platformStatus;
				return this;
			}

			public GetModsFilter PlatformStatus(ICollection<string> platformStatus, Filtering condition = Filtering.None)
			{
				Parameters["platform_status" + condition.ClearText()] = platformStatus;
				return this;
			}

			public GetModsFilter RevenueType(long revenueType, Filtering condition = Filtering.None)
			{
				Parameters["revenue_type" + condition.ClearText()] = revenueType;
				return this;
			}

			public GetModsFilter RevenueType(ICollection<long> revenueType, Filtering condition = Filtering.None)
			{
				Parameters["revenue_type" + condition.ClearText()] = revenueType;
				return this;
			}

			public GetModsFilter Stock(long stock, Filtering condition = Filtering.None)
			{
				Parameters["stock" + condition.ClearText()] = stock;
				return this;
			}

			public GetModsFilter Stock(ICollection<long> stock, Filtering condition = Filtering.None)
			{
				Parameters["stock" + condition.ClearText()] = stock;
				return this;
			}

			public GetModsFilter SortByStringType(string key, bool ascending = true)
			{
				Parameters["_sort"] = (ascending ? "" : "-") + key;
				return this;
			}
		}

		internal static async Task<(Error error, JToken modObject)> AddModAsJToken(AddModRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), modObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/games/{game-id}/mods", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.MultipartFormData);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, ModObject? modObject)> AddMod(AddModRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), modObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/games/{game-id}/mods", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.MultipartFormData);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<ModObject>(request);
		}

		internal static async Task<(Error error, JToken response204)> DeleteModAsJToken(long modId)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), response204: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}", ModioAPIRequestMethod.Delete, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, Response204? response204)> DeleteMod(long modId)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), response204: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}", ModioAPIRequestMethod.Delete, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<Response204>(request);
		}

		internal static async Task<(Error error, JToken modObject)> EditModAsJToken(long modId, EditModRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), modObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.MultipartFormData);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, ModObject? modObject)> EditMod(long modId, EditModRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), modObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.MultipartFormData);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<ModObject>(request);
		}

		internal static async Task<(Error error, JToken modObject)> GetModAsJToken(long modId)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), modObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}");
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, ModObject? modObject)> GetMod(long modId)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), modObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}");
			return await _apiInterface.GetJson<ModObject>(request);
		}

		internal static async Task<(Error error, JToken modObjects)> GetModsAsJToken(GetModsFilter filter)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), modObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/games/{game-id}/mods");
			request.Options.AddFilterParameters(filter);
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, Pagination<ModObject[]>? modObjects)> GetMods(GetModsFilter filter)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), modObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/games/{game-id}/mods");
			request.Options.AddFilterParameters(filter);
			return await _apiInterface.GetJson<Pagination<ModObject[]>>(request);
		}

		public static GetModsFilter FilterGetMods(int pageIndex = 0, int pageSize = 100)
		{
			return new GetModsFilter(pageIndex, pageSize);
		}
	}

	public static class Comments
	{
		public class GetModCommentsFilter : SearchFilter<GetModCommentsFilter>
		{
			internal GetModCommentsFilter(int pageIndex, int pageSize)
				: base(pageIndex, pageSize)
			{
			}

			public GetModCommentsFilter Id(long id, Filtering condition = Filtering.None)
			{
				Parameters["id" + condition.ClearText()] = id;
				return this;
			}

			public GetModCommentsFilter Id(ICollection<long> id, Filtering condition = Filtering.None)
			{
				Parameters["id" + condition.ClearText()] = id;
				return this;
			}

			public GetModCommentsFilter ModId(long modId, Filtering condition = Filtering.None)
			{
				Parameters["mod_id" + condition.ClearText()] = modId;
				return this;
			}

			public GetModCommentsFilter ModId(ICollection<long> modId, Filtering condition = Filtering.None)
			{
				Parameters["mod_id" + condition.ClearText()] = modId;
				return this;
			}

			public GetModCommentsFilter ResourceId(long resourceId, Filtering condition = Filtering.None)
			{
				Parameters["resource_id" + condition.ClearText()] = resourceId;
				return this;
			}

			public GetModCommentsFilter ResourceId(ICollection<long> resourceId, Filtering condition = Filtering.None)
			{
				Parameters["resource_id" + condition.ClearText()] = resourceId;
				return this;
			}

			public GetModCommentsFilter SubmittedBy(long submittedBy, Filtering condition = Filtering.None)
			{
				Parameters["submitted_by" + condition.ClearText()] = submittedBy;
				return this;
			}

			public GetModCommentsFilter SubmittedBy(ICollection<long> submittedBy, Filtering condition = Filtering.None)
			{
				Parameters["submitted_by" + condition.ClearText()] = submittedBy;
				return this;
			}

			public GetModCommentsFilter DateAdded(long dateAdded, Filtering condition = Filtering.None)
			{
				Parameters["date_added" + condition.ClearText()] = dateAdded;
				return this;
			}

			public GetModCommentsFilter DateAdded(ICollection<long> dateAdded, Filtering condition = Filtering.None)
			{
				Parameters["date_added" + condition.ClearText()] = dateAdded;
				return this;
			}

			public GetModCommentsFilter ReplyId(long replyId, Filtering condition = Filtering.None)
			{
				Parameters["reply_id" + condition.ClearText()] = replyId;
				return this;
			}

			public GetModCommentsFilter ReplyId(ICollection<long> replyId, Filtering condition = Filtering.None)
			{
				Parameters["reply_id" + condition.ClearText()] = replyId;
				return this;
			}

			public GetModCommentsFilter ThreadPosition(string threadPosition, Filtering condition = Filtering.None)
			{
				Parameters["thread_position" + condition.ClearText()] = threadPosition;
				return this;
			}

			public GetModCommentsFilter ThreadPosition(ICollection<string> threadPosition, Filtering condition = Filtering.None)
			{
				Parameters["thread_position" + condition.ClearText()] = threadPosition;
				return this;
			}

			public GetModCommentsFilter Karma(long karma, Filtering condition = Filtering.None)
			{
				Parameters["karma" + condition.ClearText()] = karma;
				return this;
			}

			public GetModCommentsFilter Karma(ICollection<long> karma, Filtering condition = Filtering.None)
			{
				Parameters["karma" + condition.ClearText()] = karma;
				return this;
			}

			public GetModCommentsFilter Content(string content, Filtering condition = Filtering.None)
			{
				Parameters["content" + condition.ClearText()] = content;
				return this;
			}

			public GetModCommentsFilter Content(ICollection<string> content, Filtering condition = Filtering.None)
			{
				Parameters["content" + condition.ClearText()] = content;
				return this;
			}
		}

		internal static async Task<(Error error, JToken commentObject)> AddModCommentAsJToken(long modId, AddCommentRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), commentObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/comments", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, CommentObject? commentObject)> AddModComment(long modId, AddCommentRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), commentObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/comments", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<CommentObject>(request);
		}

		internal static async Task<(Error error, JToken commentObject)> AddModCommentKarmaAsJToken(long modId, long commentId, UpdateCommentKarmaRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), commentObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/comments/{commentId}/karma", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, CommentObject? commentObject)> AddModCommentKarma(long modId, long commentId, UpdateCommentKarmaRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), commentObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/comments/{commentId}/karma", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<CommentObject>(request);
		}

		internal static async Task<(Error error, JToken response204)> DeleteModCommentAsJToken(long modId, long commentId)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), response204: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/comments/{commentId}", ModioAPIRequestMethod.Delete, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, Response204? response204)> DeleteModComment(long modId, long commentId)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), response204: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/comments/{commentId}", ModioAPIRequestMethod.Delete, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<Response204>(request);
		}

		internal static async Task<(Error error, JToken commentObject)> GetModCommentAsJToken(long modId, long commentId)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), commentObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/comments/{commentId}");
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, CommentObject? commentObject)> GetModComment(long modId, long commentId)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), commentObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/comments/{commentId}");
			return await _apiInterface.GetJson<CommentObject>(request);
		}

		internal static async Task<(Error error, JToken commentObjects)> GetModCommentsAsJToken(long modId, GetModCommentsFilter filter)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), commentObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/comments", ModioAPIRequestMethod.Get, ModioAPIRequestContentType.FormUrlEncoded);
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, Pagination<CommentObject[]>? commentObjects)> GetModComments(long modId, GetModCommentsFilter filter)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), commentObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/comments", ModioAPIRequestMethod.Get, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddFilterParameters(filter);
			return await _apiInterface.GetJson<Pagination<CommentObject[]>>(request);
		}

		public static GetModCommentsFilter FilterGetModComments(int pageIndex = 0, int pageSize = 100)
		{
			return new GetModCommentsFilter(pageIndex, pageSize);
		}

		internal static async Task<(Error error, JToken commentObject)> UpdateModCommentAsJToken(long modId, long commentId, UpdateCommentRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), commentObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/comments/{commentId}", ModioAPIRequestMethod.Put, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, CommentObject? commentObject)> UpdateModComment(long modId, long commentId, UpdateCommentRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), commentObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/comments/{commentId}", ModioAPIRequestMethod.Put, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<CommentObject>(request);
		}
	}

	public static class Dependencies
	{
		public class GetModDependantsFilter : SearchFilter<GetModDependantsFilter>
		{
			internal GetModDependantsFilter(int pageIndex, int pageSize)
				: base(pageIndex, pageSize)
			{
			}
		}

		public class GetModDependenciesFilter : SearchFilter<GetModDependenciesFilter>
		{
			internal GetModDependenciesFilter(int pageIndex, int pageSize)
				: base(pageIndex, pageSize)
			{
			}

			public GetModDependenciesFilter Recursive(bool recursive, Filtering condition = Filtering.None)
			{
				Parameters["recursive" + condition.ClearText()] = recursive;
				return this;
			}

			public GetModDependenciesFilter Recursive(ICollection<bool> recursive, Filtering condition = Filtering.None)
			{
				Parameters["recursive" + condition.ClearText()] = recursive;
				return this;
			}
		}

		internal static async Task<(Error error, JToken addModDependenciesResponse)> AddModDependenciesAsJToken(long modId, AddModDependenciesRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), addModDependenciesResponse: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/dependencies", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, AddModDependenciesResponse? addModDependenciesResponse)> AddModDependencies(long modId, AddModDependenciesRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), addModDependenciesResponse: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/dependencies", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<AddModDependenciesResponse>(request);
		}

		internal static async Task<(Error error, JToken response204)> DeleteModDependenciesAsJToken(long modId, DeleteModDependenciesRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), response204: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/dependencies", ModioAPIRequestMethod.Delete, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, Response204? response204)> DeleteModDependencies(long modId, DeleteModDependenciesRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), response204: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/dependencies", ModioAPIRequestMethod.Delete, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<Response204>(request);
		}

		internal static async Task<(Error error, JToken modDependantsObjects)> GetModDependantsAsJToken(long modId, GetModDependantsFilter filter)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), modDependantsObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/dependants", ModioAPIRequestMethod.Get, ModioAPIRequestContentType.FormUrlEncoded);
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, Pagination<ModDependantsObject[]>? modDependantsObjects)> GetModDependants(long modId, GetModDependantsFilter filter)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), modDependantsObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/dependants", ModioAPIRequestMethod.Get, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddFilterParameters(filter);
			return await _apiInterface.GetJson<Pagination<ModDependantsObject[]>>(request);
		}

		public static GetModDependantsFilter FilterGetModDependants(int pageIndex = 0, int pageSize = 100)
		{
			return new GetModDependantsFilter(pageIndex, pageSize);
		}

		internal static async Task<(Error error, JToken modDependenciesObjects)> GetModDependenciesAsJToken(long modId, GetModDependenciesFilter filter)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), modDependenciesObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/dependencies", ModioAPIRequestMethod.Get, ModioAPIRequestContentType.FormUrlEncoded);
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, Pagination<ModDependenciesObject[]>? modDependenciesObjects)> GetModDependencies(long modId, GetModDependenciesFilter filter)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), modDependenciesObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/dependencies", ModioAPIRequestMethod.Get, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddFilterParameters(filter);
			return await _apiInterface.GetJson<Pagination<ModDependenciesObject[]>>(request);
		}

		public static GetModDependenciesFilter FilterGetModDependencies(int pageIndex = 0, int pageSize = 100)
		{
			return new GetModDependenciesFilter(pageIndex, pageSize);
		}
	}

	public static class Files
	{
		public class GetModfilesFilter : SearchFilter<GetModfilesFilter>
		{
			internal GetModfilesFilter(int pageIndex, int pageSize)
				: base(pageIndex, pageSize)
			{
			}

			public GetModfilesFilter Id(long id, Filtering condition = Filtering.None)
			{
				Parameters["id" + condition.ClearText()] = id;
				return this;
			}

			public GetModfilesFilter Id(ICollection<long> id, Filtering condition = Filtering.None)
			{
				Parameters["id" + condition.ClearText()] = id;
				return this;
			}

			public GetModfilesFilter ModId(long modId, Filtering condition = Filtering.None)
			{
				Parameters["mod_id" + condition.ClearText()] = modId;
				return this;
			}

			public GetModfilesFilter ModId(ICollection<long> modId, Filtering condition = Filtering.None)
			{
				Parameters["mod_id" + condition.ClearText()] = modId;
				return this;
			}

			public GetModfilesFilter DateAdded(long dateAdded, Filtering condition = Filtering.None)
			{
				Parameters["date_added" + condition.ClearText()] = dateAdded;
				return this;
			}

			public GetModfilesFilter DateAdded(ICollection<long> dateAdded, Filtering condition = Filtering.None)
			{
				Parameters["date_added" + condition.ClearText()] = dateAdded;
				return this;
			}

			public GetModfilesFilter DateScanned(long dateScanned, Filtering condition = Filtering.None)
			{
				Parameters["date_scanned" + condition.ClearText()] = dateScanned;
				return this;
			}

			public GetModfilesFilter DateScanned(ICollection<long> dateScanned, Filtering condition = Filtering.None)
			{
				Parameters["date_scanned" + condition.ClearText()] = dateScanned;
				return this;
			}

			public GetModfilesFilter VirusStatus(long virusStatus, Filtering condition = Filtering.None)
			{
				Parameters["virus_status" + condition.ClearText()] = virusStatus;
				return this;
			}

			public GetModfilesFilter VirusStatus(ICollection<long> virusStatus, Filtering condition = Filtering.None)
			{
				Parameters["virus_status" + condition.ClearText()] = virusStatus;
				return this;
			}

			public GetModfilesFilter VirusPositive(long virusPositive, Filtering condition = Filtering.None)
			{
				Parameters["virus_positive" + condition.ClearText()] = virusPositive;
				return this;
			}

			public GetModfilesFilter VirusPositive(ICollection<long> virusPositive, Filtering condition = Filtering.None)
			{
				Parameters["virus_positive" + condition.ClearText()] = virusPositive;
				return this;
			}

			public GetModfilesFilter Filesize(long filesize, Filtering condition = Filtering.None)
			{
				Parameters["filesize" + condition.ClearText()] = filesize;
				return this;
			}

			public GetModfilesFilter Filesize(ICollection<long> filesize, Filtering condition = Filtering.None)
			{
				Parameters["filesize" + condition.ClearText()] = filesize;
				return this;
			}

			public GetModfilesFilter Filehash(string filehash, Filtering condition = Filtering.None)
			{
				Parameters["filehash" + condition.ClearText()] = filehash;
				return this;
			}

			public GetModfilesFilter Filehash(ICollection<string> filehash, Filtering condition = Filtering.None)
			{
				Parameters["filehash" + condition.ClearText()] = filehash;
				return this;
			}

			public GetModfilesFilter Filename(string filename, Filtering condition = Filtering.None)
			{
				Parameters["filename" + condition.ClearText()] = filename;
				return this;
			}

			public GetModfilesFilter Filename(ICollection<string> filename, Filtering condition = Filtering.None)
			{
				Parameters["filename" + condition.ClearText()] = filename;
				return this;
			}

			public GetModfilesFilter Version(string version, Filtering condition = Filtering.None)
			{
				Parameters["version" + condition.ClearText()] = version;
				return this;
			}

			public GetModfilesFilter Version(ICollection<string> version, Filtering condition = Filtering.None)
			{
				Parameters["version" + condition.ClearText()] = version;
				return this;
			}

			public GetModfilesFilter Changelog(string changelog, Filtering condition = Filtering.None)
			{
				Parameters["changelog" + condition.ClearText()] = changelog;
				return this;
			}

			public GetModfilesFilter Changelog(ICollection<string> changelog, Filtering condition = Filtering.None)
			{
				Parameters["changelog" + condition.ClearText()] = changelog;
				return this;
			}

			public GetModfilesFilter MetadataBlob(string metadataBlob, Filtering condition = Filtering.None)
			{
				Parameters["metadata_blob" + condition.ClearText()] = metadataBlob;
				return this;
			}

			public GetModfilesFilter MetadataBlob(ICollection<string> metadataBlob, Filtering condition = Filtering.None)
			{
				Parameters["metadata_blob" + condition.ClearText()] = metadataBlob;
				return this;
			}

			public GetModfilesFilter PlatformStatus(string platformStatus, Filtering condition = Filtering.None)
			{
				Parameters["platform_status" + condition.ClearText()] = platformStatus;
				return this;
			}

			public GetModfilesFilter PlatformStatus(ICollection<string> platformStatus, Filtering condition = Filtering.None)
			{
				Parameters["platform_status" + condition.ClearText()] = platformStatus;
				return this;
			}
		}

		internal static async Task<(Error error, JToken modfileObject)> AddModfileAsJToken(long modId, AddModfileRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), modfileObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/files", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.MultipartFormData);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, ModfileObject? modfileObject)> AddModfile(long modId, AddModfileRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), modfileObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/files", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.MultipartFormData);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<ModfileObject>(request);
		}

		internal static async Task<(Error error, JToken response204)> DeleteModfileAsJToken(long modId, long fileId)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), response204: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/files/{fileId}", ModioAPIRequestMethod.Delete, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, Response204? response204)> DeleteModfile(long modId, long fileId)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), response204: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/files/{fileId}", ModioAPIRequestMethod.Delete, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<Response204>(request);
		}

		internal static async Task<(Error error, JToken modfileObject)> EditModfileAsJToken(long modId, long fileId)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), modfileObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/files/{fileId}", ModioAPIRequestMethod.Put, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, ModfileObject? modfileObject)> EditModfile(long modId, long fileId)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), modfileObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/files/{fileId}", ModioAPIRequestMethod.Put, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<ModfileObject>(request);
		}

		internal static async Task<(Error error, JToken modfileObject)> GetModfileAsJToken(long modId, long fileId)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), modfileObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/files/{fileId}", ModioAPIRequestMethod.Get, ModioAPIRequestContentType.FormUrlEncoded);
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, ModfileObject? modfileObject)> GetModfile(long modId, long fileId)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), modfileObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/files/{fileId}", ModioAPIRequestMethod.Get, ModioAPIRequestContentType.FormUrlEncoded);
			return await _apiInterface.GetJson<ModfileObject>(request);
		}

		internal static async Task<(Error error, JToken modfileObjects)> GetModfilesAsJToken(long modId, GetModfilesFilter filter)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), modfileObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/files", ModioAPIRequestMethod.Get, ModioAPIRequestContentType.FormUrlEncoded);
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, Pagination<ModfileObject[]>? modfileObjects)> GetModfiles(long modId, GetModfilesFilter filter)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), modfileObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/files", ModioAPIRequestMethod.Get, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddFilterParameters(filter);
			return await _apiInterface.GetJson<Pagination<ModfileObject[]>>(request);
		}

		public static GetModfilesFilter FilterGetModfiles(int pageIndex = 0, int pageSize = 100)
		{
			return new GetModfilesFilter(pageIndex, pageSize);
		}

		internal static async Task<(Error error, JToken modfileObject)> ManagePlatformStatusAsJToken(long modId, long fileId)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), modfileObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/files/{fileId}/platforms", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, ModfileObject? modfileObject)> ManagePlatformStatus(long modId, long fileId)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), modfileObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/files/{fileId}/platforms", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<ModfileObject>(request);
		}
	}

	public static class Metadata
	{
		public class GetModKvpMetadataFilter : SearchFilter<GetModKvpMetadataFilter>
		{
			internal GetModKvpMetadataFilter(int pageIndex, int pageSize)
				: base(pageIndex, pageSize)
			{
			}
		}

		internal static async Task<(Error error, JToken addModMetadataResponse)> AddModKvpMetadataAsJToken(long modId, AddModMetadataRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), addModMetadataResponse: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/metadatakvp", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, AddModMetadataResponse? addModMetadataResponse)> AddModKvpMetadata(long modId, AddModMetadataRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), addModMetadataResponse: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/metadatakvp", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<AddModMetadataResponse>(request);
		}

		internal static async Task<(Error error, JToken response204)> DeleteModKvpMetadataAsJToken(long modId, DeleteModMetadataRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), response204: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/metadatakvp", ModioAPIRequestMethod.Delete, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, Response204? response204)> DeleteModKvpMetadata(long modId, DeleteModMetadataRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), response204: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/metadatakvp", ModioAPIRequestMethod.Delete, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<Response204>(request);
		}

		internal static async Task<(Error error, JToken metadataKvpObjects)> GetModKvpMetadataAsJToken(long modId, GetModKvpMetadataFilter filter)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), metadataKvpObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/metadatakvp", ModioAPIRequestMethod.Get, ModioAPIRequestContentType.FormUrlEncoded);
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, Pagination<MetadataKvpObject[]>? metadataKvpObjects)> GetModKvpMetadata(long modId, GetModKvpMetadataFilter filter = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), metadataKvpObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/metadatakvp", ModioAPIRequestMethod.Get, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddFilterParameters(filter ?? FilterGetModKvpMetadata());
			return await _apiInterface.GetJson<Pagination<MetadataKvpObject[]>>(request);
		}

		public static GetModKvpMetadataFilter FilterGetModKvpMetadata(int pageIndex = 0, int pageSize = 100)
		{
			return new GetModKvpMetadataFilter(pageIndex, pageSize);
		}
	}

	public static class Ratings
	{
		internal static async Task<(Error error, JToken addRatingResponse)> AddModRatingAsJToken(long modId, AddRatingRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), addRatingResponse: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/ratings", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, AddRatingResponse? addRatingResponse)> AddModRating(long modId, AddRatingRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), addRatingResponse: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/ratings", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<AddRatingResponse>(request);
		}
	}

	public static class Tags
	{
		public class GetModTagsFilter : SearchFilter<GetModTagsFilter>
		{
			internal GetModTagsFilter(int pageIndex, int pageSize)
				: base(pageIndex, pageSize)
			{
			}

			public GetModTagsFilter DateAdded(long dateAdded, Filtering condition = Filtering.None)
			{
				Parameters["date_added" + condition.ClearText()] = dateAdded;
				return this;
			}

			public GetModTagsFilter DateAdded(ICollection<long> dateAdded, Filtering condition = Filtering.None)
			{
				Parameters["date_added" + condition.ClearText()] = dateAdded;
				return this;
			}

			public GetModTagsFilter Tag(string tag, Filtering condition = Filtering.None)
			{
				Parameters["tag" + condition.ClearText()] = tag;
				return this;
			}

			public GetModTagsFilter Tag(ICollection<string> tag, Filtering condition = Filtering.None)
			{
				Parameters["tag" + condition.ClearText()] = tag;
				return this;
			}
		}

		internal static async Task<(Error error, JToken messageObject)> AddModTagsAsJToken(long modId, AddModTagsRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), messageObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/tags", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, MessageObject? messageObject)> AddModTags(long modId, AddModTagsRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), messageObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/tags", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<MessageObject>(request);
		}

		internal static async Task<(Error error, JToken response204)> DeleteModTagsAsJToken(long modId, DeleteModTagsRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), response204: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/tags", ModioAPIRequestMethod.Delete, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, Response204? response204)> DeleteModTags(long modId, DeleteModTagsRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), response204: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/tags", ModioAPIRequestMethod.Delete, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<Response204>(request);
		}

		internal static async Task<(Error error, JToken gameTagOptionObjects)> GetGameTagOptionsAsJToken()
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), gameTagOptionObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/games/{game-id}/tags", ModioAPIRequestMethod.Get, ModioAPIRequestContentType.FormUrlEncoded);
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, Pagination<GameTagOptionObject[]>? gameTagOptionObjects)> GetGameTagOptions()
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), gameTagOptionObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/games/{game-id}/tags", ModioAPIRequestMethod.Get, ModioAPIRequestContentType.FormUrlEncoded);
			return await _apiInterface.GetJson<Pagination<GameTagOptionObject[]>>(request);
		}

		internal static async Task<(Error error, JToken modTagObjects)> GetModTagsAsJToken(long modId, GetModTagsFilter filter)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), modTagObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/tags", ModioAPIRequestMethod.Get, ModioAPIRequestContentType.FormUrlEncoded);
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, Pagination<ModTagObject[]>? modTagObjects)> GetModTags(long modId, GetModTagsFilter filter)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), modTagObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/tags", ModioAPIRequestMethod.Get, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddFilterParameters(filter);
			return await _apiInterface.GetJson<Pagination<ModTagObject[]>>(request);
		}

		public static GetModTagsFilter FilterGetModTags(int pageIndex = 0, int pageSize = 100)
		{
			return new GetModTagsFilter(pageIndex, pageSize);
		}
	}

	public static class Teams
	{
		public class GetModTeamMembersFilter : SearchFilter<GetModTeamMembersFilter>
		{
			internal GetModTeamMembersFilter(int pageIndex, int pageSize)
				: base(pageIndex, pageSize)
			{
			}

			public GetModTeamMembersFilter Id(long id, Filtering condition = Filtering.None)
			{
				Parameters["id" + condition.ClearText()] = id;
				return this;
			}

			public GetModTeamMembersFilter Id(ICollection<long> id, Filtering condition = Filtering.None)
			{
				Parameters["id" + condition.ClearText()] = id;
				return this;
			}

			public GetModTeamMembersFilter UserId(long userId, Filtering condition = Filtering.None)
			{
				Parameters["user_id" + condition.ClearText()] = userId;
				return this;
			}

			public GetModTeamMembersFilter UserId(ICollection<long> userId, Filtering condition = Filtering.None)
			{
				Parameters["user_id" + condition.ClearText()] = userId;
				return this;
			}

			public GetModTeamMembersFilter Username(string username, Filtering condition = Filtering.None)
			{
				Parameters["username" + condition.ClearText()] = username;
				return this;
			}

			public GetModTeamMembersFilter Username(ICollection<string> username, Filtering condition = Filtering.None)
			{
				Parameters["username" + condition.ClearText()] = username;
				return this;
			}

			public GetModTeamMembersFilter Level(long level, Filtering condition = Filtering.None)
			{
				Parameters["level" + condition.ClearText()] = level;
				return this;
			}

			public GetModTeamMembersFilter Level(ICollection<long> level, Filtering condition = Filtering.None)
			{
				Parameters["level" + condition.ClearText()] = level;
				return this;
			}

			public GetModTeamMembersFilter DateAdded(long dateAdded, Filtering condition = Filtering.None)
			{
				Parameters["date_added" + condition.ClearText()] = dateAdded;
				return this;
			}

			public GetModTeamMembersFilter DateAdded(ICollection<long> dateAdded, Filtering condition = Filtering.None)
			{
				Parameters["date_added" + condition.ClearText()] = dateAdded;
				return this;
			}

			public GetModTeamMembersFilter Pending(long pending, Filtering condition = Filtering.None)
			{
				Parameters["pending" + condition.ClearText()] = pending;
				return this;
			}

			public GetModTeamMembersFilter Pending(ICollection<long> pending, Filtering condition = Filtering.None)
			{
				Parameters["pending" + condition.ClearText()] = pending;
				return this;
			}
		}

		internal static async Task<(Error error, JToken teamMemberObject)> AddModTeamMemberAsJToken(long modId, AddTeamMemberRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), teamMemberObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/team", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, TeamMemberObject? teamMemberObject)> AddModTeamMember(long modId, AddTeamMemberRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), teamMemberObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/team", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<TeamMemberObject>(request);
		}

		internal static async Task<(Error error, JToken response204)> DeleteModTeamMemberAsJToken(long modId, long teamMemberId)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), response204: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/team/{teamMemberId}", ModioAPIRequestMethod.Delete, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, Response204? response204)> DeleteModTeamMember(long modId, long teamMemberId)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), response204: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/team/{teamMemberId}", ModioAPIRequestMethod.Delete, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<Response204>(request);
		}

		internal static async Task<(Error error, JToken teamMemberObjects)> GetModTeamMembersAsJToken(long modId, GetModTeamMembersFilter filter)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), teamMemberObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/team", ModioAPIRequestMethod.Get, ModioAPIRequestContentType.FormUrlEncoded);
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, Pagination<TeamMemberObject[]>? teamMemberObjects)> GetModTeamMembers(long modId, GetModTeamMembersFilter filter)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), teamMemberObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/team", ModioAPIRequestMethod.Get, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddFilterParameters(filter);
			return await _apiInterface.GetJson<Pagination<TeamMemberObject[]>>(request);
		}

		public static GetModTeamMembersFilter FilterGetModTeamMembers(int pageIndex = 0, int pageSize = 100)
		{
			return new GetModTeamMembersFilter(pageIndex, pageSize);
		}

		internal static async Task<(Error error, JToken teamMemberObject)> UpdateModTeamMemberAsJToken(long modId, long teamMemberId)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), teamMemberObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/team/{teamMemberId}", ModioAPIRequestMethod.Put, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, TeamMemberObject? teamMemberObject)> UpdateModTeamMember(long modId, long teamMemberId)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), teamMemberObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/team/{teamMemberId}", ModioAPIRequestMethod.Put, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<TeamMemberObject>(request);
		}
	}

	public static class FilesMultipartUploads
	{
		public class GetMultipartUploadPartsFilter : SearchFilter<GetMultipartUploadPartsFilter>
		{
			internal string _uploadId;

			internal GetMultipartUploadPartsFilter(int pageIndex, int pageSize, string uploadId)
				: base(pageIndex, pageSize)
			{
				_uploadId = uploadId;
			}
		}

		public class GetMultipartUploadSessionsFilter : SearchFilter<GetMultipartUploadSessionsFilter>
		{
			internal GetMultipartUploadSessionsFilter(int pageIndex, int pageSize)
				: base(pageIndex, pageSize)
			{
			}

			public GetMultipartUploadSessionsFilter Status(long status, Filtering condition = Filtering.None)
			{
				Parameters["status" + condition.ClearText()] = status;
				return this;
			}

			public GetMultipartUploadSessionsFilter Status(ICollection<long> status, Filtering condition = Filtering.None)
			{
				Parameters["status" + condition.ClearText()] = status;
				return this;
			}
		}

		internal static async Task<(Error error, JToken multipartUploadPartObject)> AddMultipartUploadPartAsJToken(string uploadId, long modId, string contentRange, byte[] bytes, string digest = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), multipartUploadPartObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/files/multipart", ModioAPIRequestMethod.Put, ModioAPIRequestContentType.ByteArray);
			request.Options.AddQueryParameter("upload_id", uploadId);
			request.Options.AddHeaderParameter("Content-Range", contentRange);
			request.Options.AddHeaderParameter("Digest", digest);
			request.Options.AddBody(bytes);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, MultipartUploadPartObject? multipartUploadPartObject)> AddMultipartUploadPart(string uploadId, long modId, string contentRange, byte[] bytes, string digest = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), multipartUploadPartObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/files/multipart", ModioAPIRequestMethod.Put, ModioAPIRequestContentType.ByteArray);
			request.Options.AddQueryParameter("upload_id", uploadId);
			request.Options.AddHeaderParameter("Content-Range", contentRange);
			request.Options.AddHeaderParameter("Digest", digest);
			request.Options.AddBody(bytes);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<MultipartUploadPartObject>(request);
		}

		internal static async Task<(Error error, JToken multipartUploadObject)> CompleteMultipartUploadSessionAsJToken(string uploadId, long modId)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), multipartUploadObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/files/multipart/complete", ModioAPIRequestMethod.Post);
			request.Options.AddQueryParameter("upload_id", uploadId);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, MultipartUploadObject? multipartUploadObject)> CompleteMultipartUploadSession(string uploadId, long modId)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), multipartUploadObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/files/multipart/complete", ModioAPIRequestMethod.Post);
			request.Options.AddQueryParameter("upload_id", uploadId);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<MultipartUploadObject>(request);
		}

		internal static async Task<(Error error, JToken multipartUploadObject)> CreateMultipartUploadSessionAsJToken(long modId, CreateMultipartUploadSessionRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), multipartUploadObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/files/multipart", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, MultipartUploadObject? multipartUploadObject)> CreateMultipartUploadSession(long modId, CreateMultipartUploadSessionRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), multipartUploadObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/files/multipart", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<MultipartUploadObject>(request);
		}

		internal static async Task<(Error error, JToken response204)> DeleteMultipartUploadSessionAsJToken(string uploadId, long modId)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), response204: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/files/multipart", ModioAPIRequestMethod.Delete);
			request.Options.AddQueryParameter("upload_id", uploadId);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, Response204? response204)> DeleteMultipartUploadSession(string uploadId, long modId)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), response204: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/files/multipart", ModioAPIRequestMethod.Delete);
			request.Options.AddQueryParameter("upload_id", uploadId);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<Response204>(request);
		}

		internal static async Task<(Error error, JToken multipartUploadPartObjects)> GetMultipartUploadPartsAsJToken(long modId, GetMultipartUploadPartsFilter filter)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), multipartUploadPartObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/files/multipart");
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, Pagination<MultipartUploadPartObject[]>? multipartUploadPartObjects)> GetMultipartUploadParts(long modId, GetMultipartUploadPartsFilter filter)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), multipartUploadPartObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/files/multipart");
			request.Options.AddFilterParameters(filter);
			return await _apiInterface.GetJson<Pagination<MultipartUploadPartObject[]>>(request);
		}

		public static GetMultipartUploadPartsFilter FilterGetMultipartUploadParts(string uploadId, int pageIndex = 0, int pageSize = 100)
		{
			return new GetMultipartUploadPartsFilter(pageIndex, pageSize, uploadId);
		}

		internal static async Task<(Error error, JToken multipartUploadObjects)> GetMultipartUploadSessionsAsJToken(long modId, GetMultipartUploadSessionsFilter filter)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), multipartUploadObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/files/multipart/sessions");
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, Pagination<MultipartUploadObject[]>? multipartUploadObjects)> GetMultipartUploadSessions(long modId, GetMultipartUploadSessionsFilter filter)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), multipartUploadObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/files/multipart/sessions");
			request.Options.AddFilterParameters(filter);
			return await _apiInterface.GetJson<Pagination<MultipartUploadObject[]>>(request);
		}

		public static GetMultipartUploadSessionsFilter FilterGetMultipartUploadSessions(int pageIndex = 0, int pageSize = 100)
		{
			return new GetMultipartUploadSessionsFilter(pageIndex, pageSize);
		}
	}

	public static class Authentication
	{
		internal static async Task<(Error error, JToken accessTokenObject)> AuthenticateViaAppleAsJToken(AppleAuthenticationRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), accessTokenObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/external/appleauth", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, AccessTokenObject? accessTokenObject)> AuthenticateViaApple(AppleAuthenticationRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), accessTokenObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/external/appleauth", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			return await _apiInterface.GetJson<AccessTokenObject>(request);
		}

		internal static async Task<(Error error, JToken accessTokenObject)> AuthenticateViaDiscordAsJToken(DiscordAuthenticationRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), accessTokenObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/external/discordauth", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, AccessTokenObject? accessTokenObject)> AuthenticateViaDiscord(DiscordAuthenticationRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), accessTokenObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/external/discordauth", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			return await _apiInterface.GetJson<AccessTokenObject>(request);
		}

		internal static async Task<(Error error, JToken accessTokenObject)> AuthenticateViaEpicgamesAsJToken(EpicGamesAuthenticationRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), accessTokenObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/external/epicgamesauth", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, AccessTokenObject? accessTokenObject)> AuthenticateViaEpicgames(EpicGamesAuthenticationRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), accessTokenObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/external/epicgamesauth", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			return await _apiInterface.GetJson<AccessTokenObject>(request);
		}

		internal static async Task<(Error error, JToken accessTokenObject)> AuthenticateViaFacebookAsJToken(FacebookAuthenticationRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), accessTokenObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/external/facebookauth", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, AccessTokenObject? accessTokenObject)> AuthenticateViaFacebook(FacebookAuthenticationRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), accessTokenObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/external/facebookauth", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			return await _apiInterface.GetJson<AccessTokenObject>(request);
		}

		public static async Task<(Error error, JToken accessTokenObject)> AuthenticateViaGogGalaxyAsJToken(GogAuthenticationRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), accessTokenObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/external/galaxyauth", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			return await _apiInterface.GetJson(request);
		}

		public static async Task<(Error error, AccessTokenObject? accessTokenObject)> AuthenticateViaGogGalaxy(GogAuthenticationRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), accessTokenObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/external/galaxyauth", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			return await _apiInterface.GetJson<AccessTokenObject>(request);
		}

		internal static async Task<(Error error, JToken accessTokenObject)> AuthenticateViaGoogleAsJToken(GoogleAuthenticationRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), accessTokenObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/external/googleauth", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, AccessTokenObject? accessTokenObject)> AuthenticateViaGoogle(GoogleAuthenticationRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), accessTokenObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/external/googleauth", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			return await _apiInterface.GetJson<AccessTokenObject>(request);
		}

		internal static async Task<(Error error, JToken accessTokenObject)> AuthenticateViaItchioAsJToken(ItchioAuthenticationRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), accessTokenObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/external/itchioauth", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, AccessTokenObject? accessTokenObject)> AuthenticateViaItchio(ItchioAuthenticationRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), accessTokenObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/external/itchioauth", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			return await _apiInterface.GetJson<AccessTokenObject>(request);
		}

		public static async Task<(Error error, JToken accessTokenObject)> AuthenticateViaOculusAsJToken(MetaQuestAuthenticationRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), accessTokenObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/external/oculusauth", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			return await _apiInterface.GetJson(request);
		}

		public static async Task<(Error error, AccessTokenObject? accessTokenObject)> AuthenticateViaOculus(MetaQuestAuthenticationRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), accessTokenObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/external/oculusauth", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			return await _apiInterface.GetJson<AccessTokenObject>(request);
		}

		internal static async Task<(Error error, JToken accessTokenObject)> AuthenticateViaOpenidAsJToken(OpenIdAuthenticationRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), accessTokenObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/external/openidauth", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, AccessTokenObject? accessTokenObject)> AuthenticateViaOpenid(OpenIdAuthenticationRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), accessTokenObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/external/openidauth", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			return await _apiInterface.GetJson<AccessTokenObject>(request);
		}

		internal static async Task<(Error error, JToken accessTokenObject)> AuthenticateViaPsnAsJToken(PsnAuthenticationRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), accessTokenObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/external/psnauth", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			return await _apiInterface.GetJson(request);
		}

		public static async Task<(Error error, AccessTokenObject? accessTokenObject)> AuthenticateViaPsn(PsnAuthenticationRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), accessTokenObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/external/psnauth", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			return await _apiInterface.GetJson<AccessTokenObject>(request);
		}

		public static async Task<(Error error, JToken accessTokenObject)> AuthenticateViaSteamAsJToken(SteamAuthenticationRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), accessTokenObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/external/steamauth", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			return await _apiInterface.GetJson(request);
		}

		public static async Task<(Error error, AccessTokenObject? accessTokenObject)> AuthenticateViaSteam(SteamAuthenticationRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), accessTokenObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/external/steamauth", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			return await _apiInterface.GetJson<AccessTokenObject>(request);
		}

		public static async Task<(Error error, JToken accessTokenObject)> AuthenticateViaSwitchAsJToken(SwitchAuthenticationRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), accessTokenObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/external/switchauth", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			return await _apiInterface.GetJson(request);
		}

		public static async Task<(Error error, AccessTokenObject? accessTokenObject)> AuthenticateViaSwitch(SwitchAuthenticationRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), accessTokenObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/external/switchauth", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			return await _apiInterface.GetJson<AccessTokenObject>(request);
		}

		public static async Task<(Error error, JToken accessTokenObject)> AuthenticateViaXboxLiveAsJToken(XboxLiveAuthenticationRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), accessTokenObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/external/xboxauth", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			return await _apiInterface.GetJson(request);
		}

		public static async Task<(Error error, AccessTokenObject? accessTokenObject)> AuthenticateViaXboxLive(XboxLiveAuthenticationRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), accessTokenObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/external/xboxauth", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			return await _apiInterface.GetJson<AccessTokenObject>(request);
		}

		internal static async Task<(Error error, JToken accessTokenObject)> ExchangeEmailSecurityCodeAsJToken(EmailAuthenticationSecurityCodeRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), accessTokenObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/oauth/emailexchange", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, AccessTokenObject? accessTokenObject)> ExchangeEmailSecurityCode(EmailAuthenticationSecurityCodeRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), accessTokenObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/oauth/emailexchange", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			return await _apiInterface.GetJson<AccessTokenObject>(request);
		}

		internal static async Task<(Error error, JToken webMessageObject)> LogoutAsJToken()
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), webMessageObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/oauth/logout", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, WebMessageObject? webMessageObject)> Logout()
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), webMessageObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/oauth/logout", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<WebMessageObject>(request);
		}

		internal static async Task<(Error error, JToken emailRequestResponse)> RequestEmailSecurityCodeAsJToken(EmailAuthenticationRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), emailRequestResponse: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/oauth/emailrequest", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, EmailRequestResponse? emailRequestResponse)> RequestEmailSecurityCode(EmailAuthenticationRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), emailRequestResponse: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/oauth/emailrequest", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			return await _apiInterface.GetJson<EmailRequestResponse>(request);
		}

		internal static async Task<(Error error, JToken termsObject)> TermsAsJToken()
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), termsObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/authenticate/terms", ModioAPIRequestMethod.Get, ModioAPIRequestContentType.FormUrlEncoded);
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, TermsObject? termsObject)> Terms()
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), termsObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/authenticate/terms", ModioAPIRequestMethod.Get, ModioAPIRequestContentType.FormUrlEncoded);
			return await _apiInterface.GetJson<TermsObject>(request);
		}
	}

	public static class Monetization
	{
		internal static async Task<(Error error, JToken monetizationTeamAccountsObject)> CreateModMonetizationTeamAsJToken(long modId)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), monetizationTeamAccountsObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/monetization/team", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.MultipartFormData);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, MonetizationTeamAccountsObject? monetizationTeamAccountsObject)> CreateModMonetizationTeam(long modId)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), monetizationTeamAccountsObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/monetization/team", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.MultipartFormData);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<MonetizationTeamAccountsObject>(request);
		}

		public static async Task<(Error error, JToken gameTokenPackObject)> GetGameTokenPacksAsJToken()
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), gameTokenPackObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/games/{game-id}/monetization/token-packs");
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		public static async Task<(Error error, Pagination<GameTokenPackObject[]>? gameTokenPackObject)> GetGameTokenPacks()
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), gameTokenPackObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/games/{game-id}/monetization/token-packs");
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<Pagination<GameTokenPackObject[]>>(request);
		}

		internal static async Task<(Error error, JToken monetizationTeamAccountsObject)> GetUsersInModMonetizationTeamAsJToken(long modId)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), monetizationTeamAccountsObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/monetization/team", ModioAPIRequestMethod.Get, ModioAPIRequestContentType.MultipartFormData);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, MonetizationTeamAccountsObject? monetizationTeamAccountsObject)> GetUsersInModMonetizationTeam(long modId)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), monetizationTeamAccountsObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/monetization/team", ModioAPIRequestMethod.Get, ModioAPIRequestContentType.MultipartFormData);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<MonetizationTeamAccountsObject>(request);
		}

		internal static async Task<(Error error, JToken payObject)> PurchaseAsJToken(long modId, PayRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), payObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/checkout", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, PayObject? payObject)> Purchase(long modId, PayRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), payObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/checkout", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<PayObject>(request);
		}
	}

	public static class Agreements
	{
		internal static async Task<(Error error, JToken agreementVersionObject)> GetAgreementVersionAsJToken(long agreementVersionId)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), agreementVersionObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/agreements/versions/{agreementVersionId}", ModioAPIRequestMethod.Get, ModioAPIRequestContentType.FormUrlEncoded);
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, AgreementVersionObject? agreementVersionObject)> GetAgreementVersion(long agreementVersionId)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), agreementVersionObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/agreements/versions/{agreementVersionId}", ModioAPIRequestMethod.Get, ModioAPIRequestContentType.FormUrlEncoded);
			return await _apiInterface.GetJson<AgreementVersionObject>(request);
		}

		internal static async Task<(Error error, JToken agreementVersionObject)> GetCurrentAgreementAsJToken(long agreementTypeId)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), agreementVersionObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/agreements/types/{agreementTypeId}/current", ModioAPIRequestMethod.Get, ModioAPIRequestContentType.FormUrlEncoded);
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, AgreementVersionObject? agreementVersionObject)> GetCurrentAgreement(long agreementTypeId)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), agreementVersionObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/agreements/types/{agreementTypeId}/current", ModioAPIRequestMethod.Get, ModioAPIRequestContentType.FormUrlEncoded);
			return await _apiInterface.GetJson<AgreementVersionObject>(request);
		}
	}

	public static class Me
	{
		public class GetUserEventsFilter : SearchFilter<GetUserEventsFilter>
		{
			internal GetUserEventsFilter(int pageIndex, int pageSize)
				: base(pageIndex, pageSize)
			{
			}

			public GetUserEventsFilter Id(long id, Filtering condition = Filtering.None)
			{
				Parameters["id" + condition.ClearText()] = id;
				return this;
			}

			public GetUserEventsFilter Id(ICollection<long> id, Filtering condition = Filtering.None)
			{
				Parameters["id" + condition.ClearText()] = id;
				return this;
			}

			public GetUserEventsFilter GameId(long gameId, Filtering condition = Filtering.None)
			{
				Parameters["game_id" + condition.ClearText()] = gameId;
				return this;
			}

			public GetUserEventsFilter GameId(ICollection<long> gameId, Filtering condition = Filtering.None)
			{
				Parameters["game_id" + condition.ClearText()] = gameId;
				return this;
			}

			public GetUserEventsFilter ModId(long modId, Filtering condition = Filtering.None)
			{
				Parameters["mod_id" + condition.ClearText()] = modId;
				return this;
			}

			public GetUserEventsFilter ModId(ICollection<long> modId, Filtering condition = Filtering.None)
			{
				Parameters["mod_id" + condition.ClearText()] = modId;
				return this;
			}

			public GetUserEventsFilter UserId(long userId, Filtering condition = Filtering.None)
			{
				Parameters["user_id" + condition.ClearText()] = userId;
				return this;
			}

			public GetUserEventsFilter UserId(ICollection<long> userId, Filtering condition = Filtering.None)
			{
				Parameters["user_id" + condition.ClearText()] = userId;
				return this;
			}

			public GetUserEventsFilter DateAdded(long dateAdded, Filtering condition = Filtering.None)
			{
				Parameters["date_added" + condition.ClearText()] = dateAdded;
				return this;
			}

			public GetUserEventsFilter DateAdded(ICollection<long> dateAdded, Filtering condition = Filtering.None)
			{
				Parameters["date_added" + condition.ClearText()] = dateAdded;
				return this;
			}

			public GetUserEventsFilter EventType(string eventType, Filtering condition = Filtering.None)
			{
				Parameters["event_type" + condition.ClearText()] = eventType;
				return this;
			}

			public GetUserEventsFilter EventType(ICollection<string> eventType, Filtering condition = Filtering.None)
			{
				Parameters["event_type" + condition.ClearText()] = eventType;
				return this;
			}
		}

		public class GetUserGamesFilter : SearchFilter<GetUserGamesFilter>
		{
			internal GetUserGamesFilter(int pageIndex, int pageSize)
				: base(pageIndex, pageSize)
			{
			}

			public GetUserGamesFilter Id(long id, Filtering condition = Filtering.None)
			{
				Parameters["id" + condition.ClearText()] = id;
				return this;
			}

			public GetUserGamesFilter Id(ICollection<long> id, Filtering condition = Filtering.None)
			{
				Parameters["id" + condition.ClearText()] = id;
				return this;
			}

			public GetUserGamesFilter Status(long status, Filtering condition = Filtering.None)
			{
				Parameters["status" + condition.ClearText()] = status;
				return this;
			}

			public GetUserGamesFilter Status(ICollection<long> status, Filtering condition = Filtering.None)
			{
				Parameters["status" + condition.ClearText()] = status;
				return this;
			}

			public GetUserGamesFilter SubmittedBy(long submittedBy, Filtering condition = Filtering.None)
			{
				Parameters["submitted_by" + condition.ClearText()] = submittedBy;
				return this;
			}

			public GetUserGamesFilter SubmittedBy(ICollection<long> submittedBy, Filtering condition = Filtering.None)
			{
				Parameters["submitted_by" + condition.ClearText()] = submittedBy;
				return this;
			}

			public GetUserGamesFilter DateAdded(long dateAdded, Filtering condition = Filtering.None)
			{
				Parameters["date_added" + condition.ClearText()] = dateAdded;
				return this;
			}

			public GetUserGamesFilter DateAdded(ICollection<long> dateAdded, Filtering condition = Filtering.None)
			{
				Parameters["date_added" + condition.ClearText()] = dateAdded;
				return this;
			}

			public GetUserGamesFilter DateUpdated(long dateUpdated, Filtering condition = Filtering.None)
			{
				Parameters["date_updated" + condition.ClearText()] = dateUpdated;
				return this;
			}

			public GetUserGamesFilter DateUpdated(ICollection<long> dateUpdated, Filtering condition = Filtering.None)
			{
				Parameters["date_updated" + condition.ClearText()] = dateUpdated;
				return this;
			}

			public GetUserGamesFilter DateLive(long dateLive, Filtering condition = Filtering.None)
			{
				Parameters["date_live" + condition.ClearText()] = dateLive;
				return this;
			}

			public GetUserGamesFilter DateLive(ICollection<long> dateLive, Filtering condition = Filtering.None)
			{
				Parameters["date_live" + condition.ClearText()] = dateLive;
				return this;
			}

			public GetUserGamesFilter Name(string name, Filtering condition = Filtering.None)
			{
				Parameters["name" + condition.ClearText()] = name;
				return this;
			}

			public GetUserGamesFilter Name(ICollection<string> name, Filtering condition = Filtering.None)
			{
				Parameters["name" + condition.ClearText()] = name;
				return this;
			}

			public GetUserGamesFilter NameId(string nameId, Filtering condition = Filtering.None)
			{
				Parameters["name_id" + condition.ClearText()] = nameId;
				return this;
			}

			public GetUserGamesFilter NameId(ICollection<string> nameId, Filtering condition = Filtering.None)
			{
				Parameters["name_id" + condition.ClearText()] = nameId;
				return this;
			}

			public GetUserGamesFilter Summary(string summary, Filtering condition = Filtering.None)
			{
				Parameters["summary" + condition.ClearText()] = summary;
				return this;
			}

			public GetUserGamesFilter Summary(ICollection<string> summary, Filtering condition = Filtering.None)
			{
				Parameters["summary" + condition.ClearText()] = summary;
				return this;
			}

			public GetUserGamesFilter InstructionsUrl(string instructionsUrl, Filtering condition = Filtering.None)
			{
				Parameters["instructions_url" + condition.ClearText()] = instructionsUrl;
				return this;
			}

			public GetUserGamesFilter InstructionsUrl(ICollection<string> instructionsUrl, Filtering condition = Filtering.None)
			{
				Parameters["instructions_url" + condition.ClearText()] = instructionsUrl;
				return this;
			}

			public GetUserGamesFilter UgcName(string ugcName, Filtering condition = Filtering.None)
			{
				Parameters["ugc_name" + condition.ClearText()] = ugcName;
				return this;
			}

			public GetUserGamesFilter UgcName(ICollection<string> ugcName, Filtering condition = Filtering.None)
			{
				Parameters["ugc_name" + condition.ClearText()] = ugcName;
				return this;
			}

			public GetUserGamesFilter PresentationOption(long presentationOption, Filtering condition = Filtering.None)
			{
				Parameters["presentation_option" + condition.ClearText()] = presentationOption;
				return this;
			}

			public GetUserGamesFilter PresentationOption(ICollection<long> presentationOption, Filtering condition = Filtering.None)
			{
				Parameters["presentation_option" + condition.ClearText()] = presentationOption;
				return this;
			}

			public GetUserGamesFilter SubmissionOption(long submissionOption, Filtering condition = Filtering.None)
			{
				Parameters["submission_option" + condition.ClearText()] = submissionOption;
				return this;
			}

			public GetUserGamesFilter SubmissionOption(ICollection<long> submissionOption, Filtering condition = Filtering.None)
			{
				Parameters["submission_option" + condition.ClearText()] = submissionOption;
				return this;
			}

			public GetUserGamesFilter CurationOption(long curationOption, Filtering condition = Filtering.None)
			{
				Parameters["curation_option" + condition.ClearText()] = curationOption;
				return this;
			}

			public GetUserGamesFilter CurationOption(ICollection<long> curationOption, Filtering condition = Filtering.None)
			{
				Parameters["curation_option" + condition.ClearText()] = curationOption;
				return this;
			}

			public GetUserGamesFilter DependencyOption(long dependencyOption, Filtering condition = Filtering.None)
			{
				Parameters["dependency_option" + condition.ClearText()] = dependencyOption;
				return this;
			}

			public GetUserGamesFilter DependencyOption(ICollection<long> dependencyOption, Filtering condition = Filtering.None)
			{
				Parameters["dependency_option" + condition.ClearText()] = dependencyOption;
				return this;
			}

			public GetUserGamesFilter CommunityOptions(long communityOptions, Filtering condition = Filtering.None)
			{
				Parameters["community_options" + condition.ClearText()] = communityOptions;
				return this;
			}

			public GetUserGamesFilter CommunityOptions(ICollection<long> communityOptions, Filtering condition = Filtering.None)
			{
				Parameters["community_options" + condition.ClearText()] = communityOptions;
				return this;
			}

			public GetUserGamesFilter MonetizationOptions(long monetizationOptions, Filtering condition = Filtering.None)
			{
				Parameters["monetization_options" + condition.ClearText()] = monetizationOptions;
				return this;
			}

			public GetUserGamesFilter MonetizationOptions(ICollection<long> monetizationOptions, Filtering condition = Filtering.None)
			{
				Parameters["monetization_options" + condition.ClearText()] = monetizationOptions;
				return this;
			}

			public GetUserGamesFilter ApiAccessOptions(long apiAccessOptions, Filtering condition = Filtering.None)
			{
				Parameters["api_access_options" + condition.ClearText()] = apiAccessOptions;
				return this;
			}

			public GetUserGamesFilter ApiAccessOptions(ICollection<long> apiAccessOptions, Filtering condition = Filtering.None)
			{
				Parameters["api_access_options" + condition.ClearText()] = apiAccessOptions;
				return this;
			}

			public GetUserGamesFilter MaturityOptions(long maturityOptions, Filtering condition = Filtering.None)
			{
				Parameters["maturity_options" + condition.ClearText()] = maturityOptions;
				return this;
			}

			public GetUserGamesFilter MaturityOptions(ICollection<long> maturityOptions, Filtering condition = Filtering.None)
			{
				Parameters["maturity_options" + condition.ClearText()] = maturityOptions;
				return this;
			}

			public GetUserGamesFilter ShowHiddenTags(bool showHiddenTags, Filtering condition = Filtering.None)
			{
				Parameters["show_hidden_tags" + condition.ClearText()] = showHiddenTags;
				return this;
			}

			public GetUserGamesFilter ShowHiddenTags(ICollection<bool> showHiddenTags, Filtering condition = Filtering.None)
			{
				Parameters["show_hidden_tags" + condition.ClearText()] = showHiddenTags;
				return this;
			}
		}

		public class GetUserModfilesFilter : SearchFilter<GetUserModfilesFilter>
		{
			internal GetUserModfilesFilter(int pageIndex, int pageSize)
				: base(pageIndex, pageSize)
			{
			}

			public GetUserModfilesFilter Id(long id, Filtering condition = Filtering.None)
			{
				Parameters["id" + condition.ClearText()] = id;
				return this;
			}

			public GetUserModfilesFilter Id(ICollection<long> id, Filtering condition = Filtering.None)
			{
				Parameters["id" + condition.ClearText()] = id;
				return this;
			}

			public GetUserModfilesFilter ModId(long modId, Filtering condition = Filtering.None)
			{
				Parameters["mod_id" + condition.ClearText()] = modId;
				return this;
			}

			public GetUserModfilesFilter ModId(ICollection<long> modId, Filtering condition = Filtering.None)
			{
				Parameters["mod_id" + condition.ClearText()] = modId;
				return this;
			}

			public GetUserModfilesFilter DateAdded(long dateAdded, Filtering condition = Filtering.None)
			{
				Parameters["date_added" + condition.ClearText()] = dateAdded;
				return this;
			}

			public GetUserModfilesFilter DateAdded(ICollection<long> dateAdded, Filtering condition = Filtering.None)
			{
				Parameters["date_added" + condition.ClearText()] = dateAdded;
				return this;
			}

			public GetUserModfilesFilter DateScanned(long dateScanned, Filtering condition = Filtering.None)
			{
				Parameters["date_scanned" + condition.ClearText()] = dateScanned;
				return this;
			}

			public GetUserModfilesFilter DateScanned(ICollection<long> dateScanned, Filtering condition = Filtering.None)
			{
				Parameters["date_scanned" + condition.ClearText()] = dateScanned;
				return this;
			}

			public GetUserModfilesFilter VirusStatus(long virusStatus, Filtering condition = Filtering.None)
			{
				Parameters["virus_status" + condition.ClearText()] = virusStatus;
				return this;
			}

			public GetUserModfilesFilter VirusStatus(ICollection<long> virusStatus, Filtering condition = Filtering.None)
			{
				Parameters["virus_status" + condition.ClearText()] = virusStatus;
				return this;
			}

			public GetUserModfilesFilter VirusPositive(long virusPositive, Filtering condition = Filtering.None)
			{
				Parameters["virus_positive" + condition.ClearText()] = virusPositive;
				return this;
			}

			public GetUserModfilesFilter VirusPositive(ICollection<long> virusPositive, Filtering condition = Filtering.None)
			{
				Parameters["virus_positive" + condition.ClearText()] = virusPositive;
				return this;
			}

			public GetUserModfilesFilter Filesize(long filesize, Filtering condition = Filtering.None)
			{
				Parameters["filesize" + condition.ClearText()] = filesize;
				return this;
			}

			public GetUserModfilesFilter Filesize(ICollection<long> filesize, Filtering condition = Filtering.None)
			{
				Parameters["filesize" + condition.ClearText()] = filesize;
				return this;
			}

			public GetUserModfilesFilter Filehash(string filehash, Filtering condition = Filtering.None)
			{
				Parameters["filehash" + condition.ClearText()] = filehash;
				return this;
			}

			public GetUserModfilesFilter Filehash(ICollection<string> filehash, Filtering condition = Filtering.None)
			{
				Parameters["filehash" + condition.ClearText()] = filehash;
				return this;
			}

			public GetUserModfilesFilter Filename(string filename, Filtering condition = Filtering.None)
			{
				Parameters["filename" + condition.ClearText()] = filename;
				return this;
			}

			public GetUserModfilesFilter Filename(ICollection<string> filename, Filtering condition = Filtering.None)
			{
				Parameters["filename" + condition.ClearText()] = filename;
				return this;
			}

			public GetUserModfilesFilter Version(string version, Filtering condition = Filtering.None)
			{
				Parameters["version" + condition.ClearText()] = version;
				return this;
			}

			public GetUserModfilesFilter Version(ICollection<string> version, Filtering condition = Filtering.None)
			{
				Parameters["version" + condition.ClearText()] = version;
				return this;
			}

			public GetUserModfilesFilter Changelog(string changelog, Filtering condition = Filtering.None)
			{
				Parameters["changelog" + condition.ClearText()] = changelog;
				return this;
			}

			public GetUserModfilesFilter Changelog(ICollection<string> changelog, Filtering condition = Filtering.None)
			{
				Parameters["changelog" + condition.ClearText()] = changelog;
				return this;
			}

			public GetUserModfilesFilter MetadataBlob(string metadataBlob, Filtering condition = Filtering.None)
			{
				Parameters["metadata_blob" + condition.ClearText()] = metadataBlob;
				return this;
			}

			public GetUserModfilesFilter MetadataBlob(ICollection<string> metadataBlob, Filtering condition = Filtering.None)
			{
				Parameters["metadata_blob" + condition.ClearText()] = metadataBlob;
				return this;
			}

			public GetUserModfilesFilter PlatformStatus(string platformStatus, Filtering condition = Filtering.None)
			{
				Parameters["platform_status" + condition.ClearText()] = platformStatus;
				return this;
			}

			public GetUserModfilesFilter PlatformStatus(ICollection<string> platformStatus, Filtering condition = Filtering.None)
			{
				Parameters["platform_status" + condition.ClearText()] = platformStatus;
				return this;
			}
		}

		public class GetUserModsFilter : SearchFilter<GetUserModsFilter>
		{
			internal GetUserModsFilter(int pageIndex, int pageSize)
				: base(pageIndex, pageSize)
			{
			}

			public GetUserModsFilter Id(long id, Filtering condition = Filtering.None)
			{
				Parameters["id" + condition.ClearText()] = id;
				return this;
			}

			public GetUserModsFilter Id(ICollection<long> id, Filtering condition = Filtering.None)
			{
				Parameters["id" + condition.ClearText()] = id;
				return this;
			}

			public GetUserModsFilter GameId(long gameId, Filtering condition = Filtering.None)
			{
				Parameters["game_id" + condition.ClearText()] = gameId;
				return this;
			}

			public GetUserModsFilter GameId(ICollection<long> gameId, Filtering condition = Filtering.None)
			{
				Parameters["game_id" + condition.ClearText()] = gameId;
				return this;
			}

			public GetUserModsFilter Status(long status, Filtering condition = Filtering.None)
			{
				Parameters["status" + condition.ClearText()] = status;
				return this;
			}

			public GetUserModsFilter Status(ICollection<long> status, Filtering condition = Filtering.None)
			{
				Parameters["status" + condition.ClearText()] = status;
				return this;
			}

			public GetUserModsFilter Visible(long visible, Filtering condition = Filtering.None)
			{
				Parameters["visible" + condition.ClearText()] = visible;
				return this;
			}

			public GetUserModsFilter Visible(ICollection<long> visible, Filtering condition = Filtering.None)
			{
				Parameters["visible" + condition.ClearText()] = visible;
				return this;
			}

			public GetUserModsFilter SubmittedBy(long submittedBy, Filtering condition = Filtering.None)
			{
				Parameters["submitted_by" + condition.ClearText()] = submittedBy;
				return this;
			}

			public GetUserModsFilter SubmittedBy(ICollection<long> submittedBy, Filtering condition = Filtering.None)
			{
				Parameters["submitted_by" + condition.ClearText()] = submittedBy;
				return this;
			}

			public GetUserModsFilter DateAdded(long dateAdded, Filtering condition = Filtering.None)
			{
				Parameters["date_added" + condition.ClearText()] = dateAdded;
				return this;
			}

			public GetUserModsFilter DateAdded(ICollection<long> dateAdded, Filtering condition = Filtering.None)
			{
				Parameters["date_added" + condition.ClearText()] = dateAdded;
				return this;
			}

			public GetUserModsFilter DateUpdated(long dateUpdated, Filtering condition = Filtering.None)
			{
				Parameters["date_updated" + condition.ClearText()] = dateUpdated;
				return this;
			}

			public GetUserModsFilter DateUpdated(ICollection<long> dateUpdated, Filtering condition = Filtering.None)
			{
				Parameters["date_updated" + condition.ClearText()] = dateUpdated;
				return this;
			}

			public GetUserModsFilter DateLive(long dateLive, Filtering condition = Filtering.None)
			{
				Parameters["date_live" + condition.ClearText()] = dateLive;
				return this;
			}

			public GetUserModsFilter DateLive(ICollection<long> dateLive, Filtering condition = Filtering.None)
			{
				Parameters["date_live" + condition.ClearText()] = dateLive;
				return this;
			}

			public GetUserModsFilter Name(string name, Filtering condition = Filtering.None)
			{
				Parameters["name" + condition.ClearText()] = name;
				return this;
			}

			public GetUserModsFilter Name(ICollection<string> name, Filtering condition = Filtering.None)
			{
				Parameters["name" + condition.ClearText()] = name;
				return this;
			}

			public GetUserModsFilter NameId(string nameId, Filtering condition = Filtering.None)
			{
				Parameters["name_id" + condition.ClearText()] = nameId;
				return this;
			}

			public GetUserModsFilter NameId(ICollection<string> nameId, Filtering condition = Filtering.None)
			{
				Parameters["name_id" + condition.ClearText()] = nameId;
				return this;
			}

			public GetUserModsFilter Modfile(long modfile, Filtering condition = Filtering.None)
			{
				Parameters["modfile" + condition.ClearText()] = modfile;
				return this;
			}

			public GetUserModsFilter Modfile(ICollection<long> modfile, Filtering condition = Filtering.None)
			{
				Parameters["modfile" + condition.ClearText()] = modfile;
				return this;
			}

			public GetUserModsFilter MetadataKvp(string metadataKvp, Filtering condition = Filtering.None)
			{
				Parameters["metadata_kvp" + condition.ClearText()] = metadataKvp;
				return this;
			}

			public GetUserModsFilter MetadataKvp(ICollection<string> metadataKvp, Filtering condition = Filtering.None)
			{
				Parameters["metadata_kvp" + condition.ClearText()] = metadataKvp;
				return this;
			}

			public GetUserModsFilter MetadataBlob(string metadataBlob, Filtering condition = Filtering.None)
			{
				Parameters["metadata_blob" + condition.ClearText()] = metadataBlob;
				return this;
			}

			public GetUserModsFilter MetadataBlob(ICollection<string> metadataBlob, Filtering condition = Filtering.None)
			{
				Parameters["metadata_blob" + condition.ClearText()] = metadataBlob;
				return this;
			}

			public GetUserModsFilter Tags(string tags, Filtering condition = Filtering.None)
			{
				Parameters["tags" + condition.ClearText()] = tags;
				return this;
			}

			public GetUserModsFilter Tags(ICollection<string> tags, Filtering condition = Filtering.None)
			{
				Parameters["tags" + condition.ClearText()] = tags;
				return this;
			}

			public GetUserModsFilter MaturityOption(long maturityOption, Filtering condition = Filtering.None)
			{
				Parameters["maturity_option" + condition.ClearText()] = maturityOption;
				return this;
			}

			public GetUserModsFilter MaturityOption(ICollection<long> maturityOption, Filtering condition = Filtering.None)
			{
				Parameters["maturity_option" + condition.ClearText()] = maturityOption;
				return this;
			}

			public GetUserModsFilter MonetizationOptions(long monetizationOptions, Filtering condition = Filtering.None)
			{
				Parameters["monetization_options" + condition.ClearText()] = monetizationOptions;
				return this;
			}

			public GetUserModsFilter MonetizationOptions(ICollection<long> monetizationOptions, Filtering condition = Filtering.None)
			{
				Parameters["monetization_options" + condition.ClearText()] = monetizationOptions;
				return this;
			}

			public GetUserModsFilter PlatformStatus(string platformStatus, Filtering condition = Filtering.None)
			{
				Parameters["platform_status" + condition.ClearText()] = platformStatus;
				return this;
			}

			public GetUserModsFilter PlatformStatus(ICollection<string> platformStatus, Filtering condition = Filtering.None)
			{
				Parameters["platform_status" + condition.ClearText()] = platformStatus;
				return this;
			}
		}

		public class GetUserPurchasesFilter : SearchFilter<GetUserPurchasesFilter>
		{
			internal GetUserPurchasesFilter(int pageIndex, int pageSize)
				: base(pageIndex, pageSize)
			{
			}

			public GetUserPurchasesFilter Id(long id, Filtering condition = Filtering.None)
			{
				Parameters["id" + condition.ClearText()] = id;
				return this;
			}

			public GetUserPurchasesFilter Id(ICollection<long> id, Filtering condition = Filtering.None)
			{
				Parameters["id" + condition.ClearText()] = id;
				return this;
			}

			public GetUserPurchasesFilter GameId(long gameId, Filtering condition = Filtering.None)
			{
				Parameters["game_id" + condition.ClearText()] = gameId;
				return this;
			}

			public GetUserPurchasesFilter GameId(ICollection<long> gameId, Filtering condition = Filtering.None)
			{
				Parameters["game_id" + condition.ClearText()] = gameId;
				return this;
			}

			public GetUserPurchasesFilter Status(long status, Filtering condition = Filtering.None)
			{
				Parameters["status" + condition.ClearText()] = status;
				return this;
			}

			public GetUserPurchasesFilter Status(ICollection<long> status, Filtering condition = Filtering.None)
			{
				Parameters["status" + condition.ClearText()] = status;
				return this;
			}

			public GetUserPurchasesFilter Visible(long visible, Filtering condition = Filtering.None)
			{
				Parameters["visible" + condition.ClearText()] = visible;
				return this;
			}

			public GetUserPurchasesFilter Visible(ICollection<long> visible, Filtering condition = Filtering.None)
			{
				Parameters["visible" + condition.ClearText()] = visible;
				return this;
			}

			public GetUserPurchasesFilter SubmittedBy(long submittedBy, Filtering condition = Filtering.None)
			{
				Parameters["submitted_by" + condition.ClearText()] = submittedBy;
				return this;
			}

			public GetUserPurchasesFilter SubmittedBy(ICollection<long> submittedBy, Filtering condition = Filtering.None)
			{
				Parameters["submitted_by" + condition.ClearText()] = submittedBy;
				return this;
			}

			public GetUserPurchasesFilter DateAdded(long dateAdded, Filtering condition = Filtering.None)
			{
				Parameters["date_added" + condition.ClearText()] = dateAdded;
				return this;
			}

			public GetUserPurchasesFilter DateAdded(ICollection<long> dateAdded, Filtering condition = Filtering.None)
			{
				Parameters["date_added" + condition.ClearText()] = dateAdded;
				return this;
			}

			public GetUserPurchasesFilter DateUpdated(long dateUpdated, Filtering condition = Filtering.None)
			{
				Parameters["date_updated" + condition.ClearText()] = dateUpdated;
				return this;
			}

			public GetUserPurchasesFilter DateUpdated(ICollection<long> dateUpdated, Filtering condition = Filtering.None)
			{
				Parameters["date_updated" + condition.ClearText()] = dateUpdated;
				return this;
			}

			public GetUserPurchasesFilter DateLive(long dateLive, Filtering condition = Filtering.None)
			{
				Parameters["date_live" + condition.ClearText()] = dateLive;
				return this;
			}

			public GetUserPurchasesFilter DateLive(ICollection<long> dateLive, Filtering condition = Filtering.None)
			{
				Parameters["date_live" + condition.ClearText()] = dateLive;
				return this;
			}

			public GetUserPurchasesFilter Name(string name, Filtering condition = Filtering.None)
			{
				Parameters["name" + condition.ClearText()] = name;
				return this;
			}

			public GetUserPurchasesFilter Name(ICollection<string> name, Filtering condition = Filtering.None)
			{
				Parameters["name" + condition.ClearText()] = name;
				return this;
			}

			public GetUserPurchasesFilter NameId(string nameId, Filtering condition = Filtering.None)
			{
				Parameters["name_id" + condition.ClearText()] = nameId;
				return this;
			}

			public GetUserPurchasesFilter NameId(ICollection<string> nameId, Filtering condition = Filtering.None)
			{
				Parameters["name_id" + condition.ClearText()] = nameId;
				return this;
			}

			public GetUserPurchasesFilter Modfile(long modfile, Filtering condition = Filtering.None)
			{
				Parameters["modfile" + condition.ClearText()] = modfile;
				return this;
			}

			public GetUserPurchasesFilter Modfile(ICollection<long> modfile, Filtering condition = Filtering.None)
			{
				Parameters["modfile" + condition.ClearText()] = modfile;
				return this;
			}

			public GetUserPurchasesFilter MetadataKvp(string metadataKvp, Filtering condition = Filtering.None)
			{
				Parameters["metadata_kvp" + condition.ClearText()] = metadataKvp;
				return this;
			}

			public GetUserPurchasesFilter MetadataKvp(ICollection<string> metadataKvp, Filtering condition = Filtering.None)
			{
				Parameters["metadata_kvp" + condition.ClearText()] = metadataKvp;
				return this;
			}

			public GetUserPurchasesFilter MetadataBlob(string metadataBlob, Filtering condition = Filtering.None)
			{
				Parameters["metadata_blob" + condition.ClearText()] = metadataBlob;
				return this;
			}

			public GetUserPurchasesFilter MetadataBlob(ICollection<string> metadataBlob, Filtering condition = Filtering.None)
			{
				Parameters["metadata_blob" + condition.ClearText()] = metadataBlob;
				return this;
			}

			public GetUserPurchasesFilter Tags(string tags, Filtering condition = Filtering.None)
			{
				Parameters["tags" + condition.ClearText()] = tags;
				return this;
			}

			public GetUserPurchasesFilter Tags(ICollection<string> tags, Filtering condition = Filtering.None)
			{
				Parameters["tags" + condition.ClearText()] = tags;
				return this;
			}

			public GetUserPurchasesFilter MaturityOption(long maturityOption, Filtering condition = Filtering.None)
			{
				Parameters["maturity_option" + condition.ClearText()] = maturityOption;
				return this;
			}

			public GetUserPurchasesFilter MaturityOption(ICollection<long> maturityOption, Filtering condition = Filtering.None)
			{
				Parameters["maturity_option" + condition.ClearText()] = maturityOption;
				return this;
			}

			public GetUserPurchasesFilter MonetizationOptions(long monetizationOptions, Filtering condition = Filtering.None)
			{
				Parameters["monetization_options" + condition.ClearText()] = monetizationOptions;
				return this;
			}

			public GetUserPurchasesFilter MonetizationOptions(ICollection<long> monetizationOptions, Filtering condition = Filtering.None)
			{
				Parameters["monetization_options" + condition.ClearText()] = monetizationOptions;
				return this;
			}

			public GetUserPurchasesFilter PlatformStatus(string platformStatus, Filtering condition = Filtering.None)
			{
				Parameters["platform_status" + condition.ClearText()] = platformStatus;
				return this;
			}

			public GetUserPurchasesFilter PlatformStatus(ICollection<string> platformStatus, Filtering condition = Filtering.None)
			{
				Parameters["platform_status" + condition.ClearText()] = platformStatus;
				return this;
			}

			public GetUserPurchasesFilter Platforms(string platforms, Filtering condition = Filtering.None)
			{
				Parameters["platforms" + condition.ClearText()] = platforms;
				return this;
			}

			public GetUserPurchasesFilter Platforms(ICollection<string> platforms, Filtering condition = Filtering.None)
			{
				Parameters["platforms" + condition.ClearText()] = platforms;
				return this;
			}
		}

		public class GetUserRatingsFilter : SearchFilter<GetUserRatingsFilter>
		{
			internal GetUserRatingsFilter(int pageIndex, int pageSize)
				: base(pageIndex, pageSize)
			{
			}

			public GetUserRatingsFilter GameId(long gameId, Filtering condition = Filtering.None)
			{
				Parameters["game_id" + condition.ClearText()] = gameId;
				return this;
			}

			public GetUserRatingsFilter GameId(ICollection<long> gameId, Filtering condition = Filtering.None)
			{
				Parameters["game_id" + condition.ClearText()] = gameId;
				return this;
			}

			public GetUserRatingsFilter ModId(long modId, Filtering condition = Filtering.None)
			{
				Parameters["mod_id" + condition.ClearText()] = modId;
				return this;
			}

			public GetUserRatingsFilter ModId(ICollection<long> modId, Filtering condition = Filtering.None)
			{
				Parameters["mod_id" + condition.ClearText()] = modId;
				return this;
			}

			public GetUserRatingsFilter Rating(long rating, Filtering condition = Filtering.None)
			{
				Parameters["rating" + condition.ClearText()] = rating;
				return this;
			}

			public GetUserRatingsFilter Rating(ICollection<long> rating, Filtering condition = Filtering.None)
			{
				Parameters["rating" + condition.ClearText()] = rating;
				return this;
			}

			public GetUserRatingsFilter DateAdded(long dateAdded, Filtering condition = Filtering.None)
			{
				Parameters["date_added" + condition.ClearText()] = dateAdded;
				return this;
			}

			public GetUserRatingsFilter DateAdded(ICollection<long> dateAdded, Filtering condition = Filtering.None)
			{
				Parameters["date_added" + condition.ClearText()] = dateAdded;
				return this;
			}
		}

		public class GetUserSubscriptionsFilter : SearchFilter<GetUserSubscriptionsFilter>
		{
			internal GetUserSubscriptionsFilter(int pageIndex, int pageSize)
				: base(pageIndex, pageSize)
			{
			}

			public GetUserSubscriptionsFilter Id(long id, Filtering condition = Filtering.None)
			{
				Parameters["id" + condition.ClearText()] = id;
				return this;
			}

			public GetUserSubscriptionsFilter Id(ICollection<long> id, Filtering condition = Filtering.None)
			{
				Parameters["id" + condition.ClearText()] = id;
				return this;
			}

			public GetUserSubscriptionsFilter GameId(long gameId, Filtering condition = Filtering.None)
			{
				Parameters["game_id" + condition.ClearText()] = gameId;
				return this;
			}

			public GetUserSubscriptionsFilter GameId(ICollection<long> gameId, Filtering condition = Filtering.None)
			{
				Parameters["game_id" + condition.ClearText()] = gameId;
				return this;
			}

			public GetUserSubscriptionsFilter Status(long status, Filtering condition = Filtering.None)
			{
				Parameters["status" + condition.ClearText()] = status;
				return this;
			}

			public GetUserSubscriptionsFilter Status(ICollection<long> status, Filtering condition = Filtering.None)
			{
				Parameters["status" + condition.ClearText()] = status;
				return this;
			}

			public GetUserSubscriptionsFilter Visible(long visible, Filtering condition = Filtering.None)
			{
				Parameters["visible" + condition.ClearText()] = visible;
				return this;
			}

			public GetUserSubscriptionsFilter Visible(ICollection<long> visible, Filtering condition = Filtering.None)
			{
				Parameters["visible" + condition.ClearText()] = visible;
				return this;
			}

			public GetUserSubscriptionsFilter SubmittedBy(long submittedBy, Filtering condition = Filtering.None)
			{
				Parameters["submitted_by" + condition.ClearText()] = submittedBy;
				return this;
			}

			public GetUserSubscriptionsFilter SubmittedBy(ICollection<long> submittedBy, Filtering condition = Filtering.None)
			{
				Parameters["submitted_by" + condition.ClearText()] = submittedBy;
				return this;
			}

			public GetUserSubscriptionsFilter DateAdded(long dateAdded, Filtering condition = Filtering.None)
			{
				Parameters["date_added" + condition.ClearText()] = dateAdded;
				return this;
			}

			public GetUserSubscriptionsFilter DateAdded(ICollection<long> dateAdded, Filtering condition = Filtering.None)
			{
				Parameters["date_added" + condition.ClearText()] = dateAdded;
				return this;
			}

			public GetUserSubscriptionsFilter DateUpdated(long dateUpdated, Filtering condition = Filtering.None)
			{
				Parameters["date_updated" + condition.ClearText()] = dateUpdated;
				return this;
			}

			public GetUserSubscriptionsFilter DateUpdated(ICollection<long> dateUpdated, Filtering condition = Filtering.None)
			{
				Parameters["date_updated" + condition.ClearText()] = dateUpdated;
				return this;
			}

			public GetUserSubscriptionsFilter DateLive(long dateLive, Filtering condition = Filtering.None)
			{
				Parameters["date_live" + condition.ClearText()] = dateLive;
				return this;
			}

			public GetUserSubscriptionsFilter DateLive(ICollection<long> dateLive, Filtering condition = Filtering.None)
			{
				Parameters["date_live" + condition.ClearText()] = dateLive;
				return this;
			}

			public GetUserSubscriptionsFilter Name(string name, Filtering condition = Filtering.None)
			{
				Parameters["name" + condition.ClearText()] = name;
				return this;
			}

			public GetUserSubscriptionsFilter Name(ICollection<string> name, Filtering condition = Filtering.None)
			{
				Parameters["name" + condition.ClearText()] = name;
				return this;
			}

			public GetUserSubscriptionsFilter NameId(string nameId, Filtering condition = Filtering.None)
			{
				Parameters["name_id" + condition.ClearText()] = nameId;
				return this;
			}

			public GetUserSubscriptionsFilter NameId(ICollection<string> nameId, Filtering condition = Filtering.None)
			{
				Parameters["name_id" + condition.ClearText()] = nameId;
				return this;
			}

			public GetUserSubscriptionsFilter Modfile(long modfile, Filtering condition = Filtering.None)
			{
				Parameters["modfile" + condition.ClearText()] = modfile;
				return this;
			}

			public GetUserSubscriptionsFilter Modfile(ICollection<long> modfile, Filtering condition = Filtering.None)
			{
				Parameters["modfile" + condition.ClearText()] = modfile;
				return this;
			}

			public GetUserSubscriptionsFilter MetadataKvp(string metadataKvp, Filtering condition = Filtering.None)
			{
				Parameters["metadata_kvp" + condition.ClearText()] = metadataKvp;
				return this;
			}

			public GetUserSubscriptionsFilter MetadataKvp(ICollection<string> metadataKvp, Filtering condition = Filtering.None)
			{
				Parameters["metadata_kvp" + condition.ClearText()] = metadataKvp;
				return this;
			}

			public GetUserSubscriptionsFilter MetadataBlob(string metadataBlob, Filtering condition = Filtering.None)
			{
				Parameters["metadata_blob" + condition.ClearText()] = metadataBlob;
				return this;
			}

			public GetUserSubscriptionsFilter MetadataBlob(ICollection<string> metadataBlob, Filtering condition = Filtering.None)
			{
				Parameters["metadata_blob" + condition.ClearText()] = metadataBlob;
				return this;
			}

			public GetUserSubscriptionsFilter Tags(string tags, Filtering condition = Filtering.None)
			{
				Parameters["tags" + condition.ClearText()] = tags;
				return this;
			}

			public GetUserSubscriptionsFilter Tags(ICollection<string> tags, Filtering condition = Filtering.None)
			{
				Parameters["tags" + condition.ClearText()] = tags;
				return this;
			}

			public GetUserSubscriptionsFilter MaturityOption(long maturityOption, Filtering condition = Filtering.None)
			{
				Parameters["maturity_option" + condition.ClearText()] = maturityOption;
				return this;
			}

			public GetUserSubscriptionsFilter MaturityOption(ICollection<long> maturityOption, Filtering condition = Filtering.None)
			{
				Parameters["maturity_option" + condition.ClearText()] = maturityOption;
				return this;
			}

			public GetUserSubscriptionsFilter MonetizationOptions(long monetizationOptions, Filtering condition = Filtering.None)
			{
				Parameters["monetization_options" + condition.ClearText()] = monetizationOptions;
				return this;
			}

			public GetUserSubscriptionsFilter MonetizationOptions(ICollection<long> monetizationOptions, Filtering condition = Filtering.None)
			{
				Parameters["monetization_options" + condition.ClearText()] = monetizationOptions;
				return this;
			}

			public GetUserSubscriptionsFilter PlatformStatus(string platformStatus, Filtering condition = Filtering.None)
			{
				Parameters["platform_status" + condition.ClearText()] = platformStatus;
				return this;
			}

			public GetUserSubscriptionsFilter PlatformStatus(ICollection<string> platformStatus, Filtering condition = Filtering.None)
			{
				Parameters["platform_status" + condition.ClearText()] = platformStatus;
				return this;
			}
		}

		internal static async Task<(Error error, JToken userObject)> GetAuthenticatedUserAsJToken(string xModioDelegationToken = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), userObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/me");
			request.Options.AddHeaderParameter("X-Modio-Delegation-Token", xModioDelegationToken);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, UserObject? userObject)> GetAuthenticatedUser(string xModioDelegationToken = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), userObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/me");
			request.Options.AddHeaderParameter("X-Modio-Delegation-Token", xModioDelegationToken);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<UserObject>(request);
		}

		internal static async Task<(Error error, JToken userEventObjects)> GetUserEventsAsJToken(GetUserEventsFilter filter)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), userEventObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/me/events", ModioAPIRequestMethod.Get, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddFilterParameters(filter);
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, Pagination<UserEventObject[]>? userEventObjects)> GetUserEvents(GetUserEventsFilter filter)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), userEventObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/me/events", ModioAPIRequestMethod.Get, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddFilterParameters(filter);
			return await _apiInterface.GetJson<Pagination<UserEventObject[]>>(request);
		}

		public static GetUserEventsFilter FilterGetUserEvents(int pageIndex = 0, int pageSize = 100)
		{
			return new GetUserEventsFilter(pageIndex, pageSize);
		}

		internal static async Task<(Error error, JToken gameObjects)> GetUserGamesAsJToken(GetUserGamesFilter filter)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), gameObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/me/games");
			request.Options.AddFilterParameters(filter);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, Pagination<GameObject[]>? gameObjects)> GetUserGames(GetUserGamesFilter filter)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), gameObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/me/games");
			request.Options.AddFilterParameters(filter);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<Pagination<GameObject[]>>(request);
		}

		public static GetUserGamesFilter FilterGetUserGames(int pageIndex = 0, int pageSize = 100)
		{
			return new GetUserGamesFilter(pageIndex, pageSize);
		}

		internal static async Task<(Error error, JToken modfileObjects)> GetUserModfilesAsJToken(GetUserModfilesFilter filter)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), modfileObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/me/files");
			request.Options.AddFilterParameters(filter);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, Pagination<ModfileObject[]>? modfileObjects)> GetUserModfiles(GetUserModfilesFilter filter)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), modfileObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/me/files");
			request.Options.AddFilterParameters(filter);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<Pagination<ModfileObject[]>>(request);
		}

		public static GetUserModfilesFilter FilterGetUserModfiles(int pageIndex = 0, int pageSize = 100)
		{
			return new GetUserModfilesFilter(pageIndex, pageSize);
		}

		internal static async Task<(Error error, JToken modObjects)> GetUserModsAsJToken(GetUserModsFilter filter)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), modObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/me/mods");
			request.Options.AddFilterParameters(filter);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, Pagination<ModObject[]>? modObjects)> GetUserMods(GetUserModsFilter filter)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), modObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/me/mods");
			request.Options.AddFilterParameters(filter);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<Pagination<ModObject[]>>(request);
		}

		public static GetUserModsFilter FilterGetUserMods(int pageIndex = 0, int pageSize = 100)
		{
			return new GetUserModsFilter(pageIndex, pageSize);
		}

		internal static async Task<(Error error, JToken modObjects)> GetUserPurchasesAsJToken(GetUserPurchasesFilter filter)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), modObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/me/purchased");
			request.Options.AddFilterParameters(filter);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, Pagination<ModObject[]>? modObjects)> GetUserPurchases(GetUserPurchasesFilter filter)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), modObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/me/purchased");
			request.Options.AddFilterParameters(filter);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<Pagination<ModObject[]>>(request);
		}

		public static GetUserPurchasesFilter FilterGetUserPurchases(int pageIndex = 0, int pageSize = 100)
		{
			return new GetUserPurchasesFilter(pageIndex, pageSize);
		}

		internal static async Task<(Error error, JToken ratingObjects)> GetUserRatingsAsJToken(GetUserRatingsFilter filter)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), ratingObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/me/ratings");
			request.Options.AddFilterParameters(filter);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, Pagination<RatingObject[]>? ratingObjects)> GetUserRatings(GetUserRatingsFilter filter)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), ratingObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/me/ratings");
			request.Options.AddFilterParameters(filter);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<Pagination<RatingObject[]>>(request);
		}

		public static GetUserRatingsFilter FilterGetUserRatings(int pageIndex = 0, int pageSize = 100)
		{
			return new GetUserRatingsFilter(pageIndex, pageSize);
		}

		internal static async Task<(Error error, JToken userObjects)> GetUsersMutedAsJToken()
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), userObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/me/users/muted");
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, Pagination<UserObject[]>? userObjects)> GetUsersMuted()
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), userObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/me/users/muted");
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<Pagination<UserObject[]>>(request);
		}

		internal static async Task<(Error error, JToken modObjects)> GetUserSubscriptionsAsJToken(GetUserSubscriptionsFilter filter)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), modObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/me/subscribed");
			request.Options.AddFilterParameters(filter);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, Pagination<ModObject[]>? modObjects)> GetUserSubscriptions(GetUserSubscriptionsFilter filter)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), modObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/me/subscribed");
			request.Options.AddFilterParameters(filter);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<Pagination<ModObject[]>>(request);
		}

		public static GetUserSubscriptionsFilter FilterGetUserSubscriptions(int pageIndex = 0, int pageSize = 100)
		{
			return new GetUserSubscriptionsFilter(pageIndex, pageSize);
		}

		internal static async Task<(Error error, JToken walletObject)> GetUserWalletAsJToken(long gameId)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), walletObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/me/wallets");
			request.Options.AddQueryParameter("game_id", gameId);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, WalletObject? walletObject)> GetUserWallet(long gameId)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), walletObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/me/wallets");
			request.Options.AddQueryParameter("game_id", gameId);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<WalletObject>(request);
		}
	}

	public static class Games
	{
		public class GetGamesFilter : SearchFilter<GetGamesFilter>
		{
			internal GetGamesFilter(int pageIndex, int pageSize)
				: base(pageIndex, pageSize)
			{
			}

			public GetGamesFilter Id(long id, Filtering condition = Filtering.None)
			{
				Parameters["id" + condition.ClearText()] = id;
				return this;
			}

			public GetGamesFilter Id(ICollection<long> id, Filtering condition = Filtering.None)
			{
				Parameters["id" + condition.ClearText()] = id;
				return this;
			}

			public GetGamesFilter Status(long status, Filtering condition = Filtering.None)
			{
				Parameters["status" + condition.ClearText()] = status;
				return this;
			}

			public GetGamesFilter Status(ICollection<long> status, Filtering condition = Filtering.None)
			{
				Parameters["status" + condition.ClearText()] = status;
				return this;
			}

			public GetGamesFilter SubmittedBy(long submittedBy, Filtering condition = Filtering.None)
			{
				Parameters["submitted_by" + condition.ClearText()] = submittedBy;
				return this;
			}

			public GetGamesFilter SubmittedBy(ICollection<long> submittedBy, Filtering condition = Filtering.None)
			{
				Parameters["submitted_by" + condition.ClearText()] = submittedBy;
				return this;
			}

			public GetGamesFilter DateAdded(long dateAdded, Filtering condition = Filtering.None)
			{
				Parameters["date_added" + condition.ClearText()] = dateAdded;
				return this;
			}

			public GetGamesFilter DateAdded(ICollection<long> dateAdded, Filtering condition = Filtering.None)
			{
				Parameters["date_added" + condition.ClearText()] = dateAdded;
				return this;
			}

			public GetGamesFilter DateUpdated(long dateUpdated, Filtering condition = Filtering.None)
			{
				Parameters["date_updated" + condition.ClearText()] = dateUpdated;
				return this;
			}

			public GetGamesFilter DateUpdated(ICollection<long> dateUpdated, Filtering condition = Filtering.None)
			{
				Parameters["date_updated" + condition.ClearText()] = dateUpdated;
				return this;
			}

			public GetGamesFilter DateLive(long dateLive, Filtering condition = Filtering.None)
			{
				Parameters["date_live" + condition.ClearText()] = dateLive;
				return this;
			}

			public GetGamesFilter DateLive(ICollection<long> dateLive, Filtering condition = Filtering.None)
			{
				Parameters["date_live" + condition.ClearText()] = dateLive;
				return this;
			}

			public GetGamesFilter Name(string name, Filtering condition = Filtering.None)
			{
				Parameters["name" + condition.ClearText()] = name;
				return this;
			}

			public GetGamesFilter Name(ICollection<string> name, Filtering condition = Filtering.None)
			{
				Parameters["name" + condition.ClearText()] = name;
				return this;
			}

			public GetGamesFilter NameId(string nameId, Filtering condition = Filtering.None)
			{
				Parameters["name_id" + condition.ClearText()] = nameId;
				return this;
			}

			public GetGamesFilter NameId(ICollection<string> nameId, Filtering condition = Filtering.None)
			{
				Parameters["name_id" + condition.ClearText()] = nameId;
				return this;
			}

			public GetGamesFilter Summary(string summary, Filtering condition = Filtering.None)
			{
				Parameters["summary" + condition.ClearText()] = summary;
				return this;
			}

			public GetGamesFilter Summary(ICollection<string> summary, Filtering condition = Filtering.None)
			{
				Parameters["summary" + condition.ClearText()] = summary;
				return this;
			}

			public GetGamesFilter InstructionsUrl(string instructionsUrl, Filtering condition = Filtering.None)
			{
				Parameters["instructions_url" + condition.ClearText()] = instructionsUrl;
				return this;
			}

			public GetGamesFilter InstructionsUrl(ICollection<string> instructionsUrl, Filtering condition = Filtering.None)
			{
				Parameters["instructions_url" + condition.ClearText()] = instructionsUrl;
				return this;
			}

			public GetGamesFilter UgcName(string ugcName, Filtering condition = Filtering.None)
			{
				Parameters["ugc_name" + condition.ClearText()] = ugcName;
				return this;
			}

			public GetGamesFilter UgcName(ICollection<string> ugcName, Filtering condition = Filtering.None)
			{
				Parameters["ugc_name" + condition.ClearText()] = ugcName;
				return this;
			}

			public GetGamesFilter PresentationOption(long presentationOption, Filtering condition = Filtering.None)
			{
				Parameters["presentation_option" + condition.ClearText()] = presentationOption;
				return this;
			}

			public GetGamesFilter PresentationOption(ICollection<long> presentationOption, Filtering condition = Filtering.None)
			{
				Parameters["presentation_option" + condition.ClearText()] = presentationOption;
				return this;
			}

			public GetGamesFilter SubmissionOption(long submissionOption, Filtering condition = Filtering.None)
			{
				Parameters["submission_option" + condition.ClearText()] = submissionOption;
				return this;
			}

			public GetGamesFilter SubmissionOption(ICollection<long> submissionOption, Filtering condition = Filtering.None)
			{
				Parameters["submission_option" + condition.ClearText()] = submissionOption;
				return this;
			}

			public GetGamesFilter CurationOption(long curationOption, Filtering condition = Filtering.None)
			{
				Parameters["curation_option" + condition.ClearText()] = curationOption;
				return this;
			}

			public GetGamesFilter CurationOption(ICollection<long> curationOption, Filtering condition = Filtering.None)
			{
				Parameters["curation_option" + condition.ClearText()] = curationOption;
				return this;
			}

			public GetGamesFilter DependencyOption(long dependencyOption, Filtering condition = Filtering.None)
			{
				Parameters["dependency_option" + condition.ClearText()] = dependencyOption;
				return this;
			}

			public GetGamesFilter DependencyOption(ICollection<long> dependencyOption, Filtering condition = Filtering.None)
			{
				Parameters["dependency_option" + condition.ClearText()] = dependencyOption;
				return this;
			}

			public GetGamesFilter CommunityOptions(long communityOptions, Filtering condition = Filtering.None)
			{
				Parameters["community_options" + condition.ClearText()] = communityOptions;
				return this;
			}

			public GetGamesFilter CommunityOptions(ICollection<long> communityOptions, Filtering condition = Filtering.None)
			{
				Parameters["community_options" + condition.ClearText()] = communityOptions;
				return this;
			}

			public GetGamesFilter MonetizationOptions(long monetizationOptions, Filtering condition = Filtering.None)
			{
				Parameters["monetization_options" + condition.ClearText()] = monetizationOptions;
				return this;
			}

			public GetGamesFilter MonetizationOptions(ICollection<long> monetizationOptions, Filtering condition = Filtering.None)
			{
				Parameters["monetization_options" + condition.ClearText()] = monetizationOptions;
				return this;
			}

			public GetGamesFilter ApiAccessOptions(long apiAccessOptions, Filtering condition = Filtering.None)
			{
				Parameters["api_access_options" + condition.ClearText()] = apiAccessOptions;
				return this;
			}

			public GetGamesFilter ApiAccessOptions(ICollection<long> apiAccessOptions, Filtering condition = Filtering.None)
			{
				Parameters["api_access_options" + condition.ClearText()] = apiAccessOptions;
				return this;
			}

			public GetGamesFilter MaturityOptions(long maturityOptions, Filtering condition = Filtering.None)
			{
				Parameters["maturity_options" + condition.ClearText()] = maturityOptions;
				return this;
			}

			public GetGamesFilter MaturityOptions(ICollection<long> maturityOptions, Filtering condition = Filtering.None)
			{
				Parameters["maturity_options" + condition.ClearText()] = maturityOptions;
				return this;
			}

			public GetGamesFilter ShowHiddenTags(bool showHiddenTags, Filtering condition = Filtering.None)
			{
				Parameters["show_hidden_tags" + condition.ClearText()] = showHiddenTags;
				return this;
			}

			public GetGamesFilter ShowHiddenTags(ICollection<bool> showHiddenTags, Filtering condition = Filtering.None)
			{
				Parameters["show_hidden_tags" + condition.ClearText()] = showHiddenTags;
				return this;
			}
		}

		internal static async Task<(Error error, JToken gameObject)> GetGameAsJToken(bool? showHiddenTags = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), gameObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/games/{game-id}");
			request.Options.AddQueryParameter("show_hidden_tags", showHiddenTags);
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, GameObject? gameObject)> GetGame(bool? showHiddenTags = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), gameObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/games/{game-id}");
			request.Options.AddQueryParameter("show_hidden_tags", showHiddenTags);
			return await _apiInterface.GetJson<GameObject>(request);
		}

		internal static async Task<(Error error, JToken gameObjects)> GetGamesAsJToken(GetGamesFilter filter)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), gameObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/games");
			request.Options.AddFilterParameters(filter);
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, Pagination<GameObject[]>? gameObjects)> GetGames(GetGamesFilter filter)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), gameObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/games");
			request.Options.AddFilterParameters(filter);
			return await _apiInterface.GetJson<Pagination<GameObject[]>>(request);
		}

		public static GetGamesFilter FilterGetGames(int pageIndex = 0, int pageSize = 100)
		{
			return new GetGamesFilter(pageIndex, pageSize);
		}
	}

	public static class Stats
	{
		public class GetModsStatsFilter : SearchFilter<GetModsStatsFilter>
		{
			internal GetModsStatsFilter(int pageIndex, int pageSize)
				: base(pageIndex, pageSize)
			{
			}

			public GetModsStatsFilter ModId(long modId, Filtering condition = Filtering.None)
			{
				Parameters["mod_id" + condition.ClearText()] = modId;
				return this;
			}

			public GetModsStatsFilter ModId(ICollection<long> modId, Filtering condition = Filtering.None)
			{
				Parameters["mod_id" + condition.ClearText()] = modId;
				return this;
			}

			public GetModsStatsFilter PopularityRankPosition(long popularityRankPosition, Filtering condition = Filtering.None)
			{
				Parameters["popularity_rank_position" + condition.ClearText()] = popularityRankPosition;
				return this;
			}

			public GetModsStatsFilter PopularityRankPosition(ICollection<long> popularityRankPosition, Filtering condition = Filtering.None)
			{
				Parameters["popularity_rank_position" + condition.ClearText()] = popularityRankPosition;
				return this;
			}

			public GetModsStatsFilter PopularityRankTotalMods(long popularityRankTotalMods, Filtering condition = Filtering.None)
			{
				Parameters["popularity_rank_total_mods" + condition.ClearText()] = popularityRankTotalMods;
				return this;
			}

			public GetModsStatsFilter PopularityRankTotalMods(ICollection<long> popularityRankTotalMods, Filtering condition = Filtering.None)
			{
				Parameters["popularity_rank_total_mods" + condition.ClearText()] = popularityRankTotalMods;
				return this;
			}

			public GetModsStatsFilter DownloadsTotal(long downloadsTotal, Filtering condition = Filtering.None)
			{
				Parameters["downloads_total" + condition.ClearText()] = downloadsTotal;
				return this;
			}

			public GetModsStatsFilter DownloadsTotal(ICollection<long> downloadsTotal, Filtering condition = Filtering.None)
			{
				Parameters["downloads_total" + condition.ClearText()] = downloadsTotal;
				return this;
			}

			public GetModsStatsFilter SubscribersTotal(long subscribersTotal, Filtering condition = Filtering.None)
			{
				Parameters["subscribers_total" + condition.ClearText()] = subscribersTotal;
				return this;
			}

			public GetModsStatsFilter SubscribersTotal(ICollection<long> subscribersTotal, Filtering condition = Filtering.None)
			{
				Parameters["subscribers_total" + condition.ClearText()] = subscribersTotal;
				return this;
			}

			public GetModsStatsFilter RatingsPositive(long ratingsPositive, Filtering condition = Filtering.None)
			{
				Parameters["ratings_positive" + condition.ClearText()] = ratingsPositive;
				return this;
			}

			public GetModsStatsFilter RatingsPositive(ICollection<long> ratingsPositive, Filtering condition = Filtering.None)
			{
				Parameters["ratings_positive" + condition.ClearText()] = ratingsPositive;
				return this;
			}

			public GetModsStatsFilter RatingsNegative(long ratingsNegative, Filtering condition = Filtering.None)
			{
				Parameters["ratings_negative" + condition.ClearText()] = ratingsNegative;
				return this;
			}

			public GetModsStatsFilter RatingsNegative(ICollection<long> ratingsNegative, Filtering condition = Filtering.None)
			{
				Parameters["ratings_negative" + condition.ClearText()] = ratingsNegative;
				return this;
			}
		}

		internal static async Task<(Error error, JToken gameStatsObject)> GetGameStatsAsJToken()
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), gameStatsObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/games/{game-id}/stats", ModioAPIRequestMethod.Get, ModioAPIRequestContentType.FormUrlEncoded);
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, GameStatsObject? gameStatsObject)> GetGameStats()
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), gameStatsObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/games/{game-id}/stats", ModioAPIRequestMethod.Get, ModioAPIRequestContentType.FormUrlEncoded);
			return await _apiInterface.GetJson<GameStatsObject>(request);
		}

		internal static async Task<(Error error, JToken modStatsObjects)> GetModsStatsAsJToken(GetModsStatsFilter filter)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), modStatsObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/games/{game-id}/mods/stats", ModioAPIRequestMethod.Get, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddFilterParameters(filter);
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, Pagination<ModStatsObject[]>? modStatsObjects)> GetModsStats(GetModsStatsFilter filter)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), modStatsObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/games/{game-id}/mods/stats", ModioAPIRequestMethod.Get, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddFilterParameters(filter);
			return await _apiInterface.GetJson<Pagination<ModStatsObject[]>>(request);
		}

		public static GetModsStatsFilter FilterGetModsStats(int pageIndex = 0, int pageSize = 100)
		{
			return new GetModsStatsFilter(pageIndex, pageSize);
		}

		internal static async Task<(Error error, JToken modStatsObject)> GetModStatsAsJToken(long modId)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), modStatsObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/stats", ModioAPIRequestMethod.Get, ModioAPIRequestContentType.FormUrlEncoded);
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, ModStatsObject? modStatsObject)> GetModStats(long modId)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), modStatsObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/stats", ModioAPIRequestMethod.Get, ModioAPIRequestContentType.FormUrlEncoded);
			return await _apiInterface.GetJson<ModStatsObject>(request);
		}
	}

	public static class Events
	{
		public class GetModEventsFilter : SearchFilter<GetModEventsFilter>
		{
			internal GetModEventsFilter(int pageIndex, int pageSize)
				: base(pageIndex, pageSize)
			{
			}
		}

		public class GetModsEventsFilter : SearchFilter<GetModsEventsFilter>
		{
			internal GetModsEventsFilter(int pageIndex, int pageSize)
				: base(pageIndex, pageSize)
			{
			}

			public GetModsEventsFilter Id(long id, Filtering condition = Filtering.None)
			{
				Parameters["id" + condition.ClearText()] = id;
				return this;
			}

			public GetModsEventsFilter Id(ICollection<long> id, Filtering condition = Filtering.None)
			{
				Parameters["id" + condition.ClearText()] = id;
				return this;
			}

			public GetModsEventsFilter ModId(long modId, Filtering condition = Filtering.None)
			{
				Parameters["mod_id" + condition.ClearText()] = modId;
				return this;
			}

			public GetModsEventsFilter ModId(ICollection<long> modId, Filtering condition = Filtering.None)
			{
				Parameters["mod_id" + condition.ClearText()] = modId;
				return this;
			}

			public GetModsEventsFilter UserId(long userId, Filtering condition = Filtering.None)
			{
				Parameters["user_id" + condition.ClearText()] = userId;
				return this;
			}

			public GetModsEventsFilter UserId(ICollection<long> userId, Filtering condition = Filtering.None)
			{
				Parameters["user_id" + condition.ClearText()] = userId;
				return this;
			}

			public GetModsEventsFilter DateAdded(long dateAdded, Filtering condition = Filtering.None)
			{
				Parameters["date_added" + condition.ClearText()] = dateAdded;
				return this;
			}

			public GetModsEventsFilter DateAdded(ICollection<long> dateAdded, Filtering condition = Filtering.None)
			{
				Parameters["date_added" + condition.ClearText()] = dateAdded;
				return this;
			}

			public GetModsEventsFilter EventType(string eventType, Filtering condition = Filtering.None)
			{
				Parameters["event_type" + condition.ClearText()] = eventType;
				return this;
			}

			public GetModsEventsFilter EventType(ICollection<string> eventType, Filtering condition = Filtering.None)
			{
				Parameters["event_type" + condition.ClearText()] = eventType;
				return this;
			}

			public GetModsEventsFilter Latest(bool latest, Filtering condition = Filtering.None)
			{
				Parameters["latest" + condition.ClearText()] = latest;
				return this;
			}

			public GetModsEventsFilter Latest(ICollection<bool> latest, Filtering condition = Filtering.None)
			{
				Parameters["latest" + condition.ClearText()] = latest;
				return this;
			}

			public GetModsEventsFilter Subscribed(bool subscribed, Filtering condition = Filtering.None)
			{
				Parameters["subscribed" + condition.ClearText()] = subscribed;
				return this;
			}

			public GetModsEventsFilter Subscribed(ICollection<bool> subscribed, Filtering condition = Filtering.None)
			{
				Parameters["subscribed" + condition.ClearText()] = subscribed;
				return this;
			}
		}

		internal static async Task<(Error error, JToken modEventObjects)> GetModEventsAsJToken(long modId, GetModEventsFilter filter)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), modEventObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/events", ModioAPIRequestMethod.Get, ModioAPIRequestContentType.FormUrlEncoded);
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, Pagination<ModEventObject[]>? modEventObjects)> GetModEvents(long modId, GetModEventsFilter filter)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), modEventObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/events", ModioAPIRequestMethod.Get, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddFilterParameters(filter);
			return await _apiInterface.GetJson<Pagination<ModEventObject[]>>(request);
		}

		public static GetModEventsFilter FilterGetModEvents(int pageIndex = 0, int pageSize = 100)
		{
			return new GetModEventsFilter(pageIndex, pageSize);
		}

		internal static async Task<(Error error, JToken modEventObjects)> GetModsEventsAsJToken(GetModsEventsFilter filter)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), modEventObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/games/{game-id}/mods/events", ModioAPIRequestMethod.Get, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddFilterParameters(filter);
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, Pagination<ModEventObject[]>? modEventObjects)> GetModsEvents(GetModsEventsFilter filter)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), modEventObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/games/{game-id}/mods/events", ModioAPIRequestMethod.Get, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddFilterParameters(filter);
			return await _apiInterface.GetJson<Pagination<ModEventObject[]>>(request);
		}

		public static GetModsEventsFilter FilterGetModsEvents(int pageIndex = 0, int pageSize = 100)
		{
			return new GetModsEventsFilter(pageIndex, pageSize);
		}
	}

	public static class General
	{
		internal static async Task<(Error error, JToken userObject)> GetResourceOwnerAsJToken()
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), userObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/general/ownership", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, UserObject? userObject)> GetResourceOwner()
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), userObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/general/ownership", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<UserObject>(request);
		}

		internal static async Task<(Error error, JToken webMessageObject)> PingAsJToken()
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), webMessageObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/ping");
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, WebMessageObject? webMessageObject)> Ping()
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), webMessageObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/ping");
			return await _apiInterface.GetJson<WebMessageObject>(request);
		}
	}

	public static class Metrics
	{
		internal static async Task<(Error error, JToken response204)> MetricsSessionEndAsJToken(MetricsSessionRequest sessionRequest)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), response204: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/metrics/sessions/end", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.String, "application/json");
			request.Options.AddBody(sessionRequest, "application/json");
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, Response204? response204)> MetricsSessionEnd(MetricsSessionRequest sessionRequest)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), response204: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/metrics/sessions/end", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.String, "application/json");
			request.Options.AddBody(sessionRequest, "application/json");
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<Response204>(request);
		}

		internal static async Task<(Error error, JToken response204)> MetricsSessionHeartbeatAsJToken(MetricsSessionRequest sessionRequest)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), response204: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/metrics/sessions/heartbeat", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.String, "application/json");
			request.Options.AddBody(sessionRequest, "application/json");
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, Response204? response204)> MetricsSessionHeartbeat(MetricsSessionRequest sessionRequest)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), response204: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/metrics/sessions/heartbeat", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.String, "application/json");
			request.Options.AddBody(sessionRequest, "application/json");
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<Response204>(request);
		}

		internal static async Task<(Error error, JToken response204)> MetricsSessionStartAsJToken(MetricsSessionRequest sessionRequest)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), response204: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/metrics/sessions/start", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.String, "application/json");
			request.Options.AddBody(sessionRequest, "application/json");
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, Response204? response204)> MetricsSessionStart(MetricsSessionRequest sessionRequest)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), response204: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/metrics/sessions/start", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.String, "application/json");
			request.Options.AddBody(sessionRequest, "application/json");
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<Response204>(request);
		}
	}

	public static class Users
	{
		internal static async Task<(Error error, JToken response204)> MuteAUserAsJToken(long userId)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), response204: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/users/{userId}/mute", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, Response204? response204)> MuteAUser(long userId)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), response204: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/users/{userId}/mute", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<Response204>(request);
		}

		internal static async Task<(Error error, JToken response204)> UnmuteAUserAsJToken(long userId)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), response204: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/users/{userId}/mute", ModioAPIRequestMethod.Delete, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, Response204? response204)> UnmuteAUser(long userId)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), response204: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/users/{userId}/mute", ModioAPIRequestMethod.Delete, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<Response204>(request);
		}
	}

	public static class ServiceToService
	{
		internal static async Task<(Error error, JToken userDelegationTokenObject)> RequestUserDelegationTokenAsJToken()
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), userDelegationTokenObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/me/s2s/oauth/token", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, UserDelegationTokenObject? userDelegationTokenObject)> RequestUserDelegationToken()
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), userDelegationTokenObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/me/s2s/oauth/token", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<UserDelegationTokenObject>(request);
		}
	}

	public static class Reports
	{
		internal static async Task<(Error error, JToken addReportResponse)> SubmitReportAsJToken(AddReportRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), addReportResponse: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/report", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, AddReportResponse? addReportResponse)> SubmitReport(AddReportRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), addReportResponse: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/report", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<AddReportResponse>(request);
		}
	}

	public static class Subscribe
	{
		internal static async Task<(Error error, JToken modObject)> SubscribeToModAsJToken(long modId, AddModSubscriptionRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), modObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/subscribe", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, ModObject? modObject)> SubscribeToMod(long modId, AddModSubscriptionRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), modObject: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/subscribe", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<ModObject>(request);
		}

		internal static async Task<(Error error, JToken response204)> UnsubscribeFromModAsJToken(long modId)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), response204: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/subscribe", ModioAPIRequestMethod.Delete, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, Response204? response204)> UnsubscribeFromMod(long modId)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), response204: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New($"/games/{{game-id}}/mods/{modId}/subscribe", ModioAPIRequestMethod.Delete, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<Response204>(request);
		}
	}

	public static class InAppPurchases
	{
		internal static async Task<(Error error, JToken entitlementFulfillmentObjects)> SyncAppleEntitlementAsJToken(SyncAppleEntitlementsRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), entitlementFulfillmentObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/me/iap/apple/sync", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, Pagination<EntitlementFulfillmentObject[]>? entitlementFulfillmentObjects)> SyncAppleEntitlement(SyncAppleEntitlementsRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), entitlementFulfillmentObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/me/iap/apple/sync", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<Pagination<EntitlementFulfillmentObject[]>>(request);
		}

		internal static async Task<(Error error, JToken entitlementFulfillmentObjects)> SyncGoogleEntitlementsAsJToken()
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), entitlementFulfillmentObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/me/iap/google/sync", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		internal static async Task<(Error error, Pagination<EntitlementFulfillmentObject[]>? entitlementFulfillmentObjects)> SyncGoogleEntitlements()
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), entitlementFulfillmentObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/me/iap/google/sync", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<Pagination<EntitlementFulfillmentObject[]>>(request);
		}

		public static async Task<(Error error, JToken entitlementFulfillmentObjects)> SyncMetaEntitlementAsJToken()
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), entitlementFulfillmentObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/me/iap/meta/sync", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		public static async Task<(Error error, Pagination<EntitlementFulfillmentObject[]>? entitlementFulfillmentObjects)> SyncMetaEntitlement(long userId)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), entitlementFulfillmentObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/me/iap/meta/sync", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			string value = ((_platform == Platform.Oculus || _platform == Platform.Android) ? "quest" : "rift");
			request.Options.RequireAuthentication();
			request.Options.AddQueryParameter("device", value);
			request.Options.AddQueryParameter("user_id", userId.ToString());
			return await _apiInterface.GetJson<Pagination<EntitlementFulfillmentObject[]>>(request);
		}

		public static async Task<(Error error, JToken entitlementFulfillmentObjects)> SyncPlaystationNetworkEntitlementsAsJToken(SyncPlayStationNetworkEntitlementsRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), entitlementFulfillmentObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/me/iap/psn/sync", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		public static async Task<(Error error, Pagination<EntitlementFulfillmentObject[]>? entitlementFulfillmentObjects)> SyncPlaystationNetworkEntitlements(SyncPlayStationNetworkEntitlementsRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), entitlementFulfillmentObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/me/iap/psn/sync", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<Pagination<EntitlementFulfillmentObject[]>>(request);
		}

		public static async Task<(Error error, JToken entitlementFulfillmentObjects)> SyncSteamEntitlementAsJToken()
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), entitlementFulfillmentObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/me/iap/steam/sync", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		public static async Task<(Error error, Pagination<EntitlementFulfillmentObject[]>? entitlementFulfillmentObjects)> SyncSteamEntitlement()
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), entitlementFulfillmentObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/me/iap/steam/sync", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<Pagination<EntitlementFulfillmentObject[]>>(request);
		}

		public static async Task<(Error error, JToken entitlementFulfillmentObjects)> SyncXboxLiveEntitlementsAsJToken(SyncXboxEntitlementsRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), entitlementFulfillmentObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/me/iap/xboxlive/sync", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson(request);
		}

		public static async Task<(Error error, Pagination<EntitlementFulfillmentObject[]>? entitlementFulfillmentObjects)> SyncXboxLiveEntitlements(SyncXboxEntitlementsRequest? body = null)
		{
			if (!IsInitialized())
			{
				return (error: new Error(ErrorCode.API_NOT_INITIALIZED), entitlementFulfillmentObjects: null);
			}
			using ModioAPIRequest request = ModioAPIRequest.New("/me/iap/xboxlive/sync", ModioAPIRequestMethod.Post, ModioAPIRequestContentType.FormUrlEncoded);
			request.Options.AddBody(body);
			request.Options.RequireAuthentication();
			return await _apiInterface.GetJson<Pagination<EntitlementFulfillmentObject[]>>(request);
		}
	}

	public enum Platform
	{
		None = -1,
		Source,
		Windows,
		Mac,
		Linux,
		Android,
		IOS,
		XboxOne,
		XboxSeriesX,
		PlayStation4,
		PlayStation5,
		Switch,
		Oculus
	}

	public enum Portal
	{
		None = -1,
		Apple,
		Discord,
		EpicGamesStore,
		Facebook,
		GOG,
		Google,
		Itchio,
		Nintendo,
		PlayStationNetwork,
		SSO,
		Steam,
		XboxLive
	}

	private const string HEADER_LANGUAGE_RESPONSE = "Accept-Language";

	private const string HEADER_PLATFORM = "X-Modio-Platform";

	private const string HEADER_PORTAL = "X-Modio-Portal";

	private static string _serverURL;

	private static ModioSettings _modioSettings;

	private static Platform _platform = Platform.None;

	private static IModioAPIInterface _apiInterface;

	public static bool IsOffline { get; private set; }

	public static Portal CurrentPortal { get; private set; } = Portal.None;


	public static string LanguageCodeResponse { get; private set; } = "en";


	public static event Action<bool> OnOfflineStatusChanged;

	public static void Init()
	{
		_modioSettings = ModioServices.Resolve<ModioSettings>();
		_serverURL = (string.IsNullOrWhiteSpace(_modioSettings.ServerURL) ? $"https://g-{_modioSettings.GameId}.modapi.io/v1" : _modioSettings.ServerURL);
		ModioLog.Verbose?.Log("Initialized " + Version.GetCurrent());
		ModioLog.Verbose?.Log((_modioSettings.ServerURL == null) ? _serverURL : $"{_modioSettings.GameId}; {_modioSettings.ServerURL}");
		ModioServices.IResolveType<IModioAPIInterface> bindings = ModioServices.GetBindings<IModioAPIInterface>();
		SetAPIInterface(bindings.Resolve());
		bindings.OnNewBinding -= SetAPIInterface;
		bindings.OnNewBinding += SetAPIInterface;
		ModioServices.IResolveType<IModioAuthService> bindings2 = ModioServices.GetBindings<IModioAuthService>();
		SetPortalFromAuthService(bindings2.Resolve());
		bindings2.OnNewBinding -= SetPortalFromAuthService;
		bindings2.OnNewBinding += SetPortalFromAuthService;
	}

	public static void SetResponseLanguage(string languageCode)
	{
		if (string.IsNullOrWhiteSpace(languageCode))
		{
			ModioLog.Message?.Log("ModioAPI response language is invalid (\"" + languageCode + "\"). Use ModioAPI.SetResponseLanguage to set a valid language code. Defaulting to [en]");
			languageCode = "en";
		}
		LanguageCodeResponse = languageCode;
		if (_apiInterface != null)
		{
			_apiInterface.RemoveDefaultHeader("Accept-Language");
			_apiInterface.SetDefaultHeader("Accept-Language", languageCode);
		}
	}

	public static void SetPlatform(Platform platform)
	{
		if (platform == Platform.None)
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				platform = Platform.Windows;
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				platform = Platform.Mac;
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				platform = Platform.Linux;
			}
		}
		_platform = platform;
		if (_apiInterface != null)
		{
			_apiInterface.RemoveDefaultHeader("X-Modio-Platform");
			if (platform.GetHeader() != null)
			{
				_apiInterface.SetDefaultHeader("X-Modio-Platform", platform.GetHeader());
			}
		}
	}

	public static void SetPortal(Portal portal)
	{
		CurrentPortal = portal;
		if (_apiInterface != null)
		{
			_apiInterface.RemoveDefaultHeader("X-Modio-Portal");
			string header = portal.GetHeader();
			if (header != null)
			{
				_apiInterface.SetDefaultHeader("X-Modio-Portal", header);
			}
		}
	}

	private static void SetPortalFromAuthService(IModioAuthService authService)
	{
		SetPortal(authService?.Portal ?? Portal.None);
	}

	public static void SetAPIInterface(IModioAPIInterface apiInterface)
	{
		apiInterface.ResetConfiguration();
		_apiInterface = apiInterface;
		apiInterface.SetDefaultHeader("Accept", "application/json");
		SetResponseLanguage(LanguageCodeResponse);
		apiInterface.SetDefaultHeader("User-Agent", Version.GetCurrent());
		SetPlatform(_platform);
		SetPortal(CurrentPortal);
		_apiInterface.AddDefaultParameter("api_key=" + _modioSettings.APIKey);
		_apiInterface.SetBasePath(_serverURL);
		_apiInterface.AddDefaultPathParameter("game-id", $"{_modioSettings.GameId}");
		ModioLog.Verbose?.Log("ModioAPI.SetAPIInterface(" + _apiInterface.GetType().Name + ")");
	}

	public static void SetOfflineStatus(bool isOffline)
	{
		if (IsOffline != isOffline)
		{
			IsOffline = isOffline;
			ModioAPI.OnOfflineStatusChanged?.Invoke(isOffline);
		}
	}

	private static bool IsInitialized()
	{
		if (_modioSettings.GameId != 0L)
		{
			return true;
		}
		ModioLog.Error?.Log(ErrorCode.API_NOT_INITIALIZED.GetMessage());
		return false;
	}

	public static async Task<bool> Ping()
	{
		return !(await General.Ping()).Item1;
	}

	private static string GetHeader(this Platform platform)
	{
		return platform switch
		{
			Platform.Source => "source", 
			Platform.Windows => "windows", 
			Platform.Mac => "mac", 
			Platform.Linux => "linux", 
			Platform.Android => "android", 
			Platform.IOS => "ios", 
			Platform.XboxOne => "xboxone", 
			Platform.XboxSeriesX => "xboxseriesx", 
			Platform.PlayStation4 => "ps4", 
			Platform.PlayStation5 => "ps5", 
			Platform.Switch => "switch", 
			Platform.Oculus => "oculus", 
			_ => null, 
		};
	}

	private static string GetHeader(this Portal portal)
	{
		return portal switch
		{
			Portal.Apple => "apple", 
			Portal.Discord => "discord", 
			Portal.EpicGamesStore => "epicgames", 
			Portal.Facebook => "facebook", 
			Portal.GOG => "gog", 
			Portal.Google => "google", 
			Portal.Itchio => "itchio", 
			Portal.Nintendo => "nintendo", 
			Portal.PlayStationNetwork => "psn", 
			Portal.SSO => "sso", 
			Portal.Steam => "steam", 
			Portal.XboxLive => "xboxlive", 
			_ => null, 
		};
	}
}

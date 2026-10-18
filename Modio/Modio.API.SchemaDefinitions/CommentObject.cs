using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct CommentObject
{
	internal readonly long Id;

	internal readonly long GameId;

	internal readonly long ModId;

	internal readonly long ResourceId;

	internal readonly UserObject User;

	internal readonly long DateAdded;

	internal readonly long ReplyId;

	internal readonly string ThreadPosition;

	internal readonly long Karma;

	internal readonly long KarmaGuest;

	internal readonly string Content;

	internal readonly long Options;

	[JsonConstructor]
	public CommentObject(long id, long game_id, long mod_id, long resource_id, UserObject user, long date_added, long reply_id, string thread_position, long karma, long karma_guest, string content, long options)
	{
		Id = id;
		GameId = game_id;
		ModId = mod_id;
		ResourceId = resource_id;
		User = user;
		DateAdded = date_added;
		ReplyId = reply_id;
		ThreadPosition = thread_position;
		Karma = karma;
		KarmaGuest = karma_guest;
		Content = content;
		Options = options;
	}
}

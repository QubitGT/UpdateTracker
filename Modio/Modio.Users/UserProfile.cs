using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Modio.API;
using Modio.API.SchemaDefinitions;
using Modio.Caching;
using Modio.Errors;
using Modio.Images;
using Modio.Reports;

namespace Modio.Users;

[Serializable]
public class UserProfile : IEquatable<UserProfile>
{
	public enum AvatarResolution
	{
		X50_Y50,
		X100_Y100,
		Original
	}

	private static Dictionary<long, UserProfile> _cache = new Dictionary<long, UserProfile>();

	public string Username { get; internal set; }

	public long UserId { get; internal set; }

	public string PortalUsername { get; private set; }

	public ModioImageSource<AvatarResolution> Avatar { get; private set; }

	public string Timezone { get; private set; }

	public string Language { get; private set; }

	public event Action OnProfileUpdated;

	public override int GetHashCode()
	{
		return UserId.GetHashCode();
	}

	public Wallet GetWallet()
	{
		if (UserId != User.Current.Profile.UserId)
		{
			return null;
		}
		return User.Current.Wallet;
	}

	internal UserProfile(UserObject userObject)
	{
		ApplyDetailsFromUserObject(userObject);
	}

	internal UserProfile()
	{
	}

	internal void ApplyDetailsFromUserObject(UserObject userObject)
	{
		Username = userObject.Username;
		UserId = userObject.Id;
		PortalUsername = userObject.DisplayNamePortal;
		Timezone = userObject.Timezone;
		Language = userObject.Language;
		Avatar = new ModioImageSource<AvatarResolution>(userObject.Avatar.Filename, userObject.Avatar.Thumb50X50, userObject.Avatar.Thumb100X100, userObject.Avatar.Original);
		_cache[UserId] = this;
		this.OnProfileUpdated?.Invoke();
	}

	internal static UserProfile Get(UserObject user)
	{
		if (!_cache.TryGetValue(user.Id, out var value))
		{
			return new UserProfile(user);
		}
		value.ApplyDetailsFromUserObject(user);
		return value;
	}

	public static bool operator ==(UserProfile left, UserProfile right)
	{
		return object.Equals(left, right);
	}

	public static bool operator !=(UserProfile left, UserProfile right)
	{
		return !object.Equals(left, right);
	}

	public bool Equals(UserProfile other)
	{
		if ((object)other == null)
		{
			return false;
		}
		if ((object)this == other)
		{
			return true;
		}
		return UserId == other.UserId;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((UserProfile)obj);
	}

	public async Task<Error> Mute()
	{
		Error item = (await ModioAPI.Users.MuteAUser(UserId)).Item1;
		if ((bool)item)
		{
			if (!item.IsSilent)
			{
				ModioLog.Error?.Log($"Error muting user {Username}: {item}");
			}
			return item;
		}
		ModCache.ClearModSearchCache();
		return Error.None;
	}

	public async Task<Error> UnMute()
	{
		Error item = (await ModioAPI.Users.UnmuteAUser(UserId)).Item1;
		if ((bool)item)
		{
			if (!item.IsSilent)
			{
				ModioLog.Error?.Log($"Error un-muting user {Username}: {item}");
			}
			return item;
		}
		ModCache.ClearModSearchCache();
		return Error.None;
	}

	public async Task<Error> Report(ReportType reportType, string contact, string summary)
	{
		if (User.Current == null || !User.Current.IsAuthenticated)
		{
			return (Error)ErrorCode.USER_NOT_AUTHENTICATED;
		}
		return (await ModioAPI.Reports.SubmitReport(new AddReportRequest("users", UserId, (long)reportType, 0L, null, User.Current.Profile.Username, contact, summary))).Item1;
	}
}

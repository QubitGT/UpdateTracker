namespace Modio.Errors;

public static class ErrorExtensions
{
	public static string GetMessage(this ApiErrorCode errorCode, string append = null)
	{
		return ((ErrorCode)errorCode).GetMessage(append);
	}

	public static string GetMessage(this ArchiveErrorCode errorCode, string append = null)
	{
		return ((ErrorCode)errorCode).GetMessage(append);
	}

	public static string GetMessage(this ErrorCode errorCode, string append = null)
	{
		string text;
		if (errorCode <= ErrorCode.USER_NO_MOD_RATING)
		{
			if (errorCode <= ErrorCode.OPEN_IDNOT_CONFIGURED)
			{
				if (errorCode <= ErrorCode.APIKEY_FOR_TEST_ONLY)
				{
					ErrorCode num = errorCode - -2147483630;
					if ((ulong)num <= 80uL)
					{
						switch (num)
						{
						case ErrorCode.NONE:
							goto IL_0479;
						case (ErrorCode)1L:
							goto IL_0484;
						case (ErrorCode)2L:
							goto IL_048f;
						case (ErrorCode)3L:
							goto IL_049a;
						case (ErrorCode)4L:
							goto IL_04a5;
						case (ErrorCode)5L:
							goto IL_04b0;
						case (ErrorCode)6L:
							goto IL_04bb;
						case (ErrorCode)7L:
							goto IL_04c6;
						case (ErrorCode)8L:
							goto IL_04d1;
						case (ErrorCode)9L:
							goto IL_04dc;
						case (ErrorCode)10L:
							goto IL_04e7;
						case (ErrorCode)11L:
							goto IL_04f2;
						case (ErrorCode)12L:
							goto IL_04fd;
						case (ErrorCode)13L:
							goto IL_0508;
						case (ErrorCode)14L:
							goto IL_0513;
						case (ErrorCode)15L:
							goto IL_051e;
						case (ErrorCode)16L:
							goto IL_0529;
						case (ErrorCode)17L:
							goto IL_0534;
						case (ErrorCode)18L:
							goto IL_053f;
						case (ErrorCode)19L:
							goto IL_054a;
						case (ErrorCode)20L:
							goto IL_0555;
						case (ErrorCode)21L:
							goto IL_0560;
						case (ErrorCode)22L:
							goto IL_056b;
						case (ErrorCode)23L:
							goto IL_0576;
						case (ErrorCode)24L:
							goto IL_0581;
						case (ErrorCode)25L:
							goto IL_058c;
						case (ErrorCode)26L:
							goto IL_0597;
						case (ErrorCode)27L:
							goto IL_05a2;
						case (ErrorCode)28L:
							goto IL_05ad;
						case (ErrorCode)29L:
							goto IL_05b8;
						case (ErrorCode)30L:
							goto IL_05c3;
						case (ErrorCode)31L:
							goto IL_05ce;
						case (ErrorCode)32L:
							goto IL_05d9;
						case (ErrorCode)33L:
							goto IL_05e4;
						case (ErrorCode)34L:
							goto IL_05ef;
						case (ErrorCode)35L:
							goto IL_05fa;
						case (ErrorCode)36L:
							goto IL_0605;
						case (ErrorCode)37L:
							goto IL_0610;
						case (ErrorCode)38L:
							goto IL_061b;
						case (ErrorCode)39L:
							goto IL_0626;
						case (ErrorCode)40L:
							goto IL_0631;
						case (ErrorCode)41L:
							goto IL_063c;
						case (ErrorCode)42L:
							goto IL_0647;
						case (ErrorCode)43L:
							goto IL_0652;
						case (ErrorCode)44L:
							goto IL_065d;
						case (ErrorCode)45L:
							goto IL_0668;
						case (ErrorCode)46L:
							goto IL_0673;
						case (ErrorCode)47L:
							goto IL_067e;
						case (ErrorCode)48L:
							goto IL_0689;
						case (ErrorCode)49L:
							goto IL_0694;
						case (ErrorCode)50L:
							goto IL_069f;
						case (ErrorCode)51L:
							goto IL_06aa;
						case (ErrorCode)52L:
							goto IL_06b5;
						case (ErrorCode)53L:
							goto IL_06c0;
						case (ErrorCode)54L:
							goto IL_06cb;
						case (ErrorCode)55L:
							goto IL_06d6;
						case (ErrorCode)56L:
							goto IL_06e1;
						case (ErrorCode)57L:
							goto IL_06ec;
						case (ErrorCode)58L:
							goto IL_06f7;
						case (ErrorCode)59L:
							goto IL_0702;
						case (ErrorCode)60L:
							goto IL_070d;
						case (ErrorCode)61L:
							goto IL_0718;
						case (ErrorCode)62L:
							goto IL_0723;
						case (ErrorCode)63L:
							goto IL_072e;
						case (ErrorCode)64L:
							goto IL_0739;
						case (ErrorCode)65L:
							goto IL_0744;
						case (ErrorCode)66L:
							goto IL_074f;
						case (ErrorCode)67L:
							goto IL_075a;
						case (ErrorCode)68L:
							goto IL_0765;
						case (ErrorCode)69L:
							goto IL_0770;
						case (ErrorCode)70L:
							goto IL_077b;
						case (ErrorCode)71L:
							goto IL_0786;
						case (ErrorCode)72L:
							goto IL_0791;
						case (ErrorCode)73L:
							goto IL_079c;
						case (ErrorCode)74L:
							goto IL_07a7;
						case (ErrorCode)75L:
							goto IL_07b2;
						case (ErrorCode)76L:
							goto IL_07bd;
						case (ErrorCode)77L:
							goto IL_07c8;
						case (ErrorCode)78L:
							goto IL_07d3;
						case (ErrorCode)79L:
							goto IL_07de;
						case (ErrorCode)80L:
							goto IL_07e9;
						}
					}
					ErrorCode num2 = errorCode - 10000;
					if ((ulong)num2 <= 3uL)
					{
						switch (num2)
						{
						case ErrorCode.NONE:
							goto IL_07f4;
						case (ErrorCode)1L:
							goto IL_07ff;
						case (ErrorCode)2L:
							goto IL_080a;
						case (ErrorCode)3L:
							goto IL_0815;
						}
					}
					ErrorCode num3 = errorCode - 11000;
					if ((ulong)num3 > 17uL)
					{
						goto IL_0a9a;
					}
					switch (num3)
					{
					case ErrorCode.NONE:
						break;
					case (ErrorCode)1L:
						goto IL_082b;
					case (ErrorCode)2L:
						goto IL_0836;
					case (ErrorCode)3L:
						goto IL_0841;
					case (ErrorCode)4L:
						goto IL_084c;
					case (ErrorCode)5L:
						goto IL_0857;
					case (ErrorCode)6L:
						goto IL_0862;
					case (ErrorCode)7L:
						goto IL_086d;
					case (ErrorCode)8L:
						goto IL_0878;
					case (ErrorCode)9L:
						goto IL_0883;
					case (ErrorCode)12L:
						goto IL_088e;
					case (ErrorCode)14L:
						goto IL_0899;
					case (ErrorCode)16L:
						goto IL_08a4;
					case (ErrorCode)17L:
						goto IL_08af;
					default:
						goto IL_0a9a;
					}
					text = "api_key is missing from your request.";
				}
				else if (errorCode != ErrorCode.CANNOT_VERIFY_EXTERNAL_CREDENTIALS)
				{
					if (errorCode != ErrorCode.USER_NO_ACCEPT_TERMS_OF_USE)
					{
						if (errorCode != ErrorCode.OPEN_IDNOT_CONFIGURED)
						{
							goto IL_0a9a;
						}
						text = "You must configure your OpenID config for your game in your game authentication settings before being able to authenticate users.";
					}
					else
					{
						text = "The user has not agreed to the mod.io Terms of Use. Please see terms_agreed parameter description and the Terms endpoint for more information.";
					}
				}
				else
				{
					text = "mod.io was unable to verify the credentials against the external service provider.";
				}
			}
			else if (errorCode <= ErrorCode.REQUESTED_GAME_NOT_FOUND)
			{
				ErrorCode num4 = errorCode - 13001;
				if ((ulong)num4 <= 8uL)
				{
					switch (num4)
					{
					case ErrorCode.NONE:
						goto IL_08db;
					case (ErrorCode)1L:
						goto IL_08e6;
					case (ErrorCode)3L:
						goto IL_08f1;
					case (ErrorCode)4L:
						goto IL_08fc;
					case (ErrorCode)5L:
						goto IL_0907;
					case (ErrorCode)6L:
						goto IL_0912;
					case (ErrorCode)8L:
						goto IL_091d;
					case (ErrorCode)2L:
					case (ErrorCode)7L:
						goto IL_0a9a;
					}
				}
				if (errorCode != ErrorCode.REQUESTED_RESOURCE_NOT_FOUND)
				{
					if (errorCode != ErrorCode.REQUESTED_GAME_NOT_FOUND)
					{
						goto IL_0a9a;
					}
					text = "The requested game could not be found.";
				}
				else
				{
					text = "The requested resource does not exist.";
				}
			}
			else if (errorCode <= ErrorCode.FORBIDDEN_TACNOT_ACCEPTED)
			{
				if (errorCode != ErrorCode.REQUESTED_GAME_DELETED)
				{
					ErrorCode num5 = errorCode - 15000;
					if ((ulong)num5 > 11uL)
					{
						goto IL_0a9a;
					}
					switch (num5)
					{
					case ErrorCode.NONE:
						break;
					case (ErrorCode)1L:
						goto IL_0954;
					case (ErrorCode)4L:
						goto IL_095f;
					case (ErrorCode)5L:
						goto IL_096a;
					case (ErrorCode)6L:
						goto IL_0975;
					case (ErrorCode)10L:
						goto IL_0980;
					case (ErrorCode)11L:
						goto IL_098b;
					default:
						goto IL_0a9a;
					}
					text = "This mod is currently under DMCA and the user cannot be subscribed to it.";
				}
				else
				{
					text = "The requested game has been deleted.";
				}
			}
			else
			{
				ErrorCode num6 = errorCode - 15019;
				if ((ulong)num6 <= 17uL)
				{
					switch (num6)
					{
					case ErrorCode.NONE:
						goto IL_0996;
					case (ErrorCode)1L:
						goto IL_09a1;
					case (ErrorCode)3L:
						goto IL_09ac;
					case (ErrorCode)4L:
						goto IL_09b7;
					case (ErrorCode)7L:
						goto IL_09c2;
					case (ErrorCode)9L:
						goto IL_09cd;
					case (ErrorCode)10L:
						goto IL_09d8;
					case (ErrorCode)11L:
						goto IL_09e3;
					case (ErrorCode)16L:
						goto IL_09ee;
					case (ErrorCode)17L:
						goto IL_09f9;
					case (ErrorCode)2L:
					case (ErrorCode)5L:
					case (ErrorCode)6L:
					case (ErrorCode)8L:
					case (ErrorCode)12L:
					case (ErrorCode)13L:
					case (ErrorCode)14L:
					case (ErrorCode)15L:
						goto IL_0a9a;
					}
				}
				if (errorCode != ErrorCode.USER_NO_MOD_RATING)
				{
					goto IL_0a9a;
				}
				text = "The authenticated user cannot clear the mod rating as none exists.";
			}
		}
		else if (errorCode <= ErrorCode.MONETIZATION_WALLET_FETCH_FAILED)
		{
			if (errorCode <= ErrorCode.CANNOT_MUTE_YOURSELF)
			{
				if (errorCode != ErrorCode.MATURE_MODS_NOT_ALLOWED)
				{
					if (errorCode != ErrorCode.MUTE_USER_NOT_FOUND)
					{
						if (errorCode != ErrorCode.CANNOT_MUTE_YOURSELF)
						{
							goto IL_0a9a;
						}
						text = "You cannot mute yourself.";
					}
					else
					{
						text = "The user with the supplied UserID could not be found.";
					}
				}
				else
				{
					text = "This game does not allow mature mods.";
				}
			}
			else if (errorCode != ErrorCode.INSUFFICIENT_SPACE)
			{
				if (errorCode != ErrorCode.REQUESTED_USER_NOT_FOUND)
				{
					ErrorCode num7 = errorCode - 900000;
					if ((ulong)num7 > 8uL)
					{
						goto IL_0a9a;
					}
					switch (num7)
					{
					case ErrorCode.NONE:
						break;
					case (ErrorCode)1L:
						goto IL_0a42;
					case (ErrorCode)2L:
						goto IL_0a4a;
					case (ErrorCode)7L:
						goto IL_0a52;
					case (ErrorCode)8L:
						goto IL_0a5a;
					default:
						goto IL_0a9a;
					}
					text = "An un expected error during a purchase transaction has occured. Please try again later.";
				}
				else
				{
					text = "The requested user could not be found.";
				}
			}
			else
			{
				text = "Insufficient space for file";
			}
		}
		else if (errorCode <= ErrorCode.MONETIZATION_GAME_MONETIZATION_NOT_ENABLED)
		{
			if (errorCode != ErrorCode.MONETIZATION_IN_MAINTENANCE)
			{
				if (errorCode != ErrorCode.USER_MONETIZATION_DISABLED)
				{
					if (errorCode != ErrorCode.MONETIZATION_GAME_MONETIZATION_NOT_ENABLED)
					{
						goto IL_0a9a;
					}
					text = "The game does not have active monetization.";
				}
				else
				{
					text = "The account does not have monetization enabled.";
				}
			}
			else
			{
				text = "The monetization is currently in maintance mode. Please try again later.";
			}
		}
		else if (errorCode <= ErrorCode.MONETIZATION_ITEM_ALREADY_OWNED)
		{
			if (errorCode != ErrorCode.MONETIZATION_PAYMENT_FAILED)
			{
				if (errorCode != ErrorCode.MONETIZATION_ITEM_ALREADY_OWNED)
				{
					goto IL_0a9a;
				}
				text = "The account already owns this item.";
			}
			else
			{
				text = "The payment transaction failed. Please try again later.";
			}
		}
		else if (errorCode != ErrorCode.MONETIZATION_INCORRECT_DISPLAY_PRICE)
		{
			if (errorCode != ErrorCode.MONETIZATION_INSUFFICIENT_FUNDS)
			{
				goto IL_0a9a;
			}
			text = "The account has insufficent funds to make this purchase.";
		}
		else
		{
			text = "The given display price does not match the price of the mod.";
		}
		goto IL_0a9c;
		IL_0652:
		text = "mod.io SDK could not find required components";
		goto IL_0a9c;
		IL_07bd:
		text = "The account already owns this item.";
		goto IL_0a9c;
		IL_0836:
		text = "api_key supplied is invalid.";
		goto IL_0a9c;
		IL_0883:
		text = "You have been ratelimited from calling this endpoint again, for making too many requests. See Rate Limiting.";
		goto IL_0a9c;
		IL_0a9a:
		text = null;
		goto IL_0a9c;
		IL_08af:
		text = "The api_key supplied in the request is for test environment purposes only and cannot be used for this functionality.";
		goto IL_0a9c;
		IL_0a9c:
		string text2 = text;
		return string.Format("{0}{1}{2}", errorCode, string.IsNullOrWhiteSpace(text2) ? string.Empty : (": " + text2), string.IsNullOrWhiteSpace(append) ? string.Empty : (": " + append));
		IL_0878:
		text = "You have been ratelimited for making too many requests. See Rate Limiting.";
		goto IL_0a9c;
		IL_0899:
		text = "Email login code is invalid";
		goto IL_0a9c;
		IL_088e:
		text = "Email login code has expired, please request a new one.";
		goto IL_0a9c;
		IL_08a4:
		text = "The api_key supplied in the request must be associated with a game.";
		goto IL_0a9c;
		IL_082b:
		text = "api_key supplied is malformed.";
		goto IL_0a9c;
		IL_084c:
		text = "Access token is missing the read scope to perform the request.";
		goto IL_0a9c;
		IL_0841:
		text = "Access token is missing the write scope to perform the request.";
		goto IL_0a9c;
		IL_0862:
		text = "Authenticated user account has been deleted.";
		goto IL_0a9c;
		IL_0857:
		text = "Access token is expired, or has been revoked.";
		goto IL_0a9c;
		IL_086d:
		text = "Authenticated user account has been banned by mod.io admins.";
		goto IL_0a9c;
		IL_07b2:
		text = "The given display price does not match the price of the mod.";
		goto IL_0a9c;
		IL_07d3:
		text = "Some entitlements could not be verified. Please try again.";
		goto IL_0a9c;
		IL_080a:
		text = "mod.io failed to complete the request, please try again. (rare)";
		goto IL_0a9c;
		IL_07ff:
		text = "Cross-origin request forbidden.";
		goto IL_0a9c;
		IL_069f:
		text = "Too many symbols";
		goto IL_0a9c;
		IL_0815:
		text = "API version supplied is invalid.";
		goto IL_0a9c;
		IL_06b5:
		text = "Invalid bit length repeat";
		goto IL_0a9c;
		IL_06c0:
		text = "Missing end-of-block marker";
		goto IL_0a9c;
		IL_0a42:
		text = "Unable to communicate with the monetization system. Please try again later.";
		goto IL_0a9c;
		IL_0a4a:
		text = "A failure has occured when trying to authenticate with the monetization system.";
		goto IL_0a9c;
		IL_0a52:
		text = "The account has not been created with monetization.";
		goto IL_0a9c;
		IL_0a5a:
		text = "Unable to fetch the accounts' wallet. Please confirm the account has one";
		goto IL_0a9c;
		IL_07c8:
		text = "The account has insufficent funds to make this purchase.";
		goto IL_0a9c;
		IL_07e9:
		text = "A mod installation has previously failed, can't install all needed mods for this temporary mod session.";
		goto IL_0a9c;
		IL_07de:
		text = "The configured Metrics Secret Key is invalid.";
		goto IL_0a9c;
		IL_06aa:
		text = "Invalid code lengths";
		goto IL_0a9c;
		IL_0689:
		text = "Invalid block type";
		goto IL_0a9c;
		IL_07f4:
		text = "mod.io is currently experiencing an outage. (rare)";
		goto IL_0a9c;
		IL_0647:
		text = "mod.io SDK is shutting down, operation is cancelled";
		goto IL_0a9c;
		IL_0668:
		text = "Need more input data";
		goto IL_0a9c;
		IL_070d:
		text = "The current mod installation or update was cancelled";
		goto IL_0a9c;
		IL_075a:
		text = "Mod directory does not contain any files";
		goto IL_0a9c;
		IL_079c:
		text = "The game does not have active monetization.";
		goto IL_0a9c;
		IL_0996:
		text = "The authenticated user does not have permission to delete this mod. This action is restricted to team managers and administrators only.";
		goto IL_0a9c;
		IL_09a1:
		text = "This mod is missing a file and cannot be subscribed to.";
		goto IL_0a9c;
		IL_09ac:
		text = "The requested mod could not be found.";
		goto IL_0a9c;
		IL_09b7:
		text = "The requested mod has been deleted.";
		goto IL_0a9c;
		IL_09c2:
		text = "The requested comment could not be found.";
		goto IL_0a9c;
		IL_09cd:
		text = "The authenticated user has already submitted a rating for this mod.";
		goto IL_0a9c;
		IL_09d8:
		text = "The authenticated user does not have permission to submit reports on mod.io due to their access being revoked.";
		goto IL_0a9c;
		IL_09e3:
		text = "The specified resource is not able to be reported at this time, this is potentially due to the resource in question being removed.";
		goto IL_0a9c;
		IL_09ee:
		text = "The authenticated user does not have permission to modify this resource.";
		goto IL_0a9c;
		IL_09f9:
		text = "The authenticated user does not have permission to modify this resource.";
		goto IL_0a9c;
		IL_0791:
		text = "Unable to fetch the account's wallet. Please confirm the account has one";
		goto IL_0a9c;
		IL_07a7:
		text = "The payment transaction failed. Please try again later.";
		goto IL_0a9c;
		IL_074f:
		text = "The dependencies for this mod are incompatible with your version of the game. Please contact the mod creator for support.";
		goto IL_0a9c;
		IL_0770:
		text = "Mod MD5 does not match";
		goto IL_0a9c;
		IL_0765:
		text = "Mod directory does not exist";
		goto IL_0a9c;
		IL_077b:
		text = "The display price for the mod was out-of-date or incorrect. Please retry with the correct display price.";
		goto IL_0a9c;
		IL_0786:
		text = "A failure has occured when trying to authenticate with the monetization system.";
		goto IL_0a9c;
		IL_0954:
		text = "This mod is hidden and the user cannot be subscribed to it.";
		goto IL_0a9c;
		IL_095f:
		text = "The user is already subscribed to the specified mod";
		goto IL_0a9c;
		IL_096a:
		text = "The authenticated user is not subscribed to the mod.";
		goto IL_0a9c;
		IL_0975:
		text = "The authenticated user does not have permission to upload modfiles for the specified mod. Ensure the user is a team manager or administrator.";
		goto IL_0a9c;
		IL_0980:
		text = "The requested modfile could not be found.";
		goto IL_0a9c;
		IL_098b:
		text = "The item has not been accepted and can not be purchased at this time.";
		goto IL_0a9c;
		IL_0702:
		text = "Internal: No mods require processing for this iteration";
		goto IL_0a9c;
		IL_0723:
		text = "Mod management was already enabled and the callback remains unchanged.";
		goto IL_0a9c;
		IL_08db:
		text = "The submitted binary file is corrupted.";
		goto IL_0a9c;
		IL_08e6:
		text = "The submitted binary file is unreadable.";
		goto IL_0a9c;
		IL_08f1:
		text = "You have used the input_json parameter with semantically incorrect JSON.";
		goto IL_0a9c;
		IL_08fc:
		text = "The Content-Type header is missing from your request.";
		goto IL_0a9c;
		IL_0907:
		text = "The Content-Type header is not supported for this endpoint.";
		goto IL_0a9c;
		IL_0912:
		text = "You have requested a response format that is not supported (JSON only).";
		goto IL_0a9c;
		IL_091d:
		text = "The request contains validation errors for the data supplied. See the attached errors field within the Error Object to determine which input failed.";
		goto IL_0a9c;
		IL_0718:
		text = "Could not perform operation: Mod management is disabled and mod collection is locked";
		goto IL_0a9c;
		IL_0739:
		text = "The specified mod's files are currently being updated by the SDK. Please try again later.";
		goto IL_0a9c;
		IL_072e:
		text = "The current modfile upload was cancelled";
		goto IL_0a9c;
		IL_0744:
		text = "Temporary mod set was not initialized. Please call InitTempModSet.";
		goto IL_0a9c;
		IL_065d:
		text = "A low-level system error occured, refer to the logs for code and location";
		goto IL_0a9c;
		IL_067e:
		text = "Stream error";
		goto IL_0a9c;
		IL_06d6:
		text = "Invalid distance code";
		goto IL_0a9c;
		IL_06cb:
		text = "Invalid literal length";
		goto IL_0a9c;
		IL_06ec:
		text = "Over-subscribed length";
		goto IL_0a9c;
		IL_06e1:
		text = "Invalid distance";
		goto IL_0a9c;
		IL_06f7:
		text = "Incomplete length set";
		goto IL_0a9c;
		IL_0673:
		text = "End of deflate stream";
		goto IL_0a9c;
		IL_0694:
		text = "Invalid store block length";
		goto IL_0a9c;
		IL_0479:
		text = "HTTP service not initialized";
		goto IL_0a9c;
		IL_0484:
		text = "HTTP service already initialized";
		goto IL_0a9c;
		IL_048f:
		text = "Unable to connect to server";
		goto IL_0a9c;
		IL_049a:
		text = "Insufficient permissions";
		goto IL_0a9c;
		IL_04a5:
		text = "Invalid platform HTTP security configuration";
		goto IL_0a9c;
		IL_04b0:
		text = "Unable to connect to server";
		goto IL_0a9c;
		IL_04bb:
		text = "Invalid endpoint path";
		goto IL_0a9c;
		IL_04c6:
		text = "Exceeded the allowed number of redirects";
		goto IL_0a9c;
		IL_04d1:
		text = "Server closed connection unexpectedly";
		goto IL_0a9c;
		IL_04dc:
		text = "Trying to download file from outside of mod.io domain";
		goto IL_0a9c;
		IL_04e7:
		text = "The mod.io servers are overloaded. Please wait a bit before trying again";
		goto IL_0a9c;
		IL_04f2:
		text = "An error occurred making a HTTP request";
		goto IL_0a9c;
		IL_04fd:
		text = "The HTTP response was malformed or not in the expected format";
		goto IL_0a9c;
		IL_0508:
		text = "Too many requests made to the mod.io API within the rate-limiting window. Please wait and try again";
		goto IL_0a9c;
		IL_0513:
		text = "Could not create folder";
		goto IL_0a9c;
		IL_051e:
		text = "Could not create file";
		goto IL_0a9c;
		IL_0529:
		text = "Insufficient permission for filesystem operation";
		goto IL_0a9c;
		IL_0534:
		text = "File locked (already in use?)";
		goto IL_0a9c;
		IL_053f:
		text = "File not found";
		goto IL_0a9c;
		IL_054a:
		text = "Directory not empty";
		goto IL_0a9c;
		IL_0555:
		text = "Error reading file";
		goto IL_0a9c;
		IL_0560:
		text = "Error writing file";
		goto IL_0a9c;
		IL_056b:
		text = "Directory not found";
		goto IL_0a9c;
		IL_0576:
		text = "Could not initialize user storage";
		goto IL_0a9c;
		IL_0581:
		text = "OAuth token was missing";
		goto IL_0a9c;
		IL_058c:
		text = "The user's OAuth token was invalid";
		goto IL_0a9c;
		IL_0597:
		text = "No Auth token available";
		goto IL_0a9c;
		IL_05a2:
		text = "User is already authenticated. To use a new user and OAuth token, call ClearUserDataAsync";
		goto IL_0a9c;
		IL_05ad:
		text = "Invalid user";
		goto IL_0a9c;
		IL_05b8:
		text = "Some or all of the user data was missing from storage";
		goto IL_0a9c;
		IL_05c3:
		text = "File did not have a valid archive header";
		goto IL_0a9c;
		IL_05ce:
		text = "File uses an unsupported compression method. Please use STORE or DEFLATE";
		goto IL_0a9c;
		IL_05d9:
		text = "The asynchronous operation was cancelled before it completed";
		goto IL_0a9c;
		IL_05e4:
		text = "The asynchronous operation produced an error before it completed";
		goto IL_0a9c;
		IL_05ef:
		text = "Operating system could not create the requested handle";
		goto IL_0a9c;
		IL_05fa:
		text = "No data available";
		goto IL_0a9c;
		IL_0605:
		text = "End of file";
		goto IL_0a9c;
		IL_0610:
		text = "Operation could not be started as the service queue was missing or destroyed";
		goto IL_0a9c;
		IL_061b:
		text = "mod.io SDK was already initialized";
		goto IL_0a9c;
		IL_0626:
		text = "mod.io SDK was not initialized";
		goto IL_0a9c;
		IL_0631:
		text = "Index out of range";
		goto IL_0a9c;
		IL_063c:
		text = "Bad parameter supplied";
		goto IL_0a9c;
	}

	public static string GetMessage(this FilesystemErrorCode errorCode, string append = null)
	{
		return ((ErrorCode)errorCode).GetMessage(append);
	}

	public static string GetMessage(this GenericErrorCode errorCode, string append = null)
	{
		return ((ErrorCode)errorCode).GetMessage(append);
	}

	public static string GetMessage(this HttpErrorCode errorCode, string append = null)
	{
		return ((ErrorCode)errorCode).GetMessage(append);
	}

	public static string GetMessage(this MetricsErrorCode errorCode, string append = null)
	{
		return ((ErrorCode)errorCode).GetMessage(append);
	}

	public static string GetMessage(this ModManagementErrorCode errorCode, string append = null)
	{
		return ((ErrorCode)errorCode).GetMessage(append);
	}

	public static string GetMessage(this ModValidationErrorCode errorCode, string append = null)
	{
		return ((ErrorCode)errorCode).GetMessage(append);
	}

	public static string GetMessage(this MonetizationErrorCode errorCode, string append = null)
	{
		return ((ErrorCode)errorCode).GetMessage(append);
	}

	public static string GetMessage(this SystemErrorCode errorCode, string append = null)
	{
		return ((ErrorCode)errorCode).GetMessage(append);
	}

	public static string GetMessage(this TempModsErrorCode errorCode, string append = null)
	{
		return ((ErrorCode)errorCode).GetMessage(append);
	}

	public static string GetMessage(this UserAuthErrorCode errorCode, string append = null)
	{
		return ((ErrorCode)errorCode).GetMessage(append);
	}

	public static string GetMessage(this UserDataErrorCode errorCode, string append = null)
	{
		return ((ErrorCode)errorCode).GetMessage(append);
	}

	public static string GetMessage(this ZlibErrorCode errorCode, string append = null)
	{
		return ((ErrorCode)errorCode).GetMessage(append);
	}
}

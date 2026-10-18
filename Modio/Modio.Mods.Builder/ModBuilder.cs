using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using Modio.API;
using Modio.API.SchemaDefinitions;
using Modio.Caching;
using Modio.Errors;

namespace Modio.Mods.Builder;

public class ModBuilder
{
	[Flags]
	private enum MonetizationOptions
	{
		None = 0,
		Enabled = 1,
		Live = 2,
		LimitedStock = 8
	}

	private Dictionary<ChangeFlags, Error> _commitErrors;

	private ChangeFlags _pendingChanges;

	private byte[] _logoBytes;

	private ImageFormat _logoBytesFormat;

	private bool _appendingGallery;

	private ModfileBuilder _modfileBuilder;

	private bool _appendingDependencies;

	private MonetizationOptions _monetizationOptions;

	public List<(ChangeFlags, Error)> Results => _commitErrors?.Select((KeyValuePair<ChangeFlags, Error> yeet) => (Key: yeet.Key, Value: yeet.Value)).ToList();

	public string Name { get; private set; }

	public string Summary { get; private set; }

	public string Description { get; private set; }

	public string LogoFilePath { get; private set; }

	public string[] GalleryFilePaths { get; private set; }

	public string[] Tags { get; private set; }

	public string MetadataBlob { get; private set; }

	public Dictionary<string, string> MetadataKvps { get; private set; }

	public List<long> Dependencies { get; private set; } = new List<long>();


	public bool Visible { get; private set; }

	public ModMaturityOptions MaturityOptions { get; private set; }

	public ModCommunityOptions CommunityOptions { get; private set; }

	public bool IsMonetized { get; private set; }

	public bool IsLimitedStock { get; private set; }

	public int Price { get; private set; }

	public int Stock { get; private set; }

	public bool IsEditMode => EditTarget != null;

	public Mod EditTarget { get; private set; }

	internal ModBuilder()
	{
		EditTarget = null;
	}

	internal ModBuilder(Mod editTarget)
	{
		EditTarget = editTarget;
		Name = editTarget.Name;
		Summary = editTarget.Summary;
		Description = editTarget.Description;
		Tags = editTarget.Tags.Select((ModTag tag) => tag.ApiName).ToArray();
		MetadataBlob = editTarget.MetadataBlob;
		MetadataKvps = editTarget.MetadataKvps;
		_monetizationOptions = (editTarget.IsMonetized ? (MonetizationOptions.Enabled | MonetizationOptions.Live) : MonetizationOptions.None);
		Price = (int)(editTarget.IsMonetized ? editTarget.Price : 0);
	}

	public ModBuilder SetName(string name)
	{
		Name = name;
		_pendingChanges |= ChangeFlags.Name;
		return this;
	}

	public ModBuilder SetSummary(string summary)
	{
		Summary = summary;
		_pendingChanges |= ChangeFlags.Summary;
		return this;
	}

	public ModBuilder SetDescription(string description)
	{
		Description = description;
		_pendingChanges |= ChangeFlags.Description;
		return this;
	}

	public ModBuilder SetTags(ICollection<string> tags)
	{
		Tags = tags.ToArray();
		_pendingChanges |= ChangeFlags.Tags;
		return this;
	}

	public ModBuilder SetTags(string tag)
	{
		return SetTags(new string[1] { tag });
	}

	public ModBuilder AppendTags(ICollection<string> tags)
	{
		Tags = Tags.Concat(tags).ToArray();
		_pendingChanges |= ChangeFlags.Tags;
		return this;
	}

	public ModBuilder AppendTags(string tag)
	{
		return AppendTags(new string[1] { tag });
	}

	public ModBuilder SetMetadataBlob(string data)
	{
		MetadataBlob = data;
		_pendingChanges |= ChangeFlags.MetadataBlob;
		return this;
	}

	public ModBuilder AppendMetadataBlob(string data)
	{
		MetadataBlob += data;
		_pendingChanges |= ChangeFlags.MetadataBlob;
		return this;
	}

	public ModBuilder SetMetadataKvps(Dictionary<string, string> kvps)
	{
		foreach (var (key, value) in kvps)
		{
			MetadataKvps[key] = value;
		}
		_pendingChanges |= ChangeFlags.MetadataKvps;
		return this;
	}

	public ModBuilder SetLogo(string logoFilePath)
	{
		LogoFilePath = logoFilePath;
		_pendingChanges |= ChangeFlags.Logo;
		return this;
	}

	public ModBuilder SetLogo(byte[] imageData, ImageFormat format)
	{
		_logoBytes = imageData;
		_logoBytesFormat = format;
		_pendingChanges |= ChangeFlags.Logo;
		return this;
	}

	public ModBuilder SetGallery(ICollection<string> galleryImageFilePaths)
	{
		GalleryFilePaths = galleryImageFilePaths.ToArray();
		_appendingGallery = false;
		_pendingChanges |= ChangeFlags.Gallery;
		return this;
	}

	public ModBuilder SetGallery(string galleryImageFilePath)
	{
		return SetGallery(new string[1] { galleryImageFilePath });
	}

	public ModBuilder AppendGallery(ICollection<string> galleryImageFilePaths)
	{
		GalleryFilePaths = GalleryFilePaths.Concat(galleryImageFilePaths).ToArray();
		_appendingGallery = true;
		_pendingChanges |= ChangeFlags.Gallery;
		return this;
	}

	public ModBuilder AppendGallery(string galleryImageFilePath)
	{
		return AppendGallery(new string[1] { galleryImageFilePath });
	}

	public ModBuilder SetDependencies(ICollection<long> dependencies)
	{
		Dependencies = dependencies.ToList();
		_appendingDependencies = false;
		_pendingChanges |= ChangeFlags.Dependencies;
		return this;
	}

	public ModBuilder SetDependencies(long dependency)
	{
		return SetDependencies(new long[1] { dependency });
	}

	public ModBuilder AppendDependencies(ICollection<long> dependencies)
	{
		Dependencies = Dependencies.Concat(dependencies).ToList();
		_appendingDependencies = true;
		_pendingChanges |= ChangeFlags.Dependencies;
		return this;
	}

	public ModBuilder AppendDependencies(long dependency)
	{
		return AppendDependencies(new long[1] { dependency });
	}

	public ModfileBuilder EditModfile()
	{
		if (_modfileBuilder == null)
		{
			_modfileBuilder = new ModfileBuilder(this);
		}
		_pendingChanges |= ChangeFlags.Modfile;
		return _modfileBuilder;
	}

	public ModBuilder SetVisible(bool isVisible)
	{
		Visible = isVisible;
		_pendingChanges |= ChangeFlags.Visibility;
		return this;
	}

	public ModBuilder SetMaturityOptions(ModMaturityOptions maturityOptions)
	{
		MaturityOptions |= maturityOptions;
		_pendingChanges |= ChangeFlags.MaturityOptions;
		return this;
	}

	public ModBuilder OverwriteMaturityOptions(ModMaturityOptions maturityOptions)
	{
		MaturityOptions = maturityOptions;
		_pendingChanges |= ChangeFlags.MaturityOptions;
		return this;
	}

	public ModBuilder SetCommunityOptions(ModCommunityOptions communityOptions)
	{
		CommunityOptions |= communityOptions;
		_pendingChanges |= ChangeFlags.CommunityOptions;
		return this;
	}

	public ModBuilder OverwriteCommunityOptions(ModCommunityOptions communityOptions)
	{
		CommunityOptions = communityOptions;
		_pendingChanges |= ChangeFlags.CommunityOptions;
		return this;
	}

	public ModBuilder SetMonetized(bool isMonetized)
	{
		if (isMonetized)
		{
			_monetizationOptions |= MonetizationOptions.Enabled | MonetizationOptions.Live;
		}
		else
		{
			_monetizationOptions &= ~(MonetizationOptions.Enabled | MonetizationOptions.Live);
		}
		IsMonetized = isMonetized;
		_pendingChanges |= ChangeFlags.MonetizationConfig;
		return this;
	}

	public ModBuilder SetPrice(int price)
	{
		if (!_monetizationOptions.HasFlag(MonetizationOptions.Enabled | MonetizationOptions.Live))
		{
			ModioLog.Error?.Log("Mod is not set for Monetization! Use SetMonetized(bool isMonetized) before setting a price.");
			return this;
		}
		Price = price;
		_pendingChanges |= ChangeFlags.MonetizationConfig;
		return this;
	}

	public ModBuilder SetLimitedStock(bool isLimitedStock)
	{
		if (isLimitedStock)
		{
			_monetizationOptions |= MonetizationOptions.LimitedStock;
		}
		else
		{
			_monetizationOptions &= MonetizationOptions.LimitedStock;
		}
		IsLimitedStock = isLimitedStock;
		_pendingChanges |= ChangeFlags.MonetizationConfig;
		return this;
	}

	public ModBuilder SetStockAmount(int stockAmount)
	{
		if (!_monetizationOptions.HasFlag(MonetizationOptions.Enabled | MonetizationOptions.Live | MonetizationOptions.LimitedStock))
		{
			ModioLog.Error?.Log("Mod is not set for Monetization or Limited Stock! Use SetMonetized(bool isMonetized) & SetLimtedStock(bool isLimitedStock) before setting a stock value.");
			return this;
		}
		Stock = stockAmount;
		_pendingChanges |= ChangeFlags.MonetizationConfig;
		return this;
	}

	public async Task<(Error error, Mod mod)> Publish()
	{
		_commitErrors = new Dictionary<ChangeFlags, Error>();
		return (!IsEditMode) ? (await PublishNewMod()) : (await PublishEdits());
	}

	private async Task<(Error error, Mod mod)> PublishNewMod()
	{
		if (!_pendingChanges.HasFlag(ChangeFlags.Name | ChangeFlags.Summary | ChangeFlags.Logo))
		{
			ModioLog.Error?.Log("Can't publish mod [" + Name + "], mod must have the Name, Summary & Logo all filled before it can be added.");
			return (error: new Error(ErrorCode.BAD_PARAMETER), mod: null);
		}
		string name_id = Name.ToLowerInvariant().Replace(' ', '-');
		ModioAPIFileParameter logo;
		Error error;
		(error, logo) = TryGetLogoFileParameter();
		if ((bool)error)
		{
			ModioLog.Error?.Log($"Error getting File parameter from Logo: {error}");
			return (error: error, mod: null);
		}
		ModObject? modObject;
		(error, modObject) = await ModioAPI.Mods.AddMod(new AddModRequest(Name, name_id, Summary, Description, logo, _pendingChanges.HasFlag(ChangeFlags.Visibility) ? new long?((!Visible) ? 1 : 0) : null, _pendingChanges.HasFlag(ChangeFlags.MaturityOptions) ? new long?((long)MaturityOptions) : null, _pendingChanges.HasFlag(ChangeFlags.CommunityOptions) ? new long?((long)CommunityOptions) : null, MetadataBlob, Tags));
		_commitErrors[ChangeFlags.AddFlags] = error;
		if ((bool)error || !modObject.HasValue)
		{
			if (!error.IsSilent)
			{
				ModioLog.Error?.Log($"Error publishing new mod {Name}: {error}");
			}
			return (error: error, mod: null);
		}
		_pendingChanges &= ~ChangeFlags.AddFlags;
		Mod mod = (EditTarget = ModCache.GetMod(modObject.Value));
		await PublishRemainingChanges();
		await mod.GetModDetailsFromServer();
		return (error: Error.None, mod: mod);
	}

	private (Error error, ModioAPIFileParameter logo) TryGetLogoFileParameter()
	{
		ModioAPIFileParameter item = ModioAPIFileParameter.None;
		Error error = Error.None;
		if (!string.IsNullOrEmpty(LogoFilePath))
		{
			(error, item) = LogoFromFilePath(LogoFilePath);
			if ((bool)error)
			{
				ModioLog.Error?.Log("Couldn't create Logo file from file path " + LogoFilePath + ", cannot publish edits");
			}
		}
		else if (_logoBytes.Length != 0)
		{
			item = LogoFromByteArray();
		}
		else
		{
			ModioLog.Error?.Log("Couldn't create Logo file from either source! Cannot publish edits");
			error = new Error(ErrorCode.BAD_PARAMETER);
		}
		return (error: error, logo: item);
	}

	private async Task PublishRemainingChanges()
	{
		List<ChangeFlags> list = (from ChangeFlags flag in Enum.GetValues(typeof(ChangeFlags))
			where _pendingChanges.HasFlag(flag)
			where flag != ChangeFlags.None
			select flag).ToList();
		foreach (ChangeFlags change in list)
		{
			Error value = await GetChangeSpecificPublishTask(change);
			_commitErrors[change] = value;
		}
	}

	private async Task<(Error error, Mod mod)> PublishEdits()
	{
		if (_pendingChanges == ChangeFlags.None)
		{
			ModioLog.Error?.Log("Can't publish changes for mod " + EditTarget.Name + ", no changes pending.");
			return (error: new Error(ErrorCode.BAD_PARAMETER), mod: null);
		}
		string text = Name?.ToLowerInvariant().Replace(' ', '-');
		ModioAPIFileParameter value = ModioAPIFileParameter.None;
		Error error;
		if (_pendingChanges.HasFlag(ChangeFlags.Logo))
		{
			(error, value) = TryGetLogoFileParameter();
			if ((bool)error)
			{
				ModioLog.Error?.Log($"Error getting File parameter from logo, cannot publish edits: {error}");
				return (error: error, mod: EditTarget);
			}
		}
		ModObject? modObject;
		(error, modObject) = await ModioAPI.Mods.EditMod(body: new EditModRequest(_pendingChanges.HasFlag(ChangeFlags.Name) ? Name : null, _pendingChanges.HasFlag(ChangeFlags.Name) ? text : null, _pendingChanges.HasFlag(ChangeFlags.Summary) ? Summary : null, _pendingChanges.HasFlag(ChangeFlags.Description) ? Description : null, value, _pendingChanges.HasFlag(ChangeFlags.Visibility) ? new long?((!Visible) ? 1 : 0) : null, _pendingChanges.HasFlag(ChangeFlags.MaturityOptions) ? new long?((long)MaturityOptions) : null, _pendingChanges.HasFlag(ChangeFlags.CommunityOptions) ? new long?((long)CommunityOptions) : null, _pendingChanges.HasFlag(ChangeFlags.MetadataBlob) ? MetadataBlob : null, _pendingChanges.HasFlag(ChangeFlags.Tags) ? Tags : null, _pendingChanges.HasFlag(ChangeFlags.MonetizationConfig) ? new long?((long)_monetizationOptions) : null, _pendingChanges.HasFlag(ChangeFlags.MonetizationConfig) ? new long?(Price) : null, _pendingChanges.HasFlag(ChangeFlags.MonetizationConfig) ? new long?(Stock) : null), modId: EditTarget.Id);
		_commitErrors[ChangeFlags.EditFlags] = error;
		if ((bool)error)
		{
			if (!error.IsSilent)
			{
				ModioLog.Error?.Log($"Error publishing changes for mod {EditTarget.Name}: {error}");
			}
			return (error: error, mod: EditTarget);
		}
		_pendingChanges &= ~ChangeFlags.EditFlags;
		Mod mod = ModCache.GetMod(modObject.Value);
		await PublishRemainingChanges();
		await mod.GetModDetailsFromServer();
		return (error: Error.None, mod: mod);
	}

	private async Task<Error> PublishGallery()
	{
		bool sync = !_appendingGallery;
		ModioAPIFileParameter media;
		Error error;
		(error, media) = await GalleryZipFromFilePaths(GalleryFilePaths);
		if ((bool)error)
		{
			ModioLog.Error?.Log($"Error creating {typeof(ModioAPIFileParameter)} for gallery publish request: {error}");
			return error;
		}
		error = (await ModioAPI.Media.AddModMedia(body: new AddModMediaRequest(media, sync), modId: EditTarget.Id)).Item1;
		if ((bool)error && !error.IsSilent)
		{
			ModioLog.Error?.Log($"Error publishing Gallery for {EditTarget.Name}: {error}");
		}
		return error;
	}

	private async Task<Error> PublishMetadataKvps()
	{
		if (MetadataKvps == null)
		{
			ModioLog.Error?.Log("Can't publish null MetadataKvps for mod " + EditTarget.Name);
			return new Error(ErrorCode.BAD_PARAMETER);
		}
		Error item = (await ModioAPI.Metadata.AddModKvpMetadata(body: new AddModMetadataRequest(MetadataKvps.Select((KeyValuePair<string, string> kvp) => kvp.Key + kvp.Value).ToArray()), modId: EditTarget.Id)).Item1;
		if ((bool)item && !item.IsSilent)
		{
			ModioLog.Error?.Log($"Error publishing MetadataKvps for {EditTarget.Name}: {item}");
		}
		return item;
	}

	private async Task<Error> PublishDependencies()
	{
		bool sync = !_appendingDependencies;
		Error item = (await ModioAPI.Dependencies.AddModDependencies(body: new AddModDependenciesRequest(Dependencies.ToArray(), sync), modId: EditTarget.Id)).Item1;
		if ((bool)item && !item.IsSilent)
		{
			ModioLog.Error?.Log($"Error publishing Dependencies for {EditTarget.Name}: {item}");
		}
		return item;
	}

	private Task<Error> PublishModfile()
	{
		return _modfileBuilder.PublishModfile();
	}

	private async Task<Error> PublishMonetization()
	{
		Error item = (await ModioAPI.Mods.EditMod(body: new EditModRequest(null, null, null, null, null, null, null, null, null, null, (long)_monetizationOptions, Price, Stock), modId: EditTarget.Id)).Item1;
		if ((bool)item && !item.IsSilent)
		{
			ModioLog.Error?.Log($"Error publishing Monetization changes for {EditTarget.Id}: {item}");
		}
		return item;
	}

	private Task<Error> GetChangeSpecificPublishTask(ChangeFlags flag)
	{
		return flag switch
		{
			ChangeFlags.Gallery => PublishGallery(), 
			ChangeFlags.MetadataKvps => PublishMetadataKvps(), 
			ChangeFlags.Modfile => PublishModfile(), 
			ChangeFlags.MonetizationConfig => PublishMonetization(), 
			ChangeFlags.Dependencies => PublishDependencies(), 
			ChangeFlags.Name => throw new ArgumentException($"{flag} should be changed through the Mods endpoint"), 
			ChangeFlags.Summary => throw new ArgumentException($"{flag} should be changed through the Mods endpoint"), 
			ChangeFlags.Description => throw new ArgumentException($"{flag} should be changed through the Mods endpoint"), 
			ChangeFlags.Logo => throw new ArgumentException($"{flag} should be changed through the Mods endpoint"), 
			ChangeFlags.MetadataBlob => throw new ArgumentException($"{flag} should be changed through the Mods endpoint"), 
			ChangeFlags.Tags => throw new ArgumentException($"{flag} should be changed through the Mods endpoint"), 
			ChangeFlags.Visibility => throw new ArgumentException($"{flag} should be changed through the Mods endpoint"), 
			ChangeFlags.CommunityOptions => throw new ArgumentException($"{flag} should be changed through the Mods endpoint"), 
			ChangeFlags.MaturityOptions => throw new ArgumentException($"{flag} should be changed through the Mods endpoint"), 
			ChangeFlags.AddFlags => throw new ArgumentException(string.Format("{0} should not be gotten from the {1} function! This could result in erroneous data being uploaded!", flag, "GetChangeSpecificPublishTask")), 
			ChangeFlags.EditFlags => throw new ArgumentException(string.Format("{0} should not be gotten from the {1} function! This could result in erroneous data being uploaded!", flag, "GetChangeSpecificPublishTask")), 
			ChangeFlags.None => throw new ArgumentException("None changes?"), 
			_ => throw new ArgumentException($"Change flag {flag} doesn't exist!"), 
		};
	}

	private static bool ValidateImageFilePath(string filePath)
	{
		if (string.IsNullOrEmpty(filePath))
		{
			ModioLog.Error?.Log("Image file path " + filePath + " cannot be null or empty!");
			return false;
		}
		string text = Path.GetExtension(filePath).ToLowerInvariant();
		string text2 = Path.GetFileName(filePath).ToLowerInvariant();
		if (string.IsNullOrEmpty(text2))
		{
			ModioLog.Error?.Log("Image file name " + text2 + " from path " + filePath + " cannot be null or empty!");
			return false;
		}
		if (text != ".png" && text != ".jpeg" && text != ".jpg")
		{
			ModioLog.Error?.Log("Invalid file extension: " + text + ". It must be either a .png, .jpg or .jpeg.");
			return false;
		}
		if (!File.Exists(filePath))
		{
			ModioLog.Error?.Log("Image " + filePath + " not found on file system.");
			return false;
		}
		return true;
	}

	private ModioAPIFileParameter LogoFromByteArray()
	{
		return new ModioAPIFileParameter(new MemoryStream(_logoBytes)
		{
			Position = 0L
		}, $"logo.{_logoBytesFormat}", $"image/{_logoBytesFormat}");
	}

	private static (Error error, ModioAPIFileParameter file) LogoFromFilePath(string filePath)
	{
		if (!ValidateImageFilePath(filePath))
		{
			return (error: new Error(ErrorCode.BAD_PARAMETER), file: default(ModioAPIFileParameter));
		}
		string text = Path.GetExtension(filePath).ToLowerInvariant();
		if (string.IsNullOrEmpty(text))
		{
			return (error: new Error(ErrorCode.BAD_PARAMETER), file: default(ModioAPIFileParameter));
		}
		Error none = Error.None;
		string name = "logo" + text;
		string text2 = text;
		return (error: none, file: new ModioAPIFileParameter(name, "image/" + text2.Substring(1, text2.Length - 1), filePath));
	}

	private static async Task<(Error error, ModioAPIFileParameter file)> GalleryZipFromFilePaths(ICollection<string> imageFilePaths)
	{
		foreach (string imageFilePath in imageFilePaths)
		{
			if (!ValidateImageFilePath(imageFilePath))
			{
				ModioLog.Error?.Log($"Can't upload {imageFilePaths.Count} gallery images, {imageFilePath} is invalid.");
				return (error: new Error(ErrorCode.BAD_PARAMETER), file: default(ModioAPIFileParameter));
			}
		}
		(Error error, ModioAPIFileParameter file) result;
		await using (MemoryStream memStream = new MemoryStream())
		{
			(Error error, ModioAPIFileParameter file) tuple;
			await using (ZipOutputStream zipStream = new ZipOutputStream(memStream))
			{
				foreach (string imageFilePath2 in imageFilePaths)
				{
					ZipEntry entry = new ZipEntry(Path.GetFileName(imageFilePath2));
					zipStream.PutNextEntry(entry);
					await using Stream readStream = File.Open(imageFilePath2, FileMode.Open);
					await readStream.CopyToAsync(zipStream);
					zipStream.CloseEntry();
				}
				memStream.Position = 0L;
				tuple = (error: Error.None, file: new ModioAPIFileParameter(memStream, "images.zip", "application/zip"));
			}
			result = tuple;
		}
		return result;
	}

	private static string GetExtensionFromFormat(ImageFormat format)
	{
		return format switch
		{
			ImageFormat.Jpeg => "jpeg", 
			ImageFormat.Jpg => "jpg", 
			ImageFormat.Png => "png", 
			_ => throw new ArgumentException($"Image format {format} not supported!"), 
		};
	}
}

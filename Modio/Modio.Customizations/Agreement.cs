using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Modio.API;
using Modio.API.SchemaDefinitions;

namespace Modio.Customizations;

public class Agreement
{
	internal static Dictionary<AgreementType, Agreement> _agreementCache = new Dictionary<AgreementType, Agreement>();

	public long Id { get; private set; }

	public bool IsActive { get; private set; }

	public bool IsLatest { get; private set; }

	public AgreementType Type { get; private set; }

	public DateTime DateAdded { get; private set; }

	public DateTime DateUpdated { get; private set; }

	public DateTime DateLive { get; private set; }

	public string Name { get; private set; }

	public string Changelog { get; private set; }

	public string Content { get; private set; }

	internal Agreement(AgreementVersionObject agreementObject)
	{
		Id = agreementObject.Id;
		ApplyDetailsFromAgreementObject(agreementObject);
	}

	internal Agreement ApplyDetailsFromAgreementObject(AgreementVersionObject agreementObject)
	{
		Name = agreementObject.Name;
		IsActive = agreementObject.IsActive;
		IsLatest = agreementObject.IsLatest;
		Type = (AgreementType)agreementObject.Type;
		Changelog = agreementObject.Changelog;
		Content = agreementObject.Description;
		DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(agreementObject.DateAdded);
		DateTimeOffset dateTimeOffset2 = DateTimeOffset.FromUnixTimeSeconds(agreementObject.DateUpdated);
		DateTimeOffset dateTimeOffset3 = DateTimeOffset.FromUnixTimeSeconds(agreementObject.DateLive);
		DateAdded = dateTimeOffset.Date;
		DateUpdated = dateTimeOffset2.Date;
		DateLive = dateTimeOffset3.Date;
		return this;
	}

	public static async Task<(Error error, Agreement result)> GetAgreement(AgreementType type, bool forceUpdate = false)
	{
		if (forceUpdate)
		{
			_agreementCache.Remove(type);
		}
		if (_agreementCache.TryGetValue(type, out var value))
		{
			return (error: Error.None, result: value);
		}
		var (error, agreementVersionObject) = await ModioAPI.Agreements.GetCurrentAgreement((long)type);
		if ((bool)error)
		{
			if (!error.IsSilent)
			{
				ModioLog.Error?.Log("Error getting Agreement Type " + type.ToString() + ": " + error.GetMessage());
			}
			return (error: error, result: null);
		}
		Agreement agreement = new Agreement(agreementVersionObject.Value);
		_agreementCache[type] = agreement;
		return (error: error, result: agreement);
	}
}

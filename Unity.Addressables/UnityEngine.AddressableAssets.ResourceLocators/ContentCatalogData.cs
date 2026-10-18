using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.Util;

namespace UnityEngine.AddressableAssets.ResourceLocators;

[Serializable]
public class ContentCatalogData
{
	internal class Serializer : BinaryStorageBuffer.ISerializationAdapter<ContentCatalogData>, BinaryStorageBuffer.ISerializationAdapter
	{
		private bool resolveInternalIds = true;

		public IEnumerable<BinaryStorageBuffer.ISerializationAdapter> Dependencies => new BinaryStorageBuffer.ISerializationAdapter[3]
		{
			new ObjectInitializationData.Serializer(),
			new AssetBundleRequestOptionsSerializationAdapter(),
			new ResourceLocator.ResourceLocation.Serializer(resolveInternalIds)
		};

		public Serializer WithInternalIdResolvingDisabled()
		{
			resolveInternalIds = false;
			return this;
		}

		public object Deserialize(BinaryStorageBuffer.Reader reader, Type t, uint offset, out uint size)
		{
			ContentCatalogData contentCatalogData = new ContentCatalogData(reader);
			uint size2;
			ResourceLocator.Header header = reader.ReadValue<ResourceLocator.Header>(offset, out size2);
			if (header.magic != kMagic)
			{
				throw new Exception("Invalid header data!!!");
			}
			if (header.version != 2)
			{
				throw new Exception($"Expected catalog data version {2}, but file was written with version {header.version}.");
			}
			contentCatalogData.InstanceProviderData = reader.ReadObject<ObjectInitializationData>(header.instanceProvider, out var size3, cacheValue: false);
			contentCatalogData.SceneProviderData = reader.ReadObject<ObjectInitializationData>(header.sceneProvider, out var size4, cacheValue: false);
			contentCatalogData.ResourceProviderData = reader.ReadObjectArray<ObjectInitializationData>(header.initObjectsArray, out var size5, cacheValues: false).ToList();
			contentCatalogData.BuildResultHash = reader.ReadString(header.buildResultHash, out var size6);
			size = size2 + size3 + size4 + size5 + size6;
			return contentCatalogData;
		}

		public uint Serialize(BinaryStorageBuffer.Writer writer, object val)
		{
			ContentCatalogData contentCatalogData = val as ContentCatalogData;
			IList<ContentCatalogDataEntry> entries = contentCatalogData.m_Entries;
			Dictionary<object, List<int>> dictionary = new Dictionary<object, List<int>>();
			for (int j = 0; j < entries.Count; j++)
			{
				foreach (object key in entries[j].Keys)
				{
					if (!dictionary.TryGetValue(key, out var value))
					{
						dictionary.Add(key, value = new List<int>());
					}
					value.Add(j);
				}
			}
			uint num = writer.Reserve<ResourceLocator.Header>();
			uint num2 = writer.Reserve<ResourceLocator.KeyData>((uint)dictionary.Count);
			ResourceLocator.Header header = default(ResourceLocator.Header);
			header.magic = kMagic;
			header.version = 2;
			header.keysOffset = num2;
			header.idOffset = writer.WriteString(contentCatalogData.ProviderId);
			header.instanceProvider = writer.WriteObject(contentCatalogData.InstanceProviderData, serializeTypeData: false);
			header.sceneProvider = writer.WriteObject(contentCatalogData.SceneProviderData, serializeTypeData: false);
			header.initObjectsArray = writer.WriteObjects(contentCatalogData.m_ResourceProviderData, serizalizeTypeData: false);
			header.buildResultHash = writer.WriteString(contentCatalogData.BuildResultHash);
			ResourceLocator.Header val2 = header;
			writer.Write(num, in val2);
			uint[] locationIds = new uint[entries.Count];
			for (int k = 0; k < entries.Count; k++)
			{
				locationIds[k] = writer.WriteObject(new ResourceLocator.ContentCatalogDataEntrySerializationContext
				{
					entry = entries[k],
					allEntries = entries,
					keyToEntryIndices = dictionary
				}, serializeTypeData: false);
			}
			int num3 = 0;
			ResourceLocator.KeyData[] array = new ResourceLocator.KeyData[dictionary.Count];
			foreach (KeyValuePair<object, List<int>> item in dictionary)
			{
				uint[] values = item.Value.Select((int i) => locationIds[i]).ToArray();
				array[num3++] = new ResourceLocator.KeyData
				{
					keyNameOffset = writer.WriteObject(item.Key, serializeTypeData: true),
					locationSetOffset = writer.Write(values, hashElements: true)
				};
			}
			writer.Write(num2, array, hashElements: true);
			return num;
		}
	}

	internal class ResourceLocator : IResourceLocator
	{
		public struct Header
		{
			public int magic;

			public int version;

			public uint keysOffset;

			public uint idOffset;

			public uint instanceProvider;

			public uint sceneProvider;

			public uint initObjectsArray;

			public uint buildResultHash;
		}

		public struct KeyData
		{
			public uint keyNameOffset;

			public uint locationSetOffset;
		}

		internal class ContentCatalogDataEntrySerializationContext
		{
			public ContentCatalogDataEntry entry;

			public Dictionary<object, List<int>> keyToEntryIndices;

			public IList<ContentCatalogDataEntry> allEntries;
		}

		internal class ResourceLocation : IResourceLocation
		{
			private class ResolvedInternalId
			{
				public string InternalId;
			}

			public class ResolvedInternalIdSerializer : BinaryStorageBuffer.ISerializationAdapter<ResolvedInternalId>, BinaryStorageBuffer.ISerializationAdapter
			{
				IEnumerable<BinaryStorageBuffer.ISerializationAdapter> BinaryStorageBuffer.ISerializationAdapter.Dependencies => null;

				object BinaryStorageBuffer.ISerializationAdapter.Deserialize(BinaryStorageBuffer.Reader reader, Type t, uint offset, out uint size)
				{
					string internalId = Addressables.ResolveInternalId(reader.ReadString(offset, out size, '/'));
					return new ResolvedInternalId
					{
						InternalId = internalId
					};
				}

				uint BinaryStorageBuffer.ISerializationAdapter.Serialize(BinaryStorageBuffer.Writer writer, object val)
				{
					throw new NotImplementedException();
				}
			}

			public class Serializer : BinaryStorageBuffer.ISerializationAdapter<ResourceLocation>, BinaryStorageBuffer.ISerializationAdapter, BinaryStorageBuffer.ISerializationAdapter<ContentCatalogDataEntrySerializationContext>
			{
				public struct Data
				{
					public uint primaryKeyOffset;

					public uint internalIdOffset;

					public uint providerOffset;

					public uint dependencySetOffset;

					public int dependencyHashValue;

					public uint extraDataOffset;

					public uint typeId;
				}

				private bool resolveInternalIds;

				public IEnumerable<BinaryStorageBuffer.ISerializationAdapter> Dependencies => new BinaryStorageBuffer.ISerializationAdapter[1]
				{
					new ResolvedInternalIdSerializer()
				};

				public Serializer(bool resolveInternalIds)
				{
					this.resolveInternalIds = resolveInternalIds;
				}

				public object Deserialize(BinaryStorageBuffer.Reader reader, Type t, uint offset, out uint size)
				{
					return new ResourceLocation(reader, offset, out size, resolveInternalIds);
				}

				public uint Serialize(BinaryStorageBuffer.Writer writer, object val)
				{
					ContentCatalogDataEntrySerializationContext contentCatalogDataEntrySerializationContext = val as ContentCatalogDataEntrySerializationContext;
					ContentCatalogDataEntry entry = contentCatalogDataEntrySerializationContext.entry;
					uint dependencySetOffset = uint.MaxValue;
					if (entry.Dependencies != null && entry.Dependencies.Count > 0)
					{
						HashSet<uint> hashSet = new HashSet<uint>();
						foreach (object dependency in entry.Dependencies)
						{
							foreach (int item in contentCatalogDataEntrySerializationContext.keyToEntryIndices[dependency])
							{
								hashSet.Add(writer.WriteObject(new ContentCatalogDataEntrySerializationContext
								{
									entry = contentCatalogDataEntrySerializationContext.allEntries[item],
									allEntries = contentCatalogDataEntrySerializationContext.allEntries,
									keyToEntryIndices = contentCatalogDataEntrySerializationContext.keyToEntryIndices
								}, serializeTypeData: false));
							}
						}
						dependencySetOffset = writer.Write(hashSet.ToArray(), hashElements: false);
					}
					Data data = default(Data);
					data.primaryKeyOffset = writer.WriteString(entry.Keys[0] as string, '/');
					data.internalIdOffset = writer.WriteString(entry.InternalId, '/');
					data.providerOffset = writer.WriteString(entry.Provider, '.');
					data.dependencySetOffset = dependencySetOffset;
					data.extraDataOffset = writer.WriteObject(entry.Data, serializeTypeData: true);
					data.typeId = writer.WriteObject(entry.ResourceType, serializeTypeData: false);
					Data val2 = data;
					return writer.Write(val2);
				}
			}

			private BinaryStorageBuffer.Reader reader;

			private List<IResourceLocation> _deps;

			private uint dependencyDataOffset;

			public string InternalId { get; internal set; }

			public string ProviderId { get; internal set; }

			public IList<IResourceLocation> Dependencies
			{
				get
				{
					if (_deps == null)
					{
						_deps = new List<IResourceLocation>();
						reader.ProcessObjectArray<ResourceLocation, ResourceLocation>(dependencyDataOffset, out var _, this, ProcDependencies);
					}
					return _deps;
				}
			}

			public int DependencyHashCode => dependencyDataOffset.GetHashCode();

			public bool HasDependencies => dependencyDataOffset != uint.MaxValue;

			public object Data { get; internal set; }

			public string PrimaryKey { get; internal set; }

			public Type ResourceType { get; internal set; }

			public ResourceLocation(BinaryStorageBuffer.Reader r, uint id, out uint size, bool resolveInternalId)
			{
				reader = r;
				uint size2;
				Serializer.Data data = reader.ReadValue<Serializer.Data>(id, out size2);
				size = size2;
				ProviderId = reader.ReadString(data.providerOffset, out var size3, '.');
				size += size3;
				PrimaryKey = reader.ReadString(data.primaryKeyOffset, out var size4, '/');
				size += size4;
				Data = reader.ReadObject(data.extraDataOffset, out var size5);
				size += size5;
				if (resolveInternalId)
				{
					InternalId = reader.ReadObject<ResolvedInternalId>(data.internalIdOffset, out var size6).InternalId;
					size += size6;
				}
				else
				{
					InternalId = reader.ReadString(data.internalIdOffset, out var size7, '/');
					size += size7;
				}
				dependencyDataOffset = data.dependencySetOffset;
				ResourceType = reader.ReadObject<Type>(data.typeId, out var size8);
				size += size8;
			}

			private static void ProcDependencies(ResourceLocation l, ResourceLocation d, int i, int count)
			{
				if (d._deps == null)
				{
					d._deps = new List<IResourceLocation>(count);
				}
				d._deps.Add(l);
			}

			public override string ToString()
			{
				return InternalId;
			}

			public int Hash(Type resultType)
			{
				return InternalId.GetHashCode() * 31 + ResourceType.GetHashCode();
			}
		}

		private class LocateProcContext
		{
			public IList<IResourceLocation> locations;

			public Type type;
		}

		private Dictionary<object, uint> keyData;

		private BinaryStorageBuffer.Reader reader;

		private string providerSuffix;

		private LocateProcContext sharedContext = new LocateProcContext();

		public string LocatorId { get; private set; }

		public IEnumerable<object> Keys => keyData.Keys;

		public IEnumerable<IResourceLocation> AllLocations
		{
			get
			{
				HashSet<IResourceLocation> hashSet = new HashSet<IResourceLocation>(new ResourceLocationComparer());
				foreach (KeyValuePair<object, uint> keyDatum in keyData)
				{
					if (!Locate(keyDatum.Key, null, out var locations))
					{
						continue;
					}
					foreach (IResourceLocation item in locations)
					{
						hashSet.Add(item);
					}
				}
				return hashSet;
			}
		}

		internal ResourceLocator(string id, BinaryStorageBuffer.Reader reader, string providerSuffix)
		{
			LocatorId = id;
			this.providerSuffix = providerSuffix;
			this.reader = reader;
			this.keyData = new Dictionary<object, uint>();
			uint size;
			KeyData[] array = reader.ReadValueArray<KeyData>(reader.ReadValue<Header>(0u, out size).keysOffset, out size, cacheValue: false);
			int num = 0;
			KeyData[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				KeyData keyData = array2[i];
				object key = reader.ReadObject(keyData.keyNameOffset, out size);
				this.keyData.Add(key, keyData.locationSetOffset);
				num++;
			}
			reader.ResetCache(this.keyData.Count * 3, 0u);
		}

		private static void ProcFunc(ResourceLocation loc, LocateProcContext context, int i, int count)
		{
			if (context.type == null || context.type == typeof(object) || context.type.IsAssignableFrom(loc.ResourceType))
			{
				if (context.locations == null)
				{
					context.locations = new List<IResourceLocation>(count);
				}
				context.locations.Add(loc);
			}
		}

		public bool Locate(object key, Type type, out IList<IResourceLocation> locations)
		{
			if (!keyData.TryGetValue(key, out var value))
			{
				locations = null;
				return false;
			}
			sharedContext.type = type;
			reader.ProcessObjectArray<ResourceLocation, LocateProcContext>(value, out var _, sharedContext, ProcFunc);
			locations = sharedContext.locations;
			sharedContext.locations = null;
			sharedContext.type = null;
			if (providerSuffix != null && locations != null)
			{
				foreach (IResourceLocation location in locations)
				{
					if (!location.ProviderId.EndsWith(providerSuffix))
					{
						(location as ResourceLocation).ProviderId = location.ProviderId + providerSuffix;
					}
				}
			}
			return locations != null;
		}
	}

	internal class AssetBundleRequestOptionsSerializationAdapter : BinaryStorageBuffer.ISerializationAdapter<AssetBundleRequestOptions>, BinaryStorageBuffer.ISerializationAdapter
	{
		private struct SerializedData
		{
			public struct Common
			{
				public short timeout;

				public byte redirectLimit;

				public byte retryCount;

				public int flags;

				public AssetLoadMode assetLoadMode
				{
					get
					{
						if ((flags & 1) != 1)
						{
							return AssetLoadMode.RequestedAssetAndDependencies;
						}
						return AssetLoadMode.AllPackedAssetsAndDependencies;
					}
					set
					{
						flags = (flags & -2) | (int)value;
					}
				}

				public bool chunkedTransfer
				{
					get
					{
						return (flags & 2) == 2;
					}
					set
					{
						flags = (flags & -3) | (value ? 2 : 0);
					}
				}

				public bool useCrcForCachedBundle
				{
					get
					{
						return (flags & 4) == 4;
					}
					set
					{
						flags = (flags & -5) | (value ? 4 : 0);
					}
				}

				public bool useUnityWebRequestForLocalBundles
				{
					get
					{
						return (flags & 8) == 8;
					}
					set
					{
						flags = (flags & -9) | (value ? 8 : 0);
					}
				}

				public bool clearOtherCachedVersionsWhenLoaded
				{
					get
					{
						return (flags & 0x10) == 16;
					}
					set
					{
						flags = (flags & -17) | (value ? 16 : 0);
					}
				}
			}

			public uint hashId;

			public uint bundleNameId;

			public uint crc;

			public uint bundleSize;

			public uint commonId;
		}

		public IEnumerable<BinaryStorageBuffer.ISerializationAdapter> Dependencies => null;

		public object Deserialize(BinaryStorageBuffer.Reader reader, Type type, uint offset, out uint size)
		{
			size = 0u;
			if (type != typeof(AssetBundleRequestOptions))
			{
				return null;
			}
			uint size2;
			SerializedData serializedData = reader.ReadValue<SerializedData>(offset, out size2);
			uint size3;
			SerializedData.Common common = reader.ReadValue<SerializedData.Common>(serializedData.commonId, out size3);
			uint size4;
			string hash = reader.ReadValue<Hash128>(serializedData.hashId, out size4).ToString();
			uint size5;
			string bundleName = reader.ReadString(serializedData.bundleNameId, out size5, '_');
			AssetBundleRequestOptions result = new AssetBundleRequestOptions
			{
				Hash = hash,
				BundleName = bundleName,
				Crc = serializedData.crc,
				BundleSize = serializedData.bundleSize,
				Timeout = common.timeout,
				RetryCount = common.retryCount,
				RedirectLimit = common.redirectLimit,
				AssetLoadMode = common.assetLoadMode,
				ChunkedTransfer = common.chunkedTransfer,
				UseUnityWebRequestForLocalBundles = common.useUnityWebRequestForLocalBundles,
				UseCrcForCachedBundle = common.useCrcForCachedBundle,
				ClearOtherCachedVersionsWhenLoaded = common.clearOtherCachedVersionsWhenLoaded
			};
			size = size2 + size3 + size4 + size5;
			return result;
		}

		public uint Serialize(BinaryStorageBuffer.Writer writer, object obj)
		{
			AssetBundleRequestOptions assetBundleRequestOptions = obj as AssetBundleRequestOptions;
			Hash128 val = Hash128.Parse(assetBundleRequestOptions.Hash);
			short timeout = (short)Mathf.Clamp(assetBundleRequestOptions.Timeout, 0, 32767);
			byte retryCount = (byte)Mathf.Clamp(assetBundleRequestOptions.RetryCount, 0, 128);
			byte redirectLimit = (byte)((assetBundleRequestOptions.RedirectLimit < 0) ? 32 : ((byte)Mathf.Clamp(assetBundleRequestOptions.RedirectLimit, 0, 128)));
			SerializedData serializedData = default(SerializedData);
			serializedData.hashId = writer.Write(val);
			serializedData.bundleNameId = writer.WriteString(assetBundleRequestOptions.BundleName, '_');
			serializedData.crc = assetBundleRequestOptions.Crc;
			serializedData.bundleSize = (uint)assetBundleRequestOptions.BundleSize;
			serializedData.commonId = writer.Write(new SerializedData.Common
			{
				timeout = timeout,
				redirectLimit = redirectLimit,
				retryCount = retryCount,
				assetLoadMode = assetBundleRequestOptions.AssetLoadMode,
				chunkedTransfer = assetBundleRequestOptions.ChunkedTransfer,
				clearOtherCachedVersionsWhenLoaded = assetBundleRequestOptions.ClearOtherCachedVersionsWhenLoaded,
				useCrcForCachedBundle = assetBundleRequestOptions.UseCrcForCachedBundle,
				useUnityWebRequestForLocalBundles = assetBundleRequestOptions.UseUnityWebRequestForLocalBundles
			});
			SerializedData val2 = serializedData;
			return writer.Write(val2);
		}
	}

	private static int kMagic = "ContentCatalogData".GetHashCode();

	private const int kVersion = 2;

	[NonSerialized]
	public string LocalHash;

	[NonSerialized]
	internal IResourceLocation location;

	[SerializeField]
	internal string m_LocatorId;

	[SerializeField]
	internal string m_BuildResultHash;

	[SerializeField]
	private ObjectInitializationData m_InstanceProviderData;

	[SerializeField]
	private ObjectInitializationData m_SceneProviderData;

	[SerializeField]
	internal List<ObjectInitializationData> m_ResourceProviderData = new List<ObjectInitializationData>();

	private IList<ContentCatalogDataEntry> m_Entries;

	private BinaryStorageBuffer.Reader m_Reader;

	public string BuildResultHash
	{
		get
		{
			return m_BuildResultHash;
		}
		set
		{
			m_BuildResultHash = value;
		}
	}

	public string ProviderId
	{
		get
		{
			return m_LocatorId;
		}
		internal set
		{
			m_LocatorId = value;
		}
	}

	public ObjectInitializationData InstanceProviderData
	{
		get
		{
			return m_InstanceProviderData;
		}
		set
		{
			m_InstanceProviderData = value;
		}
	}

	public ObjectInitializationData SceneProviderData
	{
		get
		{
			return m_SceneProviderData;
		}
		set
		{
			m_SceneProviderData = value;
		}
	}

	public List<ObjectInitializationData> ResourceProviderData
	{
		get
		{
			return m_ResourceProviderData;
		}
		set
		{
			m_ResourceProviderData = value;
		}
	}

	public ContentCatalogData(string id)
	{
		m_LocatorId = id;
	}

	public ContentCatalogData()
	{
	}

	internal void CleanData()
	{
		m_LocatorId = null;
		m_Reader = null;
	}

	internal void CopyToFile(string path)
	{
		byte[] buffer = m_Reader.GetBuffer();
		File.WriteAllBytes(path, buffer);
	}

	internal ContentCatalogData(BinaryStorageBuffer.Reader reader)
	{
		m_Reader = reader;
	}

	internal byte[] GetBytes()
	{
		return m_Reader.GetBuffer();
	}

	internal IResourceLocator CreateCustomLocator(string overrideId = "", string providerSuffix = null)
	{
		m_LocatorId = overrideId;
		return new ResourceLocator(m_LocatorId, m_Reader, providerSuffix);
	}

	internal static ContentCatalogData LoadFromFile(string path, bool resolveInternalIds)
	{
		return new ContentCatalogData(new BinaryStorageBuffer.Reader(File.ReadAllBytes(path), 0, 0u, resolveInternalIds ? new Serializer() : new Serializer().WithInternalIdResolvingDisabled()));
	}
}

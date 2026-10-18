using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Pathfinding.Ionic.Zlib;

namespace Pathfinding.Ionic.Zip;

[Guid("ebc25cf6-9120-4283-b972-0e5520d00005")]
[ClassInterface(ClassInterfaceType.AutoDispatch)]
[ComVisible(true)]
public class ZipFile : IEnumerable<ZipEntry>, IDisposable, IEnumerable
{
	private long _lengthOfReadStream = -99L;

	private TextWriter _StatusMessageTextWriter;

	private bool _CaseSensitiveRetrieval;

	private Stream _readstream;

	private Stream _writestream;

	private ushort _versionMadeBy;

	private ushort _versionNeededToExtract;

	private uint _diskNumberWithCd;

	private int _maxOutputSegmentSize;

	private uint _numberOfSegmentsForMostRecentSave;

	private ZipErrorAction _zipErrorAction;

	private bool _disposed;

	private Dictionary<string, ZipEntry> _entries;

	private List<ZipEntry> _zipEntriesAsList;

	private string _name;

	private string _readName;

	private string _Comment;

	internal string _Password;

	private bool _emitNtfsTimes = true;

	private bool _emitUnixTimes;

	private CompressionStrategy _Strategy;

	private CompressionMethod _compressionMethod = CompressionMethod.Deflate;

	private bool _fileAlreadyExists;

	private string _temporaryFileName;

	private bool _contentsChanged;

	private bool _hasBeenSaved;

	private string _TempFileFolder;

	private bool _ReadStreamIsOurs = true;

	private object LOCK = new object();

	private bool _saveOperationCanceled;

	private bool _extractOperationCanceled;

	private bool _addOperationCanceled;

	private EncryptionAlgorithm _Encryption;

	private bool _JustSaved;

	private long _locEndOfCDS = -1L;

	private uint _OffsetOfCentralDirectory;

	private long _OffsetOfCentralDirectory64;

	private bool? _OutputUsesZip64;

	internal bool _inExtractAll;

	private static Encoding _defaultEncoding = Encoding.UTF8;

	private Encoding _alternateEncoding = Encoding.UTF8;

	private ZipOption _alternateEncodingUsage;

	private int _BufferSize = BufferSizeDefault;

	internal ParallelDeflateOutputStream ParallelDeflater;

	private long _ParallelDeflateThreshold;

	private int _maxBufferPairs = 16;

	internal Zip64Option _zip64;

	private bool _SavingSfx;

	public static readonly int BufferSizeDefault = 32768;

	private EventHandler<SaveProgressEventArgs> m_SaveProgress;

	private EventHandler<ReadProgressEventArgs> m_ReadProgress;

	private EventHandler<ExtractProgressEventArgs> m_ExtractProgress;

	private EventHandler<AddProgressEventArgs> m_AddProgress;

	private EventHandler<ZipErrorEventArgs> m_ZipError;

	public string Info
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append($"          ZipFile: {Name}\n");
			if (!string.IsNullOrEmpty(_Comment))
			{
				stringBuilder.Append($"          Comment: {_Comment}\n");
			}
			if (_versionMadeBy != 0)
			{
				stringBuilder.Append($"  version made by: 0x{_versionMadeBy:X4}\n");
			}
			if (_versionNeededToExtract != 0)
			{
				stringBuilder.Append($"needed to extract: 0x{_versionNeededToExtract:X4}\n");
			}
			stringBuilder.Append($"       uses ZIP64: {InputUsesZip64}\n");
			stringBuilder.Append($"     disk with CD: {_diskNumberWithCd}\n");
			if (_OffsetOfCentralDirectory == uint.MaxValue)
			{
				stringBuilder.Append($"      CD64 offset: 0x{_OffsetOfCentralDirectory64:X16}\n");
			}
			else
			{
				stringBuilder.Append($"        CD offset: 0x{_OffsetOfCentralDirectory:X8}\n");
			}
			stringBuilder.Append("\n");
			foreach (ZipEntry value in _entries.Values)
			{
				stringBuilder.Append(value.Info);
			}
			return stringBuilder.ToString();
		}
	}

	private string ArchiveNameForEvent => (_name == null) ? "(stream)" : _name;

	private long LengthOfReadStream
	{
		get
		{
			if (_lengthOfReadStream == -99)
			{
				_lengthOfReadStream = ((!_ReadStreamIsOurs) ? (-1) : SharedUtilities.GetFileLength(_name));
			}
			return _lengthOfReadStream;
		}
	}

	public bool FullScan { get; set; }

	public bool SortEntriesBeforeSaving { get; set; }

	public bool AddDirectoryWillTraverseReparsePoints { get; set; }

	public int BufferSize
	{
		get
		{
			return _BufferSize;
		}
		set
		{
			_BufferSize = value;
		}
	}

	public int CodecBufferSize { get; set; }

	public bool FlattenFoldersOnExtract { get; set; }

	public CompressionStrategy Strategy
	{
		get
		{
			return _Strategy;
		}
		set
		{
			_Strategy = value;
		}
	}

	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			_name = value;
		}
	}

	public CompressionLevel CompressionLevel { get; set; }

	public CompressionMethod CompressionMethod
	{
		get
		{
			return _compressionMethod;
		}
		set
		{
			_compressionMethod = value;
		}
	}

	public string Comment
	{
		get
		{
			return _Comment;
		}
		set
		{
			_Comment = value;
			_contentsChanged = true;
		}
	}

	public bool EmitTimesInWindowsFormatWhenSaving
	{
		get
		{
			return _emitNtfsTimes;
		}
		set
		{
			_emitNtfsTimes = value;
		}
	}

	public bool EmitTimesInUnixFormatWhenSaving
	{
		get
		{
			return _emitUnixTimes;
		}
		set
		{
			_emitUnixTimes = value;
		}
	}

	internal bool Verbose => _StatusMessageTextWriter != null;

	public bool CaseSensitiveRetrieval
	{
		get
		{
			return _CaseSensitiveRetrieval;
		}
		set
		{
			if (value != _CaseSensitiveRetrieval)
			{
				_CaseSensitiveRetrieval = value;
				_initEntriesDictionary();
			}
		}
	}

	[Obsolete("Beginning with v1.9.1.6 of DotNetZip, this property is obsolete.  It will be removed in a future version of the library. Your applications should  use AlternateEncoding and AlternateEncodingUsage instead.")]
	public bool UseUnicodeAsNecessary
	{
		get
		{
			return _alternateEncoding == Encoding.GetEncoding("UTF-8") && _alternateEncodingUsage == ZipOption.AsNecessary;
		}
		set
		{
			if (value)
			{
				_alternateEncoding = Encoding.GetEncoding("UTF-8");
				_alternateEncodingUsage = ZipOption.AsNecessary;
			}
			else
			{
				_alternateEncoding = DefaultEncoding;
				_alternateEncodingUsage = ZipOption.Default;
			}
		}
	}

	public Zip64Option UseZip64WhenSaving
	{
		get
		{
			return _zip64;
		}
		set
		{
			_zip64 = value;
		}
	}

	public bool? RequiresZip64
	{
		get
		{
			if (_entries.Count > 65534)
			{
				return true;
			}
			if (!_hasBeenSaved || _contentsChanged)
			{
				return null;
			}
			foreach (ZipEntry value in _entries.Values)
			{
				if (value.RequiresZip64.Value)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool? OutputUsedZip64 => _OutputUsesZip64;

	public bool? InputUsesZip64
	{
		get
		{
			if (_entries.Count > 65534)
			{
				return true;
			}
			using (IEnumerator<ZipEntry> enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ZipEntry current = enumerator.Current;
					if (current.Source != ZipEntrySource.ZipFile)
					{
						return null;
					}
					if (current._InputUsesZip64)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	[Obsolete("use AlternateEncoding instead.")]
	public Encoding ProvisionalAlternateEncoding
	{
		get
		{
			if (_alternateEncodingUsage == ZipOption.AsNecessary)
			{
				return _alternateEncoding;
			}
			return null;
		}
		set
		{
			_alternateEncoding = value;
			_alternateEncodingUsage = ZipOption.AsNecessary;
		}
	}

	public Encoding AlternateEncoding
	{
		get
		{
			return _alternateEncoding;
		}
		set
		{
			_alternateEncoding = value;
		}
	}

	public ZipOption AlternateEncodingUsage
	{
		get
		{
			return _alternateEncodingUsage;
		}
		set
		{
			_alternateEncodingUsage = value;
		}
	}

	public static Encoding DefaultEncoding => _defaultEncoding;

	public TextWriter StatusMessageTextWriter
	{
		get
		{
			return _StatusMessageTextWriter;
		}
		set
		{
			_StatusMessageTextWriter = value;
		}
	}

	public string TempFileFolder
	{
		get
		{
			return _TempFileFolder;
		}
		set
		{
			_TempFileFolder = value;
			if (value == null || Directory.Exists(value))
			{
				return;
			}
			throw new FileNotFoundException($"That directory ({value}) does not exist.");
		}
	}

	public string Password
	{
		private get
		{
			return _Password;
		}
		set
		{
			_Password = value;
			if (_Password == null)
			{
				Encryption = EncryptionAlgorithm.None;
			}
			else if (Encryption == EncryptionAlgorithm.None)
			{
				Encryption = EncryptionAlgorithm.PkzipWeak;
			}
		}
	}

	public ExtractExistingFileAction ExtractExistingFile { get; set; }

	public ZipErrorAction ZipErrorAction
	{
		get
		{
			if (this.ZipError != null)
			{
				_zipErrorAction = ZipErrorAction.InvokeErrorEvent;
			}
			return _zipErrorAction;
		}
		set
		{
			_zipErrorAction = value;
			if (_zipErrorAction != ZipErrorAction.InvokeErrorEvent && this.ZipError != null)
			{
				this.ZipError = null;
			}
		}
	}

	public EncryptionAlgorithm Encryption
	{
		get
		{
			return _Encryption;
		}
		set
		{
			if (value == EncryptionAlgorithm.Unsupported)
			{
				throw new InvalidOperationException("You may not set Encryption to that value.");
			}
			_Encryption = value;
		}
	}

	public SetCompressionCallback SetCompression { get; set; }

	public int MaxOutputSegmentSize
	{
		get
		{
			return _maxOutputSegmentSize;
		}
		set
		{
			if (value < 65536 && value != 0)
			{
				throw new ZipException("The minimum acceptable segment size is 65536.");
			}
			_maxOutputSegmentSize = value;
		}
	}

	public int NumberOfSegmentsForMostRecentSave => (int)(_numberOfSegmentsForMostRecentSave + 1);

	public long ParallelDeflateThreshold
	{
		get
		{
			return _ParallelDeflateThreshold;
		}
		set
		{
			if (value != 0 && value != -1 && value < 65536)
			{
				throw new ArgumentOutOfRangeException("ParallelDeflateThreshold should be -1, 0, or > 65536");
			}
			_ParallelDeflateThreshold = value;
		}
	}

	public int ParallelDeflateMaxBufferPairs
	{
		get
		{
			return _maxBufferPairs;
		}
		set
		{
			if (value < 4)
			{
				throw new ArgumentOutOfRangeException("ParallelDeflateMaxBufferPairs", "Value must be 4 or greater.");
			}
			_maxBufferPairs = value;
		}
	}

	public static Version LibraryVersion => Assembly.GetExecutingAssembly().GetName().Version;

	private List<ZipEntry> ZipEntriesAsList
	{
		get
		{
			if (_zipEntriesAsList == null)
			{
				_zipEntriesAsList = new List<ZipEntry>(_entries.Values);
			}
			return _zipEntriesAsList;
		}
	}

	public ZipEntry this[int ix] => ZipEntriesAsList[ix];

	public ZipEntry this[string fileName]
	{
		get
		{
			string text = SharedUtilities.NormalizePathForUseInZipFile(fileName);
			if (_entries.ContainsKey(text))
			{
				return _entries[text];
			}
			text = text.Replace("/", "\\");
			if (_entries.ContainsKey(text))
			{
				return _entries[text];
			}
			return null;
		}
	}

	public ICollection<string> EntryFileNames => _entries.Keys;

	public ICollection<ZipEntry> Entries => _entries.Values;

	public ICollection<ZipEntry> EntriesSorted
	{
		get
		{
			List<ZipEntry> list = new List<ZipEntry>();
			foreach (ZipEntry entry in Entries)
			{
				list.Add(entry);
			}
			StringComparison stringComparison = ((!CaseSensitiveRetrieval) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
			return list.AsReadOnly();
		}
	}

	public int Count => _entries.Count;

	internal Stream ReadStream
	{
		get
		{
			if (_readstream == null && (_readName != null || _name != null))
			{
				_readstream = File.Open(_readName ?? _name, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				_ReadStreamIsOurs = true;
			}
			return _readstream;
		}
	}

	private Stream WriteStream
	{
		get
		{
			if (_writestream != null)
			{
				return _writestream;
			}
			if (_name == null)
			{
				return _writestream;
			}
			if (_maxOutputSegmentSize != 0)
			{
				_writestream = ZipSegmentedStream.ForWriting(_name, _maxOutputSegmentSize);
				return _writestream;
			}
			SharedUtilities.CreateAndOpenUniqueTempFile(TempFileFolder ?? Path.GetDirectoryName(_name), out _writestream, out _temporaryFileName);
			return _writestream;
		}
		set
		{
			if (value != null)
			{
				throw new ZipException("Cannot set the stream to a non-null value.");
			}
			_writestream = null;
		}
	}

	public event EventHandler<SaveProgressEventArgs> SaveProgress
	{
		add
		{
			EventHandler<SaveProgressEventArgs> eventHandler = this.m_SaveProgress;
			EventHandler<SaveProgressEventArgs> eventHandler2;
			do
			{
				eventHandler2 = eventHandler;
				eventHandler = Interlocked.CompareExchange(ref this.m_SaveProgress, (EventHandler<SaveProgressEventArgs>)Delegate.Combine(eventHandler2, value), eventHandler);
			}
			while (eventHandler != eventHandler2);
		}
		remove
		{
			EventHandler<SaveProgressEventArgs> eventHandler = this.m_SaveProgress;
			EventHandler<SaveProgressEventArgs> eventHandler2;
			do
			{
				eventHandler2 = eventHandler;
				eventHandler = Interlocked.CompareExchange(ref this.m_SaveProgress, (EventHandler<SaveProgressEventArgs>)Delegate.Remove(eventHandler2, value), eventHandler);
			}
			while (eventHandler != eventHandler2);
		}
	}

	public event EventHandler<ReadProgressEventArgs> ReadProgress
	{
		add
		{
			EventHandler<ReadProgressEventArgs> eventHandler = this.m_ReadProgress;
			EventHandler<ReadProgressEventArgs> eventHandler2;
			do
			{
				eventHandler2 = eventHandler;
				eventHandler = Interlocked.CompareExchange(ref this.m_ReadProgress, (EventHandler<ReadProgressEventArgs>)Delegate.Combine(eventHandler2, value), eventHandler);
			}
			while (eventHandler != eventHandler2);
		}
		remove
		{
			EventHandler<ReadProgressEventArgs> eventHandler = this.m_ReadProgress;
			EventHandler<ReadProgressEventArgs> eventHandler2;
			do
			{
				eventHandler2 = eventHandler;
				eventHandler = Interlocked.CompareExchange(ref this.m_ReadProgress, (EventHandler<ReadProgressEventArgs>)Delegate.Remove(eventHandler2, value), eventHandler);
			}
			while (eventHandler != eventHandler2);
		}
	}

	public event EventHandler<ExtractProgressEventArgs> ExtractProgress
	{
		add
		{
			EventHandler<ExtractProgressEventArgs> eventHandler = this.m_ExtractProgress;
			EventHandler<ExtractProgressEventArgs> eventHandler2;
			do
			{
				eventHandler2 = eventHandler;
				eventHandler = Interlocked.CompareExchange(ref this.m_ExtractProgress, (EventHandler<ExtractProgressEventArgs>)Delegate.Combine(eventHandler2, value), eventHandler);
			}
			while (eventHandler != eventHandler2);
		}
		remove
		{
			EventHandler<ExtractProgressEventArgs> eventHandler = this.m_ExtractProgress;
			EventHandler<ExtractProgressEventArgs> eventHandler2;
			do
			{
				eventHandler2 = eventHandler;
				eventHandler = Interlocked.CompareExchange(ref this.m_ExtractProgress, (EventHandler<ExtractProgressEventArgs>)Delegate.Remove(eventHandler2, value), eventHandler);
			}
			while (eventHandler != eventHandler2);
		}
	}

	public event EventHandler<AddProgressEventArgs> AddProgress
	{
		add
		{
			EventHandler<AddProgressEventArgs> eventHandler = this.m_AddProgress;
			EventHandler<AddProgressEventArgs> eventHandler2;
			do
			{
				eventHandler2 = eventHandler;
				eventHandler = Interlocked.CompareExchange(ref this.m_AddProgress, (EventHandler<AddProgressEventArgs>)Delegate.Combine(eventHandler2, value), eventHandler);
			}
			while (eventHandler != eventHandler2);
		}
		remove
		{
			EventHandler<AddProgressEventArgs> eventHandler = this.m_AddProgress;
			EventHandler<AddProgressEventArgs> eventHandler2;
			do
			{
				eventHandler2 = eventHandler;
				eventHandler = Interlocked.CompareExchange(ref this.m_AddProgress, (EventHandler<AddProgressEventArgs>)Delegate.Remove(eventHandler2, value), eventHandler);
			}
			while (eventHandler != eventHandler2);
		}
	}

	public event EventHandler<ZipErrorEventArgs> ZipError
	{
		add
		{
			EventHandler<ZipErrorEventArgs> eventHandler = this.m_ZipError;
			EventHandler<ZipErrorEventArgs> eventHandler2;
			do
			{
				eventHandler2 = eventHandler;
				eventHandler = Interlocked.CompareExchange(ref this.m_ZipError, (EventHandler<ZipErrorEventArgs>)Delegate.Combine(eventHandler2, value), eventHandler);
			}
			while (eventHandler != eventHandler2);
		}
		remove
		{
			EventHandler<ZipErrorEventArgs> eventHandler = this.m_ZipError;
			EventHandler<ZipErrorEventArgs> eventHandler2;
			do
			{
				eventHandler2 = eventHandler;
				eventHandler = Interlocked.CompareExchange(ref this.m_ZipError, (EventHandler<ZipErrorEventArgs>)Delegate.Remove(eventHandler2, value), eventHandler);
			}
			while (eventHandler != eventHandler2);
		}
	}

	public ZipFile(string fileName)
	{
		try
		{
			_InitInstance(fileName, null);
		}
		catch (Exception innerException)
		{
			throw new ZipException($"Could not read {fileName} as a zip file", innerException);
		}
	}

	public ZipFile(string fileName, Encoding encoding)
	{
		try
		{
			AlternateEncoding = encoding;
			AlternateEncodingUsage = ZipOption.Always;
			_InitInstance(fileName, null);
		}
		catch (Exception innerException)
		{
			throw new ZipException($"{fileName} is not a valid zip file", innerException);
		}
	}

	public ZipFile()
	{
		_InitInstance(null, null);
	}

	public ZipFile(Encoding encoding)
	{
		AlternateEncoding = encoding;
		AlternateEncodingUsage = ZipOption.Always;
		_InitInstance(null, null);
	}

	public ZipFile(string fileName, TextWriter statusMessageWriter)
	{
		try
		{
			_InitInstance(fileName, statusMessageWriter);
		}
		catch (Exception innerException)
		{
			throw new ZipException($"{fileName} is not a valid zip file", innerException);
		}
	}

	public ZipFile(string fileName, TextWriter statusMessageWriter, Encoding encoding)
	{
		try
		{
			AlternateEncoding = encoding;
			AlternateEncodingUsage = ZipOption.Always;
			_InitInstance(fileName, statusMessageWriter);
		}
		catch (Exception innerException)
		{
			throw new ZipException($"{fileName} is not a valid zip file", innerException);
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public ZipEntry AddItem(string fileOrDirectoryName)
	{
		return AddItem(fileOrDirectoryName, null);
	}

	public ZipEntry AddItem(string fileOrDirectoryName, string directoryPathInArchive)
	{
		if (File.Exists(fileOrDirectoryName))
		{
			return AddFile(fileOrDirectoryName, directoryPathInArchive);
		}
		if (Directory.Exists(fileOrDirectoryName))
		{
			return AddDirectory(fileOrDirectoryName, directoryPathInArchive);
		}
		throw new FileNotFoundException($"That file or directory ({fileOrDirectoryName}) does not exist!");
	}

	public ZipEntry AddFile(string fileName)
	{
		return AddFile(fileName, null);
	}

	public ZipEntry AddFile(string fileName, string directoryPathInArchive)
	{
		string nameInArchive = ZipEntry.NameInArchive(fileName, directoryPathInArchive);
		ZipEntry ze = ZipEntry.CreateFromFile(fileName, nameInArchive);
		if (Verbose)
		{
			StatusMessageTextWriter.WriteLine("adding {0}...", fileName);
		}
		return _InternalAddEntry(ze);
	}

	public void RemoveEntries(ICollection<ZipEntry> entriesToRemove)
	{
		if (entriesToRemove == null)
		{
			throw new ArgumentNullException("entriesToRemove");
		}
		foreach (ZipEntry item in entriesToRemove)
		{
			RemoveEntry(item);
		}
	}

	public void RemoveEntries(ICollection<string> entriesToRemove)
	{
		if (entriesToRemove == null)
		{
			throw new ArgumentNullException("entriesToRemove");
		}
		foreach (string item in entriesToRemove)
		{
			RemoveEntry(item);
		}
	}

	public void AddFiles(IEnumerable<string> fileNames)
	{
		AddFiles(fileNames, null);
	}

	public void UpdateFiles(IEnumerable<string> fileNames)
	{
		UpdateFiles(fileNames, null);
	}

	public void AddFiles(IEnumerable<string> fileNames, string directoryPathInArchive)
	{
		AddFiles(fileNames, preserveDirHierarchy: false, directoryPathInArchive);
	}

	public void AddFiles(IEnumerable<string> fileNames, bool preserveDirHierarchy, string directoryPathInArchive)
	{
		if (fileNames == null)
		{
			throw new ArgumentNullException("fileNames");
		}
		_addOperationCanceled = false;
		OnAddStarted();
		if (preserveDirHierarchy)
		{
			foreach (string fileName in fileNames)
			{
				if (_addOperationCanceled)
				{
					break;
				}
				if (directoryPathInArchive != null)
				{
					string fullPath = Path.GetFullPath(Path.Combine(directoryPathInArchive, Path.GetDirectoryName(fileName)));
					AddFile(fileName, fullPath);
				}
				else
				{
					AddFile(fileName, null);
				}
			}
		}
		else
		{
			foreach (string fileName2 in fileNames)
			{
				if (_addOperationCanceled)
				{
					break;
				}
				AddFile(fileName2, directoryPathInArchive);
			}
		}
		if (!_addOperationCanceled)
		{
			OnAddCompleted();
		}
	}

	public void UpdateFiles(IEnumerable<string> fileNames, string directoryPathInArchive)
	{
		if (fileNames == null)
		{
			throw new ArgumentNullException("fileNames");
		}
		OnAddStarted();
		foreach (string fileName in fileNames)
		{
			UpdateFile(fileName, directoryPathInArchive);
		}
		OnAddCompleted();
	}

	public ZipEntry UpdateFile(string fileName)
	{
		return UpdateFile(fileName, null);
	}

	public ZipEntry UpdateFile(string fileName, string directoryPathInArchive)
	{
		string fileName2 = ZipEntry.NameInArchive(fileName, directoryPathInArchive);
		if (this[fileName2] != null)
		{
			RemoveEntry(fileName2);
		}
		return AddFile(fileName, directoryPathInArchive);
	}

	public ZipEntry UpdateDirectory(string directoryName)
	{
		return UpdateDirectory(directoryName, null);
	}

	public ZipEntry UpdateDirectory(string directoryName, string directoryPathInArchive)
	{
		return AddOrUpdateDirectoryImpl(directoryName, directoryPathInArchive, AddOrUpdateAction.AddOrUpdate);
	}

	public void UpdateItem(string itemName)
	{
		UpdateItem(itemName, null);
	}

	public void UpdateItem(string itemName, string directoryPathInArchive)
	{
		if (File.Exists(itemName))
		{
			UpdateFile(itemName, directoryPathInArchive);
			return;
		}
		if (Directory.Exists(itemName))
		{
			UpdateDirectory(itemName, directoryPathInArchive);
			return;
		}
		throw new FileNotFoundException($"That file or directory ({itemName}) does not exist!");
	}

	public ZipEntry AddEntry(string entryName, string content)
	{
		return AddEntry(entryName, content, Encoding.UTF8);
	}

	public ZipEntry AddEntry(string entryName, string content, Encoding encoding)
	{
		MemoryStream memoryStream = new MemoryStream();
		StreamWriter streamWriter = new StreamWriter(memoryStream, encoding);
		streamWriter.Write(content);
		streamWriter.Flush();
		memoryStream.Seek(0L, SeekOrigin.Begin);
		return AddEntry(entryName, memoryStream);
	}

	public ZipEntry AddEntry(string entryName, Stream stream)
	{
		ZipEntry zipEntry = ZipEntry.CreateForStream(entryName, stream);
		zipEntry.SetEntryTimes(DateTime.Now, DateTime.Now, DateTime.Now);
		if (Verbose)
		{
			StatusMessageTextWriter.WriteLine("adding {0}...", entryName);
		}
		return _InternalAddEntry(zipEntry);
	}

	public ZipEntry AddEntry(string entryName, WriteDelegate writer)
	{
		ZipEntry ze = ZipEntry.CreateForWriter(entryName, writer);
		if (Verbose)
		{
			StatusMessageTextWriter.WriteLine("adding {0}...", entryName);
		}
		return _InternalAddEntry(ze);
	}

	public ZipEntry AddEntry(string entryName, OpenDelegate opener, CloseDelegate closer)
	{
		ZipEntry zipEntry = ZipEntry.CreateForJitStreamProvider(entryName, opener, closer);
		zipEntry.SetEntryTimes(DateTime.Now, DateTime.Now, DateTime.Now);
		if (Verbose)
		{
			StatusMessageTextWriter.WriteLine("adding {0}...", entryName);
		}
		return _InternalAddEntry(zipEntry);
	}

	private ZipEntry _InternalAddEntry(ZipEntry ze)
	{
		ze._container = new ZipContainer(this);
		ze.CompressionMethod = CompressionMethod;
		ze.CompressionLevel = CompressionLevel;
		ze.ExtractExistingFile = ExtractExistingFile;
		ze.ZipErrorAction = ZipErrorAction;
		ze.SetCompression = SetCompression;
		ze.AlternateEncoding = AlternateEncoding;
		ze.AlternateEncodingUsage = AlternateEncodingUsage;
		ze.Password = _Password;
		ze.Encryption = Encryption;
		ze.EmitTimesInWindowsFormatWhenSaving = _emitNtfsTimes;
		ze.EmitTimesInUnixFormatWhenSaving = _emitUnixTimes;
		InternalAddEntry(ze.FileName, ze);
		AfterAddEntry(ze);
		return ze;
	}

	public ZipEntry UpdateEntry(string entryName, string content)
	{
		return UpdateEntry(entryName, content, Encoding.UTF8);
	}

	public ZipEntry UpdateEntry(string entryName, string content, Encoding encoding)
	{
		RemoveEntryForUpdate(entryName);
		return AddEntry(entryName, content, encoding);
	}

	public ZipEntry UpdateEntry(string entryName, WriteDelegate writer)
	{
		RemoveEntryForUpdate(entryName);
		return AddEntry(entryName, writer);
	}

	public ZipEntry UpdateEntry(string entryName, OpenDelegate opener, CloseDelegate closer)
	{
		RemoveEntryForUpdate(entryName);
		return AddEntry(entryName, opener, closer);
	}

	public ZipEntry UpdateEntry(string entryName, Stream stream)
	{
		RemoveEntryForUpdate(entryName);
		return AddEntry(entryName, stream);
	}

	private void RemoveEntryForUpdate(string entryName)
	{
		if (string.IsNullOrEmpty(entryName))
		{
			throw new ArgumentNullException("entryName");
		}
		string directoryPathInArchive = null;
		if (entryName.IndexOf('\\') != -1)
		{
			directoryPathInArchive = Path.GetDirectoryName(entryName);
			entryName = Path.GetFileName(entryName);
		}
		string fileName = ZipEntry.NameInArchive(entryName, directoryPathInArchive);
		if (this[fileName] != null)
		{
			RemoveEntry(fileName);
		}
	}

	public ZipEntry AddEntry(string entryName, byte[] byteContent)
	{
		if (byteContent == null)
		{
			throw new ArgumentException("bad argument", "byteContent");
		}
		MemoryStream stream = new MemoryStream(byteContent);
		return AddEntry(entryName, stream);
	}

	public ZipEntry UpdateEntry(string entryName, byte[] byteContent)
	{
		RemoveEntryForUpdate(entryName);
		return AddEntry(entryName, byteContent);
	}

	public ZipEntry AddDirectory(string directoryName)
	{
		return AddDirectory(directoryName, null);
	}

	public ZipEntry AddDirectory(string directoryName, string directoryPathInArchive)
	{
		return AddOrUpdateDirectoryImpl(directoryName, directoryPathInArchive, AddOrUpdateAction.AddOnly);
	}

	public ZipEntry AddDirectoryByName(string directoryNameInArchive)
	{
		ZipEntry zipEntry = ZipEntry.CreateFromNothing(directoryNameInArchive);
		zipEntry._container = new ZipContainer(this);
		zipEntry.MarkAsDirectory();
		zipEntry.AlternateEncoding = AlternateEncoding;
		zipEntry.AlternateEncodingUsage = AlternateEncodingUsage;
		zipEntry.SetEntryTimes(DateTime.Now, DateTime.Now, DateTime.Now);
		zipEntry.EmitTimesInWindowsFormatWhenSaving = _emitNtfsTimes;
		zipEntry.EmitTimesInUnixFormatWhenSaving = _emitUnixTimes;
		zipEntry._Source = ZipEntrySource.Stream;
		InternalAddEntry(zipEntry.FileName, zipEntry);
		AfterAddEntry(zipEntry);
		return zipEntry;
	}

	private ZipEntry AddOrUpdateDirectoryImpl(string directoryName, string rootDirectoryPathInArchive, AddOrUpdateAction action)
	{
		if (rootDirectoryPathInArchive == null)
		{
			rootDirectoryPathInArchive = string.Empty;
		}
		return AddOrUpdateDirectoryImpl(directoryName, rootDirectoryPathInArchive, action, recurse: true, 0);
	}

	internal void InternalAddEntry(string name, ZipEntry entry)
	{
		_entries.Add(name, entry);
		_zipEntriesAsList = null;
		_contentsChanged = true;
	}

	private ZipEntry AddOrUpdateDirectoryImpl(string directoryName, string rootDirectoryPathInArchive, AddOrUpdateAction action, bool recurse, int level)
	{
		if (Verbose)
		{
			StatusMessageTextWriter.WriteLine("{0} {1}...", (action != 0) ? "Adding or updating" : "adding", directoryName);
		}
		if (level == 0)
		{
			_addOperationCanceled = false;
			OnAddStarted();
		}
		if (_addOperationCanceled)
		{
			return null;
		}
		string text = rootDirectoryPathInArchive;
		ZipEntry zipEntry = null;
		if (level > 0)
		{
			int num = directoryName.Length;
			for (int num2 = level; num2 > 0; num2--)
			{
				num = directoryName.LastIndexOfAny("/\\".ToCharArray(), num - 1, num - 1);
			}
			text = directoryName.Substring(num + 1);
			text = Path.Combine(rootDirectoryPathInArchive, text);
		}
		if (level > 0 || rootDirectoryPathInArchive != string.Empty)
		{
			zipEntry = ZipEntry.CreateFromFile(directoryName, text);
			zipEntry._container = new ZipContainer(this);
			zipEntry.AlternateEncoding = AlternateEncoding;
			zipEntry.AlternateEncodingUsage = AlternateEncodingUsage;
			zipEntry.MarkAsDirectory();
			zipEntry.EmitTimesInWindowsFormatWhenSaving = _emitNtfsTimes;
			zipEntry.EmitTimesInUnixFormatWhenSaving = _emitUnixTimes;
			if (!_entries.ContainsKey(zipEntry.FileName))
			{
				InternalAddEntry(zipEntry.FileName, zipEntry);
				AfterAddEntry(zipEntry);
			}
			text = zipEntry.FileName;
		}
		if (!_addOperationCanceled)
		{
			string[] files = Directory.GetFiles(directoryName);
			if (recurse)
			{
				string[] array = files;
				foreach (string fileName in array)
				{
					if (_addOperationCanceled)
					{
						break;
					}
					if (action == AddOrUpdateAction.AddOnly)
					{
						AddFile(fileName, text);
					}
					else
					{
						UpdateFile(fileName, text);
					}
				}
				if (!_addOperationCanceled)
				{
					string[] directories = Directory.GetDirectories(directoryName);
					string[] array2 = directories;
					foreach (string directoryName2 in array2)
					{
						if (AddDirectoryWillTraverseReparsePoints)
						{
							AddOrUpdateDirectoryImpl(directoryName2, rootDirectoryPathInArchive, action, recurse, level + 1);
						}
					}
				}
			}
		}
		if (level == 0)
		{
			OnAddCompleted();
		}
		return zipEntry;
	}

	public static bool CheckZip(string zipFileName)
	{
		return CheckZip(zipFileName, fixIfNecessary: false, null);
	}

	public static bool CheckZip(string zipFileName, bool fixIfNecessary, TextWriter writer)
	{
		ZipFile zipFile = null;
		ZipFile zipFile2 = null;
		bool flag = true;
		try
		{
			zipFile = new ZipFile();
			zipFile.FullScan = true;
			zipFile.Initialize(zipFileName);
			zipFile2 = Read(zipFileName);
			foreach (ZipEntry item in zipFile)
			{
				foreach (ZipEntry item2 in zipFile2)
				{
					if (item.FileName == item2.FileName)
					{
						if (item._RelativeOffsetOfLocalHeader != item2._RelativeOffsetOfLocalHeader)
						{
							flag = false;
							writer?.WriteLine("{0}: mismatch in RelativeOffsetOfLocalHeader  (0x{1:X16} != 0x{2:X16})", item.FileName, item._RelativeOffsetOfLocalHeader, item2._RelativeOffsetOfLocalHeader);
						}
						if (item._CompressedSize != item2._CompressedSize)
						{
							flag = false;
							writer?.WriteLine("{0}: mismatch in CompressedSize  (0x{1:X16} != 0x{2:X16})", item.FileName, item._CompressedSize, item2._CompressedSize);
						}
						if (item._UncompressedSize != item2._UncompressedSize)
						{
							flag = false;
							writer?.WriteLine("{0}: mismatch in UncompressedSize  (0x{1:X16} != 0x{2:X16})", item.FileName, item._UncompressedSize, item2._UncompressedSize);
						}
						if (item.CompressionMethod != item2.CompressionMethod)
						{
							flag = false;
							writer?.WriteLine("{0}: mismatch in CompressionMethod  (0x{1:X4} != 0x{2:X4})", item.FileName, item.CompressionMethod, item2.CompressionMethod);
						}
						if (item.Crc != item2.Crc)
						{
							flag = false;
							writer?.WriteLine("{0}: mismatch in Crc32  (0x{1:X4} != 0x{2:X4})", item.FileName, item.Crc, item2.Crc);
						}
						break;
					}
				}
			}
			zipFile2.Dispose();
			zipFile2 = null;
			if (!flag && fixIfNecessary)
			{
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(zipFileName);
				fileNameWithoutExtension = $"{fileNameWithoutExtension}_fixed.zip";
				zipFile.Save(fileNameWithoutExtension);
			}
		}
		finally
		{
			zipFile?.Dispose();
			zipFile2?.Dispose();
		}
		return flag;
	}

	public static void FixZipDirectory(string zipFileName)
	{
		using ZipFile zipFile = new ZipFile();
		zipFile.FullScan = true;
		zipFile.Initialize(zipFileName);
		zipFile.Save(zipFileName);
	}

	public static bool CheckZipPassword(string zipFileName, string password)
	{
		bool result = false;
		try
		{
			using (ZipFile zipFile = Read(zipFileName))
			{
				foreach (ZipEntry item in zipFile)
				{
					if (!item.IsDirectory && item.UsesEncryption)
					{
						item.ExtractWithPassword(Stream.Null, password);
					}
				}
			}
			result = true;
		}
		catch (BadPasswordException)
		{
		}
		return result;
	}

	internal bool OnSaveBlock(ZipEntry entry, long bytesXferred, long totalBytesToXfer)
	{
		EventHandler<SaveProgressEventArgs> saveProgress = this.SaveProgress;
		if (saveProgress != null)
		{
			SaveProgressEventArgs saveProgressEventArgs = SaveProgressEventArgs.ByteUpdate(ArchiveNameForEvent, entry, bytesXferred, totalBytesToXfer);
			saveProgress(this, saveProgressEventArgs);
			if (saveProgressEventArgs.Cancel)
			{
				_saveOperationCanceled = true;
			}
		}
		return _saveOperationCanceled;
	}

	private void OnSaveEntry(int current, ZipEntry entry, bool before)
	{
		EventHandler<SaveProgressEventArgs> saveProgress = this.SaveProgress;
		if (saveProgress != null)
		{
			SaveProgressEventArgs saveProgressEventArgs = new SaveProgressEventArgs(ArchiveNameForEvent, before, _entries.Count, current, entry);
			saveProgress(this, saveProgressEventArgs);
			if (saveProgressEventArgs.Cancel)
			{
				_saveOperationCanceled = true;
			}
		}
	}

	private void OnSaveEvent(ZipProgressEventType eventFlavor)
	{
		EventHandler<SaveProgressEventArgs> saveProgress = this.SaveProgress;
		if (saveProgress != null)
		{
			SaveProgressEventArgs saveProgressEventArgs = new SaveProgressEventArgs(ArchiveNameForEvent, eventFlavor);
			saveProgress(this, saveProgressEventArgs);
			if (saveProgressEventArgs.Cancel)
			{
				_saveOperationCanceled = true;
			}
		}
	}

	private void OnSaveStarted()
	{
		EventHandler<SaveProgressEventArgs> saveProgress = this.SaveProgress;
		if (saveProgress != null)
		{
			SaveProgressEventArgs saveProgressEventArgs = SaveProgressEventArgs.Started(ArchiveNameForEvent);
			saveProgress(this, saveProgressEventArgs);
			if (saveProgressEventArgs.Cancel)
			{
				_saveOperationCanceled = true;
			}
		}
	}

	private void OnSaveCompleted()
	{
		EventHandler<SaveProgressEventArgs> saveProgress = this.SaveProgress;
		if (saveProgress != null)
		{
			SaveProgressEventArgs e = SaveProgressEventArgs.Completed(ArchiveNameForEvent);
			saveProgress(this, e);
		}
	}

	private void OnReadStarted()
	{
		EventHandler<ReadProgressEventArgs> readProgress = this.ReadProgress;
		if (readProgress != null)
		{
			ReadProgressEventArgs e = ReadProgressEventArgs.Started(ArchiveNameForEvent);
			readProgress(this, e);
		}
	}

	private void OnReadCompleted()
	{
		EventHandler<ReadProgressEventArgs> readProgress = this.ReadProgress;
		if (readProgress != null)
		{
			ReadProgressEventArgs e = ReadProgressEventArgs.Completed(ArchiveNameForEvent);
			readProgress(this, e);
		}
	}

	internal void OnReadBytes(ZipEntry entry)
	{
		EventHandler<ReadProgressEventArgs> readProgress = this.ReadProgress;
		if (readProgress != null)
		{
			ReadProgressEventArgs e = ReadProgressEventArgs.ByteUpdate(ArchiveNameForEvent, entry, ReadStream.Position, LengthOfReadStream);
			readProgress(this, e);
		}
	}

	internal void OnReadEntry(bool before, ZipEntry entry)
	{
		EventHandler<ReadProgressEventArgs> readProgress = this.ReadProgress;
		if (readProgress != null)
		{
			ReadProgressEventArgs e = ((!before) ? ReadProgressEventArgs.After(ArchiveNameForEvent, entry, _entries.Count) : ReadProgressEventArgs.Before(ArchiveNameForEvent, _entries.Count));
			readProgress(this, e);
		}
	}

	private void OnExtractEntry(int current, bool before, ZipEntry currentEntry, string path)
	{
		EventHandler<ExtractProgressEventArgs> extractProgress = this.ExtractProgress;
		if (extractProgress != null)
		{
			ExtractProgressEventArgs extractProgressEventArgs = new ExtractProgressEventArgs(ArchiveNameForEvent, before, _entries.Count, current, currentEntry, path);
			extractProgress(this, extractProgressEventArgs);
			if (extractProgressEventArgs.Cancel)
			{
				_extractOperationCanceled = true;
			}
		}
	}

	internal bool OnExtractBlock(ZipEntry entry, long bytesWritten, long totalBytesToWrite)
	{
		EventHandler<ExtractProgressEventArgs> extractProgress = this.ExtractProgress;
		if (extractProgress != null)
		{
			ExtractProgressEventArgs extractProgressEventArgs = ExtractProgressEventArgs.ByteUpdate(ArchiveNameForEvent, entry, bytesWritten, totalBytesToWrite);
			extractProgress(this, extractProgressEventArgs);
			if (extractProgressEventArgs.Cancel)
			{
				_extractOperationCanceled = true;
			}
		}
		return _extractOperationCanceled;
	}

	internal bool OnSingleEntryExtract(ZipEntry entry, string path, bool before)
	{
		EventHandler<ExtractProgressEventArgs> extractProgress = this.ExtractProgress;
		if (extractProgress != null)
		{
			ExtractProgressEventArgs extractProgressEventArgs = ((!before) ? ExtractProgressEventArgs.AfterExtractEntry(ArchiveNameForEvent, entry, path) : ExtractProgressEventArgs.BeforeExtractEntry(ArchiveNameForEvent, entry, path));
			extractProgress(this, extractProgressEventArgs);
			if (extractProgressEventArgs.Cancel)
			{
				_extractOperationCanceled = true;
			}
		}
		return _extractOperationCanceled;
	}

	internal bool OnExtractExisting(ZipEntry entry, string path)
	{
		EventHandler<ExtractProgressEventArgs> extractProgress = this.ExtractProgress;
		if (extractProgress != null)
		{
			ExtractProgressEventArgs extractProgressEventArgs = ExtractProgressEventArgs.ExtractExisting(ArchiveNameForEvent, entry, path);
			extractProgress(this, extractProgressEventArgs);
			if (extractProgressEventArgs.Cancel)
			{
				_extractOperationCanceled = true;
			}
		}
		return _extractOperationCanceled;
	}

	private void OnExtractAllCompleted(string path)
	{
		EventHandler<ExtractProgressEventArgs> extractProgress = this.ExtractProgress;
		if (extractProgress != null)
		{
			ExtractProgressEventArgs e = ExtractProgressEventArgs.ExtractAllCompleted(ArchiveNameForEvent, path);
			extractProgress(this, e);
		}
	}

	private void OnExtractAllStarted(string path)
	{
		EventHandler<ExtractProgressEventArgs> extractProgress = this.ExtractProgress;
		if (extractProgress != null)
		{
			ExtractProgressEventArgs e = ExtractProgressEventArgs.ExtractAllStarted(ArchiveNameForEvent, path);
			extractProgress(this, e);
		}
	}

	private void OnAddStarted()
	{
		EventHandler<AddProgressEventArgs> addProgress = this.AddProgress;
		if (addProgress != null)
		{
			AddProgressEventArgs addProgressEventArgs = AddProgressEventArgs.Started(ArchiveNameForEvent);
			addProgress(this, addProgressEventArgs);
			if (addProgressEventArgs.Cancel)
			{
				_addOperationCanceled = true;
			}
		}
	}

	private void OnAddCompleted()
	{
		EventHandler<AddProgressEventArgs> addProgress = this.AddProgress;
		if (addProgress != null)
		{
			AddProgressEventArgs e = AddProgressEventArgs.Completed(ArchiveNameForEvent);
			addProgress(this, e);
		}
	}

	internal void AfterAddEntry(ZipEntry entry)
	{
		EventHandler<AddProgressEventArgs> addProgress = this.AddProgress;
		if (addProgress != null)
		{
			AddProgressEventArgs addProgressEventArgs = AddProgressEventArgs.AfterEntry(ArchiveNameForEvent, entry, _entries.Count);
			addProgress(this, addProgressEventArgs);
			if (addProgressEventArgs.Cancel)
			{
				_addOperationCanceled = true;
			}
		}
	}

	internal bool OnZipErrorSaving(ZipEntry entry, Exception exc)
	{
		if (this.ZipError != null)
		{
			lock (LOCK)
			{
				ZipErrorEventArgs zipErrorEventArgs = ZipErrorEventArgs.Saving(Name, entry, exc);
				this.ZipError(this, zipErrorEventArgs);
				if (zipErrorEventArgs.Cancel)
				{
					_saveOperationCanceled = true;
				}
			}
		}
		return _saveOperationCanceled;
	}

	public void ExtractAll(string path)
	{
		_InternalExtractAll(path, overrideExtractExistingProperty: true);
	}

	public void ExtractAll(string path, ExtractExistingFileAction extractExistingFile)
	{
		ExtractExistingFile = extractExistingFile;
		_InternalExtractAll(path, overrideExtractExistingProperty: true);
	}

	private void _InternalExtractAll(string path, bool overrideExtractExistingProperty)
	{
		bool flag = Verbose;
		_inExtractAll = true;
		try
		{
			OnExtractAllStarted(path);
			int num = 0;
			foreach (ZipEntry value in _entries.Values)
			{
				if (flag)
				{
					StatusMessageTextWriter.WriteLine("\n{1,-22} {2,-8} {3,4}   {4,-8}  {0}", "Name", "Modified", "Size", "Ratio", "Packed");
					StatusMessageTextWriter.WriteLine(new string('-', 72));
					flag = false;
				}
				if (Verbose)
				{
					StatusMessageTextWriter.WriteLine("{1,-22} {2,-8} {3,4:F0}%   {4,-8} {0}", value.FileName, value.LastModified.ToString("yyyy-MM-dd HH:mm:ss"), value.UncompressedSize, value.CompressionRatio, value.CompressedSize);
					if (!string.IsNullOrEmpty(value.Comment))
					{
						StatusMessageTextWriter.WriteLine("  Comment: {0}", value.Comment);
					}
				}
				value.Password = _Password;
				OnExtractEntry(num, before: true, value, path);
				if (overrideExtractExistingProperty)
				{
					value.ExtractExistingFile = ExtractExistingFile;
				}
				value.Extract(path);
				num++;
				OnExtractEntry(num, before: false, value, path);
				if (_extractOperationCanceled)
				{
					break;
				}
			}
			if (_extractOperationCanceled)
			{
				return;
			}
			foreach (ZipEntry value2 in _entries.Values)
			{
				if (value2.IsDirectory || value2.FileName.EndsWith("/"))
				{
					string fileOrDirectory = ((!value2.FileName.StartsWith("/")) ? Path.Combine(path, value2.FileName) : Path.Combine(path, value2.FileName.Substring(1)));
					value2._SetTimes(fileOrDirectory, isFile: false);
				}
			}
			OnExtractAllCompleted(path);
		}
		finally
		{
			_inExtractAll = false;
		}
	}

	public static ZipFile Read(string fileName)
	{
		return Read(fileName, null, null, null);
	}

	public static ZipFile Read(string fileName, ReadOptions options)
	{
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		return Read(fileName, options.StatusMessageWriter, options.Encoding, options.ReadProgress);
	}

	private static ZipFile Read(string fileName, TextWriter statusMessageWriter, Encoding encoding, EventHandler<ReadProgressEventArgs> readProgress)
	{
		ZipFile zipFile = new ZipFile();
		zipFile.AlternateEncoding = encoding ?? DefaultEncoding;
		zipFile.AlternateEncodingUsage = ZipOption.Always;
		zipFile._StatusMessageTextWriter = statusMessageWriter;
		zipFile._name = fileName;
		if (readProgress != null)
		{
			zipFile.ReadProgress = readProgress;
		}
		if (zipFile.Verbose)
		{
			zipFile._StatusMessageTextWriter.WriteLine("reading from {0}...", fileName);
		}
		ReadIntoInstance(zipFile);
		zipFile._fileAlreadyExists = true;
		return zipFile;
	}

	public static ZipFile Read(Stream zipStream)
	{
		return Read(zipStream, null, null, null);
	}

	public static ZipFile Read(Stream zipStream, ReadOptions options)
	{
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		return Read(zipStream, options.StatusMessageWriter, options.Encoding, options.ReadProgress);
	}

	private static ZipFile Read(Stream zipStream, TextWriter statusMessageWriter, Encoding encoding, EventHandler<ReadProgressEventArgs> readProgress)
	{
		if (zipStream == null)
		{
			throw new ArgumentNullException("zipStream");
		}
		ZipFile zipFile = new ZipFile();
		zipFile._StatusMessageTextWriter = statusMessageWriter;
		zipFile._alternateEncoding = encoding ?? DefaultEncoding;
		zipFile._alternateEncodingUsage = ZipOption.Always;
		if (readProgress != null)
		{
			zipFile.ReadProgress += readProgress;
		}
		zipFile._readstream = ((zipStream.Position != 0) ? new OffsetStream(zipStream) : zipStream);
		zipFile._ReadStreamIsOurs = false;
		if (zipFile.Verbose)
		{
			zipFile._StatusMessageTextWriter.WriteLine("reading from stream...");
		}
		ReadIntoInstance(zipFile);
		return zipFile;
	}

	private static void ReadIntoInstance(ZipFile zf)
	{
		Stream readStream = zf.ReadStream;
		try
		{
			zf._readName = zf._name;
			if (!readStream.CanSeek)
			{
				ReadIntoInstance_Orig(zf);
				return;
			}
			zf.OnReadStarted();
			uint num = ReadFirstFourBytes(readStream);
			if (num == 101010256)
			{
				return;
			}
			int num2 = 0;
			bool flag = false;
			long num3 = readStream.Length - 64;
			long num4 = Math.Max(readStream.Length - 16384, 10L);
			do
			{
				if (num3 < 0)
				{
					num3 = 0L;
				}
				readStream.Seek(num3, SeekOrigin.Begin);
				long num5 = SharedUtilities.FindSignature(readStream, 101010256);
				if (num5 != -1)
				{
					flag = true;
					continue;
				}
				if (num3 == 0)
				{
					break;
				}
				num2++;
				num3 -= 32 * (num2 + 1) * num2;
			}
			while (!flag && num3 > num4);
			if (flag)
			{
				zf._locEndOfCDS = readStream.Position - 4;
				byte[] array = new byte[16];
				readStream.Read(array, 0, array.Length);
				zf._diskNumberWithCd = BitConverter.ToUInt16(array, 2);
				if (zf._diskNumberWithCd == 65535)
				{
					throw new ZipException("Spanned archives with more than 65534 segments are not supported at this time.");
				}
				zf._diskNumberWithCd++;
				int startIndex = 12;
				uint num6 = BitConverter.ToUInt32(array, startIndex);
				if (num6 == uint.MaxValue)
				{
					Zip64SeekToCentralDirectory(zf);
				}
				else
				{
					zf._OffsetOfCentralDirectory = num6;
					readStream.Seek(num6, SeekOrigin.Begin);
				}
				ReadCentralDirectory(zf);
			}
			else
			{
				readStream.Seek(0L, SeekOrigin.Begin);
				ReadIntoInstance_Orig(zf);
			}
		}
		catch (Exception innerException)
		{
			if (zf._ReadStreamIsOurs && zf._readstream != null)
			{
				try
				{
					zf._readstream.Dispose();
					zf._readstream = null;
				}
				finally
				{
				}
			}
			throw new ZipException("Cannot read that as a ZipFile", innerException);
		}
		zf._contentsChanged = false;
	}

	private static void Zip64SeekToCentralDirectory(ZipFile zf)
	{
		Stream readStream = zf.ReadStream;
		byte[] array = new byte[16];
		readStream.Seek(-40L, SeekOrigin.Current);
		readStream.Read(array, 0, 16);
		long num = BitConverter.ToInt64(array, 8);
		zf._OffsetOfCentralDirectory = uint.MaxValue;
		zf._OffsetOfCentralDirectory64 = num;
		readStream.Seek(num, SeekOrigin.Begin);
		uint num2 = (uint)SharedUtilities.ReadInt(readStream);
		if (num2 != 101075792)
		{
			throw new BadReadException($"  Bad signature (0x{num2:X8}) looking for ZIP64 EoCD Record at position 0x{readStream.Position:X8}");
		}
		readStream.Read(array, 0, 8);
		long num3 = BitConverter.ToInt64(array, 0);
		array = new byte[num3];
		readStream.Read(array, 0, array.Length);
		num = BitConverter.ToInt64(array, 36);
		readStream.Seek(num, SeekOrigin.Begin);
	}

	private static uint ReadFirstFourBytes(Stream s)
	{
		return (uint)SharedUtilities.ReadInt(s);
	}

	private static void ReadCentralDirectory(ZipFile zf)
	{
		bool flag = false;
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		ZipEntry zipEntry;
		while ((zipEntry = ZipEntry.ReadDirEntry(zf, dictionary)) != null)
		{
			zipEntry.ResetDirEntry();
			zf.OnReadEntry(before: true, null);
			if (zf.Verbose)
			{
				zf.StatusMessageTextWriter.WriteLine("entry {0}", zipEntry.FileName);
			}
			zf._entries.Add(zipEntry.FileName, zipEntry);
			if (zipEntry._InputUsesZip64)
			{
				flag = true;
			}
			dictionary.Add(zipEntry.FileName, null);
		}
		if (flag)
		{
			zf.UseZip64WhenSaving = Zip64Option.Always;
		}
		if (zf._locEndOfCDS > 0)
		{
			zf.ReadStream.Seek(zf._locEndOfCDS, SeekOrigin.Begin);
		}
		ReadCentralDirectoryFooter(zf);
		if (zf.Verbose && !string.IsNullOrEmpty(zf.Comment))
		{
			zf.StatusMessageTextWriter.WriteLine("Zip file Comment: {0}", zf.Comment);
		}
		if (zf.Verbose)
		{
			zf.StatusMessageTextWriter.WriteLine("read in {0} entries.", zf._entries.Count);
		}
		zf.OnReadCompleted();
	}

	private static void ReadIntoInstance_Orig(ZipFile zf)
	{
		zf.OnReadStarted();
		zf._entries = new Dictionary<string, ZipEntry>();
		if (zf.Verbose)
		{
			if (zf.Name == null)
			{
				zf.StatusMessageTextWriter.WriteLine("Reading zip from stream...");
			}
			else
			{
				zf.StatusMessageTextWriter.WriteLine("Reading zip {0}...", zf.Name);
			}
		}
		bool first = true;
		ZipContainer zc = new ZipContainer(zf);
		ZipEntry zipEntry;
		while ((zipEntry = ZipEntry.ReadEntry(zc, first)) != null)
		{
			if (zf.Verbose)
			{
				zf.StatusMessageTextWriter.WriteLine("  {0}", zipEntry.FileName);
			}
			zf._entries.Add(zipEntry.FileName, zipEntry);
			first = false;
		}
		try
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			ZipEntry zipEntry2;
			while ((zipEntry2 = ZipEntry.ReadDirEntry(zf, dictionary)) != null)
			{
				ZipEntry zipEntry3 = zf._entries[zipEntry2.FileName];
				if (zipEntry3 != null)
				{
					zipEntry3._Comment = zipEntry2.Comment;
					if (zipEntry2.IsDirectory)
					{
						zipEntry3.MarkAsDirectory();
					}
				}
				dictionary.Add(zipEntry2.FileName, null);
			}
			if (zf._locEndOfCDS > 0)
			{
				zf.ReadStream.Seek(zf._locEndOfCDS, SeekOrigin.Begin);
			}
			ReadCentralDirectoryFooter(zf);
			if (zf.Verbose && !string.IsNullOrEmpty(zf.Comment))
			{
				zf.StatusMessageTextWriter.WriteLine("Zip file Comment: {0}", zf.Comment);
			}
		}
		catch (ZipException)
		{
		}
		catch (IOException)
		{
		}
		zf.OnReadCompleted();
	}

	private static void ReadCentralDirectoryFooter(ZipFile zf)
	{
		Stream readStream = zf.ReadStream;
		int num = SharedUtilities.ReadSignature(readStream);
		byte[] array = null;
		int num2 = 0;
		if ((long)num == 101075792)
		{
			array = new byte[52];
			readStream.Read(array, 0, array.Length);
			long num3 = BitConverter.ToInt64(array, 0);
			if (num3 < 44)
			{
				throw new ZipException("Bad size in the ZIP64 Central Directory.");
			}
			zf._versionMadeBy = BitConverter.ToUInt16(array, num2);
			num2 += 2;
			zf._versionNeededToExtract = BitConverter.ToUInt16(array, num2);
			num2 += 2;
			zf._diskNumberWithCd = BitConverter.ToUInt32(array, num2);
			num2 += 2;
			array = new byte[num3 - 44];
			readStream.Read(array, 0, array.Length);
			num = SharedUtilities.ReadSignature(readStream);
			if ((long)num != 117853008)
			{
				throw new ZipException("Inconsistent metadata in the ZIP64 Central Directory.");
			}
			array = new byte[16];
			readStream.Read(array, 0, array.Length);
			num = SharedUtilities.ReadSignature(readStream);
		}
		if ((long)num != 101010256)
		{
			readStream.Seek(-4L, SeekOrigin.Current);
			throw new BadReadException($"Bad signature ({num:X8}) at position 0x{readStream.Position:X8}");
		}
		array = new byte[16];
		zf.ReadStream.Read(array, 0, array.Length);
		if (zf._diskNumberWithCd == 0)
		{
			zf._diskNumberWithCd = BitConverter.ToUInt16(array, 2);
		}
		ReadZipFileComment(zf);
	}

	private static void ReadZipFileComment(ZipFile zf)
	{
		byte[] array = new byte[2];
		zf.ReadStream.Read(array, 0, array.Length);
		short num = (short)(array[0] + array[1] * 256);
		if (num > 0)
		{
			array = new byte[num];
			zf.ReadStream.Read(array, 0, array.Length);
			string @string = zf.AlternateEncoding.GetString(array, 0, array.Length);
			zf.Comment = @string;
		}
	}

	public static bool IsZipFile(string fileName)
	{
		return IsZipFile(fileName, testExtract: false);
	}

	public static bool IsZipFile(string fileName, bool testExtract)
	{
		bool result = false;
		try
		{
			if (!File.Exists(fileName))
			{
				return false;
			}
			using FileStream stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			result = IsZipFile(stream, testExtract);
		}
		catch (IOException)
		{
		}
		catch (ZipException)
		{
		}
		return result;
	}

	public static bool IsZipFile(Stream stream, bool testExtract)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		bool result = false;
		try
		{
			if (!stream.CanRead)
			{
				return false;
			}
			Stream @null = Stream.Null;
			using (ZipFile zipFile = Read(stream, null, null, null))
			{
				if (testExtract)
				{
					foreach (ZipEntry item in zipFile)
					{
						if (!item.IsDirectory)
						{
							item.Extract(@null);
						}
					}
				}
			}
			result = true;
		}
		catch (IOException)
		{
		}
		catch (ZipException)
		{
		}
		return result;
	}

	private void DeleteFileWithRetry(string filename)
	{
		bool flag = false;
		int num = 3;
		for (int i = 0; i < num; i++)
		{
			if (flag)
			{
				break;
			}
			try
			{
				File.Delete(filename);
				flag = true;
			}
			catch (UnauthorizedAccessException)
			{
				Console.WriteLine("************************************************** Retry delete.");
				Thread.Sleep(200 + i * 200);
			}
		}
	}

	public void Save()
	{
		try
		{
			bool flag = false;
			_saveOperationCanceled = false;
			_numberOfSegmentsForMostRecentSave = 0u;
			OnSaveStarted();
			if (WriteStream == null)
			{
				throw new BadStateException("You haven't specified where to save the zip.");
			}
			if (_name != null && _name.EndsWith(".exe") && !_SavingSfx)
			{
				throw new BadStateException("You specified an EXE for a plain zip file.");
			}
			if (!_contentsChanged)
			{
				OnSaveCompleted();
				if (Verbose)
				{
					StatusMessageTextWriter.WriteLine("No save is necessary....");
				}
				return;
			}
			Reset(whileSaving: true);
			if (Verbose)
			{
				StatusMessageTextWriter.WriteLine("saving....");
			}
			if (_entries.Count >= 65535 && _zip64 == Zip64Option.Default)
			{
				throw new ZipException("The number of entries is 65535 or greater. Consider setting the UseZip64WhenSaving property on the ZipFile instance.");
			}
			int num = 0;
			ICollection<ZipEntry> collection;
			if (SortEntriesBeforeSaving)
			{
				ICollection<ZipEntry> entriesSorted = EntriesSorted;
				collection = entriesSorted;
			}
			else
			{
				collection = Entries;
			}
			ICollection<ZipEntry> collection2 = collection;
			foreach (ZipEntry item in collection2)
			{
				OnSaveEntry(num, item, before: true);
				item.Write(WriteStream);
				if (_saveOperationCanceled)
				{
					break;
				}
				num++;
				OnSaveEntry(num, item, before: false);
				if (_saveOperationCanceled)
				{
					break;
				}
				if (item.IncludedInMostRecentSave)
				{
					flag |= item.OutputUsedZip64.Value;
				}
			}
			if (_saveOperationCanceled)
			{
				return;
			}
			ZipSegmentedStream zipSegmentedStream = WriteStream as ZipSegmentedStream;
			_numberOfSegmentsForMostRecentSave = zipSegmentedStream?.CurrentSegment ?? 1;
			bool flag2 = ZipOutput.WriteCentralDirectoryStructure(WriteStream, collection2, _numberOfSegmentsForMostRecentSave, _zip64, Comment, new ZipContainer(this));
			OnSaveEvent(ZipProgressEventType.Saving_AfterSaveTempArchive);
			_hasBeenSaved = true;
			_contentsChanged = false;
			flag = flag || flag2;
			_OutputUsesZip64 = flag;
			if (_name != null && (_temporaryFileName != null || zipSegmentedStream != null))
			{
				WriteStream.Dispose();
				if (_saveOperationCanceled)
				{
					return;
				}
				if (_fileAlreadyExists && _readstream != null)
				{
					_readstream.Close();
					_readstream = null;
					foreach (ZipEntry item2 in collection2)
					{
						if (item2._archiveStream is ZipSegmentedStream zipSegmentedStream2)
						{
							zipSegmentedStream2.Dispose();
						}
						item2._archiveStream = null;
					}
				}
				string text = null;
				if (File.Exists(_name))
				{
					text = _name + "." + SharedUtilities.GenerateRandomStringImpl(8, 0) + ".tmp";
					if (File.Exists(text))
					{
						DeleteFileWithRetry(text);
					}
					File.Move(_name, text);
				}
				OnSaveEvent(ZipProgressEventType.Saving_BeforeRenameTempArchive);
				File.Move((zipSegmentedStream == null) ? _temporaryFileName : zipSegmentedStream.CurrentTempName, _name);
				OnSaveEvent(ZipProgressEventType.Saving_AfterRenameTempArchive);
				if (text != null)
				{
					try
					{
						if (File.Exists(text))
						{
							File.Delete(text);
						}
					}
					catch
					{
					}
				}
				_fileAlreadyExists = true;
			}
			NotifyEntriesSaveComplete(collection2);
			OnSaveCompleted();
			_JustSaved = true;
		}
		finally
		{
			CleanupAfterSaveOperation();
		}
	}

	private static void NotifyEntriesSaveComplete(ICollection<ZipEntry> c)
	{
		foreach (ZipEntry item in c)
		{
			item.NotifySaveComplete();
		}
	}

	private void RemoveTempFile()
	{
		try
		{
			if (File.Exists(_temporaryFileName))
			{
				File.Delete(_temporaryFileName);
			}
		}
		catch (IOException ex)
		{
			if (Verbose)
			{
				StatusMessageTextWriter.WriteLine("ZipFile::Save: could not delete temp file: {0}.", ex.Message);
			}
		}
	}

	private void CleanupAfterSaveOperation()
	{
		if (_name == null)
		{
			return;
		}
		if (_writestream != null)
		{
			try
			{
				_writestream.Dispose();
			}
			catch (IOException)
			{
			}
		}
		_writestream = null;
		if (_temporaryFileName != null)
		{
			RemoveTempFile();
			_temporaryFileName = null;
		}
	}

	public void Save(string fileName)
	{
		if (_name == null)
		{
			_writestream = null;
		}
		else
		{
			_readName = _name;
		}
		_name = fileName;
		if (Directory.Exists(_name))
		{
			throw new ZipException("Bad Directory", new ArgumentException("That name specifies an existing directory. Please specify a filename.", "fileName"));
		}
		_contentsChanged = true;
		_fileAlreadyExists = File.Exists(_name);
		Save();
	}

	public void Save(Stream outputStream)
	{
		if (outputStream == null)
		{
			throw new ArgumentNullException("outputStream");
		}
		if (!outputStream.CanWrite)
		{
			throw new ArgumentException("Must be a writable stream.", "outputStream");
		}
		_name = null;
		_writestream = new CountingStream(outputStream);
		_contentsChanged = true;
		_fileAlreadyExists = false;
		Save();
	}

	public void AddSelectedFiles(string selectionCriteria)
	{
		AddSelectedFiles(selectionCriteria, ".", null, recurseDirectories: false);
	}

	public void AddSelectedFiles(string selectionCriteria, bool recurseDirectories)
	{
		AddSelectedFiles(selectionCriteria, ".", null, recurseDirectories);
	}

	public void AddSelectedFiles(string selectionCriteria, string directoryOnDisk)
	{
		AddSelectedFiles(selectionCriteria, directoryOnDisk, null, recurseDirectories: false);
	}

	public void AddSelectedFiles(string selectionCriteria, string directoryOnDisk, bool recurseDirectories)
	{
		AddSelectedFiles(selectionCriteria, directoryOnDisk, null, recurseDirectories);
	}

	public void AddSelectedFiles(string selectionCriteria, string directoryOnDisk, string directoryPathInArchive)
	{
		AddSelectedFiles(selectionCriteria, directoryOnDisk, directoryPathInArchive, recurseDirectories: false);
	}

	public void AddSelectedFiles(string selectionCriteria, string directoryOnDisk, string directoryPathInArchive, bool recurseDirectories)
	{
		_AddOrUpdateSelectedFiles(selectionCriteria, directoryOnDisk, directoryPathInArchive, recurseDirectories, wantUpdate: false);
	}

	public void UpdateSelectedFiles(string selectionCriteria, string directoryOnDisk, string directoryPathInArchive, bool recurseDirectories)
	{
		_AddOrUpdateSelectedFiles(selectionCriteria, directoryOnDisk, directoryPathInArchive, recurseDirectories, wantUpdate: true);
	}

	private string EnsureendInSlash(string s)
	{
		if (s.EndsWith("\\"))
		{
			return s;
		}
		return s + "\\";
	}

	private void _AddOrUpdateSelectedFiles(string selectionCriteria, string directoryOnDisk, string directoryPathInArchive, bool recurseDirectories, bool wantUpdate)
	{
		if (directoryOnDisk == null && Directory.Exists(selectionCriteria))
		{
			directoryOnDisk = selectionCriteria;
			selectionCriteria = "*.*";
		}
		else if (string.IsNullOrEmpty(directoryOnDisk))
		{
			directoryOnDisk = ".";
		}
		while (directoryOnDisk.EndsWith("\\"))
		{
			directoryOnDisk = directoryOnDisk.Substring(0, directoryOnDisk.Length - 1);
		}
		if (Verbose)
		{
			StatusMessageTextWriter.WriteLine("adding selection '{0}' from dir '{1}'...", selectionCriteria, directoryOnDisk);
		}
		FileSelector fileSelector = new FileSelector(selectionCriteria, AddDirectoryWillTraverseReparsePoints);
		ReadOnlyCollection<string> readOnlyCollection = fileSelector.SelectFiles(directoryOnDisk, recurseDirectories);
		if (Verbose)
		{
			StatusMessageTextWriter.WriteLine("found {0} files...", readOnlyCollection.Count);
		}
		OnAddStarted();
		AddOrUpdateAction action = (wantUpdate ? AddOrUpdateAction.AddOrUpdate : AddOrUpdateAction.AddOnly);
		foreach (string item in readOnlyCollection)
		{
			string text = ((directoryPathInArchive != null) ? ReplaceLeadingDirectory(Path.GetDirectoryName(item), directoryOnDisk, directoryPathInArchive) : null);
			if (File.Exists(item))
			{
				if (wantUpdate)
				{
					UpdateFile(item, text);
				}
				else
				{
					AddFile(item, text);
				}
			}
			else
			{
				AddOrUpdateDirectoryImpl(item, text, action, recurse: false, 0);
			}
		}
		OnAddCompleted();
	}

	private static string ReplaceLeadingDirectory(string original, string pattern, string replacement)
	{
		string text = original.ToUpper();
		string text2 = pattern.ToUpper();
		if (text.IndexOf(text2) != 0)
		{
			return original;
		}
		return replacement + original.Substring(text2.Length);
	}

	public ICollection<ZipEntry> SelectEntries(string selectionCriteria)
	{
		FileSelector fileSelector = new FileSelector(selectionCriteria, AddDirectoryWillTraverseReparsePoints);
		return fileSelector.SelectEntries(this);
	}

	public ICollection<ZipEntry> SelectEntries(string selectionCriteria, string directoryPathInArchive)
	{
		FileSelector fileSelector = new FileSelector(selectionCriteria, AddDirectoryWillTraverseReparsePoints);
		return fileSelector.SelectEntries(this, directoryPathInArchive);
	}

	public int RemoveSelectedEntries(string selectionCriteria)
	{
		ICollection<ZipEntry> collection = SelectEntries(selectionCriteria);
		RemoveEntries(collection);
		return collection.Count;
	}

	public int RemoveSelectedEntries(string selectionCriteria, string directoryPathInArchive)
	{
		ICollection<ZipEntry> collection = SelectEntries(selectionCriteria, directoryPathInArchive);
		RemoveEntries(collection);
		return collection.Count;
	}

	public void ExtractSelectedEntries(string selectionCriteria)
	{
		foreach (ZipEntry item in SelectEntries(selectionCriteria))
		{
			item.Password = _Password;
			item.Extract();
		}
	}

	public void ExtractSelectedEntries(string selectionCriteria, ExtractExistingFileAction extractExistingFile)
	{
		foreach (ZipEntry item in SelectEntries(selectionCriteria))
		{
			item.Password = _Password;
			item.Extract(extractExistingFile);
		}
	}

	public void ExtractSelectedEntries(string selectionCriteria, string directoryPathInArchive)
	{
		foreach (ZipEntry item in SelectEntries(selectionCriteria, directoryPathInArchive))
		{
			item.Password = _Password;
			item.Extract();
		}
	}

	public void ExtractSelectedEntries(string selectionCriteria, string directoryInArchive, string extractDirectory)
	{
		foreach (ZipEntry item in SelectEntries(selectionCriteria, directoryInArchive))
		{
			item.Password = _Password;
			item.Extract(extractDirectory);
		}
	}

	public void ExtractSelectedEntries(string selectionCriteria, string directoryPathInArchive, string extractDirectory, ExtractExistingFileAction extractExistingFile)
	{
		foreach (ZipEntry item in SelectEntries(selectionCriteria, directoryPathInArchive))
		{
			item.Password = _Password;
			item.Extract(extractDirectory, extractExistingFile);
		}
	}

	public bool ContainsEntry(string name)
	{
		return _entries.ContainsKey(SharedUtilities.NormalizePathForUseInZipFile(name));
	}

	public override string ToString()
	{
		return $"ZipFile::{Name}";
	}

	internal void NotifyEntryChanged()
	{
		_contentsChanged = true;
	}

	internal Stream StreamForDiskNumber(uint diskNumber)
	{
		if (diskNumber + 1 == _diskNumberWithCd || (diskNumber == 0 && _diskNumberWithCd == 0))
		{
			return ReadStream;
		}
		return ZipSegmentedStream.ForReading(_readName ?? _name, diskNumber, _diskNumberWithCd);
	}

	internal void Reset(bool whileSaving)
	{
		if (!_JustSaved)
		{
			return;
		}
		using (ZipFile zipFile = new ZipFile())
		{
			zipFile._readName = (zipFile._name = ((!whileSaving) ? _name : (_readName ?? _name)));
			zipFile.AlternateEncoding = AlternateEncoding;
			zipFile.AlternateEncodingUsage = AlternateEncodingUsage;
			ReadIntoInstance(zipFile);
			foreach (ZipEntry item in zipFile)
			{
				using IEnumerator<ZipEntry> enumerator2 = GetEnumerator();
				while (enumerator2.MoveNext())
				{
					ZipEntry current2 = enumerator2.Current;
					if (item.FileName == current2.FileName)
					{
						current2.CopyMetaData(item);
						break;
					}
				}
			}
		}
		_JustSaved = false;
	}

	public void Initialize(string fileName)
	{
		try
		{
			_InitInstance(fileName, null);
		}
		catch (Exception innerException)
		{
			throw new ZipException($"{fileName} is not a valid zip file", innerException);
		}
	}

	private void _initEntriesDictionary()
	{
		StringComparer comparer = ((!CaseSensitiveRetrieval) ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
		_entries = ((_entries != null) ? new Dictionary<string, ZipEntry>(_entries, comparer) : new Dictionary<string, ZipEntry>(comparer));
	}

	private void _InitInstance(string zipFileName, TextWriter statusMessageWriter)
	{
		_name = zipFileName;
		_StatusMessageTextWriter = statusMessageWriter;
		_contentsChanged = true;
		AddDirectoryWillTraverseReparsePoints = true;
		CompressionLevel = CompressionLevel.Default;
		ParallelDeflateThreshold = 524288L;
		_initEntriesDictionary();
		if (zipFileName != null && File.Exists(_name))
		{
			if (FullScan)
			{
				ReadIntoInstance_Orig(this);
			}
			else
			{
				ReadIntoInstance(this);
			}
			_fileAlreadyExists = true;
		}
	}

	public void RemoveEntry(ZipEntry entry)
	{
		if (entry == null)
		{
			throw new ArgumentNullException("entry");
		}
		_entries.Remove(SharedUtilities.NormalizePathForUseInZipFile(entry.FileName));
		_zipEntriesAsList = null;
		_contentsChanged = true;
	}

	public void RemoveEntry(string fileName)
	{
		string fileName2 = ZipEntry.NameInArchive(fileName, null);
		ZipEntry zipEntry = this[fileName2];
		if (zipEntry == null)
		{
			throw new ArgumentException("The entry you specified was not found in the zip archive.");
		}
		RemoveEntry(zipEntry);
	}

	public void Dispose()
	{
		Dispose(disposeManagedResources: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposeManagedResources)
	{
		if (_disposed)
		{
			return;
		}
		if (disposeManagedResources)
		{
			if (_ReadStreamIsOurs && _readstream != null)
			{
				_readstream.Dispose();
				_readstream = null;
			}
			if (_temporaryFileName != null && _name != null && _writestream != null)
			{
				_writestream.Dispose();
				_writestream = null;
			}
			if (ParallelDeflater != null)
			{
				ParallelDeflater.Dispose();
				ParallelDeflater = null;
			}
		}
		_disposed = true;
	}

	public IEnumerator<ZipEntry> GetEnumerator()
	{
		foreach (ZipEntry value in _entries.Values)
		{
			yield return value;
		}
	}

	[DispId(-4)]
	public IEnumerator GetNewEnum()
	{
		return GetEnumerator();
	}
}

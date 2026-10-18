using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Modio.Errors;

public class ErrorException : Error
{
	public readonly Exception Exception;

	public override string GetMessage()
	{
		return $"{base.GetMessage()}: {Exception}";
	}

	internal ErrorException(Exception exception, ErrorCode code)
		: base(code)
	{
		Exception = exception;
	}

	internal ErrorException(Exception exception)
		: base(ErrorCodeFromException(exception))
	{
		Exception = exception;
	}

	private static ErrorCode ErrorCodeFromException(Exception exception)
	{
		if (!(exception is UnauthorizedAccessException))
		{
			if (!(exception is DirectoryNotFoundException))
			{
				if (!(exception is FileNotFoundException))
				{
					if (!(exception is HttpRequestException))
					{
						if (exception is TaskCanceledException)
						{
							return ErrorCode.OPERATION_CANCELLED;
						}
						return ErrorCode.OPERATION_ERROR;
					}
					return ErrorCode.HTTP_EXCEPTION;
				}
				return ErrorCode.FILE_NOT_FOUND;
			}
			return ErrorCode.DIRECTORY_NOT_FOUND;
		}
		return ErrorCode.NO_PERMISSION;
	}
}

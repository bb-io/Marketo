using System;
namespace Apps.Marketo
{
	public class FetchFileResponse
	{
		public List<Error> Errors { get; set; }
		public string RequestId { get; set; }
		public List<FileResponse> Result { get; set; }
		public bool Success { get; set; }
		public List<string> Warnings { get; set; }
	}

	public class Error
	{
		public string Message { get; set; }
		public string Code { get; set; }
	}

	public class FileResponse
	{
		public string CreatedAt { get; set; }
		public string Description { get; set; }
		// public FileFolder Folder { get; set; }
		public int Id { get; set; }
		public string MimeType { get; set; }
		public string Name { get; set; }
		public int Size { get; set; }
		public string UpdatedAt { get; set; }
		public string Url { get; set; }
	}

	public class FileFolder
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Type { get; set; }
	}

	public class AuthResponse
	{
		public string AccessToken { get; set; }
		public string TokenType { get; set; }
		public int ExpiresIn { get; set; }
		public string Scope { get; set; }
	}
}


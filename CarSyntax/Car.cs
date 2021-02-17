using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace CarSyntax
{
    internal static class Car
    {
		internal const string ContentType = nameof(Car);

		internal const string FileExtension = ".car";
		[Export]
		[Name(ContentType)]
		[BaseDefinition("code")]
		internal static ContentTypeDefinition ContentTypeDefinition = null;
		[Export]
		[Name(ContentType + nameof(FileExtensionToContentTypeDefinition))]
		[ContentType(ContentType)]
		[FileExtension(FileExtension)]
		internal static FileExtensionToContentTypeDefinition FileExtensionToContentTypeDefinition = null;
	}
}

using System;
using System.IO;
using System.Web.Mvc;
using Cassette.Configuration;
using Cassette.IO;
using Cassette.Utilities;

namespace Cassette.Web
{
    public static class UrlHelperExtensions
    {
        /// <summary>
        /// Returns the Cassette cache-friendly URL for a file, such as an image.
        /// </summary>
        /// <param name="urlHelper">The page UrlHelper object.</param>
        /// <param name="applicationRelativeFilePath">The application relative file path of the file.</param>
        public static string CassetteFile(this UrlHelper urlHelper, string applicationRelativeFilePath)
        {
            applicationRelativeFilePath = PathUtilities.AppRelative(applicationRelativeFilePath);

            var settings = CassetteApplicationContainer.Application.Settings;
            var file = settings.SourceDirectory.GetFile(applicationRelativeFilePath);
            ThrowIfFileNotFound(applicationRelativeFilePath, file);
            ThrowIfCannotRequestRawFile(applicationRelativeFilePath, file, settings);

            using (var stream = file.OpenRead())
            {
                var hash = stream.ComputeSHA1Hash().ToHexString();
                return settings.UrlGenerator.CreateRawFileUrl(applicationRelativeFilePath, hash);
            }
        }

        static void ThrowIfCannotRequestRawFile(string applicationRelativeFilePath, IFile file, CassetteSettings settings)
        {
            if (settings.CanRequestRawFile(file.FullPath)) return;
            
            throw new Exception(
                string.Format(
                    "The file {0} cannot be requested. In CassetteConfiguration, use the settings.AllowRawFileAccess method to tell Cassette which files are safe to request.",
                    applicationRelativeFilePath
                )
            );
        }

        static void ThrowIfFileNotFound(string applicationRelativeFilePath, IFile file)
        {
            if (file.Exists) return;
            throw new FileNotFoundException(
                "Cannot find file " + applicationRelativeFilePath,
                applicationRelativeFilePath
            );
        }
    }
}
// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace Analyzer.Utilities
{
    /// <summary>
    /// Copied from Roslyn.Utilities.FileNameUtilities.
    /// Implements a few file name utilities that are needed by the compiler.
    /// In general the compiler is not supposed to understand the format of the paths.
    /// In rare cases it needs to check if a string is a valid file name or change the extension 
    /// (embedded resources, netmodules, output name).
    /// The APIs are intentionally limited to cover just these rare cases. Do not add more APIs.
    /// </summary>
    internal static class FileNameUtilities
    {
        internal const char DirectorySeparatorChar = '\\';
        internal const char AltDirectorySeparatorChar = '/';
        internal const char VolumeSeparatorChar = ':';

        /// <summary>
        /// Returns the offset in <paramref name="path"/> where the dot that starts an extension is, or -1 if the path doesn't have an extension.
        /// </summary>
        /// <remarks>
        /// Returns 0 for path ".goo".
        /// Returns -1 for path "goo.".
        /// </remarks>
        private static int IndexOfExtension(string? path)
        {
            if (path == null)
            {
                return -1;
            }

            int length = path.Length;
            int i = length;

            while (--i >= 0)
            {
                char c = path[i];
                if (c == '.')
                {
                    if (i != length - 1)
                    {
                        return i;
                    }

                    return -1;
                }

                if (c == DirectorySeparatorChar || c == AltDirectorySeparatorChar || c == VolumeSeparatorChar)
                {
                    break;
                }
            }

            return -1;
        }

        /// <summary>
        /// Returns an extension of the specified path string.
        /// </summary>
        /// <remarks>
        /// The same functionality as <see cref="System.IO.Path.GetExtension(string)"/> but doesn't throw an exception
        /// if there are invalid characters in the path.
        /// </remarks>
        [return: NotNullIfNotNull(parameterName: "path")]
        internal static string? GetExtension(string? path)
        {
            if (path == null)
            {
                return null;
            }

            int index = IndexOfExtension(path);
            return (index >= 0) ? path[index..] : string.Empty;
        }

        /// <summary>
        /// Removes extension from path.
        /// </summary>
        /// <remarks>
        /// Returns "goo" for path "goo.".
        /// Returns "goo.." for path "goo...".
        /// </remarks>
        [return: NotNullIfNotNull(parameterName: "path")]
        private static string? RemoveExtension(string? path)
        {
            if (path == null)
            {
                return null;
            }

            int index = IndexOfExtension(path);
            if (index >= 0)
            {
                return path.Substring(0, index);
            }

            // trim last ".", if present
            if (path.Length > 0 && path[^1] == '.')
            {
                return path[0..^1];
            }

            return path;
        }

        /// <summary>
        /// Returns the position in given path where the file name starts.
        /// </summary>
        /// <returns>-1 if path is null.</returns>
        internal static int IndexOfFileName(string? path)
        {
            if (path == null)
            {
                return -1;
            }

            for (int i = path.Length - 1; i >= 0; i--)
            {
                char ch = path[i];
                if (ch == DirectorySeparatorChar || ch == AltDirectorySeparatorChar || ch == VolumeSeparatorChar)
                {
                    return i + 1;
                }
            }

            return 0;
        }

        /// <summary>
        /// Get file name from path.
        /// </summary>
        /// <remarks>Unlike <see cref="System.IO.Path.GetFileName(string)"/> doesn't check for invalid path characters.</remarks>
        [return: NotNullIfNotNull(parameterName: "path")]
        internal static string? GetFileName(string? path, bool includeExtension = true)
        {
            int fileNameStart = IndexOfFileName(path);
            var fileName = (fileNameStart <= 0) ? path : path![fileNameStart..];
            return includeExtension ? fileName : RemoveExtension(fileName);
        }
    }
}

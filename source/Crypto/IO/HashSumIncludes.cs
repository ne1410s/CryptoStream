using System;

namespace Crypto.IO
{
    /// <summary>
    /// Items to include in the hash sum algorithm.
    /// </summary>
    [Flags]
    public enum HashSumIncludes
    {
        /// <summary>
        /// File contents.
        /// </summary>
        FileContents        = 0b00000000,

        /// <summary>
        /// Last modified dates (files only).
        /// </summary>
        FileTimestamp       = 0b00000001,

        /// <summary>
        /// Names of files and folders (excluding root).
        /// </summary>
        DirectoryStructure  = 0b00000010,

        /// <summary>
        /// Name of root folder.
        /// </summary>
        DirectoryRootName   = 0b00000100,
    }
}

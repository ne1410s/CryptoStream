// <copyright file="HashSumIncludes.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Crypt.IO
{
    using System;

    /// <summary>
    /// Items to include in the hash sum algorithm.
    /// </summary>
    [Flags]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Critical Code Smell",
        "S2346:Flags enumerations zero-value members should be named \"None\"",
        Justification = "Better match for natural behaviour")]
    public enum HashSumIncludes
    {
        /// <summary>
        /// File contents.
        /// </summary>
        FileContents = 0b00000000,

        /// <summary>
        /// Last modified dates (files only).
        /// </summary>
        FileTimestamp = 0b00000001,

        /// <summary>
        /// Names of files and folders (excluding root).
        /// </summary>
        DirectoryStructure = 0b00000010,

        /// <summary>
        /// Name of root folder.
        /// </summary>
        DirectoryRootName = 0b00000100,
    }
}

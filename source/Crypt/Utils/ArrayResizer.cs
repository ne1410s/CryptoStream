// <copyright file="ArrayResizer.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Crypt.Utils;

using System;

/// <inheritdoc cref="IArrayResizer"/>
public class ArrayResizer : IArrayResizer
{
    /// <inheritdoc/>
    public void Resize(ref byte[] bytes, int newSize)
        => Array.Resize(ref bytes, newSize);
}

// <copyright file="FluentExtensions.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace CryptoStream;

using System;

/// <summary>
/// Fluent extensions.
/// </summary>
public static class FluentExtensions
{
    /// <summary>
    /// Ensures the reference is not null.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="o">The item.</param>
    /// <returns>The original object.</returns>
    /// <exception cref="ArgumentNullException">If null.</exception>
    public static T NotNull<T>(this T o)
        => o ?? throw new ArgumentNullException(nameof(o));
}

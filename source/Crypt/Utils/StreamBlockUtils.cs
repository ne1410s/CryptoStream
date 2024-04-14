// <copyright file="StreamBlockUtils.cs" company="ne1410s">
// Copyright (c) ne1410s. All rights reserved.
// </copyright>

namespace Crypt.Utils;

using System;

/// <summary>
/// Utilities for working with blocks and streams.
/// </summary>
public static class StreamBlockUtils
{
    /// <summary>
    /// Gets the position of the first block that covers the base position.
    /// </summary>
    /// <param name="basePosition">The queried base position.</param>
    /// <param name="bufferLength">The block buffer length.</param>
    /// <param name="blockNo">The sequential block number.</param>
    /// <param name="remainder">The number of skippable bytes in the block before the requested
    /// position is reached.</param>
    /// <returns>The discrete block position.</returns>
    public static long BlockPosition(
        long basePosition, int bufferLength, out long blockNo, out int remainder)
    {
        blockNo = 1 + (long)Math.Floor((double)basePosition / bufferLength);
        var blockStart = bufferLength * (blockNo - 1);
        remainder = (int)(basePosition - blockStart);
        return blockStart;
    }

    /// <summary>
    /// Determines how much padding is required.
    /// </summary>
    /// <param name="current">The current length.</param>
    /// <param name="reserve">Maximum amount of additional space needed.</param>
    /// <returns>New pad size.</returns>
    public static long GetPadSize(long current, long reserve = 4096)
    {
        var size = (double)(current + reserve);
        var magnitude = Math.Min(6, $"{size * .1:N0}".Length);
        var roundToNearest = Math.Pow(10, magnitude);
        return (long)(Math.Ceiling(size / roundToNearest) * roundToNearest);
    }
}

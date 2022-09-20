using System;

namespace Crypt.Utils
{
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
    }
}

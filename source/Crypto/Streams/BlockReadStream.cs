using System;
using System.IO;
using System.Linq;
using Crypto.Utils;

namespace Crypto.Streams
{
    /// <summary>
    /// A file stream that enforces discrete positions and read operations according to the
    /// specified buffer length. Using this implementation directly is not generally recommended.
    /// It is provided to separate logical processing and to assist with testing of derived streams.
    /// </summary>
    public class BlockReadStream : ReadStream, ISimpleReadStream
    {
        private readonly IArrayResizer arrayResizer;

        /// <summary>
        /// The source buffer.
        /// </summary>
        protected readonly byte[] blockBuffer;

        /// <inheritdoc/>
        public string Uri { get; }

        /// <inheritdoc/>
        public int BufferLength => blockBuffer.Length;

        /// <summary>
        /// Creates a new file block read stream.
        /// </summary>
        /// <param name="fi">The source file.</param>
        /// <param name="bufferLength">The block buffer length.</param>
        /// <param name="resizer">An array resizer.</param>
        public BlockReadStream(FileInfo fi, int bufferLength = 32768, IArrayResizer resizer = null)
            : this(new FileStream(fi.FullName, FileMode.Open, FileAccess.Read), bufferLength, resizer)
        { }

        /// <summary>
        /// Creates a new file block stream.
        /// </summary>
        /// <param name="stream">The source stream.</param>
        /// <param name="bufferLength">The block buffer length.</param>
        /// <param name="resizer">An array resizer.</param>
        public BlockReadStream(Stream stream, int bufferLength = 32768, IArrayResizer resizer = null)
            : base(stream)
        {
            arrayResizer = resizer ?? new ArrayResizer();
            blockBuffer = new byte[bufferLength];
            Uri = Guid.NewGuid().ToString();
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            var bufLen = blockBuffer.Length;
            var receivedPoint = Position;
            var startPoint = StreamBlockUtils.BlockPosition(Position, bufLen, out var initBlock, out var remainder);
            if (remainder != 0)
            {
                Seek(startPoint, SeekOrigin.Begin);
            }

            var blocksToRead = Math.Ceiling((double)(remainder + count) / bufLen);
            var bytesWritten = 0;

            var iterPosition = receivedPoint;

            foreach (var blockNo in Enumerable.Range((int)initBlock, (int)(initBlock + blocksToRead - 1)))
            {
                var maxBlockRead = Math.Min(Length - iterPosition, bufLen);
                var iterRead = inner.Read(blockBuffer, 0, (int)maxBlockRead);
                var targetBuffer = MapBlock(blockBuffer, blockNo);
                for (var j = 0; j < iterRead
                    && offset + bytesWritten < buffer.Length
                    && bytesWritten < count
                    && remainder + j < targetBuffer.Length; j++)
                {
                    buffer[offset + bytesWritten++] = targetBuffer[remainder + j];
                }

                // Subsequent blocks are always remainder-zero
                remainder = 0;
                iterPosition = Position;
            }

            // Seek to final expected position
            var seekTo = Math.Min(receivedPoint + count, Length);
            Seek(seekTo, SeekOrigin.Begin);

            return bytesWritten;
        }

        /// <summary>
        /// Obtains a mapped block.
        /// </summary>
        /// <param name="sourceBuffer">The source buffer.</param>
        /// <param name="blockNo">The discrete block number.</param>
        /// <returns>Mapped bytes.</returns>
        protected virtual byte[] MapBlock(byte[] sourceBuffer, long blockNo)
        {
            var retVal = new byte[sourceBuffer.Length];
            Array.Copy(sourceBuffer, retVal, sourceBuffer.Length);
            return retVal;
        }

        /// <inheritdoc/>
        public byte[] Read()
        {
            var buffer = new byte[BufferLength];
            var actualRead = Read(buffer, 0, buffer.Length);
            if (actualRead < buffer.Length)
            {
                arrayResizer.Resize(ref buffer, actualRead);
            }

            return buffer;
        }

        /// <inheritdoc/>
        public virtual long Seek(long offset) => Seek(offset, SeekOrigin.Begin);
    }
}

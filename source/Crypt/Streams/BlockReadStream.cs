using System;
using System.IO;
using System.Linq;
using Crypt.Utils;

namespace Crypt.Streams
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
            var originalPosition = Position;
            Position = StreamBlockUtils.BlockPosition(Position, BufferLength, out var block1, out var remainder);
            var blockSpan = (int)Math.Ceiling((double)(remainder + count) / BufferLength);
            var totalBytesRead = 0;

            foreach (var blockIndex in Enumerable.Range(0, blockSpan))
            {
                var blockRead = inner.Read(blockBuffer, 0, BufferLength);
                var pertinentBlockRead = Math.Min(blockRead - remainder, count - totalBytesRead);
                pertinentBlockRead = (int)Math.Min((double)pertinentBlockRead, Length - (originalPosition + totalBytesRead));
                var mappedBlock = MapBlock(blockBuffer, block1 + blockIndex);
                Array.Copy(mappedBlock, remainder, buffer, totalBytesRead, pertinentBlockRead);
                totalBytesRead += pertinentBlockRead;
                remainder = 0;
            }

            Position = originalPosition + totalBytesRead;
            return totalBytesRead;
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

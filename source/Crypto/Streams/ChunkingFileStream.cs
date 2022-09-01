using System;
using System.IO;

namespace Crypto.Streams
{
    /// <summary>
    /// A file stream that enforces discrete positions and read operations according to the
    /// specified buffer length. Using this implementation directly is not generally recommended.
    /// It is provided to separate logical processing and to assist with testing of derived streams.
    /// </summary>
    public class ChunkingFileStream : ReadFileStream, ISimpleReadStream
    {
        /// <summary>
        /// The source buffer.
        /// </summary>
        protected readonly byte[] chunkBuffer;

        /// <inheritdoc/>
        public string Uri { get; }

        /// <inheritdoc/>
        public int BufferLength => chunkBuffer.Length;

        /// <summary>
        /// Creates a new chunking file read stream.
        /// </summary>
        /// <param name="fi">The source file.</param>
        /// <param name="chunkSize">The chunk size.</param>
        public ChunkingFileStream(FileInfo fi, int chunkSize = 32768)
            : this(new FileStream(fi.FullName, FileMode.Open, FileAccess.Read), chunkSize)
        { }

        /// <summary>
        /// Creates a new chunking file stream.
        /// </summary>
        /// <param name="stream">The source stream.</param>
        /// <param name="chunkSize">The chunk size.</param>
        public ChunkingFileStream(Stream stream, int chunkSize = 32768)
            : base(stream)
        {
            chunkBuffer = new byte[chunkSize];
            Uri = Guid.NewGuid().ToString();
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            var chunkSz = chunkBuffer.Length;
            var receivedPoint = Position;
            var startPoint = ChunkPosition(Position, chunkSz, out var initChunk, out var remainder);
            if (remainder != 0)
            {
                Seek(startPoint, SeekOrigin.Begin);
            }

            var chunksToRead = Math.Ceiling((double)(remainder + count) / chunkSz);
            var bytesWritten = 0;

            var iterPosition = receivedPoint;
            for (var chunkNo = initChunk; chunkNo < (initChunk + chunksToRead); chunkNo++)
            {
                var maxChunkRead = Math.Min(Length - iterPosition, chunkSz);
                var iterRead = inner.Read(chunkBuffer, 0, (int)maxChunkRead);
                if (iterRead == 0)
                {
                    break;
                }

                var targetBuffer = MapChunk(chunkBuffer, chunkNo);

                for (var j = 0; j < iterRead
                    && offset + bytesWritten < buffer.Length
                    && bytesWritten < count
                    && remainder + j < targetBuffer.Length; j++)
                {
                    buffer[offset + bytesWritten++] = targetBuffer[remainder + j];
                }

                // Subsequent chunks are always remainder-zero
                remainder = 0;
                iterPosition = Position;
            }

            // Seek to final expected position
            var seekTo = Math.Min(receivedPoint + count, Length);
            Seek(seekTo, SeekOrigin.Begin);

            return bytesWritten;
        }

        /// <summary>
        /// Gets the position of the first chunk that covers the base position.
        /// </summary>
        /// <param name="basePosition">The queried base position.</param>
        /// <param name="chunkSize">The chunk size.</param>
        /// <param name="chunkNumber">The sequential chunk number.</param>
        /// <param name="remainder">The number of skippable bytes in the chunk before the requested
        /// position is reached.</param>
        /// <returns>The discrete chunk position.</returns>
        protected static long ChunkPosition(
            long basePosition, int chunkSize, out long chunkNumber, out int remainder)
        {
            chunkNumber = 1 + (long)Math.Floor((double)basePosition / chunkSize);
            var chunkStart = chunkSize * (chunkNumber - 1);
            remainder = (int)(basePosition - chunkStart);
            return chunkStart;
        }

        /// <summary>
        /// Obtains a mapped chunk.
        /// </summary>
        /// <param name="sourceBuffer">The source buffer.</param>
        /// <param name="chunkNumber">The discrete chunk number.</param>
        /// <returns>Mapped bytes.</returns>
        protected virtual byte[] MapChunk(byte[] sourceBuffer, long chunkNumber)
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
            if (actualRead < BufferLength)
            {
                Array.Resize(ref buffer, actualRead);
            }

            return buffer;
        }

        /// <inheritdoc/>
        public virtual long Seek(long offset) => Seek(offset, SeekOrigin.Begin);
    }
}

/*
 * Openize.IsoBmff
 * Copyright (c) 2024 Openize Pty Ltd. 
 *
 * This file is part of Openize.IsoBmff.
 *
 * Openize.IsoBmff is available under MIT license, which is
 * available along with Openize.IsoBmff sources.
 */

namespace Openize.IsoBmff.IO
{
	using System;
	using System.IO;
    using System.Linq;

    /// <summary>
    /// The BitStreamReader class is designed to read bits from a specified stream.
    /// It reads a minimal amount of bytes from the stream into an intermediate buffer
    /// and then reads the bits from the buffer, returning the read value.
    /// If there is still enough data in the buffer, the data is read from it.
    /// </summary>
    public class BitStreamReader
	{
		/// <summary>
		/// File stream.
		/// </summary>
        protected readonly Stream stream;

		/// <summary>
		/// Bit reader state.
		/// </summary>
        protected BitReaderState state;

		/// <summary>
		/// The constructor takes a Stream object and an optional buffer size as parameters.
		/// </summary>
		/// <param name="stream">The source stream.</param>
		/// <param name="bufferSize">The buffer size.</param>
		public BitStreamReader(Stream stream, int bufferSize = 4)
		{
			if (stream == null)
			{
				throw new ArgumentNullException(nameof(stream));
			}

			if (bufferSize <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(bufferSize));
			}

			this.stream = stream;
			state.Buffer = new byte[bufferSize];
			state.BufferActiveLength = bufferSize;
            state.BufferPosition = -1;
		}

        /// <summary>
        /// Gets the current position within the bitstream.
        /// <para>The bitstream position is x8 of stream position, adjusted according to the number of bits read from the latest byte.</para>
        /// </summary>
        /// <returns>Unsinged long value.</returns>
        public ulong GetBitPosition()
		{
			return (ulong)((stream.Position - state.BufferActiveLength + state.BufferPosition) * 8 + state.BitIndex);
        }

        /// <summary>
		/// Sets the current position within the bitstream.
        /// </summary>
        /// <param name="bytePosition">The new byte position within the bitstream.</param>
        public void SetBytePosition(long bytePosition)
        {
			stream.Position = bytePosition;
			state.Reset();
        }

        /// <summary>
        /// Indicates if the current position in the bitstream is on a byte boundary.
        /// </summary>
        /// <returns>Returns true if the current position in the bitstream is on a byte boundary, false otherwise.</returns>
        public bool ByteAligned()
        {
            return state.BitIndex == 0 || state.BitIndex == 8;
        }

        /// <summary>
        /// Indicates if there are more data in the bitstream.
        /// </summary>
        /// <returns>True if there are more data in the bitstream, false otherwise.</returns>
		public bool MoreData()
		{
			return (state.BufferPosition >= 0 && state.BitIndex < 8) ||
			       (stream.CanRead && stream.Position < stream.Length);
		}

        /// <summary>
        /// Fill reader buffer with data from stream.
        /// </summary>
        /// <returns>The tolal amount of bytes read into the buffer.</returns>
        protected int FillBufferFromStream()
        {
            if (stream.Position + state.Buffer.Length > stream.Length)
            {
                state.Buffer = new byte[state.Buffer.Length];
				state.BufferActiveLength = (int)(stream.Length - stream.Position);
            }

            state.BufferPosition = 0;
            return stream.Read(state.Buffer, 0, state.Buffer.Length);
        }

        /// <summary>
        /// Reads the specified number of bits from the stream.
        /// </summary>
        /// <param name="bitCount">The required number of bits to read.</param>
        /// <returns>The integer value.</returns>
        public int Read(int bitCount)
		{
			if (bitCount <= 0 || bitCount > 32)
			{
				if (bitCount == 0)
					return 0;

				throw new ArgumentOutOfRangeException(nameof(bitCount));
			}

			int value = 0;
			int remainingBits = bitCount;

			if (state.BufferPosition < 0)
				FillBufferFromStream();

            while (remainingBits > 0)
			{
				if (state.BitIndex == 8)
				{
					state.BitIndex = 0;
					state.BufferPosition++;

					if (state.BufferPosition == state.Buffer.Length)
                        FillBufferFromStream();
                }

				int bitsToRead = Math.Min(remainingBits, 8 - state.BitIndex);
				int mask = (1 << bitsToRead) - 1;

				value <<= bitsToRead;
				value |= (state.Buffer[state.BufferPosition] >> (8 - state.BitIndex - bitsToRead)) & mask;

				state.BitIndex += bitsToRead;
				remainingBits -= bitsToRead;
			}

			return value;
        }

        /// <summary>
        /// Reads bytes as ASCII characters until '\0'.
        /// </summary>
        /// <returns>The string value.</returns>
        public string ReadString()
        {
            string output = "";
            int b;

            do
            {
                b = Read(8);
                //if (b < 127 && b > 31) 
                output += Convert.ToChar(b);
            } while (b != 0);

            return output;
        }

        /// <summary>
        /// Reads one bit and returns true if it is 1, otherwise false.
        /// </summary>
        /// <returns>The boolean value.</returns>
        public bool ReadFlag()
        {
            return Read(1) == 1;
        }

        /// <summary>
        /// Peeks the specified number of bits from the stream.
		/// This method does not change the position of the underlying stream, state of the reader remains unchanged.
        /// </summary>
        /// <param name="bitCount">The required number of bits to read.</param>
        /// <returns>The integer value.</returns>
        public int Peek(int bitCount)
		{
            var previousState = state;
            previousState.LastStreamPosition = this.stream.Position;

			int value = Read(bitCount);

            this.stream.Position = previousState.LastStreamPosition;
            state = previousState;
            return value;
        }

        /// <summary>
        /// Skip the specified number of bits in the stream.
        /// </summary>
        /// <param name="bitsNumber">Number of bits to skip.</param>
        public void SkipBits(int bitsNumber)
        {
            int remainingBits = bitsNumber;

            int bitsInBuffer = Math.Min(remainingBits, (state.Buffer.Length - state.BufferPosition) * 8 - state.BitIndex);

            int bitsToRead = Math.Min(remainingBits, bitsInBuffer);
            Read(bitsToRead);
            remainingBits -= bitsToRead;

            if (remainingBits == 0) return;

            stream.Seek(remainingBits / 8, SeekOrigin.Current);
            remainingBits = remainingBits % 8;

            state.BufferPosition = 0;
			state.BitIndex = 0;

			if(stream.CanRead && stream.Position < stream.Length)
                stream.Read(state.Buffer, 0, state.Buffer.Length);
			else
				state.BufferPosition = -1;

            if (remainingBits == 0) return;

            Read(remainingBits);
        }

		/// <summary>
		/// Reads bit at the specified position.
		/// This method does not change the position of the underlying stream, state of the reader remains unchanged.
		/// This method is approximately 50% slower than the Read method.
		/// </summary>
		public int GetBit(long position)
		{
			var previousState = state;
			previousState.LastStreamPosition = this.stream.Position;
			this.state.Reset();

			long bytePosition = position / 8;
			long bitPosition = position % 8;
			this.stream.Position = bytePosition;
			int value = 0;
			for (int i = 0; i <= bitPosition; i++)
			{
				value = this.Read(1);
			}

			this.stream.Position = previousState.LastStreamPosition;
			state = previousState;
			return value;
		}

        /// <summary>
        /// Supporting reader structure that contains buffer with read data, position of cursor and other supporting data.
        /// </summary>
        protected struct BitReaderState
		{
			/// <summary>
			/// Buffer data read from stream.
			/// </summary>
			public byte[] Buffer {  get; set; }

            /// <summary>
            /// Buffer size in bytes.
            /// </summary>
            public int BufferActiveLength { get; set; }

            /// <summary>
            /// Tracks the current byte position in the buffer.
            /// </summary>
            public int BufferPosition { get; set; }

            /// <summary>
            /// Tracks the current bit position in the buffer.
            /// </summary>
            public int BitIndex { get; set; }

            /// <summary>
            /// Last position of the stream the data was read from.
            /// </summary>
            public long LastStreamPosition { get; set; }

			/// <summary>
			/// Resets buffer to empty state.
			/// </summary>
            public void Reset()
			{
				BitIndex = 0;
				BufferPosition = -1;
				for (int i = 0; i < Buffer.Length; i++)
				{
					Buffer[i] = 0;
				}
			}

			/// <summary>
			/// Returns buffer contect in hex notation.
			/// </summary>
			public new string ToString() => String.Join(" ", Buffer.Select(b => Convert.ToString(b, 16).PadLeft(2, '0')));
		}
	}
}
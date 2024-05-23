/*
 * Openize.HEIC 
 * Copyright (c) 2024 Openize Pty Ltd. 
 *
 * This file is part of Openize.HEIC.
 *
 * Openize.HEIC is available under Openize license, which is
 * available along with Openize.HEIC sources.
 */

using System;
using System.Collections.Generic;
using System.IO;
using Openize.IsoBmff.IO;

namespace Openize.Heic.Decoder.IO
{
    /// <summary>
    /// The BitStreamWithNalSupport class is designed to read bits from a specified stream.
    /// It allows to ignore specified byte sequences while reading.
    /// </summary>
    internal class BitStreamWithNalSupport : BitStreamReader
    {
        /// <summary>
        /// Context-adaptive arithmetic entropy-decoder. 
        /// </summary>
        internal Cabac Cabac { get; }

        /// <summary>
        /// Creates a class object with a stream object and an optional buffer size as parameters.
        /// </summary>
        /// <param name="stream">The source stream.</param>
        /// <param name="bufferSize">The buffer size.</param>
        public BitStreamWithNalSupport(Stream stream, int bufferSize = 4) : base(stream, bufferSize) {
            ContextDictionary = new Dictionary<uint, DecoderContext>();
            Cabac = new Cabac(this);
        }

        /// <summary>
        /// Returns the current image context.
        /// </summary>
        internal DecoderContext Context => ContextDictionary [CurrentImageId];

        /// <summary>
        /// Current image identificator.
        /// </summary>
        public uint CurrentImageId { get; set; }

        /// <summary>
        /// Dictionary of images context information.
        /// </summary>
        private Dictionary<uint, DecoderContext> ContextDictionary;

        /// <summary>
        /// Creates an image context object.
        /// </summary>
        /// <param name="imageId">Image identificator.</param>
        public void CreateNewImageContext(uint imageId)
        {
            if (!ContextDictionary.ContainsKey(imageId))
                ContextDictionary.Add(imageId, new DecoderContext());
            CurrentImageId = imageId;
        }

        /// <summary>
        /// Deletes the image context object by id.
        /// </summary>
        /// <param name="imageId">Image identificator.</param>
        public void DeleteImageContext(uint imageId)
        {
            ContextDictionary.Remove(imageId);
        }

        /// <summary>
        /// Nal Unit reader mode.
        /// </summary>
        private bool _nalMode = false;

        /// <summary>
        /// Previous read byte.
        /// </summary>
        private byte _prevReadByte = 0xFF;

        /// <summary>
        /// The byte read before previous.
        /// </summary>
        private byte _prevPrevReadByte = 0xFF;

        /// <summary>
        /// Turns on Nal Unit reader mode which ignores specified by standart byte sequences.
        /// </summary>
        public void TurnOnNalUnitMode()
        {
            _nalMode = true;
            _prevReadByte = 0xFF;
            _prevPrevReadByte = 0xFF;
        }

        /// <summary>
        /// Turns off Nal Unit reader mode.
        /// </summary>
        public void TurnOffNulUnitMode()
        {
            _nalMode = false;
        }

        // shall not occur 
        // 0x000000
        // 0x000001
        // 0x000002

        // any four-byte sequence that starts with 0x000003 other than the following sequences shall not occur at any byte-aligned position:
        // 0x00000300
        // 0x00000301
        // 0x00000302
        // 0x00000303

        /// <summary>
        /// Reads the specified number of bits from the stream.
        /// </summary>
        /// <param name="bitCount">The required number of bits to read.</param>
        /// <returns>The integer value.</returns>
        public new int Read(int bitCount)
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
                    if (_nalMode)
                    {
                        _prevPrevReadByte = _prevReadByte;
                        _prevReadByte = state.Buffer[state.BufferPosition];
                    }

                    state.BitIndex = 0;
                    state.BufferPosition++;

                    if (state.BufferPosition == state.Buffer.Length)
                        FillBufferFromStream();
                }

                if (_nalMode)
                {
                    if (_prevPrevReadByte == 0x00 &&
                        _prevReadByte == 0x00 &&
                        state.Buffer[state.BufferPosition] == 0x03)
                    {
                        _prevPrevReadByte = _prevReadByte;
                        _prevReadByte = state.Buffer[state.BufferPosition];
                        state.BufferPosition++;
                        if (state.BufferPosition == state.Buffer.Length)
                            FillBufferFromStream();
                    }
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
        /// <returns>String value.</returns>
        public new string ReadString()
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
        /// <returns>Boolean value.</returns>
        public new bool ReadFlag()
        {
            return Read(1) == 1;
        }

        /// <summary>
        /// Skip the specified number of bits in the stream.
        /// </summary>
        /// <param name="bitsNumber">Number of bits to skip.</param>
        public new void SkipBits(int bitsNumber)
        {
            int remainingBits = bitsNumber;

            int bitsInBuffer = Math.Min(remainingBits, (state.Buffer.Length - state.BufferPosition) * 8 - state.BitIndex);

            int bitsToRead = Math.Min(remainingBits, bitsInBuffer);
            Read(bitsToRead);
            remainingBits -= bitsToRead;

            if (remainingBits == 0) return;


            if (_nalMode)
            {
                while (remainingBits >= 32)
                {
                    Read(32);
                    remainingBits -= 32;
                }
            }
            else
            {
                stream.Seek(remainingBits / 8, SeekOrigin.Current);
                remainingBits = remainingBits % 8;

                state.BufferPosition = 0;
                state.BitIndex = 0;

                if (stream.CanRead && stream.Position < stream.Length)
                    stream.Read(state.Buffer, 0, state.Buffer.Length);
                else
                    state.BufferPosition = -1;
            }

            if (remainingBits == 0) return;

            Read(remainingBits);
        }

        /// <summary>
        /// Read an unsigned integer 0-th order Exp-Golomb-coded syntax element with the left bit first.
        /// </summary>
        /// <returns>An unsigned integer.</returns>
        public uint ReadUev()
        {
            int leadingZeroBits = 0;

            while (Read(1) == 0)
                leadingZeroBits++;

            return (uint)((1 << leadingZeroBits) - 1 + Read(leadingZeroBits));
        }

        /// <summary>
        /// Read a signed integer 0-th order Exp-Golomb-coded syntax element with the left bit first.
        /// </summary>
        /// <returns>A signed integer.</returns>
        public int ReadSev()
        {
            uint codeNum = ReadUev();

            bool negative = ((codeNum & 1) == 0);
            return (negative ? -1 : 1) * (int)((codeNum + 1) / 2);
        }


        /// <summary>
        /// Placeholder for not implemented CABAC syntax elements. Always throws an exception.
        /// </summary>
        internal int ReadAev(CabacType type = 0)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Placeholder for not implemented CABAC syntax flag elements. Always throws an exception.
        /// </summary>
        internal bool ReadAevFlag(CabacType type = 0)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks if there is more data in the RBSP.
        /// </summary>
        /// <param name="endPosition">End position of current RBSP.</param>
        /// <returns>True if there is more data in the RBSP, false otherwise</returns>
        internal bool HasMoreRbspData(ulong endPosition)
        {
            // - If there is no more data in the raw byte sequence payload (RBSP), the return value of more_rbsp_data( ) is equal to FALSE
            // - Otherwise, the RBSP data are searched for the last (least significant, right-most) bit equal to 1 that is present in
            //   the RBSP. Given the position of this bit, which is the first bit(rbsp_stop_one_bit) of the rbsp_trailing_bits()
            //   syntax structure, the following applies:
            //     - If there is more data in an RBSP before the rbsp_trailing_bits() syntax structure, the return value of
            //       more_rbsp_data() is equal to TRUE.
            //     – Otherwise, the return value of more_rbsp_data() is equal to FALSE. 

            if (GetBitPosition() >= endPosition)
                throw new Exception("Parser logic error!");

            if (GetBitPosition() == endPosition)
                return false;

            while (!ByteAligned())
                SkipBits(1);

            if (Peek(8) == 0x80)
                return false;

            return true;
        }
    }
}

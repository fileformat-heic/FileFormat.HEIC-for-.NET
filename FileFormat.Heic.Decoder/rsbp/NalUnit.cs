/*
 * FileFormat.HEIC 
 * Copyright (c) 2024 Openize Pty Ltd. 
 *
 * This file is part of FileFormat.HEIC.
 *
 * FileFormat.HEIC is available under Openize license, which is
 * available along with FileFormat.HEIC sources.
 */

using System;
using FileFormat.Heic.Decoder.IO;

namespace FileFormat.Heic.Decoder
{
    internal class NalUnit
    {
        public NalHeader NalHeader;
        public ulong StartPosition;
        public int Size;

        public ulong EndPosition => StartPosition + (ulong)Size*8;

        public string ToString => $"NAL Unit {NalHeader.type}";

        public NalUnit(BitStreamWithNalSupport stream, ulong startPosition, int size)
        {
            NalHeader = new NalHeader(stream);

            StartPosition = startPosition;
            Size = size;
        }

        public static NalUnit ParseKnownUnit(BitStreamWithNalSupport stream, int size, NalUnitType type)
        {
            NalUnit nalUnit;
            ulong startPosition = stream.GetBitPosition();
            stream.TurnOnNalUnitMode();

            // Discard all NAL units with nuh_layer_id > 0
            // These will have to be handeled by an SHVC decoder.

            // throw away NALs from higher TIDs than currently selected

            if ((int)type < 32)
            {
                // VCL
                nalUnit = new slice_segment_layer_rbsp(stream, startPosition, size);
            }
            else
            {
                // non-VCL
                switch (type)
                {
                    case NalUnitType.VPS_NUT:     // 32
                        nalUnit = new video_parameter_set_rbsp(stream, startPosition, size);
                        break;
                    case NalUnitType.SPS_NUT:     // 33
                        nalUnit = new seq_parameter_set_rbsp(stream, startPosition, size);
                        break;
                    case NalUnitType.PPS_NUT:     // 34
                        nalUnit = new pic_parameter_set_rbsp(stream, startPosition, size);
                        break;

                    default:
                        nalUnit = new NalUnit(stream, startPosition, size);
                        break;
                }
            }

            stream.TurnOffNulUnitMode();
            ulong currentPosition = stream.GetBitPosition();

            if (currentPosition - startPosition > (ulong)size * 8)
                throw new DataMisalignedException();

            if (currentPosition - startPosition < (ulong)size * 8)
                stream.SkipBits(size * 8 - (int)(currentPosition - startPosition));

            return nalUnit;
        }

        public static NalUnit ParseUnit(BitStreamWithNalSupport stream)
        {
            int size = stream.Read(32);
            int peek = stream.Peek(8);
            NalUnitType type = (NalUnitType)((peek >> 1) & 0x3F);
            return ParseKnownUnit(stream, size, type);
        }
    }
}

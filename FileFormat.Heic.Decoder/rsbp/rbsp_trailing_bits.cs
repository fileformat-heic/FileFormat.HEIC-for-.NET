/*
 * FileFormat.HEIC 
 * Copyright (c) 2024 Openize Pty Ltd. 
 *
 * This file is part of FileFormat.HEIC.
 *
 * FileFormat.HEIC is available under Openize license, which is
 * available along with FileFormat.HEIC sources.
 */

using FileFormat.Heic.Decoder.IO;

namespace FileFormat.Heic.Decoder
{
    internal class rbsp_trailing_bits {
        public rbsp_trailing_bits(BitStreamWithNalSupport stream)
        {
            int one = stream.Read(1);   /* equal to 1; rbsp_stop_one_bit */

            while (!stream.ByteAligned())
                stream.SkipBits(1);     /* equal to 0; rbsp_alignment_zero_bit */
        }
    }

}

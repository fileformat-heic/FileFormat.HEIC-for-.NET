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
    internal class pcm_sample
    {

        public pcm_sample(BitStreamWithNalSupport stream, slice_segment_header header, 
            int x0, int y0, int log2CbSize)
        {
            var picture = header.parentPicture;

            picture.pcm_sample_luma = new int[1 << (log2CbSize << 1)];

            for (int i = 0; i < 1 << (log2CbSize << 1); i++)
                picture.pcm_sample_luma[i] = 
                    stream.Read(header.pps.sps.pcm_sample_bit_depth_luma);

            picture.pcm_sample_chroma = new int[((2 << (log2CbSize << 1)) / (header.pps.sps.SubWidthC * header.pps.sps.SubHeightC))];

            if (header.pps.sps.ChromaArrayType != 0)
                for (int i = 0; i < ((2 << (log2CbSize << 1)) / (header.pps.sps.SubWidthC * header.pps.sps.SubHeightC)); i++)
                    picture.pcm_sample_chroma[i] = 
                        stream.Read(header.pps.sps.pcm_sample_bit_depth_chroma);

        }
    }
}

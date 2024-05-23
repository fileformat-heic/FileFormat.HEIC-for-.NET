/*
 * Openize.HEIC 
 * Copyright (c) 2024 Openize Pty Ltd. 
 *
 * This file is part of Openize.HEIC.
 *
 * Openize.HEIC is available under Openize license, which is
 * available along with Openize.HEIC sources.
 */

using Openize.Heic.Decoder.IO;

namespace Openize.Heic.Decoder
{
    internal class coding_tree_unit
    {
        public coding_tree_unit(BitStreamWithNalSupport stream, 
            slice_segment_header header)
        {
            var sps = header.pps.sps;
            var picture = header.parentPicture;

            int xCtb = (int)(header.CtbAddrInRs % sps.PicWidthInCtbsY) << sps.CtbLog2SizeY;
            int yCtb = (int)(header.CtbAddrInRs / sps.PicWidthInCtbsY) << sps.CtbLog2SizeY;

            picture.SliceAddrRs[xCtb >> sps.CtbLog2SizeY, yCtb >> sps.CtbLog2SizeY] = header.SliceAddrRs;
            picture.SliceHeaderIndex[xCtb, yCtb] = header.slice_index;

            if (header.slice_sao_luma_flag || header.slice_sao_chroma_flag)
                new sao(stream, xCtb >> sps.CtbLog2SizeY, yCtb >> sps.CtbLog2SizeY, header);

            new coding_quadtree(stream, header, xCtb, yCtb, sps.CtbLog2SizeY, 0);
        }
    }
}

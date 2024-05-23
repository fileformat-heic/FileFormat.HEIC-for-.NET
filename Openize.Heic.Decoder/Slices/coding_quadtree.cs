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
    internal class coding_quadtree
    {

        public coding_quadtree(BitStreamWithNalSupport stream, slice_segment_header header,
            int x0, int y0, int log2CbSize, int cqtDepth)
        {
            var sps = header.pps.sps;
            var picture = header.parentPicture;
            bool split_cu_flag = false;

            if (x0 + (1 << log2CbSize) <= header.pps.sps.pic_width_in_luma_samples &&
                y0 + (1 << log2CbSize) <= header.pps.sps.pic_height_in_luma_samples &&
                log2CbSize > sps.MinCbLog2SizeY)
                split_cu_flag = stream.Cabac.read_split_cu_flag(x0, y0, picture, cqtDepth);
            else
            {
                split_cu_flag = log2CbSize > sps.MinCbLog2SizeY;
            }

            uint Log2MinCuQpDeltaSize = header.pps.sps.CtbLog2SizeY - header.pps.diff_cu_qp_delta_depth;
            uint Log2MinCuChromaQpOffsetSize = header.pps.sps.CtbLog2SizeY - header.pps.pps_range_ext.diff_cu_chroma_qp_offset_depth;

            if (header.pps.cu_qp_delta_enabled_flag && log2CbSize >= Log2MinCuQpDeltaSize)
            {
                stream.Context.IsCuQpDeltaCoded = false;
                stream.Context.CuQpDeltaVal = 0;
            }

            if (header.cu_chroma_qp_offset_enabled_flag &&
                log2CbSize >= Log2MinCuChromaQpOffsetSize)
                stream.Context.IsCuChromaQpOffsetCoded = false;

            if (split_cu_flag)
            {
                int x1 = x0 + (1 << (log2CbSize - 1));
                int y1 = y0 + (1 << (log2CbSize - 1));
                new coding_quadtree(stream, header, x0, y0, log2CbSize - 1, cqtDepth + 1);

                if (x1 < header.pps.sps.pic_width_in_luma_samples)
                    new coding_quadtree(stream, header, x1, y0, log2CbSize - 1, cqtDepth + 1);
                if (y1 < header.pps.sps.pic_height_in_luma_samples)
                    new coding_quadtree(stream, header, x0, y1, log2CbSize - 1, cqtDepth + 1);
                if (x1 < header.pps.sps.pic_width_in_luma_samples && y1 < header.pps.sps.pic_height_in_luma_samples)
                    new coding_quadtree(stream, header, x1, y1, log2CbSize - 1, cqtDepth + 1);
            }
            else
            {
                picture.SetCtDepth(x0, y0, log2CbSize, cqtDepth);
                new coding_unit(stream, header, x0, y0, log2CbSize);
            }
        }

    }
}

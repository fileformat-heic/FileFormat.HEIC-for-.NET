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
    internal class transform_tree
    {
        bool split_transform_flag;

        public transform_tree(BitStreamWithNalSupport stream, slice_segment_header header,
            bool IntraSplitFlag, uint MaxTrafoDepth, PartMode partMode,
            int x0, int y0, int xBase, int yBase, int log2TrafoSize, int trafoDepth, int blkIdx)
        {
            var sps = header.pps.sps;
            var picture = header.parentPicture;

            if (picture.cbf_cb[x0, y0] == null) picture.cbf_cb[x0, y0] = new bool[MaxTrafoDepth + 1];
            if (picture.cbf_cr[x0, y0] == null) picture.cbf_cr[x0, y0] = new bool[MaxTrafoDepth + 1];
            if (picture.cbf_luma[x0, y0] == null) picture.cbf_luma[x0, y0] = new bool[MaxTrafoDepth + 1];

            if (log2TrafoSize <= sps.MaxTbLog2SizeY &&
                log2TrafoSize > sps.MinTbLog2SizeY &&
                trafoDepth < MaxTrafoDepth &&
                !(IntraSplitFlag && (trafoDepth == 0)))
            {
                split_transform_flag = stream.Cabac.read_split_transform_flag(log2TrafoSize);
            }
            else
            {
                bool interSplitFlag =
                    sps.max_transform_hierarchy_depth_inter == 0 &&
                    picture.CuPredMode[x0, y0] == PredMode.MODE_INTER &&
                    partMode != PartMode.PART_2Nx2N &&
                    trafoDepth == 0;

                split_transform_flag =
                    log2TrafoSize > sps.MaxTbLog2SizeY ||
                    (IntraSplitFlag && trafoDepth == 0) ||
                    interSplitFlag;
            }

            if ((log2TrafoSize > 2 && sps.ChromaArrayType != 0) || sps.ChromaArrayType == 3)
            {
                if (trafoDepth == 0 || picture.cbf_cb[xBase, yBase][trafoDepth - 1])
                {
                    picture.cbf_cb[x0, y0][trafoDepth] = stream.Cabac.read_cbf_chroma(trafoDepth);

                    if (sps.ChromaArrayType == 2 && (!split_transform_flag || log2TrafoSize == 3))
                        picture.cbf_cb[x0, y0 + (1 << (log2TrafoSize - 1))][trafoDepth] = stream.Cabac.read_cbf_chroma(trafoDepth);
                }

                if (trafoDepth == 0 || picture.cbf_cr[xBase, yBase][trafoDepth - 1])
                {
                    picture.cbf_cr[x0, y0][trafoDepth] = stream.Cabac.read_cbf_chroma(trafoDepth);

                    if (sps.ChromaArrayType == 2 && (!split_transform_flag || log2TrafoSize == 3))
                        picture.cbf_cr[x0, y0 + (1 << (log2TrafoSize - 1))][trafoDepth] = stream.Cabac.read_cbf_chroma(trafoDepth);
                }
            }

            if (split_transform_flag)
            {
                int x1 = x0 + (1 << (log2TrafoSize - 1));
                int y1 = y0 + (1 << (log2TrafoSize - 1));
                new transform_tree(stream, header, IntraSplitFlag, MaxTrafoDepth, partMode, x0, y0, x0, y0, log2TrafoSize - 1, trafoDepth + 1, 0);
                new transform_tree(stream, header, IntraSplitFlag, MaxTrafoDepth, partMode, x1, y0, x0, y0, log2TrafoSize - 1, trafoDepth + 1, 1);
                new transform_tree(stream, header, IntraSplitFlag, MaxTrafoDepth, partMode, x0, y1, x0, y0, log2TrafoSize - 1, trafoDepth + 1, 2);
                new transform_tree(stream, header, IntraSplitFlag, MaxTrafoDepth, partMode, x1, y1, x0, y0, log2TrafoSize - 1, trafoDepth + 1, 3);
            }
            else
            {
                if (picture.CuPredMode[x0, y0] == PredMode.MODE_INTRA || 
                    trafoDepth != 0 ||
                    picture.cbf_cb[x0, y0][trafoDepth] ||
                    picture.cbf_cr[x0, y0][trafoDepth] ||
                    (header.pps.sps.ChromaArrayType == 2 &&
                    (picture.cbf_cb[x0, y0 + (1 << (log2TrafoSize - 1))][trafoDepth] ||
                    picture.cbf_cr[x0, y0 + (1 << (log2TrafoSize - 1))][trafoDepth])))
                {
                    picture.cbf_luma[x0, y0][trafoDepth] = stream.Cabac.read_cbf_luma(trafoDepth); 
                }

                new transform_unit(stream, header, partMode, x0, y0, xBase, yBase, log2TrafoSize, trafoDepth, blkIdx);
            }
        }
    }
}

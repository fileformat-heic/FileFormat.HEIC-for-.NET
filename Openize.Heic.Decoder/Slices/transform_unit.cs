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
using Openize.Heic.Decoder.IO;

namespace Openize.Heic.Decoder
{
    internal partial class transform_unit
    {
        public transform_unit(BitStreamWithNalSupport stream, slice_segment_header header, PartMode partMode,
            int x0, int y0, int xBase, int yBase, int log2TrafoSize, int trafoDepth, int blkIdx)
        {
            var pps = header.pps;
            var sps = header.pps.sps;
            var picture = header.parentPicture;

            int log2TrafoSizeC = Math.Max(2, log2TrafoSize - (header.pps.sps.ChromaArrayType == 3 ? 0 : 1));
            int cbfDepthC = trafoDepth - (header.pps.sps.ChromaArrayType != 3 && log2TrafoSize == 2 ? 1 : 0);
            int xC = (header.pps.sps.ChromaArrayType != 3 && log2TrafoSize == 2) ? xBase : x0;
            int yC = (header.pps.sps.ChromaArrayType != 3 && log2TrafoSize == 2) ? yBase : y0;

            int offset = 1 << log2TrafoSizeC;

            bool cbfLuma = picture.cbf_luma[x0, y0][trafoDepth];
            bool cbfChroma =
                picture.cbf_cb[xC, yC][cbfDepthC] ||
                picture.cbf_cr[xC, yC][cbfDepthC] ||
                (header.pps.sps.ChromaArrayType == 2 &&
                (picture.cbf_cb[xC, yC + (1 << log2TrafoSizeC)][cbfDepthC] ||
                 picture.cbf_cr[xC, yC + (1 << log2TrafoSizeC)][cbfDepthC]));

            if (cbfLuma || cbfChroma)
            {
                int xP = (x0 >> sps.MinCbLog2SizeY) << sps.MinCbLog2SizeY;
                int yP = (y0 >> sps.MinCbLog2SizeY) << sps.MinCbLog2SizeY;
                int nCbS = 1 << sps.MinCbLog2SizeY;

                if (pps.pps_scc_ext.residual_adaptive_colour_transform_enabled_flag &&
                    (picture.CuPredMode[x0, y0] == PredMode.MODE_INTER ||
                    (partMode == PartMode.PART_2Nx2N && (int)picture.intra_chroma_pred_mode[x0, y0] == 4) ||
                    ((int)picture.intra_chroma_pred_mode[xP, yP] == 4 &&
                     (int)picture.intra_chroma_pred_mode[xP + nCbS / 2, yP] == 4 &&
                     (int)picture.intra_chroma_pred_mode[xP, yP + nCbS / 2] == 4 &&
                     (int)picture.intra_chroma_pred_mode[xP + nCbS / 2, yP + nCbS / 2] == 4)))
                {
                    picture.tu_residual_act_flag[x0, y0] = stream.Cabac.read_tu_residual_act_flag();
                }

                bool decodeQp = false;

                //delta_qp
                if (header.pps.cu_qp_delta_enabled_flag && !stream.Context.IsCuQpDeltaCoded)
                {
                    decodeQp = true;
                    stream.Context.IsCuQpDeltaCoded = true;
                    int cu_qp_delta_abs = stream.Cabac.read_cu_qp_delta_abs();

                    if (cu_qp_delta_abs != 0)
                    {
                        bool cu_qp_delta_sign_flag = stream.Cabac.read_cu_qp_delta_sign_flag();

                        stream.Context.CuQpDeltaVal = cu_qp_delta_abs * (cu_qp_delta_sign_flag ? -1 : 1);
                    }
                }

                //chroma_qp_offset
                if (cbfChroma && !stream.Context.cu_transquant_bypass_flag)
                {
                    if (header.cu_chroma_qp_offset_enabled_flag && !stream.Context.IsCuChromaQpOffsetCoded)
                    {
                        decodeQp = true;
                        stream.Context.IsCuChromaQpOffsetCoded = true;
                        bool cu_chroma_qp_offset_flag = stream.Cabac.read_cu_chroma_qp_offset_flag();

                        if (cu_chroma_qp_offset_flag && header.pps.pps_range_ext.chroma_qp_offset_list_len_minus1 > 0)
                        {
                            int cu_chroma_qp_offset_idx =
                                stream.Cabac.read_cu_chroma_qp_offset_idx(header.pps.pps_range_ext.chroma_qp_offset_list_len_minus1);

                            stream.Context.CuQpOffsetCb = pps.pps_range_ext.cb_qp_offset_list[cu_chroma_qp_offset_idx];
                            stream.Context.CuQpOffsetCr = pps.pps_range_ext.cr_qp_offset_list[cu_chroma_qp_offset_idx];
                        }
                        else
                        {
                            stream.Context.CuQpOffsetCb = 0;
                            stream.Context.CuQpOffsetCr = 0;
                        }
                    }
                }

                if (decodeQp)
                    stream.Context.DerivationOfQuantizationParameters(stream, picture, x0, y0, xBase, yBase, log2TrafoSize);
            }

            if (cbfLuma)
                new residual_coding(stream, header, x0, y0, log2TrafoSize, 0);

            Decode(stream, header.parentPicture, x0, y0, trafoDepth, log2TrafoSize, 0);

            //if (cbfLuma || cbfChroma)
            if (sps.chroma_format_idc != 0) // this may be incorrect 
            {
                if (log2TrafoSize > 2 || header.pps.sps.ChromaArrayType == 3)
                {
                    bool res_scale_needed = cbfLuma &&
                        header.pps.pps_range_ext.cross_component_prediction_enabled_flag &&
                        (picture.CuPredMode[x0, y0] == PredMode.MODE_INTER || picture.intra_chroma_pred_mode[x0, y0] == 4);

                    // Cb
                    if (res_scale_needed)
                        picture.ResScaleVal[1][x0, y0] = get_ResScaleVal(stream, 0);

                    if (picture.cbf_cb[x0, y0][trafoDepth])
                        new residual_coding(stream, header, x0, y0, log2TrafoSizeC, 1);

                    Decode(stream, header.parentPicture, x0 / sps.SubWidthC, y0 / sps.SubHeightC, trafoDepth, log2TrafoSizeC, 1);

                    if (header.pps.sps.ChromaArrayType == 2)
                    {
                        if (picture.cbf_cb[x0, y0 + offset][trafoDepth])
                            new residual_coding(stream, header, x0, y0 + offset, log2TrafoSizeC, 1);

                        Decode(stream, header.parentPicture, x0 / sps.SubWidthC, y0 / sps.SubHeightC + offset, trafoDepth, log2TrafoSizeC, 1);
                    }

                    // Cr
                    if (res_scale_needed)
                        picture.ResScaleVal[2][x0, y0] = get_ResScaleVal(stream, 1);

                    if (picture.cbf_cr[x0, y0][trafoDepth])
                        new residual_coding(stream, header, x0, y0, log2TrafoSizeC, 2);

                    Decode(stream, header.parentPicture, x0 / sps.SubWidthC, y0 / sps.SubHeightC, trafoDepth, log2TrafoSizeC, 2);

                    if (header.pps.sps.ChromaArrayType == 2)
                    {
                        if (picture.cbf_cr[x0, y0 + offset][trafoDepth])
                            new residual_coding(stream, header, x0, y0 + offset, log2TrafoSizeC, 2);

                        Decode(stream, header.parentPicture, x0 / sps.SubWidthC, y0 / sps.SubHeightC + offset, trafoDepth, log2TrafoSizeC, 2);
                    }
                }
                else if (blkIdx == 3)
                {
                    // Cb
                    if (picture.cbf_cb[xBase, yBase][trafoDepth - 1])
                        new residual_coding(stream, header, xBase, yBase, log2TrafoSize, 1);

                    Decode(stream, header.parentPicture, xBase / sps.SubWidthC, yBase / sps.SubHeightC, trafoDepth, log2TrafoSize, 1);

                    if (header.pps.sps.ChromaArrayType == 2)
                    {
                        if (picture.cbf_cb[xBase, yBase + offset][trafoDepth - 1])
                            new residual_coding(stream, header, xBase, yBase + offset, log2TrafoSize, 1);

                        Decode(stream, header.parentPicture, xBase / sps.SubWidthC, yBase / sps.SubHeightC + offset, trafoDepth, log2TrafoSize, 1);
                    }

                    // Cr
                    if (picture.cbf_cr[xBase, yBase][trafoDepth - 1])
                        new residual_coding(stream, header, xBase, yBase, log2TrafoSize, 2);

                    Decode(stream, header.parentPicture, xBase / sps.SubWidthC, yBase / sps.SubHeightC, trafoDepth, log2TrafoSize, 2);

                    if (header.pps.sps.ChromaArrayType == 2)
                    {
                        if (picture.cbf_cr[xBase, yBase + offset][trafoDepth - 1])
                            new residual_coding(stream, header, xBase, yBase + offset, log2TrafoSize, 2);

                        Decode(stream, header.parentPicture, xBase / sps.SubWidthC, yBase / sps.SubHeightC + offset, trafoDepth, log2TrafoSize, 2);
                    }
                }
            }

        }

        private void Decode(BitStreamWithNalSupport stream, HeicPicture picture, int x0, int y0, int trafoDepth, int log2TrafoSize, int cIdx)
        {
            PredMode cuPredMode = picture.CuPredMode[x0, y0];
            IntraPredMode intraPredMode;

            int residualDpcm = 0;
            int nTbS = 1 << log2TrafoSize;
            int[,] samples = new int[nTbS, nTbS];

            if (cuPredMode == PredMode.MODE_INTRA)
            {
                intraPredMode = cIdx == 0 ? 
                    picture.IntraPredModeY[x0, y0] : 
                    picture.IntraPredModeC[x0 * picture.sps.SubWidthC, y0 * picture.sps.SubHeightC];

                samples = DecodeIntraPrediction(stream, picture, x0, y0, intraPredMode, nTbS, cIdx);

                residualDpcm = picture.sps.sps_range_ext.implicit_rdpcm_enabled_flag &&
                  (stream.Context.cu_transquant_bypass_flag || stream.Context.transform_skip_flag[cIdx]) &&
                  ((int)intraPredMode == 10 || (int)intraPredMode == 26) ? 1 : 0;
            }
            else
            {
                throw new NotImplementedException();
                // MODE_INTER is valid only for animations
            }

            //if (cbf)
            //{
            //    scale_coefficients();
            //}

            int[,] transSamples = ScalingAndTransformation(stream, picture, x0, y0, cuPredMode, trafoDepth, cIdx, nTbS);

            int bitDepth = (cIdx == 0) ? picture.sps.BitDepthY : picture.sps.BitDepthC;

            for (int y = 0; y < nTbS; y++)
                for (int x = 0; x < nTbS; x++)
                    picture.pixels[cIdx][x0 + x, y0 + y] = 
                        (ushort)MathExtra.ClipBitDepth(samples[x, y] + transSamples[x, y], bitDepth);
        }

        // 8.4.4.2.1 General intra sample prediction
        private int[,] DecodeIntraPrediction(BitStreamWithNalSupport stream, HeicPicture picture, int x0, int y0, IntraPredMode intraPredMode, int nTbS, int cIdx)
        { 
            var samples = NeighbouringSamplesGenerator.Generate(picture, x0, y0, nTbS, cIdx);

            if (picture.sps.sps_range_ext.intra_smoothing_disabled_flag == false &&
                (cIdx == 0 || picture.sps.ChromaArrayType == 3)) // chroma 4:4:4
            {
                samples = FilteringOfNeighbouringSamples(stream, picture, samples, nTbS, cIdx, intraPredMode);
            }

            switch (intraPredMode)
            {
                case IntraPredMode.INTRA_PLANAR:
                    return DecodeIntraPredictionPlanar(picture, nTbS, samples);
                case IntraPredMode.INTRA_DC:
                    return DecodeIntraPredictionDc(picture, nTbS, cIdx, samples);
                default:
                    return DecodeIntraPredictionAngular(stream, picture, nTbS, cIdx, intraPredMode, samples);
            }
        }

        // 8.4.4.2.3 Filtering process of neighbouring samples
        private NeighbouringSamples FilteringOfNeighbouringSamples(BitStreamWithNalSupport stream, HeicPicture picture, NeighbouringSamples p, int nTbS, int cIdx, IntraPredMode intraPredMode)
        {
            // Inputs to this process are:
            // – the neighbouring samples p[x][y], with x = −1, y = −1..nTbS * 2 − 1 and x = 0..nTbS * 2 − 1, y = −1,
            // – a variable nTbS specifying the transform block size.
            // Outputs of this process are the filtered samples pF[x][y], with x = −1, y = −1..nTbS * 2 − 1 and x = 0..nTbS * 2 − 1, y = −1.

            bool filterFlag;

            if (intraPredMode == IntraPredMode.INTRA_DC || nTbS == 4)
            {
                filterFlag = false;
            }
            else
            {
                int minDistVerHor = Math.Min(Math.Abs((int)intraPredMode - 26), Math.Abs((int)intraPredMode - 10));

                switch (nTbS)
                {
                    case 8:
                        filterFlag = minDistVerHor > 7;
                        break;
                    case 16:
                        filterFlag = minDistVerHor > 1;
                        break;
                    case 32:
                        filterFlag = minDistVerHor > 0;
                        break;
                    default:
                        filterFlag = false;
                        break;
                }
            }

            if (filterFlag)
            {
                bool biIntFlag =
                    picture.sps.strong_intra_smoothing_enabled_flag &&
                    cIdx == 0 &&
                    nTbS == 32 &&
                    Math.Abs(p[-1, -1] + p[nTbS * 2 - 1, -1] - 2 * p[nTbS - 1, -1]) < (1 << (picture.sps.BitDepthY - 5)) &&
                    Math.Abs(p[-1, -1] + p[-1, nTbS * 2 - 1] - 2 * p[-1, nTbS - 1]) < (1 << (picture.sps.BitDepthY - 5));

                var pF = new NeighbouringSamples(nTbS);

                if (biIntFlag)
                {
                    pF[-1, -1] = p[-1, -1];
                    pF[-1, 63] = p[-1, 63];
                    pF[63, -1] = p[63, -1];

                    for (int i = 0; i < 63; i++)
                    {
                        pF[-1, i] = (ushort)(((63 - i) * p[-1, -1] + (i + 1) * p[-1, 63] + 32) >> 6);
                        pF[i, -1] = (ushort)(((63 - i) * p[-1, -1] + (i + 1) * p[63, -1] + 32) >> 6);
                    }
                }
                else
                {
                    pF[-1, -1] = (ushort)((p[-1, 0] + 2 * p[-1, -1] + p[0, -1] + 2) >> 2);
                    pF[-1, nTbS * 2 - 1] = p[-1, nTbS * 2 - 1];
                    pF[nTbS * 2 - 1, -1] = p[nTbS * 2 - 1, -1];

                    for (int i = 0; i < nTbS * 2 - 1; i++)
                    {
                        pF[-1, i] = (ushort)((p[-1, i + 1] + 2 * p[-1, i] + p[-1, i - 1] + 2) >> 2);
                        pF[i, -1] = (ushort)((p[i - 1, -1] + 2 * p[i, -1] + p[i + 1, -1] + 2) >> 2);
                    }
                }

                p = pF;
            }

            return p;
        }

        // 8.4.4.2.4 Specification of intra prediction mode INTRA_PLANAR
        private int[,] DecodeIntraPredictionPlanar(HeicPicture picture, int nTbS, NeighbouringSamples p)
        {
            // Inputs to this process are:
            // – the neighbouring samples p[x][y], with x = -1, y = -1..nTbS * 2 - 1 and x = 0..nTbS * 2 - 1, y = -1,
            // – a variable nTbS specifying the transform block size.
            // Outputs of this process are the predicted samples predSamples[x][y], with x, y = 0..nTbS - 1.

            int[,] predSamples = new int[nTbS, nTbS];

            for (int x = 0; x < nTbS; x++)
                for (int y = 0; y < nTbS; y++)
                    predSamples[x, y] =
                        ((nTbS - 1 - x) * p[-1, y] + (x + 1) * p[nTbS, -1] +
                         (nTbS - 1 - y) * p[x, -1] + (y + 1) * p[-1, nTbS] + nTbS) >> ((int)Math.Log(nTbS, 2) + 1);
            
            return predSamples;
        }

        // 8.4.4.2.5 Specification of intra prediction mode INTRA_DC
        private int[,] DecodeIntraPredictionDc(HeicPicture picture, int nTbS, int cIdx, NeighbouringSamples p)
        {
            // Inputs to this process are:
            // – the neighbouring samples p[x][y], with x = −1, y = −1..nTbS * 2 − 1 and x = 0..nTbS * 2 − 1, y = −1,
            // – a variable nTbS specifying the transform block size,
            // – a variable cIdx specifying the colour component of the current block.
            // Outputs of this process are the predicted samples predSamples[x][y], with x, y = 0..nTbS − 1.

            int dcVal = nTbS;
            for (int i = 0; i < nTbS; i++)
            {
                dcVal += p[i, -1];
                dcVal += p[-1, i];
            }
            dcVal >>= ((int)Math.Log(nTbS, 2) + 1);

            int[,] predSamples = new int[nTbS, nTbS];

            if (cIdx == 0 && nTbS < 32)
            {
                predSamples[0, 0] = (p[-1, 0] + 2 * dcVal + p[0, -1] + 2) >> 2;

                for (int x = 1; x < nTbS; x++)
                    predSamples[x, 0] = (p[x, -1] + 3 * dcVal + 2) >> 2; 

                for (int y = 1; y < nTbS; y++)
                    predSamples[0, y] = (p[-1, y] + 3 * dcVal + 2) >> 2; 

                for (int y = 1; y < nTbS; y++)
                    for (int x = 1; x < nTbS; x++)
                        predSamples[x, y] = dcVal;
            }
            else
            {
                for (int y = 0; y < nTbS; y++)
                {
                    for (int x = 0; x < nTbS; x++)
                    {
                        predSamples[x, y] = dcVal;
                    }
                }
            }

            return predSamples;
        }

        // 8.4.4.2.6 Specification of intra prediction mode INTRA_ANGULAR
        private int[,] DecodeIntraPredictionAngular(BitStreamWithNalSupport stream, HeicPicture picture, 
            int nTbS, int cIdx, IntraPredMode intraPredMode, NeighbouringSamples p)
        {
            // Inputs to this process are:
            // – the intra prediction mode predModeIntra,
            // – the neighbouring samples p[x][y], with x = −1, y = −1..nTbS*2−1 and x = 0..nTbS*2−1, y = −1,
            // – a variable nTbS specifying the transform block size,
            // – a variable cIdx specifying the colour component of the current block.
            // Outputs of this process are the predicted samples predSamples[x][y], with x, y = 0..nTbS − 1.

            int[,] predSamples = new int[nTbS, nTbS];

            int intraPredAngle = intraPredAngleTable[(int)intraPredMode];

            bool disableIntraBoundaryFilter = false;
            if (picture.sps.sps_scc_ext.intra_boundary_filtering_disabled_flag)
                disableIntraBoundaryFilter = true;
            else if (picture.sps.sps_range_ext.implicit_rdpcm_enabled_flag && stream.Context.cu_transquant_bypass_flag)
                disableIntraBoundaryFilter = true;

            var reference = new int[3 * nTbS + 1];

            if ((int)intraPredMode >= 18)
            {
                for (int x = 0; x <= nTbS; x++)
                    reference[x + nTbS] = p[x - 1, -1];

                if (intraPredAngle < 0)
                {
                    int invAngle = invAngleTable[(int)intraPredMode];

                    if ((nTbS * intraPredAngle) >> 5 < -1)
                        for (int x = (nTbS * intraPredAngle) >> 5; x <= -1; x++)
                            reference[x + nTbS] = p[-1, -1 + ((x * invAngle + 128) >> 8)];
                }
                else
                {
                    for (int x = nTbS + 1; x <= 2 * nTbS; x++)
                        reference[x + nTbS] = p[x - 1, -1];
                }

                for (int x = 0; x < nTbS; x++)
                {
                    for (int y = 0; y < nTbS; y++)
                    {
                        int iIdx = ((y + 1) * intraPredAngle) >> 5;
                        int iFact = ((y + 1) * intraPredAngle) & 31;

                        if (iFact != 0)
                        {
                            predSamples[x, y] =
                                ((32 - iFact) * reference[x + nTbS + iIdx + 1] +
                                iFact * reference[x + nTbS + iIdx + 2] + 16) >> 5;
                        }
                        else
                        {
                            predSamples[x, y] = reference[x + nTbS + iIdx + 1];
                        }
                    }
                }

                if ((int)intraPredMode == 26 && cIdx == 0 && nTbS < 32 && !disableIntraBoundaryFilter)
                    for (int y = 0; y < nTbS; y++)
                        predSamples[0, y] =
                            MathExtra.ClipBitDepth(p[0, -1] + ((p[-1, y] - p[-1, -1]) >> 1), picture.sps.BitDepthY);
            }
            else
            {
                for (int x = 0; x <= nTbS; x++)
                    reference[x + nTbS] = p[-1, x - 1];

                if (intraPredAngle < 0)
                {
                    int invAngle = invAngleTable[(int)intraPredMode];

                    if ((nTbS * intraPredAngle) >> 5 < -1)
                        for (int x = (nTbS * intraPredAngle) >> 5; x <= -1; x++)
                            reference[x + nTbS] = p[-1 + ((x * invAngle + 128) >> 8), -1];
                }
                else
                {
                    for (int x = nTbS + 1; x <= 2 * nTbS; x++)
                        reference[x + nTbS] = p[-1, x - 1];
                }

                for (int x = 0; x < nTbS; x++)
                {
                    for (int y = 0; y < nTbS; y++)
                    {
                        int iIdx = ((x + 1) * intraPredAngle) >> 5;
                        int iFact = ((x + 1) * intraPredAngle) & 31;

                        if (iFact != 0)
                        {
                            predSamples[x, y] =
                                ((32 - iFact ) * reference[y + nTbS + iIdx + 1] + 
                                iFact * reference[y + nTbS + iIdx + 2] + 16 ) >> 5;
                        }
                        else
                        {
                            predSamples[x, y] = reference[y + nTbS + iIdx + 1];
                        }
                    }
                }

                if ((int)intraPredMode == 10 && cIdx == 0 && nTbS < 32 && !disableIntraBoundaryFilter)
                    for (int x = 0; x < nTbS; x++)
                        predSamples[x, 0] =
                            MathExtra.ClipBitDepth(p[-1, 0] + ((p[x, -1] - p[-1, -1]) >> 1), picture.sps.BitDepthY);

            }

            return predSamples;
        }


        // 8.6.2 Scaling and transformation process
        private int[,] ScalingAndTransformation(BitStreamWithNalSupport stream, HeicPicture picture, 
            int xTbY, int yTbY, PredMode cuPredMode, int trafoDepth, int cIdx, int nTbS)
        {
            // Inputs to this process are:
            // – a luma location(xTbY, yTbY) specifying the top-left sample of the current luma transform block
            //   relative to the top-left luma sample of the current picture,
            // – a variable trafoDepth specifying the hierarchy depth of the current block relative to the coding block,
            // – a variable cIdx specifying the colour component of the current block,
            // – a variable nTbS specifying the size of the current transform block.
            // Output of this process is the (nTbS)x(nTbS) array of residual samples r with elements r[x][y].

            var slice_header = picture.SliceHeaders[picture.SliceHeaderIndex[xTbY, yTbY]];

            int bitDepth = (cIdx == 0) ? picture.sps.BitDepthY : picture.sps.BitDepthC;
            int bdShift = Math.Max(20 - bitDepth, picture.sps.sps_range_ext.extended_precision_processing_flag ? 11 : 0);
            int tsShift = 5 + (int)Math.Log(nTbS, 2);

            bool rotateCoeffs = (picture.sps.sps_range_ext.transform_skip_rotation_enabled_flag &&
                                 nTbS == 4 &&
                                 cuPredMode == PredMode.MODE_INTRA);

            int[,] rotateSamples = new int[nTbS, nTbS];

            if (stream.Context.cu_transquant_bypass_flag)
            {
                if (rotateCoeffs)
                {
                    for (int x = 0; x < nTbS; x++)
                        for (int y = 0; y < nTbS; y++)
                            rotateSamples[x, y] = picture.TransCoeffLevel[cIdx][xTbY + nTbS - x - 1, yTbY + nTbS - y - 1];
                }
                else
                {
                    for (int x = 0; x < nTbS; x++)
                        for (int y = 0; y < nTbS; y++)
                            rotateSamples[x, y] = picture.TransCoeffLevel[cIdx][xTbY + x, yTbY + y];
                }
            }
            else
            {
                int qP;
                
                switch (cIdx)
                {
                    case 1:
                        qP = stream.Context.QpCb;
                        break;
                    case 2:
                        qP = stream.Context.QpCr;
                        break;
                    case 0:
                    default:
                        qP = MathExtra.Clip3(0, 51 + picture.sps.QpBdOffsetY, stream.Context.QpY +
                        (picture.tu_residual_act_flag[xTbY, yTbY] ? picture.pps.PpsActQpOffsetY + slice_header.slice_act_y_qp_offset : 0));
                        break;

                }

                int[,] d = GetScalingProcessForTransformCoefficients(stream, picture, xTbY, yTbY, cuPredMode, cIdx, nTbS, qP);

                if (stream.Context.transform_skip_flag[cIdx])
                {
                    for (int x = 0; x < nTbS; x++)
                        for (int y = 0; y < nTbS; y++)
                            rotateSamples[x, y] = (rotateCoeffs ? d[nTbS - x - 1, nTbS - y - 1] : d[x, y]) << tsShift;
                }
                else
                {
                    rotateSamples = TransformationOfScaledCoefficients(stream, picture, xTbY, yTbY, cuPredMode, cIdx, nTbS, d);
                }

                int offset = 1 << (bdShift - 1);

                for (int x = 0; x < nTbS; x++)
                    for (int y = 0; y < nTbS; y++)
                        rotateSamples[x, y] = (rotateSamples[x, y] + offset) >> bdShift;
            }
            return rotateSamples;
        }

        // 8.6.3 Scaling process for transform coefficients
        private int[,] GetScalingProcessForTransformCoefficients(BitStreamWithNalSupport stream, HeicPicture picture, 
            int xTbY, int yTbY, PredMode cuPredMode, int cIdx, int nTbS, int qP)
        {
            // Inputs to this process are:
            // – a luma location(xTbY, yTbY) specifying the top-left sample of the current luma transform block
            //   relative to the top-left luma sample of the current picture,
            // – a variable nTbS specifying the size of the current transform block,
            // – a variable cIdx specifying the colour component of the current block,
            // – a variable qP specifying the quantization parameter.
            // Output of this process is the(nTbS) x(nTbS)array d of scaled transform coefficients with elements d[x][y].

            int log2TransformRange = cIdx == 0 ?
                (picture.sps.sps_range_ext.extended_precision_processing_flag ? Math.Max(15, picture.sps.BitDepthY + 6) : 15) :
                (picture.sps.sps_range_ext.extended_precision_processing_flag ? Math.Max(15, picture.sps.BitDepthC + 6) : 15);

            int bdShift = (cIdx == 0 ? picture.sps.BitDepthY : picture.sps.BitDepthC) + 
                (int)Math.Log(nTbS, 2) + 10 - log2TransformRange;
            int coeffMin = cIdx == 0 ? picture.sps.CoeffMinY : picture.sps.CoeffMinC;
            int coeffMax = cIdx == 0 ? picture.sps.CoeffMaxY : picture.sps.CoeffMaxC;

            int sizeId = (int)Math.Log(nTbS, 2) - 2;
            int matrixId = cIdx + (cuPredMode == PredMode.MODE_INTRA ? 0 : 3);

            int[,] m = new int[nTbS, nTbS];
            int[,] d = new int[nTbS, nTbS];

            int coeff;

            for (int x = 0; x < nTbS; x++)
            {
                for (int y = 0; y < nTbS; y++)
                {
                    coeff = picture.TransCoeffLevel[cIdx][xTbY + x, yTbY + y];

                    if (!picture.sps.scaling_list_enabled_flag ||
                        (stream.Context.transform_skip_flag[cIdx] && nTbS > 4))
                    {
                        m[x, y] = 16;
                    }
                    else
                    {
                        m[x, y] = Scaling.ScalingFactor[sizeId][matrixId][x, y];
                    }


                    d[x, y] = MathExtra.Clip3(coeffMin, coeffMax,
                        ((coeff * m[x, y] *
                        levelScale[qP % 6] << (qP / 6)) + (1 << (bdShift - 1))) >> bdShift);
                }
            }

            return d;
        }

        // 8.6.4 Transformation process for scaled transform coefficients
        private int[,] TransformationOfScaledCoefficients(BitStreamWithNalSupport stream, HeicPicture picture,
            int xTbY, int yTbY, PredMode cuPredMode, int cIdx, int nTbS, int[,] d)
        {
            // Inputs to this process are:
            // – a luma location(xTbY, yTbY) specifying the top - left sample of the current luma transform block relative to the
            //   top - left luma sample of the current picture,
            // – a variable nTbS specifying the size of the current transform block,
            // – a variable cIdx specifying the colour component of the current block,
            // – an(nTbS)x(nTbS) array d of scaled transform coefficients with elements d[x][y].
            // Output of this process is the(nTbS) x(nTbS)array r of residual samples with elements r[x][y].

            int coeffMin = cIdx == 0 ? picture.sps.CoeffMinY : picture.sps.CoeffMinC;
            int coeffMax = cIdx == 0 ? picture.sps.CoeffMaxY : picture.sps.CoeffMaxC;

            int trType = (cuPredMode == PredMode.MODE_INTRA && nTbS == 4 && cIdx == 0) ? 1 : 0;
            int mult = 1 << (5 - (int)Math.Log(nTbS, 2));

            int[,] e = new int[nTbS, nTbS];
            int[,] g = new int[nTbS, nTbS];
            int[,] r = new int[nTbS, nTbS];

            for (int x = 0; x < nTbS; x++)
            {
                if (trType == 1)
                    for (int i = 0; i < nTbS; i++)
                        for (int j = 0; j < nTbS; j++)
                            e[x, i] += transMatrix4x4[j, i] * d[x, j];
                else
                    for (int i = 0; i < nTbS; i++)
                        for (int j = 0; j < nTbS; j++)
                            e[x, i] += transMatrix32x32[j * mult, i] * d[x, j];
            }

            for (int x = 0; x < nTbS; x++)
                for (int y = 0; y < nTbS; y++)
                    g[x, y] = MathExtra.Clip3(coeffMin, coeffMax, (e[x, y] + 64) >> 7);

            for (int y = 0; y < nTbS; y++)
            {
                if (trType == 1)
                    for (int i = 0; i < nTbS; i++)
                        for (int j = 0; j < nTbS; j++)
                            r[i, y] += transMatrix4x4[j, i] * g[j, y];
                else
                    for (int i = 0; i < nTbS; i++)
                        for (int j = 0; j < nTbS; j++)
                            r[i, y] += transMatrix32x32[j * mult, i] * g[j, y];
            }

            return r;
        }

        // 8.6.4.2 Transformation process
        private int[] Transformation(int nTbS, int[] x, int trType)
        {
            // Inputs to this process are:
            // – a variable nTbS specifying the sample size of scaled transform coefficients,
            // – a list of scaled transform coefficients x with elements x[j], with j = 0..nTbS − 1.
            // – a transform type variable trType
            // Output of this process is the list of transformed samples y with elements y[ i ], with i = 0..nTbS − 1.

            int[] y = new int[nTbS];
            int mult = 1 << (5 - (int)Math.Log(nTbS, 2));

            if (trType == 1)
                for (int i = 0; i < nTbS; i++)
                    for (int j = 0; j < nTbS; j++)
                        y[i] += transMatrix4x4[i, j] * x[j];
            else
                for (int i = 0; i < nTbS; i++)
                    for (int j = 0; j < nTbS; j++)
                        y[i] += transMatrix32x32[i, j * mult] * x[j];

            return y;
        }

        readonly private int[,] transMatrix4x4 = {
            { 29, 55, 74, 84 },
            { 74, 74,  0,-74 },
            { 84,-29,-74, 55 },
            { 55,-84, 74,-29 }
        };

        readonly private int[,] transMatrix32x32 = {
            { 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64, 64},
            { 90, 90, 88, 85, 82, 78, 73, 67, 61, 54, 46, 38, 31, 22, 13,  4, -4,-13,-22,-31,-38,-46,-54,-61,-67,-73,-78,-82,-85,-88,-90,-90},
            { 90, 87, 80, 70, 57, 43, 25,  9, -9,-25,-43,-57,-70,-80,-87,-90,-90,-87,-80,-70,-57,-43,-25, -9,  9, 25, 43, 57, 70, 80, 87, 90},
            { 90, 82, 67, 46, 22, -4,-31,-54,-73,-85,-90,-88,-78,-61,-38,-13, 13, 38, 61, 78, 88, 90, 85, 73, 54, 31,  4,-22,-46,-67,-82,-90},
            { 89, 75, 50, 18,-18,-50,-75,-89,-89,-75,-50,-18, 18, 50, 75, 89, 89, 75, 50, 18,-18,-50,-75,-89,-89,-75,-50,-18, 18, 50, 75, 89},
            { 88, 67, 31,-13,-54,-82,-90,-78,-46, -4, 38, 73, 90, 85, 61, 22,-22,-61,-85,-90,-73,-38,  4, 46, 78, 90, 82, 54, 13,-31,-67,-88},
            { 87, 57,  9,-43,-80,-90,-70,-25, 25, 70, 90, 80, 43, -9,-57,-87,-87,-57, -9, 43, 80, 90, 70, 25,-25,-70,-90,-80,-43,  9, 57, 87},
            { 85, 46,-13,-67,-90,-73,-22, 38, 82, 88, 54, -4,-61,-90,-78,-31, 31, 78, 90, 61,  4,-54,-88,-82,-38, 22, 73, 90, 67, 13,-46,-85},
            { 83, 36,-36,-83,-83,-36, 36, 83, 83, 36,-36,-83,-83,-36, 36, 83, 83, 36,-36,-83,-83,-36, 36, 83, 83, 36,-36,-83,-83,-36, 36, 83},
            { 82, 22,-54,-90,-61, 13, 78, 85, 31,-46,-90,-67,  4, 73, 88, 38,-38,-88,-73, -4, 67, 90, 46,-31,-85,-78,-13, 61, 90, 54,-22,-82},
            { 80,  9,-70,-87,-25, 57, 90, 43,-43,-90,-57, 25, 87, 70, -9,-80,-80, -9, 70, 87, 25,-57,-90,-43, 43, 90, 57,-25,-87,-70,  9, 80},
            { 78, -4,-82,-73, 13, 85, 67,-22,-88,-61, 31, 90, 54,-38,-90,-46, 46, 90, 38,-54,-90,-31, 61, 88, 22,-67,-85,-13, 73, 82,  4,-78},
            { 75,-18,-89,-50, 50, 89, 18,-75,-75, 18, 89, 50,-50,-89,-18, 75, 75,-18,-89,-50, 50, 89, 18,-75,-75, 18, 89, 50,-50,-89,-18, 75},
            { 73,-31,-90,-22, 78, 67,-38,-90,-13, 82, 61,-46,-88, -4, 85, 54,-54,-85,  4, 88, 46,-61,-82, 13, 90, 38,-67,-78, 22, 90, 31,-73},
            { 70,-43,-87,  9, 90, 25,-80,-57, 57, 80,-25,-90, -9, 87, 43,-70,-70, 43, 87, -9,-90,-25, 80, 57,-57,-80, 25, 90,  9,-87,-43, 70},
            { 67,-54,-78, 38, 85,-22,-90,  4, 90, 13,-88,-31, 82, 46,-73,-61, 61, 73,-46,-82, 31, 88,-13,-90, -4, 90, 22,-85,-38, 78, 54,-67},
            { 64,-64,-64, 64, 64,-64,-64, 64, 64,-64,-64, 64, 64,-64,-64, 64, 64,-64,-64, 64, 64,-64,-64, 64, 64,-64,-64, 64, 64,-64,-64, 64},
            { 61,-73,-46, 82, 31,-88,-13, 90, -4,-90, 22, 85,-38,-78, 54, 67,-67,-54, 78, 38,-85,-22, 90,  4,-90, 13, 88,-31,-82, 46, 73,-61},
            { 57,-80,-25, 90, -9,-87, 43, 70,-70,-43, 87,  9,-90, 25, 80,-57,-57, 80, 25,-90,  9, 87,-43,-70, 70, 43,-87, -9, 90,-25,-80, 57},
            { 54,-85, -4, 88,-46,-61, 82, 13,-90, 38, 67,-78,-22, 90,-31,-73, 73, 31,-90, 22, 78,-67,-38, 90,-13,-82, 61, 46,-88,  4, 85,-54},
            { 50,-89, 18, 75,-75,-18, 89,-50,-50, 89,-18,-75, 75, 18,-89, 50, 50,-89, 18, 75,-75,-18, 89,-50,-50, 89,-18,-75, 75, 18,-89, 50},
            { 46,-90, 38, 54,-90, 31, 61,-88, 22, 67,-85, 13, 73,-82,  4, 78,-78, -4, 82,-73,-13, 85,-67,-22, 88,-61,-31, 90,-54,-38, 90,-46},
            { 43,-90, 57, 25,-87, 70,  9,-80, 80, -9,-70, 87,-25,-57, 90,-43,-43, 90,-57,-25, 87,-70, -9, 80,-80,  9, 70,-87, 25, 57,-90, 43},
            { 38,-88, 73, -4,-67, 90,-46,-31, 85,-78, 13, 61,-90, 54, 22,-82, 82,-22,-54, 90,-61,-13, 78,-85, 31, 46,-90, 67,  4,-73, 88,-38},
            { 36,-83, 83,-36,-36, 83,-83, 36, 36,-83, 83,-36,-36, 83,-83, 36, 36,-83, 83,-36,-36, 83,-83, 36, 36,-83, 83,-36,-36, 83,-83, 36},
            { 31,-78, 90,-61,  4, 54,-88, 82,-38,-22, 73,-90, 67,-13,-46, 85,-85, 46, 13,-67, 90,-73, 22, 38,-82, 88,-54, -4, 61,-90, 78,-31},
            { 25,-70, 90,-80, 43,  9,-57, 87,-87, 57, -9,-43, 80,-90, 70,-25,-25, 70,-90, 80,-43, -9, 57,-87, 87,-57,  9, 43,-80, 90,-70, 25},
            { 22,-61, 85,-90, 73,-38, -4, 46,-78, 90,-82, 54,-13,-31, 67,-88, 88,-67, 31, 13,-54, 82,-90, 78,-46,  4, 38,-73, 90,-85, 61,-22},
            { 18,-50, 75,-89, 89,-75, 50,-18,-18, 50,-75, 89,-89, 75,-50, 18, 18,-50, 75,-89, 89,-75, 50,-18,-18, 50,-75, 89,-89, 75,-50, 18},
            { 13,-38, 61,-78, 88,-90, 85,-73, 54,-31,  4, 22,-46, 67,-82, 90,-90, 82,-67, 46,-22, -4, 31,-54, 73,-85, 90,-88, 78,-61, 38,-13},
            {  9,-25, 43,-57, 70,-80, 87,-90, 90,-87, 80,-70, 57,-43, 25, -9, -9, 25,-43, 57,-70, 80,-87, 90,-90, 87,-80, 70,-57, 43,-25,  9},
            {  4,-13, 22,-31, 38,-46, 54,-61, 67,-73, 78,-82, 85,-88, 90,-90, 90,-90, 88,-85, 82,-78, 73,-67, 61,-54, 46,-38, 31,-22, 13, -4}
        };

        readonly private int[] levelScale = { 40, 45, 51, 57, 64, 72 };

        // Table 8-5
        readonly private int[] intraPredAngleTable =
        { 
            0, 0, 32, 26, 21, 17, 13, 9, 5, 2, 0, -2, -5, -9, -13, -17, -21, -26,
            -32, -26, -21, -17, -13, -9, -5, -2, 0, 2, 5, 9, 13, 17, 21, 26, 32
        };

        // Table 8-6 further specifies the mapping table between predModeIntra and the inverse angle parameter invAngle.
        readonly private int[] invAngleTable =
        { 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            -4096, -1638, -910, -630, -482, -390, -315, -256,
            -315, -390, -482, -630, -910, -1638, -4096
        };

        int get_ResScaleVal(BitStreamWithNalSupport stream, int c)
        {
            var log2_res_scale_abs_plus1 = stream.Cabac.read_log2_res_scale_abs_plus1(c);

            if (log2_res_scale_abs_plus1 == 0)
                return 0;

            bool res_scale_sign_flag = stream.Cabac.read_res_scale_sign_flag(c);

            return (1 << (log2_res_scale_abs_plus1 - 1)) * (res_scale_sign_flag ? -1 : 1);
        }
    }
}

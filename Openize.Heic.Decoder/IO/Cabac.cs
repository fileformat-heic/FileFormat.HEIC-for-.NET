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
    internal class Cabac
    {
        private BitStreamWithNalSupport stream;

        internal ContextVariable[][] contextTable = new ContextVariable[40][];
        internal byte[] StatCoeff = new byte[4];
        internal uint PredictorPaletteSize;
        internal ushort[,] PredictorPaletteEntries;
        internal int ivlCurrRange;
        internal int ivlOffset;

        // sync section
        private ContextVariable[][] contextTableSync = new ContextVariable[40][];
        internal byte[] tableStatCoeffSync = new byte[4];

        #region Construstor

        public Cabac(BitStreamWithNalSupport stream)
        {
            this.stream = stream;
            Scans.Initialize();
        }

        #endregion

        // FL = fixed-length
        // TR = truncated Rice
        // EGk = k-th order Exp-Golomb

        #region Decoding values slice_segment_data

        internal bool read_end_of_slice_segment_flag()
        {
            //FL: cMax: 1
            //bin 1: terminate
            return DecodeBeforeTermination() == 1;
        }

        internal int read_end_of_subset_one_bit()
        {
            //FL: cMax: 1
            //bin 1: terminate
            return DecodeBeforeTermination();
        }

        #endregion

        #region Decoding values sao

        internal bool read_sao_merge_flag()
        {
            //FL: cMax: 1
            //bin 1: 0
            ContextVariable[] model = contextTable[(int)CabacType.sao_merge_flag];
            return DecodeGeneralForBinaryDecision(model[0]) == 1;
        }

        internal int read_sao_type_idx()
        {
            //TR: cMax: 2, cRiceParam: 0
            //bin 1: 0, bin 2: bypass
            ContextVariable[] model = contextTable[(int)CabacType.sao_type_idx];

            var bin0 = DecodeGeneralForBinaryDecision(model[0]);

            if (bin0 == 0)
                return 0;

            var bin1 = DecodeBypass();

            if (bin1 == 0)
                return 1;
            else
                return 2;
        }

        internal int read_sao_offset_abs(int bitDepth)
        {
            //TR: cMax: defined next line, cRiceParam: 0
            //bin 1-5: bypass
            int cMax = (1 << (Math.Min(bitDepth, 10) - 5)) - 1;

            for (int i = 0; i < cMax; i++)
                if (DecodeBypass() == 0)
                    return i;

            return cMax;
        }

        internal int read_sao_offset_sign()
        {
            //FL: cMax: 1
            //bin 1: bypass
            return DecodeBypass();
        }

        internal int read_sao_band_position()
        {
            //FL: cMax: 31
            //bin 1-5+: bypass
            return DecodeFixedLengthBypass(5);
        }

        internal byte read_sao_class()
        {
            //FL: cMax: 3
            //bin 1-2: bypass
            return (byte)DecodeFixedLengthBypass(2);
        }

        #endregion

        #region Decoding values coding_quadtree

        internal bool read_split_cu_flag(int x0, int y0, HeicPicture picture, int cqtDepth)
        {
            // FL: cMax: 1
            // bin 1: clause 9.3.4.2.2
            int index = GetContextIndexFromNeighborState(x0, y0, picture, cqtDepth);
            ContextVariable[] model = contextTable[(int)CabacType.split_cu_flag];
            return DecodeGeneralForBinaryDecision(model[index]) == 1;
        }

        #endregion

        #region Decoding values coding_unit

        internal bool read_cu_transquant_bypass_flag()
        {
            // FL: cMax: 1
            // bin 1: 0
            ContextVariable[] model = contextTable[(int)CabacType.cu_transquant_bypass_flag];
            return DecodeGeneralForBinaryDecision(model[0]) == 1;
        }

        internal bool read_cu_skip_flag(int x0, int y0, HeicPicture picture, int cqtDepth)
        {
            // FL: cMax: 1
            // bin 1: clause 9.3.4.2.2
            int index = GetContextIndexFromNeighborState(x0, y0, picture, cqtDepth);
            ContextVariable[] model = contextTable[(int)CabacType.split_cu_flag];
            return DecodeGeneralForBinaryDecision(model[index]) == 1;
        }

        internal bool read_palette_mode_flag()
        {
            // FL: cMax: 1
            // bin 1: 0
            ContextVariable[] model = contextTable[(int)CabacType.palette_mode_flag];
            return DecodeGeneralForBinaryDecision(model[0]) == 1;
        }

        internal bool read_pred_mode_flag()
        {
            // FL: cMax: 1
            // bin 1: 0
            ContextVariable[] model = contextTable[(int)CabacType.pred_mode_flag];
            return DecodeGeneralForBinaryDecision(model[0]) == 1;
        }

        internal PartMode read_part_mode(HeicPicture picture, PredMode pred_mode, int cLog2CbSize)
        {
            // 9.3.3.7
            // bin 1: 0
            // bin 2: 1
            // bin 3: 2 (log2CbSize == MinCbLog2SizeY)
            // bin 3: 3 (log2CbSize > MinCbLog2SizeY)
            // bin 4: bypass

            ContextVariable[] model = contextTable[(int)CabacType.part_mode];

            if (pred_mode == PredMode.MODE_INTRA) {
                int bit = DecodeGeneralForBinaryDecision(model[0]);
                return bit == 1 ? PartMode.PART_2Nx2N : PartMode.PART_NxN;
            }
            else // MODE_INTER
            {
                var sps = picture.sps;

                int bit0 = DecodeGeneralForBinaryDecision(model[0]);
                if (bit0 == 1) 
                    return PartMode.PART_2Nx2N;

                int bit1 = DecodeGeneralForBinaryDecision(model[1]);

                if (cLog2CbSize > sps.MinCbLog2SizeY)
                {
                    if (!sps.amp_enabled_flag)
                        return bit1 == 1 ? PartMode.PART_2NxN : PartMode.PART_Nx2N;

                    int bit2 = DecodeGeneralForBinaryDecision(model[3]);

                    if (bit2 == 1)
                        return bit1 == 1 ? PartMode.PART_2NxN : PartMode.PART_Nx2N;

                    int bit3 = DecodeBypass();
                    if (bit1 == 1)
                        return bit3 == 0 ? PartMode.PART_2NxnU : PartMode.PART_2NxnD;
                    else
                        return bit3 == 0 ? PartMode.PART_nLx2N : PartMode.PART_nRx2N;
                }
                else
                {
                    if (bit1 == 1)
                        return PartMode.PART_2NxN;

                    if (cLog2CbSize == 3)
                        return PartMode.PART_Nx2N;

                    int bit2 = DecodeGeneralForBinaryDecision(model[2]);
                    return bit2 == 1 ? PartMode.PART_Nx2N : PartMode.PART_NxN;
                }
            }

            throw new FormatException("UNREACHABLE!");
        }

        internal bool read_pcm_flag()
        {
            // FL: cMax: 1
            // bin 1: terminate
            return DecodeBeforeTermination() == 1;
        }

        internal bool read_prev_intra_luma_pred_flag()
        {
            // FL: cMax: 1
            // bin 1: 0
            ContextVariable[] model = contextTable[(int)CabacType.prev_intra_luma_pred_flag];
            return DecodeGeneralForBinaryDecision(model[0]) == 1;
        }

        internal int read_mpm_idx()
        {
            // TR: cMax: 2, cRiceParam: 0
            // bin 1: bypass, bin 2: bypass

            for (int i = 0; i < 2; i++)
                if (DecodeBypass() == 0)
                    return i;

            return 2;
        }

        internal IntraPredMode read_rem_intra_luma_pred_mode()
        {
            // FL: cMax: 31
            // bin 1-5+: bypass
            return (IntraPredMode)DecodeFixedLengthBypass(5);
        }

        internal byte read_intra_chroma_pred_mode()
        {
            // 9.3.3.8
            // bin 1: 0
            // bin 2-3: bypass
            ContextVariable[] model = contextTable[(int)CabacType.intra_chroma_pred_mode];

            var bin0 = DecodeGeneralForBinaryDecision(model[0]);

            if (bin0 == 0)
                return (byte)4;
            else
                return (byte)DecodeFixedLengthBypass(2);
        }

        internal bool read_rqt_root_cbf()
        {
            // FL: cMax: 1
            // bin 1: 0
            ContextVariable[] model = contextTable[(int)CabacType.rqt_root_cbf];
            return DecodeGeneralForBinaryDecision(model[0]) == 1;
        }

        #endregion

        #region Decoding values prediction_unit

        // not implemented

        #endregion

        #region Decoding values transform_tree

        internal bool read_split_transform_flag(int log2TrafoSize)
        {
            // FL: cMax: 1
            // bin 1: 5 − log2TrafoSize
            int index = 5 - log2TrafoSize;
            ContextVariable[] model = contextTable[(int)CabacType.split_transform_flag];
            return DecodeGeneralForBinaryDecision(model[index]) == 1;
        }

        internal bool read_cbf_luma(int trafoDepth)
        {
            // FL: cMax: 1
            // bin 1: trafoDepth == 0 ? 1 : 0
            ContextVariable[] model = contextTable[(int)CabacType.cbf_luma];
            return DecodeGeneralForBinaryDecision(model[trafoDepth == 0 ? 1 : 0]) == 1;
        }

        internal bool read_cbf_chroma(int trafoDepth)
        {
            // FL: cMax: 1
            // bin 1: trafoDepth
            ContextVariable[] model = contextTable[(int)CabacType.cbf_chroma];
            return DecodeGeneralForBinaryDecision(model[trafoDepth]) == 1;
        }

        #endregion

        #region Decoding values mvd_coding

        // not implemented

        #endregion

        #region Decoding values transform_unit

        internal bool read_tu_residual_act_flag()
        {
            // FL: cMax: 1
            // bin 1: 0
            ContextVariable[] model = contextTable[(int)CabacType.tu_residual_act_flag];
            return DecodeGeneralForBinaryDecision(model[0]) == 1;
        }

        internal int read_cu_qp_delta_abs()
        {
            // 9.3.3.10
            // bin 1: 0
            // bin 2-5: 1
            // bin 5+: bypass
            ContextVariable[] model = contextTable[(int)CabacType.cu_qp_delta_abs];

            if (DecodeGeneralForBinaryDecision(model[0]) == 0)
                return 0;

            int value = 1;
            for (int i = 0; i < 4; i++) // TR; cMax = 5 and cRiceParam = 0
            {
                if (DecodeGeneralForBinaryDecision(model[1]) == 0)
                    return value;
                else
                    value++;
            }

            return value + DecodeExpGolombBypass(0);
        }

        internal bool read_cu_qp_delta_sign_flag()
        {
            // FL: cMax: 1
            // bin 1: bypass
            return DecodeBypass() == 1;
        }

        internal bool read_cu_chroma_qp_offset_flag()
        {
            // FL: cMax: 1
            // bin 1: 0
            ContextVariable[] model = contextTable[(int)CabacType.cu_chroma_qp_offset_flag];
            return DecodeGeneralForBinaryDecision(model[0]) == 1;
        }

        internal int read_cu_chroma_qp_offset_idx(int cMax)
        {
            // TR: cMax: chroma_qp_offset_list_len_minus1, cRiceParam = 0
            // bin 0-4: 0
            ContextVariable[] model = contextTable[(int)CabacType.cu_chroma_qp_offset_idx];

            for (int i = 0; i < cMax; i++)
                if (DecodeGeneralForBinaryDecision(model[0]) == 0)
                    return i;

            return cMax;
            // may be incorrect as not covered with tests. Another realisation:
            // return DecodeGeneralForBinaryDecision(model[0]);
        }

        #endregion

        #region Decoding values cross_comp_pred

        internal int read_log2_res_scale_abs_plus1(int c)
        {
            // TR cMax = 4, cRiceParam = 0
            // bin n: 4 * c + n
            ContextVariable[] model = contextTable[(int)CabacType.log2_res_scale_abs_plus1];
            int value = 0;

            for (int n = 0; n < 4; n++)
            {
                if (DecodeGeneralForBinaryDecision(model[4 * c + n]) == 0)
                    break;

                value++;
            }

            return value;
        }

        internal bool read_res_scale_sign_flag(int c)
        {
            // FL cMax = 1
            // bin 0: c
            ContextVariable[] model = contextTable[(int)CabacType.res_scale_sign_flag];
            return DecodeGeneralForBinaryDecision(model[c]) == 1;
        }

        #endregion

        #region Decoding values residual_coding

        internal bool read_transform_skip_flag(int cIdx) 
        {
            // FL cMax = 1
            // bin 0: 0
            int index = cIdx == 0 ? 0 : 1;
            ContextVariable[] model = contextTable[(int)CabacType.transform_skip_flag];
            return DecodeGeneralForBinaryDecision(model[index]) == 1;
        }

        internal bool read_explicit_rdpcm_flag(int cIdx) 
        {
            // FL cMax = 1
            // bin 0: 0
            int index = (cIdx == 0) ? 0 : 1;
            ContextVariable[] model = contextTable[(int)CabacType.explicit_rdpcm_flag];
            return DecodeGeneralForBinaryDecision(model[index]) == 1;
        }

        internal bool read_explicit_rdpcm_dir_flag(int cIdx)
        {
            // FL cMax = 1
            // bin 0: 0
            int index = (cIdx == 0) ? 0 : 1;
            ContextVariable[] model = contextTable[(int)CabacType.explicit_rdpcm_dir_flag];
            return DecodeGeneralForBinaryDecision(model[index]) == 1;
        }

        internal int read_last_sig_coeff_prefix(int log2TrafoSize, int cIdx, CabacType type)
        {
            // TR cMax = (log2TrafoSize << 1 ) − 1, cRiceParam = 0
            // 0..17 (clause 9.3.4.2.3)
            ContextVariable[] model = contextTable[(int)type];
            int cMax = (log2TrafoSize << 1) - 1;

            int ctxOffset, ctxShift;

            if (cIdx == 0)
            {
                ctxOffset = 3 * (log2TrafoSize - 2) + ((log2TrafoSize - 1) >> 2);
                ctxShift = (log2TrafoSize + 1) >> 2;
            }
            else
            {
                ctxOffset = 15;
                ctxShift = log2TrafoSize - 2;
            }

            int ctxInc;

            for (int binIdx = 0; binIdx < cMax; binIdx++)
            {
                ctxInc = (binIdx >> ctxShift) + ctxOffset;

                if (DecodeGeneralForBinaryDecision(model[ctxInc]) == 0)
                    return binIdx;
            }

            return cMax;
        }

        internal int read_last_sig_coeff_suffix(int last_significant_coeff_prefix)
        {
            // FL cMax = ( 1 << ((last_sig_coeff_x_prefix >> 1 ) − 1 ) − 1 )
            // bin 0-2: bypass
            return DecodeFixedLengthBypass((last_significant_coeff_prefix >> 1) - 1);
        }

        internal bool read_coded_sub_block_flag(uint xS, uint yS,
            bool[,] coded_sub_block_flag, int cIdx, int log2TrafoSize)
        {
            // FL cMax = 1
            // bin 0: 0..3 (clause 9.3.4.2.4)
            ContextVariable[] model = contextTable[(int)CabacType.coded_sub_block_flag];
            
            int csbfCtx = 0;

            if (xS < (1 << (log2TrafoSize - 2)) - 1)
                csbfCtx += coded_sub_block_flag[xS + 1, yS] ? 1 : 0;

            if (yS < (1 << (log2TrafoSize - 2)) - 1)
                csbfCtx += coded_sub_block_flag[xS, yS + 1] ? 1 : 0;

            int ctxInc = Math.Min(csbfCtx, 1);

            if (cIdx != 0)
                ctxInc += 2;

            return DecodeGeneralForBinaryDecision(model[ctxInc]) == 1;
        }

        internal bool read_sig_coeff_flag(HeicPicture picture, 
            bool transform_skip_flag, 
            int xC, int yC, 
            bool[,] coded_sub_block_flag, 
            int cIdx, int log2TrafoSize, int scanIdx) 
        {
            // FL cMax = 1
            // bin 0: 0..43 (clause 9.3.4.2.5)
            var sps = picture.sps;

            int sigCtx;

            if (sps.sps_range_ext.transform_skip_context_enabled_flag &&
                (transform_skip_flag || stream.Context.cu_transquant_bypass_flag))
            {
                sigCtx = (cIdx == 0) ? 42 : 16;
            }
            else if (log2TrafoSize == 2)
            {
                sigCtx = ctxIdxMap[(yC << 2) + xC];
            }
            else if (xC + yC == 0)
            {
                sigCtx = 0;
            }
            else
            {
                int xS = xC >> 2;
                int yS = yC >> 2;

                int prevCsbf = 0;
                if (xS < (1 << (log2TrafoSize - 2)) - 1) prevCsbf += coded_sub_block_flag[xS + 1, yS] ? 1 : 0;
                if (yS < (1 << (log2TrafoSize - 2)) - 1) prevCsbf += coded_sub_block_flag[xS, yS + 1] ? 2 : 0;

                int xP = xC & 3;
                int yP = yC & 3;

                switch (prevCsbf)
                {
                    case 0:
                        sigCtx = (xP + yP == 0) ? 2 : (xP + yP < 3) ? 1 : 0;
                        break;
                    case 1:
                        sigCtx = (yP == 0) ? 2 : (yP == 1) ? 1 : 0;
                        break;
                    case 2:
                        sigCtx = (xP == 0) ? 2 : (xP == 1) ? 1 : 0;
                        break;
                    default:
                        sigCtx = 2;
                        break;
                }

                if (cIdx == 0)
                {
                    if (xS + yS > 0)
                        sigCtx += 3;

                    if (log2TrafoSize == 3)
                        sigCtx += (scanIdx == 0) ? 9 : 15;
                    else
                        sigCtx += 21;
                }
                else
                {
                    if (log2TrafoSize == 3)
                        sigCtx += 9;
                    else
                        sigCtx += 12;
                }
            }

            int ctxInc = sigCtx + (cIdx == 0 ? 0 : 27);

            ContextVariable[] model = contextTable[(int)CabacType.sig_coeff_flag];
            return DecodeGeneralForBinaryDecision(model[ctxInc]) == 1;
        }

        internal bool read_coeff_abs_level_greater1_flag(int cIdx, int i,
                                                         bool firstCoeffInSubblock,
                                                         bool firstSubblock,
                                                         int lastSubblock_greater1Ctx,
                                                         ref int lastInvocation_ctxSet,
                                                         ref int lastInvocation_greater1Ctx,
                                                         ref bool lastInvocation_coeff_abs_level_greater1_flag)
        {
            // FL cMax = 1
            // bin 0: 0..23 (clause 9.3.4.2.6)

            int lastGreater1Ctx;
            int greater1Ctx;
            int ctxSet;

            if (firstCoeffInSubblock)
            {
                ctxSet = (i == 0 || cIdx > 0) ? 0 : 2;

                if (firstSubblock)
                {
                    lastGreater1Ctx = 1;
                }
                else
                {
                    lastGreater1Ctx = lastSubblock_greater1Ctx;

                    if (lastGreater1Ctx > 0)
                    {
                        if (lastInvocation_coeff_abs_level_greater1_flag)
                            lastGreater1Ctx = 0;
                        else
                            lastGreater1Ctx++;
                    }
                }
                

                if (lastGreater1Ctx == 0)
                    ctxSet++;

                greater1Ctx = 1;
            }
            else
            {
                ctxSet = lastInvocation_ctxSet;

                greater1Ctx = lastInvocation_greater1Ctx;

                if (greater1Ctx > 0)
                {
                    if (lastInvocation_coeff_abs_level_greater1_flag)
                        greater1Ctx = 0;
                    else 
                        greater1Ctx++;
                }
            }

            int ctxIdxInc = (ctxSet * 4) + Math.Min(3, greater1Ctx);

            if (cIdx > 0)
                ctxIdxInc += 16;

            ContextVariable[] model = contextTable[(int)CabacType.coeff_abs_level_greater1_flag];
            bool value = DecodeGeneralForBinaryDecision(model[ctxIdxInc]) == 1;

            lastInvocation_greater1Ctx = greater1Ctx;
            lastInvocation_coeff_abs_level_greater1_flag = value;
            lastInvocation_ctxSet = ctxSet;

            return value;
        }
        internal bool read_coeff_abs_level_greater2_flag(int cIdx, int ctxSet)
        {
            // FL cMax = 1
            // bin 0: 0..5 (clause 9.3.4.2.7)
            ContextVariable[] model = contextTable[(int)CabacType.coeff_abs_level_greater2_flag];
            int index = ctxSet + (cIdx > 0 ? 4 : 0);
            return DecodeGeneralForBinaryDecision(model[index]) == 1;
        }

        internal int read_coeff_abs_level_remaining(int cRiceParam)
        {
            // 9.3.3.11 current sub-block scan index i, baseLevel
            // all bins: bypass

            int prefix = -1;

            while (prefix <= 64)
            {
                prefix++;

                if (DecodeBypass() == 0)
                    break;
            }

            if (prefix > 64) // 8x8 block
                throw new FormatException("Parser logic error!");

            if (prefix <= 3)
                return (prefix << cRiceParam) + DecodeFixedLengthBypass(cRiceParam);

            return (((1 << (prefix - 3)) + 3 - 1) << cRiceParam) + DecodeFixedLengthBypass(prefix - 3 + cRiceParam);
        }

        internal bool read_coeff_sign_flag()
        {
            // FL cMax = 1
            // bin 0: bypass
            return DecodeBypass() == 1;
        }

        #endregion

        #region Decoding values palette_coding

        // not implemented

        #endregion

        #region Decoding patterns

        //The specification of the truncated Rice (TR) binarization process, the k-th order Exp-Golomb (EGk) binarization
        // process, limited k-th order Exp-Golomb(EGk) binarization process and  are
        // given in clauses 9.3.3.2 through 9.3.3.5, respectively.Other binarizations are specified in clauses 9.3.3.6
        // through 9.3.3.10.

        // 9.3.3.5 the fixed-length(FL) binarization process with bypass only
        int DecodeFixedLengthBypass(int nBits)
        {
            int value = 0;

            while (nBits > 0)
            {
                nBits--;
                value <<= 1;
                value |= DecodeBypass();
            }

            return value;
        }

        // 9.3.3.3 k-th order Exp-Golomb binarization process with bypass only
        int DecodeExpGolombBypass(int k)
        {
            int value = 0;
            int i = k;

            while (true)
            {
                if (DecodeBypass() == 0)
                    break;

                value += 1 << i;
                i++;

                if (i == k + 32)
                    throw new FormatException("Parser logic error!");
            }

            int suffix = DecodeFixedLengthBypass(i);
            return value + suffix;
        }

        // 9.3.4.3.2 Arithmetic decoding process for a binary decision
        // ContextVariable[] model = contextVariables[(int)cabacType];
        int DecodeGeneralForBinaryDecision(ContextVariable model)
        {
            // Inputs to this process are the variables ctxTable, ctxIdx, ivlCurrRange and ivlOffset.
            // Outputs of this process are the decoded value binVal and the updated variables ivlCurrRange and ivlOffset

            int qRangeIdx = (ivlCurrRange >> 6) & 3;
            int ivlLpsRange = rangeTabLps[model.pStateIndex, qRangeIdx];
            ivlCurrRange -= ivlLpsRange;

            bool binVal;

            if (ivlOffset >= ivlCurrRange)
            {
                // LPS
                binVal = !model.valMps;
                ivlOffset -= ivlCurrRange;
                ivlCurrRange = ivlLpsRange;

                if (model.pStateIndex == 0)
                    model.valMps = !model.valMps;

                model.pStateIndex = transIdxLps[model.pStateIndex];
            }
            else
            {
                // MPS
                binVal = model.valMps;

                model.pStateIndex = transIdxMps[model.pStateIndex];
            }

            Renormalization();

            return binVal ? 1 : 0;
        }

        // 9.3.4.3.3 Renormalization process in the arithmetic decoding engine
        void Renormalization()
        {
            while (ivlCurrRange < 256)
            {
                ivlCurrRange <<= 1;
                ivlOffset <<= 1;
                ivlOffset |= stream.Read(1);
            }
        }

        // 9.3.4.3.4 Bypass decoding process for binary decisions
        int DecodeBypass()
        {
            ivlOffset <<= 1;
            ivlOffset |= stream.Read(1);

            if (ivlOffset >= ivlCurrRange)
            {
                ivlOffset -= ivlCurrRange;
                return 1;
            }
            else
            {
                return 0;
            }
            // returns binVal
        }

        // 9.3.4.3.5 Decoding process for binary decisions before termination
        int DecodeBeforeTermination()
        {
            ivlCurrRange -= 2;

            if (ivlOffset >= ivlCurrRange)
            {
                return 1;
            }
            else
            {
                Renormalization();
                return 0;
            }
            // returns binVal
        }

        // 9.3.4.2.2 Derivation process of ctxInc using left and above syntax elements
        private int GetContextIndexFromNeighborState(int x0, int y0, HeicPicture picture, int cqtDepth)
        {

            bool availableL = picture.CheckZScanAvaliability(x0, y0, x0 - 1, y0);
            bool availableA = picture.CheckZScanAvaliability(x0, y0, x0, y0 - 1);

            bool condL = x0 > 0 && picture.CtDepth[x0 - 1, y0] > cqtDepth;
            bool condA = y0 > 0 && picture.CtDepth[x0, y0 - 1] > cqtDepth;

            return (condL && availableL ? 1 : 0) + (condA && availableA ? 1 : 0);
        }

        #endregion

        #region Syncing segment

        public void SyncTables()
        {
            for (CabacType cabacType = 0; (int)cabacType < 40; cabacType++)
            {
                contextTableSync[(int)cabacType] = new ContextVariable[contextTable[(int)cabacType].Length];

                for (int i = 0; i < contextTable[(int)cabacType].Length; i++)
                {
                    contextTableSync[(int)cabacType][i] = new ContextVariable();
                    contextTableSync[(int)cabacType][i].pStateIndex = contextTable[(int)cabacType][i].pStateIndex;
                    contextTableSync[(int)cabacType][i].valMps = contextTable[(int)cabacType][i].valMps;
                }
            }

            for (int i = 0; i < 4; i++)
            {
                tableStatCoeffSync[i] = StatCoeff[i];
            }
        }

        public void RestoreSyncedTables()
        {
            for (CabacType cabacType = 0; (int)cabacType < 40; cabacType++)
            {
                contextTable[(int)cabacType] = new ContextVariable[contextTableSync[(int)cabacType].Length];

                for (int i = 0; i < contextTableSync[(int)cabacType].Length; i++)
                {
                    contextTable[(int)cabacType][i] = new ContextVariable();
                    contextTable[(int)cabacType][i].pStateIndex = contextTableSync[(int)cabacType][i].pStateIndex;
                    contextTable[(int)cabacType][i].valMps = contextTableSync[(int)cabacType][i].valMps;
                }
            }

            for (int i = 0; i < 4; i++)
            {
                StatCoeff[i] = tableStatCoeffSync[i];
            }
        }

        #endregion

        #region Initialize segment


        // 9.3.2.6 Initialization process for the arithmetic decoding engine
        public void Initialization(slice_segment_header slice_header)
        {

            // The bitstream shall not contain data that result in a value of ivlOffset being equal to 510 or 511.
            ivlCurrRange = 510;
            ivlOffset = stream.Read(9);

            int SliceQpY = slice_header.SliceQPY;
            int initType = slice_header.initType;

            InitializationOfContextVariables(SliceQpY, initType);

            for (int i = 0; i < 4; i++)
            {
                StatCoeff[i] = 0;
            }

            InitializationOfPalettePredictorEntries(slice_header);
        }

        public void ResetStreamState()
        {
            ivlCurrRange = 510;
            ivlOffset = stream.Read(9);
        }


        private void InitializationOfPalettePredictorEntries(slice_segment_header slice_header)
        {
            // Outputs of this process are the initialized palette predictor variables
            // PredictorPaletteSize and PredictorPaletteEntries

            var pps = slice_header.pps;
            var sps = pps.sps;

            byte numComps = ((int)sps.ChromaArrayType == 0) ? (byte)1 : (byte)3;

            if (pps.pps_scc_ext?.pps_palette_predictor_initializers_present_flag ?? false)
            {
                PredictorPaletteSize = pps.pps_scc_ext.pps_num_palette_predictor_initializers;

                PredictorPaletteEntries = new ushort[numComps, PredictorPaletteSize];

                for (int comp = 0; comp < numComps; comp++)
                    for (int i = 0; i < PredictorPaletteSize; i++)
                        PredictorPaletteEntries[comp,i] = pps.pps_scc_ext.pps_palette_predictor_initializer[comp,i];

            }
            else if (sps.sps_scc_ext?.sps_palette_predictor_initializers_present_flag ?? false)
            {
                PredictorPaletteSize = sps.sps_scc_ext.sps_num_palette_predictor_initializers_minus1 + 1;

                PredictorPaletteEntries = new ushort[numComps, PredictorPaletteSize];

                for (int comp = 0; comp < numComps; comp++)
                    for (int i = 0; i < PredictorPaletteSize; i++)
                        PredictorPaletteEntries[comp, i] = sps.sps_scc_ext.sps_palette_predictor_initializer[comp, i];

            }
            else
            {
                PredictorPaletteSize = 0;
                PredictorPaletteEntries = new ushort[numComps, 0];
            }

        }

        private void InitializationOfContextVariables(int SliceQpY, int initType)
        {
            for (CabacType cabacType = 0; (int)cabacType < 40; cabacType++)
            {
                var init_value = Getinit_valueByType(cabacType, initType);
                contextTable[(int)cabacType] = new ContextVariable[init_value.Length];

                for (int i = 0; i < init_value.Length; i++)
                {
                    contextTable[(int)cabacType][i] = InitContextVariable(SliceQpY, init_value[i]);
                }
            }
        }

        private ContextVariable InitContextVariable(int SliceQpY, byte init_value)
        {
            // Outputs of this process are the initialized CABAC context variables indexed by ctxTable and ctxIdx.
            //
            // Table 9-5 to Table 9-37 contain the values of the 8 bit variable init_value used in the initialization
            // of context variables that are assigned to all syntax elements in clauses 7.3.8.1 through 7.3.8.12,
            // except end_of_slice_segment_flag, end_of_subset_one_bit and pcm_flag.


            byte slopeIdx = (byte)(init_value >> 4);  // 4 bit
            byte offsetIdx = (byte)(init_value & 15); // 4 bit
            sbyte m = (sbyte)(slopeIdx * 5 - 45);
            sbyte n = (sbyte)((offsetIdx << 3) - 16);

            int preCtxState = MathExtra.Clip3(1, 126, ((m * MathExtra.Clip3(0, 51, SliceQpY)) >> 4) + n);

            // the value of the most probable symbol as further described in clause 9.3.4.3
            bool valMps = preCtxState > 63;
            // a probability state index
            byte pStateIdx = (byte)(valMps ? (preCtxState - 64) : (63 - preCtxState)); // 4 bit

            return new ContextVariable() { pStateIndex = pStateIdx, valMps = valMps };
        }

        private byte[] Getinit_valueByType(CabacType type, int initType)
        {
            if (initType == 0)
            {
                if (type == CabacType.pred_mode_flag ||
                    type == CabacType.rqt_root_cbf ||
                    type == CabacType.merge_flag ||
                    type == CabacType.merge_idx ||
                    type == CabacType.mvp_flag ||
                    type == CabacType.abs_mvd_greater0_flag ||
                    type == CabacType.abs_mvd_greater1_flag ||
                    type == CabacType.cu_skip_flag ||
                    type == CabacType.inter_pred_idc ||
                    type == CabacType.ref_idx ||
                    type == CabacType.explicit_rdpcm_flag ||
                    type == CabacType.explicit_rdpcm_dir_flag)
                    return new byte[0];
            }

            switch (type)
            {
                // variables

                case CabacType.sao_merge_flag:
                    return new byte[] { init_value_sao_merge_flag[initType] };
                case CabacType.sao_type_idx:
                    return new byte[] { init_value_sao_type_idx_flag[initType] };
                case CabacType.cu_transquant_bypass_flag:
                    return new byte[] { init_value_cu_transquant_bypass_flag[initType] };
                case CabacType.pred_mode_flag:
                    return new byte[] { init_value_pred_mode_flag[initType - 1] };
                case CabacType.prev_intra_luma_pred_flag:
                    return new byte[] { init_value_prev_intra_luma_pred_flag[initType] };
                case CabacType.intra_chroma_pred_mode:
                    return new byte[] { init_value_intra_chroma_pred_mode[initType] };
                case CabacType.rqt_root_cbf:
                    return new byte[] { init_value_rqt_root_cbf[initType - 1] };
                case CabacType.merge_flag:
                    return new byte[] { init_value_merge_flag[initType - 1] };
                case CabacType.merge_idx:
                    return new byte[] { init_value_merge_idx[initType - 1] };
                case CabacType.mvp_flag:
                    return new byte[] { init_value_mvp_flag[initType - 1] };
                case CabacType.abs_mvd_greater0_flag:
                    return new byte[] { init_value_abs_mvd_greater_flag[(initType - 1) * 2] };
                case CabacType.abs_mvd_greater1_flag:
                    return new byte[] { init_value_abs_mvd_greater_flag[(initType - 1) * 2 + 1] };
                case CabacType.transform_skip_flag:
                case CabacType.explicit_rdpcm_flag:
                case CabacType.explicit_rdpcm_dir_flag:
                    return new byte[] { 139, 139 };
                case CabacType.palette_mode_flag:
                case CabacType.tu_residual_act_flag:
                case CabacType.copy_above_palette_indices_flag:
                case CabacType.copy_above_indices_for_final_run_flag:
                case CabacType.palette_transpose_flag:
                case CabacType.cu_chroma_qp_offset_flag:
                case CabacType.cu_chroma_qp_offset_idx:
                    return new byte[] { 154 };

                // arrays

                case CabacType.split_cu_flag:
                    return init_value_split_cu_flag[initType];
                case CabacType.cu_skip_flag:
                    return init_value_cu_skip_flag[initType - 1];
                case CabacType.part_mode:
                    return init_value_part_mode[initType];
                case CabacType.inter_pred_idc:
                    return init_value_inter_pred_idc[initType - 1];
                case CabacType.ref_idx:
                    return init_value_ref_idx[initType - 1];
                case CabacType.split_transform_flag:
                    return init_value_split_transform_flag[initType];
                case CabacType.cbf_luma:
                    return init_value_cbf_luma[initType];
                case CabacType.cbf_chroma:
                    return init_value_cbf_chroma[initType];
                case CabacType.last_sig_coeff_x_prefix:
                case CabacType.last_sig_coeff_y_prefix:
                    return init_value_last_sig_coeff_prefix[initType];
                case CabacType.coded_sub_block_flag:
                    return init_value_coded_sub_block_flag[initType];
                case CabacType.sig_coeff_flag:
                    return init_value_sig_coeff_flag[initType];
                case CabacType.coeff_abs_level_greater1_flag:
                    return init_value_coeff_abs_level_greater1_flag[initType];
                case CabacType.coeff_abs_level_greater2_flag:
                    return init_value_coeff_abs_level_greater2_flag[initType];
                case CabacType.palette_run_prefix:
                case CabacType.log2_res_scale_abs_plus1:
                    return new byte[] { 154, 154, 154, 154, 154, 154, 154, 154 };
                case CabacType.cu_qp_delta_abs:
                case CabacType.res_scale_sign_flag:
                    return new byte[] { 154, 154 };
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        #endregion

        #region Initialize value constants

        private static readonly byte[] init_value_sao_merge_flag = { 153, 153, 153 };
        private static readonly byte[] init_value_sao_type_idx_flag = { 200, 185, 160 };
        private static readonly byte[][] init_value_split_cu_flag = {
            new byte[] { 139, 141, 157 },
            new byte[] { 107, 139, 126 },
            new byte[] { 107, 139, 126 },
        };
        private static readonly byte[] init_value_cu_transquant_bypass_flag = { 154, 154, 154 };
        private static readonly byte[][] init_value_cu_skip_flag = {
            new byte[] { 197,185,201 },
            new byte[] { 197,185,201 },
        };
        private static readonly byte[] init_value_pred_mode_flag = { 149, 134 };
        private static readonly byte[][] init_value_part_mode = {
            new byte[] { 184 },
            new byte[] { 154, 139, 154, 154 },
            new byte[] { 154, 139, 154, 154 } 
        };
        private static readonly byte[] init_value_prev_intra_luma_pred_flag = { 184, 154, 183 };
        private static readonly byte[] init_value_intra_chroma_pred_mode = { 63, 152, 152 };
        private static readonly byte[] init_value_rqt_root_cbf = { 79, 79 };
        private static readonly byte[] init_value_merge_flag = { 110, 154 };
        private static readonly byte[] init_value_merge_idx = { 122, 137 };
        private static readonly byte[][] init_value_inter_pred_idc = {
            new byte[] { 95, 79, 63, 31, 31 },
            new byte[] { 95, 79, 63, 31, 31 }
        };
        private static readonly byte[][] init_value_ref_idx = {
            new byte[] { 153, 153 },
            new byte[] { 153, 153 }
        };
        private static readonly byte[] init_value_mvp_flag = { 168, 168 };
        private static readonly byte[][] init_value_split_transform_flag = {
            new byte[] { 153, 138, 138 },
            new byte[] { 124, 138, 94 },
            new byte[] { 224, 167, 122 }
        };
        private static readonly byte[][] init_value_cbf_luma = {
            new byte[] { 111, 141 },
            new byte[] { 153, 111 },
            new byte[] { 153, 111 }
        };
        private static readonly byte[][] init_value_cbf_chroma = {
            new byte[] { 94, 138, 182, 154, 154 },
            new byte[] { 149, 107, 167, 154, 154 },
            new byte[] { 149, 92, 167, 154, 154 }
        };
        private static readonly byte[] init_value_abs_mvd_greater_flag = { 140, 198, 169, 198 };
        private static readonly byte[][] init_value_last_sig_coeff_prefix = {
            new byte[] { 110, 110, 124, 125, 140, 153, 125, 127, 140, 109, 111, 143, 127, 111,  79, 108, 123,  63 },
            new byte[] { 125, 110,  94, 110,  95,  79, 125, 111, 110,  78, 110, 111, 111,  95,  94, 108, 123, 108 },
            new byte[] { 125, 110, 124, 110,  95,  94, 125, 111, 111,  79, 125, 126, 111, 111,  79, 108, 123,  93 }
        };
        private static readonly byte[][] init_value_coded_sub_block_flag = {
            new byte[] { 91, 171, 134, 141 },
            new byte[] { 121, 140, 61, 154 },
            new byte[] { 121, 140, 61, 154 }
        };
        private static readonly byte[][] init_value_sig_coeff_flag = {
            new byte[] {
                111,  111,  125,  110,  110,   94,  124,  108,  124,  107,  125,  141,  179,  153,  125,  107,
                125,  141,  179,  153,  125,  107,  125,  141,  179,  153,  125,  140,  139,  182,  182,  152,
                136,  152,  136,  153,  136,  139,  111,  136,  139,  111,  141,  111
            },
            new byte[] {
                155,  154,  139,  153,  139,  123,  123,   63,  153,  166,  183,  140,  136,  153,  154,  166,
                183,  140,  136,  153,  154,  166,  183,  140,  136,  153,  154,  170,  153,  123,  123,  107,
                121,  107,  121,  167,  151,  183,  140,  151,  183,  140,  140,  140
            },
            new byte[] {
                170,  154,  139,  153,  139,  123,  123,   63,  124,  166,  183,  140,  136,  153,  154,  166,
                183,  140,  136,  153,  154,  166,  183,  140,  136,  153,  154,  170,  153,  138,  138,  122,
                121,  122,  121,  167,  151,  183,  140,  151,  183,  140,  140,  140
            },
        };
        private static readonly byte[][] init_value_coeff_abs_level_greater1_flag = {
            new byte[] {
                140,  92, 137, 138, 140, 152, 138, 139, 153,  74, 149,  92,
                139, 107, 122, 152, 140, 179, 166, 182, 140, 227, 122, 197
            },
            new byte[] {
                154, 196, 196, 167, 154, 152, 167, 182, 182, 134, 149, 136,
                153, 121, 136, 137, 169, 194, 166, 167, 154, 167, 137, 182
            },
            new byte[] {
                154, 196, 167, 167, 154, 152, 167, 182, 182, 134, 149, 136,
                153, 121, 136, 122, 169, 208, 166, 167, 154, 152, 167, 182
            }
        };
        private static readonly byte[][] init_value_coeff_abs_level_greater2_flag = {
            new byte[] { 138, 153, 136, 167, 152, 152 },
            new byte[] { 107, 167, 91, 122, 107, 167 },
            new byte[] { 107, 167, 91, 107, 107, 167 }
        };

        // LPS = Least Probable Symbol
        // MPS = Most Probable Symbol

        // Table 9-46 – Specification of rangeTabLps depending on the values of pStateIdx and qRangeIdx 
        public static readonly byte[,] rangeTabLps = new byte[64, 4]
        {
            { 128, 176, 208, 240},
            { 128, 167, 197, 227},
            { 128, 158, 187, 216},
            { 123, 150, 178, 205},
            { 116, 142, 169, 195},
            { 111, 135, 160, 185},
            { 105, 128, 152, 175},
            { 100, 122, 144, 166},
            {  95, 116, 137, 158},
            {  90, 110, 130, 150},
            {  85, 104, 123, 142},
            {  81,  99, 117, 135},
            {  77,  94, 111, 128},
            {  73,  89, 105, 122},
            {  69,  85, 100, 116},
            {  66,  80,  95, 110},
            {  62,  76,  90, 104},
            {  59,  72,  86,  99},
            {  56,  69,  81,  94},
            {  53,  65,  77,  89},
            {  51,  62,  73,  85},
            {  48,  59,  69,  80},
            {  46,  56,  66,  76},
            {  43,  53,  63,  72},
            {  41,  50,  59,  69},
            {  39,  48,  56,  65},
            {  37,  45,  54,  62},
            {  35,  43,  51,  59},
            {  33,  41,  48,  56},
            {  32,  39,  46,  53},
            {  30,  37,  43,  50},
            {  29,  35,  41,  48},
            {  27,  33,  39,  45},
            {  26,  31,  37,  43},
            {  24,  30,  35,  41},
            {  23,  28,  33,  39},
            {  22,  27,  32,  37},
            {  21,  26,  30,  35},
            {  20,  24,  29,  33},
            {  19,  23,  27,  31},
            {  18,  22,  26,  30},
            {  17,  21,  25,  28},
            {  16,  20,  23,  27},
            {  15,  19,  22,  25},
            {  14,  18,  21,  24},
            {  14,  17,  20,  23},
            {  13,  16,  19,  22},
            {  12,  15,  18,  21},
            {  12,  14,  17,  20},
            {  11,  14,  16,  19},
            {  11,  13,  15,  18},
            {  10,  12,  15,  17},
            {  10,  12,  14,  16},
            {   9,  11,  13,  15},
            {   9,  11,  12,  14},
            {   8,  10,  12,  14},
            {   8,   9,  11,  13},
            {   7,   9,  11,  12},
            {   7,   9,  10,  12},
            {   7,   8,  10,  11},
            {   6,   8,   9,  11},
            {   6,   7,   9,  10},
            {   6,   7,   8,   9},
            {   2,   2,   2,   2}
        };

        // Table 9-47 – State transition table pStateIdx to transIdxMps
        public static readonly byte[] transIdxMps = new byte[64]
        {
             1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15, 16,
            17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32,
            33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48,
            49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 62, 63
        };

        // Table 9-47 – State transition table pStateIdx to transIdxLps
        public static readonly byte[] transIdxLps = new byte[64]
        {
             0,  0,  1,  2,  2,  4,  4,  5,  6,  7,  8,  9,  9, 11, 11, 12,
            13, 13, 15, 15, 16, 16, 18, 18, 19, 19, 21, 21, 22, 22, 23, 24,
            24, 25, 26, 26, 27, 27, 28, 29, 29, 30, 30, 30, 31, 32, 32, 33,
            33, 33, 34, 34, 35, 35, 35, 36, 36, 36, 37, 37, 37, 38, 38, 63
        };

        // Table 9-50 – Specification of ctxIdxMap
        public static readonly byte[] ctxIdxMap = {
            0, 1, 4, 5, 2, 3, 4, 5, 6, 6, 8, 8, 7, 7, 8
        };

        #endregion
    }

}

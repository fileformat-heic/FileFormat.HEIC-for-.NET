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
    internal class palette_coding
    {
        internal int num_signalled_palette_entries;
        internal int num_palette_indices;
        internal bool copy_above_indices_for_final_run_flag;
        internal bool palette_transpose_flag;
        internal bool palette_escape_val_present_flag;
        internal int palette_idx_idc;
        internal bool copy_above_palette_indices_flag;
        internal int palette_run_prefix;
        internal int palette_run_suffix;
        internal int palette_escape_val;

        internal int[][] new_palette_entries;

        public int NumPredictedPaletteEntries { get; }
        public int PredictorPaletteSize { get; } // IS NOT DEFINED
        public bool[] PalettePredictorEntryReuseFlags { get; }
        public int[] PaletteIndexIdc { get; internal set; }
        public int PaletteMaxRun { get; internal set; }
        public int PaletteRun { get; internal set; }
        public int RunToEnd { get; internal set; }
        public int[,] PaletteIndexMap { get; internal set; }
        public int[,,] PaletteEscapeVal { get; internal set; }
        public bool[,] CopyAboveIndicesFlag { get; }
        public int CurrPaletteIndex { get; internal set; }

        public palette_coding(BitStreamWithNalSupport stream, slice_segment_header header, 
            int x0, int y0, int nCbS)
        {
            bool palettePredictionFinished = false;
            NumPredictedPaletteEntries = 0;
            PalettePredictorEntryReuseFlags = new bool[PredictorPaletteSize];

            for (int predictorEntryIdx = 0; predictorEntryIdx < PredictorPaletteSize &&
                !palettePredictionFinished && NumPredictedPaletteEntries < header.pps.sps.sps_scc_ext.palette_max_size;
                predictorEntryIdx++)
            {
                int palette_predictor_run = stream.ReadAev(); // ae(v)
                if (palette_predictor_run != 1)
                {
                    if (palette_predictor_run > 1)
                        predictorEntryIdx += palette_predictor_run - 1;

                    PalettePredictorEntryReuseFlags[predictorEntryIdx] = true;
                    NumPredictedPaletteEntries++;
                }
                else
                {
                    palettePredictionFinished = true;
                }
            }

            if (NumPredictedPaletteEntries < header.pps.sps.sps_scc_ext.palette_max_size)
                num_signalled_palette_entries = stream.ReadAev(); // ae(v)

            int numComps = (header.pps.sps.ChromaArrayType == 0) ? 1 : 3;

            new_palette_entries = new int[numComps][];
            for (int cIdx = 0; cIdx < numComps; cIdx++)
            {
                new_palette_entries[cIdx] = new int[num_signalled_palette_entries];

                for (int i = 0; i < num_signalled_palette_entries; i++)
                {
                    new_palette_entries[cIdx][i] = stream.ReadAev(); // ae(v)
                }
            }

            if (CurrentPaletteSize != 0)
                palette_escape_val_present_flag = stream.ReadAevFlag(); // ae(v)

            if (MaxPaletteIndex > 0)
            {
                num_palette_indices = stream.ReadAev() + 1; // ae(v)
                int adjust = 0;
                PaletteIndexIdc = new int[num_palette_indices - 1];
                for (int i = 0; i <= num_palette_indices - 1; i++)
                {
                    if (MaxPaletteIndex - adjust > 0 ) {
                        palette_idx_idc = stream.ReadAev(); // ae(v)
                        PaletteIndexIdc[i] = palette_idx_idc;
                    }
                    adjust = 1;
                }
                copy_above_indices_for_final_run_flag = stream.ReadAevFlag(); // ae(v)
                palette_transpose_flag = stream.ReadAevFlag(); // ae(v)
            }

            if (palette_escape_val_present_flag) {
                //new delta_qp();
                //if (!cu_transquant_bypass_flag)
                //    new chroma_qp_offset();
            }

            int remainingNumIndices = num_palette_indices;
            int PaletteScanPos = 0;
            int log2BlockSize = (int)Math.Log(nCbS, 2);

            while(PaletteScanPos < nCbS * nCbS ) {
                int xC = x0 + Scans.ScanOrder[log2BlockSize][3][PaletteScanPos][0];
                int yC = y0 + Scans.ScanOrder[log2BlockSize][3][PaletteScanPos][1];

                int xcPrev = 0;
                int ycPrev = 0;

                if (PaletteScanPos > 0) {
                    xcPrev = x0 + Scans.ScanOrder[log2BlockSize][3][PaletteScanPos - 1][0];
                    ycPrev = y0 + Scans.ScanOrder[log2BlockSize][3][PaletteScanPos - 1][1];
                }

                PaletteRun = nCbS * nCbS - PaletteScanPos;
                RunToEnd = 1;
                CopyAboveIndicesFlag[xC, yC] = false;

                if (MaxPaletteIndex > 0)
                {
                    if (PaletteScanPos >= nCbS && CopyAboveIndicesFlag[xcPrev, ycPrev] == false)
                    {
                        if (remainingNumIndices > 0 && PaletteScanPos < nCbS * nCbS - 1)
                        {
                            copy_above_palette_indices_flag = stream.ReadAevFlag(); // ae(v)
                            CopyAboveIndicesFlag[xC, yC] = copy_above_palette_indices_flag;
                        }
                        else
                        {
                            if (PaletteScanPos == nCbS * nCbS - 1 && remainingNumIndices > 0)
                                CopyAboveIndicesFlag[xC, yC] = false;
                            else
                                CopyAboveIndicesFlag[xC, yC] = true;
                        }
                    }
                }

                if (CopyAboveIndicesFlag[xC, yC] == false)
                {
                    int currNumIndices = num_palette_indices - remainingNumIndices;
                    CurrPaletteIndex = PaletteIndexIdc[currNumIndices];
                }

                if (MaxPaletteIndex > 0)
                {
                    if (CopyAboveIndicesFlag[xC, yC] == false)
                        remainingNumIndices -= 1;

                    if (remainingNumIndices > 0 || 
                        CopyAboveIndicesFlag[xC, yC] != copy_above_indices_for_final_run_flag)
                    {
                        PaletteMaxRun = nCbS * nCbS - PaletteScanPos - remainingNumIndices - 
                            (copy_above_indices_for_final_run_flag ? 1 : 0);
                        RunToEnd = 0;

                        if (PaletteMaxRun - 1 > 0)
                        {
                            palette_run_prefix = stream.ReadAev(); // ae(v)
                            if ((palette_run_prefix > 1) && (PaletteMaxRun - 1 !=
                                (1 << (palette_run_prefix - 1))))
                                palette_run_suffix = stream.ReadAev(); // ae(v)
                        }
                    }
                }

                int runPos = 0;
                while (runPos <= PaletteRun - 1)
                {
                    int xR = x0 + Scans.ScanOrder[log2BlockSize][3][PaletteScanPos][0];
                    int yR = y0 + Scans.ScanOrder[log2BlockSize][3][PaletteScanPos][1];

                    if (CopyAboveIndicesFlag[xC, yC] == false)
                    {
                        CopyAboveIndicesFlag[xR, yR] = false;
                        PaletteIndexMap[xR, yR] = CurrPaletteIndex;
                    }
                    else
                    {
                        CopyAboveIndicesFlag[xR, yR] = true;
                        PaletteIndexMap[xR, yR] = PaletteIndexMap[xR, yR - 1];
                    }

                    runPos++;
                    PaletteScanPos++;
                }
            }

            if (palette_escape_val_present_flag)
            {
                for (int cIdx = 0; cIdx < numComps; cIdx++) {
                    for (int sPos = 0; sPos < nCbS * nCbS; sPos++)
                    {
                        int xC = x0 + Scans.ScanOrder[log2BlockSize][3][sPos][0];
                        int yC = y0 + Scans.ScanOrder[log2BlockSize][3][sPos][1];

                        if (PaletteIndexMap[xC, yC] == MaxPaletteIndex)
                        {
                            if (cIdx == 0 || 
                                (xC % 2 == 0 && yC % 2 == 0 && header.pps.sps.ChromaArrayType == 1) || 
                                (xC % 2 == 0 && !palette_transpose_flag && header.pps.sps.ChromaArrayType == 2) ||
                                (yC % 2 == 0 && palette_transpose_flag && header.pps.sps.ChromaArrayType == 2) || 
                                header.pps.sps.ChromaArrayType == 3)
                            {
                                palette_escape_val = stream.ReadAev(); // ae(v)
                                PaletteEscapeVal[cIdx, xC, yC] = palette_escape_val;
                            }
                        }
                    }
                }
            }
        }

        internal int CurrentPaletteSize => NumPredictedPaletteEntries + num_signalled_palette_entries;
        internal int MaxPaletteIndex => CurrentPaletteSize - 1 + (palette_escape_val_present_flag ? 1 : 0);

    }
}

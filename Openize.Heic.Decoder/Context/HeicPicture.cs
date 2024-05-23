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

namespace Openize.Heic.Decoder
{
    internal class HeicPicture
    {
        public Dictionary<int, slice_segment_header> SliceHeaders { get; }
        public Dictionary<int, slice_segment_data> SliceUnits { get; }

        public video_parameter_set_rbsp vps;
        public seq_parameter_set_rbsp sps;
        public pic_parameter_set_rbsp pps;
        public DecoderContext Context;
        public NalHeader NalHeader;

        public int PicOrderCntVal;
        public int picture_order_cnt_lsb;

        public int[][,] SaoTypeIdx;
        public int[][,][] SaoOffsetVal;
        public int[][,] SaoEoClass;

        public int[][,] ResScaleVal;

        public int[][,][] sao_offset_abs;
        public int[][,][] sao_offset_sign;
        public int[][,] sao_band_position;

        public uint[,] SliceAddrRs;
        public int[,] SliceHeaderIndex;
        public IntraPredMode[,] IntraPredModeY;
        public IntraPredMode[,] IntraPredModeC;

        public int[][,] TransCoeffLevel;

        public int[,] CtDepth;
        public PredMode[,] CuPredMode;
        public int[,] QpY;
        public int[,] Log2CbSize;
        public bool[,] merge_flag;

        public bool[,] cu_skip_flag;
        public bool[,] palette_mode_flag;
        public bool[,] pcm_flag;
        public bool[,] prev_intra_luma_pred_flag;
        public int[,] mpm_idx;
        public IntraPredMode[,] rem_intra_luma_pred_mode;
        public byte[,] intra_chroma_pred_mode;

        public bool[,] tu_residual_act_flag;

        //public bool[,][] split_transform_flag;
        public bool[,][] cbf_cb;
        public bool[,][] cbf_cr;
        public bool[,][] cbf_luma;

        //public bool[,][] transform_skip_flag;
        public bool[,][] explicit_rdpcm_flag;
        public bool[,][] explicit_rdpcm_dir_flag;

        public int[] pcm_sample_luma;
        public int[] pcm_sample_chroma;

        public ushort[][,] pixels;

        public HeicPicture(slice_segment_header slice_header, NalHeader nal_header)
        {
            SliceHeaders = new Dictionary<int, slice_segment_header>();
            SliceUnits = new Dictionary<int, slice_segment_data>();

            pps = slice_header.pps;
            sps = pps.sps;
            vps = sps.vps;
            NalHeader = nal_header;

            uint widthMax = sps.pic_width_in_luma_samples;
            uint heightMax = sps.pic_height_in_luma_samples;

            widthMax = sps.PicWidthInCtbsY << sps.CtbLog2SizeY;
            heightMax = sps.PicHeightInCtbsY << sps.CtbLog2SizeY;

            int chroma_count = sps.ChromaArrayType != 0 ? 3 : 1;

            SaoTypeIdx = new int[chroma_count][,];
            SaoEoClass = new int[chroma_count][,];
            ResScaleVal = new int[chroma_count][,];
            SaoOffsetVal = new int[chroma_count][,][];
            sao_offset_abs = new int[chroma_count][,][];
            sao_offset_sign = new int[chroma_count][,][];
            sao_band_position = new int[chroma_count][,];
            pixels = new ushort[chroma_count][,];
            TransCoeffLevel = new int[chroma_count][,];

            for (int i = 0; i < chroma_count; i++)
            {
                SaoTypeIdx[i] = new int[sps.PicWidthInCtbsY, sps.PicHeightInCtbsY];
                SaoEoClass[i] = new int[sps.PicWidthInCtbsY, sps.PicHeightInCtbsY];
                ResScaleVal[i] = new int[sps.PicWidthInCtbsY, sps.PicHeightInCtbsY];
                SaoOffsetVal[i] = new int[sps.PicWidthInCtbsY, sps.PicHeightInCtbsY][];
                sao_offset_abs[i] = new int[sps.PicWidthInCtbsY, sps.PicHeightInCtbsY][];
                sao_offset_sign[i] = new int[sps.PicWidthInCtbsY, sps.PicHeightInCtbsY][];
                sao_band_position[i] = new int[sps.PicWidthInCtbsY, sps.PicHeightInCtbsY];

                pixels[i] = new ushort[widthMax, heightMax];
                TransCoeffLevel[i] = new int[widthMax, heightMax];
            }

            CtDepth = new int[widthMax, heightMax];
            CuPredMode = new PredMode[widthMax, heightMax];
            QpY = new int[widthMax, heightMax];
            Log2CbSize = new int[widthMax, heightMax];
            IntraPredModeY = new IntraPredMode[widthMax, heightMax];
            IntraPredModeC = new IntraPredMode[widthMax, heightMax];


            merge_flag = new bool[widthMax, heightMax];
            cu_skip_flag = new bool[widthMax, heightMax];
            palette_mode_flag = new bool[widthMax, heightMax];
            pcm_flag = new bool[widthMax, heightMax];
            prev_intra_luma_pred_flag = new bool[widthMax, heightMax];

            mpm_idx = new int[widthMax, heightMax];
            rem_intra_luma_pred_mode = new IntraPredMode[widthMax, heightMax];
            intra_chroma_pred_mode = new byte[widthMax, heightMax];

            tu_residual_act_flag = new bool[widthMax, heightMax];

            //split_transform_flag = new bool[xMax, yMax][];
            cbf_cb = new bool[widthMax, heightMax][];
            cbf_cr = new bool[widthMax, heightMax][];
            cbf_luma = new bool[widthMax, heightMax][];

            //transform_skip_flag = new bool[xMax, yMax][];
            explicit_rdpcm_flag = new bool[widthMax, heightMax][];
            explicit_rdpcm_dir_flag = new bool[widthMax, heightMax][];
        }

        // 6.4.1 Derivation process for z-scan order block availability
        // 
        // The luma location ( xCurr, yCurr ) of the top-left sample of the current block
        // relative to the top-left luma sample of the current picture
        //
        // The luma location ( xNbY, yNbY ) covered by a neighbouring block relative
        // to the top-left luma sample of the current picture.
        internal bool CheckZScanAvaliability(int xCurr, int yCurr, int xNbY, int yNbY)
        {
            if (xNbY < 0 || yNbY < 0)
                return false;

            if (xNbY >= sps.pic_width_in_luma_samples || yNbY >= sps.pic_height_in_luma_samples)
                return false;

            // The minimum luma block address in z-scan order minBlockAddrCurr of the current block
            uint minBlockAddrCurr = pps.MinTbAddrZs[xCurr >> sps.MinTbLog2SizeY, yCurr >> sps.MinTbLog2SizeY];

            // The minimum luma block address in z-scan order minBlockAddrN of the neighbouring block covering the location (xNbY, yNbY)
            uint minBlockAddrN = pps.MinTbAddrZs[xNbY >> sps.MinTbLog2SizeY, yNbY >> sps.MinTbLog2SizeY];

            if (minBlockAddrN > minBlockAddrCurr)
                return false;

            // SliceAddrRs associated with the slice segment containing the current block
            int xCurrCtb = xCurr >> sps.CtbLog2SizeY;
            int yCurrCtb = yCurr >> sps.CtbLog2SizeY;
            int ctbCurrAddrRs = yCurrCtb * (int)sps.PicWidthInCtbsY + xCurrCtb;

            // SliceAddrRs associated with the slice segment containing the neighbouring block
            int xNbCtb = xNbY >> sps.CtbLog2SizeY;
            int yNbCtb = yNbY >> sps.CtbLog2SizeY;
            int ctbNbAddrRs = yNbCtb * (int)sps.PicWidthInCtbsY + xNbCtb;

            // the current block and the neighbouring block are in the same slice
            if (SliceAddrRs[xCurrCtb, yCurrCtb] != SliceAddrRs[xNbCtb, yNbCtb])
                return false;

            // the neighbouring block is contained in a different tile than the current block
            if (pps.TileIdFromRs[ctbCurrAddrRs] != pps.TileIdFromRs[ctbNbAddrRs])
                return false;

            return true;
        }

        // 6.4.2 Derivation process for prediction block availability
        //
        // the luma location ( xCb, yCb ) of the top-left sample of the current luma coding block
        // relative to the top-left luma sample of the current picture
        //
        // a variable nCbS specifying the size of the current luma coding block
        //
        // the luma location ( xPb, yPb ) of the top-left sample of the current luma prediction block
        // relative to the top-left luma sample of the current picture
        //
        // two variables nPbW and nPbH specifying the width and the height of the current luma prediction block
        //
        // a variable partIdx specifying the partition index of the current prediction unit within the current coding unit
        //
        // the luma location ( xNbY, yNbY ) covered by a neighbouring prediction block
        // relative to the top-left luma sample of the current picture
        //
        // Output of this process is the availability of the neighbouring prediction block covering the location ( xNbY, yNbY )
        internal bool CheckPredictionAvaliability(int xCb, int yCb, int nCbS, int xPb, int yPb, int nPbW, int nPbH, int partIdx, int xNbY, int yNbY)
        {
            bool sameCb =
                (xCb <= xNbY) && (yCb <= yNbY) &&
                (xCb + nCbS > xNbY) && (yCb + nCbS > yNbY);

            bool availableN;

            if (!sameCb)
                availableN = CheckZScanAvaliability(xPb, yPb, xNbY, yNbY);
            else if (
                nPbW << 1 == nCbS &&
                nPbH << 1 == nCbS &&
                partIdx == 1 &&
                yCb + nPbH <= yNbY &&
                xCb + nPbW > xNbY
                )
            {
                availableN = false;
            }
            else
                availableN = true;

            if (availableN && CuPredMode[xNbY, yNbY] == PredMode.MODE_INTRA)
                availableN = false;

            return availableN;
        }

        internal void SetCtDepth(int x0, int y0, int log2CbSize, int cqtDepth)
        {
            int nCbS = 1 << log2CbSize;
            for (int i = x0; i < x0 + nCbS; i++)
                for (int j = y0; j < y0 + nCbS; j++)
                    CtDepth[i, j] = cqtDepth;
        }
        internal void SetCuPredMode(int x0, int y0, int nCbS, PredMode mode)
        {
            for (int i = x0; i < x0 + nCbS; i++)
            {
                for (int j = y0; j < y0 + nCbS; j++)
                {
                    CuPredMode[i, j] = mode;

                    if(mode == PredMode.MODE_SKIP)
                        merge_flag[i, j] = true;
                }
            }
        }
        internal void SetPcmFlag(int x0, int y0, int nCbS, bool value)
        {
            for (int i = x0; i < x0 + nCbS; i++)
                for (int j = y0; j < y0 + nCbS; j++)
                    pcm_flag[i, j] = value;
        }
        internal void SetQpY(int x0, int y0, int log2CbSize, int qpY)
        {
            int nCbS = 1 << log2CbSize;
            for (int i = x0; i < x0 + nCbS; i++)
                for (int j = y0; j < y0 + nCbS; j++)
                    QpY[i, j] = qpY;
        }
        internal void SetLog2CbSize(int x0, int y0, int log2CbSize)
        {
            int nCbS = 1 << log2CbSize;
            for (int i = x0; i < x0 + nCbS; i++)
                for (int j = y0; j < y0 + nCbS; j++)
                    Log2CbSize[i, j] = log2CbSize;
        }

        // 8.4.2 Derivation process for luma intra prediction mode
        internal IntraPredMode[] GenerateIntraPredModeCandidates(int xPb, int yPb, int pbOffset, bool availableA, bool availableB)
        {
            IntraPredMode candIntraPredModeA, candIntraPredModeB;

            if (availableA == false)
            {
                candIntraPredModeA = IntraPredMode.INTRA_DC;
            }
            else if (CuPredMode[xPb - 1, yPb] != PredMode.MODE_INTRA || pcm_flag[xPb - 1, yPb])
            {
                candIntraPredModeA = IntraPredMode.INTRA_DC;
            }
            else
            {
                candIntraPredModeA = IntraPredModeY[xPb - 1, yPb];
            }

            if (availableB == false)
            {
                candIntraPredModeB = IntraPredMode.INTRA_DC;
            }
            else if (CuPredMode[xPb, yPb - 1] != PredMode.MODE_INTRA || pcm_flag[xPb, yPb - 1])
            {
                candIntraPredModeB = IntraPredMode.INTRA_DC;
            }
            else if (yPb - 1 < ((yPb >> sps.CtbLog2SizeY) << sps.CtbLog2SizeY))
            {
                candIntraPredModeB = IntraPredMode.INTRA_DC;
            }
            else
            {
                candIntraPredModeB = IntraPredModeY[xPb, yPb - 1];
            }

            IntraPredMode[] candModeList = new IntraPredMode[3];

            if (candIntraPredModeB == candIntraPredModeA)
            {
                if ((int)candIntraPredModeA < 2)
                {
                    candModeList[0] = IntraPredMode.INTRA_PLANAR;
                    candModeList[1] = IntraPredMode.INTRA_DC;
                    candModeList[2] = IntraPredMode.INTRA_ANGULAR26;
                }
                else
                {
                    candModeList[0] = candIntraPredModeA;
                    candModeList[1] = (IntraPredMode)(2 + (((int)candIntraPredModeA + 29) % 32));
                    candModeList[2] = (IntraPredMode)(2 + (((int)candIntraPredModeA - 2 + 1 ) % 32 ));
                }
            }
            else
            {
                candModeList[0] = candIntraPredModeA;
                candModeList[1] = candIntraPredModeB;

                if (candModeList[0] != IntraPredMode.INTRA_PLANAR && candModeList[1] != IntraPredMode.INTRA_PLANAR)
                {
                    candModeList[2] = IntraPredMode.INTRA_PLANAR;
                }
                else if (candModeList[0] != IntraPredMode.INTRA_DC && candModeList[1] != IntraPredMode.INTRA_DC)
                {
                    candModeList[2] = IntraPredMode.INTRA_DC;
                }
                else
                {
                    candModeList[2] = IntraPredMode.INTRA_ANGULAR26;
                }
            }

            return candModeList;
        }

        internal IntraPredMode SetIntraPredModeY(int xPb, int yPb, int pbSize, IntraPredMode[] candModeList)
        {
            IntraPredMode mode;

            if (prev_intra_luma_pred_flag[xPb, yPb])
            {
                mode = candModeList[mpm_idx[xPb, yPb]];
            }
            else
            {
                if (candModeList[0] > candModeList[1])
                    (candModeList[0], candModeList[1]) = (candModeList[1], candModeList[0]);

                if (candModeList[0] > candModeList[2])
                    (candModeList[0], candModeList[2]) = (candModeList[2], candModeList[0]);

                if (candModeList[1] > candModeList[2])
                    (candModeList[1], candModeList[2]) = (candModeList[2], candModeList[1]);

                mode = rem_intra_luma_pred_mode[xPb, yPb];

                for (int i = 0; i < 3; i++)
                {
                    if (mode >= candModeList[i])
                        mode++;
                }
            }

            for (int i = xPb; i < xPb + pbSize; i++)
                for (int j = yPb; j < yPb + pbSize; j++)
                    IntraPredModeY[i, j] = mode;

            return mode;
        }
        internal void SetIntraPredModeC(int xPb, int yPb, int pbSize, IntraPredMode mode)
        {
            for (int i = xPb; i < xPb + pbSize; i++)
                for (int j = yPb; j < yPb + pbSize; j++)
                    IntraPredModeC[i, j] = mode;
        }

        internal IntraPredMode DeriveIntraPredModeIndex(IntraPredMode intraLumaPredMode, byte intraChromaPredMode)
        {
            if (intraChromaPredMode == 4)
                return intraLumaPredMode;

            IntraPredMode candidate = intraChromaPredMode switch
            {
                0 => IntraPredMode.INTRA_PLANAR,
                1 => IntraPredMode.INTRA_ANGULAR26,
                2 => IntraPredMode.INTRA_ANGULAR10,
                3 => IntraPredMode.INTRA_DC
            };

            if (intraLumaPredMode == candidate)
                return IntraPredMode.INTRA_ANGULAR34;

            return candidate;
        }
    }
}

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
using System.Collections.Generic;
using System;

namespace Openize.Heic.Decoder
{
    internal class DecoderContext
    {
        public Dictionary<int, video_parameter_set_rbsp> VPS { get; }
        public Dictionary<int, seq_parameter_set_rbsp> SPS { get; }
        public Dictionary<int, pic_parameter_set_rbsp> PPS { get; }
        public Dictionary<uint, HeicPicture> Pictures { get; }

        public bool first_decoded_picture = true;
        public bool NoRaslOutputFlag = false;
        public bool HandleCraAsBlaFlag = false;
        public bool FirstAfterEndOfSequenceNAL = false;

        public int PicOrderCntMsb;
        public int prevPicOrderCntLsb;  // at precTid0Pic
        public int prevPicOrderCntMsb;  // at precTid0Pic

        // coding_unit
        internal bool cu_transquant_bypass_flag;
        internal bool[] transform_skip_flag = new bool[3];

        // CuQp
        public bool IsCuQpDeltaCoded { get; set; }
        public int CuQpDeltaVal { get; set; }
        public bool IsCuChromaQpOffsetCoded { get; set; }
        public int CuQpOffsetCb { get; set; }
        public int CuQpOffsetCr { get; set; }

        public int QpY { get; set; }
        public int QpCb { get; set; }
        public int QpCr { get; set; }
        public int LastQpYPrev { get; set; }
        public int CurrentQgX { get; set; }
        public int CurrentQgY { get; set; }
        public int CurrentQpY { get; set; }
        

        public DecoderContext()
        {
            VPS = new Dictionary<int, video_parameter_set_rbsp>();
            SPS = new Dictionary<int, seq_parameter_set_rbsp>();
            PPS = new Dictionary<int, pic_parameter_set_rbsp>();
            Pictures = new Dictionary<uint, HeicPicture>();

            prevPicOrderCntLsb = 0;
            prevPicOrderCntMsb = 0;
        }


        public void AddNalContext(NalUnitType type, NalUnit nalUnit)
        {
            switch (type)
            {
                case NalUnitType.VPS_NUT:
                    VPS.Add(((video_parameter_set_rbsp)nalUnit).vps_video_parameter_set_id, (video_parameter_set_rbsp)nalUnit);
                    break;
                case NalUnitType.SPS_NUT:
                    SPS.Add(((seq_parameter_set_rbsp)nalUnit).sps_seq_parameter_set_id, (seq_parameter_set_rbsp)nalUnit);
                    break;
                case NalUnitType.PPS_NUT:
                    PPS.Add(((pic_parameter_set_rbsp)nalUnit).pps_pic_parameter_set_id, (pic_parameter_set_rbsp)nalUnit);
                    break;
            }
        }

        // 8.3.1 Decoding process for picture order count
        public void DecodingPictureOrderCount(HeicPicture picture)
        {
            var header = picture.NalHeader;
            var slice_header = picture.SliceHeaders[0];

            if (header.IsIrapPicture && NoRaslOutputFlag)
            {
                PicOrderCntMsb = 0;
            }
            else
            {
                int MaxPicOrderCntLsb = (int)picture.sps.MaxPicOrderCntLsb;

                if ((slice_header.slice_pic_order_cnt_lsb < prevPicOrderCntLsb) &&
                    ((prevPicOrderCntLsb - slice_header.slice_pic_order_cnt_lsb) >= (MaxPicOrderCntLsb / 2)))
                {
                    PicOrderCntMsb = prevPicOrderCntMsb + MaxPicOrderCntLsb;
                }
                else if ((slice_header.slice_pic_order_cnt_lsb > prevPicOrderCntLsb) &&
                        ((slice_header.slice_pic_order_cnt_lsb - prevPicOrderCntLsb) > (MaxPicOrderCntLsb / 2)))
                {
                    PicOrderCntMsb = prevPicOrderCntMsb - MaxPicOrderCntLsb;
                }
                else
                {
                    PicOrderCntMsb = prevPicOrderCntMsb;
                }
            }


            picture.PicOrderCntVal = PicOrderCntMsb + slice_header.slice_pic_order_cnt_lsb;
            picture.picture_order_cnt_lsb = slice_header.slice_pic_order_cnt_lsb;

            if (header.nuh_temporal_id_plus1 == 1 &&
                    !header.IsSublayerNonReference &&
                    !header.IsRaslPicture &&
                    !header.IsRadlPicture)
            {
                prevPicOrderCntLsb = slice_header.slice_pic_order_cnt_lsb;
                prevPicOrderCntMsb = PicOrderCntMsb;
            }
        }


        // 8.6.1 Derivation process for quantization parameters
        internal void DerivationOfQuantizationParameters(BitStreamWithNalSupport stream, HeicPicture picture, int xCb, int yCb, int xCUBase, int yCUBase, int log2TrafoSize)
        {
            // Input to this process is a luma location ( xCb, yCb ) specifying the top-left sample of the current luma
            // coding block relative to the top-left luma sample of the current picture.
            // In this process, the variable QpY, the luma quantization parameter Qp′Y and
            // the chroma quantization parameters Qp′Cb and Qp′Cr are derived.

            var slice_header = picture.SliceHeaders[picture.SliceHeaderIndex[xCb, yCb]];

            int xQg = xCb - (xCb & ((1 << (int)picture.pps.Log2MinCuQpDeltaSize) - 1));
            int yQg = yCb - (yCb & ((1 << (int)picture.pps.Log2MinCuQpDeltaSize) - 1));

            if (xQg != stream.Context.CurrentQgX || yQg != stream.Context.CurrentQgY)
            {
                stream.Context.LastQpYPrev = stream.Context.CurrentQpY;
                stream.Context.CurrentQgX = xQg;
                stream.Context.CurrentQgY = yQg;
            }

            int qPY_PREV, qPY_PRED;

            bool firstQgInTile = false;

            int ctbLSBMask = ((1 << picture.sps.CtbLog2SizeY) - 1);
            bool firstInCTBRow = (xQg == 0 && ((yQg & ctbLSBMask) == 0));

            uint first_ctb_in_slice_RS = slice_header.SliceAddrRs;

            uint SliceStartX = (first_ctb_in_slice_RS % picture.sps.PicWidthInCtbsY) * picture.sps.CtbSizeY;
            uint SliceStartY = (first_ctb_in_slice_RS / picture.sps.PicWidthInCtbsY) * picture.sps.CtbSizeY;

            bool firstQgInSlice = ((int)SliceStartX == xQg && (int)SliceStartY == yQg);

            if (picture.pps.tiles_enabled_flag)
                throw new NotImplementedException("Tile mode is not supported");

            if (firstQgInSlice || firstQgInTile || (firstInCTBRow && picture.pps.entropy_coding_sync_enabled_flag))
            {
                qPY_PREV = slice_header.SliceQPY;
            }
            else
            {
                qPY_PREV = stream.Context.LastQpYPrev;
            }

            int qPY_A = GetNeighbouringQpY(picture, slice_header, qPY_PREV, xQg, yQg, xQg - 1, yQg);
            int qPY_B = GetNeighbouringQpY(picture, slice_header, qPY_PREV, xQg, yQg, xQg, yQg - 1);

            qPY_PRED = (qPY_A + qPY_B + 1) >> 1;

            int QpY = ((qPY_PRED + stream.Context.CuQpDeltaVal + 52 + 2 * picture.sps.QpBdOffsetY) %
                (52 + picture.sps.QpBdOffsetY)) - picture.sps.QpBdOffsetY;

            stream.Context.QpY = QpY + picture.sps.QpBdOffsetY;
            stream.Context.CurrentQpY = QpY;

            int qPiCb, qPiCr, qPCb, qPCr;
            if (picture.sps.ChromaArrayType != 0)
            {
                if (!picture.tu_residual_act_flag[xCb, yCb])
                {
                    qPiCb = MathExtra.Clip3(-picture.sps.QpBdOffsetC, 57,
                        QpY + picture.pps.pps_cb_qp_offset + slice_header.slice_cb_qp_offset + stream.Context.CuQpOffsetCb);
                    qPiCr = MathExtra.Clip3(-picture.sps.QpBdOffsetC, 57,
                        QpY + picture.pps.pps_cr_qp_offset + slice_header.slice_cr_qp_offset + stream.Context.CuQpOffsetCr);
                }
                else
                {
                    qPiCb = MathExtra.Clip3(-picture.sps.QpBdOffsetC, 57,
                        QpY + picture.pps.PpsActQpOffsetCb + slice_header.slice_act_cb_qp_offset + stream.Context.CuQpOffsetCb);
                    qPiCr = MathExtra.Clip3(-picture.sps.QpBdOffsetC, 57,
                        QpY + picture.pps.PpsActQpOffsetCr + slice_header.slice_act_cr_qp_offset + stream.Context.CuQpOffsetCr);
                }

                if (picture.sps.ChromaArrayType == 1)
                {
                    qPCb = GetChromaQpFromQPi(qPiCb);
                    qPCr = GetChromaQpFromQPi(qPiCr);
                }
                else
                {
                    qPCb = Math.Min(qPiCb, 51);
                    qPCr = Math.Min(qPiCr, 51);
                }

                stream.Context.QpCb = qPCb + picture.sps.QpBdOffsetC;
                stream.Context.QpCr = qPCr + picture.sps.QpBdOffsetC;
            }

            picture.SetQpY(xCUBase, yCUBase, log2TrafoSize < 3 ? 3 : log2TrafoSize, stream.Context.QpY);
        }

        private int GetNeighbouringQpY(HeicPicture picture, slice_segment_header slice_header, int qPY_PREV,
            int xQg, int yQg, int xQgN, int yQgN)
        {
            if (!picture.CheckZScanAvaliability(xQg, yQg, xQgN, yQgN))
            {
                return qPY_PREV;
            }
            else
            {
                int xTmp = xQgN >> picture.sps.MinTbLog2SizeY;
                int yTmp = yQgN >> picture.sps.MinTbLog2SizeY;
                int minTbAddrA = (int)picture.pps.MinTbAddrZs[xTmp, yTmp];
                int ctbAddrA = minTbAddrA >> (2 * (picture.sps.CtbLog2SizeY - picture.sps.MinTbLog2SizeY));

                if (ctbAddrA != slice_header.CtbAddrInTs)
                {
                    return qPY_PREV;
                }
                else
                {
                    return picture.QpY[xQgN, yQgN];
                }
            }
        }

        private int GetChromaQpFromQPi(int qPi)
        {
            if (qPi < 30)
                return qPi;
            else if (qPi > 43)
                return qPi - 6;
            else
                return qPiToQpCTable[qPi - 30];
        }

        private static readonly int[] qPiToQpCTable = { 29, 30, 31, 32, 33, 33, 34, 34, 35, 35, 36, 36, 37, 37 };

    }
}

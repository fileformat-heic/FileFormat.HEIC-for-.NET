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
using System.Threading;
using Openize.Heic.Decoder.IO;

namespace Openize.Heic.Decoder
{
    internal class pred_weight_table
    {
        internal uint luma_log2_weight_denom;
        internal int delta_chroma_log2_weight_denom;
        internal bool[] luma_weight_l0_flag;
        internal bool[] chroma_weight_l0_flag;
        internal int[] delta_luma_weight_l0;
        internal int[] luma_offset_l0;
        internal int[,] delta_chroma_weight_l0;
        internal int[,] delta_chroma_offset_l0;
        internal bool[] luma_weight_l1_flag;
        internal bool[] chroma_weight_l1_flag;
        internal int[] delta_luma_weight_l1;
        internal int[] luma_offset_l1;
        internal int[,] delta_chroma_weight_l1;
        internal int[,] delta_chroma_offset_l1;

        internal pred_weight_table(
            BitStreamWithNalSupport stream,
            slice_segment_header slice_header)
        {
            var pps = slice_header.pps;
            var sps = pps.sps;

            //slice_header.
            luma_log2_weight_denom = stream.ReadUev();                         // ue(v)

            if (sps.ChromaArrayType != 0)
                delta_chroma_log2_weight_denom = stream.ReadSev();             // se(v)

            int sumWeightFlags = 0;

            luma_weight_l0_flag = new bool[slice_header.num_ref_idx_l0_active_minus1 + 1];
            chroma_weight_l0_flag = new bool[slice_header.num_ref_idx_l0_active_minus1 + 1];

            for (int i = 0; i <= slice_header.num_ref_idx_l0_active_minus1; i++)
                //if ((pic_layer_id(RefPicList0[i]) != nuh_layer_id) ||
                //    (PicOrderCnt(RefPicList0[i]) != PicOrderCnt(CurrPic)))
                    luma_weight_l0_flag[i] = stream.ReadFlag();         // u(1)

            if (sps.ChromaArrayType != 0)
                for (int i = 0; i <= slice_header.num_ref_idx_l0_active_minus1; i++)
                    //if ((pic_layer_id(RefPicList0[i]) != nuh_layer_id) ||
                    //    (PicOrderCnt(RefPicList0[i]) != PicOrderCnt(CurrPic)))
                        chroma_weight_l0_flag[i] = stream.ReadFlag();   // u(1)

            delta_luma_weight_l0 = new int[slice_header.num_ref_idx_l0_active_minus1 + 1];
            luma_offset_l0 = new int[slice_header.num_ref_idx_l0_active_minus1 + 1];
            delta_chroma_weight_l0 = new int[slice_header.num_ref_idx_l0_active_minus1 + 1, 2];
            delta_chroma_offset_l0 = new int[slice_header.num_ref_idx_l0_active_minus1 + 1, 2];

            for (int i = 0; i <= slice_header.num_ref_idx_l0_active_minus1; i++)
            {
                if (luma_weight_l0_flag[i])
                {
                    delta_luma_weight_l0[i] = stream.ReadSev();             // se(v)
                    luma_offset_l0[i] = stream.ReadSev();                   // se(v)
                }

                if (chroma_weight_l0_flag[i]) {
                    for (int j = 0; j < 2; j++)
                    {
                        delta_chroma_weight_l0[i, j] = stream.ReadSev();   // se(v)
                        delta_chroma_offset_l0[i, j] = stream.ReadSev();   // se(v)
                    }
                }
            }

            if (slice_header.slice_type == SliceType.B)
            {
                luma_weight_l1_flag = new bool[slice_header.num_ref_idx_l1_active_minus1 + 1];
                chroma_weight_l1_flag = new bool[slice_header.num_ref_idx_l1_active_minus1 + 1];

                for (int i = 0; i <= slice_header.num_ref_idx_l1_active_minus1; i++)
                    //if ((pic_layer_id(RefPicList0[i]) != nuh_layer_id) ||
                    //    (PicOrderCnt(RefPicList1[i]) != PicOrderCnt(CurrPic)))
                        luma_weight_l1_flag[i] = stream.ReadFlag();        // u(1)
                
                if (sps.ChromaArrayType != 0)
                    for (int i = 0; i <= slice_header.num_ref_idx_l1_active_minus1; i++)
                        //if ((pic_layer_id(RefPicList0[i]) != sps.nuh_layer_id) ||
                        //    (PicOrderCnt(RefPicList1[i]) != PicOrderCnt(CurrPic)))
                            chroma_weight_l1_flag[i] = stream.ReadFlag();  // u(1)

                delta_luma_weight_l1 = new int[slice_header.num_ref_idx_l1_active_minus1 + 1];
                luma_offset_l1 = new int[slice_header.num_ref_idx_l1_active_minus1 + 1];
                delta_chroma_weight_l1 = new int[slice_header.num_ref_idx_l1_active_minus1 + 1, 2];
                delta_chroma_offset_l1 = new int[slice_header.num_ref_idx_l1_active_minus1 + 1, 2];

                for (int i = 0; i <= slice_header.num_ref_idx_l1_active_minus1; i++)
                {
                    if (luma_weight_l1_flag[i])
                    {
                        delta_luma_weight_l1[i] = stream.ReadSev();            // se(v)
                        luma_offset_l1[i] = stream.ReadSev();                  // se(v)
                    }
                    if (chroma_weight_l1_flag[i])
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            delta_chroma_weight_l1[i, j] = stream.ReadSev();   // se(v)
                            delta_chroma_offset_l1[i, j] = stream.ReadSev();   // se(v)
                        }
                    }
                }
            }
        }
    }

}

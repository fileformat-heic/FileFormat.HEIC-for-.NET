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
    internal class slice_segment_header
    {
        internal bool first_slice_segment_in_pic_flag;
        internal bool no_output_of_prior_pics_flag;
        internal byte slice_pic_parameter_set_id;
        internal bool dependent_slice_segment_flag;
        internal uint slice_segment_address;
        internal bool[] slice_reserved_flag;
        internal SliceType slice_type;
        internal bool pic_output_flag;
        internal byte colour_plane_id;
        internal int slice_pic_order_cnt_lsb;
        internal bool short_term_ref_pic_set_sps_flag;
        internal int short_term_ref_pic_set_idx;
        internal uint num_long_term_sps;
        internal uint num_long_term_pics;
        internal bool[] used_by_curr_pic_lt_flag;
        internal uint[] lt_idx_sps;
        internal int[] poc_lsb_lt;
        internal bool[] delta_poc_msb_present_flag;
        internal uint[] delta_poc_msb_cycle_lt;
        internal bool slice_temporal_mvp_enabled_flag;
        internal bool slice_sao_luma_flag;
        internal bool slice_sao_chroma_flag;
        internal bool num_ref_idx_active_override_flag;
        internal uint num_ref_idx_l0_active_minus1;
        internal uint num_ref_idx_l1_active_minus1;
        internal bool collocated_from_l0_flag;
        internal uint collocated_ref_idx;
        internal bool cabac_init_flag;
        internal bool mvd_l1_zero_flag;
        internal byte five_minus_max_num_merge_cand;
        internal bool use_integer_mv_flag;
        internal int slice_qp_delta;
        internal int slice_cb_qp_offset;
        internal int slice_cr_qp_offset;
        internal int slice_act_y_qp_offset;
        internal int slice_act_cb_qp_offset;
        internal int slice_act_cr_qp_offset;
        internal bool cu_chroma_qp_offset_enabled_flag;
        internal bool deblocking_filter_override_flag;
        internal bool slice_deblocking_filter_disabled_flag;
        internal int slice_beta_offset_div2;
        internal int slice_tc_offset_div2;
        internal bool slice_loop_filter_across_slices_enabled_flag;
        internal uint num_entry_point_offsets;
        internal int offset_len_minus1;
        internal int[] entry_point_offset_minus1;
        internal uint slice_segment_header_extension_length;
        internal byte[] slice_segment_header_extension_data_byte;
        internal ref_pic_lists_modification ref_pic_lists_modification;
        internal pred_weight_table pred_weight_table;

        public pic_parameter_set_rbsp pps;
        public int slice_index;
        //public int CurrRpsIdx;

        public HeicPicture parentPicture;

        public slice_segment_header(
            BitStreamWithNalSupport stream,
            slice_segment_layer_rbsp parent)
        {
            first_slice_segment_in_pic_flag = stream.ReadFlag();

            if (parent.NalHeader.IsRapPicture)
                no_output_of_prior_pics_flag = stream.ReadFlag();

            slice_pic_parameter_set_id = (byte)stream.ReadUev();

            pps = stream.Context.PPS[slice_pic_parameter_set_id];
            var sps = pps.sps;

            if (!first_slice_segment_in_pic_flag)
            {
                if (pps.dependent_slice_segments_enabled_flag)
                    dependent_slice_segment_flag = stream.ReadFlag();

                slice_segment_address = (uint)stream.Read(
                    (int)Math.Ceiling(Math.Log(sps.PicSizeInCtbsY, 2)));

                CtbAddrInRs = slice_segment_address;
                CtbAddrInTs = pps.CtbAddrRsToTs[CtbAddrInRs];
            }

            int CuQpDeltaVal = 0;
            slice_reserved_flag = new bool[pps.num_extra_slice_header_bits];
            if (!dependent_slice_segment_flag)
            {
                for (int i = 0; i < pps.num_extra_slice_header_bits; i++)
                    slice_reserved_flag[i] = stream.ReadFlag();

                slice_type = (SliceType)stream.ReadUev();

                if (slice_type != SliceType.I)
                    throw new Exception("ALARM! NOT I FRAME");

                if (pps.output_flag_present_flag)
                    pic_output_flag = stream.ReadFlag();
                //else = true? 


                if (sps.separate_colour_plane_flag)
                    colour_plane_id = (byte)stream.Read(2);

                if (parent.NalHeader.type != NalUnitType.IDR_W_RADL &&
                    parent.NalHeader.type != NalUnitType.IDR_N_LP)
                {

                    slice_pic_order_cnt_lsb
                        = stream.Read(sps.log2_max_pic_order_cnt_lsb_minus4 + 4);
                    short_term_ref_pic_set_sps_flag = stream.ReadFlag();

                    if (!short_term_ref_pic_set_sps_flag)
                    {

                        sps.st_ref_pic_sets.Add(sps.st_ref_pic_sets.Count, new st_ref_pic_set(stream, sps, sps.num_short_term_ref_pic_sets));
                    }
                    else if (sps.num_short_term_ref_pic_sets > 1)
                        short_term_ref_pic_set_idx = stream.Read((int)
                            Math.Ceiling(Math.Log(sps.num_short_term_ref_pic_sets, 2)));

                    if (sps.long_term_ref_pics_present_flag)
                    {
                        if (sps.num_long_term_ref_pics_sps > 0)
                            num_long_term_sps = stream.ReadUev();
                        num_long_term_pics = stream.ReadUev();

                        uint num_long_term_sum = num_long_term_sps + num_long_term_pics;
                        used_by_curr_pic_lt_flag = new bool[num_long_term_sum];
                        lt_idx_sps = new uint[num_long_term_sum];
                        poc_lsb_lt = new int[num_long_term_sum];
                        delta_poc_msb_present_flag = new bool[num_long_term_sum];
                        delta_poc_msb_cycle_lt = new uint[num_long_term_sum];

                        for (int i = 0; i < num_long_term_sps + num_long_term_pics; i++)
                        {
                            if (i < num_long_term_sps)
                            {
                                if (sps.num_long_term_ref_pics_sps > 1)
                                    lt_idx_sps[i] = stream.ReadUev();
                            }
                            else
                            {
                                poc_lsb_lt[i]
                                    = stream.Read(sps.log2_max_pic_order_cnt_lsb_minus4 + 4);
                                used_by_curr_pic_lt_flag[i]
                                    = stream.ReadFlag();
                            }

                            delta_poc_msb_present_flag[i]
                                = stream.ReadFlag();

                            if (delta_poc_msb_present_flag[i])
                                delta_poc_msb_cycle_lt[i] = stream.ReadUev();
                        }
                    }
                    if (sps.sps_temporal_mvp_enabled_flag)
                        slice_temporal_mvp_enabled_flag
                            = stream.ReadFlag();
                }

                if (sps.sample_adaptive_offset_enabled_flag)
                {
                    slice_sao_luma_flag = stream.ReadFlag();
                    if (sps.ChromaArrayType != 0)
                        slice_sao_chroma_flag = stream.ReadFlag();
                }

                if (slice_type == SliceType.P || slice_type == SliceType.B)
                {
                    num_ref_idx_active_override_flag = stream.ReadFlag();
                    if (num_ref_idx_active_override_flag)
                    {
                        num_ref_idx_l0_active_minus1 = stream.ReadUev();
                        if (slice_type == SliceType.B)
                            num_ref_idx_l1_active_minus1 = stream.ReadUev();
                    }

                    if (pps.lists_modification_present_flag && NumPicTotalCurr > 1)
                        ref_pic_lists_modification = new ref_pic_lists_modification(
                            stream, slice_type,
                            num_ref_idx_l0_active_minus1,
                            num_ref_idx_l1_active_minus1,
                            pps);

                    if (slice_type == SliceType.B)
                        mvd_l1_zero_flag = stream.ReadFlag();
                    if (pps.cabac_init_present_flag)
                        cabac_init_flag = stream.ReadFlag();

                    if (slice_temporal_mvp_enabled_flag)
                    {
                        if (slice_type == SliceType.B)
                            collocated_from_l0_flag = stream.ReadFlag();
                        if ((collocated_from_l0_flag && num_ref_idx_l1_active_minus1 > 0) ||
                            (!collocated_from_l0_flag && num_ref_idx_l1_active_minus1 > 0))
                            collocated_ref_idx = stream.ReadUev();
                    }

                    if ((pps.weighted_pred_flag && slice_type == SliceType.P) ||
                    (pps.weighted_bipred_flag && slice_type == SliceType.B))
                        pred_weight_table = new pred_weight_table(
                            stream, this);

                    five_minus_max_num_merge_cand = (byte)stream.ReadUev();
                    if (sps.sps_scc_ext?.motion_vector_resolution_control_idc == 2)
                        use_integer_mv_flag = stream.ReadFlag();
                }

                slice_qp_delta = stream.ReadSev();
                if (pps.pps_slice_chroma_qp_offsets_present_flag)
                {
                    slice_cb_qp_offset = stream.ReadSev();
                    slice_cr_qp_offset = stream.ReadSev();
                }

                if (pps.pps_scc_ext?.pps_slice_act_qp_offsets_present_flag ?? false)
                {
                    slice_act_y_qp_offset = stream.ReadSev();
                    slice_act_cb_qp_offset = stream.ReadSev();
                    slice_act_cr_qp_offset = stream.ReadSev();
                }

                if (pps.pps_range_ext?.chroma_qp_offset_list_enabled_flag ?? false)
                    cu_chroma_qp_offset_enabled_flag = stream.ReadFlag();

                if (pps.deblocking_filter_override_enabled_flag)
                    deblocking_filter_override_flag = stream.ReadFlag();

                if (deblocking_filter_override_flag)
                {
                    slice_deblocking_filter_disabled_flag = stream.ReadFlag();
                    if (!slice_deblocking_filter_disabled_flag)
                    {
                        slice_beta_offset_div2 = stream.ReadSev();
                        slice_tc_offset_div2 = stream.ReadSev();
                    }
                }

                if (pps.pps_loop_filter_across_slices_enabled_flag &&
                    (slice_sao_luma_flag || slice_sao_chroma_flag ||
                    !slice_deblocking_filter_disabled_flag))
                    slice_loop_filter_across_slices_enabled_flag = stream.ReadFlag();
            }

            if (pps.tiles_enabled_flag || pps.entropy_coding_sync_enabled_flag)
            {
                num_entry_point_offsets = stream.ReadUev();
                if (num_entry_point_offsets > 0)
                {
                    offset_len_minus1 = (byte)stream.ReadUev();
                    entry_point_offset_minus1 = new int[num_entry_point_offsets];
                    for (int i = 0; i < num_entry_point_offsets; i++)
                        entry_point_offset_minus1[i]
                            = stream.Read(offset_len_minus1 + 1);
                    //entry_point_offset_minus1[i+1] += entry_point_offset_minus1[i]
                }
            }
            if (pps.slice_segment_header_extension_present_flag)
            {
                slice_segment_header_extension_length = stream.ReadUev();
                slice_segment_header_extension_data_byte =
                    new byte[slice_segment_header_extension_length];
                for (int i = 0; i < slice_segment_header_extension_length; i++)
                    slice_segment_header_extension_data_byte[i]
                        = (byte)stream.Read(8);
            }

            stream.Read(1);             /* equal to 1; alignment_bit_equal_to_one */
            while (!stream.ByteAligned())
                stream.SkipBits(1);     /* equal to 0; alignment_bit_equal_to_zero */
        }

        public int MaxNumMergeCand => 5 - five_minus_max_num_merge_cand;

        public int initType
        {
            get
            {
                switch (slice_type)
                {
                    case SliceType.P:
                        return (cabac_init_flag ? 1 : 0) + 1;
                    case SliceType.B:
                        return 2 - (cabac_init_flag ? 1 : 0);
                    case SliceType.I:
                    default:
                        return 0;
                }
            }
        }

        public int SliceQPY => 26 + pps.init_qp_minus26 + slice_qp_delta;

        internal uint CtbAddrInRs { get; set; }
        internal uint CtbAddrInTs { get; set; }

        internal uint SliceAddrRs => 
            dependent_slice_segment_flag == false ? 
            slice_segment_address : // if not dependent slice
            pps.CtbAddrTsToRs[pps.CtbAddrRsToTs[slice_segment_address] - 1];// else take preceding slice segment

        internal uint CurrRpsIdx => 
            short_term_ref_pic_set_sps_flag == true ? 
            (uint)short_term_ref_pic_set_idx : 
            pps.sps.num_short_term_ref_pic_sets;
        public uint NumPicTotalCurr
        {
            get
            {
                uint NumPicTotalCurr = 0;

                for (int i = 0; i < pps.sps.NumNegativePics((int)CurrRpsIdx); i++)
                    if (pps.sps.UsedByCurrPicS0((int)CurrRpsIdx)[i])
                        NumPicTotalCurr++;

                for (int i = 0; i < pps.sps.NumPositivePics((int)CurrRpsIdx); i++)
                    if (pps.sps.UsedByCurrPicS1((int)CurrRpsIdx)[i])
                        NumPicTotalCurr++;

                for (int i = 0; i < num_long_term_sps + num_long_term_pics; i++)
                    if (UsedByCurrPicLt(i))
                        NumPicTotalCurr++;

                if (pps.pps_scc_ext.pps_curr_pic_ref_enabled_flag)
                    NumPicTotalCurr++;

                return NumPicTotalCurr;
            }
        }

        public int PocLsbLt(int i) => 
            (i < num_long_term_sps) ? 
            pps.sps.lt_ref_pic_poc_lsb_sps[lt_idx_sps[i]] : 
            poc_lsb_lt[i];


        public bool UsedByCurrPicLt(int i) => 
            (i < num_long_term_sps) ? 
            pps.sps.used_by_curr_pic_lt_sps_flag[lt_idx_sps[i]] : 
            used_by_curr_pic_lt_flag[i];
    }

}

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
using System.Collections.Generic;
using System.Diagnostics;
using FileFormat.Heic.Decoder.IO;

namespace FileFormat.Heic.Decoder
{
    internal class seq_parameter_set_rbsp : NalUnit
    {
        internal byte sps_video_parameter_set_id;
        internal byte sps_max_sub_layers_minus1;
        internal bool sps_temporal_id_nesting_flag;
        internal profile_tier_level profile_tier_level;
        internal byte sps_seq_parameter_set_id; // max - 16
        internal uint chroma_format_idc;
        internal bool separate_colour_plane_flag;
        internal uint pic_width_in_luma_samples;
        internal uint pic_height_in_luma_samples;
        internal bool conformance_window_flag;
        internal uint conf_win_left_offset;
        internal uint conf_win_right_offset;
        internal uint conf_win_top_offset;
        internal uint conf_win_bottom_offset;
        internal byte bit_depth_luma_minus8;
        internal byte bit_depth_chroma_minus8;
        internal byte log2_max_pic_order_cnt_lsb_minus4;
        internal bool sps_sub_layer_ordering_info_present_flag;
        internal uint[] sps_max_dec_pic_buffering_minus1;
        internal uint[] sps_max_num_reorder_pics;
        internal uint[] sps_max_latency_increase_plus1;
        internal byte log2_min_luma_coding_block_size_minus3;
        internal byte log2_diff_max_min_luma_coding_block_size;
        internal byte log2_min_luma_transform_block_size_minus2;
        internal byte log2_diff_max_min_luma_transform_block_size;
        internal uint max_transform_hierarchy_depth_inter;
        internal uint max_transform_hierarchy_depth_intra;
        internal bool scaling_list_enabled_flag;
        internal bool sps_scaling_list_data_present_flag;
        internal bool amp_enabled_flag;
        internal bool sample_adaptive_offset_enabled_flag;
        internal bool pcm_enabled_flag;
        internal scaling_list_data scaling_list_data;
        internal byte pcm_sample_bit_depth_luma;
        internal byte pcm_sample_bit_depth_chroma;
        internal byte log2_min_pcm_luma_coding_block_size_minus3;
        internal byte log2_diff_max_min_pcm_luma_coding_block_size;
        internal bool pcm_loop_filter_disabled_flag;
        internal uint num_short_term_ref_pic_sets;

        internal Dictionary<int, st_ref_pic_set> st_ref_pic_sets;

        internal bool long_term_ref_pics_present_flag;
        internal uint num_long_term_ref_pics_sps;
        internal int[] lt_ref_pic_poc_lsb_sps;
        internal bool[] used_by_curr_pic_lt_sps_flag;
        internal bool sps_temporal_mvp_enabled_flag;
        internal bool strong_intra_smoothing_enabled_flag;
        internal bool vui_parameters_present_flag;
        internal bool sps_extension_present_flag;
        internal bool sps_range_extension_flag;
        internal bool sps_multilayer_extension_flag;
        internal bool sps_3d_extension_flag;
        internal bool sps_scc_extension_flag;
        internal byte sps_extension_4bits;
        internal sps_range_extension sps_range_ext;
        internal sps_multilayer_extension sps_multilayer_ext;
        internal sps_3d_extension sps_3d_ext;
        internal sps_scc_extension sps_scc_ext;
        internal bool sps_extension_data_flag;
        internal vui_parameters vui_parameters;

        public video_parameter_set_rbsp vps;

        public new string ToString() => $"NAL Unit SPS " +
            $"\nSize: {pic_width_in_luma_samples}x{pic_height_in_luma_samples} " +
            $"\nBit depth: {bit_depth_luma_minus8 + 8} {bit_depth_chroma_minus8 + 8}";

        public seq_parameter_set_rbsp(BitStreamWithNalSupport stream, ulong startPosition, int size) : base(stream, startPosition, size)
        {
            sps_video_parameter_set_id = (byte)stream.Read(4);                  // u(4)
            
            vps = stream.Context.VPS[sps_video_parameter_set_id];

            sps_max_sub_layers_minus1 = (byte)stream.Read(3);                   // u(3)
            sps_temporal_id_nesting_flag = stream.ReadFlag();                   // u(1)

            profile_tier_level = new profile_tier_level(stream, true, sps_max_sub_layers_minus1);

            sps_seq_parameter_set_id = (byte)stream.ReadUev();                       // ue(v)
            chroma_format_idc = stream.ReadUev();                              // ue(v)
            if (chroma_format_idc == 3)
                separate_colour_plane_flag = stream.ReadFlag();                // u(1)

            pic_width_in_luma_samples = stream.ReadUev();                      // ue(v)
            pic_height_in_luma_samples = stream.ReadUev();                     // ue(v)
            conformance_window_flag = stream.ReadFlag();                       // u(1)
            if (conformance_window_flag)
            {
                conf_win_left_offset = stream.ReadUev();                       // ue(v)
                conf_win_right_offset = stream.ReadUev();                      // ue(v)
                conf_win_top_offset = stream.ReadUev();                        // ue(v)
                conf_win_bottom_offset = stream.ReadUev();                     // ue(v)
            }

            bit_depth_luma_minus8 = (byte)stream.ReadUev();                    // ue(v)
            bit_depth_chroma_minus8 = (byte)stream.ReadUev();                  // ue(v)
            log2_max_pic_order_cnt_lsb_minus4 = (byte)stream.ReadUev();        // ue(v)
            sps_sub_layer_ordering_info_present_flag = stream.ReadFlag();      // u(1)

            int layers_count = sps_sub_layer_ordering_info_present_flag ? 0 : sps_max_sub_layers_minus1;
            sps_max_dec_pic_buffering_minus1 = new uint[sps_max_sub_layers_minus1 + 1];
            sps_max_num_reorder_pics = new uint[sps_max_sub_layers_minus1 + 1];
            sps_max_latency_increase_plus1 = new uint[sps_max_sub_layers_minus1 + 1];
            for (int i = layers_count; i <= sps_max_sub_layers_minus1; i++)
            {
                sps_max_dec_pic_buffering_minus1[i] = stream.ReadUev();         // ue(v)
                sps_max_num_reorder_pics[i] = stream.ReadUev();                // ue(v)
                sps_max_latency_increase_plus1[i] = stream.ReadUev();          // ue(v)
            }

            log2_min_luma_coding_block_size_minus3 = (byte)stream.ReadUev();          // ue(v)
            log2_diff_max_min_luma_coding_block_size = (byte)stream.ReadUev();       // ue(v)
            log2_min_luma_transform_block_size_minus2 = (byte)stream.ReadUev();      // ue(v)
            log2_diff_max_min_luma_transform_block_size = (byte)stream.ReadUev();// ue(v)
            max_transform_hierarchy_depth_inter = stream.ReadUev();            // ue(v)
            max_transform_hierarchy_depth_intra = stream.ReadUev();            // ue(v)

            scaling_list_enabled_flag = stream.ReadFlag();               // u(1)
            if (scaling_list_enabled_flag)
            {
                sps_scaling_list_data_present_flag = stream.ReadFlag();  // u(1)
                if (sps_scaling_list_data_present_flag)
                    scaling_list_data = new scaling_list_data(stream);
            }

            amp_enabled_flag = stream.ReadFlag();                        // u(1)
            sample_adaptive_offset_enabled_flag = stream.ReadFlag();     // u(1)

            pcm_enabled_flag = stream.ReadFlag();                        // u(1)
            if (pcm_enabled_flag)
            {
                pcm_sample_bit_depth_luma = (byte)(stream.Read(4) + 1);    // u(4)
                pcm_sample_bit_depth_chroma = (byte)(stream.Read(4) + 1);  // u(4)
                log2_min_pcm_luma_coding_block_size_minus3 = (byte)stream.ReadUev(); // ue(v)
                log2_diff_max_min_pcm_luma_coding_block_size = (byte)stream.ReadUev();//ue(v)
                pcm_loop_filter_disabled_flag = stream.ReadFlag();       // u(1)
            }

            num_short_term_ref_pic_sets = stream.ReadUev();                    // ue(v)

            st_ref_pic_sets = new Dictionary<int, st_ref_pic_set>();
            for (uint i = 0; i < num_short_term_ref_pic_sets; i++)
                st_ref_pic_sets.Add((int)i, new st_ref_pic_set(stream, this, i));

            long_term_ref_pics_present_flag = stream.ReadFlag();         // u(1)
            if (long_term_ref_pics_present_flag)
            {
                num_long_term_ref_pics_sps = stream.ReadUev();                 // ue(v)
                lt_ref_pic_poc_lsb_sps = new int[num_long_term_ref_pics_sps];
                used_by_curr_pic_lt_sps_flag = new bool[num_long_term_ref_pics_sps];
                for (int i = 0; i < num_long_term_ref_pics_sps; i++)
                {
                    lt_ref_pic_poc_lsb_sps[i] =
                        stream.Read((int)log2_max_pic_order_cnt_lsb_minus4 + 1); // u(v)
                    used_by_curr_pic_lt_sps_flag[i] = stream.ReadFlag(); // u(1)
                }
            }

            sps_temporal_mvp_enabled_flag = stream.ReadFlag();           // u(1)
            strong_intra_smoothing_enabled_flag = stream.ReadFlag();     // u(1)
            vui_parameters_present_flag = stream.ReadFlag();             // u(1)
            if (vui_parameters_present_flag)
            {
                vui_parameters = new vui_parameters(stream, this);
            }
            else
            {
                vui_parameters = new vui_parameters();
            }

            sps_extension_present_flag = stream.ReadFlag();              // u(1)
            if (sps_extension_present_flag)
            {
                sps_range_extension_flag = stream.ReadFlag();            // u(1)
                sps_multilayer_extension_flag = stream.ReadFlag();       // u(1)
                sps_3d_extension_flag = stream.ReadFlag();               // u(1)
                sps_scc_extension_flag = stream.ReadFlag();              // u(1)
                sps_extension_4bits = (byte)stream.Read(4);                // u(4)
            }

            sps_range_ext = sps_range_extension_flag ?
                new sps_range_extension(stream) :
                new sps_range_extension();

            
            if (sps_multilayer_extension_flag)
                sps_multilayer_ext = new sps_multilayer_extension(stream); /* specified in Annex F */
            
            if (sps_3d_extension_flag)
                sps_3d_ext = new sps_3d_extension(stream); /* specified in Annex I */

            sps_scc_ext = sps_range_extension_flag ?
                new sps_scc_extension(stream, chroma_format_idc, bit_depth_luma_minus8, bit_depth_chroma_minus8) :
                new sps_scc_extension(chroma_format_idc);
            
            if (sps_extension_4bits > 0)
                while (stream.HasMoreRbspData(EndPosition))
                    sps_extension_data_flag = stream.ReadFlag();         // u(1)

            new rbsp_trailing_bits(stream);
        }

        /// <summary>
        /// 0 - mono, 1 - 420, 2 - 422, 3 - 444
        /// </summary>
        public uint ChromaArrayType
        {
            get
            {
                if (!separate_colour_plane_flag)
                    return chroma_format_idc;
                return 0;
            }
        }

        public uint NumNegativePics(int stRpsIdx) => st_ref_pic_sets[stRpsIdx].num_negative_pics;
        public uint NumPositivePics(int stRpsIdx) => st_ref_pic_sets[stRpsIdx].num_positive_pics;
        public uint NumDeltaPocs(int stRpsIdx) => NumNegativePics(stRpsIdx) + NumPositivePics(stRpsIdx);
        public bool[] UsedByCurrPicS0(int stRpsIdx) => st_ref_pic_sets[stRpsIdx].used_by_curr_pic_s0_flag;
        public bool[] UsedByCurrPicS1(int stRpsIdx) => st_ref_pic_sets[stRpsIdx].used_by_curr_pic_s1_flag;
        public long DeltaPocS0(int stRpsIdx, int i) => i == 0 ?
            -(st_ref_pic_sets[stRpsIdx].delta_poc_s0_minus1[i] + 1) :
             DeltaPocS0(stRpsIdx, i - 1) - (st_ref_pic_sets[stRpsIdx].delta_poc_s0_minus1[i] + 1);
        public long DeltaPocS1(int stRpsIdx, int i) => i == 0 ?
            -(st_ref_pic_sets[stRpsIdx].delta_poc_s1_minus1[i] + 1) :
             DeltaPocS1(stRpsIdx, i - 1) - (st_ref_pic_sets[stRpsIdx].delta_poc_s1_minus1[i] + 1);

        /// <summary>
        /// 2 if 420 or 422, otherwise 1
        /// </summary>
        public byte SubWidthC => (byte)((chroma_format_idc == 1 || chroma_format_idc == 2) ? 2 : 1);

        /// <summary>
        /// 2 if 420, otherwise 1
        /// </summary>
        public byte SubHeightC => (byte)((chroma_format_idc == 1) ? 2 : 1);

        public byte MinCbLog2SizeY => (byte)(log2_min_luma_coding_block_size_minus3 + 3);
        public byte CtbLog2SizeY => (byte)(MinCbLog2SizeY + log2_diff_max_min_luma_coding_block_size);
        public uint MinCbSizeY => (uint)(1 << (ushort)MinCbLog2SizeY);
        public uint CtbSizeY => (uint)(1 << (ushort)CtbLog2SizeY);
        public uint CtbWidthC => (chroma_format_idc == 0 || separate_colour_plane_flag) ? 0 : CtbSizeY / SubWidthC;
        public uint CtbHeightC => (chroma_format_idc == 0 || separate_colour_plane_flag) ? 0 : CtbSizeY / SubHeightC;
        public uint PicWidthInMinCbsY => pic_width_in_luma_samples / MinCbSizeY;
        public uint PicWidthInCtbsY => MathExtra.CeilDiv(pic_width_in_luma_samples, CtbSizeY);
        public uint PicHeightInMinCbsY => pic_height_in_luma_samples / MinCbSizeY;
        public uint PicHeightInCtbsY => MathExtra.CeilDiv(pic_height_in_luma_samples, CtbSizeY);
        public uint PicSizeInMinCbsY => PicWidthInMinCbsY * PicHeightInMinCbsY;
        public uint PicSizeInCtbsY => PicWidthInCtbsY * PicHeightInCtbsY;
        public uint PicSizeInSamplesY => pic_width_in_luma_samples * pic_height_in_luma_samples;
        public uint PicWidthInSamplesC => pic_width_in_luma_samples / SubWidthC;
        public uint PicHeightInSamplesC => pic_height_in_luma_samples / SubHeightC;

        public byte BitDepthY => (byte)(8 + bit_depth_luma_minus8);
        public byte QpBdOffsetY => (byte)(6 * bit_depth_luma_minus8);
        public byte BitDepthC => (byte)(8 + bit_depth_chroma_minus8);
        public byte QpBdOffsetC => (byte)(6 * bit_depth_chroma_minus8);
        public byte MinTbLog2SizeY => (byte)(log2_min_luma_transform_block_size_minus2 + 2);
        public byte MaxTbLog2SizeY => (byte)(MinTbLog2SizeY + log2_diff_max_min_luma_transform_block_size);
        public uint MinTbSizeY => (uint)(1 << (ushort)MinTbLog2SizeY);
        public uint MaxPicOrderCntLsb => (uint)(1 << (log2_max_pic_order_cnt_lsb_minus4 + 4));
        public int Log2MinIpcmCbSizeY => log2_min_pcm_luma_coding_block_size_minus3 + 3;
        public int Log2MaxIpcmCbSizeY => log2_diff_max_min_pcm_luma_coding_block_size + Log2MinIpcmCbSizeY;

        public int CoeffMinY => -(1 << (sps_range_ext.extended_precision_processing_flag ? Math.Max(15, BitDepthY + 6) : 15));
        public int CoeffMinC => -(1 << (sps_range_ext.extended_precision_processing_flag ? Math.Max(15, BitDepthC + 6) : 15));
        public int CoeffMaxY => (1 << (sps_range_ext.extended_precision_processing_flag ? Math.Max(15, BitDepthY + 6) : 15)) - 1;
        public int CoeffMaxC => (1 << (sps_range_ext.extended_precision_processing_flag ? Math.Max(15, BitDepthC + 6) : 15)) - 1;

    }

    internal class vui_parameters
    {
        internal bool aspect_ratio_info_present_flag;
        internal byte aspect_ratio_idc; // 17 to 254 == 0
        internal ushort sar_width;
        internal ushort sar_height;
        internal bool overscan_info_present_flag;
        internal bool overscan_appropriate_flag;

        internal bool video_signal_type_present_flag = false;
        internal byte video_format = 5;
        internal bool video_full_range_flag = false;

        internal bool colour_description_present_flag = false;
        internal byte colour_primaries = 2;
        internal byte transfer_characteristics = 2;
        internal byte matrix_coeffs = 2;

        internal bool chroma_loc_info_present_flag;
        internal uint chroma_sample_loc_type_top_field;
        internal uint chroma_sample_loc_type_bottom_field;
        internal bool neutral_chroma_indication_flag;
        internal bool field_seq_flag;
        internal bool frame_field_info_present_flag;
        internal bool default_display_window_flag;
        internal uint def_disp_win_left_offset;
        internal uint def_disp_win_right_offset;
        internal uint def_disp_win_top_offset;
        internal uint def_disp_win_bottom_offset;
        internal bool vui_timing_info_present_flag;
        internal uint vui_num_units_in_tick;
        internal uint vui_time_scale;
        internal bool vui_poc_proportional_to_timing_flag;
        internal uint vui_num_ticks_poc_diff_one_minus1;
        internal bool vui_hrd_parameters_present_flag;
        internal hrd_parameters hrd_parameters;
        internal bool bitstream_restriction_flag;
        internal bool tiles_fixed_structure_flag;
        internal bool motion_vectors_over_pic_boundaries_flag;
        internal bool restricted_ref_pic_lists_flag;
        internal uint min_spatial_segmentation_idc;
        internal uint max_bytes_per_pic_denom;
        internal uint max_bits_per_min_cu_denom;
        internal uint log2_max_mv_length_horizontal;
        internal uint log2_max_mv_length_vertical;

        public vui_parameters(BitStreamWithNalSupport stream, seq_parameter_set_rbsp sps)
        {
            aspect_ratio_info_present_flag = stream.ReadFlag();                // u(1)
            if (aspect_ratio_info_present_flag)
            {
                aspect_ratio_idc = (byte)stream.Read(8);                   // u(8)
                if (aspect_ratio_idc == 255)
                {
                    sar_width = (ushort)stream.Read(8);                    // u(16)
                    sar_height = (ushort)stream.Read(8);                   // u(16)
                }
            }

            overscan_info_present_flag = stream.ReadFlag();                // u(1)
            if (overscan_info_present_flag)
                overscan_appropriate_flag = stream.ReadFlag();             // u(1)

            video_signal_type_present_flag = stream.ReadFlag();            // u(1)
            if (video_signal_type_present_flag)
            {
                video_format = (byte)stream.Read(3);                       // u(3)
                video_full_range_flag = stream.ReadFlag();                 // u(1)
                colour_description_present_flag = stream.ReadFlag();       // u(1)

                if (colour_description_present_flag)
                {
                    colour_primaries = (byte)stream.Read(8);               // u(8)
                    transfer_characteristics = (byte)stream.Read(8);       // u(8)
                    matrix_coeffs = (byte)stream.Read(8);                  // u(8)
                }
            }

            chroma_loc_info_present_flag = stream.ReadFlag();                  // u(1)
            if (chroma_loc_info_present_flag)
            {
                chroma_sample_loc_type_top_field = stream.ReadUev();           // ue(v)
                chroma_sample_loc_type_bottom_field = stream.ReadUev();        // ue(v)
            }

            neutral_chroma_indication_flag = stream.ReadFlag();                // u(1)
            field_seq_flag = stream.ReadFlag();                                // u(1)
            frame_field_info_present_flag = stream.ReadFlag();                 // u(1)
            default_display_window_flag = stream.ReadFlag();                   // u(1)
            if (default_display_window_flag)
            {
                def_disp_win_left_offset = stream.ReadUev();                   // ue(v)
                def_disp_win_right_offset = stream.ReadUev();                  // ue(v)
                def_disp_win_top_offset = stream.ReadUev();                    // ue(v)
                def_disp_win_bottom_offset = stream.ReadUev();                 // ue(v)
            }

            vui_timing_info_present_flag = stream.ReadFlag();                  // u(1)
            if (vui_timing_info_present_flag)
            {
                vui_num_units_in_tick = (uint)stream.Read(32);             // u(32)
                vui_time_scale = (uint)stream.Read(32);                    // u(32)
                vui_poc_proportional_to_timing_flag = stream.ReadFlag();       // u(1)
                if (vui_poc_proportional_to_timing_flag)
                    vui_num_ticks_poc_diff_one_minus1 = stream.ReadUev();      // ue(v)

                vui_hrd_parameters_present_flag = stream.ReadFlag();           // u(1)
                if (vui_hrd_parameters_present_flag)
                    hrd_parameters = new hrd_parameters(stream, true, sps.sps_max_sub_layers_minus1);
            }

            bitstream_restriction_flag = stream.ReadFlag();                    // u(1)
            if (bitstream_restriction_flag)
            {
                tiles_fixed_structure_flag = stream.ReadFlag();                // u(1)
                motion_vectors_over_pic_boundaries_flag = stream.ReadFlag();   // u(1)
                restricted_ref_pic_lists_flag = stream.ReadFlag();             // u(1)
                min_spatial_segmentation_idc = stream.ReadUev();               // ue(v)
                max_bytes_per_pic_denom = stream.ReadUev();                    // ue(v)
                max_bits_per_min_cu_denom = stream.ReadUev();                  // ue(v)
                log2_max_mv_length_horizontal = stream.ReadUev();              // ue(v)
                log2_max_mv_length_vertical = stream.ReadUev();                // ue(v)
            }
        }


        public vui_parameters()
        {
        }
    }

    internal class sps_scc_extension
    {
        internal bool sps_curr_pic_ref_enabled_flag;
        internal bool palette_mode_enabled_flag;
        internal uint palette_max_size;
        internal uint delta_palette_max_predictor_size;
        internal bool sps_palette_predictor_initializers_present_flag;
        internal uint sps_num_palette_predictor_initializers_minus1;
        internal ushort[,] sps_palette_predictor_initializer;
        internal byte motion_vector_resolution_control_idc;
        internal bool intra_boundary_filtering_disabled_flag;

        public sps_scc_extension(BitStreamWithNalSupport stream, uint chroma_format_idc, byte bit_depth_luma_minus8, byte bit_depth_chroma_minus8)
        {
            sps_curr_pic_ref_enabled_flag = stream.ReadFlag();           // u(1)
            palette_mode_enabled_flag = stream.ReadFlag();               // u(1)
            if (palette_mode_enabled_flag)
            {
                palette_max_size = stream.ReadUev();                           // ue(v)
                delta_palette_max_predictor_size = stream.ReadUev();           // ue(v)
                sps_palette_predictor_initializers_present_flag
                                                    = stream.ReadFlag(); // u(1)
                if (sps_palette_predictor_initializers_present_flag)
                {
                    sps_num_palette_predictor_initializers_minus1 = stream.ReadUev(); // ue(v)
                    Debug.Assert(sps_num_palette_predictor_initializers_minus1 + 1 <= palette_max_size + delta_palette_max_predictor_size);

                    int numComps = (chroma_format_idc == 0) ? 1 : 3;

                    sps_palette_predictor_initializer = new ushort[numComps, sps_num_palette_predictor_initializers_minus1 + 1];

                    for (int comp = 0; comp < numComps; comp++)
                        for (int i = 0; i <= sps_num_palette_predictor_initializers_minus1; i++)
                            sps_palette_predictor_initializer[comp, i] =        // u(v)
                                (ushort)stream.Read((comp == 0 ?
                                                       bit_depth_luma_minus8 :
                                                       bit_depth_chroma_minus8) + 8);
                }
            }
            motion_vector_resolution_control_idc = (byte)stream.Read(2);   // u(2)
            intra_boundary_filtering_disabled_flag = stream.ReadFlag();  // u(1)
        }

        public sps_scc_extension(uint chroma_format_idc)
        {
            sps_curr_pic_ref_enabled_flag = false;
            palette_mode_enabled_flag = false;
            palette_max_size = 0;
            delta_palette_max_predictor_size = 0;
            sps_palette_predictor_initializers_present_flag = false;
            sps_num_palette_predictor_initializers_minus1 = 0;

            int numComps = (chroma_format_idc == 0) ? 1 : 3; 
            sps_palette_predictor_initializer = new ushort[numComps, sps_num_palette_predictor_initializers_minus1 + 1];
            
            motion_vector_resolution_control_idc = 0;
            intra_boundary_filtering_disabled_flag = false;
        }
    }
    internal class sps_range_extension
    {
        internal bool transform_skip_rotation_enabled_flag;
        internal bool transform_skip_context_enabled_flag;
        internal bool implicit_rdpcm_enabled_flag;
        internal bool explicit_rdpcm_enabled_flag;
        internal bool extended_precision_processing_flag;
        internal bool intra_smoothing_disabled_flag;
        internal bool high_precision_offsets_enabled_flag;
        internal bool persistent_rice_adaptation_enabled_flag;
        internal bool cabac_bypass_alignment_enabled_flag;

        public sps_range_extension(BitStreamWithNalSupport stream)
        {
            transform_skip_rotation_enabled_flag = stream.ReadFlag();    // u(1)
            transform_skip_context_enabled_flag = stream.ReadFlag();     // u(1)
            implicit_rdpcm_enabled_flag = stream.ReadFlag();             // u(1)
            explicit_rdpcm_enabled_flag = stream.ReadFlag();             // u(1)
            extended_precision_processing_flag = stream.ReadFlag();      // u(1)
            intra_smoothing_disabled_flag = stream.ReadFlag();           // u(1)
            high_precision_offsets_enabled_flag = stream.ReadFlag();     // u(1)
            persistent_rice_adaptation_enabled_flag = stream.ReadFlag(); // u(1)
            cabac_bypass_alignment_enabled_flag = stream.ReadFlag();     // u(1)
        }
        public sps_range_extension()
        {
            transform_skip_rotation_enabled_flag = false;
            transform_skip_context_enabled_flag = false;
            implicit_rdpcm_enabled_flag = false;
            explicit_rdpcm_enabled_flag = false;
            extended_precision_processing_flag = false;
            intra_smoothing_disabled_flag = false;
            high_precision_offsets_enabled_flag = false;
            persistent_rice_adaptation_enabled_flag = false;
            cabac_bypass_alignment_enabled_flag = false;
        }
    }

    internal class sps_3d_extension
    {
        public sps_3d_extension(BitStreamWithNalSupport stream)
        {
            throw new NotImplementedException();
        }
    }

    internal class sps_multilayer_extension
    {
        public sps_multilayer_extension(BitStreamWithNalSupport stream)
        {
            throw new NotImplementedException();
        }
    }
}

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
    internal class video_parameter_set_rbsp : NalUnit
    {
        internal byte vps_video_parameter_set_id; // max - 16
        internal bool vps_base_layer_internal_flag;
        internal bool vps_base_layer_available_flag;
        internal byte vps_max_layers_minus1;
        internal byte vps_max_sub_layers_minus1;
        internal bool vps_temporal_id_nesting_flag;
        internal profile_tier_level profile_tier_level;

        internal bool vps_sub_layer_ordering_info_present_flag;

        internal uint[] vps_max_dec_pic_buffering_minus1;
        internal uint[] vps_max_num_reorder_pics;
        internal uint[] vps_max_latency_increase_plus1;

        internal byte vps_max_layer_id;
        internal uint vps_num_layer_sets_minus1;

        internal bool[,] layer_id_included_flag;
        internal bool vps_timing_info_present_flag;
        internal uint vps_num_units_in_tick;
        internal uint vps_time_scale;
        internal bool vps_poc_proportional_to_timing_flag;
        internal uint vps_num_ticks_poc_diff_one;
        internal uint vps_num_hrd_parameters;

        internal uint[] hrd_layer_set_idx;
        internal bool[] cprms_present_flag;
        internal bool vps_extension_flag;
        internal bool vps_extension_data_flag;
        internal hrd_parameters hrd_parameters;

        public string ToString() => $"NAL Unit VPS " +
            $"\nProfile: {profile_tier_level.general_profile_idc} " +
            $"\nLevel: {profile_tier_level.general_level_idc}";
#if DEBUG
        public string AsString => ToString();
#endif

        internal video_parameter_set_rbsp(BitStreamWithNalSupport stream, ulong startPosition, int size) : base (stream, startPosition, size)
        {
            vps_video_parameter_set_id = (byte)stream.Read(4);             // u(4)
            vps_base_layer_internal_flag = stream.ReadFlag();            // u(1)
            vps_base_layer_available_flag = stream.ReadFlag();           // u(1)
            vps_max_layers_minus1 = (byte)stream.Read(6);                  // u(6)
            vps_max_sub_layers_minus1 = (byte)stream.Read(3);              // u(3)
            vps_temporal_id_nesting_flag = stream.ReadFlag();            // u(1)
            stream.SkipBits(16); // vps_reserved_0xffff_16bits                  // u(16)

            profile_tier_level =  new profile_tier_level(stream, true, vps_max_sub_layers_minus1);

            vps_sub_layer_ordering_info_present_flag = stream.ReadFlag();// u(1)

            byte layers_count = (byte)(vps_sub_layer_ordering_info_present_flag ? 0 : vps_max_layers_minus1);
            vps_max_dec_pic_buffering_minus1 = new uint[vps_max_layers_minus1 + 1];
            vps_max_num_reorder_pics = new uint[vps_max_layers_minus1 + 1];
            vps_max_latency_increase_plus1 = new uint[vps_max_layers_minus1 + 1];

            for (int i = layers_count; i <= vps_max_layers_minus1; i++)
            {
                vps_max_dec_pic_buffering_minus1[i] = stream.ReadUev();        // ue(v)
                vps_max_num_reorder_pics[i] = stream.ReadUev();                // ue(v)
                vps_max_latency_increase_plus1[i] = stream.ReadUev();          // ue(v)
            }

            vps_max_layer_id = (byte)stream.Read(6);                       // u(6)
            vps_num_layer_sets_minus1 = stream.ReadUev();                // ue(v)

            layer_id_included_flag = new bool[vps_num_layer_sets_minus1 + 2, vps_max_layer_id + 1];

            for (int i = 1; i <= vps_num_layer_sets_minus1; i++)
                for (int j = 0; j <= vps_max_layer_id; j++)
                    layer_id_included_flag[i, j] = stream.ReadFlag();    // u(1)

            vps_timing_info_present_flag = stream.ReadFlag();            // u(1)

            if (vps_timing_info_present_flag)
            {
                vps_num_units_in_tick = (uint)stream.Read(32);             // u(32)
                vps_time_scale = (uint)stream.Read(32);                    // u(32)
                vps_poc_proportional_to_timing_flag = stream.ReadFlag(); // u(1)
                if (vps_poc_proportional_to_timing_flag)
                    vps_num_ticks_poc_diff_one = (stream.ReadUev() + 1);       // ue(v)

                vps_num_hrd_parameters = (stream.ReadUev());                   // ue(v)
                hrd_layer_set_idx = new uint[vps_num_hrd_parameters];
                cprms_present_flag = new bool[vps_num_hrd_parameters];
                for (int i = 0; i < vps_num_hrd_parameters; i++)
                {
                    hrd_layer_set_idx[i] = stream.ReadUev();                   // ue(v)
                    if (i > 0)
                        cprms_present_flag[i] = stream.ReadFlag();       // u(1)

                    hrd_parameters = new hrd_parameters(stream, cprms_present_flag[i], vps_max_sub_layers_minus1);
                }
            }

            vps_extension_flag = stream.ReadFlag();                      // u(1)
            if (vps_extension_flag)
            {
                while (stream.HasMoreRbspData(EndPosition))
                    vps_extension_data_flag = stream.ReadFlag();         // u(1)
            }
            new rbsp_trailing_bits(stream);
        }
    }
    
}

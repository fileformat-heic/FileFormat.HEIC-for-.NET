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
    internal class pps_range_extension
    {
        internal uint log2_max_transform_skip_block_size_minus2;
        internal bool cross_component_prediction_enabled_flag;
        internal bool chroma_qp_offset_list_enabled_flag;
        internal uint diff_cu_chroma_qp_offset_depth;
        internal int chroma_qp_offset_list_len_minus1;
        internal int[] cb_qp_offset_list;
        internal int[] cr_qp_offset_list;
        internal uint log2_sao_offset_scale_luma;
        internal uint log2_sao_offset_scale_chroma;

        public pps_range_extension(BitStreamWithNalSupport stream, bool transform_skip_enabled_flag)
        {
            if (transform_skip_enabled_flag)
                log2_max_transform_skip_block_size_minus2 = stream.ReadUev();     // ue(v)
            cross_component_prediction_enabled_flag = stream.ReadFlag(); // u(1)
            chroma_qp_offset_list_enabled_flag = stream.ReadFlag();      // u(1)
            if (chroma_qp_offset_list_enabled_flag)
            {
                diff_cu_chroma_qp_offset_depth = stream.ReadUev();             // ue(v)
                chroma_qp_offset_list_len_minus1 = (int)stream.ReadUev();              // ue(v)
                cb_qp_offset_list = new int[chroma_qp_offset_list_len_minus1 + 1];
                cr_qp_offset_list = new int[chroma_qp_offset_list_len_minus1 + 1];
                for (int i = 0; i <= chroma_qp_offset_list_len_minus1; i++)
                {
                    cb_qp_offset_list[i] = stream.ReadSev();                   // se(v)
                    cr_qp_offset_list[i] = stream.ReadSev();                   // se(v)
                }
            }
            log2_sao_offset_scale_luma = stream.ReadUev();                     // ue(v)
            log2_sao_offset_scale_chroma = stream.ReadUev();                   // ue(v)
        }
        public pps_range_extension()
        {
            log2_max_transform_skip_block_size_minus2 = 0;
            cross_component_prediction_enabled_flag = false;
            chroma_qp_offset_list_enabled_flag = false;
            diff_cu_chroma_qp_offset_depth = 0;
            chroma_qp_offset_list_len_minus1 = -1;
            log2_sao_offset_scale_luma = 0;
            log2_sao_offset_scale_chroma = 0;
        }
    }
}

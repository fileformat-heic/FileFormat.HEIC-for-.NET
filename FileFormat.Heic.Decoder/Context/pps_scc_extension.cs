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
    internal class pps_scc_extension
    {
        internal bool pps_curr_pic_ref_enabled_flag;
        internal bool residual_adaptive_colour_transform_enabled_flag;
        internal bool pps_slice_act_qp_offsets_present_flag;
        internal int pps_act_y_qp_offset_plus5;
        internal int pps_act_cb_qp_offset_plus5;
        internal int pps_act_cr_qp_offset_plus3;
        internal bool pps_palette_predictor_initializers_present_flag;
        internal uint pps_num_palette_predictor_initializers;
        internal bool monochrome_palette_flag;
        internal byte luma_bit_depth_entry_minus8;
        internal byte chroma_bit_depth_entry_minus8;
        internal ushort[,] pps_palette_predictor_initializer;

        public pps_scc_extension(BitStreamWithNalSupport stream)
        {
            pps_curr_pic_ref_enabled_flag = stream.ReadFlag();           // u(1)
            residual_adaptive_colour_transform_enabled_flag = stream.ReadFlag();// u(1)
            if (residual_adaptive_colour_transform_enabled_flag)
            {
                pps_slice_act_qp_offsets_present_flag =
                    stream.ReadFlag();                                   // u(1)
                pps_act_y_qp_offset_plus5 = stream.ReadSev();                    // se(v)
                pps_act_cb_qp_offset_plus5 = stream.ReadSev();                   // se(v)
                pps_act_cr_qp_offset_plus3 = stream.ReadSev();                   // se(v)
            }
            pps_palette_predictor_initializers_present_flag = stream.ReadFlag();// u(1)
            if (pps_palette_predictor_initializers_present_flag)
            {
                pps_num_palette_predictor_initializers = stream.ReadUev();     // ue(v)
                if (pps_num_palette_predictor_initializers > 0)
                {
                    monochrome_palette_flag = stream.ReadFlag();         // u(1)
                    luma_bit_depth_entry_minus8 = (byte)stream.ReadUev();       // ue(v)
                    if (!monochrome_palette_flag)
                        chroma_bit_depth_entry_minus8 = (byte)stream.ReadUev();

                    int numComps = monochrome_palette_flag ? 1 : 3;
                    pps_palette_predictor_initializer = new ushort[numComps, pps_num_palette_predictor_initializers];

                    for (int comp = 0; comp < numComps; comp++)
                        for (int i = 0; i < pps_num_palette_predictor_initializers; i++)
                            pps_palette_predictor_initializer[comp, i] =
                                (ushort)stream.Read(8 + (comp == 0 ?        // u(v)
                                                       luma_bit_depth_entry_minus8 :
                                                       chroma_bit_depth_entry_minus8));
                }
            }
        }
        public pps_scc_extension()
        {
            pps_curr_pic_ref_enabled_flag = false;
            residual_adaptive_colour_transform_enabled_flag = false;
            pps_slice_act_qp_offsets_present_flag = false;
            pps_act_y_qp_offset_plus5 = 0;
            pps_act_cb_qp_offset_plus5 = 0;
            pps_act_cr_qp_offset_plus3 = 0;
            pps_palette_predictor_initializers_present_flag = false;
            pps_num_palette_predictor_initializers = 0;
            monochrome_palette_flag = false;
            luma_bit_depth_entry_minus8 = 0;
            chroma_bit_depth_entry_minus8 = 0;
            pps_palette_predictor_initializer = new ushort[3, 0];
        }
    }
}

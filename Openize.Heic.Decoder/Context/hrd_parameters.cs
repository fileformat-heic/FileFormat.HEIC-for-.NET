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

namespace Openize.Heic.Decoder
{
    internal class hrd_parameters
    {
        private bool nal_hrd_parameters_present_flag;
        private bool vcl_hrd_parameters_present_flag;
        private bool sub_pic_hrd_params_present_flag;
        private byte tick_divisor_minus2;
        private byte du_cpb_removal_delay_increment_length_minus1;
        private byte sub_pic_cpb_params_in_pic_timing_sei_flag;
        private byte dpb_output_delay_du_length_minus1;
        private byte bit_rate_scale;
        private byte cpb_size_scale;
        private byte initial_cpb_removal_delay_length_minus1;
        private byte au_cpb_removal_delay_length_minus1;
        private byte dpb_output_delay_length_minus1;
        private byte cpb_size_du_scale;
        private bool[] fixed_pic_rate_general_flag;
        private bool[] fixed_pic_rate_within_cvs_flag;
        private uint[] elemental_duration_in_tc_minus1;
        private bool[] low_delay_hrd_flag;
        private uint[] cpb_cnt_minus1;
        private sub_layer_hrd_parameters[] sub_layer_hrd_parameters;

        public hrd_parameters(BitStreamWithNalSupport stream, bool commonInfPresentFlag, int maxNumSubLayersMinus1)
        {
            if (commonInfPresentFlag)
            {
                nal_hrd_parameters_present_flag = stream.ReadFlag();           // u(1)
                vcl_hrd_parameters_present_flag = stream.ReadFlag();           // u(1)
                if (nal_hrd_parameters_present_flag || vcl_hrd_parameters_present_flag)
                {
                    sub_pic_hrd_params_present_flag = stream.ReadFlag();       // u(1)
                    if (sub_pic_hrd_params_present_flag)
                    {
                        tick_divisor_minus2 = (byte)stream.Read(8);        // u(8)
                        du_cpb_removal_delay_increment_length_minus1 = 
                            (byte)stream.Read(5);                          // u(5)
                        sub_pic_cpb_params_in_pic_timing_sei_flag = 
                            (byte)stream.Read(8);                          // u(1)
                        dpb_output_delay_du_length_minus1 = 
                            (byte)stream.Read(5);                          // u(5)
                    }

                    bit_rate_scale = (byte)stream.Read(4);                 // u(4)
                    cpb_size_scale = (byte)stream.Read(4);                 // u(4)
                    
                    if (sub_pic_hrd_params_present_flag)
                        cpb_size_du_scale = (byte)stream.Read(4);          // u(4)

                    initial_cpb_removal_delay_length_minus1 = 
                        (byte)stream.Read(5);                              // u(5)
                    au_cpb_removal_delay_length_minus1 = 
                        (byte)stream.Read(5);                              // u(5)
                    dpb_output_delay_length_minus1 = 
                        (byte)stream.Read(5);                              // u(5)
                }
            }

            fixed_pic_rate_general_flag = new bool[maxNumSubLayersMinus1 + 1];
            fixed_pic_rate_within_cvs_flag = new bool[maxNumSubLayersMinus1 + 1];
            elemental_duration_in_tc_minus1 = new uint[maxNumSubLayersMinus1 + 1];
            low_delay_hrd_flag = new bool[maxNumSubLayersMinus1 + 1];
            cpb_cnt_minus1 = new uint[maxNumSubLayersMinus1 + 1];
            sub_layer_hrd_parameters = new sub_layer_hrd_parameters[maxNumSubLayersMinus1 + 1];

            for (int i = 0; i <= maxNumSubLayersMinus1; i++)
            {
                fixed_pic_rate_general_flag[i] = stream.ReadFlag();            // u(1)
                if (!fixed_pic_rate_general_flag[i])
                    fixed_pic_rate_within_cvs_flag[i] = stream.ReadFlag();     // u(1)

                if (fixed_pic_rate_within_cvs_flag[i])
                    elemental_duration_in_tc_minus1[i] = stream.ReadUev();     // ue(v)
                else
                    low_delay_hrd_flag[i] = stream.ReadFlag();                 // u(1)

                if (!low_delay_hrd_flag[i])
                    cpb_cnt_minus1[i] = stream.ReadUev();                      // ue(v)

                uint CpbCnt = cpb_cnt_minus1[i] + 1;

                if (nal_hrd_parameters_present_flag)
                    sub_layer_hrd_parameters[i] = 
                        new sub_layer_hrd_parameters(stream, i, CpbCnt, sub_pic_hrd_params_present_flag);

                if (vcl_hrd_parameters_present_flag)
                    sub_layer_hrd_parameters[i] = 
                        new sub_layer_hrd_parameters(stream, i, CpbCnt, sub_pic_hrd_params_present_flag);
            }

        }
    }
    internal class sub_layer_hrd_parameters
    {
        private uint[] bit_rate_value_minus1;
        private uint[] cpb_size_value_minus1;
        private uint[] cpb_size_du_value_minus1;
        private uint[] bit_rate_du_value_minus1;
        private bool[] cbr_flag;

        public sub_layer_hrd_parameters(BitStreamWithNalSupport stream, int subLayerId, uint CpbCnt, bool sub_pic_hrd_params_present_flag)
        {
            bit_rate_value_minus1 = new uint[CpbCnt];
            cpb_size_value_minus1 = new uint[CpbCnt];
            cpb_size_du_value_minus1 = new uint[CpbCnt];
            bit_rate_du_value_minus1 = new uint[CpbCnt];
            cbr_flag = new bool[CpbCnt];

            for (int i = 0; i < CpbCnt; i++)
            {
                bit_rate_value_minus1[i] = stream.ReadUev();                   // ue(v)
                cpb_size_value_minus1[i] = stream.ReadUev();                   // ue(v)

                if (sub_pic_hrd_params_present_flag)
                {
                    cpb_size_du_value_minus1[i] = stream.ReadUev();            // ue(v)
                    bit_rate_du_value_minus1[i] = stream.ReadUev();            // ue(v)
                }

                cbr_flag[i] = stream.ReadFlag();                               // u(1)
            }
        }
    }
}

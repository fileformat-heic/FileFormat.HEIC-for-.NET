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
    internal class st_ref_pic_set
    {
        internal uint stRpsIdx;

        internal bool inter_ref_pic_set_prediction_flag;
        internal uint delta_idx_minus1;
        internal byte delta_rps_sign;
        internal uint abs_delta_rps_minus1;
        internal bool[] used_by_curr_pic_flag;
        internal bool[] use_delta_flag;
        internal uint num_negative_pics;
        internal uint num_positive_pics;
        internal uint[] delta_poc_s0_minus1;
        internal bool[] used_by_curr_pic_s0_flag;
        internal uint[] delta_poc_s1_minus1;
        internal bool[] used_by_curr_pic_s1_flag;

        internal st_ref_pic_set(BitStreamWithNalSupport stream, seq_parameter_set_rbsp sps, uint stRpsIdx)
        {
            this.stRpsIdx = stRpsIdx;

            if (stRpsIdx != 0)
                inter_ref_pic_set_prediction_flag = stream.ReadFlag();     // u(1)

            if (inter_ref_pic_set_prediction_flag)
            {
                if (stRpsIdx == sps.num_short_term_ref_pic_sets)
                    delta_idx_minus1 = stream.ReadUev();                          // ue(v)

                delta_rps_sign = (byte)stream.Read(1);                     // u(1)
                abs_delta_rps_minus1 = stream.ReadUev();                          // ue(v)
                uint RefRpsIdx = stRpsIdx - (delta_idx_minus1 + 1);

                used_by_curr_pic_flag = new bool[sps.NumDeltaPocs((int)RefRpsIdx) + 1];
                use_delta_flag = new bool[sps.NumDeltaPocs((int)RefRpsIdx) + 1];

                for (int j = 0; j <= sps.NumDeltaPocs((int)RefRpsIdx); j++)
                {
                    used_by_curr_pic_flag[j] = stream.ReadFlag();          // u(1)
                    if (!used_by_curr_pic_flag[j])
                        use_delta_flag[j] = stream.ReadFlag();             // u(1)
                }
            }
            else
            {
                num_negative_pics = stream.ReadUev();                          // ue(v)
                num_positive_pics = stream.ReadUev();                          // ue(v)
                delta_poc_s0_minus1 = new uint[num_negative_pics];
                used_by_curr_pic_s0_flag = new bool[num_negative_pics];
                delta_poc_s1_minus1 = new uint[num_positive_pics];
                used_by_curr_pic_s1_flag = new bool[num_positive_pics];

                for (int i = 0; i < num_negative_pics; i++)
                {
                    delta_poc_s0_minus1[i] = stream.ReadUev();                    // ue(v)
                    used_by_curr_pic_s0_flag[i] = stream.ReadFlag();       // u(1)
                }

                for (int i = 0; i < num_positive_pics; i++)
                {
                    delta_poc_s1_minus1[i] = stream.ReadUev();                    // ue(v)
                    used_by_curr_pic_s1_flag[i] = stream.ReadFlag();       // u(1)
                }
            }
        }
        public uint NumNegativePics => num_negative_pics;
        public uint NumPositivePics => num_positive_pics;
        public uint NumDeltaPocs => NumNegativePics + NumPositivePics;
        public bool[] UsedByCurrPicS0 => used_by_curr_pic_s0_flag;
        public bool[] UsedByCurrPicS1 => used_by_curr_pic_s1_flag;
        public long DeltaPocS0(int i) => i == 0 ?
            -(delta_poc_s0_minus1[i] + 1) :
             DeltaPocS0(i - 1) - (delta_poc_s0_minus1[i] + 1);
        public long DeltaPocS1(int i) => i == 0 ?
            -(delta_poc_s1_minus1[i] + 1) :
             DeltaPocS1(i - 1) - (delta_poc_s1_minus1[i] + 1);
    }

}

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
    internal class ref_pic_lists_modification {
        internal bool ref_pic_list_modification_flag_l0;
        internal bool ref_pic_list_modification_flag_l1;
        internal int[] list_entry_l0;
        internal int[] list_entry_l1;

        internal ref_pic_lists_modification(
            BitStreamWithNalSupport stream,
            SliceType slice_type,
            uint num_ref_idx_l0_active_minus1,
            uint num_ref_idx_l1_active_minus1,
            pic_parameter_set_rbsp pps)
        {
            ref_pic_list_modification_flag_l0 = stream.ReadFlag();         // u(1)
            list_entry_l0 = new int[num_ref_idx_l0_active_minus1 + 1];

            int NumPicTotalCurr = GetNumPicTotalCurr(pps);


            if (ref_pic_list_modification_flag_l0)
                for (int i = 0; i <= num_ref_idx_l1_active_minus1; i++)
                    list_entry_l0[i] = stream.Read((int)                   // u(v)
                        Math.Ceiling(Math.Log(NumPicTotalCurr, 2)));

            if (slice_type == SliceType.B)
            {
                ref_pic_list_modification_flag_l1 = stream.ReadFlag();     // u(1)
                list_entry_l1 = new int[num_ref_idx_l0_active_minus1 + 1];

                if (ref_pic_list_modification_flag_l1)
                    for (int i = 0; i <= num_ref_idx_l1_active_minus1; i++)
                        list_entry_l1[i] = stream.Read((int)               // u(v)
                            Math.Ceiling(Math.Log(NumPicTotalCurr, 2)));
            }
        }

        int GetNumPicTotalCurr(pic_parameter_set_rbsp pps)
        {
            throw new NotImplementedException();


            /*
            int NumPicTotalCurr = 0;
            for (int i = 0; i < NumNegativePics[CurrRpsIdx]; i++)
                if (UsedByCurrPicS0[CurrRpsIdx][i])
                    NumPicTotalCurr++;

            for (int i = 0; i < NumPositivePics[CurrRpsIdx]; i++)
                if (UsedByCurrPicS1[CurrRpsIdx][i])
                    NumPicTotalCurr++;

            for (int i = 0; i < header.num_long_term_sps + header.num_long_term_pics; i++)
                if (UsedByCurrPicLt[i])
                    NumPicTotalCurr++;

            if (pps.pps_scc_ext.pps_curr_pic_ref_enabled_flag)
                NumPicTotalCurr++;
            
            return NumPicTotalCurr;*/
        }
    }

}

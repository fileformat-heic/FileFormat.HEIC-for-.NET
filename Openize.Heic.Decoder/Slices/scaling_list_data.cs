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
using System;

namespace Openize.Heic.Decoder
{
    internal class scaling_list_data
    {
        internal bool[,] scaling_list_pred_mode_flag;
        internal uint[,] scaling_list_pred_matrix_id_delta;
        internal int[][] scaling_list_dc_coef_minus8;

        internal byte[,][] ScalingList;

        public scaling_list_data(BitStreamWithNalSupport stream)
        {
            throw new NotImplementedException("Scaling list config is not met in static image cases.");

            scaling_list_pred_mode_flag = new bool[4, 6];
            scaling_list_pred_matrix_id_delta = new uint[4, 6];
            scaling_list_dc_coef_minus8 = new int[2][];
            ScalingList = new byte[4, 6][];

            for (int sizeId = 0; sizeId < 4; sizeId++)
            {
                if (sizeId > 1)
                    scaling_list_dc_coef_minus8[sizeId - 2] = new int[6];

                for (int matrixId = 0; matrixId < 6; matrixId += (sizeId == 3) ? 3 : 1)
                {
                    scaling_list_pred_mode_flag[sizeId, matrixId] = stream.ReadFlag();
                    if (!scaling_list_pred_mode_flag[sizeId, matrixId]) 
                    {
                        scaling_list_pred_matrix_id_delta[sizeId, matrixId] = stream.ReadUev();
                    }
                    else
                    {
                        int nextCoef = 8;
                        int coefNum = 1 << (4 + (sizeId << 1));
                        coefNum = coefNum < 64 ? coefNum : 64;
                        ScalingList[sizeId, matrixId] = new byte[coefNum];

                        if (sizeId > 1)
                        {
                            scaling_list_dc_coef_minus8[sizeId-2][matrixId] = stream.ReadSev();
                            nextCoef = (int)scaling_list_dc_coef_minus8[sizeId-2][matrixId];
                        }

                        for (int i = 0; i < coefNum; i++)
                        {
                            int scaling_list_delta_coef = stream.ReadSev();
                            nextCoef = (nextCoef + scaling_list_delta_coef + 256) % 256;
                            ScalingList[sizeId, matrixId][i] = (byte)nextCoef;
                        }
                    }
                }
            }
        }
    }

}

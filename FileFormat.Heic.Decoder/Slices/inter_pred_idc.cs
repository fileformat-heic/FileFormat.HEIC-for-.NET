/*
 * FileFormat.HEIC 
 * Copyright (c) 2024 Openize Pty Ltd. 
 *
 * This file is part of FileFormat.HEIC.
 *
 * FileFormat.HEIC is available under Openize license, which is
 * available along with FileFormat.HEIC sources.
 */

namespace FileFormat.Heic.Decoder
{
    internal enum inter_pred_idc
    {
        PRED_L0 = 1,
        PRED_L1 = 2,
        PRED_BI = 3 // ( nPbW + nPbH ) != 12
    };
}

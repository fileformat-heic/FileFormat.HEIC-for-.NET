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
    internal enum PartMode
    {
        PART_2Nx2N = 0,
        PART_2NxN = 1,
        PART_Nx2N = 2,
        PART_NxN = 3,
        PART_2NxnU = 4,
        PART_2NxnD = 5,
        PART_nLx2N = 6,
        PART_nRx2N = 7
    }
}

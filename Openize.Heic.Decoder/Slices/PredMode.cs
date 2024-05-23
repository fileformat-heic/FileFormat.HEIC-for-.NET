/*
 * Openize.HEIC 
 * Copyright (c) 2024 Openize Pty Ltd. 
 *
 * This file is part of Openize.HEIC.
 *
 * Openize.HEIC is available under Openize license, which is
 * available along with Openize.HEIC sources.
 */

namespace Openize.Heic.Decoder
{
    internal enum PredMode
    {
        MODE_INTRA = 0,
        MODE_INTER = 1,
        MODE_SKIP = 2
    }

    internal enum IntraPredMode
    {
        INTRA_PLANAR = 0,
        INTRA_DC = 1,
        INTRA_ANGULAR2 = 2,
        INTRA_ANGULAR3 = 3,
        INTRA_ANGULAR4 = 4,
        INTRA_ANGULAR5 = 5,
        INTRA_ANGULAR6 = 6,
        INTRA_ANGULAR7 = 7,
        INTRA_ANGULAR8 = 8,
        INTRA_ANGULAR9 = 9,
        INTRA_ANGULAR10 = 10,
        INTRA_ANGULAR11 = 11,
        INTRA_ANGULAR12 = 12,
        INTRA_ANGULAR13 = 13,
        INTRA_ANGULAR14 = 14,
        INTRA_ANGULAR15 = 15,
        INTRA_ANGULAR16 = 16,
        INTRA_ANGULAR17 = 17,
        INTRA_ANGULAR18 = 18,
        INTRA_ANGULAR19 = 19,
        INTRA_ANGULAR20 = 20,
        INTRA_ANGULAR21 = 21,
        INTRA_ANGULAR22 = 22,
        INTRA_ANGULAR23 = 23,
        INTRA_ANGULAR24 = 24,
        INTRA_ANGULAR25 = 25,
        INTRA_ANGULAR26 = 26,
        INTRA_ANGULAR27 = 27,
        INTRA_ANGULAR28 = 28,
        INTRA_ANGULAR29 = 29,
        INTRA_ANGULAR30 = 30,
        INTRA_ANGULAR31 = 31,
        INTRA_ANGULAR32 = 32,
        INTRA_ANGULAR33 = 33,
        INTRA_ANGULAR34 = 34,
        INTRA_WEDGE = 35,
        INTRA_CONTOUR = 36,
        INTRA_SINGLE = 37
    };


    internal enum IntraChromaPredMode
    {
        INTRA_CHROMA_PLANAR_OR_34 = 0,
        INTRA_CHROMA_ANGULAR26_OR_34 = 1,
        INTRA_CHROMA_ANGULAR10_OR_34 = 2,
        INTRA_CHROMA_DC_OR_34 = 3,
        INTRA_CHROMA_LIKE_LUMA = 4
    };


    internal enum InterPredIdc
    {
        PRED_L0 = 1,
        PRED_L1 = 2,
        PRED_BI = 3
    };
}

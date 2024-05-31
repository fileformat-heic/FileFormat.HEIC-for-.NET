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
    internal struct NalHeader
    {

        public NalUnitType type;
        public byte nuh_layer_id;
        public byte nuh_temporal_id_plus1; // > 0

        internal NalHeader(BitStreamWithNalSupport stream)
        {
            stream.SkipBits(1); // 1
            type = (NalUnitType)stream.Read(6);
            nuh_layer_id = (byte)stream.Read(6);
            nuh_temporal_id_plus1 = (byte)stream.Read(3);
        }

        internal byte TemporalId => (byte)(nuh_temporal_id_plus1 - 1);

        internal bool IsRadlPicture =>
            type == NalUnitType.RADL_N ||          // 6
            type == NalUnitType.RADL_R;            // 7
        internal bool IsRaslPicture =>
            type == NalUnitType.RASL_N ||          // 8
            type == NalUnitType.RASL_R;            // 9

        internal bool IsIdrPicture =>
            type == NalUnitType.IDR_W_RADL ||      // 19
            type == NalUnitType.IDR_N_LP;          // 20
        internal bool IsBlaPicture =>
            type == NalUnitType.BLA_W_LP ||        // 16
            type == NalUnitType.BLA_W_RADL ||      // 17
            type == NalUnitType.BLA_N_LP;          // 18
        internal bool IsCraPicture =>
            type == NalUnitType.CRA_NUT;           // 21

        internal bool IsRapPicture =>
            IsIdrPicture || IsBlaPicture || IsCraPicture;   // 16..21

        internal bool IsIrapPicture =>                      // 16..23
            (int)type >= 16 &&
            (int)type <= 23;

        internal bool IsReferenceNALU =>
            (((type <= NalUnitType.RSV_VCL_R15) && ((int)type % 2 != 0)) ||
             ((type >= NalUnitType.BLA_W_LP) && (type <= NalUnitType.RSV_IRAP_VCL23)));

        internal bool IsSublayerNonReference {
            get {
                switch (type)
                {
                    case NalUnitType.TRAIL_N:
                    case NalUnitType.TSA_N:
                    case NalUnitType.STSA_N:
                    case NalUnitType.RADL_N:
                    case NalUnitType.RASL_N:
                    case NalUnitType.RSV_VCL_N10:
                    case NalUnitType.RSV_VCL_N12:
                    case NalUnitType.RSV_VCL_N14:
                        return true;
                    default:
                        return false;
                };
            }
        }
    }
}

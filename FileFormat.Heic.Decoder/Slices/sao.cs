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
    internal class sao
    {
        internal bool sao_merge_left_flag;
        internal bool sao_merge_up_flag;

        internal bool leftCtbInSliceSeg;
        internal bool leftCtbInTile;

        internal bool upCtbInSliceSeg;
        internal bool upCtbInTile;

        internal byte sao_type_idx_luma;
        internal byte sao_type_idx_chroma;
        internal byte sao_eo_class_luma;
        internal byte sao_eo_class_chroma;

        public sao(BitStreamWithNalSupport stream, int rx, int ry,
            slice_segment_header header)
        {
            var sps = header.pps.sps;
            var picture = header.parentPicture;

            if (rx > 0)
            {
                leftCtbInSliceSeg = header.CtbAddrInRs > header.SliceAddrRs;
                leftCtbInTile = 
                    header.pps.TileId[header.CtbAddrInTs] == 
                    header.pps.TileId[header.pps.CtbAddrRsToTs[header.CtbAddrInRs - 1]];

                if (leftCtbInSliceSeg && leftCtbInTile)
                    sao_merge_left_flag = stream.Cabac.read_sao_merge_flag();
            }

            if (ry > 0 && !sao_merge_left_flag)
            {
                upCtbInSliceSeg = (header.CtbAddrInRs - sps.PicWidthInCtbsY ) >= header.SliceAddrRs;
                upCtbInTile = 
                    header.pps.TileId[header.CtbAddrInTs] == 
                    header.pps.TileId[header.pps.CtbAddrRsToTs[header.CtbAddrInRs - sps.PicWidthInCtbsY]];

                if (upCtbInSliceSeg && upCtbInTile)
                    sao_merge_up_flag = stream.Cabac.read_sao_merge_flag();
            }

            int chroma_count = (header.pps.sps.ChromaArrayType != 0 ? 3 : 1);

            for (int cIdx = 0; cIdx < chroma_count; cIdx++)
            {
                if (picture.sao_offset_abs[cIdx][rx, ry] == null)
                    picture.sao_offset_abs[cIdx][rx, ry] = new int[4];

                if (picture.sao_offset_sign[cIdx][rx, ry] == null)
                    picture.sao_offset_sign[cIdx][rx, ry] = new int[4];

                if (picture.SaoOffsetVal[cIdx][rx, ry] == null)
                    picture.SaoOffsetVal[cIdx][rx, ry] = new int[5];

                if (!sao_merge_up_flag && !sao_merge_left_flag)
                {

                    if ((header.slice_sao_luma_flag && cIdx == 0) ||
                        (header.slice_sao_chroma_flag && cIdx > 0))
                    {
                        if (cIdx == 0)
                        {
                            sao_type_idx_luma = (byte)stream.Cabac.read_sao_type_idx();
                            picture.SaoTypeIdx[cIdx][rx, ry] = sao_type_idx_luma;
                        }
                        else if (cIdx == 1)
                        {
                            sao_type_idx_chroma = (byte)stream.Cabac.read_sao_type_idx();
                            picture.SaoTypeIdx[cIdx][rx, ry] = sao_type_idx_chroma;
                            picture.SaoTypeIdx[cIdx + 1][rx, ry] = sao_type_idx_chroma;
                        }

                        if (picture.SaoTypeIdx[cIdx][rx, ry] != 0)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                picture.sao_offset_abs[cIdx][rx, ry][i] =
                                    stream.Cabac.read_sao_offset_abs(cIdx == 0 ? sps.BitDepthY : sps.BitDepthC); // ae(v)
                            }

                            if (picture.SaoTypeIdx[cIdx][rx, ry] == 1) // Band offset
                            {
                                for (int i = 0; i < 4; i++)
                                {
                                    if (picture.sao_offset_abs[cIdx][rx, ry][i] != 0)
                                        picture.sao_offset_sign[cIdx][rx, ry][i] = stream.Cabac.read_sao_offset_sign(); // ae(v)
                                    else
                                        picture.sao_offset_sign[cIdx][rx, ry][i] = 0;
                                }

                                picture.sao_band_position[cIdx][rx, ry] = stream.Cabac.read_sao_band_position(); // ae(v)
                            }
                            else // SaoTypeIdx == 2; Edge offset
                            {
                                if (cIdx == 0)
                                {
                                    sao_eo_class_luma = (byte)stream.Cabac.read_sao_class();      // ae(v)
                                    picture.SaoEoClass[cIdx][rx, ry] = sao_eo_class_luma;
                                }
                                else if (cIdx == 1)
                                {
                                    sao_eo_class_chroma = (byte)stream.Cabac.read_sao_class();    // ae(v)
                                    picture.SaoEoClass[cIdx][rx, ry] = sao_eo_class_chroma;
                                    picture.SaoEoClass[cIdx + 1][rx, ry] = sao_eo_class_chroma;
                                }

                                for (int i = 0; i < 4; i++)
                                    picture.sao_offset_sign[cIdx][rx, ry][i] = i < 2 ? 0 : 1;
                            }
                        }
                    }
                    else
                    {
                        picture.SaoTypeIdx[cIdx][rx, ry] = 0;
                        picture.SaoEoClass[cIdx][rx, ry] = 0;
                        picture.sao_band_position[cIdx][rx, ry] = 0;

                        for (int i = 0; i < 4; i++)
                        {
                            picture.sao_offset_abs[cIdx][rx, ry][i] = 0;
                            picture.sao_offset_sign[cIdx][rx, ry][i] = 0;
                        }
                    }
                }
                else if (sao_merge_left_flag)
                {
                    picture.SaoTypeIdx[cIdx][rx, ry] = picture.SaoTypeIdx[cIdx][rx - 1, ry];
                    picture.SaoEoClass[cIdx][rx, ry] = picture.SaoEoClass[cIdx][rx - 1, ry];
                    picture.sao_band_position[cIdx][rx, ry] = picture.sao_band_position[cIdx][rx - 1, ry];

                    for (int i = 0; i < 4; i++)
                    {
                        picture.sao_offset_abs[cIdx][rx, ry][i] = picture.sao_offset_abs[cIdx][rx - 1, ry][i];
                        picture.sao_offset_sign[cIdx][rx, ry][i] = picture.sao_offset_sign[cIdx][rx - 1, ry][i];
                    }
                }
                else if (sao_merge_up_flag)
                {
                    picture.SaoTypeIdx[cIdx][rx, ry] = picture.SaoTypeIdx[cIdx][rx, ry - 1];
                    picture.SaoEoClass[cIdx][rx, ry] = picture.SaoEoClass[cIdx][rx, ry - 1];
                    picture.sao_band_position[cIdx][rx, ry] = picture.sao_band_position[cIdx][rx, ry - 1];

                    for (int i = 0; i < 4; i++)
                    {
                        picture.sao_offset_abs[cIdx][rx, ry][i] = picture.sao_offset_abs[cIdx][rx, ry - 1][i];
                        picture.sao_offset_sign[cIdx][rx, ry][i] = picture.sao_offset_sign[cIdx][rx, ry - 1][i];
                    }
                }

                if (header.pps.pps_range_ext != null)
                {
                    int log2OffsetScale = (int)(cIdx == 0 ?
                        header.pps.pps_range_ext.log2_sao_offset_scale_luma :
                        header.pps.pps_range_ext.log2_sao_offset_scale_chroma);

                    picture.SaoOffsetVal[cIdx][rx, ry][0] = 0;

                    for (int i = 0; i < 4; i++)
                        picture.SaoOffsetVal[cIdx][rx, ry][i + 1] =
                            (1 - 2 * picture.sao_offset_sign[cIdx][rx, ry][i]) *
                            picture.sao_offset_abs[cIdx][rx, ry][i] << log2OffsetScale;

                }
            }
        }
    }
}

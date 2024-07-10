/*
 * FileFormat.HEIC 
 * Copyright (c) 2024 Openize Pty Ltd. 
 *
 * This file is part of FileFormat.HEIC.
 *
 * FileFormat.HEIC is available under Openize license, which is
 * available along with FileFormat.HEIC sources.
 */

using System.Collections.Generic;
using FileFormat.Heic.Decoder.IO;

namespace FileFormat.Heic.Decoder
{
    internal class slice_segment_data
    {
        List<coding_tree_unit> CTUs = new List<coding_tree_unit>();

        public slice_segment_data(
            BitStreamWithNalSupport stream,
            slice_segment_header header)
        {
            bool end_of_slice_segment_flag;
            var sps = header.pps.sps;
            var picture = header.parentPicture;

            if (picture.SliceAddrRs == null) 
                picture.SliceAddrRs = new uint[sps.PicWidthInCtbsY, sps.PicHeightInCtbsY];

            if (picture.SliceHeaderIndex == null)
                picture.SliceHeaderIndex = new int[sps.PicWidthInCtbsY << sps.CtbLog2SizeY, sps.PicHeightInCtbsY << sps.CtbLog2SizeY];

            do
            {
                int xCtu = (int)(header.CtbAddrInRs % sps.PicWidthInCtbsY);
                int yCtu = (int)(header.CtbAddrInRs / sps.PicWidthInCtbsY);

                if (header.pps.entropy_coding_sync_enabled_flag &&
                    xCtu == 0 && yCtu > 0)
                {
                    stream.Cabac.RestoreSyncedTables();
                }

                var ctu = new coding_tree_unit(stream, header);
                CTUs.Add(ctu);

                if (header.pps.entropy_coding_sync_enabled_flag &&
                    xCtu == 1 &&
                    yCtu < sps.PicHeightInCtbsY - 1)
                {
                    stream.Cabac.SyncTables();
                }

                end_of_slice_segment_flag = stream.Cabac.read_end_of_slice_segment_flag(); 

                header.CtbAddrInTs++;
                if (header.CtbAddrInTs < sps.PicSizeInCtbsY)
                    header.CtbAddrInRs = header.pps.CtbAddrTsToRs[header.CtbAddrInTs];

                if (header.CtbAddrInTs == sps.PicSizeInCtbsY)
                    return;

                if (!end_of_slice_segment_flag &&

                    ((header.pps.tiles_enabled_flag && 
                    header.pps.TileId[header.CtbAddrInTs] != header.pps.TileId[header.CtbAddrInTs - 1]) ||

                    (header.pps.entropy_coding_sync_enabled_flag &&
                    (header.CtbAddrInRs % sps.PicWidthInCtbsY == 0 ||
                    header.pps.TileId[header.CtbAddrInTs] != header.pps.TileId[header.pps.CtbAddrRsToTs[header.CtbAddrInRs - 1]]))))
                {
                    int one = stream.Cabac.read_end_of_subset_one_bit(); /* equal to 1; */

                    while (!stream.ByteAligned())
                        stream.SkipBits(1);

                    stream.Cabac.ResetStreamState();
                }
            
            } while (!end_of_slice_segment_flag);
        }
    }
}

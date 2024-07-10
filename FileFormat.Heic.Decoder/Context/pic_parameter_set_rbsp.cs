/*
 * FileFormat.HEIC 
 * Copyright (c) 2024 Openize Pty Ltd. 
 *
 * This file is part of FileFormat.HEIC.
 *
 * FileFormat.HEIC is available under Openize license, which is
 * available along with FileFormat.HEIC sources.
 */

using System;
using FileFormat.Heic.Decoder.IO;

namespace FileFormat.Heic.Decoder
{
    internal class pic_parameter_set_rbsp : NalUnit
    {
        internal byte pps_pic_parameter_set_id; // max - 64
        internal byte pps_seq_parameter_set_id;
        internal bool dependent_slice_segments_enabled_flag;
        internal bool output_flag_present_flag;
        internal byte num_extra_slice_header_bits;
        internal bool sign_data_hiding_enabled_flag;
        internal bool cabac_init_present_flag;
        internal uint num_ref_idx_l0_default_active_minus1;
        internal uint num_ref_idx_l1_default_active_minus1;
        internal int init_qp_minus26;
        internal bool constrained_intra_pred_flag;
        internal bool transform_skip_enabled_flag;
        internal bool cu_qp_delta_enabled_flag;
        internal uint diff_cu_qp_delta_depth;
        internal int pps_cb_qp_offset;
        internal int pps_cr_qp_offset;
        internal bool pps_slice_chroma_qp_offsets_present_flag;
        internal bool weighted_pred_flag;
        internal bool weighted_bipred_flag;
        internal bool transquant_bypass_enabled_flag;
        internal bool tiles_enabled_flag;
        internal bool entropy_coding_sync_enabled_flag;
        internal uint num_tile_columns_minus1;
        internal uint num_tile_rows_minus1;
        internal bool uniform_spacing_flag;
        internal uint[] column_width_minus1;
        internal uint[] row_height_minus1;
        internal bool loop_filter_across_tiles_enabled_flag;
        internal bool pps_loop_filter_across_slices_enabled_flag;
        internal bool deblocking_filter_control_present_flag;
        internal bool deblocking_filter_override_enabled_flag;
        internal bool pps_deblocking_filter_disabled_flag;
        internal int pps_beta_offset_div2;
        internal int pps_tc_offset_div2;
        internal bool pps_scaling_list_data_present_flag;
        internal bool lists_modification_present_flag;
        internal uint log2_parallel_merge_level_minus2;
        internal bool slice_segment_header_extension_present_flag;
        internal bool pps_extension_present_flag;
        internal bool pps_range_extension_flag;
        internal bool pps_multilayer_extension_flag;
        internal bool pps_3d_extension_flag;
        internal bool pps_scc_extension_flag;
        internal byte pps_extension_4bits;
        internal bool pps_extension_data_flag;
        internal pps_scc_extension pps_scc_ext;
        internal pps_3d_extension pps_3d_ext;
        internal pps_multilayer_extension pps_multilayer_ext;
        internal pps_range_extension pps_range_ext;

        public seq_parameter_set_rbsp sps;

        // the width of the i-th tile column in units of CTBs
        public uint[] colWidth;

        // the height of the j-th tile row in units of CTBs
        public uint[] rowHeight;

        // the location of the i-th tile column boundary in units of CTBs
        public uint[] colBd;

        // the location of the j-th tile row boundary in units of CTBs
        public uint[] rowBd;


        // the conversion from a CTB address in CTB raster scan of a picture to a CTB address in tile scan
        public uint[] CtbAddrRsToTs;

        // the conversion from a CTB address in tile scan to a CTB address in CTB raster scan of a picture
        public uint[] CtbAddrTsToRs;

        // the conversion from a CTB address in tile scan to a tile ID
        public uint[] TileId;

        // the conversion from a CTB address in CTB raster scan to a tile ID
        public uint[] TileIdFromRs;

        // the width of the i-th tile column in units of luma samples
        public uint[] ColumnWidthInLumaSamples;

        // the height of the j-th tile row in units of luma samples
        public uint[] RowHeightInLumaSamples;

        // the conversion from a location (x, y) in units of minimum transform blocks
        // to a transform block address in z-scan order
        public uint[,] MinTbAddrZs;

        public new string ToString() => $"NAL Unit PPS " +
            $"\nNumber of l0 l1 refs: {num_ref_idx_l0_default_active_minus1 + 1} {num_ref_idx_l1_default_active_minus1 + 1} ";

        public uint Log2MinCuQpDeltaSize => sps.CtbLog2SizeY - diff_cu_qp_delta_depth;
        public uint Log2MinCuChromaQpOffsetSize => sps.CtbLog2SizeY - pps_range_ext.diff_cu_chroma_qp_offset_depth;
        public uint Log2MaxTransformSkipSize => pps_range_ext.log2_max_transform_skip_block_size_minus2 + 2;

        public int PpsActQpOffsetY => pps_scc_ext.pps_act_y_qp_offset_plus5 - 5;
        public int PpsActQpOffsetCb => pps_scc_ext.pps_act_cb_qp_offset_plus5 - 5;
        public int PpsActQpOffsetCr => pps_scc_ext.pps_act_cr_qp_offset_plus3 - 3;
        
        public pic_parameter_set_rbsp(BitStreamWithNalSupport stream, ulong startPosition, int size) : base(stream, startPosition, size)
        {
            pps_pic_parameter_set_id = (byte)stream.ReadUev();                 // ue(v)
            pps_seq_parameter_set_id = (byte)stream.ReadUev();                 // ue(v)

            sps = stream.Context.SPS[pps_seq_parameter_set_id];

            dependent_slice_segments_enabled_flag = stream.ReadFlag();          // u(1)
            output_flag_present_flag = stream.ReadFlag();                       // u(1)
            num_extra_slice_header_bits = (byte)stream.Read(3);                 // u(3)
            sign_data_hiding_enabled_flag = stream.ReadFlag();                  // u(1)
            cabac_init_present_flag = stream.ReadFlag();                        // u(1)

            num_ref_idx_l0_default_active_minus1 = stream.ReadUev();           // ue(v)
            num_ref_idx_l1_default_active_minus1 = stream.ReadUev();           // ue(v)
            init_qp_minus26 = stream.ReadSev();                                // se(v)
            constrained_intra_pred_flag = stream.ReadFlag();                    // u(1)
            transform_skip_enabled_flag = stream.ReadFlag();                    // u(1)
            cu_qp_delta_enabled_flag = stream.ReadFlag();                       // u(1)
            if (cu_qp_delta_enabled_flag)
                diff_cu_qp_delta_depth = stream.ReadUev();                     // ue(v)
            pps_cb_qp_offset = stream.ReadSev();                               // se(v)
            pps_cr_qp_offset = stream.ReadSev();                               // se(v)
            pps_slice_chroma_qp_offsets_present_flag = stream.ReadFlag();       // u(1)
            weighted_pred_flag = stream.ReadFlag();                             // u(1)
            weighted_bipred_flag = stream.ReadFlag();                           // u(1)
            transquant_bypass_enabled_flag = stream.ReadFlag();                 // u(1)
            tiles_enabled_flag = stream.ReadFlag();                             // u(1)
            entropy_coding_sync_enabled_flag = stream.ReadFlag();               // u(1)

            if (tiles_enabled_flag)
            {
                num_tile_columns_minus1 = stream.ReadUev();                    // ue(v)
                num_tile_rows_minus1 = stream.ReadUev();                       // ue(v)
                uniform_spacing_flag = stream.ReadFlag();                       // u(1)

                if (!uniform_spacing_flag)
                {
                    column_width_minus1 = new uint[num_tile_columns_minus1];
                    row_height_minus1 = new uint[num_tile_rows_minus1];

                    for (int i = 0; i < num_tile_columns_minus1; i++)
                        column_width_minus1[i] = stream.ReadUev();             // ue(v)

                    for (int i = 0; i < num_tile_rows_minus1; i++)
                        row_height_minus1[i] = stream.ReadUev();               // ue(v)
                }

                loop_filter_across_tiles_enabled_flag = stream.ReadFlag();      // u(1)
            }

            pps_loop_filter_across_slices_enabled_flag = stream.ReadFlag();     // u(1)
            deblocking_filter_control_present_flag = stream.ReadFlag();         // u(1)
            if (deblocking_filter_control_present_flag)
            {
                deblocking_filter_override_enabled_flag = stream.ReadFlag();    // u(1)
                pps_deblocking_filter_disabled_flag = stream.ReadFlag();        // u(1)
                if (!pps_deblocking_filter_disabled_flag)
                {
                    pps_beta_offset_div2 = stream.ReadSev();                   // se(v)
                    pps_tc_offset_div2 = stream.ReadSev();                     // se(v)
                }
            }

            pps_scaling_list_data_present_flag = stream.ReadFlag();             // u(1)
            if (pps_scaling_list_data_present_flag)
                new scaling_list_data(stream);

            lists_modification_present_flag = stream.ReadFlag();                // u(1)
            log2_parallel_merge_level_minus2 = stream.ReadUev();               // ue(v)
            slice_segment_header_extension_present_flag = stream.ReadFlag();    // u(1)
            
            pps_extension_present_flag = stream.ReadFlag();                     // u(1)
            if (pps_extension_present_flag)
            {
                pps_range_extension_flag = stream.ReadFlag();                   // u(1)
                pps_multilayer_extension_flag = stream.ReadFlag();              // u(1)
                pps_3d_extension_flag = stream.ReadFlag();                      // u(1)
                pps_scc_extension_flag = stream.ReadFlag();                     // u(1)
                pps_extension_4bits = (byte)stream.Read(4);                     // u(4)
            }

            pps_range_ext = pps_range_extension_flag ? 
                new pps_range_extension(stream, transform_skip_enabled_flag) :
                new pps_range_extension();

            if (pps_multilayer_extension_flag)
                pps_multilayer_ext = new pps_multilayer_extension(stream); /* specified in Annex F */

            if (pps_3d_extension_flag)
                pps_3d_ext = new pps_3d_extension(stream); /* specified in Annex I */

            pps_scc_ext = pps_scc_extension_flag ?
                new pps_scc_extension(stream) :
                new pps_scc_extension();

            if (pps_extension_4bits > 0)
            {
                while (stream.HasMoreRbspData(EndPosition))
                    pps_extension_data_flag = stream.ReadFlag();         // u(1)
            }
            
            new rbsp_trailing_bits(stream);

            ctb_raster_and_tile_scan();
            zscan_init();
        }


        // 6.5.1 CTB raster and tile scanning conversion process
        internal void ctb_raster_and_tile_scan()
        {
            colWidth = new uint[num_tile_columns_minus1 + 1];
            rowHeight = new uint[num_tile_rows_minus1 + 1];

            if (uniform_spacing_flag)
            {
                for (int i = 0; i <= num_tile_columns_minus1; i++)
                {
                    colWidth[i] = (uint)(((i + 1) * sps.PicWidthInCtbsY) / (num_tile_columns_minus1 + 1) -
                        (i * sps.PicWidthInCtbsY) / (num_tile_columns_minus1 + 1));
                }

                for (int j = 0; j <= num_tile_rows_minus1; j++)
                {
                    rowHeight[j] = (uint)(((j + 1) * sps.PicHeightInCtbsY) / (num_tile_rows_minus1 + 1) -
                        (j * sps.PicHeightInCtbsY) / (num_tile_rows_minus1 + 1));
                }
            }
            else
            {
                colWidth[num_tile_columns_minus1] = sps.PicWidthInCtbsY;
                rowHeight[num_tile_rows_minus1] = sps.PicHeightInCtbsY;

                for (int i = 0; i < num_tile_columns_minus1; i++)
                {
                    colWidth[i] = column_width_minus1[i] + 1;
                    colWidth[num_tile_columns_minus1] -= colWidth[i];
                }

                for (int j = 0; j < num_tile_rows_minus1; j++)
                {
                    rowHeight[j] = row_height_minus1[j] + 1;
                    rowHeight[num_tile_rows_minus1] -= rowHeight[j];
                }
            }


            colBd = new uint[num_tile_columns_minus1 + 2];
            rowBd = new uint[num_tile_rows_minus1 + 2];

            colBd[0] = 0;
            rowBd[0] = 0;

            for (int i = 0; i <= num_tile_columns_minus1; i++)
                colBd[i + 1] = colBd[i] + colWidth[i];

            for (int j = 0; j <= num_tile_rows_minus1; j++)
                rowBd[j + 1] = rowBd[j] + rowHeight[j];


            CtbAddrRsToTs = new uint[sps.PicSizeInCtbsY];
            CtbAddrTsToRs = new uint[sps.PicSizeInCtbsY];
            TileId = new uint[sps.PicSizeInCtbsY];
            TileIdFromRs = new uint[sps.PicSizeInCtbsY];

            uint tbX, tbY;
            uint tileX = 0;
            uint tileY = 0;

            for (uint ctbAddrRs = 0; ctbAddrRs < sps.PicSizeInCtbsY; ctbAddrRs++)
            {
                tbX = ctbAddrRs % sps.PicWidthInCtbsY;
                tbY = ctbAddrRs / sps.PicWidthInCtbsY;

                for (uint i = 0; i <= num_tile_columns_minus1; i++)
                    if (tbX >= colBd[i])
                        tileX = i;

                for (uint j = 0; j <= num_tile_rows_minus1; j++)
                    if (tbY >= rowBd[j])
                        tileY = j;

                CtbAddrRsToTs[ctbAddrRs] = 0;

                for (int i = 0; i < tileX; i++)
                    CtbAddrRsToTs[ctbAddrRs] += rowHeight[tileY] * colWidth[i];

                for (int j = 0; j < tileY; j++)
                    CtbAddrRsToTs[ctbAddrRs] += sps.PicWidthInCtbsY * rowHeight[j];

                CtbAddrRsToTs[ctbAddrRs] +=
                    (tbY - rowBd[tileY]) * colWidth[tileX] + tbX - colBd[tileX];

                CtbAddrTsToRs[CtbAddrRsToTs[ctbAddrRs]] = ctbAddrRs;
            }

            for (uint j = 0, tileIdx = 0; j <= num_tile_rows_minus1; j++)
            {
                for (uint i = 0; i <= num_tile_columns_minus1; i++, tileIdx++)
                {
                    for (uint y = rowBd[j]; y < rowBd[j + 1]; y++)
                    {
                        for (uint x = colBd[i]; x < colBd[i + 1]; x++)
                        {
                            TileId[CtbAddrRsToTs[y * sps.PicWidthInCtbsY + x]] = tileIdx;
                            TileIdFromRs[y * sps.PicWidthInCtbsY + x] = tileIdx;
                        }
                    }
                }
            }


            ColumnWidthInLumaSamples = new uint[num_tile_columns_minus1 + 1];
            RowHeightInLumaSamples = new uint[num_tile_rows_minus1 + 1];

            for (int i = 0; i <= num_tile_columns_minus1; i++)
                ColumnWidthInLumaSamples[i] = colWidth[i] << sps.CtbLog2SizeY;

            for (int i = 0; i <= num_tile_columns_minus1; i++)
                RowHeightInLumaSamples[i] = rowHeight[i] << sps.CtbLog2SizeY;
        }

        // 6.5.2 Z-scan order array initialization process
        internal void zscan_init()
        {
            uint tbX, tbY, ctbAddrRs, p, m;
            uint xMax = sps.PicWidthInCtbsY << (sps.CtbLog2SizeY - sps.MinTbLog2SizeY);
            uint yMax = sps.PicHeightInCtbsY << (sps.CtbLog2SizeY - sps.MinTbLog2SizeY);
            MinTbAddrZs = new uint[xMax, yMax];

            for (uint y = 0; y < yMax; y++)
            {
                for (uint x = 0; x < xMax; x++)
                {
                    tbX = (x << sps.MinTbLog2SizeY) >> sps.CtbLog2SizeY;
                    tbY = (y << sps.MinTbLog2SizeY) >> sps.CtbLog2SizeY;
                    ctbAddrRs = sps.PicWidthInCtbsY * tbY + tbX;
                    MinTbAddrZs[x, y] = CtbAddrRsToTs[ctbAddrRs] << ((sps.CtbLog2SizeY - sps.MinTbLog2SizeY) * 2);

                    p = 0;
                    for (byte i = 0; i < (sps.CtbLog2SizeY - sps.MinTbLog2SizeY); i++)
                    {
                        m = (uint)(1 << i);
                        p += (m & x) != 0 ? m * m : 0;
                        p += (m & y) != 0 ? 2 * m * m : 0;
                    }
                    MinTbAddrZs[x, y] += p;
                }
            }
        }
    }

    internal class pps_3d_extension
    {
        public pps_3d_extension(BitStreamWithNalSupport stream)
        {
            throw new NotImplementedException();
        }
    }

    internal class pps_multilayer_extension
    {
        public pps_multilayer_extension(BitStreamWithNalSupport stream)
        {
            throw new NotImplementedException();
        }
    }
}

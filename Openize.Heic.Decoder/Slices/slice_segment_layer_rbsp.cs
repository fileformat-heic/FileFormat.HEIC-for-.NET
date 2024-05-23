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
    internal class slice_segment_layer_rbsp : NalUnit
    {

        public slice_segment_header slice_header;
        public slice_segment_data data;

        public slice_segment_layer_rbsp(BitStreamWithNalSupport stream, ulong startPosition, int size) : base (stream, startPosition, size)
        {
            slice_header = new slice_segment_header(stream, this);

            stream.Cabac.Initialization(slice_header);

            if (slice_header.first_slice_segment_in_pic_flag)
            {
                var picture = new HeicPicture(slice_header, NalHeader);
                stream.Context.Pictures.Add(stream.CurrentImageId, picture);
                slice_header.parentPicture = picture;

                if (NalHeader.IsIrapPicture)
                {
                    if (NalHeader.IsIdrPicture || 
                        NalHeader.IsBlaPicture ||
                        //first_decoded_picture ||
                        stream.Context.FirstAfterEndOfSequenceNAL)
                    {
                        stream.Context.NoRaslOutputFlag = true;
                        stream.Context.FirstAfterEndOfSequenceNAL = false;
                    }
                    else
                    {
                        stream.Context.NoRaslOutputFlag = false;
                        stream.Context.HandleCraAsBlaFlag = false;
                    }
                }
            }

            if (stream.Context.Pictures.Count == 0)
                throw new IndexOutOfRangeException();

            var currentPicture = stream.Context.Pictures[stream.CurrentImageId];

            if (slice_header.parentPicture == null)
                slice_header.parentPicture = currentPicture;

            slice_header.slice_index = currentPicture.SliceHeaders.Count;
            currentPicture.SliceHeaders.Add(slice_header.slice_index, slice_header);
            Scaling.Initiate(currentPicture.sps);

            stream.Context.DecodingPictureOrderCount(currentPicture);

            data = new slice_segment_data(stream, slice_header);

            return;
            //new rbsp_slice_segment_trailing_bits(stream, EndPosition);
        }


        internal int CuQpOffsetCb = 0;
        internal int CuQpOffsetCr = 0;
    }
}

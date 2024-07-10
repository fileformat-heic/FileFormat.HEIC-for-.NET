/*
 * FileFormat.HEIC 
 * Copyright (c) 2024 Openize Pty Ltd. 
 *
 * This file is part of FileFormat.HEIC.
 *
 * FileFormat.HEIC is available under Openize license, which is
 * available along with FileFormat.HEIC sources.
 */

using System.Collections.ObjectModel;
using FileFormat.IsoBmff;
using FileFormat.Heic.Decoder.IO;

namespace FileFormat.Heic.Decoder
{
    internal class HEVCConfigurationBox : Box
    {
        public HEVCDecoderConfigurationRecord record;
        public ulong offset;

        public new string ToString() => $"{type}";

        public ObservableCollection<HEVCDecoderConfigurationRecord> Children { get; set; }

        public HEVCConfigurationBox(BitStreamWithNalSupport stream, ulong size) : base(BoxType.hvcC, size)
        {
            offset = stream.GetBitPosition() / 8;
            Children = new ObservableCollection<HEVCDecoderConfigurationRecord>();
        }
    }
}

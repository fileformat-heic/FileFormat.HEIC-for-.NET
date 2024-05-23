/*
 * Openize.HEIC 
 * Copyright (c) 2024 Openize Pty Ltd. 
 *
 * This file is part of Openize.HEIC.
 *
 * Openize.HEIC is available under Openize license, which is
 * available along with Openize.HEIC sources.
 */

using System.Collections.ObjectModel;
using Openize.IsoBmff;
using Openize.Heic.Decoder.IO;

namespace Openize.Heic.Decoder
{
    internal class HEVCConfigurationBox : Box
    {
        public HEVCDecoderConfigurationRecord record;
        public ulong offset;

        public string ToString() => $"{type}";
#if DEBUG
        public string AsString => ToString();
#endif

        public ObservableCollection<HEVCDecoderConfigurationRecord> Children { get; set; }

        public HEVCConfigurationBox(BitStreamWithNalSupport stream, ulong size) : base(BoxType.hvcC, size)
        {
            offset = stream.GetBitPosition() / 8;
            Children = new ObservableCollection<HEVCDecoderConfigurationRecord>();
        }
    }
}

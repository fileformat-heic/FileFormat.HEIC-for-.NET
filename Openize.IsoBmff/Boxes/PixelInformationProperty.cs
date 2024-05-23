/*
 * Openize.IsoBmff
 * Copyright (c) 2024 Openize Pty Ltd. 
 *
 * This file is part of Openize.IsoBmff.
 *
 * Openize.IsoBmff is available under MIT license, which is
 * available along with Openize.IsoBmff sources.
 */

using Openize.IsoBmff.IO;
using System;

namespace Openize.IsoBmff
{
    /// <summary>
    /// The PixelInformationProperty descriptive item property indicates the number and bit depth of
    /// colour components in the reconstructed image of the associated image item.
    /// </summary>
    public class PixelInformationProperty : FullBox
    {
        /// <summary>
        /// This field signals the number of channels by each pixel of the reconstructed image ofthe associated image item.
        /// </summary>
        public byte num_channels;

        /// <summary>
        /// This field indicates the bits per channel for the pixels of the reconstructed image of the associated image item.
        /// </summary>
        public byte[] bits_per_channel;

        /// <summary>
        /// Text summary of the box.
        /// </summary>
        public new string ToString =>
            $"{type} ch: {num_channels} bits: {String.Join(" ", bits_per_channel)}";

        /// <summary>
        /// Create the box object from the bitstream and box size.
        /// </summary>
        /// <param name="stream">File stream.</param>
        /// <param name="size">Box size in bytes.</param>
        public PixelInformationProperty(BitStreamReader stream, ulong size) : base(stream, BoxType.pixi, size)
        {
            num_channels = (byte)stream.Read(8);
            bits_per_channel = new byte[num_channels];
            for (int i = 0; i < num_channels; i++)
                bits_per_channel[i] = (byte)stream.Read(8);
        }
    }
}

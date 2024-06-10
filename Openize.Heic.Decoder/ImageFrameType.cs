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
    /// <summary>
    /// Type of an image frame.
    /// </summary>
    public enum ImageFrameType : uint
    {
        /// <summary>
        /// HEVC coded image frame.
        /// </summary>
        hvc1 = 0x68766331,

        /// <summary>
        /// Identity transformation.
        /// Cropping and/or rotation by 90, 180, or 270 degrees, imposed through the respective transformative properties.
        /// </summary>
        iden = 0x6964656e,

        /// <summary>
        /// Image Overlay.
        /// Overlaying any number of input images in indicated order and locations onto the canvas of the output image.
        /// </summary>
        iovl = 0x696f766c,

        /// <summary>
        /// Image Grid.
        /// Reconstructing a grid of input images of the same width and height.
        /// </summary>
        grid = 0x67726964,

        /// <summary>
        /// Exif metadata.
        /// Exchangeable image file format metadata.
        /// </summary>
        Exif = 0x45786966,

        /// <summary>
        /// MIME metadata.
        /// Resource Description Framework metadata.
        /// </summary>
        mime = 0x6d696d65
    }
}

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

namespace Openize.IsoBmff
{
    /// <summary>
    /// If the decoding of a multi-layer image item results into more than one reconstructed image, the 'lsel'
    /// item property shall be associated with the image item.Otherwise, the 'lsel' item property shall not
    /// be associated with an image item.
    /// <para>This property is used to select which of the reconstructed images is described by subsequent
    /// descriptive item properties in the item property association order and manipulated by transformative
    /// item properties, if any, to generate an output image of the image item.</para>
    /// </summary>
    public class LayerSelectorProperty : Box
    {
        /// <summary>
        /// Specifies the layer identifier of the image among the reconstructed images that is described
        /// by subsequent descriptive item properties in the item property association order and manipulated by
        /// transformative item properties, if any, to generate an output image of the image item.The semantics of
        /// layer_id are specific to the coding format and are therefore defined for each coding format for which
        /// the decoding of a multi-layer image item can result into more than one reconstructed images.
        /// </summary>
        public ushort layer_id;

        /// <summary>
        /// Text summary of the box.
        /// </summary>
        public new string ToString => $"{type} layer: {layer_id}";

        /// <summary>
        /// Create the box object from the bitstream and box size.
        /// </summary>
        /// <param name="stream">File stream.</param>
        /// <param name="size">Box size in bytes.</param>
        public LayerSelectorProperty(BitStreamReader stream, ulong size) : base(BoxType.lsel, size)
        {
            layer_id = (ushort)stream.Read(16);
        }
    }
}

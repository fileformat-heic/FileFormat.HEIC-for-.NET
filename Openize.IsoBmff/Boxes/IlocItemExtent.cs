/*
 * Openize.IsoBmff
 * Copyright (c) 2024 Openize Pty Ltd. 
 *
 * This file is part of Openize.IsoBmff.
 *
 * Openize.IsoBmff is available under MIT license, which is
 * available along with Openize.IsoBmff sources.
 */

namespace Openize.IsoBmff
{
    /// <summary>
    /// Data class for organised storage on location data extents.
    /// </summary>
    public class IlocItemExtent
    {
        /// <summary>
        /// An index as defined for the constructionmethod.
        /// </summary>
        public uint index;

        /// <summary>
        /// The absolute offset, in bytes from the data origin of the container, of this extent data.
        /// If offset_size is 0, extent_offset takes the value 0.
        /// </summary>
        public uint offset;

        /// <summary>
        /// The absolute length in bytes of this metadata item extent.
        /// If length_size is 0, extent_length takes the value 0.
        /// If the value is 0, then length of the extent is the length of the entire referenced container.
        /// </summary>
        public uint length;

        /// <summary>
        /// Text summary of the box.
        /// </summary>
        public new string ToString => $"extent:: index: {index} offset: {offset} length: {length}";

    }
}

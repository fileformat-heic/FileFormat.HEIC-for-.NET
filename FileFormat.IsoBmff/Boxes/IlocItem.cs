/*
 * FileFormat.IsoBmff
 * Copyright (c) 2024 Openize Pty Ltd. 
 *
 * This file is part of FileFormat.IsoBmff.
 *
 * FileFormat.IsoBmff is available under MIT license, which is
 * available along with FileFormat.IsoBmff sources.
 */

using System;

namespace FileFormat.IsoBmff
{
    /// <summary>
    /// Data class for organised storage on location data.
    /// </summary>
    public class IlocItem
    {
        /// <summary>
        /// An arbitrary integer 'name' for this resource which can be used to refer to it.
        /// </summary>
        public uint item_ID;

        /// <summary>
        /// Indicates the location of data:
        /// 0 means data are located in the current file in mdat box;
        /// 1 means data are located in the current file in idat box;
        /// 2 means data are located in the external file.
        /// </summary>
        public byte construction_method;

        /// <summary>
        /// Contains either zero (‘this file’) or a 1‐based index into the data references in the data information box.
        /// </summary>
        public uint data_reference_index;

        /// <summary>
        /// A base value for offset calculations within the referenced data.
        /// If base_offset_size equals 0, base_offset takes the value 0, i.e. it is unused.
        /// </summary>
        public uint base_offset;

        /// <summary>
        /// Provides the count of the number of extents into which the resource is fragmented;
        /// it must have the value 1 or greater.
        /// </summary>
        public uint extent_count;

        /// <summary>
        /// Array of extent data.
        /// </summary>
        public IlocItemExtent[] extents;

        /// <summary>
        /// Text summary of the box.
        /// </summary>
        public new string ToString
        {
            get
            {
                return $"item id: {item_ID} method: {construction_method} offset : {base_offset}\n" +
                $"{String.Join("\n", Array.ConvertAll(extents, i => i.ToString))}";
            }
        }

    }
}

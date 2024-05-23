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
    /// Collects all references for one item of a specific type.
    /// </summary>
    public class SingleItemTypeReferenceBox : Box
    {
        /// <summary>
        /// The ID of the item that refers to other items.
        /// </summary>
        public uint from_item_ID;

        /// <summary>
        /// The number of references.
        /// </summary>
        public uint reference_count;

        /// <summary>
        /// The array of the IDs of the item referred to.
        /// </summary>
        public uint[] to_item_ID;

        /// <summary>
        /// Text summary of the box.
        /// </summary>
        public new string ToString
        {
            get{
                var str = "";
                foreach (var id in to_item_ID)
                    str += id + " ";

                return $"{type} from_item_ID: {from_item_ID} refs: {String.Join(" ", to_item_ID)}";
            }
        }

        /// <summary>
        /// Create the box object from the bitstream and item id size.
        /// </summary>
        /// <param name="stream">File stream.</param>
        /// <param name="id_size">Item id size in bytes.</param>
        public SingleItemTypeReferenceBox(BitStreamReader stream, int id_size) : base(stream)
        {
            from_item_ID = (uint)stream.Read(id_size);
            reference_count = (uint)stream.Read(16);

            to_item_ID = new uint[reference_count];

            for (int j = 0; j < reference_count; j++)
            {
                to_item_ID[j] = (uint)stream.Read(id_size);
            }
        }
    }
}

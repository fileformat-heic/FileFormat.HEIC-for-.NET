/*
 * FileFormat.IsoBmff
 * Copyright (c) 2024 Openize Pty Ltd. 
 *
 * This file is part of FileFormat.IsoBmff.
 *
 * FileFormat.IsoBmff is available under MIT license, which is
 * available along with FileFormat.IsoBmff sources.
 */

using FileFormat.IsoBmff.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FileFormat.IsoBmff
{
    /// <summary>
    /// Contains all the linking of one item to others via typed references.
    /// All references for one item of a specific type are collected into a single item type reference box.
    /// </summary>
    public class ItemReferenceBox : FullBox
    {
        /// <summary>
        /// List of references.
        /// </summary>
        public List<SingleItemTypeReferenceBox> references;

        /// <summary>
        /// Text summary of the box.
        /// </summary>
        public new string ToString => $"{type}";

        /// <summary>
        /// Observable collection of the references.
        /// </summary>
        public ObservableCollection<SingleItemTypeReferenceBox> Children { get; set; }

        /// <summary>
        /// Create the box object from the bitstream, box size and start position.
        /// </summary>
        /// <param name="stream">File stream.</param>
        /// <param name="size">Box size in bytes.</param>
        /// <param name="startPos">Start position in bits.</param>
        public ItemReferenceBox(BitStreamReader stream, ulong size, ulong startPos) : base(stream, BoxType.iref, size)
        {
            references = new List<SingleItemTypeReferenceBox>();

            while (stream.GetBitPosition() < startPos + size * 8)
            {
                var box = new SingleItemTypeReferenceBox(stream, version == 0 ? 16 : 32);
                references.Add(box);
            }

            Children = new ObservableCollection<SingleItemTypeReferenceBox>(references);
        }
    }
}

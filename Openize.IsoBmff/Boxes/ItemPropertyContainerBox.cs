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
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Openize.IsoBmff
{
    /// <summary>
    /// Contains an implicitly indexed list of item properties.
    /// </summary>
    public class ItemPropertyContainerBox : Box
    {
        /// <summary>
        /// Dictionary of properties.
        /// </summary>
        public Dictionary<int, Box> items;

        /// <summary>
        /// Text summary of the box.
        /// </summary>
        public new string ToString => $"{type}";

        /// <summary>
        /// Observable collection of the nested boxes.
        /// </summary>
        public ObservableCollection<Box> Children { get; set; }

        /// <summary>
        /// Create the box object from the bitstream and start position.
        /// </summary>
        /// <param name="stream">File stream.</param>
        /// <param name="startPos">Start position in bits.</param>
        public ItemPropertyContainerBox(BitStreamReader stream, ulong startPos) : base(stream, BoxType.ipco)
        {
            items = new Dictionary<int, Box>();
            int i = 1;
            while (stream.GetBitPosition() < startPos + size * 8)
            {
                items.Add(i, Box.ParceBox(stream));
                i++;
            }

            Children= new ObservableCollection<Box>(items.Values);
        }

        /// <summary>
        /// Returns property by index.
        /// </summary>
        /// <param name="id">Property index.</param>
        /// <returns>Box with property data.</returns>
        public Box GetPropertyByIndex(int id) => items[id];
    }
}

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
    /// The metadata for a presentation is stored in the single Movie Box which occurs at the top‐level of a file. 
    /// </summary>
    public class MovieBox : Box
    {
        /// <summary>
        /// List of nested boxes.
        /// </summary>
        private List<Box> boxes;

        /// <summary>
        /// Text summary of the box.
        /// </summary>
        public new string ToString => $"{type}";

        /// <summary>
        /// Observable collection of the nested boxes.
        /// </summary>
        public ObservableCollection<Box> Children { get; set; }

        /// <summary>
        /// Create the box object from the bitstream, box size and start position.
        /// </summary>
        /// <param name="stream">File stream.</param>
        /// <param name="size">Box size in bytes.</param>
        /// <param name="startPos">Start position in bits.</param>
        public MovieBox(BitStreamReader stream, ulong size, ulong startPos) : base(BoxType.moov, size)
        {
            boxes = new List<Box>();

            while (stream.GetBitPosition() < startPos + size * 8)
            {
                Box box = Box.ParceBox(stream);
                boxes.Add(box);
            }

            Children = new ObservableCollection<Box>(boxes);
        }
    }
}

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
    /// A common base structure that contains general metadata.
    /// </summary>
    public class MetaBox : FullBox
    {
        /// <summary>
        /// Dictionary of nested boxes.
        /// </summary>
        private Dictionary<BoxType, Box> boxes;

        /// <summary>
        /// Handler box.
        /// </summary>
        public HandlerBox hdlr => TryGetBox(BoxType.hdlr) as HandlerBox;

        /// <summary>
        /// Primary item box.
        /// </summary>
        public PrimaryItemBox pitm => TryGetBox(BoxType.pitm) as PrimaryItemBox;

        /// <summary>
        /// Item location box.
        /// </summary>
        public ItemLocationBox iloc => TryGetBox(BoxType.iloc) as ItemLocationBox;

        /// <summary>
        /// Item info box.
        /// </summary>
        public ItemInfoBox iinf => TryGetBox(BoxType.iinf) as ItemInfoBox;

        /// <summary>
        /// Item properties box.
        /// </summary>
        public ItemPropertiesBox iprp => TryGetBox(BoxType.iprp) as ItemPropertiesBox;

        /// <summary>
        /// Item reference box.
        /// </summary>
        public ItemReferenceBox iref => TryGetBox(BoxType.iref) as ItemReferenceBox;

        /// <summary>
        /// Item data box.
        /// </summary>
        public ItemDataBox idat => TryGetBox(BoxType.idat) as ItemDataBox;

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
        public MetaBox(BitStreamReader stream, ulong size, ulong startPos) : base (stream, BoxType.meta, size)
        {
            boxes = new Dictionary<BoxType, Box>();

            boxes.Add(BoxType.hdlr, new HandlerBox(stream));

            while(stream.GetBitPosition() < startPos + size * 8)
            {
                Box box = Box.ParceBox(stream);
                boxes.Add(box.type, box);
            }

            Children = new ObservableCollection<Box>(boxes.Values);
        }

        /// <summary>
        /// Try to get specified box. Return null if required box not available.
        /// </summary>
        /// <param name="type">Box type.</param>
        /// <returns>Box.</returns>
        private Box TryGetBox(BoxType type)
        {
            return boxes.ContainsKey(type) ? boxes[type] : null;
        }
    }
}

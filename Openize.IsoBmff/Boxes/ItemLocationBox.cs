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
using System.Collections.ObjectModel;

namespace Openize.IsoBmff
{
    /// <summary>
    /// The item location box provides a directory of resources in this or other files, by locating their container, 
    /// their offset within that container, and their length.
    /// </summary>
    public class ItemLocationBox : FullBox
    {
        /// <summary>
        /// Indicates the length in bytes of the offset field.
        /// </summary>
        public byte offset_size;

        /// <summary>
        /// Indicates the length in bytes of the length field.
        /// </summary>
        public byte length_size;

        /// <summary>
        /// Indicates the length in bytes of the base_offset field.
        /// </summary>
        public byte base_offset_size;

        /// <summary>
        /// Indicates the length in bytes of the extent.index field.
        /// </summary>
        public byte index_size;

        /// <summary>
        /// Counts the number of items in the location item array. 
        /// </summary>
        public uint item_count;

        /// <summary>
        /// Array of the location items.
        /// </summary>
        public IlocItem[] items;

        /// <summary>
        /// Text summary of the box.
        /// </summary>
        public new string ToString => $"{type} count: {item_count}";

        /// <summary>
        /// Observable collection of the location items.
        /// </summary>
        public ObservableCollection<IlocItem> Children { get; set; }

        /// <summary>
        /// Create the box object from the bitstream and box size.
        /// </summary>
        /// <param name="stream">File stream.</param>
        /// <param name="size">Box size in bytes.</param>
        public ItemLocationBox (BitStreamReader stream, ulong size) : base (stream, BoxType.iloc, size)
        {
            offset_size = (byte)stream.Read(4);
            length_size = (byte)stream.Read(4);
            base_offset_size = (byte)stream.Read(4);

            if ((version == 1) || (version == 2))
                index_size = (byte)stream.Read(4);
            else
                stream.SkipBits(4);

            if (version < 2)
                item_count = (uint)stream.Read(16);
            else if (version == 2)
                item_count = (uint)stream.Read(32);

            items = new IlocItem[item_count];

            for (int i = 0; i < item_count; i++)
            {
                items[i] = new IlocItem ();
                if (version < 2)
                    items[i].item_ID = (uint)stream.Read(16);
                else if (version == 2)
                    items[i].item_ID = (uint)stream.Read(32);

                if ((version == 1) || (version == 2))
                {
                    stream.SkipBits(12); //0x000
                    items[i].construction_method = (byte)stream.Read(4);
                }

                items[i].data_reference_index = (uint)stream.Read(16);
                items[i].base_offset = (uint)stream.Read(base_offset_size * 8);
                items[i].extent_count = (uint)stream.Read(16);
                items[i].extents = new IlocItemExtent[items[i].extent_count];

                for (int j = 0; j < items[i].extent_count; j++)
                {
                    items[i].extents[j] = new IlocItemExtent ();

                    if (((version == 1) || (version == 2)) && (index_size > 0))
                        items[i].extents[j].index = (uint)stream.Read(index_size * 8);

                    items[i].extents[j].offset = (uint)stream.Read(offset_size * 8);
                    items[i].extents[j].length = (uint)stream.Read(length_size * 8);
                }
            }

            Children = new ObservableCollection<IlocItem>(items);
        }
    }
}

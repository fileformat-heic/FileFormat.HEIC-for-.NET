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
    /// The item information box provides extra information about file entries.
    /// </summary>
    public class ItemInfoBox : FullBox
    {
        /// <summary>
        /// A count of the number of entries in the info entry array.
        /// </summary>
        public uint entry_count;

        /// <summary>
        /// Array of entries of extra information, each entry is formatted as a box.
        /// This array is sorted by increasing item_ID in the entry records.
        /// </summary>
        public ItemInfoEntry[] item_infos;

        /// <summary>
        /// Text summary of the box.
        /// </summary>
        public new string ToString => $"{type} count: {entry_count}";

        /// <summary>
        /// Observable collection of entries of extra information.
        /// </summary>
        public ObservableCollection<ItemInfoEntry> Children { get; set; }

        /// <summary>
        /// Create the box object from the bitstream and box size.
        /// </summary>
        /// <param name="stream">File stream.</param>
        /// <param name="size">Box size in bytes.</param>
        public ItemInfoBox(BitStreamReader stream, ulong size) : base(stream, BoxType.iinf, size)
        {
            entry_count = (uint)stream.Read(version == 0 ? 16 : 32);

            item_infos = new ItemInfoEntry[entry_count];
            for (int i = 0; i < entry_count; i++)
                item_infos[i] = new ItemInfoEntry(stream);

            Children = new ObservableCollection<ItemInfoEntry>(item_infos);
        }
    }
}

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
using System.Collections.ObjectModel;

namespace FileFormat.IsoBmff
{
    /// <summary>
    /// Associates items with item properties.
    /// </summary>
    public class ItemPropertyAssociation : FullBox
    {
        /// <summary>
        /// Count of entries.
        /// </summary>
        public uint entry_count;

        /// <summary>
        /// Property entrie array.
        /// </summary>
        public ItemPropertyEntry[] entries;

        /// <summary>
        /// Text summary of the box.
        /// </summary>
        public new string ToString => $"{type} count: {entry_count}";

        /// <summary>
        /// Observable collection of the property entries.
        /// </summary>
        public ObservableCollection<ItemPropertyEntry> Children { get; set; }

        /// <summary>
        /// Create the box object from the bitstream.
        /// </summary>
        /// <param name="stream">File stream.</param>
        public ItemPropertyAssociation(BitStreamReader stream) : base(stream, BoxType.ipma)
        {
            entry_count = (uint)stream.Read(32);
            entries = new ItemPropertyEntry[entry_count];

            for (int i = 0; i < entry_count; i++)
            {
                entries[i] = new ItemPropertyEntry();

                entries[i].item_ID = (uint)stream.Read(version < 1 ? 16 : 32);
                entries[i].association_count = (byte)stream.Read(8);

                entries[i].associations = new ItemPropertyEntryAssociation[entries[i].association_count];
                for (int j = 0; j < entries[i].association_count; j++)
                {
                    entries[i].associations[j] = new ItemPropertyEntryAssociation();
                    entries[i].associations[j].essential = stream.ReadFlag();
                    entries[i].associations[j].property_index = (ushort)stream.Read(flags[0] ? 15 : 7);
                }
            }

            Children = new ObservableCollection<ItemPropertyEntry>(entries);
        }

        /// <summary>
        /// Item property entry data.
        /// </summary>
        public class ItemPropertyEntry
        {
            /// <summary>
            /// Identifies the item with which property is associated.
            /// </summary>
            public uint item_ID;

            /// <summary>
            /// Count of associations.
            /// </summary>
            public byte association_count;

            /// <summary>
            /// Array of associations.
            /// </summary>
            public ItemPropertyEntryAssociation[] associations;
            
            /// <summary>
            /// Text summary of the box.
            /// </summary>
            public new string ToString
            {
                get
                {
                    var str = "";
                    foreach (var item in associations)
                        str += item.ToString + ", ";
                    return $"entry:: id: {item_ID} count: {association_count} data: {str.Remove(str.Length - 2)}";
                }
            }
        }

        /// <summary>
        /// Item property association entry data.
        /// </summary>
        public class ItemPropertyEntryAssociation
        {
            /// <summary>
            /// Set to 1 indicates that the associated property is essential to the item, otherwise it is non-essential.
            /// </summary>
            public bool essential;

            /// <summary>
            /// Is either 0 indicating that no property is associated (the essential indicator shall also be 0),
            /// or is the 1-based index of the associated property box in the ItemPropertyContainerBox
            /// contained in the same ItemPropertiesBox.
            /// </summary>
            public ushort property_index;

            /// <summary>
            /// Text summary of the box.
            /// </summary>
            public new string ToString => $"{(essential ? 1 : 0)} {property_index}";
        }

    }
}

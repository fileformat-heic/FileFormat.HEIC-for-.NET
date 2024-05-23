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
    /// The data reference object contains a table of data references (normally URLs) that declare the 
    /// location(s) of the media data used within the presentation.
    /// </summary>
    public class DataReferenceBox : FullBox
    {
        /// <summary>
        /// The count of data references.
        /// </summary>
        public uint entry_count;

        /// <summary>
        /// The list of data references.
        /// </summary>
        public List<DataEntryUrlBox> entries;

        /// <summary>
        /// Text summary of the box.
        /// </summary>
        public new string ToString => $"{type} count: {entry_count}";

        /// <summary>
        /// Observable collection of the nested boxes.
        /// </summary>
        public ObservableCollection<Box> Children { get; set; }

        /// <summary>
        /// Create the box object from the bitstream.
        /// </summary>
        /// <param name="stream">File stream.</param>
        public DataReferenceBox(BitStreamReader stream) : base (stream, BoxType.dref)
        {
            entry_count = (uint)stream.Read(32);
            entries = new List<DataEntryUrlBox>();
            for (int i = 1; i <= entry_count; i++)
            {
                entries.Add(new DataEntryUrlBox(stream));
            }
            
            Children = new ObservableCollection<Box>(entries);
        }
    }
}
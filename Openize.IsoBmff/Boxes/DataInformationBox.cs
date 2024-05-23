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
    /// The data information box contains objects that declare the location of the media information in a track.
    /// </summary>
    public class DataInformationBox : Box
    {
        /// <summary>
        /// The data reference object contains a table of data references (normally URLs) that declare the 
        /// location(s) of the media data used within the presentation.
        /// </summary>
        DataReferenceBox dref;

        /// <summary>
        /// Text summary of the box.
        /// </summary>
        public new string ToString => $"{type}";

        /// <summary>
        /// Observable collection of the nested boxes.
        /// </summary>
        public ObservableCollection<Box> Children { get; set; }

        /// <summary>
        /// Create the box object from the bitstream and box size.
        /// </summary>
        /// <param name="stream">File stream.</param>
        /// <param name="size">Box size in bytes.</param>
        public DataInformationBox(BitStreamReader stream, ulong size ) : base (BoxType.dinf, size)
        {
            dref = new DataReferenceBox(stream);

            Children = new ObservableCollection<Box>
            {
                dref
            };
        }
    }
}
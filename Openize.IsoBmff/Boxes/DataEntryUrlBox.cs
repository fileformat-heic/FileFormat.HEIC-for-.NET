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

namespace Openize.IsoBmff
{
    /// <summary>
    /// Data reference URL that declare the location of the media data used within the presentation.
    /// The data reference index in the sample description ties entries in this table to the samples in the track.
    /// A track may be split over several sources in this way.
    /// </summary>
    public class DataEntryUrlBox : FullBox
    {
        /// <summary>
        /// Name of the entry.
        /// </summary>
        public string name;

        /// <summary>
        /// Location of the entry.
        /// </summary>
        public string location;

        /// <summary>
        /// Text summary of the box.
        /// </summary>
        public new string ToString => $"{type} {location} {name}";

        /// <summary>
        /// Create the box object from the bitstream.
        /// </summary>
        /// <param name="stream">File stream.</param>
        public DataEntryUrlBox(BitStreamReader stream) : base(stream)
        {
            if (flags[0])
            {
                if(type == BoxType.urn)
                    name = stream.ReadString();

                location = stream.ReadString();
            }
        }
    }
}
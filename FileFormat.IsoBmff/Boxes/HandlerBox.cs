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

namespace FileFormat.IsoBmff
{
    /// <summary>
    /// This box declares media type of the track, and thus the process by which the media‐data in the track is presented.
    /// </summary>
    public class HandlerBox : FullBox
    {
        /// <summary>
        /// When present in a media box, contains a value as defined in clause 12, or a value from a derived specification, or registration.
        /// When present in a meta box, contains an appropriate value to indicate the format of the meta box contents.
        /// The value 'null' can be used in the primary meta box to indicate that it is merely being used to hold resources. 
        /// </summary>
        public uint handler_type;

        /// <summary>
        /// A null‐terminated string in UTF‐8 characters which gives a human‐readable name for the track type (for debugging and inspection purposes).
        /// </summary>
        public string name;

        /// <summary>
        /// Text summary of the box.
        /// </summary>
        public new string ToString => $"{type}\ndata: {UintToString(handler_type)}";

        /// <summary>
        /// Create the box object from the bitstream.
        /// </summary>
        /// <param name="stream">File stream.</param>
        public HandlerBox(BitStreamReader stream) : base (stream, BoxType.hdlr)
        {
            var pos = stream.GetBitPosition();        
            stream.Read(32);

            handler_type = (uint)stream.Read(32);

            for(int i = 0; i< 3 ; i++)
                stream.Read(32);

            name = stream.ReadString();
            while (size - 12 > (stream.GetBitPosition() - pos) / 8)
                stream.Read(8);
        }
    }
}

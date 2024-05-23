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
using System;

namespace Openize.IsoBmff
{
    /// <summary>
    /// This box provides extra information about one entry.
    /// </summary>
    public class ItemInfoEntry : FullBox
    {
        /// <summary>
        /// Contains either 0 for the primary resource (e.g., the XML contained in an 'xml ' box) or 
        /// the ID of the item for which the following information is defined.
        /// </summary>
        public uint item_ID;

        /// <summary>
        /// Contains either 0 for an unprotected item, or the one‐based index 
        /// into the item protection  box defining the protection applied to this item(the first box in the item
        /// protection box has the index 1).
        /// </summary>
        public ushort item_protection_index;

        /// <summary>
        /// A 32‐bit value, typically 4 printable characters, that is a defined valid item type indicator, such as 'mime'. 
        /// </summary>
        public uint item_type;

        /// <summary>
        /// A null‐terminated string in UTF‐8 characters containing a symbolic name of the item (source file for file delivery transmissions). 
        /// </summary>
        public string item_name;

        /// <summary>
        /// A null‐terminated string in UTF‐8 characters with the MIME type of the item.
        /// If the item is content encoded (see below), then the content type refers to the item after content decoding.
        /// </summary>
        public string content_type;

        /// <summary>
        /// A string that is an absolute URI, that is used as a type indicator.
        /// </summary>
        public string item_uri_type;

        /// <summary>
        /// An optional null‐terminated string in UTF‐8 characters used to indicate 
        /// that the binary file is encoded and needs to be decoded before interpreted.
        /// The values are as defined for Content‐Encoding for HTTP/1.1.
        /// </summary>
        public string content_encoding;

        /// <summary>
        /// Text summary of the box.
        /// </summary>
        public new string ToString => $"{type} id: {item_ID} type: {UintToString(item_type)} name: {item_name}{content_type}";

        /// <summary>
        /// Create the box object from the bitstream.
        /// </summary>
        /// <param name="stream">File stream.</param>
        internal ItemInfoEntry(BitStreamReader stream) : base(stream, BoxType.infe)
        {
            if (version < 2)
            {
                item_ID = (uint)stream.Read(16);
                item_protection_index = (ushort)stream.Read(16);

                item_name = stream.ReadString();
                content_type = stream.ReadString();
                content_encoding = stream.ReadString(); //optional

                if (version == 1)
                {
                    uint extension_type = (uint)stream.Read(32); //optional
                    //ItemInfoExtension(extension_type); //optional
                    throw new NotImplementedException();
                }
            }
            else
            {
                item_ID = (uint)stream.Read(version == 2 ? 16 : 32);
                item_protection_index = (ushort)stream.Read(16);
                item_type = (uint)stream.Read(32);

                item_name = stream.ReadString();

                if (item_type == 0x6d696d65 /*mime*/ ) {
                    content_type = stream.ReadString();
                    //content_encoding = stream.ReadString(); //optional
                } else if (item_type == 0x75726920 /*uri */) {
                    item_uri_type = stream.ReadString();
                }
            }

        }
    }
}

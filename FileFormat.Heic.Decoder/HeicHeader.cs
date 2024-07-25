/*
 * FileFormat.HEIC 
 * Copyright (c) 2024 Openize Pty Ltd. 
 *
 * This file is part of FileFormat.HEIC.
 *
 * FileFormat.HEIC is available under Openize license, which is
 * available along with FileFormat.HEIC sources.
 */

using FileFormat.IsoBmff;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FileFormat.Heic.Decoder
{
    /// <summary>
    /// Heic image header class. Grants convinient access to IsoBmff container meta data.
    /// </summary>
    public class HeicHeader
    {
        /// <summary>
        /// Meta data IsoBmff box.
        /// </summary>
        public MetaBox Meta {  get; private set; }

        /// <summary>
        /// The identificator of the default frame.
        /// </summary>
        public uint DefaultFrameId => Meta.pitm?.item_ID ?? 0;

        /// <summary>
        /// Initializes a new instance of the heic image header.
        /// </summary>
        /// <param name="meta"><see cref="MetaBox"/> data.</param>
        internal HeicHeader(MetaBox meta)
        {
            Meta = meta;
        }

        /// <summary>
        /// Returns properties grouped by frames.
        /// </summary>
        /// <returns>Dictionary filled with lists of properties that can be accessed by frame id.</returns>
        internal Dictionary<uint, List<Box>> GetProperties()
        {
            return Meta.iprp.GetProperties();
        }

        /// <summary>
        /// Returns frame type and name.
        /// </summary>
        /// <param name="id">Identificator of the frame.</param>
        /// <returns><see cref="ItemInfoEntry"/> that contains type information.</returns>
        internal ItemInfoEntry GetInfoBoxById(uint id)
        {
            return Meta.iinf.item_infos.First(i => i.item_ID == id);
        }

        /// <summary>
        /// Returns frame location.
        /// </summary>
        /// <param name="id">Identificator of the frame.</param>
        /// <returns><see cref="IlocItem"/> that contains location information.</returns>
        internal IlocItem GetLocationBoxById(uint id)
        {
            return Meta.iloc.items.First(i => i.item_ID == id);
        }

        /// <summary>
        /// Returns content from idat (item data) box by offset and length.
        /// </summary>
        /// <param name="offset">The offset from the start on the idat box.</param>
        /// <param name="length">The length of the data.</param>
        /// <returns>Byte array.</returns>
        internal byte[] GetItemDataBoxContent(uint offset, uint length)
        {
            if (Meta.idat == null)
                return new byte[0];

            var data = new byte[length];
            Array.Copy(Meta.idat.data, offset, data, 0, length);
            return data;
        }

        /// <summary>
        /// Returns the list of the frames that are used in calculation of the current frame if exists.
        /// </summary>
        /// <param name="id">Identificator of the parent frame.</param>
        /// <returns>Unsigned integer array.</returns>
        internal uint[] GetDerivedList(uint id)
        {
            if (Meta.iref == null || Meta.iref.references.Count(i => i.from_item_ID == id) == 0)
                return new uint[0];

            return Meta.iref.references.First(i => i.from_item_ID == id)?.to_item_ID;
        }

        /// <summary>
        /// Returns the derivative type of the frame if exists.
        /// </summary>
        /// <param name="id">Identificator of the frame.</param>
        /// <returns>BoxType enum value.</returns>
        internal BoxType? GetDerivedType(uint id)
        {
            if (Meta.iref == null || Meta.iref.references.Count(i => i.from_item_ID == id) == 0)
                return null;

            return Meta.iref.references.First(i => i.from_item_ID == id)?.type;
        }
    }
}

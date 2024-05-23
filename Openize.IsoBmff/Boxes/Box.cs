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
using System.Collections.Generic;

namespace Openize.IsoBmff
{
    /// <summary>
    /// Structure for storing data in IsoBmff files.
    /// </summary>
    public class Box
    {
        /// <summary>
        /// An integer that specifies the number of bytes in this box, including all its fields and 
        /// contained boxes; if size is 1 then the actual size is in the field largesize; if size is 0, then this
        /// box is the last one in the file, and its contents extend to the end of the file
        /// </summary>
        public ulong size;

        /// <summary>
        /// Identifies the box type; standard boxes use a compact type, which is normally four printable 
        /// characters, to permit ease of identification, and is shown so in the boxes below. User extensions
        /// use an extended type; in this case, the type field is set to 'uuid'.
        /// </summary>
        public BoxType type;

        /// <summary>
        /// External box constructor for unimplemented box types.
        /// </summary>
        /// <param name="stream">Stream reader.</param>
        /// <param name="size">Box size in bytes.</param>
        public delegate Box ExternalBoxConstructor(BitStreamReader stream, ulong size);

        /// <summary>
        /// Dictionary of external box constructors.
        /// </summary>
        private static Dictionary<BoxType, ExternalBoxConstructor> _externalParsersDictionary
            = new Dictionary<BoxType, ExternalBoxConstructor>();

        /// <summary>
        /// Text summary of the box.
        /// </summary>
        public string ToString => $"{type} size: {size} [Box to string]";

        /// <summary>
        /// Create the box object from the bitstream.
        /// </summary>
        /// <param name="stream">File stream.</param>
        public Box(BitStreamReader stream)
        {
            size = (ulong)stream.Read(32);
            type = (BoxType)stream.Read(32);

            if (size == 1)
                size = (ulong)stream.Read(32) << 32 | (ulong)stream.Read(32);
        }

        /// <summary>
        /// Create the box object from the box type and box size in bytes.
        /// This constructor doesn't read data from the stream.
        /// </summary>
        /// <param name="boxtype">Box type integer.</param>
        /// <param name="size">Box size in bytes.</param>
        public Box(BoxType boxtype, ulong size)
        {
            this.size = size;
            type = boxtype;
        }

        /// <summary>
        /// Create the box object from the bitstream and box type.
        /// </summary>
        /// <param name="stream">File stream.</param>
        /// <param name="boxtype">Box type integer.</param>
        public Box(BitStreamReader stream, BoxType boxtype)
        {
            size = (ulong)stream.Read(32);
            var readtype = (BoxType)stream.Read(32);

            if (readtype != boxtype)
                throw new ArgumentException();

            type = readtype;

            if (size == 1)
                size = (ulong)stream.Read(32) << 32 | (ulong)stream.Read(32);
        }

        /// <summary>
        /// Read next box from stream.
        /// </summary>
        /// <param name="stream">File stream reader.</param>
        /// <returns>Parces box.</returns>
        public static Box ParceBox(BitStreamReader stream)
        {
            ulong startPosition = stream.GetBitPosition();
            ulong size = (ulong)stream.Read(32);
            BoxType type = (BoxType)stream.Read(32);

            if (size == 1)
                size = (ulong)stream.Read(32) << 32 | (ulong)stream.Read(32);

            Box box;
            switch (type)
            {
                case BoxType.ftyp:
                    box = new FileTypeBox(stream, size);
                    break;
                case BoxType.meta:
                    box = new MetaBox(stream, size, startPosition);
                    break;
                case BoxType.pitm:
                    box = new PrimaryItemBox(stream, size);
                    break;
                case BoxType.iloc:
                    box = new ItemLocationBox(stream, size);
                    break;
                case BoxType.ipro:
                    box = new ItemProtectionBox(stream, size);
                    break;
                case BoxType.iinf:
                    box = new ItemInfoBox(stream, size);
                    break;
                case BoxType.iref:
                    box = new ItemReferenceBox(stream, size, startPosition);
                    break;
                case BoxType.idat:
                    box = new ItemDataBox(stream, size, startPosition);
                    break;
                case BoxType.iprp:
                    box = new ItemPropertiesBox(stream, size, startPosition);
                    break;
                case BoxType.dinf:
                    box = new DataInformationBox(stream, size);
                    break;
                case BoxType.grpl:
                    box = new GroupsListBox(stream, size, startPosition);
                    break;

                case BoxType.ispe:
                    box = new ImageSpatialExtentsProperty(stream, size);
                    break;
                case BoxType.pasp:
                    box = new PixelAspectRatioBox(stream, size);
                    break;
                case BoxType.colr:
                    box = new ColourInformationBox(stream, size);
                    break;
                case BoxType.pixi:
                    box = new PixelInformationProperty(stream, size);
                    break;
                case BoxType.rloc:
                    box = new RelativeLocationProperty(stream, size);
                    break;
                case BoxType.auxC:
                    box = new AuxiliaryTypeProperty(stream, size);
                    break;
                case BoxType.clap:
                    box = new CleanApertureBox(stream, size);
                    break;
                case BoxType.irot:
                    box = new ImageRotation(stream, size);
                    break;
                case BoxType.lsel:
                    box = new LayerSelectorProperty(stream, size);
                    break;
                case BoxType.imir:
                    box = new ImageMirror(stream, size);
                    break;

                case BoxType.oinf:
                    box = new OperatingPointsInformationProperty(stream, size);
                    break;
                case BoxType.udes:
                    box = new UserDescriptionBox(stream, size, startPosition);
                    break;

                case BoxType.moov:
                    box = new MovieBox(stream, size, startPosition);
                    break;

                case BoxType.hvcC:
                case BoxType.av1C:
                default:
                    if (_externalParsersDictionary.ContainsKey(type))
                        box = _externalParsersDictionary[type](stream, size);
                    else
                        box = new Box(type, size);
                    break;
            }

            ulong currentPosition = stream.GetBitPosition();
            if (currentPosition - startPosition < size * 8)
                stream.SkipBits((int)(size * 8 - (currentPosition - startPosition)));

            return box;
        }

        /// <summary>
        /// Add external constructor for unimplemented box type.
        /// </summary>
        /// <param name="type">Box type.</param>
        /// <param name="parser">External box constructor.</param>
        public static void SetExternalConstructor(BoxType type, ExternalBoxConstructor parser){
            _externalParsersDictionary[type] = parser;
        }

        /// <summary>
        /// Convert uint value to string with ASCII coding.
        /// </summary>
        /// <param name="value">Unsigned integer.</param>
        /// <returns>ASCII string.</returns>
        protected static string UintToString(uint value)
        {
            string str = "";

            for (int i = 24; i >= 0; i -= 8)
            {
                var b = (value >> i) & 0xFF;
                str += Convert.ToChar(b);
            }
            return str;
        }
    }
}
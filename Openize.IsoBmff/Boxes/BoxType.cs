/*
 * Openize.IsoBmff
 * Copyright (c) 2024 Openize Pty Ltd. 
 *
 * This file is part of Openize.IsoBmff.
 *
 * Openize.IsoBmff is available under MIT license, which is
 * available along with Openize.IsoBmff sources.
 */

namespace Openize.IsoBmff
{
    /// <summary>
    /// Type of the container box.
    /// Used definitions from ISO IEC 14496 Part 12, ISO IEC 14496 Part 15 and ISO IEC 23008 Part 12.
    /// </summary>
    public enum BoxType : uint
    {
        /// <summary>
        /// File type and compatibility. Located in the root.
        /// </summary>
        ftyp = 0x66747970,

        /// <summary>
        /// Media data container. Located in the root.
        /// </summary>
        mdat = 0x6d646174,

        /// <summary>
        /// Free space.
        /// </summary>
        free = 0x66726565,

        /// <summary>
        /// Free space.
        /// </summary>
        skip = 0x736b6970,

        /// <summary>
        /// Container for all the movie metadata. Located in the root.
        /// </summary>
        moov = 0x6d6f6f76,

        /// <summary>
        /// Metadata. Located in the root.
        /// </summary>
        meta = 0x6d657461,

        /// <summary>
        /// Handler, declares the metadata (handler) type. Located in the 'meta' box.
        /// </summary>
        hdlr = 0x68646c72,

        /// <summary>
        /// Data information box, container. Located in the 'meta' box.
        /// </summary>
        dinf = 0x64696e66,

        /// <summary>
        /// Data reference box, declares source(s) of items. Located in the 'dinf' box.
        /// </summary>
        dref = 0x64726566,

        /// <summary>
        /// Data reference box, declares source(s) of metadata items.
        /// </summary>
        url = 0x75726c20,

        /// <summary>
        /// Data reference box, declares source(s) of metadata items.
        /// </summary>
        urn = 0x75726e20,

        /// <summary>
        /// Item location. Located in the 'meta' box.
        /// </summary>
        iloc = 0x696c6f63,

        /// <summary>
        /// Item data. Located in the 'meta' box.
        /// </summary>
        idat = 0x69646174,

        /// <summary>
        /// Item information. Located in the 'meta' box.
        /// </summary>
        iinf = 0x69696e66,

        /// <summary>
        /// Item information entry. Located in the 'iinf' box.
        /// </summary>
        infe = 0x696e6665,

        /// <summary>
        /// Item reference box. Located in the 'meta' box.
        /// </summary>
        iref = 0x69726566,

        /// <summary>
        /// Derived image item. Located in the 'iref' box.
        /// </summary>
        dimg = 0x64696d67,

        /// <summary>
        /// Auxiliary media for the indicated track (e.g. depth map or alpha plane for video). Located in the 'iref' box.
        /// </summary>
        auxl = 0x6175786c,

        /// <summary>
        /// Thumbnails for the referenced track. Located in the 'iref' box.
        /// </summary>
        thmb = 0x74686d62,

        /// <summary>
        /// Primary item reference. Located in the 'meta' box.
        /// </summary>
        pitm = 0x7069746d,

        /// <summary>
        /// Item properties. Located in the 'meta' box.
        /// </summary>
        iprp = 0x69707270,

        /// <summary>
        /// Item property association. Located in the 'iprp' box.
        /// </summary>
        ipma = 0x69706d61,

        /// <summary>
        /// List of item properties. Located in the 'iprp' box.
        /// </summary>
        ipco = 0x6970636f,

        /// <summary>
        /// AV1 coded item property. Located in the 'ipco' box.
        /// </summary>
        av1C = 0x61763143,

        /// <summary>
        /// HEVC coded item property. Located in the 'ipco' box.
        /// </summary>
        hvcC = 0x68766343,

        /// <summary>
        /// Width and height item property. Located in the 'ipco' box.
        /// </summary>
        ispe = 0x69737065,

        /// <summary>
        /// Pixel aspect ratio item property. Located in the 'ipco' box.
        /// </summary>
        pasp = 0x70617370,

        /// <summary>
        /// Colour information item property. Located in the 'ipco' box.
        /// </summary>
        colr = 0x636f6c72,

        /// <summary>
        /// Bit depth item property. Located in the 'ipco' box.
        /// </summary>
        pixi = 0x70697869,

        /// <summary>
        /// Position item property. Located in the 'ipco' box.
        /// </summary>
        rloc = 0x726c6f63,

        /// <summary>
        /// Auxiliary images association item property. Located in the 'ipco' box.
        /// </summary>
        auxC = 0x61757843,

        /// <summary>
        /// Transformation item property. Located in the 'ipco' box.
        /// </summary>
        clap = 0x636c6170,

        /// <summary>
        /// Rotation item property. Located in the 'ipco' box.
        /// </summary>
        irot = 0x69726f74,

        /// <summary>
        /// Multi-layer item property. Located in the 'ipco' box.
        /// </summary>
        lsel = 0x6c73656c,

        /// <summary>
        /// Mirroring item property. Located in the 'ipco' box.
        /// </summary>
        imir = 0x696d6972,

        /// <summary>
        /// Operation points information item property. Located in the 'ipco' box.
        /// </summary>
        oinf = 0x6f696e66,

        /// <summary>
        /// User description item property. Located in the 'ipco' box.
        /// </summary>
        udes = 0x75646573,

        /// <summary>
        /// Content describtion item property. Located in the 'ipco' box.
        /// </summary>
        cdsc = 0x63647363,

        /// <summary>
        /// Grouping property box. Located in the 'meta' box.
        /// </summary>
        grpl = 0x6772706c,

        /// <summary>
        /// Alternatives entity group. Located in the 'grpl' box.
        /// </summary>
        altr = 0x616c7472,

        /// <summary>
        /// Stereo pair entity group. Located in the 'grpl' box.
        /// </summary>
        ster = 0x73746572,

        /// <summary>
        /// A time-synchronized capture entity group. Located in the 'grpl' box.
        /// </summary>
        tsyn = 0x7473796e,

        /// <summary>
        /// Item protection. Located in the root.
        /// </summary>
        ipro = 0x6970726f,

        /// <summary>
        /// Protection scheme information box. Located in the 'ipro' box.
        /// </summary>
        sinf = 0x73696e66,

        /// <summary>
        /// Original format box. Located in the 'sinf' box.
        /// </summary>
        frma = 0x66726d61,

        /// <summary>
        /// Scheme type box. Located in the 'sinf' box.
        /// </summary>
        schm = 0x7363686d,

        /// <summary>
        /// Scheme information box. Located in the 'sinf' box.
        /// </summary>
        schi = 0x73636869,

        /// <summary>
        /// Extended type.
        /// </summary>
        uuid = 0x75756964,
    }

    /*
    foreach (var c in "iloc")
        Console.Write(Convert.ToString(c, 16).PadLeft(2, '0'));
    */
}

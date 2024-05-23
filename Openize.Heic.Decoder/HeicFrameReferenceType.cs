/*
 * Openize.HEIC 
 * Copyright (c) 2024 Openize Pty Ltd. 
 *
 * This file is part of Openize.HEIC.
 *
 * Openize.HEIC is available under Openize license, which is
 * available along with Openize.HEIC sources.
 */

namespace Openize.Heic.Decoder
{
    /// <summary>
    /// Type of reference layers that are reverce-linked.
    /// </summary>
    enum HeicFrameReferenceType
    {
        /// <summary>
        /// Transparency layer.
        /// </summary>
        Alpha,

        /// <summary>
        /// Depth map layer.
        /// </summary>
        DepthMap,

        /// <summary>
        /// High dynamic range layer.
        /// </summary>
        Hdr,

        /// <summary>
        /// Undefined layer.
        /// </summary>
        Undefined
    }
}

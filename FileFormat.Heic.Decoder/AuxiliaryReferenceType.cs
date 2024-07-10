/*
 * FileFormat.HEIC 
 * Copyright (c) 2024 Openize Pty Ltd. 
 *
 * This file is part of FileFormat.HEIC.
 *
 * FileFormat.HEIC is available under Openize license, which is
 * available along with FileFormat.HEIC sources.
 */

namespace FileFormat.Heic.Decoder
{
    /// <summary>
    /// Type of auxiliary reference layer.
    /// </summary>
    public enum AuxiliaryReferenceType
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

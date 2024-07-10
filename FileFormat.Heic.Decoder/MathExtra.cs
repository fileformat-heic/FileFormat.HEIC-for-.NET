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
    /// Class used to create extended math functions.
    /// </summary>
    internal static class MathExtra
    {
        internal static int Clip3(int x, int y, int z)
        {
            if (z < x)
                return x;
            else if (z > y)
                return y;
            else
                return z;
        }

        internal static uint Clip3(uint x, uint y, uint z)
        {
            if (z < x)
                return x;
            else if (z > y)
                return y;
            else
                return z;
        }
        internal static int ClipBitDepth(int x, int bitDepth)
        {
            return Clip3(0, (1 << bitDepth) - 1, x);
        }
        internal static uint ClipBitDepth(uint x, int bitDepth)
        {
            return Clip3(0, (uint)((1 << bitDepth) - 1), x);
        }

        internal static int CeilDiv(int a, int b)
        {
            int deleted = a / b;
            return deleted +
                (deleted > 0 && a % b != 0 ? 1 : 0);
        }

        internal static uint CeilDiv(uint a, uint b)
        {
            return (a + b - 1) / b;
        }
    }
}

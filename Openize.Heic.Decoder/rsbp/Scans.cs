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
    internal static class Scans
    {
        public static byte[][][][] ScanOrder {  get; private set; }

        private static bool _initialised = false;

        internal static void Initialize()
        {
            if (_initialised) return;
            _initialised = true;

            ScanOrderInitialize();
        }

        internal static void ScanOrderInitialize()
        {
            ScanOrder = new byte[6][][][];
            for (int log2BlockSize = 0; log2BlockSize < 6; log2BlockSize++)
            {
                ScanOrder[log2BlockSize] = new byte[4][][];

                if (log2BlockSize < 4)
                {
                    ScanOrder[log2BlockSize][0] = DiagonalScanInitialize((byte)(1 << log2BlockSize));
                    ScanOrder[log2BlockSize][1] = HorizontalScanInitialize((byte)(1 << log2BlockSize));
                    ScanOrder[log2BlockSize][2] = VerticalScanInitialize((byte)(1 << log2BlockSize));
                }

                if (log2BlockSize > 1)
                    ScanOrder[log2BlockSize][3] = TraverseScanInitialize((byte)(1 << log2BlockSize));
            }
        }

        // 6.5.3 Up-right diagonal scan order array initialization process
        // Up-right diagonal scan order array [ sPos ][ sComp ].
        // The array index sPos specify the scan position ranging from 0 to(blkSize* blkSize) − 1.
        // The array index sComp equal to 0 specifies the horizontal component and
        // the array index sComp equal to 1 specifies the vertical component.
        internal static byte[][] DiagonalScanInitialize(byte blkSize)
        {
            byte[][] diagScan = new byte[blkSize * blkSize][];
            int i = 0;
            int x = 0;
            int y = 0;
            bool stopLoop = false;

            while (!stopLoop)
            {
                while (y >= 0)
                {
                    if (x < blkSize && y < blkSize)
                    {
                        diagScan[i] = new byte[2];
                        diagScan[i][0] = (byte)x;
                        diagScan[i][1] = (byte)y;
                        i++;
                    }
                    y--;
                    x++;
                }

                y = x;
                x = 0;

                if (i >= blkSize * blkSize)
                    stopLoop = true;
            }

            return diagScan;
        }

        // 6.5.4 Horizontal scan order array initialization process
        internal static byte[][] HorizontalScanInitialize(byte blkSize)
        {
            byte[][] horScan = new byte[blkSize * blkSize][];

            int i = 0;
            for (byte y = 0; y < blkSize; y++)
            {
                for (byte x = 0; x < blkSize; x++)
                {
                    horScan[i] = new byte[2];
                    horScan[i][0] = x;
                    horScan[i][1] = y;
                    i++;
                }
            }

            return horScan;
        }

        // 6.5.5 Vertical scan order array initialization process
        internal static byte[][] VerticalScanInitialize(byte blkSize)
        {
            byte[][] verScan = new byte[blkSize * blkSize][];

            int i = 0;
            for (byte x = 0; x < blkSize; x++)
            {
                for (byte y = 0; y < blkSize; y++)
                {
                    verScan[i] = new byte[2];
                    verScan[i][0] = x;
                    verScan[i][1] = y;
                    i++;
                }
            }

            return verScan;
        }

        // 6.5.6 Traverse scan order array initialization process
        internal static byte[][] TraverseScanInitialize(byte blkSize)
        {
            byte[][] travScan = new byte[blkSize * blkSize][];

            int i = 0;
            for (byte y = 0; y < blkSize; y++)
            {
                if (y % 2 == 0)
                {
                    for (byte x = 0; x < blkSize; x++)
                    {
                        travScan[i] = new byte[2];
                        travScan[i][0] = x;
                        travScan[i][1] = y;
                        i++;
                    }
                }
                else
                {
                    for (int x = blkSize - 1; x >= 0; x--)
                    {
                        travScan[i] = new byte[2];
                        travScan[i][0] = (byte)x;
                        travScan[i][1] = y;
                        i++;
                    }
                }
            }

            return travScan;
        }



    }
}

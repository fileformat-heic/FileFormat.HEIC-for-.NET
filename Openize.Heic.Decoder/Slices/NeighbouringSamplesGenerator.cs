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
    internal class NeighbouringSamplesGenerator
    {
        bool availableLeft;
        bool availableTop;
        bool availableTopRight;
        bool availableTopLeft;

        HeicPicture picture;
        int x0;
        int y0;
        int nTbS;
        int cIdx;

        int ctbLog2Size;
        uint picWidthInCtbs;
        int SubWidth;
        int SubHeight;
        int xBLuma;
        int yBLuma;

        int currCTBSlice;
        int currCTBTileId;

        ushort firstValue;
        //int nBottom;
        //int nRight;
        //int nAvail;

        NeighbouringSamplesGenerator(HeicPicture picture, int x0, int y0, int nTbS, int cIdx)
        {
            this.picture = picture;
            this.x0 = x0;
            this.y0 = y0;
            this.nTbS = nTbS;
            this.cIdx = cIdx;

            ctbLog2Size = picture.sps.CtbLog2SizeY;
            picWidthInCtbs = picture.sps.PicWidthInCtbsY;

            if (cIdx > 0)
            {

                if (picture.sps.ChromaArrayType == 1)// for 420
                {
                    //ctbLog2Size = picture.sps.CtbLog2SizeY * picture.sps.SubWidthC;
                }
            }

            SubWidth = (cIdx == 0) ? 1 : picture.sps.SubWidthC;
            SubHeight = (cIdx == 0) ? 1 : picture.sps.SubHeightC;

            xBLuma = this.x0 * SubWidth;
            yBLuma = this.y0 * SubHeight;
        }

        private void CalculateFlags()
        {
            availableLeft = true;
            availableTop = true;
            availableTopRight = true;
            availableTopLeft = true;

            if (xBLuma == 0)
            {
                availableLeft = false;
                availableTopLeft = false;
            }

            if (yBLuma == 0)
            {
                availableTop = false;
                availableTopLeft = false;
                availableTopRight = false;
            }

            if (xBLuma + nTbS * SubWidth >= picture.sps.pic_width_in_luma_samples)
            {
                availableTopRight = false;
            }

            // check for tile and slice boundaries

            int xCurrCtb = xBLuma >> ctbLog2Size;
            int yCurrCtb = yBLuma >> ctbLog2Size;
            int xLeftCtb = (xBLuma - 1) >> ctbLog2Size;
            int xRightCtb = (xBLuma + nTbS * SubWidth) >> ctbLog2Size;
            int yTopCtb = (yBLuma - 1) >> ctbLog2Size;

            currCTBSlice = (int)picture.SliceAddrRs[xCurrCtb, yCurrCtb];
            currCTBTileId = (int)picture.pps.TileIdFromRs[xCurrCtb + yCurrCtb * picWidthInCtbs];

            availableLeft = ReCheckFlag(availableLeft, xLeftCtb, yCurrCtb);
            availableTop = ReCheckFlag(availableTop, xCurrCtb, yTopCtb);
            availableTopRight = ReCheckFlag(availableTopRight, xRightCtb, yTopCtb);
            availableTopLeft = ReCheckFlag(availableTopLeft, xLeftCtb, yTopCtb);
        }

        private bool ReCheckFlag(bool avaliable, int x, int y)
        {
            if (avaliable)
                if (currCTBSlice != picture.SliceAddrRs[x, y] ||
                    currCTBTileId != picture.pps.TileIdFromRs[x + y * picWidthInCtbs])
                    return false;

            return avaliable;
        }

        private NeighbouringSamples FillNeighbouringSamples()
        {
            int nBottom = (int)picture.sps.pic_height_in_luma_samples - y0 * SubHeight;
            nBottom = (nBottom + SubHeight - 1) / SubHeight;
            if (nBottom > 2 * nTbS) nBottom = 2 * nTbS;

            int nRight = (int)picture.sps.pic_width_in_luma_samples - x0 * SubWidth;
            nRight = (nRight + SubWidth - 1) / SubWidth;
            if (nRight > 2 * nTbS) nRight = 2 * nTbS;

            int nAvail = 0;
            bool availableN;

            uint currBlockAddr = picture.pps.MinTbAddrZs[
                xBLuma >> picture.sps.MinTbLog2SizeY, 
                yBLuma >> picture.sps.MinTbLog2SizeY];


            var samples = new NeighbouringSamples(nTbS);
            var available = new NeighbouringSamplesAvaliablity(nTbS);

            if (availableLeft)
            {
                for (int y = nBottom - 1; y >= 0; y -= 4) {
                    uint NBlockAddr = picture.pps.MinTbAddrZs[
                        ((x0 - 1) * SubWidth) >> picture.sps.MinTbLog2SizeY,
                        ((y0 + y) * SubHeight) >> picture.sps.MinTbLog2SizeY];

                    availableN = NBlockAddr <= currBlockAddr;

                    if (picture.pps.constrained_intra_pred_flag)
                    {
                        if (picture.CuPredMode[(x0 - 1) * SubWidth, (y0 + y) * SubHeight] != PredMode.MODE_INTRA)
                            availableN = false;
                    }

                    if (availableN)
                    {
                        if (nAvail == 0) 
                            firstValue = picture.pixels[cIdx][x0 - 1, y0 + y];

                        for (int i = 0; i < 4; i++)
                        {
                            available[-1, y - i] = true;
                            samples[-1, y - i] = picture.pixels[cIdx][x0 - 1, y0 + y - i];
                            nAvail++;
                        }
                    }
                }
            }

            if (availableTopLeft)
            {
                uint NBlockAddr = picture.pps.MinTbAddrZs[
                    ((x0 - 1) * SubWidth) >> picture.sps.MinTbLog2SizeY,
                    ((y0 - 1) * SubHeight) >> picture.sps.MinTbLog2SizeY];

                availableN = NBlockAddr <= currBlockAddr;

                if (picture.pps.constrained_intra_pred_flag)
                {
                    if (picture.CuPredMode[(x0 - 1) * SubWidth, (y0 - 1) * SubHeight] != PredMode.MODE_INTRA)
                        availableN = false;
                }

                if (availableN)
                {
                    if (nAvail == 0) 
                        firstValue = picture.pixels[cIdx][x0 - 1, y0 - 1];

                    available[-1, -1] = true;
                    samples[-1, -1] = picture.pixels[cIdx][x0 - 1, y0 - 1];
                    nAvail++;
                }
            }

            for (int x = 0; x < nRight; x += 4)
            {
                if (x < nTbS ? availableTop : availableTopRight)
                {
                    uint NBlockAddr = picture.pps.MinTbAddrZs[
                        ((x0 + x) * SubWidth) >> picture.sps.MinTbLog2SizeY,
                        ((y0 - 1) * SubHeight) >> picture.sps.MinTbLog2SizeY];

                    availableN = NBlockAddr <= currBlockAddr;

                    if (picture.pps.constrained_intra_pred_flag)
                    {
                        if (picture.CuPredMode[(x0 + x) * SubWidth, (y0 - 1) * SubHeight] != PredMode.MODE_INTRA)
                            availableN = false;
                    }


                    if (availableN)
                    {
                        if (nAvail == 0) firstValue = picture.pixels[cIdx][x0 + x, y0 - 1];

                        for (int i = 0; i < 4; i++)
                        {
                            available[x + i, -1] = true;
                            samples[x + i, -1] = picture.pixels[cIdx][x0 + x + i, y0 - 1];
                            nAvail++;
                        }
                    }
                }
            }


            if (nAvail != 4 * nTbS + 1)
            {
                if (nAvail == 0)
                {
                    samples.Reset(cIdx == 0 ?
                        picture.sps.BitDepthY :
                        picture.sps.BitDepthC);
                }
                else
                {
                    if (!available[-1, 2 * nTbS - 1])
                        samples[-1, 2 * nTbS - 1] = firstValue;

                    for (int i = 2 * nTbS - 2; i >= -1; i--)
                        if (!available[-1, i])
                            samples[-1, i] = samples[-1, i + 1];

                    for (int i = 0; i <= 2 * nTbS - 1; i++)
                        if (!available[i, -1])
                            samples[i, -1] = samples[i - 1, -1];
                }
            }
            
            return samples;
        }

        public static NeighbouringSamples Generate(HeicPicture picture, int x0, int y0, int nTbS, int cIdx)
        {
            NeighbouringSamples samples = new NeighbouringSamples(nTbS);

            var gen = new NeighbouringSamplesGenerator(picture, x0, y0, nTbS, cIdx);
            gen.CalculateFlags();
            return gen.FillNeighbouringSamples();
        }
    }
}

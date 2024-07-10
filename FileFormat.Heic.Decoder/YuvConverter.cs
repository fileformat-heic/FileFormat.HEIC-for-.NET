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
    /// Class used to convert colors from YUV colorspace to RGB.
    /// </summary>
    internal class YuvConverter
    {
        /// <summary>
        /// Full or TV range flag.
        /// </summary>
        bool fullRangeFlag;

        /// <summary>
        /// Coded image.
        /// </summary>
        HeicImageFrame picture;

        double Kr;
        double Kb;
        double Kg;

        double cooffCrToR;
        double cooffCrToG;
        double cooffCbToG;
        double cooffCbToB;

        public YuvConverter(HeicImageFrame picture)
        {
            fullRangeFlag = picture.hvcConfig.SPS.vui_parameters.video_full_range_flag;

            this.picture = picture;

            DefineCoefficients(picture.hvcConfig.SPS.vui_parameters);
        }


        // 8 bit!

        /// <summary>
        /// Convert YUV byte array to RGBA.
        /// </summary>
        /// <returns>RGBA byte array.</returns>
        public byte[,,] GetRgbaByteArray()
        {
            uint width = picture.hvcConfig.SPS.pic_width_in_luma_samples;
            uint height = picture.hvcConfig.SPS.pic_height_in_luma_samples;

            byte[,,] pixels = new byte[width, height, 4];

            double Y, Cr, Cb, R, G, B;

            int chromaHalfRange = 1 << (picture.hvcConfig.SPS.BitDepthC - 1);
            int lumaOffset = 16 << (picture.hvcConfig.SPS.BitDepthY - 8);

            double tvRangeCoeffLuma = 255.0 / 219;
            double tvRangeCoeffChroma = 255.0 / 224;

            // convertion to 8 bit
            double BitKoefY = 256.0 / (1 << picture.hvcConfig.SPS.BitDepthY);
            double BitKoefC = 256.0 / (1 << picture.hvcConfig.SPS.BitDepthC);

            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    Y = picture.rawPixels[0][col, row];

                    if (!fullRangeFlag)
                        Y = tvRangeCoeffLuma * (Y - lumaOffset);

                    if (picture.hvcConfig.SPS.ChromaArrayType == 0)
                    {
                        (R, G, B) = (Y, Y, Y);
                    }
                    else
                    {
                        Cb = picture.rawPixels[1][col / picture.hvcConfig.SPS.SubWidthC, row / picture.hvcConfig.SPS.SubHeightC] - chromaHalfRange;
                        Cr = picture.rawPixels[2][col / picture.hvcConfig.SPS.SubWidthC, row / picture.hvcConfig.SPS.SubHeightC] - chromaHalfRange;

                        if (!fullRangeFlag)
                        {
                            Cb = tvRangeCoeffChroma * Cb;
                            Cr = tvRangeCoeffChroma * Cr;
                        }

                        (R, G, B) = YCrCb2RGB(Y * BitKoefY, Cb * BitKoefC, Cr * BitKoefC);
                    }

                    pixels[col, row, 0] = (byte)MathExtra.Clip3(0, 255, (int)R);
                    pixels[col, row, 1] = (byte)MathExtra.Clip3(0, 255, (int)G);
                    pixels[col, row, 2] = (byte)MathExtra.Clip3(0, 255, (int)B);
                    pixels[col, row, 3] = 255;
                }
            }

            return pixels;
        }

        /// <summary>
        /// Calculate RGB values from YCrCb based on curred converter settings.
        /// </summary>
        /// <param name="Y">Luma value.</param>
        /// <param name="Cb">Chroma blue value.</param>
        /// <param name="Cr">Chroma red value.</param>
        /// <returns>Red, Green and Blue values.</returns>
        private (double, double, double) YCrCb2RGB(double Y, double Cb, double Cr)
        {
            return (
                Y + cooffCrToR * Cr,
                Y - cooffCrToG * Cr - cooffCbToG * Cb,
                Y + cooffCbToB * Cb
            );
        }

        /// <summary>
        /// Define converter coefficients based on parameters of image meta data.
        /// </summary>
        /// <param name="vui_parameters">Image usability information data.</param>
        private void DefineCoefficients(vui_parameters vui_parameters)
        {
            //vui_parameters.video_full_range_flag;
            //vui_parameters.colour_primaries;
            //vui_parameters.transfer_characteristics;
            //vui_parameters.matrix_coeffs;

            switch (vui_parameters.matrix_coeffs)
            {
                case 1:
                    Kr = 0.2126;
                    Kb = 0.0722;
                    break;
                case 4:
                    Kr = 0.30;
                    Kb = 0.11;
                    break;
                case 5:
                case 6:
                    Kr = 0.299;
                    Kb = 0.114;
                    break;
                case 7:
                    Kr = 0.212;
                    Kb = 0.087;
                    break;
                case 9:
                case 10:
                    Kr = 0.2627;
                    Kb = 0.0593;
                    break;

                case 12:
                case 13:
                    var p = ColorPrimaries.GetSpecified(vui_parameters.colour_primaries);

                    double zR = 1 - (p.xR + p.yR);
                    double zG = 1 - (p.xG + p.yG);
                    double zB = 1 - (p.xB + p.yB);
                    double zW = 1 - (p.xW + p.yW);

                    double denom = p.yW * (p.xR * (p.yG * zB - p.yB * zG) + p.xG * (p.yB * zR - p.yR * zB) + p.xB * (p.yR * zG - p.yG * zR));

                    try
                    {
                        Kr = (p.yR * (p.xW * (p.yG * zB - p.yB * zG) + p.yW * (p.xB * zG - p.xG * zB) + zW * (p.xG * p.yB - p.xB * p.yG))) / denom;
                        Kb = (p.yB * (p.xW * (p.yR * zG - p.yG * zR) + p.yW * (p.xG * zR - p.xR * zG) + zW * (p.xR * p.yG - p.xG * p.yR))) / denom;
                    }
                    catch
                    {
                        Kr = 0.299;
                        Kb = 0.114;
                    }
                    break;

                case 0:  // Identity; See Equations E-31 to E-33
                case 2:  // Unspecified
                case 3:  // Reserved
                case 8:  // YCgCo;    See Equations E-28 to E-30
                case 11: // Y′D′ZD′X; See Equations E-59 to E-61
                case 14: // ICTCP
                default:
                    Kr = 0.299;
                    Kb = 0.114;
                    break;

            }

            Kg = 1 - Kr - Kb;

            cooffCrToR = 2 * (1 - Kr);
            cooffCrToG = 2 * Kr * (1 - Kr) / Kg;
            cooffCbToG = 2 * Kb * (1 - Kb) / Kg;
            cooffCbToB = 2 * (1 - Kb);
        }


        internal class ColorPrimaries
        {
            public double xG;
            public double yG;
            public double xB;
            public double yB;
            public double xR;
            public double yR;
            public double xW;
            public double yW;

            public ColorPrimaries(double xG, double yG, double xB, double yB, double xR, double yR, double xW, double yW)
            {
                this.xG = xG;
                this.yG = yG;
                this.xB = xB;
                this.yB = yB;
                this.xR = xR;
                this.yR = yR;
                this.xW = xW;
                this.yW = yW;
            }

            public static ColorPrimaries GetSpecified(int colourPrimaries)
            {
                switch (colourPrimaries)
                {
                    case 1:
                        return new ColorPrimaries(0.300, 0.600, 0.150, 0.060, 0.640, 0.330, 0.3127, 0.3290);
                    case 4:
                        return new ColorPrimaries(0.210, 0.710, 0.140, 0.080, 0.670, 0.330, 0.3100, 0.3160);
                    case 5:
                        return new ColorPrimaries(0.290, 0.600, 0.150, 0.060, 0.640, 0.330, 0.3127, 0.3290);
                    case 6:
                    case 7:
                        return new ColorPrimaries(0.310, 0.595, 0.155, 0.070, 0.630, 0.340, 0.3127, 0.3290);
                    case 8:
                        return new ColorPrimaries(0.243, 0.692, 0.145, 0.049, 0.681, 0.319, 0.3100, 0.3160);
                    case 9:
                        return new ColorPrimaries(0.170, 0.797, 0.131, 0.046, 0.708, 0.292, 0.3127, 0.3290);
                    case 10:
                        return new ColorPrimaries(0.000, 1.000, 0.000, 0.000, 1.000, 0.000, 0.333333, 0.33333);
                    case 11:
                        return new ColorPrimaries(0.265, 0.690, 0.150, 0.060, 0.680, 0.320, 0.3140, 0.3510);
                    case 12:
                        return new ColorPrimaries(0.265, 0.690, 0.150, 0.060, 0.680, 0.320, 0.3127, 0.3290);
                    case 22:
                        return new ColorPrimaries(0.295, 0.605, 0.155, 0.077, 0.630, 0.340, 0.3127, 0.3290);
                    case 2:  // Unspecified
                    default: // Reserved
                        return null;
                }
            }
        }
    }
}

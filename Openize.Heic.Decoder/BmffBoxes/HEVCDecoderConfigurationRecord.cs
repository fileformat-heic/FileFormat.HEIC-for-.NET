/*
 * Openize.HEIC 
 * Copyright (c) 2024 Openize Pty Ltd. 
 *
 * This file is part of Openize.HEIC.
 *
 * Openize.HEIC is available under Openize license, which is
 * available along with Openize.HEIC sources.
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Openize.Heic.Decoder.IO;

namespace Openize.Heic.Decoder
{
    internal class HEVCDecoderConfigurationRecord
    {
        public byte configurationVersion; // 1
        public byte general_profile_space;
        public bool general_tier_flag;
        public byte general_profile_idc;
        public bool[] general_profile_compatibility_flags;
        public bool[] general_constraint_indicator_flags;
        public byte general_level_idc;
        public ushort min_spatial_segmentation_idc;
        public byte parallelismType;
        public byte chromaFormat;
        public byte bitDepthLumaMinus8;
        public byte bitDepthChromaMinus8;
        public int avgFrameRate; // takes the value 0
        public int constantFrameRate; // takes the value 0
        public int numTemporalLayers; // takes the value 0
        public int temporalIdNested; // takes the value 0

        private byte lengthSizeMinusOne;
        private byte numOfArrays;
        private bool[] array_completeness;
        private List<NalUnit> nalUnits;

        public string ToString() => $"HEVCDecoderConfiguration; completeness: " + String.Join(" ", array_completeness);
#if DEBUG
        public string AsString => ToString();
#endif

        public ObservableCollection<NalUnit> Children { get; set; }

        internal pic_parameter_set_rbsp PPS => (pic_parameter_set_rbsp)nalUnits.Find(i => i.NalHeader.type == NalUnitType.PPS_NUT);

        internal seq_parameter_set_rbsp SPS => (seq_parameter_set_rbsp)nalUnits.Find(i => i.NalHeader.type == NalUnitType.SPS_NUT);

        internal video_parameter_set_rbsp VPS => (video_parameter_set_rbsp)nalUnits.Find(i => i.NalHeader.type == NalUnitType.VPS_NUT);

        public HEVCDecoderConfigurationRecord(BitStreamWithNalSupport stream)
        {
            configurationVersion = (byte)stream.Read(8);

            general_profile_space = (byte)stream.Read(2);
            general_tier_flag = stream.ReadFlag();
            general_profile_idc = (byte)stream.Read(5);

            general_profile_compatibility_flags = new bool[32];
            for (int i = 0; i < 32; i++)
                general_profile_compatibility_flags[i] = stream.ReadFlag();

            general_constraint_indicator_flags = new bool[48];
            for (int i = 0; i < 48; i++)
                general_constraint_indicator_flags[i] = stream.ReadFlag();

            general_level_idc = (byte)stream.Read(8);

            stream.SkipBits(4); // 1111
            min_spatial_segmentation_idc = (ushort)stream.Read(12);
            stream.SkipBits(6); // 111111
            parallelismType = (byte)stream.Read(2);
            stream.SkipBits(6); // 111111
            chromaFormat = (byte)stream.Read(2);
            stream.SkipBits(5); // 11111
            bitDepthLumaMinus8 = (byte)stream.Read(3);
            stream.SkipBits(5); // 11111
            bitDepthChromaMinus8 = (byte)stream.Read(3);

            avgFrameRate = stream.Read(16);
            constantFrameRate = stream.Read(2);
            numTemporalLayers = stream.Read(3);
            temporalIdNested = stream.Read(1);

            lengthSizeMinusOne = (byte)stream.Read(2);
            if (lengthSizeMinusOne != 3)
                throw new FormatException();

            numOfArrays = (byte)stream.Read(8);

            nalUnits = new List<NalUnit>();
            array_completeness = new bool[numOfArrays];
            for (int j = 0; j < numOfArrays; j++)
            {
                array_completeness[j] = stream.ReadFlag();
                stream.SkipBits(1); // = 0
                NalUnitType NAL_unit_type = (NalUnitType)stream.Read(6);
                ushort numNalus = (ushort)stream.Read(16);

                for (int i = 0; i < numNalus; i++)
                {
                    ushort nalUnitLength = (ushort)stream.Read(16);
                    var unit = NalUnit.ParseKnownUnit(stream, nalUnitLength, NAL_unit_type);
                    stream.Context.AddNalContext(NAL_unit_type, unit);
                    nalUnits.Add(unit);
                }
            }
            Children = new ObservableCollection<NalUnit>(nalUnits);
        }
    }
}

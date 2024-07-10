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
    internal class NeighbouringSamplesTemplate<T>
    {
        protected T[] data;
        protected int size;

        public NeighbouringSamplesTemplate(int nTbS)
        {
            data = new T[4 * nTbS + 1];
            size = nTbS;
        }

        public T this[int x, int y]
        {
            get => data[getIndex(x, y)];
            set => data[getIndex(x, y)] = value;
        }

        protected int getIndex(int x, int y)
        {
            if (y == -1)
                return x + 1;
            else
                return 2 * size + y + 1;
        }
    }

    internal class NeighbouringSamples : NeighbouringSamplesTemplate<ushort>
    {
        public NeighbouringSamples(int nTbS) : base(nTbS) { }

        public void Reset(int bitDepth)
        {
            var value = 1 << (bitDepth - 1);
            for (int i = 0; i < data.Length; i++)
                data[i] = (ushort)value;
        }
    }
    internal class NeighbouringSamplesAvaliablity : NeighbouringSamplesTemplate<bool>
    {
        public NeighbouringSamplesAvaliablity(int nTbS) : base(nTbS) { }
    }
}

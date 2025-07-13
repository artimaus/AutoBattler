using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Reflection.Metadata.BlobBuilder;

namespace AutoBattlerLib
{

    public struct BitFlagArray
    {
        private ulong[] bits;
        public readonly int Length { get; }

        public BitFlagArray(int size)
        {
            Length = size;
            bits = new ulong[(size + 63) / 64]; // 64 bits per ulong
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Flag(int index)
        {
            if ((uint)index >= (uint)Length) return false;

            int wordIndex = index >> 6;  // Divide by 64
            int bitIndex = index & 63;   // Modulo 64
            bits[wordIndex] |= 1UL << bitIndex;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool DeFlag(int index)
        {
            if ((uint)index >= (uint)Length) return false;

            int wordIndex = index >> 6;
            int bitIndex = index & 63;
            bits[wordIndex] &= ~(1UL << bitIndex);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ToggleFlag(int index)
        {
            if ((uint)index >= (uint)Length) return false;

            int wordIndex = index >> 6;
            int bitIndex = index & 63;
            bits[wordIndex] ^= 1UL << bitIndex;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool GetBit(int index)
        {
            if ((uint)index >= (uint)Length || index < 0) return false;

            int wordIndex = index >> 6;
            int bitIndex = index & 63;
            return (bits[wordIndex] & (1UL << bitIndex)) != 0;
        }

        public bool this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetBit(index);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (value) Flag(index);
                else DeFlag(index);
            }
        }
    }



    public struct BloomMap<T, K> where T : struct, IEquatable<T>, IComparable<T> where K : struct, IEquatable<K>
    {
        private const int hashes = 4;
        private const double FalsePositiveRate = 0.075;

        private BitFlagArray bloom;
        private T[] t;
        private int nextElement;
        private K[] k;

        public BloomMap(T[] tArray, K[] kArray)
        {
            int elements = Math.Min(tArray.Length, kArray.Length);
            if (elements > 1000000)
            {
                elements = 1000000;
            }
            int bloomSize = CalculateOptimalBloomSize(elements, FalsePositiveRate);
            bloom = new BitFlagArray(bloomSize);
            t = new T[elements];
            k = new K[elements];
            nextElement = 0;

            Add(tArray, kArray);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int CalculateOptimalBloomSize(int numItems, double falsePositiveRate)
        {
            // m = -(n * ln(p)) / (ln(2)^2)
            // where m = size, n = number of items, p = false positive rate
            double size = -(numItems * Math.Log(falsePositiveRate)) / (Math.Log(2) * Math.Log(2));
            return NextPowerOf2((int)Math.Ceiling(size));
        }

        public int FirstHashFunction(int t)
        {
            t ^= (int)((uint)t >> 16);
            t *= unchecked((int)0x85ebca6b);
            t ^= (int)((uint)t >> 13);
            t *= unchecked((int)0xc2b2ae35);
            t ^= (int)((uint)t >> 16);
            return t;
        }

        public int SecondHashFunction(int t)
        {
            t = ~t + (t << 15); // input = (input << 15) - input - 1;
            t = t ^ ((int)((uint)t >> 12));
            t = t + (t << 2);
            t = t ^ ((int)((uint)t >> 4));
            t = t * 2057; // input = (input + (input << 3)) + (input << 11);
            t = t ^ ((int)((uint)t >> 16));
            return t;
        }

        public void Add(T[] tArray, K[] kArray)
        {
            Array.Sort(tArray, kArray);
            for (int i = 0; i < t.Length; i++)
            {
                if(!Add(tArray[i], kArray[i]))
                {
                    break;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool Add(T newT, K newK)
        {
            if (nextElement >= t.Length)
            {
                return false;
            }

            int code = newT.GetHashCode();
            int firstHash = FirstHashFunction(code);
            int secondHash = SecondHashFunction(code);

            // Use double hashing: h(k,i) = (h1(k) + i*h2(k)) mod m
            uint gthHash = (uint)firstHash;
            for (int i = 0; i < hashes; i++)
            {
                if (i > 0) gthHash += (uint)secondHash;
                bloom[(int)(gthHash % (uint)bloom.Length)] = true;
            }

            t[nextElement] = newT;
            k[nextElement] = newK;
            nextElement++;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T searchT)
        {
            int code = searchT.GetHashCode();
            int firstHash = FirstHashFunction(code);
            int secondHash = SecondHashFunction(code);

            uint gthHash = (uint)firstHash;
            for (int i = 0; i < hashes; i++)
            {
                if (i > 0) gthHash += (uint)secondHash;
                if (!bloom[(int)(gthHash % (uint)bloom.Length)])
                {
                    return false;  // Definitely not present
                }
            }
            return true;  // Possibly present (could be false positive)
        }

        public bool TryGetValue(T atT, out K outK)
        {
            if (Contains(atT))
            {
                int left = 0;
                int right = nextElement - 1;

                while (left <= right)
                {
                    int mid = left + ((right - left) >> 1);

                    int comparison = t[mid].CompareTo(atT);
                    if (comparison == 0)
                    {
                        outK = k[mid];
                        return true;
                    }
                    else if (comparison < 0)
                    {
                        left = mid + 1;
                    }
                    else
                    {
                        right = mid - 1;
                    }
                }
            }
            outK = k[0];
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int NextPowerOf2(int n)
        {
            if (n <= 1) return 1;
            return 1 << (32 - BitOperations.LeadingZeroCount((uint)(n - 1)));
        }
    }
}

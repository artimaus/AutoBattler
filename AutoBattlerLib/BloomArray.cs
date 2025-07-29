using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Numerics;
using System.Reflection;
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
                if (!Add(tArray[i], kArray[i]))
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

    public struct BitFlagMap<T> where T : struct, IEquatable<T>
    {
        private int nextIndex;
        private ulong[] lookup;
        private T[] values;
        private int[] reverseIndex; // valueIndex -> key mapping

        private byte bits;
        private byte pack;
        private ulong mask;

        public T this[int key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetValue(key);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                SetValue(key, value);
            }
        }

        public BitFlagMap(int keyRange)
        {
            bits = 8;
            pack = 8;
            mask = (1UL << bits) - 1;
            nextIndex = 1;
            lookup = new ulong[(keyRange + 7) / 8];
            values = new T[256];
            reverseIndex = new int[256];
        }

        public BitFlagMap(int keyRange, T[] initialValues)
        {
            bits = 8;
            while (initialValues.Length + 1 > (1 << bits))
            {
                bits += 4;
            }
            pack = (byte)(64 / bits);
            mask = (1UL << bits) - 1;
            nextIndex = 1;
            lookup = new ulong[(keyRange + pack - 1) / pack];
            values = new T[1 << bits];
            reverseIndex = new int[1 << bits];

            // Directly populate without going through SetValues
            for (int key = 0; key < initialValues.Length; key++)
            {
                T value = initialValues[key];

                // Skip default values (they don't need to be stored)
                if (value.Equals(default(T)))
                    continue;

                // Direct assignment - we know we have capacity
                int index = key / pack;
                int targetBit = (key % pack) * bits;
                ulong valueIndex = (ulong)nextIndex++;

                lookup[index] |= valueIndex << targetBit;
                values[valueIndex] = value;
                reverseIndex[valueIndex] = key;
            }
        }

        public BitFlagMap(int keyRange, int[] keys, T[] initialValues)
        {
            bits = 8;
            while (initialValues.Length + 1 > (1 << bits))
            {
                bits += 4;
            }
            pack = (byte)(64 / bits);
            mask = (1UL << bits) - 1;
            nextIndex = 1;
            lookup = new ulong[(keyRange + pack - 1) / pack];
            values = new T[1 << bits];
            reverseIndex = new int[1 << bits];

            SetValues(keys, initialValues);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CurrentValueNum()
        {
            return nextIndex - 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(int key)
        {
            int index = key / pack;
            int targetBit = (key % pack) * bits;
            ulong valueIndex = (lookup[index] >> targetBit) & mask;
            return valueIndex > 0 && valueIndex < (ulong)nextIndex; // Check if valueIndex is valid
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetValue(int key)
        {
            if ((uint)key >= (uint)(lookup.Length * pack)) // Single unsigned comparison
                return default(T);

            int index = key / pack;
            int targetBit = (key % pack) * bits;
            ulong valueIndex = (lookup[index] >> targetBit) & mask;

            // Branchless: return default if valueIndex is 0 or >= nextIndex
            return (valueIndex > 0 && valueIndex < (ulong)nextIndex) ? values[valueIndex] : default(T);
        }

        public T Pop()
        {
            T poppedValue = GetValue(reverseIndex[1]);
            if (poppedValue.Equals(default(T)))
            {
                return default(T); // Nothing to pop
            }
            RemoveValueWithValidKey(reverseIndex[1]);
            return poppedValue;
        }

        public int[] GetValidKeys()
        {
            int[] result = new int[nextIndex - 1];
            Array.Copy(reverseIndex, 1, result, 0, nextIndex - 1);
            return result;
        }

        public T[] GetValues(int[] keys)
        {
            T[] result = new T[keys.Length];
            int maxKey = lookup.Length * pack;

            for (int i = 0; i < result.Length; i++)
            {
                int key = keys[i];
                if ((uint)key >= (uint)maxKey)
                {
                    result[i] = default(T);
                    continue;
                }

                int index = key / pack;
                int targetBit = (key % pack) * bits;
                ulong valueIndex = (lookup[index] >> targetBit) & mask;

                result[i] = (valueIndex > 0 && valueIndex < (ulong)nextIndex)
                    ? values[valueIndex]
                    : default(T);
            }
            return result;
        }

        public bool SetValue(int key, T value)
        {
            if ((uint)key >= (uint)(lookup.Length * pack))
                return false;

            if (value.Equals(default(T)))
            {
                RemoveValueWithValidKey(key);
                return true; // Successfully removed
            }

            while(nextIndex >= values.Length)
            {
                if (bits < 24)
                {
                    UpSize();
                }
                else
                {
                    return false;
                }
            }

            int index = key / pack;
            int targetBit = (key % pack) * bits;
            ulong lookupValue = (lookup[index] >> targetBit) & mask;

            if (lookupValue == 0)
            {
                lookupValue = (ulong)nextIndex++;
            }

            lookup[index] = (lookup[index] & ~(mask << targetBit)) | (lookupValue << targetBit);
            values[lookupValue] = value;
            reverseIndex[lookupValue] = key;
            return true;
        }

        public bool SetValues(int[] keys, T[] values)
        {
            if (keys.Length == 0 || keys.Length != values.Length)
                return false;

            // First pass: handle removals and count new entries needed
            int potentialNewEntries = 0;
            bool hasValidNonDefaultKey = false;

            for (int i = 0; i < keys.Length; i++)
            {
                int key = keys[i];

                if ((uint)key >= (uint)(lookup.Length * pack))
                    continue; // Invalid key, skip

                if (values[i].Equals(default(T)))
                {
                    RemoveValueWithValidKey(key); // Remove if value is default
                }
                else
                {
                    hasValidNonDefaultKey = true;

                    // Check if this key already exists
                    int index = key / pack;
                    int targetBit = (key % pack) * bits;
                    ulong existingValue = (lookup[index] >> targetBit) & mask;
                    if (existingValue == 0)
                        potentialNewEntries++;
                }
            }

            if (!hasValidNonDefaultKey)
                return false; // No valid non-default keys to process

            // Ensure we have capacity for new entries
            while (nextIndex + potentialNewEntries >= this.values.Length)
            {
                if (bits < 24)
                {
                    UpSize();
                }
                else
                {
                    return false; // Can't expand further
                }
            }

            // Second pass: set all non-default values
            for (int i = 0; i < keys.Length; i++)
            {
                int key = keys[i];

                if ((uint)key >= (uint)(lookup.Length * pack) || values[i].Equals(default(T)))
                    continue; // Skip invalid keys and default values (already removed)

                int index = key / pack;
                int targetBit = (key % pack) * bits;
                ulong lookupValue = (lookup[index] >> targetBit) & mask;

                if (lookupValue == 0)
                {
                    lookupValue = (ulong)nextIndex++;
                }

                lookup[index] = (lookup[index] & ~(mask << targetBit)) | (lookupValue << targetBit);
                this.values[lookupValue] = values[i];
                reverseIndex[lookupValue] = key;
            }

            return true;
        }

        private void RemoveValueWithValidKey(int key)
        {
            int index = key / pack;
            int targetBit = (key % pack) * bits;
            ulong valueIndex = (lookup[index] >> targetBit) & mask;
            if (valueIndex == 0 || valueIndex >= (ulong)nextIndex)
            {
                return; // No value to remove
            }

            // Clear the lookup entry being removed
            lookup[index] &= ~(mask << targetBit);

            // If removing the last element, just decrement
            if (valueIndex == (ulong)nextIndex - 1)
            {
                values[valueIndex] = default(T);
                reverseIndex[valueIndex] = 0;
                nextIndex--;
                return;
            }

            // Move the last element to fill the gap
            values[valueIndex] = values[nextIndex - 1];
            reverseIndex[valueIndex] = reverseIndex[nextIndex - 1];

            // Update the lookup for the moved element using reverse lookup (O(1)!)
            int movedKey = reverseIndex[valueIndex];
            int movedPos = (movedKey % pack) * bits;
            lookup[movedKey / pack] = (lookup[movedKey / pack] & ~(mask << movedPos)) | (valueIndex << movedPos);

            // Clear the old last position
            values[nextIndex - 1] = default(T);
            reverseIndex[nextIndex - 1] = 0;
            nextIndex--;
        }

        public void RemoveValue(int key)
        {
            if ((uint)key >= (uint)(lookup.Length * pack))
            {
                return;
            }
            int index = key / pack;
            int targetBit = (key % pack) * bits;
            ulong valueIndex = (lookup[index] >> targetBit) & mask;
            if (valueIndex == 0 || valueIndex >= (ulong)nextIndex)
            {
                return; // No value to remove
            }

            // Clear the lookup entry being removed
            lookup[index] &= ~(mask << targetBit);

            // If removing the last element, just decrement
            if (valueIndex == (ulong)nextIndex - 1)
            {
                values[valueIndex] = default(T);
                reverseIndex[valueIndex] = 0;
                nextIndex--;
                return;
            }

            // Move the last element to fill the gap
            values[valueIndex] = values[nextIndex - 1];
            reverseIndex[valueIndex] = reverseIndex[nextIndex - 1];

            // Update the lookup for the moved element using reverse lookup (O(1)!)
            int movedKey = reverseIndex[valueIndex];
            int movedPos = (movedKey % pack) * bits;
            lookup[movedKey / pack] = (lookup[movedKey / pack] & ~(mask << movedPos)) | (valueIndex << movedPos);

            // Clear the old last position
            values[nextIndex - 1] = default(T);
            reverseIndex[nextIndex - 1] = 0;
            nextIndex--;
        }

        private void UpSize()
        {
            if (bits == 0 || bits >= 24)
                return;
            byte newBits;
            if (bits >= 20)
            {
                newBits = 24;
            }
            else
            {
                newBits = (byte)(bits + 4);
            }
            byte newPack = (byte)(64 / newBits);
            T[] newValues = new T[1UL << newBits];
            int[] newReverseIndex = new int[1UL << newBits];

            // Fix: Calculate correct array length
            int totalValues = lookup.Length * pack;
            int newLookupLength = (totalValues + newPack - 1) / newPack; // Ceiling division
            ulong[] newLookup = new ulong[newLookupLength];

            for (int i = 1; i < nextIndex; i++)
            {
                int key = reverseIndex[i];
                int newIndex = key / newPack;
                int oldTargetBit = (key % pack) * bits;
                int newTargetBit = (key % newPack) * newBits;
                newLookup[newIndex] |= ((lookup[key / pack] >> oldTargetBit) & mask) << newTargetBit;
                newValues[i] = values[i];
                newReverseIndex[i] = reverseIndex[i];
            }
            lookup = newLookup;
            values = newValues;
            reverseIndex = newReverseIndex;
            bits = newBits;
            pack = newPack;
            mask = (1UL << bits) - 1;
        }
    }
}

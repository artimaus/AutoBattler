using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Drawing;

namespace AutoBattlerLib
{
    public class BattlefieldManager
    {
        public Tick CurrentTick { get; set; } = new Tick(0);
        public BattleScheduler TickSchedule { get; set; } // [10][6682]
        public Entity[] entities { get; set; } // [26512]
        public UnitCard[] UnitCards { get; set; }

    }

    public class BattlefieldTileComponent : IComponentData
    {
        
    }

    public struct Round : IEquatable<Round>, IComparable<Round>
    {
        public int Value { get; }

        public Round(int value)
        {
            Value = value;
        }

        // Comparison operators for scheduling
        public int CompareTo(Round other) => Value.CompareTo(other.Value);
        public bool Equals(Round other) => Value == other.Value;

        // Arithmetic for recovery time calculations
        public static Round operator +(Round round, int recovery) => new Round(round.Value + recovery);
        public static int operator -(Round a, Round b) => a.Value - b.Value;

        // Standard overrides
        public override bool Equals(object obj) => obj is Round other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public static bool operator ==(Round left, Round right) => left.Equals(right);
        public static bool operator !=(Round left, Round right) => !left.Equals(right);
        public static bool operator <(Round left, Round right) => left.CompareTo(right) < 0;
        public static bool operator >(Round left, Round right) => left.CompareTo(right) > 0;
    }
    public struct Tick : IEquatable<Tick>, IComparable<Tick>
    {
        public int Value { get; }

        public Tick(int value)
        {
            Value = value;
        }

        // Comparison operators for scheduling
        public int CompareTo(Tick other) => Value.CompareTo(other.Value);
        public bool Equals(Tick other) => Value == other.Value;

        // Arithmetic for recovery time calculations
        public static Tick operator +(Tick tick, int recovery) => new Tick(tick.Value + recovery);
        public static int operator -(Tick a, Tick b) => a.Value - b.Value;

        // Standard overrides
        public override bool Equals(object obj) => obj is Tick other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public static bool operator ==(Tick left, Tick right) => left.Equals(right);
        public static bool operator !=(Tick left, Tick right) => !left.Equals(right);
        public static bool operator <(Tick left, Tick right) => left.CompareTo(right) < 0;
        public static bool operator >(Tick left, Tick right) => left.CompareTo(right) > 0;
    }

    public struct EntityLookup
    {
        public sbyte round;
        public byte tick;
        public ushort index;
    }

    public struct BattleScheduler
    {
        private ushort[][] tickEntities;// [10][6628]
        private ushort[] tickNextIndex; // [10]
        private ushort[][] roundEntities; // [4][26512,13256,6628,3314]
        private byte[][] roundEntityTicks; // [4][13256,6628,3314,1657]
        private ushort[] roundNextIndex; // [4]

        private EntityLookup[] entityLookup; // [26512]

        public BattleScheduler(ushort entityNum)
        {
            if(entityNum < 400)
            {
                entityNum = 400;
            }
            else if (entityNum > 6628)
            {
                entityNum = 6628;
            }

            tickEntities = new ushort[10][];
            tickNextIndex = new ushort[10];
            roundEntities = new ushort[4][];
            roundEntityTicks = new byte[4][];
            roundNextIndex = new ushort[4];

            for (int i = 0; i < tickEntities.Length; i++)
            {
                tickEntities[i] = new ushort[entityNum];

                if (i == 0)
                {
                    roundEntities[i] = new ushort[entityNum * 4];
                    roundEntityTicks[i] = new byte[entityNum * 2];
                }
                else if (i == 1)
                {
                    roundEntities[i] = new ushort[entityNum * 2];
                    roundEntityTicks[i] = new byte[entityNum];
                }
                else if (i == 2)
                {
                    roundEntities[i] = new ushort[entityNum];
                    roundEntityTicks[i] = new byte[(entityNum + 1) / 2];
                }
                else
                {
                    roundEntities[i] = new ushort[(entityNum + 1) / 2];
                    roundEntityTicks[i] = new byte[(entityNum + 3) / 4];
                }
            }
        }

        public void RescheduleEntity(byte tick, ushort index, byte tickAdvancement)
        {
            ushort entity = tickEntities[tick][index];
            tickEntities[tick][index] = 0; // Clear the old index
            ushort delta = (ushort)(tick + tickAdvancement);
            sbyte roundDelta = (sbyte)((delta / 10) - 1);
            byte tickDelta = (byte)(delta % 10);
            while ((roundDelta >= 0 && roundNextIndex[roundDelta] >= roundEntities[roundDelta].Length)
                    || tickNextIndex[tickDelta] >= tickEntities[tickDelta].Length)
            {
                tickDelta++;
                if(tickDelta > 9)
                {
                    roundDelta++;
                    tickDelta = 0;
                }
            }

            if (roundDelta >= 0)
            {
                EntityLookup eLookup = new EntityLookup
                {
                    round = roundDelta,
                    tick = tickDelta,
                    index = roundNextIndex[roundDelta]++
                };
                roundEntities[eLookup.round][eLookup.index] = entity;
                roundEntityTicks[eLookup.round][eLookup.index] = eLookup.tick;
                entityLookup[entity] = eLookup; // Update the lookup
            }
            else
            {
                EntityLookup eLookup = new EntityLookup
                {
                    round = 0,
                    tick = tickDelta,
                    index = tickNextIndex[tickDelta]++
                };
                tickEntities[eLookup.tick][eLookup.index] = entity;
                entityLookup[entity] = eLookup; // Update the lookup
            }
        }

        public void RescheduleEntity(ushort entity, byte tickAdvancement)
        {
            RescheduleEntity(entityLookup[entity].tick);
        }

        public void 
    }
}

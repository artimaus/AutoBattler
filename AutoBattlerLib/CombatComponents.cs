using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Drawing;

namespace AutoBattlerLib
{
    public class BattlefieldComponent : IComponentData
    {
        public Tick CurrentTick { get; set; } = new Tick(0);
        public SortedDictionary<Tick, HashSet<Entity>> Schedule { get; set; } = new SortedDictionary<Tick, HashSet<Entity>>();
        public Tick[] EntitySchedule { get; set; }
        public UnitCard[] UnitCards { get; set; }

    }

    public class BattlefieldTileComponent : IComponentData
    {
        
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

    public struct UnitCard
    {
        public Entity Unit { get; set; }

        public int BodyPartCardStartIndex { get; set; } // Index in the tracker array where this unit's body parts start
        public int BodyPartCardCount { get; set; } // Number of body parts this unit has in the tracker

        public int WeaponCardStartIndex { get; set; } // Index in the tracker array where this unit's weapons start
        public int WeaponCardCount { get; set; } // Number of weapons this unit has in the tracker

        public readonly byte Size;
        public readonly byte Strength;
        public readonly byte Dexterity;
        public readonly byte Agility;
        public readonly byte Celerity; // Reflexive or striking speed
        public readonly byte Vigor;
        public readonly byte Toughness;
        public readonly byte Will;
        public readonly byte Constitution;

        public readonly byte StrikingSkill;
        public readonly byte ParryingSkill;
        public readonly byte EvasionSkill;
        public readonly byte BlockingSkill;
        public readonly byte AthleticSkill;

        public int MaxHealth { get; set; }
        public int CurrentHealth { get; set; }
        public int CurrentExhaustion { get; set; }
        public int CurrentMorale { get; set; }
    }

    [Flags]
    public enum ArmorFlags
    {
        None = 0,
        Natural = 1 << 0,
        Damaged = 1 << 1,
        Broken = 1 << 2,
        LimbDamaged = 1 << 3,
        LimbBroken = 1 << 4
    }

    public struct ArmorCard
    {
        public string Name { get; set; }
        public ArmorType Type { get; set; }
        public sbyte AttackModifier { get; set; }
        public sbyte DefenseModifier { get; set; }
        public sbyte ToughnessModifier { get; set; }
        public sbyte ArmorResilience { get; set; }
        public ArmorFlags Flags { get; set; }
    }

    [Flags]
    public enum WeaponFlags
    {
        None = 0,
        Natural = 1 << 0,
        Damaged = 1 << 1,
        Broken = 1 << 2,
        LimbDamaged = 1 << 3,
        LimbBroken = 1 << 4
    }

    public struct WeaponCard
    {
        public string Name { get; set; }
        public WeaponType Type { get; set; }
        public sbyte Range { get; set; }
        public sbyte NumAttacks { get; set; }
        public sbyte DamageModifier { get; set; }
        public sbyte AttackModifier { get; set; }
        public sbyte DefenseModifier { get; set; }
        public WeaponFlags Flags { get; set; }
    }

    public struct BodyCard
    {
        public byte Heads;
        public byte EyesPerHead;
        public byte HeadSlots; // HeadSlots + CircletOnlySLots should never be more than Heads
        public byte CircletOnlySlots; //Circlets don't overwrite any natural weapons
        public byte Arms;
        public byte ArmSlots; // ArmSlots should never be more than Arms
        public byte Legs;
        public byte Wings;
        public byte Tails;
        public byte TrinketSlots;
        public BodyFlags Flags;
    }
}

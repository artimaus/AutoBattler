using System;
using System.Collections.Generic;

namespace AutoBattlerLib
{


    /// <summary>
    /// Defines the possible states of equipment as a bitmask.
    /// Multiple states can be combined using bitwise operations.
    /// </summary>
    [Flags]
    public enum EquipmentState
    {
        Pristine = 0,
        Rusted = 1 << 0,
        Dulled = 1 << 1,
        Damaged = 1 << 2,
        Broken = 1 << 3,
        Frozen = 1 << 4,
        Burning = 1 << 5
    }


    public class ItemComponent : IComponentData
    {
        public Equipment EquipId { get; set; }
        //public EffectId EffectId { get; set; } // Optional effect ID for items that apply effects
    }

    /// <summary>
    /// Defines the types of equipment that can be equipped by units.
    /// Each equipment can only be of one type.
    /// </summary>
    public enum EquipmentType
    {
        TwoHandedWeapon,
        OneHandedWeapon,
        Shield,
        RangedWeapon,
        Helmet,
        Circlet,
        FullArmor,
        ChestArmor,
        Barding,
        Boots,
        Trinket
    }
    public struct Equipment : IEquatable<Equipment>, IComponentData
    {
        public ushort Id;

        public Equipment(ushort id)
        {
            Id = id;
        }
        public bool Equals(Equipment other)
        {
            return Id == other.Id;
        }
        public override bool Equals(object obj)
        {
            return obj is Equipment other && Equals(other);
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        public static bool operator ==(Equipment left, Equipment right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(Equipment left, Equipment right)
        {
            return !left.Equals(right);
        }
    }

    // Equipment prototype used for creating new equipment entities
    public struct EquipmentPrototype
    {
        public string Name { get; set; }
        public EquipmentType Type { get; set; }
        public sbyte NumAttacks { get; set; }
        public sbyte DamageModifier { get; set; }
        public sbyte AttackModifier { get; set; }
        public sbyte DefenseModifier { get; set; }
    }

    /// <summary>
    /// Defines the types of equipment that can be equipped by units.
    /// Each equipment can only be of one type.
    /// </summary>
    public enum NaturalWeaponType
    {
        HeadWeapon,
        ArmWeapon,
        LegWeapon,
        TailWeapon
    }
    public struct NaturalWeapon : IEquatable<NaturalWeapon>, IComponentData
    {
        public ushort Id;

        public NaturalWeapon(ushort id)
        {
            Id = id;
        }
        public bool Equals(NaturalWeapon other)
        {
            return Id == other.Id;
        }
        public override bool Equals(object obj)
        {
            return obj is NaturalWeapon other && Equals(other);
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        public static bool operator ==(NaturalWeapon left, NaturalWeapon right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(NaturalWeapon left, NaturalWeapon right)
        {
            return !left.Equals(right);
        }
    }

    // Equipment prototype used for creating new equipment entities
    public struct NaturalWeaponPrototype
    {
        public string Name { get; set; }
        public NaturalWeaponType Type { get; set; }
        public sbyte NumAttacks { get; set; }
        public sbyte DamageModifier { get; set; }
        public sbyte AttackModifier { get; set; }
        public sbyte DefenseModifier { get; set; }
    }
}
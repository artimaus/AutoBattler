using System;
using System.Collections.Generic;

namespace AutoBattlerLib
{
    public enum ItemType
    {
        Helmet,
        Circlet,
        OneHanded,
        TwoHanded,
        Chest,
        Barding,
        Boots,
        Trinket
    }

    public struct MagicItem : IEquatable<MagicItem>, IComponentData
    {
        public ushort Id;

        public MagicItem(ushort id)
        {
            Id = id;
        }
        public bool Equals(MagicItem other)
        {
            return Id == other.Id;
        }
        public override bool Equals(object obj)
        {
            return obj is MagicItem other && Equals(other);
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        public static bool operator ==(MagicItem left, MagicItem right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(MagicItem left, MagicItem right)
        {
            return !left.Equals(right);
        }
    }

    public struct MagicItemPrototype
    {
        public string Name;
        public ItemType Slot { get; set; }
        public Armor ItemArmor { get; set; }
        public Weapon ItemWeapon { get; set; }
        //public EffectId EffectId { get; set; } // Optional effect ID for items that apply effects
    }

    /// <summary>
    /// Defines the types of equipment that can be equipped by units.
    /// Each equipment can only be of one type.
    /// </summary>
    public enum ArmorType
    {
        Head,
        Shield,
        FullBody, //Excludes Head, Wing, Tail
        Chest,
        Barding,
        Wing,
        Tail,
        Leg
    }

    public struct Armor : IEquatable<Armor>, IComponentData
    {
        public ushort Id;

        public Armor(ushort id)
        {
            Id = id;
        }
        public bool Equals(Armor other)
        {
            return Id == other.Id;
        }
        public override bool Equals(object obj)
        {
            return obj is Armor other && Equals(other);
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        public static bool operator ==(Armor left, Armor right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(Armor left, Armor right)
        {
            return !left.Equals(right);
        }
    }
    public struct NaturalArmor : IEquatable<NaturalArmor>, IComponentData
    {
        public ushort Id;

        public NaturalArmor(ushort id)
        {
            Id = id;
        }
        public bool Equals(NaturalArmor other)
        {
            return Id == other.Id;
        }
        public override bool Equals(object obj)
        {
            return obj is NaturalArmor other && Equals(other);
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        public static bool operator ==(NaturalArmor left, NaturalArmor right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(NaturalArmor left, NaturalArmor right)
        {
            return !left.Equals(right);
        }
    }

    // Equipment prototype used for creating new equipment entities
    public struct ArmorPrototype
    {
        public string Name { get; set; }
        public ArmorType Type { get; set; }
        public sbyte AttackModifier { get; set; }
        public sbyte DefenseModifier { get; set; }
        public sbyte ToughnessModifier { get; set; }
        public sbyte ExhaustionModifier { get; set; } // Exhaustion modifier for the armor, affects how quickly it can be used
        public sbyte SpeedModifier { get; set; } // Speed modifier for the armor, affects movement speed
        public sbyte ArmorResilience { get; set; }
    }

    /// <summary>
    /// Defines the types of equipment that can be equipped by units.
    /// Each equipment can only be of one type.
    /// </summary>
    public enum WeaponType
    {
        Head,
        Arm,
        TwoArm,
        Leg,
        Tail,
        Misc
    }
    public struct Weapon : IEquatable<Weapon>, IComponentData
    {
        public ushort Id;

        public Weapon(ushort id)
        {
            Id = id;
        }
        public bool Equals(Weapon other)
        {
            return Id == other.Id;
        }
        public override bool Equals(object obj)
        {
            return obj is Weapon other && Equals(other);
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        public static bool operator ==(Weapon left, Weapon right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(Weapon left, Weapon right)
        {
            return !left.Equals(right);
        }
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
    public struct WeaponPrototype
    {
        public string Name { get; set; }
        public WeaponType Type { get; set; }
        public sbyte Range { get; set; }
        public sbyte NumAttacks { get; set; }
        public sbyte DelayModifier { get; set; } // Delay modifier for the weapon, affects how quickly it can be used
        public sbyte DamageModifier { get; set; }
        public sbyte AttackModifier { get; set; }
        public sbyte DefenseModifier { get; set; }
        public sbyte WeaponResilience { get; set; }
        public sbyte WeaponLength { get; set; }
    }
}
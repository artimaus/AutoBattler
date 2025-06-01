using System;
using System.Collections.Generic;

namespace AutoBattlerLib
{
    /// <summary>
    /// Defines the types of equipment that can be equipped by units.
    /// Each equipment can only be of one type.
    /// </summary>
    public enum EquipmentType
    {
        NaturalWeapon,
        OneHandedWeapon,
        TwoHandedWeapon,
        Shield,
        RangedWeapon,
        Helmet,
        Circlet,
        ChestArmor,
        FullArmor,
        Barding,
        Boots,
        Greaves,
        Trinket
    }

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

    public struct Equipment
    {
        public int EquipmentPrototypeId { get; set; }
        public EquipmentState State { get; set; }
    }

    public class ItemComponent : IComponentData
    {
        public ref Equipment Equipment => ref _Equipment;
        private Equipment _Equipment; // Private backing field
    }

    // Equipment prototype used for creating new equipment entities
    public struct EquipmentPrototype
    {
        public string Name { get; set; }
        public EquipmentType Type { get; set; }
        public int NumAttacks { get; set; } = -1;
        public int DamageModifier { get; set; }
        public int AttackModifier { get; set; }
        public int DefenseModifier { get; set; }

        public EquipmentPrototype(string name, EquipmentType type, int numAttacks, int damageModifier, int attackModifier, int defenseModifier)
        {
            Name = name;
            Type = type;
            NumAttacks = numAttacks;
            DamageModifier = damageModifier;
            AttackModifier = attackModifier;
            DefenseModifier = defenseModifier;
        }
    }
}
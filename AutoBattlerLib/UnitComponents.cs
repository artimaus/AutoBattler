using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace AutoBattlerLib
{
    public enum ResistanceType
    {
        Physical,
        Magical,
        Mental,
        Fire,
        Ice,
        Lightning,
        Poison
    }

    [Flags]
    public enum BodyPartState
    {
        Healthy = 0,
        Damaged = 1 << 0,   // 1
        Crippled = 1 << 1,  // 2
        Gone = 1 << 2,      // 4
        Scarred = 1 << 3    // 8
    }

    public enum BodyPartType
    {
        HeadWithNatWeapon,
        Head,
        ArmWithNatWeapon,
        Arm,
        Legs,
        LegsWithNatWeapon,
        Chest,
        BeastTorso,
        Wing,
        Tail,
        TailLegs,
        TrinketSlot
    }

    public class BodyPartComponent : IComponentData
    {
        BodyPartType type;
        HashSet<FormId> validForm;
        byte subParts; // Number of sub-parts, e.g. number of fingers on a hand
    }

    public class ProficienciesComponent : IComponentData
    {
        public ushort StrikingXP { get; set; }
        public ushort ParryingXP { get; set; }
        public ushort EvasionXP { get; set; }
        public ushort BlockingXP { get; set; }
        public ushort AthleticXP { get; set; }
        public byte StrikingSkill { get; set; }
        public byte ParryingSkill { get; set; }
        public byte EvasionSkill { get; set; }
        public byte BlockingSkill { get; set; }
        public byte AthleticSkill{ get; set; }
    }

    /// <summary>
    /// Identifies an entity as a unit with references to its forms
    /// </summary>
    public class UnitComponent : IComponentData
    {
        public string Name { get; set; } // needs work
        public FormId CurrentForm { get; set; } // List of forms this unit can take
        public EquipmentId[] Loadout { get; set; } // Equipment loadout for the unit

    }

    // Prototypes for creating new entities
    public struct UnitPrototype
    {
        public string Name { get; set; } // May not be needed
        public FormId defaultForm { get; set; }
        public LoadoutPrototypeId Loadout { get; set; } // Loadout prototype ID for the unit
        public int Command { get; set; } = -1;
        public int MoraleModifier { get; set; } = 0;

        public UnitPrototype(string name, List<int> formPrototypeIds)
        {
            Name = name;
            FormPrototypeIds = formPrototypeIds;
        }

        public UnitPrototype(string name, List<int> formPrototypeIds, int command, int moraleModifier) : this(name, formPrototypeIds)
        {
            Command = command;
            MoraleModifier = moraleModifier;
        }
    }

    public struct FormId : IEquatable<FormId>
    {
        public ushort Id;

        public FormId(ushort id)
        {
            Id = id;
        }
        public bool Equals(FormId other)
        {
            return Id == other.Id;
        }
        public override bool Equals(object obj)
        {
            return obj is FormId other && Equals(other);
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        public static bool operator ==(FormId left, FormId right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(FormId left, FormId right)
        {
            return !left.Equals(right);
        }
    }

    public struct FormPrototype
    {
        public BodyPrototypeId Body { get; set; }
        public AttributesId Attributes { get; set; }
    }

    public enum TransitionType
    {
        Voluntary,
        PermanentDeath,
        TemporaryDeath
    }

    public struct FormTransition
    {
        TransitionType type { get; set; }
        FormId newFormId { get; set; } // The form to transition to
    }

    public struct AttributesId : IEquatable<AttributesId>
    {
        public ushort Id;

        public AttributesId(ushort id)
        {
            Id = id;
        }
        public bool Equals(AttributesId other)
        {
            return Id == other.Id;
        }
        public override bool Equals(object obj)
        {
            return obj is AttributesId other && Equals(other);
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        public static bool operator ==(AttributesId left, AttributesId right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(AttributesId left, AttributesId right)
        {
            return !left.Equals(right);
        }
    }

    public readonly struct AttributesPrototype
    {
        private readonly byte Size;
        private readonly byte Strength;
        private readonly byte Dexterity;
        private readonly byte Agility;
        private readonly byte Stamina;
        private readonly byte Toughness;
        private readonly byte Will;
        private readonly byte Constitution;
    }

    public struct BodyPrototypeId : IEquatable<BodyPrototypeId>
    {
        public int Id;

        public BodyPrototypeId(ushort id)
        {
            Id = id;
        }
        public bool Equals(BodyPrototypeId other)
        {
            return Id == other.Id;
        }
        public override bool Equals(object obj)
        {
            return obj is BodyPrototypeId other && Equals(other);
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        public static bool operator ==(BodyPrototypeId left, BodyPrototypeId right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(BodyPrototypeId left, BodyPrototypeId right)
        {
            return !left.Equals(right);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BodyPrototype
    {
        public byte Heads { get; set; }
        public byte HeadsWithNatWeapon { get; set; }
        public byte Arms { get; set; }
        public byte ArmsWithNatWeapon { get; set; }
        public byte Legs { get; set; }
        public byte LegsWithNatWeapon { get; set; }
        public byte Wings { get; set; }
        public byte Tails { get; set; }
        public byte TailLegs { get; set; }
        public byte TrinketSlots { get; set; }
        public byte EyesPerHead { get; set; }
        public bool HasChest { get; set; }
        public bool HasBeastTorso { get; set; }
    }

    public struct LoadoutPrototypeId : IEquatable<LoadoutPrototypeId>
    {
        public ushort Id;

        public LoadoutPrototypeId(ushort id)
        {
            Id = id;
        }
        public bool Equals(LoadoutPrototypeId other)
        {
            return Id == other.Id;
        }
        public override bool Equals(object obj)
        {
            return obj is LoadoutPrototypeId other && Equals(other);
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        public static bool operator ==(LoadoutPrototypeId left, LoadoutPrototypeId right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(LoadoutPrototypeId left, LoadoutPrototypeId right)
        {
            return !left.Equals(right);
        }
    }
}
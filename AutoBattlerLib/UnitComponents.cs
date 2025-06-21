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

    public class ExperienceComponent : IComponentData
    {
        public ushort StrikingXP { get; set; }
        public ushort ParryingXP { get; set; }
        public ushort EvasionXP { get; set; }
        public ushort BlockingXP { get; set; }
        public ushort AthleticXP { get; set; }
    }

    public readonly struct DrillingPrototype
    {
        public readonly ushort BaseStrikingXP;
        public readonly ushort BaseParryingXP;
        public readonly ushort BaseEvasionXP;
        public readonly ushort BaseBlockingXP;
        public readonly ushort BaseAthleticXP;
    }

    public readonly struct ProficienciesPrototype
    {
        public readonly byte BaseStrikingSkill;
        public readonly byte BaseParryingSkill;
        public readonly byte BaseEvasionSkill;
        public readonly byte BaseBlockingSkill;
        public readonly byte BaseAthleticSkill;
    }

    public struct CommanderComponent : IEquatable<CommanderComponent>, IComponentData
    {
        public ushort Id;

        public CommanderComponent(ushort id)
        {
            Id = id;
        }
        public bool Equals(CommanderComponent other)
        {
            return Id == other.Id;
        }
        public override bool Equals(object obj)
        {
            return obj is CommanderComponent other && Equals(other);
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        public static bool operator ==(CommanderComponent left, CommanderComponent right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(CommanderComponent left, CommanderComponent right)
        {
            return !left.Equals(right);
        }
    }

    public struct CommanderAttributes
    {
        public readonly byte Command;
        public readonly byte MoraleModifier;
    }

    public struct UnitComponent : IEquatable<UnitComponent>, IComponentData
    {
        public ushort Id;

        public UnitComponent(ushort id)
        {
            Id = id;
        }
        public bool Equals(UnitComponent other)
        {
            return Id == other.Id;
        }
        public override bool Equals(object obj)
        {
            return obj is UnitComponent other && Equals(other);
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        public static bool operator ==(UnitComponent left, UnitComponent right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(UnitComponent left, UnitComponent right)
        {
            return !left.Equals(right);
        }
    }

    // Prototypes for creating new entities
    public struct UnitPrototypes
    {
        public string[] Name; // May not be needed
        public FormComponent[] firstForm; // Always needed
        public LoadoutPrototypeId[] Loadout; // Loadout prototype ID for the unit, May not be needed
    }

    public struct FormComponent : IEquatable<FormComponent>, IComponentData
    {
        public ushort Id;

        public FormComponent(ushort id)
        {
            Id = id;
        }
        public bool Equals(FormComponent other)
        {
            return Id == other.Id;
        }
        public override bool Equals(object obj)
        {
            return obj is FormComponent other && Equals(other);
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        public static bool operator ==(FormComponent left, FormComponent right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(FormComponent left, FormComponent right)
        {
            return !left.Equals(right);
        }
    }

    public struct FormPrototypes
    {
        public readonly string[] Name;
        public readonly BodyPrototypeId[] Body;
        public readonly AttributesId[] Attributes;
    }

    public enum TransitionType
    {
        Arbitrary,
        PermanentDeath,
        TemporaryDeath,
        TemporaryDeathRevert
    }

    public enum AttributeType
    {
        Size,
        Strength,
        Dexterity,
        Agility,
        Celerity,
        Vigor,
        Toughness,
        Will,
        Constitution
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
        public readonly byte BaseSize;
        public readonly byte BaseStrength;
        public readonly byte BaseDexterity;
        public readonly byte BaseAgility;
        public readonly byte BaseCelerity; // Reflexive or striking speed
        public readonly byte BaseVigor;
        public readonly byte BaseToughness;
        public readonly byte BaseWill;
        public readonly byte BaseConstitution;
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

    [Flags]
    public enum BodyFlags : byte
    {
        None = 0,
        BootSlot = 1 << 0,      // 1
        HasChest = 1 << 1,      // 2
        ChestSlot = 1 << 2,     // 4
        HasBeastTorso = 1 << 3, // 8
        BardingSlot = 1 << 4    // 16
                                // 3 bits remaining for future use
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BodyPrototype
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

    public enum BodyPartType
    {
        Head,
        Arm,
        Leg,
        Wing,
        Tail,
        Chest,
        BeastTorso
    }

    public enum BodySlotType
    {
        Head,
        CircletOnly,
        Arm,
        Boots,
        Chest,
        Barding,
        Trinket
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
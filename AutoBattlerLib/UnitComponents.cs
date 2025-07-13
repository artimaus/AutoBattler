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

    public struct ExperienceComponent : IComponentData
    {
        public ushort StrikingXP { get; set; }
        public ushort ParryingXP { get; set; }
        public ushort EvasionXP { get; set; }
        public ushort BlockingXP { get; set; }
        public ushort AthleticXP { get; set; }
    }

    public struct TrainingId : IEquatable<TrainingId>
    {
        public ushort Id;

        public TrainingId(ushort id)
        {
            Id = id;
        }
        public bool Equals(TrainingId other)
        {
            return Id == other.Id;
        }
        public override bool Equals(object obj)
        {
            return obj is TrainingId other && Equals(other);
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        public static bool operator ==(TrainingId left, TrainingId right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(TrainingId left, TrainingId right)
        {
            return !left.Equals(right);
        }
    }

    public readonly struct TrainingPrototypes
    {
        public readonly ushort[] BaseStrikingXP;
        public readonly ushort[] BaseParryingXP;
        public readonly ushort[] BaseEvasionXP;
        public readonly ushort[] BaseBlockingXP;
        public readonly ushort[] BaseAthleticXP;
    }

    public struct ProficiencyId : IEquatable<ProficiencyId>
    {
        public ushort Id;

        public ProficiencyId(ushort id)
        {
            Id = id;
        }
        public bool Equals(ProficiencyId other)
        {
            return Id == other.Id;
        }
        public override bool Equals(object obj)
        {
            return obj is ProficiencyId other && Equals(other);
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        public static bool operator ==(ProficiencyId left, ProficiencyId right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(ProficiencyId left, ProficiencyId right)
        {
            return !left.Equals(right);
        }
    }

    public readonly struct ProficiencyPrototypes
    {
        public readonly byte[] BaseStrikingSkill;
        public readonly byte[] BaseParryingSkill;
        public readonly byte[] BaseEvasionSkill;
        public readonly byte[] BaseBlockingSkill;
        public readonly byte[] BaseAthleticSkill;
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

    public struct CommanderAttributeProtoypes
    {
        public readonly byte[] Command;
        public readonly byte[] MoraleModifier;
    }

    public struct UnitComponent : IEquatable<UnitComponent>, IComparable<UnitComponent>, IComponentData
    {
        public ushort Id;

        public UnitComponent(ushort id)
        {
            Id = id;
        }

        public int CompareTo(UnitComponent other)
        {
            return Id.CompareTo(other.Id);
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
        public FormComponent[] FirstForm; // Always needed
        public LoadoutPrototypeId[] Loadout; // Optional, can be 0
    }

    public struct FormComponent : IEquatable<FormComponent>, IComparable<FormComponent>, IComponentData
    {
        public ushort Id;

        public FormComponent(ushort id)
        {
            Id = id;
        }

        public int CompareTo(FormComponent other)
        {
            return Id.CompareTo(other.Id);
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
        public string[] Name;
        public BodyPrototypeId[] Body;
        public AttributesId[] Attributes;
        public FormLoadoutPrototypeId[] Loadout; // Optional, can be 0
    }

    public struct FormLoadoutPrototypeId : IEquatable<FormLoadoutPrototypeId>
    {
        public ushort Id;

        public FormLoadoutPrototypeId(ushort id)
        {
            Id = id;
        }
        public bool Equals(FormLoadoutPrototypeId other)
        {
            return Id == other.Id;
        }
        public override bool Equals(object obj)
        {
            return obj is FormLoadoutPrototypeId other && Equals(other);
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        public static bool operator ==(FormLoadoutPrototypeId left, FormLoadoutPrototypeId right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(FormLoadoutPrototypeId left, FormLoadoutPrototypeId right)
        {
            return !left.Equals(right);
        }
    }

    public struct FormLoadoutPrototypes
    {
        public byte[] natWeaponNum;
        public NaturalWeapon[] natWeaponStartIndex;
        public byte[] natArmorNum;
        public NaturalArmor[] natArmorStartIndex;
    }

    public enum TransitionType
    {
        OnDeath,
        OnBattleStart,
        OnBattleEnd,
        OnCommand
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

    public struct AttributesPrototypes
    {
        public byte[] BaseSize;
        public byte[] BaseStrength;
        public byte[] BaseDexterity;
        public byte[] BaseAgility;
        public byte[] BaseCelerity; // Reflexive or striking speed
        public byte[] BaseVigor;
        public byte[] BaseToughness;
        public byte[] BaseWill;
        public byte[] BaseConstitution;
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
    public enum BodyPrototypeEncodings
    {
        None = 0,
        HasChest = 1 << 0, // Has a chest slot
        ChestSlot = 1 << 1, // Chest slot is used
        HasBeastTorso = 1 << 2, // Has a beast torso slot
        BardingSlot = 1 << 3, // Barding slot is used
        BootSlot = 1 << 4 // Boot slot is used
        // 3 more bits can be used for future expansions
    }

    public struct BodyPrototypes
    {
        public byte[] Heads;
        public byte[] EyesPerHead;
        public byte[] HeadSlots; // HeadSlots + CircletOnlySLots should never be more than Heads
        public byte[] CircletOnlySlots; //Circlets don't overwrite any natural weapons
        public byte[] Arms;
        public byte[] ArmSlots; // ArmSlots should never be more than Arms
        public byte[] Legs;
        public byte[] Wings;
        public byte[] Tails;
        public byte[] TrinketSlots;
        public BodyPrototypeEncodings[] Encodings; // Encodings for the body prototype
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

    public struct LoadoutPrototypes
    {
        public byte[] weaponNum;
        public Weapon[] weaponStartIndex;
        public byte[] armorNum;
        public Armor[] armorStartIndex;
    }
}
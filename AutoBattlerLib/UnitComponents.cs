using System;
using System.Collections.Generic;

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
    public enum BodyPartBlights
    {
        Healthy = 0,
        Damaged = 1 << 0,   // 1
        Crippled = 1 << 1,  // 2
        Gone = 1 << 2,      // 4
        Scarred = 1 << 3    // 8
    }

    public enum BodyPartType
    {
        Head,
        Eye,
        Arm,
        LowerBody,
        Leg,
        Chest,
        BeastTorso,
        Wing,
        Tail,
        TrinketSlot
    }

    public class FormComponent : IComponentData
    {
        public AttributesId Attributes { get; set; }
        public FormId Form { get; set; }
    }

    public class BodyComponent : IComponentData
    {
        BodyPartType type;
        BodyPartBlights[] blights;
    }
    public class ProficienciesComponent : IComponentData
    {
        // Proficiencies for this unit
        public int StrikingSkill { get; set; }
        public int StrikingXP { get; set; }
        public int ParryingSkill { get; set; }
        public int ParryingXP { get; set; }
        public int EvasionSkill { get; set; }
        public int EvasionXP { get; set; }
        public int BlockingSkill { get; set; }
        public int BlockingXP { get; set; }
        public int Athleticism { get; set; }
        public int AthleticismXP { get; set; }
    }

    /// <summary>
    /// Identifies an entity as a unit with references to its forms
    /// </summary>
    public class UnitComponent : IComponentData
    {
        public Component Forms { get; set; }
        public string Name { get; set; } // needs work
    }

    // Prototypes for creating new entities
    public struct UnitPrototype
    {
        public string Name { get; set; } // May not be needed
        public FormId defaultForm { get; set; }

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
        public int Id;

        public FormId(int id)
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
        public string Name { get; set; }
        public BodyPrototypeId Body { get; set; }
        public AttributesId Attributes { get; set; }

        public FormPrototype(string name, int bodyPrototypeId, int attributesPrototypeId, int loadoutPrototypeId)
        {
            Name = name;
            BodyPrototypeId = bodyPrototypeId;
            AttributesId = attributesPrototypeId;
            LoadoutPrototypeID = loadoutPrototypeId;
        }
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
        public int Id;

        public AttributesId(int id)
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
        private readonly int Size;
        private readonly int Strength;
        private readonly int Dexterity;
        private readonly int Agility;
        private readonly int Stamina;
        private readonly int Toughness;
        private readonly int Will;
        private readonly int Constitution;

        public int GetSize(AttributesId id) => Size[id.Id];
        public int GetStrength(AttributesId id) => Strength[id.Id];
        public int GetDexterity(AttributesId id) => Dexterity[id.Id];
        public int GetAgility(AttributesId id) => Agility[id.Id];
        public int GetStamina(AttributesId id) => Stamina[id.Id];
        public int GetToughness(AttributesId id) => Toughness[id.Id];
        public int GetWill(AttributesId id) => Will[id.Id];
        public int GetConstitution(AttributesId id) => Constitution[id.Id];


    }

    public struct BodyPrototypeId : IEquatable<BodyPrototypeId>
    {
        public int Id;

        public BodyPrototypeId(int id)
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

    public struct BodyPrototype
    {
        public string Name { get; set; }
        public int Heads { get; set; }
        public int Arms { get; set; }
        public int Legs { get; set; }
        public int HasChest { get; set; }
        public int HasBeastTorso { get; set; }
        public int Wings { get; set; }
        public int Tails { get; set; }
        public int TrinketSlots { get; set; }
        public int EyesPerHead { get; set; } // when loading BodyPrototypes in, make sure that this is at least zero if heads > 0
    }

    public struct LoadoutPrototypeId : IEquatable<LoadoutPrototypeId>
    {
        public int Id;

        public LoadoutPrototypeId(int id)
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

    public struct LoadoutPrototype
    {
        public string Name { get; set; }
        // Equipment mappings for different body parts
        public List<int> DefaultEquipmentIds { get; set; } = new List<int>();

        public LoadoutPrototype(
            string name = "",
            List<int> defaultEquipmentIds = null)
        {
            Name = name;
            DefaultEquipmentIds = defaultEquipmentIds ?? new List<int>();
        }
    }
}
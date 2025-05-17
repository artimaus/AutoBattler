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
        Head,
        Arm,
        LowerBody,
        Chest,
        BeastTorso,
        Wing,
        Tail,
        TrinketSlot
    }

    public class FormComponent : IComponent
    {
        public bool Alive { get; set; } = true;
        public int AttributesId { get; set; } = -1;

        public int FormPrototypeId { get; set; } = -1;
    }

    public class BodyPartComponent : IComponent
    {
        public BodyPartType Type { get; set; }
        public List<BodyPartState> State { get; set; } = new List<BodyPartState>(); // element zero is always the state of the main bodypart and subsequent elements are subparts
        public ref Equipment DefaultEquipment => ref _defaultEquipment;
        private Equipment _defaultEquipment; // Private backing field

    }

    /// <summary>
    /// A component to store leader statistics
    /// </summary>
    public class LeaderStatComponent : IComponent
    {
        // Leadership capabilities
        public int Command { get; set; }
        public int MoraleModifier { get; set; }
    }

    public class ProficienciesComponent : IComponent
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
    public class UnitComponent : IComponent
    {
        public EntityId currentForm { get; set; }
        public string Name { get; set; } // needs work
    }

    // Prototypes for creating new entities
    public struct UnitPrototype
    {
        public string Name { get; set; } // May not be needed
        public List<int> FormPrototypeIds { get; set; } = new List<int>();

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

    public struct FormPrototype
    {
        public string Name { get; set; }
        public int BodyPrototypeId { get; set; }
        public int AttributesId { get; set; }
        public int LoadoutPrototypeID { get; set; }

        public FormPrototype(string name, int bodyPrototypeId, int attributesPrototypeId, int loadoutPrototypeId)
        {
            Name = name;
            BodyPrototypeId = bodyPrototypeId;
            AttributesId = attributesPrototypeId;
            LoadoutPrototypeID = loadoutPrototypeId;
        }
    }

    public struct Attributes
    {
        public int Size { get; set; }
        public int Strength { get; set; }
        public int Dexterity { get; set; }
        public int Agility { get; set; }
        public int Stamina { get; set; }
        public int Toughness { get; set; }
        public int Will { get; set; }
        public int Constitution { get; set; }

        public Attributes(
            int size = 4,
            int strength = 10,
            int dexterity = 10,
            int agility = 10,
            int stamina = 4,
            int toughness = 10,
            int will = 10,
            int constitution = 10)
        {
            Size = size;
            Strength = strength;
            Dexterity = dexterity;
            Agility = agility;
            Stamina = stamina;
            Toughness = toughness;
            Will = will;
            Constitution = constitution;
        }
    }

    public struct BodyPrototype
    {
        public string Name { get; set; }
        public int Heads { get; set; }
        public int Arms { get; set; }
        public int Legs { get; set; } = 2;
        public bool HasChest { get; set; }
        public bool HasBeastTorso { get; set; }
        public int Wings { get; set; } = 2;
        public int Tails { get; set; }
        public int TrinketSlots { get; set; }
        public int EyesPerHead { get; set; } = 1;

        public BodyPrototype(string name, int heads, int arms, int legs, bool hasChest, bool hasBeastTorso,
                             int wings, int tails, int trinketSlots, int eyesPerHead)
        {
            Name = name;
            Heads = heads;
            Arms = arms;
            Legs = legs;
            HasChest = hasChest;
            HasBeastTorso = hasBeastTorso;
            Wings = wings;
            Tails = tails;
            TrinketSlots = trinketSlots;
            EyesPerHead = eyesPerHead;
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
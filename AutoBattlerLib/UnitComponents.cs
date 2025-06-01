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

    public class FormComponent : IComponentData
    {
        public bool Alive { get; set; } = true;
        public int AttributesId { get; set; } = -1;

        public int FormPrototypeId { get; set; } = -1;
    }

    public interface BodyPartComponent : IComponentData
    {
        public BodyPartState[] State { get; set; } // element zero is always the state of the main bodypart and subsequent elements are subparts
    }

    /// <summary>
    /// A component to store leader statistics
    /// </summary>
    public class LeaderStatComponent : IComponentData
    {
        // Leadership capabilities
        public int Command { get; set; }
        public int MoraleModifier { get; set; }
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
        public int[] Forms { get; set; }
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

    public enum AttributeType
    {
        Size,
        Strength,
        Dexterity,
        Agility,
        Stamina,
        Toughness,
        Will,
        Constitution
    }

    public struct BodyPrototypes
    {
        public Dictionary<int, string> Name { get; set; }
        public Dictionary<int, int> Heads { get; set; }
        public Dictionary<int, int> Arms { get; set; }
        public Dictionary<int, int> Legs { get; set; }
        public HashSet<int> HasChest { get; set; }
        public HashSet<int> HasBeastTorso { get; set; }
        public Dictionary<int, int> Wings { get; set; }
        public Dictionary<int, int> Tails { get; set; }
        public Dictionary<int, int> TrinketSlots { get; set; }
        public Dictionary<int, int> EyesPerHead { get; set; } // when loading BodyPrototypes in, make sure that this is at least zero if heads > 0
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
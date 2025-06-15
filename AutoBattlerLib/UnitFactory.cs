using System;
using System.Collections.Generic;

namespace AutoBattlerLib
{
    /// <summary>
    /// Factory for creating complete unit entities with all required components
    /// Following ECS principles, focused on modularity and flexibility for modding
    /// </summary>
    public class UnitFactory
    {
        private readonly EntityManager _EntityManager;
        private readonly ComponentManager _ComponentManager;

        public UnitFactory(EntityManager entityManager, ComponentManager componentManager)
        {
            _EntityManager = entityManager;
            _ComponentManager = componentManager;
        }

        /// <summary>
        /// Creates a complete unit entity from a prototype
        /// </summary>
        /// <param name="prototype">The unit prototype to create from</param>
        /// <returns>The created entity</returns>
        public Entity CreateUnit(UnitComponent unitComp)
        {
            if (!(Prototypes.unitPrototypes.firstForm.Length < unitComp.Id))
            {
                return default;
            }
            var formId = Prototypes.unitPrototypes.firstForm[unitComp.Id];
            var entity = _EntityManager.CreateEntity();
            _ComponentManager.AddNewComponentToEntity(entity, unitComp, ComponentType.Unit);
            _ComponentManager.AddNewComponentToEntity(entity, formId, ComponentType.Form);
            AddProficienciesComponent(entity, unitComp);
            if (!(Prototypes.unitPrototypes.Loadout[unitComp.Id].Id == 0))
            {
                AddEquipmentFromLoadout(entity, Prototypes.unitPrototypes.Loadout[unitComp.Id]);
            }
            return entity;
        }

        public Entity CreateCommander(UnitComponent prototypeId)
        {
            if (!Prototypes.unitCommand.TryGetValue(prototypeId, out var commandId))
            {
                return default;
            }
            var entity = CreateUnit(prototypeId);
            _ComponentManager.AddNewComponentToEntity(entity, commandId, ComponentType.Commander);
            return entity;
        }

        ///// <summary>
        ///// Transitions a unit to a new form
        ///// </summary>
        ///// <param name="entity">The unit entity</param>
        ///// <param name="newFormId">The new form to transition to</param>
        ///// <param name="transitionType">The type of transition</param>
        ///// <returns>True if transition was successful</returns>
        //public bool TransitionUnitForm(Entity entity, FormId newFormId, TransitionType transitionType)
        //{
        //    if (!entityManager.EntityExists(entity))
        //        return false;

        //    // Verify the transition is valid
        //    if (!IsValidTransition(entity, newFormId, transitionType))
        //        return false;

        //    // Remove form-specific components
        //    RemoveFormSpecificComponents(entity);

        //    // Update the unit's current form
        //    if (componentManager.TryGetComponentDataOfType(entity, ComponentType.Form, out var unitComponents))
        //    {
        //        foreach (var component in unitComponents)
        //        {
        //            if (component is UnitComponent unitData)
        //            {
        //                unitData.CurrentForm = newFormId;
        //                break;
        //            }
        //        }
        //    }

        //    // Add new form-based components
        //    AddFormBasedComponents(entity, newFormId);

        //    return true;
        //}


        /// <summary>
        /// Adds proficiencies component with default values
        /// </summary>
        private void AddProficienciesComponent(Entity entity, UnitComponent unitId)
        {
            ProficienciesComponent proficiencies;
            if (Prototypes.defaultProficiencies.TryGetValue(unitId, out var profId))
            {
                proficiencies = new ProficienciesComponent
                {
                    StrikingXP = profId.StrikingXP,
                    ParryingXP = profId.ParryingXP,
                    EvasionXP = profId.EvasionXP,
                    BlockingXP = profId.BlockingXP,
                    AthleticXP = profId.AthleticXP,
                    StrikingSkill = profId.StrikingSkill,
                    ParryingSkill = profId.ParryingSkill,
                    EvasionSkill = profId.EvasionSkill,
                    BlockingSkill = profId.BlockingSkill,
                    AthleticSkill = profId.AthleticSkill
                };
            }
            else
            {
                proficiencies = new ProficienciesComponent
                {
                    StrikingXP = 0,
                    ParryingXP = 0,
                    EvasionXP = 0,
                    BlockingXP = 0,
                    AthleticXP = 0,
                    StrikingSkill = 0,
                    ParryingSkill = 0,
                    EvasionSkill = 0,
                    BlockingSkill = 0,
                    AthleticSkill = 0
                };
            }
            var component = _ComponentManager.CreateComponent(ComponentType.Proficiencies);
            _ComponentManager.AddNewComponentToEntity(entity, proficiencies, ComponentType.Proficiencies);
        }

        private void AddBodyPartComponents(Entity entity, FormComponent form)
        {
            BodyPrototypeId bodyId = Prototypes.formPrototypes.Body[form.Id];
            BodyPrototype body = Prototypes.bodyPrototypes[bodyId.Id];
            BodyPartComponent part;
            BodySlotComponent slot;
            Component partCompId;
            for (int i = 0; i < body.Heads; i++)
            {
                part = new BodyPartComponent();
                part.Type = BodyPartType.Head;
                part.Id = bodyId;
                partCompId = _ComponentManager.AddNewComponentToEntity(entity, part, ComponentType.BodyPart);
                if (i < body.HeadSlots)
                {
                    slot = new BodySlotComponent();
                    slot.Type = BodySlotType.Head;
                    slot.Part = partCompId;
                    _ComponentManager.AddNewComponentToEntity(entity, slot, ComponentType.BodySlot);
                }
                else if (i < body.CircletOnlySlots + body.HeadSlots)
                {
                    slot = new BodySlotComponent();
                    slot.Type = BodySlotType.CircletOnly;
                    slot.Part = partCompId;
                    _ComponentManager.AddNewComponentToEntity(entity, slot, ComponentType.BodySlot);
                }
            }
            for (int i = 0; i < body.Arms; i++)
            {
                part = new BodyPartComponent();
                part.Type = BodyPartType.Arm;
                part.Id = bodyId;
                partCompId = _ComponentManager.AddNewComponentToEntity(entity, part, ComponentType.BodyPart);
                if (i < body.ArmSlots)
                {
                    slot = new BodySlotComponent();
                    slot.Type = BodySlotType.Arm;
                    slot.Part = partCompId;
                    _ComponentManager.AddNewComponentToEntity(entity, slot, ComponentType.BodySlot);
                }
            }
            for (int i = 0; i < body.Legs; i++)
            {
                part = new BodyPartComponent();
                part.Type = BodyPartType.Leg;
                part.Id = bodyId;
                partCompId = _ComponentManager.AddNewComponentToEntity(entity, part, ComponentType.BodyPart);
                if (i == 0 && (body.Flags & BodyFlags.BootSlot) != 0)
                {
                    slot = new BodySlotComponent();
                    slot.Type = BodySlotType.Boots;
                    slot.Part = partCompId;
                    _ComponentManager.AddNewComponentToEntity(entity, slot, ComponentType.BodySlot);
                }
            }
            for (int i = 0; i < body.Wings; i++)
            {
                part = new BodyPartComponent();
                part.Type = BodyPartType.Wing;
                part.Id = bodyId;
                _ComponentManager.AddNewComponentToEntity(entity, part, ComponentType.BodyPart);
            }
            for (int i = 0; i < body.Tails; i++)
            {
                part = new BodyPartComponent();
                part.Type = BodyPartType.Tail;
                part.Id = bodyId;
                _ComponentManager.AddNewComponentToEntity(entity, part, ComponentType.BodyPart);
            }
            for (int i = 0; i < body.TrinketSlots; i++)
            {
                slot = new BodySlotComponent();
                slot.Type = BodySlotType.Trinket;
                slot.Part = new Component(-1);
                _ComponentManager.AddNewComponentToEntity(entity, slot, ComponentType.BodySlot);
            }
            if ((body.Flags & BodyFlags.HasChest) != 0)
            {
                part = new BodyPartComponent();
                part.Type = BodyPartType.Chest;
                part.Id = bodyId;
                partCompId = _ComponentManager.AddNewComponentToEntity(entity, part, ComponentType.BodyPart);
                if ((body.Flags & BodyFlags.ChestSlot) != 0)
                {
                    slot = new BodySlotComponent();
                    slot.Type = BodySlotType.Chest;
                    slot.Part = partCompId;
                    _ComponentManager.AddNewComponentToEntity(entity, slot, ComponentType.BodySlot);
                }
            }
            if ((body.Flags & BodyFlags.HasBeastTorso) != 0)
            {
                part = new BodyPartComponent();
                part.Type = BodyPartType.BeastTorso;
                part.Id = bodyId;
                partCompId = _ComponentManager.AddNewComponentToEntity(entity, part, ComponentType.BodyPart);
                if ((body.Flags & BodyFlags.BardingSlot) != 0)
                {
                    slot = new BodySlotComponent();
                    slot.Type = BodySlotType.Barding;
                    slot.Part = partCompId;
                    _ComponentManager.AddNewComponentToEntity(entity, slot, ComponentType.BodySlot);
                }
            }

            foreach (NaturalWeapon n in Prototypes.formWeapons[form])
            {
                _ComponentManager.AddNewComponentToEntity(entity, n, ComponentType.NaturalWeapon);
            }
        }

        public void AddEquipmentFromLoadout(Entity entity, LoadoutPrototypeId loadout)
        {
            foreach (Equipment e in Prototypes.loadoutPrototypes[loadout])
            {
                _ComponentManager.AddNewComponentToEntity(entity, e, ComponentType.Equipment);
            }
        }
    }
}
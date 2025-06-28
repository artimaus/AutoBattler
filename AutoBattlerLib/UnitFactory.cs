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
            if (!(Prototypes.unitPrototypes.FirstForm.Length < unitComp.Id))
            {
                return default;
            }
            var formId = Prototypes.unitPrototypes.FirstForm[unitComp.Id];
            var entity = _EntityManager.CreateEntity();
            _ComponentManager.AddNewComponentToEntity(entity, unitComp, ComponentType.Unit);
            _ComponentManager.AddNewComponentToEntity(entity, formId, ComponentType.Form);
            AddExperienceComponent(entity, unitComp);
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
        private void AddExperienceComponent(Entity entity, UnitComponent unitId)
        {
            ExperienceComponent experience;
            if (Prototypes.defaultDrilling.TryGetValue(unitId, out var drillId))
            {
                experience = new ExperienceComponent
                {
                    StrikingXP = drillId.BaseStrikingXP,
                    ParryingXP = drillId.BaseParryingXP,
                    EvasionXP = drillId.BaseEvasionXP,
                    BlockingXP = drillId.BaseBlockingXP,
                    AthleticXP = drillId.BaseAthleticXP
                };
            }
            else
            {
                experience = new ExperienceComponent
                {
                    StrikingXP = 0,
                    ParryingXP = 0,
                    EvasionXP = 0,
                    BlockingXP = 0,
                    AthleticXP = 0
                };
            }
            _ComponentManager.AddNewComponentToEntity(entity, experience, ComponentType.Experience);
        }

        public void AddEquipmentFromLoadout(Entity entity, LoadoutPrototypeId loadout)
        {
            foreach (Armor e in Prototypes.loadoutArmor[loadout])
            {
                _ComponentManager.AddNewComponentToEntity(entity, e, ComponentType.Armor);
            }
            foreach (Weapon e in Prototypes.loadoutWeapons[loadout])
            {
                _ComponentManager.AddNewComponentToEntity(entity, e, ComponentType.Weapon);
            }
        }
    }
}
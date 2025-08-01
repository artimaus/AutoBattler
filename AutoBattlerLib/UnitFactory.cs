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
        private readonly EntityComponentManager _EntityComponentManager;

        public UnitFactory(EntityComponentManager entityComponentManager)
        {
            _EntityComponentManager = entityComponentManager;
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
            var entity = _EntityComponentManager.CreateEntity();
            _EntityComponentManager.AttachComponentToEntity(entity, unitComp);
            _EntityComponentManager.AttachComponentToEntity(entity, formId);
            AddExperienceComponent(entity, unitComp);
            return entity;
        }

        public Entity CreateCommander(UnitComponent prototypeId)
        {
            if (!Prototypes.unitCommand.TryGetValue(prototypeId, out var commandId))
            {
                return default;
            }
            var entity = CreateUnit(prototypeId);
            _EntityComponentManager.AttachComponentToEntity(entity, commandId);
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
            Prototypes.defaultTraining.TryGetValue(unitId, out var trainingId);
            ExperienceComponent experience = new ExperienceComponent
            {
                StrikingXP = Prototypes.trainingPrototypes.BaseStrikingXP[trainingId.Id],
                ParryingXP = Prototypes.trainingPrototypes.BaseParryingXP[trainingId.Id],
                EvasionXP = Prototypes.trainingPrototypes.BaseEvasionXP[trainingId.Id],
                BlockingXP = Prototypes.trainingPrototypes.BaseBlockingXP[trainingId.Id],
                AthleticXP = Prototypes.trainingPrototypes.BaseAthleticXP[trainingId.Id]
            };
            _EntityComponentManager.AttachComponentToEntity(entity, experience);
        }
    }
}
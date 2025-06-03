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
        private readonly EntityManager _entityManager;
        private readonly ComponentManager _componentManager;

        public UnitFactory(EntityManager entityManager, ComponentManager componentManager)
        {
            this._entityManager = entityManager ?? throw new ArgumentNullException(nameof(entityManager));
            this._componentManager = componentManager ?? throw new ArgumentNullException(nameof(componentManager));
        }

        /// <summary>
        /// Creates a unit from a prototype ID
        /// </summary>
        /// <param name="unitPrototypeId">The ID of the unit prototype</param>
        /// <param name="factionId">The faction this unit belongs to (optional)</param>
        /// <param name="position">Initial position (optional)</param>
        /// <returns>The EntityId of the created unit</returns>
        public Entity CreateUnit(int unitPrototypeId)
        {
            if (unitPrototypeId < 0 || unitPrototypeId >= Prototypes.unitPrototypes.Count)
                return default;

            UnitPrototype prototype = Prototypes.unitPrototypes[unitPrototypeId];

            // Create the unit entity
            Entity unit = _entityManager.CreateEntity();

            // Create the unit component
            UnitComponent unitComponent = new UnitComponent();
            // Add the unit component to the entity
            _componentManager.AddComponent(unit, unitComponent);


            unitComponent.Name = prototype.Name;
            // Create forms for the unit
            bool first = true;
            foreach (int formPrototypeId in prototype.FormPrototypeIds)
            {
                Entity form = CreateBodyFromPrototype(formPrototypeId);
                _entityManager.PairParentChild(unit, form);
                if (first)
                {
                    unitComponent.currentFormId = form.Id;
                    first = false;
                }
            }

            // Add the unit component to the entity
            _componentManager.AddComponent(unit, unitComponent);

            // Add leadership capabilities if the unit is a leader
            if (prototype.Command >= 0)
            {
                LeaderStatComponent leaderComponent = new LeaderStatComponent
                {
                    Command = prototype.Command,
                    MoraleModifier = prototype.MoraleModifier
                };
                _componentManager.AddComponent(unit, leaderComponent);
            }

            // Add proficiencies component
            ProficienciesComponent proficienciesComponent = new ProficienciesComponent();
            _componentManager.AddComponent(unit, proficienciesComponent);

            return unit;
        }

        /// <summary>
        /// Creates a body based on a form prototype
        /// </summary>
        private Entity CreateBodyFromPrototype(int formPrototypeId)
        {
            if (formPrototypeId < 0 || formPrototypeId >= Prototypes.formPrototypes.Count)
                return default; // Return null instead of default for reference types

            // Create the body
            Entity form = new Entity();

            // Get form prototype to access loadout information
            FormPrototype formPrototype = Prototypes.formPrototypes[formPrototypeId];
            ref BodyPrototype bodyPrototype = ref Prototypes.bodyPrototypes;
            int bodyPrototypeId = formPrototype.BodyPrototypeId;

            //Create the body parts from the template
            bodyPrototype.Heads.TryGetValue(bodyPrototypeId, out var headNum);
            for (int i = 0; i < headNum; i++) _entityManager.PairParentChild(form, CreateBodyPart(BodyPartType.Head, bodyPrototype.EyesPerHead[bodyPrototypeId]));
            bodyPrototype.Arms.TryGetValue(bodyPrototypeId, out var armNum);
            for (int i = 0; i < armNum; i++) _entityManager.PairParentChild(form, CreateBodyPart(BodyPartType.Arm));
            if(bodyPrototype.Legs.TryGetValue(bodyPrototypeId, out var legNum)) _entityManager.PairParentChild(form, CreateBodyPart(BodyPartType.LowerBody, legNum));
            if(bodyPrototype.HasChest.Contains(bodyPrototypeId)) _entityManager.PairParentChild(form, CreateBodyPart(BodyPartType.Chest));
            if(bodyPrototype.HasBeastTorso.Contains(bodyPrototypeId)) _entityManager.PairParentChild(form, CreateBodyPart(BodyPartType.BeastTorso));
            bodyPrototype.Wings.TryGetValue(bodyPrototypeId, out var wingNum);
            for (int i = 0; i < wingNum; i++) _entityManager.PairParentChild(form, CreateBodyPart(BodyPartType.Wing));
            bodyPrototype.Tails.TryGetValue(bodyPrototypeId, out var tailNum);
            for (int i = 0; i < tailNum; i++) _entityManager.PairParentChild(form, CreateBodyPart(BodyPartType.Tail));
            bodyPrototype.TrinketSlots.TryGetValue(bodyPrototypeId, out var trinketNum);
            for (int i = 0; i < trinketNum; i++) _entityManager.PairParentChild(form, CreateBodyPart(BodyPartType.TrinketSlot));

            // Equip the body based on the loadout prototype if specified
            if (formPrototype.LoadoutPrototypeID >= 0 &&
                formPrototype.LoadoutPrototypeID < Prototypes.loadoutPrototypes.Count)
            {
                EquipBodyFromLoadout(form, formPrototype.LoadoutPrototypeID); // No 'ref' needed
            }

            return form;
        }

        private Entity CreateBodyPart(BodyPartType type)
        {
            Entity part = _entityManager.CreateEntity();
            BodyPartComponent bodyPart = new BodyPartComponent
            {
                Type = type,
                State = new BodyPartState[] { BodyPartState.Healthy }, // Initialize with a default state
                DefaultEquipment = new Equipment { EquipmentPrototypeId = -1, State = EquipmentState.Pristine }
            };
            _componentManager.AddComponent(part, bodyPart);
            return part;
        }

        private Entity CreateBodyPart(BodyPartType type, int subParts)
        {
            Entity part = CreateBodyPart(type);
            BodyPartComponent bp = _componentManager.GetComponent<BodyPartComponent>(part);
            bp.State = new BodyPartState[subParts + 1]; 
            for(int i = 0; i < subParts; i++)
            {
                bp.State[i] = BodyPartState.Healthy;
            }
            return part;
        }

        /// <summary>
        /// Equips a body with items based on a loadout prototype
        /// </summary>
        private void EquipBodyFromLoadout(Entity form, int loadoutPrototypeId)
        {
            List<int> availableSlots = _componentManager.GetChildrenWithComponent<BodyPartComponent>(_entityManager, form);
            LoadoutPrototype loadout = Prototypes.loadoutPrototypes[loadoutPrototypeId];

            foreach (var equipmentId in loadout.DefaultEquipmentIds)
            {
                bool twoHanded = (Prototypes.equipmentPrototypes[equipmentId].Type == EquipmentType.TwoHandedWeapon);
                bool equipped = false;

                for (int i = 0; i < availableSlots.Count && !equipped; i++)
                {
                    if (_entityManager.GetEntity(availableSlots[i], out var part)) {
                        BodyPartComponent currentSlot = _componentManager.GetComponent<BodyPartComponent>(part);
                        if (!IsSlotValid(currentSlot.Type, Prototypes.equipmentPrototypes[equipmentId].Type))
                            continue;

                        if (twoHanded)
                        {
                            // Find a second valid slot for two-handed weapon
                            for (int k = i + 1; k < availableSlots.Count; k++)
                            {
                                if (_entityManager.GetEntity(availableSlots[k], out var secondPart)) {
                                    BodyPartComponent secondSlot = _componentManager.GetComponent<BodyPartComponent>(secondPart);
                                    if (!IsSlotValid(secondSlot.Type, Prototypes.equipmentPrototypes[equipmentId].Type))
                                        continue;

                                    // Equip the two-handed weapon
                                    currentSlot.DefaultEquipment.EquipmentPrototypeId = equipmentId;
                                    secondSlot.DefaultEquipment.EquipmentPrototypeId = -999; // Mark second hand as used

                                    // Mark these parts as no longer available
                                    availableSlots.RemoveAt(k);
                                    availableSlots.RemoveAt(i);
                                    equipped = true;
                                }
                            }
                        }
                        else
                        {
                            // Equip the one-handed item
                            currentSlot.DefaultEquipment.EquipmentPrototypeId = equipmentId;

                            // Mark this part as no longer available
                            availableSlots.RemoveAt(i);
                            equipped = true;
                        }
                    }
                }
            }
        }

        private bool IsSlotValid(BodyPartType bType, EquipmentType eType)
        {
            if (bType == BodyPartType.Head && (eType == EquipmentType.Helmet || eType == EquipmentType.Circlet))
            {
                return true;
            }
            else if (bType == BodyPartType.Arm &&
                (eType == EquipmentType.OneHandedWeapon || eType == EquipmentType.TwoHandedWeapon ||
                eType == EquipmentType.NaturalWeapon || eType == EquipmentType.RangedWeapon || eType == EquipmentType.Shield))
            {
                return true;
            }
            else if (bType == BodyPartType.Chest && (eType == EquipmentType.ChestArmor || eType == EquipmentType.FullArmor))
            {
                return true;
            }
            else if (bType == BodyPartType.BeastTorso && eType == EquipmentType.Barding)
            {
                return true;
            }
            else if (bType == BodyPartType.Tail && eType == EquipmentType.NaturalWeapon)
            {
                return true;
            }
            else if (bType == BodyPartType.LowerBody && (eType == EquipmentType.Boots || eType == EquipmentType.Greaves))
            {
                return true;
            }
            else if (bType == BodyPartType.TrinketSlot && eType == EquipmentType.Trinket)
            {
                return true;
            }
            return false;
        }

    }
}
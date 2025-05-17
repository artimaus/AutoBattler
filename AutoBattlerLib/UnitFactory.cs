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
        public EntityId CreateUnit(int unitPrototypeId)
        {
            if (unitPrototypeId < 0 || unitPrototypeId >= Prototypes.unitPrototypes.Count)
                return default;

            UnitPrototype prototype = Prototypes.unitPrototypes[unitPrototypeId];

            // Create the unit entity
            EntityId unit = _entityManager.CreateEntity();

            // Create the unit component
            UnitComponent unitComponent = new UnitComponent();
            // Add the unit component to the entity
            _componentManager.AddComponent(unit, unitComponent);


            unitComponent.Name = prototype.Name;
            // Create forms for the unit
            bool first = true;
            foreach (int formPrototypeId in prototype.FormPrototypeIds)
            {
                EntityId form = CreateBodyFromPrototype(formPrototypeId);
                _entityManager.PairParentChild(unit, form);
                if (first)
                {
                    unitComponent.currentForm = form;
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
        private EntityId CreateBodyFromPrototype(int formPrototypeId)
        {
            if (formPrototypeId < 0 || formPrototypeId >= Prototypes.formPrototypes.Count)
                return default; // Return null instead of default for reference types

            // Create the body
            EntityId form = new EntityId();

            // Get form prototype to access loadout information
            FormPrototype formPrototype = Prototypes.formPrototypes[formPrototypeId];
            BodyPrototype bodyPrototype = Prototypes.bodyPrototypes[formPrototype.BodyPrototypeId];

            //Create the body parts from the template
            for (int i = 0; i < bodyPrototype.Heads; i++) _entityManager.PairParentChild(form, CreateBodyPart(BodyPartType.Head, bodyPrototype.EyesPerHead));
            for (int i = 0; i < bodyPrototype.Arms; i++) _entityManager.PairParentChild(form, CreateBodyPart(BodyPartType.Arm));
            if(bodyPrototype.Legs > 0) _entityManager.PairParentChild(form, CreateBodyPart(BodyPartType.LowerBody, bodyPrototype.Legs));
            if(bodyPrototype.HasChest) _entityManager.PairParentChild(form, CreateBodyPart(BodyPartType.Chest));
            if(bodyPrototype.HasBeastTorso) _entityManager.PairParentChild(form, CreateBodyPart(BodyPartType.BeastTorso));
            for(int i = 0; i < bodyPrototype.Wings; i++) _entityManager.PairParentChild(form, CreateBodyPart(BodyPartType.Wing));
            for (int i = 0; i < bodyPrototype.Tails; i++) _entityManager.PairParentChild(form, CreateBodyPart(BodyPartType.Tail));
            for (int i = 0; i < bodyPrototype.TrinketSlots; i++) _entityManager.PairParentChild(form, CreateBodyPart(BodyPartType.TrinketSlot));

            // Equip the body based on the loadout prototype if specified
            if (formPrototype.LoadoutPrototypeID >= 0 &&
                formPrototype.LoadoutPrototypeID < Prototypes.loadoutPrototypes.Count)
            {
                EquipBodyFromLoadout(form, formPrototype.LoadoutPrototypeID); // No 'ref' needed
            }

            return form;
        }

        private EntityId CreateBodyPart(BodyPartType type)
        {
            EntityId part = _entityManager.CreateEntity();
            BodyPartComponent bodyPart = new BodyPartComponent
            {
                Type = type,
                State = new List<BodyPartState> { BodyPartState.Healthy }, // Initialize with a default state
                DefaultEquipment = new Equipment { EquipmentPrototypeId = -1, State = EquipmentState.Pristine }
            };
            _componentManager.AddComponent(part, bodyPart);
            return part;
        }

        private EntityId CreateBodyPart(BodyPartType type, int subParts)
        {
            EntityId part = CreateBodyPart(type);
            for(int i = 0; i < subParts; i++)
            {
                _componentManager.GetComponent<BodyPartComponent>(part).State.Add(BodyPartState.Healthy);
            }
            return part;
        }

        /// <summary>
        /// Equips a body with items based on a loadout prototype
        /// </summary>
        private void EquipBodyFromLoadout(EntityId form, int loadoutPrototypeId)
        {
            List<EntityId> availableSlots = _entityManager.GetChildrenWithComponent<BodyPartComponent>(form);
            LoadoutPrototype loadout = Prototypes.loadoutPrototypes[loadoutPrototypeId];

            foreach (var equipmentId in loadout.DefaultEquipmentIds)
            {
                bool twoHanded = (Prototypes.equipmentPrototypes[equipmentId].Type == EquipmentType.TwoHandedWeapon);
                bool equipped = false;

                for (int i = 0; i < availableSlots.Count && !equipped; i++)
                {
                    BodyPartComponent currentSlot = _componentManager.GetComponent<BodyPartComponent>(availableSlots[i]);
                    if (!IsSlotValid(currentSlot.Type, Prototypes.equipmentPrototypes[equipmentId].Type))
                        continue;

                    if (twoHanded)
                    {
                        // Find a second valid slot for two-handed weapon
                        for (int k = i + 1; k < availableSlots.Count && !equipped; k++)
                        {
                            BodyPartComponent secondSlot = _componentManager.GetComponent<BodyPartComponent>(availableSlots[k]);
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
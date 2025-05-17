using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoBattlerLib
{
    public class UnitSystem
    {
        private ComponentManager _componentManager;
        private EntityManager _entityManager;

        public UnitSystem(EntityManager entityManager, ComponentManager componentManager)
        {
            _componentManager = componentManager;
            _entityManager = entityManager;
        }

        public int GetCurrentSize(EntityId form)
        {
            return Prototypes.attributeTemplate[_componentManager.GetComponent<FormComponent>(form).AttributesId].Size;
        }

        public int GetCurrentStrength(EntityId form)
        {
            return Prototypes.attributeTemplate[_componentManager.GetComponent<FormComponent>(form).AttributesId].Strength;
        }
        public int GetCurrentDexterity(EntityId form)
        {
            return Prototypes.attributeTemplate[_componentManager.GetComponent<FormComponent>(form).AttributesId].Dexterity;
        }
        public int GetCurrentAgility(EntityId form)
        {
            return Prototypes.attributeTemplate[_componentManager.GetComponent<FormComponent>(form).AttributesId].Agility;
        }
        public int GetCurrentStamina(EntityId form)
        {
            return Prototypes.attributeTemplate[_componentManager.GetComponent<FormComponent>(form).AttributesId].Stamina;
        }
        public int GetCurrentToughness(EntityId form)
        {
            return Prototypes.attributeTemplate[_componentManager.GetComponent<FormComponent>(form).AttributesId].Toughness;
        }
        public int GetCurrentWill(EntityId form)
        {
            return Prototypes.attributeTemplate[_componentManager.GetComponent<FormComponent>(form).AttributesId].Will;
        }
        public int GetCurrentConstitution(EntityId form)
        {
            return Prototypes.attributeTemplate[_componentManager.GetComponent<FormComponent>(form).AttributesId].Constitution;
        }
        public int GetCurrentCombatSpeed(EntityId form)
        {
            return (int)(GetCurrentSize(form) / 2.0 + GetCurrentAgility(form) / 2.0 +
                (GetAthleticism(form) + 1) / 2.0) + 1;
        }
        public int GetAthleticism(EntityId form)
        {
            return _componentManager.GetComponent<ProficienciesComponent>(form).Athleticism;
        }
        public int GetStrikingSkill(EntityId form)
        {
            return _componentManager.GetComponent<ProficienciesComponent>(form).StrikingSkill;
        }
        public int GetParryingSkill(EntityId form)
        {
            return _componentManager.GetComponent<ProficienciesComponent>(form).ParryingSkill;
        }
        public int GetEvasionSkill(EntityId form)
        {
            return _componentManager.GetComponent<ProficienciesComponent>(form).EvasionSkill;
        }
        public int GetBlockingSkill(EntityId form)
        {
            return _componentManager.GetComponent<ProficienciesComponent>(form).BlockingSkill;
        }
        public int GetCurrentMaxHealth(EntityId form)
        {
            return (int)(GetCurrentSize(form) * ((GetCurrentConstitution(form) / 5.0) +
                (GetCurrentToughness(form) / 10.0)));
        }
        public EntityId GetCurrentForm(EntityId unit)
        {
            var unitComponent = _componentManager.GetComponent<UnitComponent>(unit);

            return unitComponent.currentForm;
        }
        public List<EntityId> GetCurrentEquipment(List<IComponent> body)
        {

        }
    }
}

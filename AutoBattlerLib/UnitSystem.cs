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

        public int GetFormAttribute(Entity form, AttributeType type)
        {

            return Prototypes.statlines[type][_componentManager.GetComponent<FormComponent>(form).AttributesId];
        }

        public int GetFormCombatSpeed(Entity form)
        {
            return (int)(GetFormAttribute(form, AttributeType.Size) / 2.0 + GetFormAttribute(form, AttributeType.Agility) / 2.0 +
                (GetAthleticism(form) + 1) / 2.0) + 1;
        }
        public int GetAthleticism(Entity form)
        {
            return _componentManager.GetComponent<ProficienciesComponent>(form).Athleticism;
        }
        public int GetStrikingSkill(Entity form)
        {
            return _componentManager.GetComponent<ProficienciesComponent>(form).StrikingSkill;
        }
        public int GetParryingSkill(Entity form)
        {
            return _componentManager.GetComponent<ProficienciesComponent>(form).ParryingSkill;
        }
        public int GetEvasionSkill(Entity form)
        {
            return _componentManager.GetComponent<ProficienciesComponent>(form).EvasionSkill;
        }
        public int GetBlockingSkill(Entity form)
        {
            return _componentManager.GetComponent<ProficienciesComponent>(form).BlockingSkill;
        }
        public int GetCurrentMaxHealth(Entity form)
        {
            return (int)(GetFormAttribute(form, AttributeType.Size) * 
                ((GetFormAttribute(form, AttributeType.Constitution) / 5.0) +
                (GetFormAttribute(form, AttributeType.Toughness) / 10.0)));
        }
        public Entity GetCurrentForm(Entity unit)
        {
            var unitComponent = _componentManager.GetComponent<UnitComponent>(unit);

            return unitComponent.currentForm;
        }
        public List<Entity> GetCurrentEquipment(List<IComponent> body)
        {

        }
    }
}

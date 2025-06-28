using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoBattlerLib
{
    internal static class Prototypes
    {
        internal static UnitPrototypes unitPrototypes = new UnitPrototypes();
        internal static FormPrototypes formPrototypes = new FormPrototypes();
        internal static ProficiencyPrototypes proficiencyPrototypes = new ProficiencyPrototypes();
        internal static BodyPrototypes bodyPrototypes = new BodyPrototypes();
        internal static AttributePrototypes attributes = new AttributePrototypes();
        internal static WeaponPrototypes weaponPrototypes = new WeaponPrototypes();
        internal static ArmorPrototypes armorPrototypes = new ArmorPrototypes();
        internal static WeaponPrototypes naturalWeaponPrototypes = new WeaponPrototypes();
        internal static ArmorPrototypes naturalArmorPrototypes = new ArmorPrototypes();
        internal static CommanderAttributeProtoypes commanderAttributes = new CommanderAttributeProtoypes();

        internal static Dictionary<UnitComponent, CommanderComponent> unitCommand = new Dictionary<UnitComponent, CommanderComponent>();
        internal static Dictionary<UnitComponent, DrillingPrototype> defaultDrilling = new Dictionary<UnitComponent, DrillingPrototype>();
        internal static Dictionary<UnitComponent, ProficiencyPrototypeId> defaultProficiencies = new Dictionary<UnitComponent, ProficiencyPrototypeId>();
        internal static Dictionary<UnitComponent, LoadoutPrototypeId> loadouts = new Dictionary<UnitComponent, LoadoutPrototypeId>();
        internal static Dictionary<LoadoutPrototypeId, List<Armor>> loadoutArmor = new Dictionary<LoadoutPrototypeId, List<Armor>>();
        internal static Dictionary<LoadoutPrototypeId, List<Weapon>> loadoutWeapons = new Dictionary<LoadoutPrototypeId, List<Weapon>>();
        internal static Dictionary<FormComponent, List<NaturalWeapon>> formNaturalWeapons = new Dictionary<FormComponent, List<NaturalWeapon>>();
        internal static Dictionary<TransitionType, Dictionary<FormComponent, FormComponent>> formTransitions = new Dictionary<TransitionType, Dictionary<FormComponent, FormComponent>>();
    }
}

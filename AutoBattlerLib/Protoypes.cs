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
        internal static AttributesPrototype[] attributes = new AttributesPrototype[20000];
        internal static BodyPrototype[] bodyPrototypes = new BodyPrototype[20000];
        internal static EquipmentPrototype[] equipmentPrototypes = new EquipmentPrototype[30000];
        internal static NaturalWeaponPrototype[] naturalWeaponPrototypes = new NaturalWeaponPrototype[30000];
        internal static CommanderAttributes[] commanderAttributes = new CommanderAttributes[20000];
        internal static Dictionary<UnitComponent, CommanderComponent> unitCommand = new Dictionary<UnitComponent, CommanderComponent>();
        internal static Dictionary<UnitComponent, ProficienciesPrototype> defaultProficiencies = new Dictionary<UnitComponent, ProficienciesPrototype>();
        internal static Dictionary<FormComponent, HashSet<NaturalWeapon>> formWeapons = new Dictionary<FormComponent, HashSet<NaturalWeapon>>();
        internal static Dictionary<LoadoutPrototypeId, HashSet<Equipment>> loadoutPrototypes = new Dictionary<LoadoutPrototypeId, HashSet<Equipment>>();
        internal static Dictionary<TransitionType, Dictionary<FormComponent, FormComponent>> formTransitions = new Dictionary<TransitionType, Dictionary<FormComponent, FormComponent>>();
    }
}

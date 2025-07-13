using System;
using System.Collections;
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
        internal static TrainingPrototypes trainingPrototypes = new TrainingPrototypes();
        internal static BodyPrototypes bodyPrototypes = new BodyPrototypes();
        internal static AttributesPrototypes attributes = new AttributesPrototypes();
        internal static LoadoutPrototypes loadoutPrototypes = new LoadoutPrototypes();
        internal static FormLoadoutPrototypes formLoadoutPrototypes = new FormLoadoutPrototypes();
        internal static WeaponPrototypes weaponPrototypes = new WeaponPrototypes();
        internal static ArmorPrototypes armorPrototypes = new ArmorPrototypes();
        internal static WeaponPrototypes naturalWeaponPrototypes = new WeaponPrototypes();
        internal static ArmorPrototypes naturalArmorPrototypes = new ArmorPrototypes();
        internal static CommanderAttributeProtoypes commanderAttributes = new CommanderAttributeProtoypes();

        internal static BloomMap<UnitComponent, CommanderComponent> unitCommand;
        internal static BloomMap<UnitComponent, TrainingId> defaultTraining;
        internal static BloomMap<UnitComponent, ProficiencyId> specialProficiencies;
        internal static BloomMap<FormComponent, FormLoadoutPrototypeId> formLoadoutIds;

        internal static BloomMap<FormComponent, FormComponent> OnDeathTransition;
        internal static BloomMap<FormComponent, FormComponent> OnBattleStartTransition;
        internal static BloomMap<FormComponent, FormComponent> OnBattleEndTransition;
        internal static BloomMap<FormComponent, FormComponent> OnCommandTransition;
    }
}

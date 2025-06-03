using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoBattlerLib
{
    internal static class Prototypes
    {
        internal static UnitPrototype unitPrototypes = new List<UnitPrototype>();
        internal static AttributesPrototype[] attributes = new AttributesPrototype();
        internal static BodyPrototype[] bodyPrototypes = new BodyPrototype();
        internal static EquipmentPrototype[] equipmentPrototypes = new List<EquipmentPrototype>();
        internal static FormPrototype[] forms = new Dictionary<FormId, FormPrototype>();
        internal static Dictionary<FormId, FormTransition> formTransitions = new Dictionary<FormId, FormTransition>();
        internal static Dictionary<FormId, HashSet<EquipmentId>> formPrototypeLoadouts = new Dictionary<LoadoutPrototypeId, LoadoutPrototype>();
    }
}

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
        internal static Dictionary<FormId, string> formNames = new Dictionary<FormId, string>();
        internal static Dictionary<FormId, BodyPrototypeId> formBodies = new Dictionary<FormId, BodyPrototypeId>();
        internal static Dictionary<FormId, AttributesPrototype> formAttributes = new Dictionary<FormId, AttributesPrototype>();
        internal static Dictionary<LoadoutPrototypeId, HashSet<EquipmentId>> loadoutPrototypes = new Dictionary<LoadoutPrototypeId, HashSet<EquipmentId>>();
        internal static Dictionary<TransitionType, Dictionary<FormId, FormId>> formTransitions = new Dictionary<TransitionType, Dictionary<FormId, FormId>>();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoBattlerLib
{
    internal static class Prototypes
    {
        internal static List<UnitPrototype> unitPrototypes = new List<UnitPrototype>();
        internal static List<FormPrototype> formPrototypes = new List<FormPrototype>();
        internal static Dictionary<AttributeType, List<int>> statlines = new Dictionary<AttributeType, List<int>>();
        internal static BodyPrototypes bodyPrototypes = new BodyPrototypes();
        internal static List<EquipmentPrototype> equipmentPrototypes = new List<EquipmentPrototype>();
        internal static List<LoadoutPrototype> loadoutPrototypes = new List<LoadoutPrototype>();
    }
}

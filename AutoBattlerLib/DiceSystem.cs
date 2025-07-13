using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoBattlerLib
{
    internal static class Dice
    {
        public static int Roll(Entity unit, EntityComponentManager _componentManager)
        {
            if (_componentManager.HasComponent<FortuneComponent>(unit))
            {
                if (_componentManager.GetComponent<FortuneComponent>(unit).Fortune == FortuneType.Lucky)
                {
                    return LuckyExplodingDiceRoll();
                }
                else
                {
                    return CursedExplodingDiceRoll();
                }
            }
            return ExplodingDiceRoll();
        }

        public static int LuckyExplodingDiceRoll()
        {
            return Math.Max(ExplodingDiceRoll(), ExplodingDiceRoll());
        }

        public static int CursedExplodingDiceRoll()
        {
            return Math.Min(ExplodingDiceRoll(), ExplodingDiceRoll());
        }

        public static int ExplodingDiceRoll()
        {
            int dieValue = Random.Shared.Next(0, 6);
            if (dieValue == 5)
            {
                return dieValue + ExplodingDiceRoll();
            }
            return dieValue;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoBattlerLib
{
    internal class CombatSystem
    {
        private EntityManager _entityManager;
        private ComponentManager _componentManager;
        private int CombatRound = 1;

        public CombatSystem(EntityManager entityManager, ComponentManager componentManager)
        {
            _entityManager = entityManager;
            _componentManager = componentManager;
        }

        public void ResolveCombatAction(Entity attacker, Entity Defender)
        {
            
        }






    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoBattlerLib
{


    /// <summary>
    /// Manages entity creation and destruction
    /// </summary>
    public class EntityManager
    {
        private int nextEntityId = 1;
        private HashSet<Entity> entities = new HashSet<Entity>();
        private Queue<Entity> recycledIds = new Queue<Entity>();

        /// <summary>
        /// Attempts to get an entity by ID
        /// </summary>


    }
}

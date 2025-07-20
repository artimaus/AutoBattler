using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Drawing;

namespace AutoBattlerLib
{
    public class BattlefieldManager
    {
        private EntityComponentManager battleEntities;
        public ushort currentRound;
        public BattleScheduler TickSchedule { get; set; }
        public Entity[][][] battlefieldTiles; // [100][100][10] - 4x4 grid of tiles, each with up to 1000 entities

        public void Initialize(EntityComponentManager entityComponentManager)
        {
            battleEntities = entityComponentManager;
            TickSchedule = new BattleScheduler();
        }

        public void Update()
        {
            Entity[] currentEntities = TickSchedule.GetCurrentActiveEntities(currentRound);


        }

        public void MovementPhase()
        {

        }

        public void ActionPhase()
        {

        }

        public void EffectPhase()
        {

        }

        public void CleanupPhase()
        {

        }
    }



    public class BattlefieldTileComponent : IComponentData
    {
        
    }


    public struct BattleScheduler
    {
        private BitFlagArray[][] tickEntities;// [4][4][1000]
        private ushort[][] tickedEntityCount; //[4][4]
        private ulong[] entitySchedule; //[4000]

        public BattleScheduler()
        {
            tickEntities = new BitFlagArray[4][];
            for (int i = 0; i < tickEntities.Length; i++)
            {
                tickEntities[i] = new BitFlagArray[4];
                for (int j = 0; j < tickEntities[i].Length; j++)
                {
                    tickEntities[i][j] = new BitFlagArray(1000);
                }
            }
            entitySchedule = new ulong[4000];
        }

        public Entity[] GetCurrentActiveEntities(ushort currentRound)
        {
            byte roundIndex = (byte)((currentRound & 15) >> 2);
            byte tickIndex = (byte)((currentRound & 15) & 3);
            Entity[] result = new Entity[tickedEntityCount[roundIndex][tickIndex]];
            for (int i = 0, j = 0; i < tickEntities[roundIndex][tickIndex].Length && j < result.Length; i++)
            {
                if (tickEntities[roundIndex][tickIndex][i])
                {
                    result[j++] = new Entity((ushort)i);
                }
            }
            return result;
        }

        public void RescheduleEntities(Entity[] entities, byte[] offset, int currentRound)
        {
            byte[] entityRound = new byte[entities.Length];
            bool[] reschedule = new bool[entities.Length];
            byte[] currentOffset = new byte[entities.Length];

            for (int e = 0; e < entities.Length; e++)
            {
                entityRound[e] = (byte)(entitySchedule[entities[e].Id >> 4] >> ((entities[e].Id & 15) << 2) & 0xF);
                currentOffset[e] = (byte)((entityRound[e] - (currentRound & 15) + 16) & 15);
                if (offset[e] > currentOffset[e])
                {
                    reschedule[e] = true;
                    entitySchedule[entities[e].Id >> 4] = 
                                  (entitySchedule[entities[e].Id >> 4] & ~(0xFUL << ((entities[e].Id & 15) << 2))) | 
                                  ((ulong)((currentRound + offset[e]) & 15) << ((entities[e].Id & 15) << 2));
                }
            }
            for (int e = 0; e < entities.Length; e++)
            {
                if (reschedule[e])
                {
                    tickEntities[entityRound[e] >> 2][entityRound[e] & 3][entities[e].Id] = false;
                    tickEntities[((currentRound + offset[e]) & 15) >> 2][((currentRound + offset[e]) & 15) & 3][entities[e].Id] = true;
                    tickedEntityCount[((currentRound + offset[e]) & 15) >> 2][((currentRound + offset[e]) & 15) & 3]++;
                }
            }
        }
    }
}

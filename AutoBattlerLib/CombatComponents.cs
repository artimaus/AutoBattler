using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Drawing;
using System.Dynamic;

namespace AutoBattlerLib
{

    public class BattlefieldManager
    {
        private EntityComponentManager battleEntities;
        public ushort currentTick;
        public BattleScheduler TickSchedule { get; set; }
        public BattlefieldTileManager battlefieldTiles { get; set; }
        public ActionCard[] actionCards;

        public struct BattleScheduler
        {
            private BitFlagMap<Entity>[][] tickSchedule;
            private BitFlagArray[][] tickEntities;// [4][10][1000]
            private ushort[][] tickedEntityCount; //[4][10]
            private ulong[] entitySchedule; //[4000]

            public BattleScheduler()
            {
                tickEntities = new BitFlagArray[4][];
                tickedEntityCount = new ushort[4][];
                for (int i = 0; i < tickEntities.Length; i++)
                {
                    tickEntities[i] = new BitFlagArray[10];
                    tickedEntityCount[i] = new ushort[10];
                    for (int j = 0; j < tickEntities[i].Length; j++)
                    {
                        tickEntities[i][j] = new BitFlagArray(1000);
                    }
                }
                entitySchedule = new ulong[4000];
            }

            public Entity[] GetCurrentActiveEntities(ushort currentTick)
            {
                byte roundIndex = (byte)((currentTick & 15) >> 2);
                byte tickIndex = (byte)((currentTick & 15) & 3);
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

            public void RescheduleEntities(Entity[] entities, byte[] offset, ushort currentTick)
            {
                byte[] entityRound = new byte[entities.Length];
                bool[] reschedule = new bool[entities.Length];
                byte[] currentOffset = new byte[entities.Length];

                for (int e = 0; e < entities.Length; e++)
                {
                    entityRound[e] = (byte)(entitySchedule[entities[e].Id >> 4] >> ((entities[e].Id & 15) << 2) & 0xF);
                    currentOffset[e] = (byte)((entityRound[e] - (currentTick & 15) + 16) & 15);
                    if (offset[e] > currentOffset[e])
                    {
                        reschedule[e] = true;
                        entitySchedule[entities[e].Id >> 4] =
                                      (entitySchedule[entities[e].Id >> 4] & ~(0xFUL << ((entities[e].Id & 15) << 2))) |
                                      ((ulong)((currentTick + offset[e]) & 15) << ((entities[e].Id & 15) << 2));
                    }
                }
                for (int e = 0; e < entities.Length; e++)
                {
                    if (reschedule[e])
                    {
                        tickEntities[entityRound[e] >> 2][entityRound[e] & 3][entities[e].Id] = false;
                        tickEntities[((currentTick + offset[e]) & 15) >> 2][((currentTick + offset[e]) & 15) & 3][entities[e].Id] = true;
                        tickedEntityCount[((currentTick + offset[e]) & 15) >> 2][((currentTick + offset[e]) & 15) & 3]++;
                    }
                }
            }
        }

        public struct BattlefieldTileManager
        {
            private Entity[][][] tileEntities; // [100][100][10] - 100x100 grid of tiles, each with up to 10 entities
            private byte[][] tileEntityCount; // [100][100] - Count of entities on each tile
            private ushort[][] tileFactionId; // [100][100] - Faction ID of the tile

            public Entity[] GetTileEntities(int x, int y)
            {
                if (x < 0 || x >= 100 || y < 0 || y >= 100)
                    return Array.Empty<Entity>();
                return tileEntities[x][y];
            }

            public byte GetTileEntityCount(int x, int y)
            {
                if (x < 0 || x >= 100 || y < 0 || y >= 100)
                    return 0;
                return tileEntityCount[x][y];
            }

            public ushort GetTileFactionId(int x, int y)
            {
                if (x < 0 || x >= 100 || y < 0 || y >= 100)
                    return 0;
                return tileFactionId[x][y];
            }

            public Entity[][] GetCurrentRelevantTileEntities(ActionCard[] currentEntities)
            {
                Entity[][] relevantTiles = new Entity[currentEntities.Length * 9][];
                for (int i = 0; i < currentEntities.Length * 9; i++)
                {
                    relevantTiles[i] = GetTileEntities(currentEntities[i / 9].x + ((i / 3) % 3 - 1), currentEntities[i / 9].y + (i % 3 - 1));
                }
                return relevantTiles;
            }

            public byte[] GetCurrentRelevantTileEntityCounts(ActionCard[] currentEntities)
            {
                byte[] relevantCounts = new byte[currentEntities.Length * 9];
                for (int i = 0; i < currentEntities.Length * 9; i++)
                {
                    relevantCounts[i] = GetTileEntityCount(currentEntities[i / 9].x + ((i / 3) % 3 - 1), currentEntities[i / 9].y + (i % 3 - 1));
                }
                return relevantCounts;
            }

            public ushort[] GetCurrentRelevantTileFactions(ActionCard[] currentEntities)
            {
                ushort[] relevantFactions = new ushort[currentEntities.Length * 9];
                for (int i = 0; i < currentEntities.Length * 9; i++)
                {
                    relevantFactions[i] = GetTileFactionId(currentEntities[i / 9].x + ((i / 3) % 3 - 1), currentEntities[i / 9].y + (i % 3 - 1));
                }
                return relevantFactions;
            }

            public BattlefieldTileManager()
            {
                tileEntities = new Entity[100][][];
                tileEntityCount = new byte[100][];
                tileFactionId = new ushort[100][];
                for (int i = 0; i < 100; i++)
                {
                    tileEntities[i] = new Entity[100][];
                    tileEntityCount[i] = new byte[100];
                    tileFactionId[i] = new ushort[100];
                    for (int j = 0; j < 100; j++)
                    {
                        tileEntities[i][j] = new Entity[10]; // Up to 10 entities per tile
                    }
                }
            }
        }

        public struct ActionCard
        {
            public ushort factionId; // ID of the faction
            public byte x, y; // Coordinates on the battlefield

        }

        public ActionCard[] GetCurrentActionCards(Entity[] currentEntities)
        {
            ActionCard[] currentActionCards = new ActionCard[currentEntities.Length];
            for (int i = 0; i < currentEntities.Length; i++)
            {
                currentActionCards[i] = actionCards[currentEntities[i].Id];
            }
            return currentActionCards;
        }

        public void Initialize(EntityComponentManager entityComponentManager)
        {
            battleEntities = entityComponentManager;
            TickSchedule = new BattleScheduler();
        }

        public void Update()
        {
            Entity[] currentEntities = TickSchedule.GetCurrentActiveEntities(currentTick);
            ActionCard[] currentActionCards = GetCurrentActionCards(currentEntities);
            Entity[][] currentTiles = battlefieldTiles.GetCurrentRelevantTileEntities(currentActionCards);
            byte[] currentTileEntityCounts = battlefieldTiles.GetCurrentRelevantTileEntityCounts(currentActionCards);
            ushort[] currentTileFactions = battlefieldTiles.GetCurrentRelevantTileFactions(currentActionCards);

        }

        public void ActionPhase(Entity[] currentEntities, ActionCard[] currentActionCards, Entity[][] currentTiles, byte[] currentTileEntityCounts, ushort[] currentTileFactions)
        {

        }

        public void EffectPhase()
        {

        }

        public void CleanupPhase()
        {

        }
    }
}

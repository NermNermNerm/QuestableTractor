using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using StardewValley;
using StardewValley.Quests;

namespace NermNermNerm.Stardew.QuestableTractor
{
    public abstract class FakeQuest
    {
        private static readonly Dictionary<Quest, FakeQuest> realQuestToFakeQuestMap = new Dictionary<Quest, FakeQuest>();
        private static bool hasHarmonyPatchBeenInstalled = false;

        public abstract bool checkIfComplete(NPC n, int number1, int number2, Item? item, string str, bool probe);

        private readonly Quest realQuest = new Quest();

        public void SetDisplayAsNew()
        {
            this.realQuest.showNew.Value = true;
        }

        public virtual void questComplete()
        {
            this.realQuest.questComplete();
        }

        public string questTitle { get => this.realQuest.questTitle; set => this.realQuest.questTitle = value; }
        public string questDescription { get => this.realQuest.questDescription; set => this.realQuest.questDescription = value; }
        public string currentObjective { get => this.realQuest.currentObjective; set => this.realQuest.currentObjective = value; }
        public void MarkAsViewed() => this.realQuest.MarkAsViewed();

        public static void AddToQuestLog(Farmer player, FakeQuest quest)
        {
            player.questLog.Add(quest.realQuest);
            realQuestToFakeQuestMap[quest.realQuest] = quest;

            EnsureHarmonyPatchInstalled();
        }

        private static void EnsureHarmonyPatchInstalled()
        {
            if (hasHarmonyPatchBeenInstalled)
            {
                return;
            }

            var onItemOfferedToNpcMethod = typeof(Quest).GetMethod("OnItemOfferedToNpc");
            ModEntry.Instance.Harmony.Patch(onItemOfferedToNpcMethod, prefix: new HarmonyMethod(typeof(FakeQuest), nameof(Prefix_OnItemOfferedToNpc)));

            var onOnNpcSocializedMethod = typeof(Quest).GetMethod("OnNpcSocialized");
            ModEntry.Instance.Harmony.Patch(onOnNpcSocializedMethod, prefix: new HarmonyMethod(typeof(FakeQuest), nameof(Prefix_OnNpcSocialized)));

            hasHarmonyPatchBeenInstalled = true;
        }

        private static bool Prefix_OnNpcSocialized(Quest __instance, NPC npc, bool probe, ref bool __result)
        {
            if (realQuestToFakeQuestMap.TryGetValue(__instance, out var fake))
            {
                __result = fake.checkIfComplete(npc, 0, 0, null, "", probe);
                return false;
            }
            else
            {
                return true;
            }
        }

        private static bool Prefix_OnItemOfferedToNpc(Quest __instance, NPC npc, Item item, bool probe, ref bool __result)
        {
            if (realQuestToFakeQuestMap.TryGetValue(__instance, out var fake))
            {
                __result = fake.checkIfComplete(npc, 0, 0, item, "", probe);
                return false;
            }
            else
            {
                return true;
            }
        }

        public static void RemoveQuest(Farmer player, FakeQuest quest)
        {
            player.questLog.Remove(quest.realQuest);
            realQuestToFakeQuestMap.Remove(quest.realQuest);
        }

        public static T? GetFakeQuestByType<T>(Farmer player)
            where T : FakeQuest
        {
            foreach (var quest in player.questLog)
            {
                if (realQuestToFakeQuestMap.TryGetValue(quest, out var fakeQuest) && fakeQuest is T t)
                {
                    return t;
                }
            }

            return null;
        }

        public static BaseQuest? GetFakeQuestByController(Farmer player, BaseQuestController controller)
        {
            foreach (var quest in player.questLog)
            {
                if (realQuestToFakeQuestMap.TryGetValue(quest, out var fakeQuest) && fakeQuest is BaseQuest bq && bq.Controller == controller)
                {
                    return bq;
                }
            }

            return null;
        }

        public static void RemoveAllFakeQuests(Farmer player)
        {
            var realQuests = player.questLog.Where(realQuestToFakeQuestMap.ContainsKey).ToArray();

            foreach (var quest in realQuests)
            {
                player.questLog.Remove(quest);
                realQuestToFakeQuestMap.Remove(quest);
            }
        }
    }
}

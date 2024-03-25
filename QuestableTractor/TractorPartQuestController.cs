using System.Linq;
using StardewValley;
using Microsoft.Xna.Framework;

namespace NermNermNerm.Stardew.QuestableTractor
{
    /// <summary>
    ///   This class represents aspects of the class that need monitoring whether or not the player is
    ///   actually on the quest.  For example, if there are triggers that start the quest, the controller
    ///   will detect them.  The controller is also the place that knows whether the quest has been
    ///   completed or not.
    /// </summary>
    /// <remarks>
    ///   Quest Controllers should be constructed when the mod is initialized (in <code>Mod.Entry</code>)
    ///   and they are never destroyed.
    /// </remarks>
    public abstract class TractorPartQuestController<TQuestState> : BaseQuestController<TQuestState>
        where TQuestState : struct
    {
        protected TractorPartQuestController(ModEntry mod) : base(mod) { }

        public abstract string WorkingAttachmentPartId { get; }
        public abstract string BrokenAttachmentPartId { get; }

        protected sealed override BaseQuest CreateQuest() => this.CreatePartQuest();

        public new TractorPartQuest<TQuestState>? GetQuest(Farmer player) => (TractorPartQuest<TQuestState>?)base.GetQuest(player);

        protected abstract TractorPartQuest<TQuestState> CreatePartQuest();

        public virtual void AnnounceGotBrokenPart(Item brokenPart)
        {
            Game1.player.holdUpItemThenMessage(brokenPart);
        }

        protected override void OnStateChanged()
        {
            if (this.OverallQuestState == OverallQuestState.NotStarted)
            {
                if (Game1.IsMasterGame)
                {
                    this.HideStarterItemIfNeeded();
                }

                this.MonitorInventoryForItem(this.BrokenAttachmentPartId, this.PlayerGotBrokenPart);
                this.StopMonitoringInventoryFor(this.WorkingAttachmentPartId);
            }
            else if (this.OverallQuestState == OverallQuestState.InProgress)
            {
                this.StopMonitoringInventoryFor(this.BrokenAttachmentPartId);
                this.MonitorInventoryForItem(this.WorkingAttachmentPartId, this.PlayerGotWorkingPart);
            }
            else
            {
                this.StopMonitoringInventoryFor(this.WorkingAttachmentPartId);
                this.StopMonitoringInventoryFor(this.BrokenAttachmentPartId);
            }
        }

        private void PlayerGotBrokenPart(Item brokenPart)
        {
            // This is a callback from MonitorInventoryItem so Game1.player.IsMainPlayer must be true.
            if (this.IsStarted)
            {
                this.LogWarning($"Player found a broken attachment, {brokenPart.ItemId}, when the quest was active?!");
                return;
            }

            this.AnnounceGotBrokenPart(brokenPart);
            this.CreateQuestNew(Game1.player);
            this.MonitorInventoryForItem(this.WorkingAttachmentPartId, this.PlayerGotWorkingPart);
            this.StopMonitoringInventoryFor(this.BrokenAttachmentPartId);
        }

        public void PlayerGotWorkingPart(Item workingPart)
        {
            // This is a callback from MonitorInventoryItem so Game1.player.IsMainPlayer must be true.
            var quest = this.GetQuest(Game1.player);
            if (quest is null)
            {
                this.LogWarning($"Player found a working attachment, {workingPart.ItemId}, when the quest was not active?!");
                // consider recovering by creating the quest?
                return;
            }

            quest.GotWorkingPart(workingPart);
            this.StopMonitoringInventoryFor(this.WorkingAttachmentPartId);
        }

        protected abstract string QuestCompleteMessage { get; }

        public override bool PlayerIsInGarage(Item itemInHand)
        {
            if (itemInHand.ItemId != this.WorkingAttachmentPartId)
            {
                return false;
            }

            var activeQuest = this.GetQuest(Game1.player);
            if (activeQuest is null)
            {
                this.LogWarning($"An active quest for {this.GetType().Name} should exist, but doesn't?!");
                return false;
            }

            activeQuest.questComplete();
            Game1.player.removeFirstOfThisItemFromInventory(this.WorkingAttachmentPartId);
            Game1.DrawDialogue(new Dialogue(null, null, this.QuestCompleteMessage));
            return true;
        }

        protected virtual void HideStarterItemIfNeeded() { }

        protected void PlaceBrokenPartUnderClump(int preferredResourceClumpToHideUnder)
        {
            var farm = Game1.getFarm();
            var existing = farm.objects.Values.FirstOrDefault(o => o.ItemId == this.BrokenAttachmentPartId);
            if (existing is not null)
            {
                this.LogInfoOnce($"{this.BrokenAttachmentPartId} is already placed at {existing.TileLocation.X},{existing.TileLocation.Y}");
                return;
            }

            var position = this.FindPlaceToPutItem(preferredResourceClumpToHideUnder);
            if (position != default)
            {
                var o = ItemRegistry.Create<StardewValley.Object>(this.BrokenAttachmentPartId);
                o.questItem.Value = true;
                o.Location = Game1.getFarm();
                o.TileLocation = position;
                this.LogInfoOnce($"{this.BrokenAttachmentPartId} placed at {position.X},{position.Y}");
                o.IsSpawnedObject = true;
                farm.objects[o.TileLocation] = o;
            }
        }

        private Vector2 FindPlaceToPutItem(int preferredResourceClumpToHideUnder)
        {
            var farm = Game1.getFarm();
            var bottomMostResourceClump = farm.resourceClumps.Where(tf => tf.parentSheetIndex.Value == preferredResourceClumpToHideUnder).OrderByDescending(tf => tf.Tile.Y).FirstOrDefault();
            if (bottomMostResourceClump is not null)
            {
                return bottomMostResourceClump.Tile;
            }

            this.LogWarning($"Couldn't find the preferred location ({preferredResourceClumpToHideUnder}) for the {this.BrokenAttachmentPartId}");
            bottomMostResourceClump = farm.resourceClumps.OrderByDescending(tf => tf.Tile.Y).FirstOrDefault();
            if (bottomMostResourceClump is not null)
            {
                return bottomMostResourceClump.Tile;
            }

            this.LogWarning($"The farm contains no resource clumps under which to stick the {this.BrokenAttachmentPartId}");

            // We're probably dealing with an old save,  Try looking for any clear space.
            //  This technique is kinda dumb, but whatev's.  This mod is pointless on a fully-developed farm.
            for (int i = 0; i < 1000; ++i)
            {
                Vector2 positionToCheck = new Vector2(Game1.random.Next(farm.map.DisplayWidth / 64), Game1.random.Next(farm.map.DisplayHeight / 64));
                if (farm.CanItemBePlacedHere(positionToCheck))
                {
                    return positionToCheck;
                }
            }

            this.LogError($"Couldn't find any place at all to put the {this.BrokenAttachmentPartId}");
            return default;
        }
    }
}

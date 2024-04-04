using StardewValley;

namespace NermNermNerm.Stardew.QuestableTractor
{
    public abstract class TractorPartQuest<TQuestState> : BaseQuest<TQuestState> where TQuestState : struct
    {
        protected TractorPartQuest(TractorPartQuestController<TQuestState> controller) : base(controller) { }

        public new TractorPartQuestController<TQuestState> Controller => (TractorPartQuestController<TQuestState>)base.Controller;

        public abstract void GotWorkingPart(Item workingPart);

        public override bool IsConversationPiece(Item? item)
        {
            // The working items are never conversation pieces - they always just get dragged to the building.
            return item?.ItemId == this.Controller.BrokenAttachmentPartId; // || item?.ItemId == this.Controller.WorkingAttachmentPartId;
        }
    }
}

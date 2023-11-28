using System;
using System.Linq;
using StardewValley;

namespace NermNermNerm.Stardew.QuestableTractor
{
    public class ScytheQuest
        : BaseQuest<ScytheQuestState>
    {
        private bool jazTradeKnown;
        private bool vincentTradeKnown;
        private bool jazPartGot;
        private bool vincentPartGot;

        public ScytheQuest()
            : this(ScytheQuestState.NoCluesYet, false, false, false, false, null!)
        {
            this.showNew.Value = true;
        }

        internal ScytheQuest(ScytheQuestState questState, bool jazTradeKnown, bool vincentTradeKnown, bool jazPartGot, bool vincentPartGot, ScytheQuestController controller)
            : base(questState)
        {
            this.questTitle = "Fix the harvester";
            this.questDescription = "I found the harvester attachment for the tractor, but won't work like it is now.  I should ask around town about it.";
            this.jazPartGot = jazPartGot;
            this.vincentPartGot = vincentPartGot;
            this.jazTradeKnown = jazTradeKnown;
            this.vincentTradeKnown = vincentTradeKnown;
            this.Controller = controller;
            this.SetObjective();
        }

        public override void GotWorkingPart(Item workingPart)
        {
            this.State = ScytheQuestState.InstallPart;
        }

        public override void CheckIfComplete(NPC n, Item? item)
        {
            if (n.Name == "Clint" && this.State < ScytheQuestState.MissingParts)
            {
                this.Spout(n, "Let's see what you go there...$s#$b#You know, there's no real metalwork to do here.  It basically needs a good cleaning and oiling.$h#$b#But something seems wrong about it still, like are you sure you got all of it out of the weeds?");
                this.SetMissingPartsKnown();
            }
            else if (n.Name == "Robin" && this.State < ScytheQuestState.InstallPart)
            {
                this.Spout(n, "Oh you found the old harvester!  I think your Grandpa broke every other thing, but I don't recall any misadventures with that part.  If you can't get it to work, you might ask Demetrius or Maru.  They're good at metalworking.");
            }
            else if ((n.Name == "Maru" || n.Name == "Demetrius") && this.State < ScytheQuestState.InstallPart)
            {
                this.Spout(n, "Oh you found the old harvester!  You know, just looking at it, I think it's okay, but I don't think it's quite complete.  Looks like there might be some parts missing.#$b#You're getting pretty good around the farm, I'm sure you could get it to work again if you can find the rest of the parts.  Let me know if you need help with it, though, I'd be happy to help.");
                this.SetMissingPartsKnown();
            }
            else if (n.Name == "Lewis" && this.State < ScytheQuestState.InstallPart)
            {
                this.Spout(n, "Oh I'm not much for farm equipment.  You say that's a harvester?  Well, I'll have to take your word for it!$h#$b#Yaknow he broke a lot of stuff on that tractor, but I don't recall that being one of them.  Maybe it just needs cleaning and oiling?");
            }
            else if (n.Name == "Marnie" && this.State < ScytheQuestState.JazAndVincentFingered)
            {
                this.Spout(n, "Oh that's the old harvester!  Or most of one anyway...$s#$b#Well, did you look around where you found that piece?  Might be more pieces under that log.#$b#You might enlist Jaz and Vincent to help - they used to play out in your south pasture, but when you moved back in, I asked them to keep clear so you could get your work done.  But they know that area pretty well.");
                this.State = ScytheQuestState.JazAndVincentFingered;
            }
            else if (n.Name == "Marnie" && this.State < ScytheQuestState.JazAndVincentFingered)
            {
                this.Spout(n, "Oh that's a *harvester*?  I never would have guessed!  Yep, I saw it on your farm and always wondered about it...#$b#Well, unless I was prepared to accept Jaz and Vincent's explanation that it was a machine made by Greebles!$l");
                this.State = ScytheQuestState.JazAndVincentFingered;
            }
            else if (n.Name == "Penny" && this.State == ScytheQuestState.NoCluesYet)
            {
                this.Spout(n, "That's the old harvester for the tractor?  I guess it looks like that.#$b#I'm sorry, I don't know anything about farming.$2");
            }
            else if (n.Name == "Penny" && this.State == ScytheQuestState.MissingParts)
            {
                this.Spout(n, "That's the old harvester for the tractor?  I guess it looks like that.  You think it's incomplete?$s#$b#You might ask Jaz and Vincent to help look for other parts like that - they told me they used to play out in your south pasture, but they don't anymore because Marnie shooed them away.$4");
                this.State = ScytheQuestState.JazAndVincentFingered;
            }
            else if (n.Name == "Jodi" && this.State == ScytheQuestState.NoCluesYet)
            {
                this.Spout(n, "What is that thing?  A harvester?  Sure, if you say so...  I tell you though, something about that thing seems familiar to me.$s#$b#Well, it looks like a mess, and I've cleaned up a lot messes.  It must be that!$l");
            }
            else if (n.Name == "Jodi" && this.State == ScytheQuestState.MissingParts)
            {
                this.Spout(n, "That's the old harvester for the tractor?  I guess it looks like that.  You think it's incomplete?$s#$b#You might ask Jaz and Vincent to help look for other parts like that - they used to play out in your south pasture, but don't worry!  Marnie and I asked them not to since you moved in.#$b#You've got enough on your hands without those two hooligans running through the corn!$l");
                this.State = ScytheQuestState.JazAndVincentFingered;
            }
            else if (n.Name == "Wizard" && this.State < ScytheQuestState.InstallPart && (Game1.Date.DayOfWeek == DayOfWeek.Friday || Game1.Date.DayOfWeek == DayOfWeek.Saturday))
            {
                this.Spout(n, "Aha!!!!  I FORSEE THAT YOU HAVE NEED OF A SCRY!$a#$b#And you've come at the perfect time.  I happen to have need to flex my muscles from time to time, to keep in tip top form, you know.  Let's have a look#$b#Aahh through the mists I see two people, NO, CHILDREN, in your farm playing...#$b#They have taken parts of off and headed off...  But wait, there's more...  Yes...#$b#They will each need to be placated to get them to give the parts back.  The boy will want 3 crayfish and the girl will want a gemstone, but her tastes are not yet developed, so a cheap one will do...");
                this.jazTradeKnown = true;
                this.vincentTradeKnown = true;
                this.State = ScytheQuestState.JazAndVincentFingered;
            }
            else if (n.Name == "Wizard" && item is not null && (Game1.Date.DayOfWeek == DayOfWeek.Friday || Game1.Date.DayOfWeek == DayOfWeek.Saturday))
            {
                this.Spout(n, "BAH!  Begone with this mundane contrivance!$a");
            }
            else if (n.Name == "Jas" && this.jazPartGot)
            {
                this.Spout(n, "I love how my jumprope sparkles in the sun now!  Did you have any luck getting it to fit back together?");
            }
            else if (n.Name == "Jas" && this.jazTradeKnown)
            {
                // It'd be nice if there was a way to make this a little more interactive with Jaz having, like a random taste-of-the-day and
                //  only one gem will be shiny enough on that day.  I don't see a way to make that happen right now.
                foreach (string shinyItemId in new string[] { "80" /* quartz */, "82" /* fire quartz */, "68" /* topaz */, "66" /* amethyst */, "60" /* Emerald */, "62" /* Aquamarine */, "70" /* jade */})
                {
                    var shinyThing = Game1.player.Items.FirstOrDefault(i => i?.ItemId == shinyItemId);
                    if (shinyThing is not null)
                    {
                        _ = this.TryTakeItemsFromPlayer(shinyItemId, 1);

                        this.AddItemToInventory(ObjectIds.ScythePart2);

                        this.Spout(n, $"Ooh!  Oh this {shinyThing.DisplayName} is very sparkly!  Thanks!  Here's your thingamajig.  I sure hope it works!  I really want to ride on your tractor some day!$l");
                        this.jazPartGot = true;
                        this.SetObjective();
                        return;
                    }
                }

                this.Spout(n, "Oohh when are you gonna bring me something shiny!  I'm really looking forward to it!");
            }
            else if (n.Name == "Jas")
            {
                this.Spout(n, "Ooh!  You found the Greeble machine!$h#$b#Vincent and I used to play games with that, but we had to stop because Aunt Marnie told us we couldn't go into your pasture anymore unless you invite us.#$b#There are parts missing?  Well of course there are!  Vincent and I kept some of the shinier bits, see, one's on my jumprope!  It's so shiny, it sparkles when I jump!  Oh, and FINDERS KEEPERS!$l#$b#Well, I suppose I could give it back to you...  BUT ONLY IF YOU TRADE ME SOMETHING REALLY SHINY!$h");
                this.jazTradeKnown = true;
                this.State = ScytheQuestState.JazAndVincentFingered;
                this.SetObjective();
            }
            else if (n.Name == "Vincent" && this.vincentPartGot)
            {
                this.Spout(n, "Did you have any luck getting it to fit back together?  I could probably help you put it back together, it sure came apart easy!$l");
            }
            else if (n.Name == "Vincent" && this.vincentTradeKnown && this.TryTakeItemsFromPlayer("716", 3))
            {
                this.AddItemToInventory(ObjectIds.ScythePart1);
                this.Spout(n, "Ooh!  Oh these 3 crawdads would be great!  Thanks!  Here's your thingamajig.  I Hope it works!$l#$b#Sure, I won't tell Mom that you gave them to me if you want...  Why?$s");
                this.vincentPartGot = true;
                this.SetObjective();
            }
            else if (n.Name == "Vincent" && this.vincentTradeKnown)
            {
                this.Spout(n, "Did you get the bugs?  Big ones?");
            }
            else if (n.Name == "Vincent")
            {
                this.Spout(n, "The Greeble machine!$h#$b#Jaz and I used to play games with that, but we had to stop because Marnie told us we couldn't go into your pasture anymore unless you invite us.#$b#There are parts missing?  Sure there are!  Me and Jaz took a couple of pieces.  I used mine in a trap I was building to trap the Greebles under my bed!  It didn't work.  Hey, have you got any good bugs on your farm?  I'll find it for you if you can bring me some really big crawly ones!$h");
                this.vincentTradeKnown = true;
                this.State = ScytheQuestState.JazAndVincentFingered;
                this.SetObjective();
            }
            // else this.Spout(n, "What is that thing?  A harvester?  Sure, if you say so...$s");
        }

        private void SetMissingPartsKnown()
        {
            if (this.State == ScytheQuestState.NoCluesYet)
            {
                this.State = ScytheQuestState.MissingParts;
                this.SetObjective();
            }
        }


        protected override void SetObjective()
        {
            switch (this.State)
            {
                case ScytheQuestState.NoCluesYet:
                    this.currentObjective = "Ask the people in town about this thing.";
                    break;
                case ScytheQuestState.MissingParts:
                    this.currentObjective = "It seems some parts are missing, but I know there's nothing else out there in that field, at least not that's easy to see.  Maybe somebody else who noses around the farm would know about it.";
                    break;
                case ScytheQuestState.JazAndVincentFingered:
                    if (!this.jazTradeKnown || !this.vincentTradeKnown)
                    {
                        this.currentObjective = "Ask Jaz and Vincent about the harvester";
                    }
                    else if (this.jazPartGot && this.vincentPartGot)
                    {
                        this.currentObjective = "Fix the scythe attachment (HINT: craft it from the parts)";
                    }
                    else if (this.jazTradeKnown && this.vincentTradeKnown)
                    {
                        this.currentObjective = "Get a 'shiny thing' for Jaz (perhaps a gem?) and 3 big bugs for Vincent.  (Hm.  The bugs in the mines seem TOO big...  Maybe a lobster?  Hm.  Again maybe too big...)";
                    }
                    else
                    {
                        this.currentObjective = "Find a way to get the kids to give me the parts";
                    }
                    break;
                case ScytheQuestState.InstallPart:
                    this.currentObjective = "Take the fixed scythe attachment to the tractor garage.";
                    break;
            }
        }

        public override string Serialize()
            => FormattableString.Invariant($"{this.State},{this.jazTradeKnown},{this.vincentTradeKnown},{this.jazPartGot},{this.vincentPartGot}");
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley;

using static NermNermNerm.Stardew.LocalizeFromSource.SdvLocalize;

namespace NermNermNerm.Stardew.QuestableTractor
{
    public class ScytheQuest
        : TractorPartQuest<ScytheQuestState>
    {
        private const string crawfishQiid = "(O)716";
        private static readonly HashSet<string> shinyItems = new HashSet<string>() { "(O)80" /* quartz */, "(O)82" /* fire quartz */, "(O)68" /* topaz */, "(O)66" /* amethyst */, "(O)60" /* Emerald */, "(O)62" /* Aquamarine */, "(O)70" /* jade */};

        internal ScytheQuest(ScytheQuestController controller)
            : base(controller)
        {
            this.questTitle = L("Fix the harvester");
            this.questDescription = L("I found the harvester attachment for the tractor, but won't work like it is now.  I should ask around town about it.");
        }

        public override void GotWorkingPart(Item workingPart)
        {
            this.State = this.State with { Progress = ScytheQuestStateProgress.InstallPart };
        }

        public override void CheckIfComplete(NPC n, Item? item)
        {
            bool itemIsBrokenPart = item?.ItemId == ObjectIds.BustedScythe;
            if (n.Name == "Clint" && this.State.Progress < ScytheQuestStateProgress.MissingParts)
            {
                this.Spout(n, L("Let's see what you go there...$s#$b#You know, there's no real metalwork to do here.  It basically needs a good cleaning and oiling.$h#$b#But something seems wrong about it still, like are you sure you got all of it out of the weeds?"));
                this.State = new ScytheQuestState { Progress = ScytheQuestStateProgress.MissingParts };
            }
            else if (n.Name == "Robin" && itemIsBrokenPart && this.State.Progress < ScytheQuestStateProgress.InstallPart)
            {
                this.Spout(n, L("Oh you found the old harvester!  I think your Grandpa broke every other thing, but I don't recall any misadventures with that part.  If you can't get it to work, you might ask Demetrius or Maru.  They're good at metalworking."));
            }
            else if ((n.Name == "Maru" || n.Name == "Demetrius") && this.State.Progress < ScytheQuestStateProgress.MissingParts)
            {
                this.Spout(n, L("Oh you found the old harvester!  You know, just looking at it, I think it's okay, but I don't think it's quite complete.  Looks like there might be some parts missing.#$b#You're getting pretty good around the farm, I'm sure you could get it to work again if you can find the rest of the parts.  Let me know if you need help with it, though, I'd be happy to help."));
                this.State = new ScytheQuestState { Progress = ScytheQuestStateProgress.MissingParts };
            }
            else if (n.Name == "Lewis" && itemIsBrokenPart && this.State.Progress < ScytheQuestStateProgress.InstallPart)
            {
                this.Spout(n, L("Oh I'm not much for farm equipment.  You say that's a harvester?  Well, I'll have to take your word for it!$h#$b#Yaknow he broke a lot of stuff on that tractor, but I don't recall that being one of them.  Maybe it just needs cleaning and oiling?"));
            }
            else if (n.Name == "Marnie" && itemIsBrokenPart && this.State.Progress < ScytheQuestStateProgress.JasAndVincentFingered)
            {
                this.Spout(n, L("Oh that's the old harvester!  Or most of one anyway...$s#$b#Well, did you look around where you found that piece?  Might be more pieces under that log.#$b#You might enlist Jas and Vincent to help - they used to play out in your south pasture, but when you moved back in, I asked them to keep clear so you could get your work done.  But they know that area pretty well."));
                this.State = new ScytheQuestState { Progress = ScytheQuestStateProgress.JasAndVincentFingered };
            }
            else if (n.Name == "Marnie" && itemIsBrokenPart && this.State.Progress < ScytheQuestStateProgress.JasAndVincentFingered)
            {
                this.Spout(n, L("Oh that's a *harvester*?  I never would have guessed!  Yep, I saw it on your farm and always wondered about it...#$b#Well, unless I was prepared to accept Jas and Vincent's explanation that it was a machine made by Greebles!$l"));
                this.State = new ScytheQuestState { Progress = ScytheQuestStateProgress.JasAndVincentFingered };
            }
            else if (n.Name == "Penny" && itemIsBrokenPart && this.State.Progress == ScytheQuestStateProgress.NoCluesYet)
            {
                this.Spout(n, L("That's the old harvester for the tractor?  I guess it looks like that.#$b#I'm sorry, I don't know anything about farming.$2"));
            }
            else if (n.Name == "Penny" && this.State.Progress == ScytheQuestStateProgress.MissingParts)
            {
                this.Spout(n, L("Missing parts for a 'harvester'?$s#$b#You might ask Jas and Vincent to help look for other parts like that - they told me they used to play out in your south pasture, but they don't anymore because Marnie shooed them away.$4"));
                this.State = new ScytheQuestState { Progress = ScytheQuestStateProgress.JasAndVincentFingered };
            }
            else if (n.Name == "Abigail" && (itemIsBrokenPart || this.State.Progress < ScytheQuestStateProgress.JasAndVincentFingered))
            {
                this.Spout(n, L("Oh yeah I've seen that - wedged under a tree, right?  Must have been a feat to get it out!#$b#Did Jas or Vincent tell you where to find it?  They used to play near it quite a bit."));
                this.State = new ScytheQuestState { Progress = ScytheQuestStateProgress.JasAndVincentFingered };
            }
            else if (n.Name == "Jodi" && itemIsBrokenPart && this.State.Progress == ScytheQuestStateProgress.NoCluesYet)
            {
                this.Spout(n, L("What is that thing?  A harvester?  Sure, if you say so...  I tell you though, something about that thing seems familiar to me.$s#$b#Well, it looks like a mess, and I've cleaned up a lot messes.  It must be that!$l"));
            }
            else if (n.Name == "Jodi" && this.State.Progress == ScytheQuestStateProgress.MissingParts)
            {
                this.Spout(n, L("That's the old harvester for the tractor?  I guess it looks like that.  You think it's incomplete?$s#$b#You might ask Jas and Vincent to help look for other parts like that - they used to play out in your south pasture, but don't worry!  Marnie and I asked them not to since you moved in.#$b#You've got enough on your hands without those two hooligans running through the corn!$l"));
                this.State = new ScytheQuestState { Progress = ScytheQuestStateProgress.JasAndVincentFingered };
            }
            else if (n.Name == "Wizard"
                && (itemIsBrokenPart || (!this.State.JasTradeKnown || !this.State.VincentTradeKnown))
                && (!this.State.JasPartGot || !this.State.VincentPartGot)
                && (Game1.Date.DayOfWeek == DayOfWeek.Friday || Game1.Date.DayOfWeek == DayOfWeek.Saturday))
            {
                this.Spout(n, L("Aha!!!!  I FORESEE THAT YOU HAVE NEED OF A SCRY!$a#$b#And you've come at the perfect time.  I happen to have need to flex my muscles from time to time, to keep in tip top form, you know.  Let's have a look#$b#Aahh through the mists I see two people, NO, CHILDREN, in your farm playing...#$b#They have taken parts of off and headed off...  But wait, there's more...  Yes...#$b#They will each need to be placated to get them to give the parts back.  The boy will want 3 crayfish and the girl will want a gemstone, but her tastes are not yet developed, so a cheap one will do..."));
                this.State = this.State with { Progress = ScytheQuestStateProgress.JasAndVincentFingered }; // Don't set TradeKnown because the kids don't know you know.
            }
            else if (n.Name == "Wizard" && itemIsBrokenPart && (!this.State.JasPartGot || !this.State.VincentPartGot) && !(Game1.Date.DayOfWeek == DayOfWeek.Friday || Game1.Date.DayOfWeek == DayOfWeek.Saturday))
            {
                this.Spout(n, L("BAH!  Begone with this mundane contrivance!  Return on Friday or Saturday and I may have time.$a"));
            }
            else if (n.Name == "Jas" && itemIsBrokenPart && this.State.JasPartGot)
            {
                this.Spout(n, L("I love how my jumprope sparkles in the sun now!  Did you have any luck getting it to fit back together?"));
            }
            else if (n.Name == "Jas" && this.State.JasTradeKnown && !this.State.JasPartGot)
            {
                // It'd be nice if there was a way to make this a little more interactive with Jas having, like a random taste-of-the-day and
                //  only one gem will be shiny enough on that day.  I don't see a way to make that happen right now.
                foreach (string shinyItemId in shinyItems)
                {
                    var shinyThing = Game1.player.Items.FirstOrDefault(i => i?.QualifiedItemId == shinyItemId && i.Stack > 0);
                    if (shinyThing is not null)
                    {
                        _ = this.TryTakeItemsFromPlayer(shinyItemId, 1); // Guaranteed true

                        this.AddQuestItemToInventory(ObjectIds.ScythePart2);

                        this.Spout(n, LF($"Ooh!  Oh this {shinyThing.DisplayName} is very sparkly!  Thanks!  Here's your thingamajig.  I sure hope it works!  I really want to ride on your tractor some day!$l"));
                        this.State = this.State with { JasPartGot = true };
                        return;
                    }
                }

                if (itemIsBrokenPart)
                {
                    this.Spout(n, L("Oohh when are you gonna bring me something shiny!  I'm really looking forward to it!"));
                }
            }
            else if (n.Name == "Jas" && !this.State.JasTradeKnown)
            {
                this.Spout(n, L("Ooh!  You found the Greeble machine!$h#$b#Vincent and I used to play games with that, but we had to stop because Aunt Marnie told us we couldn't go into your pasture anymore unless you invite us.#$b#There are parts missing?  Well of course there are!  Vincent and I kept some of the shinier bits, see, one's on my jumprope!  It's so shiny, it sparkles when I jump!  Oh, and FINDERS KEEPERS!$l#$b#Well, I suppose I could give it back to you...  BUT ONLY IF YOU TRADE ME SOMETHING REALLY SHINY!$h"));
                this.State = this.State with { Progress = ScytheQuestStateProgress.JasAndVincentFingered, JasTradeKnown = true };
            }
            else if (n.Name == "Vincent" && itemIsBrokenPart && this.State.VincentPartGot)
            {
                this.Spout(n, L("Did you have any luck getting it to fit back together?  I could probably help you put it back together, it sure came apart easy!$l"));
            }
            else if (n.Name == "Vincent" && this.State.VincentTradeKnown && !this.State.VincentPartGot && this.TryTakeItemsFromPlayer(crawfishQiid, 3))
            {
                this.AddQuestItemToInventory(ObjectIds.ScythePart1);
                this.Spout(n, L("Ooh!  Oh these 3 crawdads would be great!  Thanks!  Here's your thingamajig.  I Hope it works!$l#$b#Sure, I won't tell Mom that you gave them to me if you want...  Why?$s"));
                this.State = this.State with { VincentPartGot = true };
            }
            else if (n.Name == "Vincent" && itemIsBrokenPart && this.State.VincentTradeKnown && !this.State.VincentPartGot)
            {
                this.Spout(n, L("Did you get the bugs?  3 Big ones?"));
            }
            else if (n.Name == "Vincent" && !this.State.VincentTradeKnown)
            {
                this.Spout(n, L("The Greeble machine!$h#$b#Jas and I used to play games with that, but we had to stop because Marnie told us we couldn't go into your pasture anymore unless you invite us.#$b#There are parts missing?  Sure there are!  Me and Jas took a couple of pieces.  I used mine in a trap I was building to trap the Greebles under my bed!  It didn't work.  Hey, have you got any good bugs on your farm?  I'll find it for you if you can bring me some really big crawly ones!$h"));
                this.State = this.State with { Progress = ScytheQuestStateProgress.JasAndVincentFingered, VincentTradeKnown = true };
            }
            // else this.Spout(n, "What is that thing?  A harvester?  Sure, if you say so...$s");
        }


        protected override void SetObjective()
        {
            switch (this.State.Progress)
            {
                case ScytheQuestStateProgress.NoCluesYet:
                    this.currentObjective = L("Ask the people in town about this thing.");
                    break;
                case ScytheQuestStateProgress.MissingParts:
                    this.currentObjective = L("It seems some parts are missing, but I know there's nothing else out there in that field, at least not that's easy to see.  Maybe somebody else who noses around the farm would know about it.");
                    break;
                case ScytheQuestStateProgress.JasAndVincentFingered:
                    if (!this.State.JasTradeKnown || !this.State.VincentTradeKnown)
                    {
                        this.currentObjective = L("Ask Jas and Vincent about the harvester");
                    }
                    else if (this.State.JasPartGot && this.State.VincentPartGot)
                    {
                        this.currentObjective = L("Fix the scythe attachment (HINT: craft it from the parts)");
                    }
                    else if (this.State.JasTradeKnown && this.State.VincentTradeKnown)
                    {
                        this.currentObjective = L("Get a 'shiny thing' for Jas (perhaps a gem?) and 3 big bugs for Vincent.  (Hm.  The bugs in the mines seem TOO big...  Maybe a lobster?  Hm.  Again maybe too big...)");
                    }
                    else
                    {
                        this.currentObjective = L("Find a way to get the kids to give me the parts");
                    }
                    break;
                case ScytheQuestStateProgress.InstallPart:
                    this.currentObjective = L("Take the fixed scythe attachment to the tractor garage.");
                    break;
            }
        }
    }
}

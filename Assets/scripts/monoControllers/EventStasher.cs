using System.Collections;
using System.Collections.Generic;
using CourtNS;

namespace MonoNS {
  public class EventStasher : BaseController {
    public bool stepAnimating = false;
    public override void PreGameInit(HexMap hexMap, BaseController me)
    {
      base.PreGameInit(hexMap, me);
    }

    class Stash {
      public General general;
      public EventDialog.EventName eventName;
      public int turns2Expire;

      public Stash(General general, EventDialog.EventName eventName) {
        this.general = general;
        this.eventName = eventName;
        this.turns2Expire = Util.Rand(2, 6);
      }
    }

    EventDialog eventDialog {
      get {
        return hexMap.eventDialog;
      }
    }

    List<Stash> stash = new List<Stash>();
    public override void UpdateChild() {}

    public void Step()
    {
      stepAnimating = true;
      StartCoroutine(CoStep());
    }

    IEnumerator CoStep() {
      List<Stash> newStash = new List<Stash>();

      foreach(Stash s in stash) {
        s.turns2Expire -= 1;
        if (s.turns2Expire > 0) {
          newStash.Add(s);
        } else if (s.general.IsOnField()) {

          if (s.eventName == EventDialog.EventName.FarmDestroyed) {
            if ((s.general.party.GetRelation() == Party.Relation.tense && Cons.MostLikely())
              || (s.general.party.GetRelation() == Party.Relation.xTense) && Cons.FiftyFifty()) {
              UnitNS.Unit unit = s.general.commandUnit.onFieldUnit;
              int influence = 100;
              s.general.party.influence -= influence;
              eventDialog.Show(new MonoNS.Event(MonoNS.EventDialog.EventName.FarmDestroyedReported,
                unit,
                null, influence));
              while (eventDialog.Animating) { yield return null; }
            }
          }

        }
      }

      stash = newStash;
      stepAnimating = false;
    }

    public void Add(General general, EventDialog.EventName eventName) {
      stash.Add(new Stash(general, eventName));
    }

  }

}
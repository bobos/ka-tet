namespace MapTileNS
{
  public class DeadZone
  {
    public const int CorpseThreshold = 500; 
    public const int CorpseLastInSummer = 2; 
    public const int CorpseLastInSpring = 4; 
    public const int CorpseLastInAutumn = 3; 
    public const int CorpseLastInWinter = 10; 
    public int corpseNum = 0;
    public int DecompositionCntDown = 0;

    Tile tile;

    public DeadZone(Tile tile) {
      this.tile = tile;
    }

    public void OnTurnEnd() {
      if (DecompositionCntDown == 0) {
        return;
      }

      DecompositionCntDown--;
      if (DecompositionCntDown == 0)
      {
        // TODO: contribute corpseNum to global sickness point
        corpseNum = 0;
        tile.SetFieldType(tile.field);
      }
    }

    public void Clean() {
      DecompositionCntDown = 0;
      corpseNum = 0;
      tile.SetFieldType(tile.field);
    }

    public void Occur(int deadNum) {
      if (deadNum < CorpseThreshold)
      {
        return;
      }
      corpseNum += deadNum;
      if (Cons.IsSpring(tile.weatherGenerator.season)) {
        DecompositionCntDown = CorpseLastInSpring;
      }
      if (Cons.IsSummer(tile.weatherGenerator.season)) {
        DecompositionCntDown = CorpseLastInSummer;
      }
      if (Cons.IsAutumn(tile.weatherGenerator.season)) {
        DecompositionCntDown = CorpseLastInAutumn;
      }
      if (Cons.IsWinter(tile.weatherGenerator.season)) {
        DecompositionCntDown = CorpseLastInWinter;
      }
      tile.SetFieldType(tile.field);
    }

    public bool Apply(UnitNS.Unit unit) {
      return DecompositionCntDown > 0 && unit != null &&
        !unit.epidemic.IsValid() && Cons.HighlyLikely();
    }
  }

}
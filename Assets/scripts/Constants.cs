using NatureNS;
using TextNS;
using CourtNS;

public class Cons {
  
  public const int InterceptMoraleImpact = 5;
  public static bool HighlyLikely() {
    return Util.Rand(0f, 1f) < 0.8f;
  }

  public static bool MostLikely() {
    return Util.Rand(0f, 1f) < 0.65f;
  }

  public static bool FiftyFifty() {
    return Util.Rand(0f, 1f) < 0.5f;
  }

  public static bool EvenChance() {
    return Util.Rand(0f, 1f) < 0.45f;
  }

  public static bool FairChance() {
    return Util.Rand(0f, 1f) < 0.25f;
  }

  public static bool SlimChance() {
    return Util.Rand(0f, 1f) < 0.1f;
  }

  public static bool TinyChance() {
    return Util.Rand(0f, 1f) < 0.06f;
  }

  public static Season spring = new Spring();
  public static bool IsSpring(Season season) {
    return Util.eq<Season>(season, spring);
  }

  public static Season summer = new Summer();
  public static bool IsSummer(Season season) {
    return Util.eq<Season>(season, summer);
  }

  public static Season autumn = new Autumn();
  public static bool IsAutumn(Season season) {
    return Util.eq<Season>(season, autumn);
  }

  public static Season winter = new Winter();
  public static bool IsWinter(Season season) {
    return Util.eq<Season>(season, winter);
  }

  public static Weather cloudy = new Cloudy();
  public static bool IsCloudy(Weather weather) {
    return Util.eq<Weather>(weather, cloudy);
  }
  public static Weather rain = new Rain();
  public static bool IsRain(Weather weather) {
    return Util.eq<Weather>(weather, rain);
  }
  public static Weather heavyRain = new HeavyRain();
  public static bool IsHeavyRain(Weather weather) {
    return Util.eq<Weather>(weather, heavyRain);
  }
  public static Weather heat = new Heat();
  public static bool IsHeat(Weather weather) {
    return Util.eq<Weather>(weather, heat);
  }
  public static Weather dry = new Dry();
  public static bool IsDry(Weather weather) {
    return Util.eq<Weather>(weather, dry);
  }
  public static Weather snow = new Snow();
  public static bool IsSnow(Weather weather) {
    return Util.eq<Weather>(weather, snow);
  }
  public static Weather blizard = new Blizard();
  public static bool IsBlizard(Weather weather) {
    return Util.eq<Weather>(weather, blizard);
  }

  public static Current nowind = new Nowind();
  public static bool IsNowind(Current current) {
    return Util.eq<Current>(current, nowind);
  }

  public static Current wind = new Wind();
  public static bool IsWind(Current current) {
    return Util.eq<Current>(current, wind);
  }

  public static Current gale = new Gale();
  public static bool IsGale(Current current) {
    return Util.eq<Current>(current, gale);
  }

  public enum Direction {
    dueNorth,
    dueSouth,
    southEast,
    southWest,
    northEast,
    northWest
  }

  public static string DirectionDisplay(Direction direction) {
    TextLib textLib = GetTextLib();
    if (direction == Direction.dueNorth) return textLib.get("d_dueNorth");
    if (direction == Direction.dueSouth) return textLib.get("d_dueSouth");
    if (direction == Direction.northEast) return textLib.get("d_northEast");
    if (direction == Direction.northWest) return textLib.get("d_northWest");
    if (direction == Direction.southEast) return textLib.get("d_southEast");
    return textLib.get("d_southWest");
  }

  public static TextLib textLib;
  public static TextLib GetTextLib () {
    // TODO: use strategy pattern
    if (textLib == null) {
      textLib = new TextLibChn();
    }
    return textLib;
  }

  public static UnitNS.Rank rookie = new UnitNS.Rookie();
  public static UnitNS.Rank veteran = new UnitNS.Veteran();
  public static UnitNS.Rank elite = new UnitNS.Elite();

  public static Region upLand = new Upland();
  public static Region lowLand = new Lowland();
  public static Region plain = new Plain();
  public static Region hillLand = new Hillland();
  public static Region grassLand = new Grassland();

  public static Province riverRun = new RiverRun(hillLand);
  public static Province riverSouth = new RiverSouth(plain);
  public static Province riverNorth = new RiverNorth(plain);
  public static Province riverWest = new RiverWest(upLand);
  public static Province riverEast = new RiverEast(plain);
  public static Province middleEarth = new MiddleEarth(plain);
  public static Province farWest = new FarWest(upLand);
  public static Province farNorth = new FarNorth(grassLand);
  public static Province huaiWest = new HuaiWest(lowLand);
  public static Province huaiNorth = new HuaiNorth(lowLand);
  public static Province huaiSouth = new HuaiSouth(lowLand);

  public static void Init() {
    NewParty.counterParty = OldParty;
    OldParty.counterParty = NewParty;
    NorthCourt.counterParty = SouthCourt;
    SouthCourt.counterParty = NorthCourt;

    Cons.upLand.UncomfortableRegions.Add(plain);
    Cons.upLand.UncomfortableRegions.Add(lowLand);

    Cons.plain.UncomfortableRegions.Add(upLand);
    Cons.plain.UncomfortableRegions.Add(lowLand);

    Cons.grassLand.UncomfortableRegions.Add(lowLand);
    Cons.grassLand.UncomfortableRegions.Add(upLand);

    Cons.lowLand.UncomfortableRegions.Add(upLand);
  }

  //* parties */
  public static Party NewParty = new Party("p_newParty", "p_newParty_d");
  public static Party OldParty = new Party("p_oldParty", "p_oldParty_d");
  public static Party NorthCourt = new Party("p_northCourt", "p_northCourt_d");
  public static Party SouthCourt = new Party("p_southCourt", "p_southCourt_d");
  public static Party NoParty = new Party("p_noParty", "p_noParty_d");

  //* factions */
  public static Faction Liao = new Liao(true, 5000000);
  public static Faction Song = new Song(false, 8000000);
  public static Faction Xia = new Xia(true, 2000000);
  public static Faction GF = new Ghost();

}

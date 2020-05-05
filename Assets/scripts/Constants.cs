using NatureNS;
using TextNS;
using CourtNS;

public class Cons {
  
  public const int InterceptMoraleImpact = 5;
  public static bool HighlyLikely() {
    return Util.Rand(0f, 1f) < 0.9f;
  }

  public static bool MostLikely() {
    return Util.Rand(0f, 1f) < 0.75f;
  }

  public static bool FiftyFifty() {
    return Util.Rand(0f, 1f) < 0.5f;
  }

  public static bool EvenChance() {
    return MostLikely() && MostLikely();
  }

  public static bool FairChance() {
    return FiftyFifty() && FiftyFifty();
  }

  public static bool SlimChance() {
    return FairChance() && FairChance();
  }

  public static bool TinyChance() {
    return SlimChance() && SlimChance();
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
  public static Weather mist = new Mist();
  public static bool IsMist(Weather weather) {
    return Util.eq<Weather>(weather, mist);
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

  public static bool IsQidan(Region region) {
    return Util.eq<Region>(Cons.qidan, region);
  }

  public static bool IsDangxiang(Region region) {
    return Util.eq<Region>(Cons.dangxiang, region);
  }

  public static bool IsTubo(Region region) {
    return Util.eq<Region>(Cons.tubo, region);
  }

  public static bool IsNvzhen(Region region) {
    return Util.eq<Region>(Cons.nvzhen, region);
  }

  public static bool IsHan(Region region) {
    return Util.eq<Region>(Cons.han, region);
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

  public static Region han = new HanRegion();
  public static Region qidan = new QidanRegion();
  public static Region dangxiang = new DangxiangRegion();
  public static Region tubo = new TuboRegion();
  public static Region nvzhen = new NvZhenRegion();

  public static Province heHuang = new HeHuang(tubo);
  public static Province heXi = new HeXi(dangxiang);
  public static Province heDong = new HeDong(han);
  public static Province heBei = new HeBei(han);
  public static Province heNan = new HeNan(han);
  public static Province shanXi = new ShanXi(han);
  public static Province xiJing = new XiJing(qidan);
  public static Province zhongJing = new ZhongJing(han);
  public static Province shangJing = new ShangJing(nvzhen);

  public static void Init() {
    NewParty.counterParty = OldParty;
    OldParty.counterParty = NewParty;
    NorthCourt.counterParty = SouthCourt;
    SouthCourt.counterParty = NorthCourt;
    Liao.AddProvince(shangJing);
    Liao.AddProvince(zhongJing);
    Liao.AddProvince(xiJing);

    shangJing.ownerFaction = zhongJing.ownerFaction = xiJing.ownerFaction = Liao;
    shangJing.ownerParty = zhongJing.ownerParty = NorthCourt;
    xiJing.ownerParty = SouthCourt;
    
    Song.AddProvince(heBei);
    Song.AddProvince(heDong);
    Song.AddProvince(heNan);
    Song.AddProvince(shanXi);
    heBei.ownerFaction = heDong.ownerFaction = heNan.ownerFaction = shanXi.ownerFaction = Song;
    heBei.ownerParty = heDong.ownerParty = OldParty;
    shanXi.ownerParty = heNan.ownerParty = NewParty;
    
    Xia.AddProvince(heXi);
    heXi.ownerFaction = Xia;
    heXi.ownerParty = NoParty;
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

  // abilities
  public static Ability forecaster = new Forecaster();
  public static Ability discipline = new Discipline();
  public static Ability hammer = new Hammer();
  public static Ability mechanician = new Mechanician();
  public static Ability diminisher = new Diminisher();
  public static Ability staminaManager = new StaminaManager();
  public static Ability generous = new Generous();
  public static Ability runner = new Runner();
  public static Ability fireBug = new FireBug();
  public static Ability holdTheGround = new HoldTheGround();
  public static Ability breaker = new Breaker();
  public static Ability tactic = new Tactic();
  public static Ability outlooker = new Outlooker();
  public static Ability ambusher = new Ambusher();
  public static Ability improvisor = new Improvisor();
  public static Ability formidable = new Formidable();
  public static Ability doctor = new Doctor();

  // trait
  public static Trait reckless = new Reckless();
  public static Trait brave = new Brave();
  public static Trait loyal = new Loyal();
  public static Trait conservative = new Conservative();
  public static Trait cunning = new Cunning();
  public static Trait calm = new Calm();

}

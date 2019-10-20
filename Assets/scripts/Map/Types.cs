using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MapTileNS
{
  public enum TerrianType {
    Mountain,
    Hill,
    Plain,
    Water
  }

  public enum FieldType {
    Wild,
    Village,
    Road,
    Settlement,
    Schorched,
    Flooded,
    Burning,
    Flooding
  }

  public enum DisasterType {
    Flood,
    WildFire
  }
}
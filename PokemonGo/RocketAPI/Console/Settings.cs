using POGOProtos.Inventory.Item;
using PokemonGo.RocketAPI.Enums;
using System.Collections.Generic;

namespace PokemonGo.RocketAPI.Console
{
    public static class Settings
    {
        //Fetch these settings from intercepting the /auth call in headers and body (only needed for google auth)
        public static AuthType AuthType = Enums.AuthType.Google;
        public static string PtcUsername  = "User";
        public static string PtcPassword = "alligator2";
        public static string GoogleRefreshToken = string.Empty;

        public static double DefaultLatitude = 40.764858121285975;
        public static double DefaultLongitude = -73.97272288799286;
        public static double SantaMonicaLatitude = 34.009713;
        public static double SantaMonicaLongitude = -118.496104;
        public static double SanFranLatitude = 34.009713;
        public static double SanFranLongitude = -118.496104;
        public static double SidneyLatitude = -33.86467850677313;
        public static double SidneyLongitude = 151.21024131774902;

        public static double DratiniLatitude = 38.54292094612488;
        public static double DratiniLongitude = -121.22782230377197;
        public static bool DratiniMode = false;
        public static bool UsingIV = true;
        public static bool RenameMode = true;

        //San Fran 37.80788523279169,-122.41833686828613
        //Sydney -33.86467850677313,151.21024131774902

        public static ICollection<KeyValuePair<ItemId, int>> ItemRecycleFilter = new List<KeyValuePair<ItemId, int>>();

        //Miami long: -80.1917902, lat: 25.7616798
        //public static double DefaultLatitude = 40.764858121285975;
        //public static double DefaultLongitude = -73.97272288799286;
    }
}

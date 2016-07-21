using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.GeneratedCode;
using System.Collections.Generic;

namespace PokemonGo.RocketAPI.Console
{
    public static class Settings
    {
        //Fetch these settings from intercepting the /auth call in headers and body (only needed for google auth)
        public const AuthType AuthType = Enums.AuthType.Google;
        public static string PtcUsername  = "User";
        public static string PtcPassword = "alligator2";
        public static string GoogleRefreshToken = string.Empty;

        public static double DefaultLatitude = 40.764858121285975;
        public static double DefaultLongitude = -73.97272288799286;

        public static ICollection<KeyValuePair<AllEnum.ItemId, int>> ItemRecycleFilter = new List<KeyValuePair<AllEnum.ItemId, int>>();

        //Miami long: -80.1917902, lat: 25.7616798
        //public static double DefaultLatitude = 40.764858121285975;
        //public static double DefaultLongitude = -73.97272288799286;
    }
}

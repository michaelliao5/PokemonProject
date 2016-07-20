using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.GeneratedCode;

namespace PokemonGo.RocketAPI.Console
{
    public static class Settings
    {
        //Fetch these settings from intercepting the /auth call in headers and body (only needed for google auth)
        public const AuthType AuthType = Enums.AuthType.Google;
        public const string PtcUsername  = "User";
        public const string PtcPassword = "alligator2";
        public static string GoogleRefreshToken = string.Empty;
        public const double DefaultLatitude = 40.764858121285975;
        public const double DefaultLongitude = -73.97272288799286;
    }
}

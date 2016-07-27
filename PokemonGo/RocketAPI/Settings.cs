using PokemonGo.RocketAPI.Enums;

namespace PokemonGo.RocketAPI
{
    public class Settings
    {
        public Settings() { }
        public AuthType AuthType { get; set; }
        public double DefaultLatitude { get; set; }
        public double DefaultLongitude { get; set; }
        public double DefaultAltitude { get; set; }
        public string GoogleRefreshToken { get; set; }
        public string PtcPassword { get; set; }
        public string PtcUsername { get; set; }
    }
}
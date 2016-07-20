using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGo.RocketAPI
{
    public static class Settings
    {
        //Fetch these settings from intercepting the /auth call in headers and body (only needed for google auth)
        public const bool UsePTC = false;
        public const string PtcUsername = "ASDFGHJKLHA1";
        public const string PtcPassword = "12LILIHO";
        public const string DeviceId = "cool-device-id";
        public const string Email = "michael.liao28@gmail.com";
        public const string ClientSig = "fake";
        public const string LongDurationToken = "fakeid";
        public const double DefaultLatitude = 40.764858121285975;
        public const double DefaultLongitude = -73.97272288799286;
        //40.764858121285975,-73.97272288799286

    }
}

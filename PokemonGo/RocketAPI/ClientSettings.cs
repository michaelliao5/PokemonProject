using PokemonGo.RocketAPI.Enums;

namespace PokemonGo.RocketAPI
{
    public class ClientSettings
    {
        public ClientSettings()
        {
            AndroidBoardName = "msm8994";
            AndroidBootloader = "unknown";
            DeviceBrand = "OnePlus";
            DeviceModel = "OnePlus2";
            DeviceModelIdentifier = "ONE A2003_24_160604";
            DeviceModelBoot = "qcom";
            HardwareManufacturer = "OnePlus";
            HardwareModel = "ONE A2003";
            FirmwareBrand = "OnePlus2";
            FirmwareTags = "dev-keys";
            FirmwareType = "user";
            FirmwareFingerprint = "OnePlus/OnePlus2/OnePlus2:6.0.1/MMB29M/1447840820:user/release-keys";
        }
        public AuthType AuthType { get; set; }
        public double DefaultLatitude { get; set; }
        public double DefaultLongitude { get; set; }
        public double DefaultAltitude { get; set; }
        public string GoogleRefreshToken { get; set; }
        public string PtcPassword { get; set; }
        public string PtcUsername { get; set; }
        public string GoogleUsername { get; set; }
        public string GooglePassword { get; set; }
        public string DeviceId { get; set; }
        public string AndroidBoardName { get; set; }
        public string AndroidBootloader { get; set; }
        public string DeviceBrand { get; set; }
        public string DeviceModel { get; set; }
        public string DeviceModelIdentifier { get; set; }
        public string DeviceModelBoot { get; set; }
        public string HardwareManufacturer { get; set; }
        public string HardwareModel { get; set; }
        public string FirmwareBrand { get; set; }
        public string FirmwareTags { get; set; }
        public string FirmwareType { get; set; }
        public string FirmwareFingerprint { get; set; }
        public bool UseProxy { get; set; }
        public bool UseProxyAuthentication { get; set; }
        public string UseProxyHost { get; set; }
        public string UseProxyPort { get; set; }
        public string UseProxyUsername { get; set; }
        public string UseProxyPassword { get; set; }
    }
}
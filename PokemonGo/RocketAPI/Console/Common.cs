using POGOProtos.Enums;
using POGOProtos.Inventory.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGo.RocketAPI.Console
{
    public static class Common
    {
        public static List<PokemonId> SnipePokemons = new List<PokemonId>
        {
            PokemonId.Lapras,
            PokemonId.Snorlax,
            PokemonId.Gyarados,
            PokemonId.Aerodactyl,
            PokemonId.Blastoise,
            PokemonId.Dragonite,
            PokemonId.Charizard,
        };
        public static IList<Location> DratiniSpawn = new List<Location>
        {
            new Location(48.851438, 2.357557),
            new Location(33.762338, -118.196181),
            new Location(37.808966, -122.410153),
            new Location(37.7981849, -122.4025602),
            new Location(33.74168493949062, -118.10637474060057),
        };
        public static List<PokemonId> PublishingPowerUpPokemons = new List<PokemonId>
        {
            PokemonId.Lapras,
            PokemonId.Snorlax,
            PokemonId.Dragonite,
            PokemonId.Exeggutor,
            PokemonId.Vaporeon,
            PokemonId.Arcanine,
            PokemonId.Venusaur,
            PokemonId.Gyarados,
            PokemonId.Charizard,
            PokemonId.Blastoise,
            PokemonId.Aerodactyl,
            PokemonId.Machamp,
            PokemonId.Slowbro,
        };

        public static List<PokemonId> EvolveLevelOnePokemons = new List<PokemonId>
        {
            PokemonId.Dratini,
            PokemonId.Squirtle,
            PokemonId.Charmander,
            PokemonId.Bulbasaur,
            PokemonId.NidoranMale,
            PokemonId.NidoranFemale,
            PokemonId.Oddish,
            PokemonId.Poliwag,
            PokemonId.Abra,
            PokemonId.Machop,
            PokemonId.Bellsprout,
            PokemonId.Geodude,
            PokemonId.Gastly,
        };

        public static List<PokemonId> EvolveLevelTwoPokemons = new List<PokemonId>
        {
            PokemonId.Dragonair,
            PokemonId.Ivysaur,
            PokemonId.Charmeleon,
            PokemonId.Wartortle,
            PokemonId.Ekans,
            PokemonId.Pikachu,
            PokemonId.Sandshrew,
            PokemonId.Nidorina,
            PokemonId.Nidorino,
            PokemonId.Clefairy,
            PokemonId.Vulpix,
            PokemonId.Jigglypuff,
            PokemonId.Gloom,
            PokemonId.Psyduck,
            PokemonId.Growlithe,
            PokemonId.Poliwhirl,
            PokemonId.Kadabra,
            PokemonId.Machoke,
            PokemonId.Weepinbell,
            PokemonId.Graveler,
            PokemonId.Haunter,
            PokemonId.Ponyta,
            PokemonId.Slowpoke,
            PokemonId.Seel,
            PokemonId.Grimer,
            PokemonId.Drowzee,
            PokemonId.Exeggcute,
            PokemonId.Cubone,
            PokemonId.Koffing,
            PokemonId.Rhyhorn,
            PokemonId.Staryu,
            PokemonId.Magikarp,
            PokemonId.Eevee,
            PokemonId.Omanyte,
            PokemonId.Kabuto,
        };

        public static List<PokemonId> EvolveJunkPokemons = new List<PokemonId>
        {
            PokemonId.Caterpie,
            PokemonId.Weedle,
            PokemonId.Pidgey,
            PokemonId.Rattata,
            PokemonId.Ekans,
            PokemonId.Sandshrew,
            PokemonId.NidoranFemale,
            PokemonId.NidoranMale,
            PokemonId.Zubat,
            PokemonId.Paras,
            PokemonId.Diglett,
            PokemonId.Meowth,
            PokemonId.Mankey,
            PokemonId.Oddish,
            PokemonId.Poliwag,
            PokemonId.Abra,
            PokemonId.Bellsprout,
            PokemonId.Tentacool,
            PokemonId.Geodude,
            PokemonId.Ponyta,
            PokemonId.Magnemite,
            PokemonId.Doduo,
            PokemonId.Seel,
            PokemonId.Shellder,
            PokemonId.Drowzee,
            PokemonId.Voltorb,
            PokemonId.Cubone,
            PokemonId.Horsea,
            PokemonId.Goldeen,
            PokemonId.Staryu,
            PokemonId.Psyduck,
        };

        public static List<PokemonId> WantedPokemons = new List<PokemonId>
        {
            PokemonId.Lapras,
            PokemonId.Snorlax,
            PokemonId.Dragonite,
            PokemonId.Exeggutor,
            PokemonId.Vaporeon,
            PokemonId.Arcanine,
            PokemonId.Flareon,
            PokemonId.Venusaur,
            PokemonId.Jolteon,
            PokemonId.Charizard,
            PokemonId.Blastoise,
            PokemonId.Aerodactyl,
            PokemonId.Machamp,
            PokemonId.Slowbro,
            PokemonId.Gyarados,
        };
        public static List<ItemId> itemFarmingList = new List<ItemId>
        {
            //ItemId.ItemSuperPotion,
            //ItemId.ItemPotion,
            //ItemId.ItemRevive,
            //ItemId.ItemMaxPotion,
            //ItemId.ItemMaxRevive,
            //ItemId.ItemHyperPotion,
        };
        public static List<ItemId> itemRecycleList = new List<ItemId>
        {
            ItemId.ItemSuperPotion,
            ItemId.ItemPotion,
            ItemId.ItemRevive,
            ItemId.ItemMaxPotion,
            ItemId.ItemMaxRevive,
            ItemId.ItemHyperPotion,
            ItemId.ItemRazzBerry
        };
        public static List<PokemonId> PokemonIgnorelist = new List<PokemonId>
        {
            PokemonId.Pinsir
        };

        public static List<PokemonId> BerryPokemons = new List<PokemonId>
        {
            PokemonId.Lapras,
                PokemonId.Snorlax,
                PokemonId.Magmar,
                PokemonId.Dragonite,
                PokemonId.Exeggutor,
                PokemonId.Vaporeon,
                PokemonId.Golduck,
                PokemonId.Arcanine,
                PokemonId.Flareon,
                PokemonId.Venusaur,
                PokemonId.Blastoise,
                PokemonId.Wigglytuff,
                PokemonId.Electabuzz,
                PokemonId.Gyarados,
                PokemonId.Victreebel,
                PokemonId.Poliwrath,
                PokemonId.Vileplume,
                PokemonId.Golem,
                PokemonId.Rhydon,
                PokemonId.Jolteon,
                PokemonId.Rapidash,
                PokemonId.Clefable,
                PokemonId.Starmie,
                PokemonId.Scyther,
                PokemonId.Alakazam,
                PokemonId.Dragonair,
                PokemonId.Dratini,
        };
        //Tuple <Lat double>
        public static List<Tuple<double, double>> Coordinates = new List<Tuple<double, double>>
        {

        };
    }
}

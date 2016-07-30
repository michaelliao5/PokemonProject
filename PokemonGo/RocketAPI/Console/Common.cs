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
            PokemonId.Magmar,
            PokemonId.Dragonite,
            PokemonId.Exeggutor,
            PokemonId.Vaporeon,
            PokemonId.Arcanine,
            PokemonId.Flareon,
            PokemonId.Venusaur,
            PokemonId.Wigglytuff,
            PokemonId.Gyarados,
            PokemonId.Jolteon,
            PokemonId.Scyther,
            PokemonId.Electabuzz,
            PokemonId.Machamp,
            PokemonId.Slowbro,
            PokemonId.Ninetales,
            PokemonId.Blastoise,
            PokemonId.Charizard,
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
            PokemonId.Hitmonlee,
            PokemonId.Koffing,
            PokemonId.Rhyhorn,
            PokemonId.Staryu,
            PokemonId.Magikarp,
            PokemonId.Eevee,
            PokemonId.Omanyte,
            PokemonId.Kabuto,
        };

        public static List<PokemonId> WantedPokemons = new List<PokemonId>
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
            PokemonId.Wigglytuff,
            PokemonId.Gyarados,
            PokemonId.Victreebel,
            PokemonId.Poliwrath,
            PokemonId.Nidoking,
            PokemonId.Nidoqueen,
            PokemonId.Vileplume,
            PokemonId.Golem,
            PokemonId.Rhydon,
            PokemonId.Jolteon,
            PokemonId.Rapidash,
            PokemonId.Clefable,
            PokemonId.Starmie,
            PokemonId.Scyther,
            PokemonId.Alakazam,
        };
        public static List<ItemId> itemFarmingList = new List<ItemId>
        {
            ItemId.ItemSuperPotion,
            ItemId.ItemPotion,
            ItemId.ItemRevive,
            ItemId.ItemMaxPotion,
            ItemId.ItemMaxRevive,
            ItemId.ItemHyperPotion,
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

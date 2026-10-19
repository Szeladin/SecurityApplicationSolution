using System.Security.Cryptography;

namespace SecurityApplication.Security
{
    // Hasło jednorazowe wariantu 10: fraza 3 słów + 2 cyfry, ważna 12 h (np. "rower-lampa-sowa-47").
    public static class OneTimePasswordGenerator
    {
        public static readonly TimeSpan Validity = TimeSpan.FromHours(12);

        private static readonly string[] Words =
        [
            "kot", "pies", "dom", "rower", "lampa", "kawa", "herbata", "okno", "drzwi", "stol",
            "krzeslo", "ksiazka", "zeszyt", "olowek", "kubek", "talerz", "widelec", "lyzka", "chleb", "ser",
            "jablko", "gruszka", "sliwka", "wisnia", "malina", "marchew", "ogorek", "pomidor", "cebula", "czosnek",
            "rzeka", "jezioro", "morze", "gora", "las", "drzewo", "kwiat", "trawa", "kamien", "piasek",
            "chmura", "deszcz", "wiatr", "burza", "slonce", "ksiezyc", "gwiazda", "planeta", "rakieta", "samolot",
            "pociag", "statek", "autobus", "tramwaj", "most", "droga", "miasto", "wioska", "zamek", "wieza",
            "kino", "teatr", "muzyka", "gitara", "beben", "farba", "pedzel", "obraz", "ramka", "zegar",
            "budzik", "telefon", "radio", "kabel", "klucz", "portfel", "plecak", "parasol", "kurtka", "czapka",
            "szalik", "koszula", "sweter", "guzik", "nitka", "igla", "mlotek", "gwozdz", "sruba", "wiadro",
            "konewka", "ogrod", "brama", "ptak", "sowa", "orzel", "wilk", "zajac", "borsuk", "jelen",
            "ryba", "wieloryb", "delfin", "foka", "zolw", "zaba", "motyl", "pszczola", "mrowka", "biedronka",
        ];

        public static string Generate()
        {
            var words = Enumerable.Range(0, 3).Select(_ => Words[RandomNumberGenerator.GetInt32(Words.Length)]);
            var digits = RandomNumberGenerator.GetInt32(100).ToString("D2");
            return $"{string.Join('-', words)}-{digits}";
        }
    }
}

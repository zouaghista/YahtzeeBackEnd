using YahtzeeBackEnd.Enums;

namespace YahtzeeBackEnd.Mappers
{
    public static class YathzeeFieldNameMapper
    {
        public static Dictionary<string, YathzeeMove> FieldCodes = new()
        {
            {"aces", YathzeeMove.Aces },
            { "twos", YathzeeMove.Twos },
            { "threes", YathzeeMove.Threes },
            { "fours", YathzeeMove.Fours },
            { "fives", YathzeeMove.Fives },
            { "sixes", YathzeeMove.Sixes },
            { "threeOfAKind", YathzeeMove.Three_of_a_kind },
            { "fourOfAKind", YathzeeMove.Four_of_a_kind },
            { "fullHouse", YathzeeMove.Full_house },
            { "smallStraight", YathzeeMove.Small_straight },
            { "largeStraight", YathzeeMove.Large_straight },
            { "chance", YathzeeMove.Chance },
            { "yahtzee", YathzeeMove.Yahtzee }
        };
        public static Dictionary<YathzeeMove, string> FieldNames = new()
        {
            { YathzeeMove.Aces, "aces" },
            { YathzeeMove.Twos, "twos" },
            { YathzeeMove.Threes, "threes" },
            { YathzeeMove.Fours, "fours" },
            { YathzeeMove.Fives, "fives" },
            { YathzeeMove.Sixes, "sixes" },
            { YathzeeMove.Three_of_a_kind, "threeOfAKind" },
            { YathzeeMove.Four_of_a_kind, "fourOfAKind" },
            { YathzeeMove.Full_house, "fullHouse" },
            { YathzeeMove.Small_straight, "smallStraight" },
            { YathzeeMove.Large_straight, "largeStraight" },
            { YathzeeMove.Chance, "chance" },
            { YathzeeMove.Yahtzee, "yahtzee" }
        };
    }
}

using YahtzeeBackEnd.Enums;

namespace YahtzeeBackEnd.Entites
{
    public class GameInstance
    {
        public bool Started = false;
        public int Round = 0;
        public bool StartingPlayerPlayed = false;
        public bool Player1Turn = true;
        public int[][] Playerscores = new int[2][];
        public byte[] Dice = new byte[5];
        public bool[] RollingDice = new bool[5];
        public int[] TotalPlayerScores = [-1, -1];
        public byte RollCount = 0;

        //DICE LOGIC
        public void KeepDice(int dice)
        {
            RollingDice[dice] = false;
        }
        public void DiscardDice(int dice)
        {
            RollingDice[dice] = true;
        }
        public void ResetDice()
        {
            for(int i = 0; i < 5; i++)
            {
                RollingDice[i] = true;
            }
        }
        public void RollDice()
        {
            var rnd = new Random();
            for (int i = 0; i < 5; i++)
            {
                if (RollingDice[i])
                {
                    Dice[i] = (byte)rnd.Next(1, 6);
                }
            }
            RollCount++;
        }

        /// <summary>
        /// Registers a list selection of either player by turn.
        /// </summary>
        /// <param name="move">The field which the player has selected to score in</param>
        /// <returns>True if the game has ended, False otherwise</returns>
        public bool SelectPlayerField(YathzeeMove move)
        {
            switch (move)
            {
                case YathzeeMove.Aces:
                    Playerscores[Player1Turn ? 0 : 1][(int)move] = Dice.Where(e => e == 1).Count();
                    break;
                case YathzeeMove.Twos:
                    Playerscores[Player1Turn ? 0 : 1][(int)move] = Dice.Where(e => e == 2).Count() * 2;
                    break;
                case YathzeeMove.Threes:
                    Playerscores[Player1Turn ? 0 : 1][(int)move] = Dice.Where(e => e == 3).Count() * 3;
                    break;
                case YathzeeMove.Fours:
                    Playerscores[Player1Turn ? 0 : 1][(int)move] = Dice.Where(e => e == 4).Count() * 4;
                    break;
                case YathzeeMove.Fives:
                    Playerscores[Player1Turn ? 0 : 1][(int)move] = Dice.Where(e => e == 5).Count() * 5;
                    break;
                case YathzeeMove.Sixes:
                    Playerscores[Player1Turn ? 0 : 1][(int)move] = Dice.Where(e => e == 6).Count() * 6;
                    break;
                case YathzeeMove.Three_of_a_kind:
                    //I dont even know why I am typing like this but it is what it is
                    Playerscores[Player1Turn ? 0 : 1][(int)move] = Dice.GroupBy(e => e)
                        .OrderByDescending(e => e.Count())
                        .First().Count() >= 3 ? Dice.Sum(e=>e) : 0;
                    break;
                case YathzeeMove.Four_of_a_kind:
                    Playerscores[Player1Turn ? 0 : 1][(int)move] = Dice.GroupBy(e => e)
                        .OrderByDescending(e => e.Count())
                        .First().Count() >= 4 ? Dice.Sum(e => e) : 0;
                    break;
                case YathzeeMove.Full_house:
                    var diceDisposition = Dice.GroupBy(e => e).Select(e=>e.Count()).OrderByDescending(e => e).ToList();
                    Playerscores[Player1Turn ? 0 : 1][(int)move] = (diceDisposition.Count() == 2
                        && diceDisposition.First() ==3 ) ? 25 : 0;
                    break;
                case YathzeeMove.Small_straight:
                    Playerscores[Player1Turn ? 0 : 1][(int)move] = ((List<byte[]>)[[1, 2, 3, 4],[2, 3, 4, 5],[3, 4, 5, 6]]).
                        Contains(Dice.OrderBy(e=>e).Take(4).ToArray()) ? 30 : 0;
                    break;
                case YathzeeMove.Large_straight:
                    Playerscores[Player1Turn ? 0 : 1][(int)move] = ((List<byte[]>)[[1, 2, 3, 4, 5], [2, 3, 4, 5, 6]]).
                            Contains([.. Dice.OrderBy(e => e)]) ? 50 : 0;
                    break;
                case YathzeeMove.Chance:
                    Playerscores[Player1Turn ? 0 : 1][(int)move] = Dice.Sum(e=>e);
                    break;
                case YathzeeMove.Yahtzee:
                    Playerscores[Player1Turn ? 0 : 1][(int)move] = Dice.OrderBy(e=>e).Count() == 1 ? 50 : 0;
                    break;
                default:
                    break;
            }
            if(move != YathzeeMove.Yahtzee && Dice.OrderBy(e => e).Count() == 1)
            {
                //joker rules simulation, will check for double yathzees, only applies if player has 50 in the yathzee field.
                Playerscores[Player1Turn ? 0 : 1][(int)YathzeeMove.Yahtzee] += Playerscores[Player1Turn ? 0 : 1][(int)YathzeeMove.Yahtzee] == 50 ? 50 : 0;
            }
            if (Round == 1&&StartingPlayerPlayed) // 12
            {
                TotalPlayerScores[0] = CalculateScore(Playerscores[0]);
                TotalPlayerScores[1] = CalculateScore(Playerscores[1]);
                Started = false;
                return true;
            }
            if (StartingPlayerPlayed)
            {
                Round++;
            }
            RollCount = 0;
            ResetDice();
            StartingPlayerPlayed = !StartingPlayerPlayed;
            Player1Turn = !Player1Turn;
            return false;
        }
        private int CalculateScore(int[] fields)
        {
            return fields.Sum(e=>e) + fields.Take(6).Sum(e=>e) > 62 ? 35 : 0;
        }
        public int[] GetCurrentPlayerScore()
        {
            return Playerscores[Player1Turn ? 0 : 1];
        }
        public void StartGame()
        {
            Player1Turn = TotalPlayerScores[0]>TotalPlayerScores[1];
            TotalPlayerScores[0] = -1;
            RollCount = 0;
            TotalPlayerScores[1] = -1;
            ResetDice();
            Started = true;
        }
        public int[] GetLastPlayerScore()
        {
            return Playerscores[Player1Turn ? 1 : 0];
        }
        public void ResetGame()
        {
            Started = false;
            Round = 0;
            StartingPlayerPlayed = false;
            Player1Turn = true;
            RollCount = 0;
            TotalPlayerScores[0] = -1;
            TotalPlayerScores[1] = -1;
            for (int i = 0; i < 13; i++)
            {
                Playerscores[0][i] = -1;
                Playerscores[1][i] = -1;
            }
            ResetDice();
        }
        public GameInstance() {
            Playerscores[0] = new int[13];
            Playerscores[1] = new int[13];
            ResetGame();
        }

    }
}
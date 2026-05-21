using System.Collections.Generic;
using System.Linq;

namespace Gamebot.Models.Roulette
{
    internal class Roulette
    {
        private Roulette()
        {
            Roulette.Hands = new Dictionary<int, List<BtnFiches>>();
        }

        // (get) Token: 0x06000161 RID: 353 RVA: 0x0001C137 File Offset: 0x0001A337
        public static Roulette Instance
        {
            get
            {
                if (Roulette.instance == null)
                {
                    Roulette.instance = new Roulette();
                }
                return Roulette.instance;
            }
        }

        public List<BtnFiches> GetNumbersOfHand(int h)
        {
            if (Roulette.Hands.ContainsKey(h))
            {
                return Roulette.Hands[h];
            }
            return new List<BtnFiches>();
        }

        public BtnFiches AddNumberToList(BtnFiches btnFiches, int hand)
        {
            BtnFiches btnCurrent = btnFiches;
            List<BtnFiches> listBtnFiches = new List<BtnFiches>();
            if (Roulette.Hands.ContainsKey(hand))
            {
                listBtnFiches = Roulette.Hands[hand];
                if (listBtnFiches == null)
                {
                    listBtnFiches = new List<BtnFiches>();
                }
            }
            if (listBtnFiches.Any((BtnFiches item) => item.Value == btnFiches.Value))
            {
                btnCurrent = listBtnFiches.Where((BtnFiches item) => item.Value == btnFiches.Value).First<BtnFiches>();
                btnCurrent.Removed = true;
                listBtnFiches.RemoveAt(listBtnFiches.FindIndex((BtnFiches item) => item.Value == btnFiches.Value));
            }
            else
            {
                listBtnFiches.Add(btnFiches);
            }
            Roulette.Hands[hand] = listBtnFiches;
            return btnCurrent;
        }

        public void CleanHands()
        {
            Roulette.Hands = new Dictionary<int, List<BtnFiches>>();
        }

        public bool CheckForNumberPresence(int hand, int num)
        {
            if (Roulette.Hands.ContainsKey(hand))
            {
                using (List<BtnFiches>.Enumerator enumerator = Roulette.Hands[hand].GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        if (enumerator.Current.Value == num)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            return false;
        }

        public bool WithinBoundsOfMartingala(int martingalaLevel)
        {
            return Roulette.Hands.Count > martingalaLevel;
        }

        public int GetNumOfNumbers(int h)
        {
            if (Roulette.Hands.ContainsKey(h))
            {
                return Roulette.Hands[h].Count;
            }
            return 0;
        }

        public void PrintAllHand()
        {
            string number = "";
            if (Roulette.Hands.ContainsKey(1))
            {
                foreach (BtnFiches item in Roulette.Hands[1])
                {
                    number = number + item.Value.ToString() + ",";
                }
                number = "[" + number + "]";
            }
            string number2 = "";
            if (Roulette.Hands.ContainsKey(2))
            {
                foreach (BtnFiches item2 in Roulette.Hands[2])
                {
                    number2 = number2 + item2.Value.ToString() + ",";
                }
                number2 = "[" + number2 + "]";
            }
            string number3 = "";
            if (Roulette.Hands.ContainsKey(3))
            {
                foreach (BtnFiches item3 in Roulette.Hands[3])
                {
                    number3 = number3 + item3.Value.ToString() + ",";
                }
                number3 = "[" + number3 + "]";
            }
        }

        private static Dictionary<int, List<BtnFiches>> Hands;

        private static Roulette instance;
    }
}

using Gamebot.Models.Roulette.Funcs;

namespace Gamebot.Models.Roulette
{
    internal class RoulettePlayer
    {
        private RoulettePlayer()
        {
        }

        // (get) Token: 0x0600014F RID: 335 RVA: 0x0001BF9F File Offset: 0x0001A19F
        public static RoulettePlayer Instance
        {
            get
            {
                if (RoulettePlayer.instance == null)
                {
                    RoulettePlayer.instance = new RoulettePlayer();
                }
                return RoulettePlayer.instance;
            }
        }

        public void Start()
        {
            RouletteTask.Instance.StartGameBot();
        }

        public void Stop()
        {
            RouletteTask.Instance.StopGameBot();
        }

        private static RoulettePlayer instance;
    }
}

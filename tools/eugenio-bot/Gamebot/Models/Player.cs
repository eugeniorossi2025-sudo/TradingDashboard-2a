using Gamebot.Models.UI;

namespace Gamebot.Models
{
    internal class Player
    {
        private Player()
        {
        }

        // (get) Token: 0x060000BA RID: 186 RVA: 0x0001752C File Offset: 0x0001572C
        public static Player Instance
        {
            get
            {
                if (Player.instance == null)
                {
                    Player.instance = new Player();
                }
                return Player.instance;
            }
        }

        public void Start()
        {
            WorkerTask.Instance.StartGameBot();
        }

        public void Stop()
        {
            WorkerTask.Instance.StopGameBot();
        }

        private static Player instance;
    }
}

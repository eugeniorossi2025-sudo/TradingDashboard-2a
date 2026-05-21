using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gamebot.Models.Communication
{
    public partial class WaitScanBalanceWindow : Form
    {
        public Action Worker { get; set; }

        public WaitScanBalanceWindow(Action worker)
        {
            this.InitializeComponent();
            if (worker == null)
            {
                throw new ArgumentNullException();
            }
            this.Worker = worker;
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Task.Factory.StartNew(this.Worker).ContinueWith(delegate (Task t)
            {
                base.Close();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}

using Gamebot.UI.WindowForm;

namespace Gamebot.Models.UI
{
    internal class UpdateInterface
    {
        private UpdateInterface()
        {
        }

        // (get) Token: 0x06000122 RID: 290 RVA: 0x0001A708 File Offset: 0x00018908
        public static UpdateInterface Instance
        {
            get
            {
                if (UpdateInterface.instance == null)
                {
                    UpdateInterface.instance = new UpdateInterface();
                }
                return UpdateInterface.instance;
            }
        }

        public void SetRefForm(Configuratore  formInput)
        {
            UpdateInterface.form = formInput;
        }

        public static Configuratore  GetInstanceForm()
        {
            return UpdateInterface.form;
        }

        private static UpdateInterface instance;

        private static Configuratore  form;
    }
}

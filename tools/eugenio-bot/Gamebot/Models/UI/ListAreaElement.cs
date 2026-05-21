using Gamebot.Helpers;
using System.Collections.Generic;

namespace Gamebot.Models.UI
{
    public sealed class ListAreaElement
    {
        private ListAreaElement()
        {
        }

        // (get) Token: 0x060000D8 RID: 216 RVA: 0x00017B86 File Offset: 0x00015D86
        public static ListAreaElement Instance
        {
            get
            {
                if (ListAreaElement.instance == null)
                {
                    ListAreaElement.instance = new ListAreaElement();
                }
                return ListAreaElement.instance;
            }
        }

        public void AddArea(string key, AreaElement area)
        {
            if (this.ListAreaSelected.ContainsKey(key))
            {
                this.ListAreaSelected[key] = area;
                return;
            }
            this.ListAreaSelected.Add(key, area);
        }

        public Dictionary<string, AreaElement> GetAllArea()
        {
            return this.ListAreaSelected;
        }

        public bool CheckKey(string key)
        {
            return this.ListAreaSelected.ContainsKey(key);
        }

        public AreaElement GetAreaByKey(string key)
        {
            if (!this.ListAreaSelected.ContainsKey(key))
            {
                return null;
            }
            return this.ListAreaSelected[key];
        }

        public void PrintArea(string key)
        {
            AreaElement currentArea = this.ListAreaSelected[key];
            Log.PrintInfo(string.Format("AREA: {0} | StartX: {1} | StartY: {2} | EndX: {3} | EndY: {4}", new object[] { key, currentArea.startX, currentArea.startY, currentArea.endX, currentArea.endY }));
        }

        public void ClearAll()
        {
            this.ListAreaSelected = new Dictionary<string, AreaElement>();
        }

        private static ListAreaElement instance;

        private Dictionary<string, AreaElement> ListAreaSelected = new Dictionary<string, AreaElement>();
    }
}

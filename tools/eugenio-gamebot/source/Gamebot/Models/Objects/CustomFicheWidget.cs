using Gamebot.Models.UI;

namespace Gamebot.Models.Objects
{
    public class CustomFicheWidget
    {
        public void setTag(string newTag)
        {
            this.tag = newTag;
        }

        public void setLabel(string newLabel)
        {
            this.label = newLabel;
        }

        public void setValue(double newValue)
        {
            this.value = newValue;
        }

        public void setArea(int newStartX, int newStartY, int newEndX, int newEndY)
        {
            this.area.startX = newStartX;
            this.area.startY = newStartY;
            this.area.endX = newEndX;
            this.area.endY = newEndY;
        }

        public void setArea(AreaElement newArea)
        {
            this.area = newArea;
        }

        public string getTag()
        {
            return this.tag;
        }

        public string getLabel()
        {
            return this.label;
        }

        public double getValue()
        {
            return this.value;
        }

        public AreaElement getArea()
        {
            return this.area;
        }

        public string label = string.Empty;

        public string tag = string.Empty;

        public double value;

        public AreaElement area = new AreaElement();
    }
}

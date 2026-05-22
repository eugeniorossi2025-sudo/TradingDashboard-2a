using Gamebot.Helpers;
using Gamebot.Models.Entity;
using Gamebot.Models.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Gamebot.Models.Objects
{
    public static class CustomFicheWidgetsContainer
    {
        public static void addEntry(CustomFicheWidget newCfw)
        {
            foreach (CustomFicheWidget cfw in CustomFicheWidgetsContainer.cfws)
            {
                if (cfw.tag.Equals(newCfw.tag) || cfw.value == newCfw.value)
                {
                    return;
                }
            }
            CustomFicheWidgetsContainer.cfws.Add(newCfw);
        }

        public static void removeEntryNotInThisList(List<CustomFiche> newCfs)
        {
            List<string> cfwTagsToRemove = new List<string>();
            foreach (CustomFicheWidget customFicheWidget in CustomFicheWidgetsContainer.cfws)
            {
                string tag = customFicheWidget.tag.ToString();
                bool found = false;
                foreach (CustomFiche cf in newCfs)
                {
                    if (tag.Equals(cf.getDicitura()))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    cfwTagsToRemove.Add(tag);
                }
            }
            foreach (string cfwTagTR in cfwTagsToRemove)
            {
                CustomFicheWidgetsContainer.cfws.Remove(CustomFicheWidgetsContainer.getCustomFicheWidgetByTag(cfwTagTR));
            }
        }

        public static void clearAllEntries()
        {
            CustomFicheWidgetsContainer.cfws = new List<CustomFicheWidget>();
        }

        public static void modEntry(double value, AreaElement ae)
        {
            foreach (CustomFicheWidget cfw in CustomFicheWidgetsContainer.cfws)
            {
                if (cfw.getValue() == value)
                {
                    cfw.setArea(ae);
                    break;
                }
            }
        }

        public static void modEntry(string tag, AreaElement ae)
        {
            foreach (CustomFicheWidget cfw in CustomFicheWidgetsContainer.cfws)
            {
                if (cfw.getTag() == tag)
                {
                    cfw.setArea(ae);
                    break;
                }
            }
        }

        public static CustomFicheWidget getCustomFicheWidgetByValue(double value)
        {
            foreach (CustomFicheWidget cfw in CustomFicheWidgetsContainer.cfws)
            {
                if (cfw.getValue() == value)
                {
                    return cfw;
                }
            }
            return null;
        }

        public static CustomFicheWidget getCustomFicheWidgetByTag(string tag)
        {
            foreach (CustomFicheWidget cfw in CustomFicheWidgetsContainer.cfws)
            {
                if (cfw.getTag() == tag)
                {
                    return cfw;
                }
            }
            return null;
        }

        public static bool containsValue(int value)
        {
            using (List<CustomFicheWidget>.Enumerator enumerator = CustomFicheWidgetsContainer.cfws.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.getValue() == (double)value)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool containsTag(string tag)
        {
            using (List<CustomFicheWidget>.Enumerator enumerator = CustomFicheWidgetsContainer.cfws.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.getTag() == tag)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static List<CustomFicheData> getCustomFichesToSave()
        {
            List<CustomFicheData> data = new List<CustomFicheData>();
            foreach (CustomFicheWidget cfw in CustomFicheWidgetsContainer.cfws)
            {
                CustomFicheData cfdata = new CustomFicheData();
                cfdata.label = cfw.getLabel();
                cfdata.tag = cfw.getTag();
                cfdata.value = Math.Round(cfw.getValue(), 2, MidpointRounding.AwayFromZero);
                AreaElement area = cfw.getArea();
                cfdata.startX = area.startX;
                cfdata.startY = area.startY;
                cfdata.endX = area.endX;
                cfdata.endY = area.endY;
                data.Add(cfdata);
            }
            return data;
        }

        public static void LoadDataFormCustomFiches(List<CustomFicheData> data)
        {
            CustomFicheWidgetsContainer.cfws = new List<CustomFicheWidget>();
            List<double> newCustomFiches = new List<double>();
            foreach (CustomFicheData cfd in data)
            {
                CustomFicheWidget cfw = new CustomFicheWidget();
                cfw.setTag(cfd.tag);
                cfw.setLabel(cfd.label);
                cfw.setValue(cfd.value);
                cfw.setArea(cfd.startX, cfd.startY, cfd.endX, cfd.endY);
                ListAreaElement.Instance.AddArea("FICHE_250", cfw.getArea());
                newCustomFiches.Add(cfd.value);
                CustomFicheWidgetsContainer.cfws.Add(cfw);
            }
            Runtime.availableCustomFiches = newCustomFiches.ToArray();
            Runtime.availableCustomFiches = Runtime.availableCustomFiches.OrderBy((double i) => i).ToArray<double>();
            Array.Reverse(Runtime.availableCustomFiches);
        }

        public static List<CustomFicheWidget> getAllCustomFiches()
        {
            return CustomFicheWidgetsContainer.cfws;
        }

        public static List<CustomFiche> getAsReturnedFiches()
        {
            List<CustomFiche> returnedCustomFiches = new List<CustomFiche>();
            foreach (CustomFicheWidget cfw in CustomFicheWidgetsContainer.cfws)
            {
                CustomFiche newCF = new CustomFiche();
                newCF.setValue(cfw.getValue());
                newCF.setLabel(cfw.getLabel());
                returnedCustomFiches.Add(newCF);
            }
            return returnedCustomFiches;
        }

        public static bool checkFicheIsValid(double value, string label)
        {
            foreach (CustomFicheWidget cfw in CustomFicheWidgetsContainer.cfws)
            {
                if (cfw.value == value && cfw.label == label)
                {
                    return true;
                }
            }
            return false;
        }

        public static List<Rectangle> getAllRectangles()
        {
            List<Rectangle> ret = new List<Rectangle>();
            foreach (CustomFicheWidget cfw in CustomFicheWidgetsContainer.cfws)
            {
                if (cfw.area.startX != 0 && cfw.area.startY != 0 && cfw.area.endX != 0 && cfw.area.endY != 0)
                {
                    ret.Add(new Rectangle
                    {
                        X = cfw.area.startX,
                        Y = cfw.area.startY,
                        Width = cfw.area.endX - cfw.area.startX,
                        Height = cfw.area.endY - cfw.area.startY
                    });
                }
            }
            return ret;
        }

        public static double getLowestFicheValueAvailable()
        {
            double lowest = double.MaxValue;

            foreach (CustomFicheWidget cfw in CustomFicheWidgetsContainer.cfws)
            {
                if (cfw.value < lowest)
                {
                    lowest = cfw.value;
                }
            }

            Log.PrintInfo($"LOWEST VALUE AVAILABLE: {lowest}");

            return lowest != double.MaxValue ? lowest : 1.0;
        }


        private static List<CustomFicheWidget> cfws = new List<CustomFicheWidget>();
    }
}

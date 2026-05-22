using Gamebot.Models.Objects;
using Gamebot.Models.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Gamebot.UI.WindowForm
{
    public partial class CustomFichesWindow : Form
    {
        public CustomFichesWindow(List<CustomFiche> initFiches = null)
        {
            this.InitializeComponent();
            if (initFiches.Count > 0)
            {
                this.setInitFiches(initFiches);
            }
        }

        private void setInitFiches(List<CustomFiche> initFiches)
        {
            foreach (CustomFiche cf in initFiches)
            {
                this.createFicheEntry(this.customFichesQty, cf.getValue(), cf.getLabel());
                this.customFichesQty++;
            }
        }

        // (get) Token: 0x0600000E RID: 14 RVA: 0x000023A4 File Offset: 0x000005A4
        // (set) Token: 0x0600000F RID: 15 RVA: 0x000023AC File Offset: 0x000005AC
        public List<CustomFiche> returnedFiches { get; set; }

        private void editFichesOkBtn_Click(object sender, EventArgs e)
        {
            int check = this.checkForIrregularities();
            if (check == 0)
            {
                this.returnedFiches = this.getCustomFiches();
                base.DialogResult = DialogResult.OK;
                base.Close();
                return;
            }
            if (check == 1)
            {
                MessageBox.Show("Il valore delle fiches deve essere superiore a zero", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }
            if (check == 2)
            {
                MessageBox.Show("Tutte le fiches devono avere valori diversi fra loro", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void editFichesCancelBtn_Click(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.Cancel;
            base.Close();
        }

        private int checkForIrregularities()
        {
            List<double> listOfValues = new List<double>();
            foreach (CustomFiche cf in this.getCustomFiches())
            {
                if (cf.getValue() < 0.01f)
                {
                    return 1;
                }
                if (listOfValues.Contains(cf.getValue()))
                {
                    return 2;
                }
                listOfValues.Add(cf.getValue());
            }
            return 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private List<CustomFiche> getCustomFiches()
        {
            List<CustomFiche> fichesToReturn = new List<CustomFiche>();
            foreach (Control item in this.customFichesContainerPanel.Controls.OfType<Control>().ToList<Control>())
            {
                if (item.Name.StartsWith("entryPanel_"))
                {
                    CustomFiche newCF = new CustomFiche();
                    foreach (Control subItem in item.Controls.OfType<Control>().ToList<Control>())
                    {
                        if (subItem.Name.StartsWith("ficheValueField_"))
                        {
                            NumericUpDown x = (NumericUpDown)subItem;
                            x.ValueChanged += this.newEntryValueField_ChangeDotToComma;
                            newCF.setValue(Convert.ToSingle(x.Value));
                        }
                        if (subItem.Name.StartsWith("ficheLabelField_"))
                        {
                            newCF.setLabel(subItem.Text);
                        }
                    }
                    fichesToReturn.Add(newCF);
                }
            }
            return fichesToReturn;
        }

        private void editFichesAddBtn_Click(object sender, EventArgs e)
        {
            if (this.customFichesQty < 12)
            {
                this.createFicheEntry(this.customFichesQty, 0f, "");
                this.customFichesQty++;
                if (this.customFichesQty >= 12)
                {
                    this.editFichesAddBtn.Enabled = false;
                    this.editFichesAddBtn.BackColor = Color.DimGray;
                }
            }
        }

        private void createFicheEntry(int customFichesQty, double initValue = 0, string initLabel = "")
        {
            Panel newCustomFicheEditor = this.createEntryEditor(customFichesQty);
            Label newEntryValueLabel = this.createEntryValueLabel(customFichesQty);
            NumericUpDown newEntryValueField = this.createEntryValueField(customFichesQty, initValue);
            Label newEntryLabelLabel = this.createEntryLabelLabel(customFichesQty);
            TextBox newEntryLabelField = this.createEntryLabelField(customFichesQty, initLabel);
            Button newEntryRemoveButton = this.createEntryRemoveButton(customFichesQty);
            newCustomFicheEditor.Controls.Add(newEntryValueLabel);
            newCustomFicheEditor.Controls.Add(newEntryValueField);
            newCustomFicheEditor.Controls.Add(newEntryLabelLabel);
            newCustomFicheEditor.Controls.Add(newEntryLabelField);
            newCustomFicheEditor.Controls.Add(newEntryRemoveButton);
            this.customFichesContainerPanel.Controls.Add(newCustomFicheEditor);
            newCustomFicheEditor.BringToFront();
        }

        private void rearrangeEntries()
        {
            this.returnedFiches = new List<CustomFiche>();
            foreach (Control item in this.customFichesContainerPanel.Controls.OfType<Control>().ToList<Control>())
            {
                if (item.Name.StartsWith("entryPanel_"))
                {
                    CustomFiche newCF = new CustomFiche();
                    foreach (Control subItem in item.Controls.OfType<Control>().ToList<Control>())
                    {
                        if (subItem.Name.StartsWith("ficheValueField_"))
                        {
                            NumericUpDown x = (NumericUpDown)subItem;
                            newCF.setValue(Convert.ToSingle(x.Value));
                        }
                        if (subItem.Name.StartsWith("ficheLabelField_"))
                        {
                            newCF.setLabel(subItem.Text);
                        }
                    }
                    this.returnedFiches.Add(newCF);
                    this.customFichesContainerPanel.Controls.Remove(item);
                }
            }
            this.returnedFiches.Sort((CustomFiche p, CustomFiche q) => p.getValue().CompareTo(q.getValue()));
            this.customFichesQty = 0;
            foreach (CustomFiche cf in this.returnedFiches)
            {
                this.createFicheEntry(this.customFichesQty, cf.getValue(), cf.getLabel());
                this.customFichesQty++;
            }
            if (this.customFichesQty < 12)
            {
                this.editFichesAddBtn.Enabled = true;
                this.editFichesAddBtn.BackColor = Color.Transparent;
                return;
            }
            this.editFichesAddBtn.Enabled = false;
            this.editFichesAddBtn.BackColor = Color.DimGray;
        }

        private Panel createEntryEditor(int customFichesQty)
        {
            return new Panel
            {
                Name = "entryPanel_" + customFichesQty.ToString(),
                BackColor = SystemColors.GradientInactiveCaption,
                Top = 14 + customFichesQty * 36,
                Left = 18,
                Width = 356,
                Height = 24,
                TabIndex = 2001 + customFichesQty
            };
        }

        private Label createEntryValueLabel(int customFichesQty)
        {
            return new Label
            {
                Name = "ficheValueLabel_" + customFichesQty.ToString(),
                Top = 5,
                Left = 16,
                ForeColor = Color.Black,
                Font = new Font("Arial", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0),
                Width = 48,
                Height = 12,
                Text = "Valore",
                TabIndex = 2001 + customFichesQty
            };
        }

        private NumericUpDown createEntryValueField(int customFichesQty, double initialValue = 0f)
        {
            NumericUpDown numericUpDown = new NumericUpDown();
            numericUpDown.Name = "ficheValueField_" + customFichesQty.ToString();
            numericUpDown.Top = 2;
            numericUpDown.Left = 80;
            numericUpDown.Maximum = 100000m;
            numericUpDown.ForeColor = Color.Black;
            numericUpDown.Font = new Font("Arial", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
            numericUpDown.Width = 48;
            numericUpDown.Height = 12;
            numericUpDown.Value = Convert.ToDecimal(initialValue);
            numericUpDown.TabIndex = 2002 + customFichesQty;
            numericUpDown.DecimalPlaces = 2;
            numericUpDown.Increment = 0.01m;
            numericUpDown.ValueChanged += this.newEntryValueField_ChangeDotToComma;
            numericUpDown.Text = UIForm.ReplaceDotIntoCommaValueText(numericUpDown.Text);
            return numericUpDown;
        }

        private void newEntryValueField_ChangeDotToComma(object sender, EventArgs e)
        {
            NumericUpDown numericUpDown = (NumericUpDown)sender;
            numericUpDown.Text = UIForm.ReplaceDotIntoCommaValueText(numericUpDown.Text);
        }

        private Label createEntryLabelLabel(int customFichesQty)
        {
            return new Label
            {
                Name = "ficheLabelLabel_" + customFichesQty.ToString(),
                Top = 5,
                Left = 160,
                ForeColor = Color.Black,
                Font = new Font("Arial", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0),
                Width = 48,
                Height = 12,
                Text = "Dicitura",
                TabIndex = 2003 + customFichesQty
            };
        }

        private TextBox createEntryLabelField(int customFichesQty, string initialText = "")
        {
            return new TextBox
            {
                Name = "ficheLabelField_" + customFichesQty.ToString(),
                Top = 2,
                Left = 224,
                ForeColor = Color.Black,
                Font = new Font("Arial", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0),
                Width = 80,
                Height = 12,
                Text = initialText,
                TabIndex = 2004 + customFichesQty
            };
        }

        private Button createEntryRemoveButton(int customFichesQty)
        {
            Button button = new Button();
            button.Text = "X";
            button.TextAlign = ContentAlignment.MiddleCenter;
            button.Name = "customFicheDel_" + customFichesQty.ToString();
            button.Top = 2;
            button.Left = 333;
            button.Width = 20;
            button.Height = 20;
            button.AccessibleName = "entryPanel_" + customFichesQty.ToString();
            button.Click += this.removeCustomFicheEntry_Click;
            return button;
        }

        private void removeCustomFicheEntry_Click(object sender, EventArgs e)
        {
            string strName = ((Button)sender).AccessibleName;
            foreach (Control item in this.customFichesContainerPanel.Controls.OfType<Control>().ToList<Control>())
            {
                if (item.Name == strName)
                {
                    foreach (Control control in item.Controls.OfType<Control>().ToList<Control>())
                    {
                        control.Dispose();
                    }
                    this.customFichesContainerPanel.Controls.Remove(item);
                }
            }
            this.rearrangeEntries();
        }

        private void helpbtn_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Per aggiungere una fiche, cliccare sul tasto \"Aggiungi\".\n\nImpostare il valore. Si può immettere anche una dicitura aggiuntiva da mostrare prima del valore (facoltativo).\n\nTutte le fiches devono avere valori diversi fra loro.\n\nPer eliminare una fiche personalizzata creata tornare su questo pannello ed eliminare la voce premendo sul tasto \"X\".\n\nPremendo \"OK\" vengono impostati i pulsanti nella schermata principale con i valori immessi.\n\nA prescindere dall'ordine di immissione, tali pulsanti verranno automaticamente messi in ordine crescente per valore.\n\nPer eliminare un pulsante, tornare sul pannello e eliminare la voce da li.\n\nLe nuove voci e quelle a cui vengono modificati valore e/o dicitura necessitano la rimappatura dell'area.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private int customFichesQty;
    }
}

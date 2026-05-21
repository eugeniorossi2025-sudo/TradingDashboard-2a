using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Windows.Forms;
using Gamebot.Communication.Firebase;
using Gamebot.Helpers;
using Gamebot.Models;
using Gamebot.Models.Communication;
using Gamebot.Models.Entity;
using Gamebot.Models.Interfaces;
using Gamebot.Models.MainState;
using Gamebot.Models.Objects;
using Gamebot.Models.Roulette;
using Gamebot.Models.Roulette.Funcs;
using Gamebot.Models.UI;
using Gamebot.Properties;
using Google.Cloud.Firestore;

namespace Gamebot.UI.WindowForm
{
		public partial class Configuratore : global::System.Windows.Forms.Form
	{
				protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

      
                //public IProgress<List<string>> progressUI;

        //        //public IProgress<List<string>> progressUIRoulette;

        //        //public IProgress<List<string>> progressBalance;

        //        //public IProgress<List<string>> progressTimeElapsed;

        //        //public int numUpDownInputMartingala;

        //        //public List<Control> controlsToEnableDisable = new List<Control>();

        //        //public List<Control> controlsRouletteToEnableDisable = new List<Control>();

        //        //public List<Control> controlsRouletteToEnableDisableHand1 = new List<Control>();

        //        //public List<Control> controlsRouletteToEnableDisableHand2 = new List<Control>();

        //        //public List<Control> controlsRouletteToEnableDisableHand3 = new List<Control>();

        //        //public readonly IRequestApi _requestApiRepository;

                public IContainer components;

                public Button button1;

                public Label labelVersion;

                public Button showboxbtn;

                public Label saldoLetto;

                public Label saldoLettoCorrect;

                public TextBox readsaldo;

                public Button button2;

                public Button testBtnWindowOnTop;

                public TabPage tabPage4;

                public TabPage tabPage2;

                public CheckBox sendEndSculpingMessage;

                public Button btnSendTelegram;

                public Button btnStartTelegram;

                public TextBox textChatName;

                public TextBox textVerifiedCode;

                public TextBox textActualPhone;

                public Label labelChatName;

                public Label labelVerifiedCode;

                public Label labelActualPhone;

                public Label labelTGminiguide;

                public TabPage tabPage3;

                public Label labelStatusRoulette;

                public Label autoBalanceLabelRoulette;

                public CheckBox checkBoxAutoSaldoRoulette;

                public Button buttonBalanceAreaRoulette;

                public Button roulettemainhelpbtn;

                public Button roulettestopwinlossinfobtn;

                public Button roulettebalanceinfobtn;

                public Button btnRouletteOCRWaitingArea;

                public Label lblRouletteHandLossText;

                public Label lblRouletteHandLoss;

                public Label lblRouletteHandWinText;

                public Label lblRouletteHandWin;

                public Label lblRouletteGlobalProfitText;

                public Label lblRouletteGlobalProfit;

                public Label balanceRouletteTotalValueText;

                public NumericUpDown balanceRouletteStartValue;

                public Label label5;

                public Button btnRouletteStart;

                public NumericUpDown numericRouletteValueHand3;

                public NumericUpDown numericRouletteValueHand2;

                public NumericUpDown numericRouletteValueHand1;

                public Label lblRouletteValueHand3;

                public Label lblRouletteValueHand2;

                public Label lblRouletteValueHand1;

                public NumericUpDown globalRouletteStopLoss;

                public NumericUpDown globalRouletteStopWin;

                public Label label4;

                public Label label3;

                public Label lblRouletteNameConfig;

                public Label lblRouletteTextConfigUpload;

                public Button btnRouletteSaveConfig;

                public Button btnRouletteLoadConfig;

                public Button btnRouletteOCRWinArea;

                public Button btnRouletteOCRHand3;

                public Button btnRouletteOCRHand2;

                public Button btnRouletteOCRHand1;

                public Label labelRouletteOCRArea;

                public Panel panelRoulettePlayed3;

                public Button btnRoulettePlayed3Number36;

                public Button btnRoulettePlayed3Number35;

                public Button btnRoulettePlayed3Number34;

                public Button btnRoulettePlayed3Number33;

                public Button btnRoulettePlayed3Number32;

                public Button btnRoulettePlayed3Number31;

                public Button btnRoulettePlayed3Number30;

                public Button btnRoulettePlayed3Number29;

                public Button btnRoulettePlayed3Number28;

                public Button btnRoulettePlayed3Number27;

                public Button btnRoulettePlayed3Number26;

                public Button btnRoulettePlayed3Number25;

                public Button btnRoulettePlayed3Number24;

                public Button btnRoulettePlayed3Number23;

                public Button btnRoulettePlayed3Number22;

                public Button btnRoulettePlayed3Number21;

                public Button btnRoulettePlayed3Number20;

                public Button btnRoulettePlayed3Number19;

                public Button btnRoulettePlayed3Number18;

                public Button btnRoulettePlayed3Number17;

                public Button btnRoulettePlayed3Number16;

                public Button btnRoulettePlayed3Number15;

                public Button btnRoulettePlayed3Number14;

                public Button btnRoulettePlayed3Number13;

                public Button btnRoulettePlayed3Number12;

                public Button btnRoulettePlayed3Number11;

                public Button btnRoulettePlayed3Number10;

                public Button btnRoulettePlayed3Number9;

                public Button btnRoulettePlayed3Number8;

                public Button btnRoulettePlayed3Number7;

                public Button btnRoulettePlayed3Number6;

                public Button btnRoulettePlayed3Number5;

                public Button btnRoulettePlayed3Number4;

                public Button btnRoulettePlayed3Number3;

                public Button btnRoulettePlayed3Number2;

                public Button btnRoulettePlayed3Number1;

                public Button btnRoulettePlayed3Number0;

                public Panel panelRoulettePlayed2;

                public Button btnRoulettePlayed2Number36;

                public Button btnRoulettePlayed2Number35;

                public Button btnRoulettePlayed2Number34;

                public Button btnRoulettePlayed2Number33;

                public Button btnRoulettePlayed2Number32;

                public Button btnRoulettePlayed2Number31;

                public Button btnRoulettePlayed2Number30;

                public Button btnRoulettePlayed2Number29;

                public Button btnRoulettePlayed2Number28;

                public Button btnRoulettePlayed2Number27;

                public Button btnRoulettePlayed2Number26;

                public Button btnRoulettePlayed2Number25;

                public Button btnRoulettePlayed2Number24;

                public Button btnRoulettePlayed2Number23;

                public Button btnRoulettePlayed2Number22;

                public Button btnRoulettePlayed2Number21;

                public Button btnRoulettePlayed2Number20;

                public Button btnRoulettePlayed2Number19;

                public Button btnRoulettePlayed2Number18;

                public Button btnRoulettePlayed2Number17;

                public Button btnRoulettePlayed2Number16;

                public Button btnRoulettePlayed2Number15;

                public Button btnRoulettePlayed2Number14;

                public Button btnRoulettePlayed2Number13;

                public Button btnRoulettePlayed2Number12;

                public Button btnRoulettePlayed2Number11;

                public Button btnRoulettePlayed2Number10;

                public Button btnRoulettePlayed2Number9;

                public Button btnRoulettePlayed2Number8;

                public Button btnRoulettePlayed2Number7;

                public Button btnRoulettePlayed2Number6;

                public Button btnRoulettePlayed2Number5;

                public Button btnRoulettePlayed2Number4;

                public Button btnRoulettePlayed2Number3;

                public Button btnRoulettePlayed2Number2;

                public Button btnRoulettePlayed2Number1;

                public Button btnRoulettePlayed2Number0;

                public Panel panelRoulettePlayed1;

                public Button btnRoulettePlayed1Number36;

                public Button btnRoulettePlayed1Number35;

                public Button btnRoulettePlayed1Number34;

                public Button btnRoulettePlayed1Number33;

                public Button btnRoulettePlayed1Number32;

                public Button btnRoulettePlayed1Number31;

                public Button btnRoulettePlayed1Number30;

                public Button btnRoulettePlayed1Number29;

                public Button btnRoulettePlayed1Number28;

                public Button btnRoulettePlayed1Number27;

                public Button btnRoulettePlayed1Number26;

                public Button btnRoulettePlayed1Number25;

                public Button btnRoulettePlayed1Number24;

                public Button btnRoulettePlayed1Number23;

                public Button btnRoulettePlayed1Number22;

                public Button btnRoulettePlayed1Number21;

                public Button btnRoulettePlayed1Number20;

                public Button btnRoulettePlayed1Number19;

                public Button btnRoulettePlayed1Number18;

                public Button btnRoulettePlayed1Number17;

                public Button btnRoulettePlayed1Number16;

                public Button btnRoulettePlayed1Number15;

                public Button btnRoulettePlayed1Number14;

                public Button btnRoulettePlayed1Number13;

                public Button btnRoulettePlayed1Number12;

                public Button btnRoulettePlayed1Number11;

                public Button btnRoulettePlayed1Number10;

                public Button btnRoulettePlayed1Number9;

                public Button btnRoulettePlayed1Number8;

                public Button btnRoulettePlayed1Number7;

                public Button btnRoulettePlayed1Number6;

                public Button btnRoulettePlayed1Number5;

                public Button btnRoulettePlayed1Number4;

                public Button btnRoulettePlayed1Number3;

                public Button btnRoulettePlayed1Number2;

                public Button btnRoulettePlayed1Number1;

                public Button btnRoulettePlayed1Number0;

                public Label lblRoulettePlayed3;

                public Label lblRoulettePlayed2;

                public Label lblRoulettePlayed1;

                public TabPage tabPage1;

                public CheckBox checkPragmaticFilter;

                public Button buttonBet;

                public Label timeElapsedValueToChange;

                public Label labelTimeElapsed;

                public TextBox textAreaPuntare;

                public TextBox textAreaPlayer;

                public TextBox textAreaBench;

                public TextBox textAreaWin;

                public TextBox textAreaTie;

                public Label label7;

                public Label numberDeckValueToChange;

                public Label labelNumberDeck;

                public Button typeGamenInfobtn;

                public GroupBox groupBox1;

                public RadioButton baccaratDemoBtnRadioEnabled;

                public RadioButton baccaratDemoBtnRadioDisabled;

                public Label label6;

                public Label labelEnvironment;

                public Button textAreaInfoBtn;

                public Label labelTextAreaGiocatore;

                public Label labelTextAreaBanco;

                public Label labelTextAreaVince;

                public Label labelTextAreaTie;

                public NumericUpDown numberChangeEndDeck;

                public Label labelChangeNumberEndDeck;

                public Label labelZoomPerc;

                public Label autoBalanceLabel;

                public CheckBox checkBoxAutoSaldo;

                public Button mainareehelpbtn;

                public Button stopwinlossinfobtn;

                public Button balanceinfobtn;

                public Button cardcolorsinfobtn;

                public Button martingalaHelpBtn;

                public Button mainhelpbtn;

                public Panel customFichesPanel;

                public Label noFichesLabel;

                public Button customFichesEditBtn;

                public Label lblNameConfig;

                public Label label2;

                public Button btnSaveConfig;

                public Label labelRiconoscimentoFiches;

                public Label labelStatus;

                public Label balanceTotalValueText;

                public NumericUpDown balanceStartValue;

                public Label labelStartBalance;

                public CheckBox checkSafeWin;

                public Label label1;

                public NumericUpDown txtZoomMonitor;

                public Label labelNumberProfittoSculping;

                public Label labelProfittoSculping;

                public Label labelNumerLose;

                public Label labelNumberProfittoGlobale;

                public Label labelNumerWin;

                public Label labelProfittoGlobale;

                public Label labelVinte;

                public Panel panelMartingala;

                public Label labelPerse;

                public Label labelMartingala;

                public Label labelPerc;

                public NumericUpDown safeWinPerc;

                public Button btnAddMartingala;

                public Label labelSafeWin;

                public Button buttonLoadConfig;

                public NumericUpDown stopLossValue;

                public Label labelStopWinGlob;

                public Label labelStopLoss;

                public NumericUpDown globalStopWinValue;

                public Button buttonStart;

                public GroupBox groupBoxMode;

                public RadioButton radioModeMonocolore;

                public RadioButton radioModeAlternata;

                public Label labelMode;

                public Label labelRiconoscimentoArea;

                public GroupBox groupBoxStartColor;

                public RadioButton radioColorBlu;

                public RadioButton radioColorRed;

                public NumericUpDown stopWinValue;

                public Label labelColorStart;

                public Label labelStopWin;

                public Button buttonFish250;

                public Button buttonRed;

                public Button buttonFish100;

                public Button buttonBlu;

                public Button buttonFish1;

                public Button buttonDoubling;

                public Button buttonBalanceArea;

                public Button buttonFish500;

                public Button buttonAreaVincita;

                public Button buttonDeckArea;

                public Button buttonAreaCentrale;

                public Button buttonFish5;

                public Button buttonFish25;

                public TabControl tabControl1;

                public GroupBox groupBox2;

                public GroupBox groupBox3;

                public GroupBox groupBox4;

                public GroupBox groupBox5;

                public NumericUpDown martingala1EndDeckValue;

                public NumericUpDown martingala1StartDeckValue;

                public Label martingala1LblEndDeck;

                public Label martingala1LblStartDeck;

                public Label martingala2LblEndDeck;

                public Label martingala2LblStartDeck;

                public Label martingala3LblEndDeck;

                public Label martingala3LblStartDeck;

                public Label martingala4LblEndDeck;

                public Label martingala4LblStartDeck;

                public NumericUpDown martingala2EndDeckValue;

                public NumericUpDown martingala2StartDeckValue;

                public NumericUpDown martingala3StartDeckValue;

                public NumericUpDown martingala3EndDeckValue;

                public NumericUpDown martingala4StartDeckValue;

                public NumericUpDown martingala4EndDeckValue;

                public Label martingala2LblChangeColor;

                public Label martingala1LblChangeColor;

                public Label martingala3LblChangeColor;

                public Label martingala4LblChangeColor;

                public NumericUpDown martingala4ChangeColorValue;

                public NumericUpDown martingala3ChangeColorValue;

                public NumericUpDown martingala2ChangeColorValue;

                public NumericUpDown martingala1ChangeColorValue;

                public Label label8;

                public CheckBox checkSkipPostSculping;

                public Label martingala2LblAlarmMartingala;

                public Label martingala1LblAlarmMartingala;

                public Label martingala3LblAlarmMartingala;

                public Label martingala4LblAlarmMartingala;

                public NumericUpDown martingala1IndexAlarmValue;

                public NumericUpDown martingala2IndexAlarmValue;

                public NumericUpDown martingala3IndexAlarmValue;

                public NumericUpDown martingala4IndexAlarmValue;

        public NumericUpDown numericUpDown;
        public NumericUpDown numericUpDown2;
        public NumericUpDown numericUpDown3;
        public NumericUpDown numericUpDown4;
        public NumericUpDown numericUpDown5;
        public NumericUpDown numericUpDown6;
        public NumericUpDown numericUpDown7;
        public NumericUpDown numericUpDown8;
        public NumericUpDown numericUpDown9;
        public NumericUpDown numericUpDown10;
        public NumericUpDown numericUpDown11;
        public NumericUpDown numericUpDown12;
        public NumericUpDown numericUpDown13;
        public NumericUpDown numericUpDown14;
        public NumericUpDown numericUpDown15;
        public NumericUpDown numericUpDown16;
        public NumericUpDown numericUpDown17;
        public NumericUpDown numericUpDown18;
        public NumericUpDown numericUpDown19;
        public NumericUpDown numericUpDown20;
        public NumericUpDown numericUpDown21;
        public NumericUpDown numericUpDown22;
        public NumericUpDown numericUpDown23;
        public NumericUpDown numericUpDown24;
        public NumericUpDown numericUpDown25;
        public NumericUpDown numericUpDown26;


                public void InitializeComponent()
        {
            this.saldoLettoCorrect = new System.Windows.Forms.Label();
            this.saldoLetto = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.readsaldo = new System.Windows.Forms.TextBox();
            this.showboxbtn = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.labelVersion = new System.Windows.Forms.Label();
            this.testBtnWindowOnTop = new System.Windows.Forms.Button();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.martingala4IndexAlarmValue = new System.Windows.Forms.NumericUpDown();
            this.martingala4LblAlarmMartingala = new System.Windows.Forms.Label();
            this.martingala4ChangeColorValue = new System.Windows.Forms.NumericUpDown();
            this.martingala4LblChangeColor = new System.Windows.Forms.Label();
            this.martingala4EndDeckValue = new System.Windows.Forms.NumericUpDown();
            this.martingala4StartDeckValue = new System.Windows.Forms.NumericUpDown();
            this.martingala4LblEndDeck = new System.Windows.Forms.Label();
            this.martingala4LblStartDeck = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.martingala3IndexAlarmValue = new System.Windows.Forms.NumericUpDown();
            this.martingala3LblAlarmMartingala = new System.Windows.Forms.Label();
            this.martingala3ChangeColorValue = new System.Windows.Forms.NumericUpDown();
            this.martingala3LblChangeColor = new System.Windows.Forms.Label();
            this.martingala3EndDeckValue = new System.Windows.Forms.NumericUpDown();
            this.martingala3StartDeckValue = new System.Windows.Forms.NumericUpDown();
            this.martingala3LblEndDeck = new System.Windows.Forms.Label();
            this.martingala3LblStartDeck = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.martingala2IndexAlarmValue = new System.Windows.Forms.NumericUpDown();
            this.martingala2LblAlarmMartingala = new System.Windows.Forms.Label();
            this.martingala2ChangeColorValue = new System.Windows.Forms.NumericUpDown();
            this.martingala2LblChangeColor = new System.Windows.Forms.Label();
            this.martingala2EndDeckValue = new System.Windows.Forms.NumericUpDown();
            this.martingala2StartDeckValue = new System.Windows.Forms.NumericUpDown();
            this.martingala2LblEndDeck = new System.Windows.Forms.Label();
            this.martingala2LblStartDeck = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.martingala1IndexAlarmValue = new System.Windows.Forms.NumericUpDown();
            this.martingala1LblAlarmMartingala = new System.Windows.Forms.Label();
            this.martingala1ChangeColorValue = new System.Windows.Forms.NumericUpDown();
            this.martingala1LblChangeColor = new System.Windows.Forms.Label();
            this.martingala1EndDeckValue = new System.Windows.Forms.NumericUpDown();
            this.martingala1StartDeckValue = new System.Windows.Forms.NumericUpDown();
            this.martingala1LblEndDeck = new System.Windows.Forms.Label();
            this.martingala1LblStartDeck = new System.Windows.Forms.Label();
            this.numericUpDown = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown3 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown4 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown5 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown6 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown7 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown8 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown9 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown10 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown11 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown12 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown13 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown14 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown15 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown16 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown17 = new System.Windows.Forms.NumericUpDown();
            this.balanceRouletteStartValue = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown18 = new System.Windows.Forms.NumericUpDown();
            this.numericRouletteValueHand3 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown19 = new System.Windows.Forms.NumericUpDown();
            this.numericRouletteValueHand2 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown20 = new System.Windows.Forms.NumericUpDown();
            this.numericRouletteValueHand1 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown21 = new System.Windows.Forms.NumericUpDown();
            this.globalRouletteStopLoss = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown22 = new System.Windows.Forms.NumericUpDown();
            this.globalRouletteStopWin = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown23 = new System.Windows.Forms.NumericUpDown();
            this.balanceStartValue = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown24 = new System.Windows.Forms.NumericUpDown();
            this.stopLossValue = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown25 = new System.Windows.Forms.NumericUpDown();
            this.globalStopWinValue = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown26 = new System.Windows.Forms.NumericUpDown();
            this.stopWinValue = new System.Windows.Forms.NumericUpDown();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.sendEndSculpingMessage = new System.Windows.Forms.CheckBox();
            this.btnSendTelegram = new System.Windows.Forms.Button();
            this.btnStartTelegram = new System.Windows.Forms.Button();
            this.textChatName = new System.Windows.Forms.TextBox();
            this.textVerifiedCode = new System.Windows.Forms.TextBox();
            this.textActualPhone = new System.Windows.Forms.TextBox();
            this.labelChatName = new System.Windows.Forms.Label();
            this.labelVerifiedCode = new System.Windows.Forms.Label();
            this.labelActualPhone = new System.Windows.Forms.Label();
            this.labelTGminiguide = new System.Windows.Forms.Label();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.labelStatusRoulette = new System.Windows.Forms.Label();
            this.autoBalanceLabelRoulette = new System.Windows.Forms.Label();
            this.checkBoxAutoSaldoRoulette = new System.Windows.Forms.CheckBox();
            this.buttonBalanceAreaRoulette = new System.Windows.Forms.Button();
            this.roulettemainhelpbtn = new System.Windows.Forms.Button();
            this.roulettestopwinlossinfobtn = new System.Windows.Forms.Button();
            this.roulettebalanceinfobtn = new System.Windows.Forms.Button();
            this.btnRouletteOCRWaitingArea = new System.Windows.Forms.Button();
            this.lblRouletteHandLossText = new System.Windows.Forms.Label();
            this.lblRouletteHandLoss = new System.Windows.Forms.Label();
            this.lblRouletteHandWinText = new System.Windows.Forms.Label();
            this.lblRouletteHandWin = new System.Windows.Forms.Label();
            this.lblRouletteGlobalProfitText = new System.Windows.Forms.Label();
            this.lblRouletteGlobalProfit = new System.Windows.Forms.Label();
            this.balanceRouletteTotalValueText = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.btnRouletteStart = new System.Windows.Forms.Button();
            this.lblRouletteValueHand3 = new System.Windows.Forms.Label();
            this.lblRouletteValueHand2 = new System.Windows.Forms.Label();
            this.lblRouletteValueHand1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblRouletteNameConfig = new System.Windows.Forms.Label();
            this.lblRouletteTextConfigUpload = new System.Windows.Forms.Label();
            this.btnRouletteSaveConfig = new System.Windows.Forms.Button();
            this.btnRouletteLoadConfig = new System.Windows.Forms.Button();
            this.btnRouletteOCRWinArea = new System.Windows.Forms.Button();
            this.btnRouletteOCRHand3 = new System.Windows.Forms.Button();
            this.btnRouletteOCRHand2 = new System.Windows.Forms.Button();
            this.btnRouletteOCRHand1 = new System.Windows.Forms.Button();
            this.labelRouletteOCRArea = new System.Windows.Forms.Label();
            this.panelRoulettePlayed3 = new System.Windows.Forms.Panel();
            this.btnRoulettePlayed3Number36 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed3Number35 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed3Number34 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed3Number33 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed3Number32 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed3Number31 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed3Number30 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed3Number29 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed3Number28 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed3Number27 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed3Number26 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed3Number25 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed3Number24 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed3Number23 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed3Number22 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed3Number21 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed3Number20 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed3Number19 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed3Number18 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed3Number17 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed3Number16 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed3Number15 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed3Number14 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed3Number13 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed3Number12 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed3Number11 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed3Number10 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed3Number9 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed3Number8 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed3Number7 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed3Number6 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed3Number5 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed3Number4 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed3Number3 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed3Number2 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed3Number1 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed3Number0 = new System.Windows.Forms.Button();
            this.panelRoulettePlayed2 = new System.Windows.Forms.Panel();
            this.btnRoulettePlayed2Number36 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed2Number35 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed2Number34 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed2Number33 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed2Number32 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed2Number31 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed2Number30 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed2Number29 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed2Number28 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed2Number27 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed2Number26 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed2Number25 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed2Number24 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed2Number23 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed2Number22 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed2Number21 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed2Number20 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed2Number19 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed2Number18 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed2Number17 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed2Number16 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed2Number15 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed2Number14 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed2Number13 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed2Number12 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed2Number11 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed2Number10 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed2Number9 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed2Number8 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed2Number7 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed2Number6 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed2Number5 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed2Number4 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed2Number3 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed2Number2 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed2Number1 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed2Number0 = new System.Windows.Forms.Button();
            this.panelRoulettePlayed1 = new System.Windows.Forms.Panel();
            this.btnRoulettePlayed1Number36 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed1Number35 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed1Number34 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed1Number33 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed1Number32 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed1Number31 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed1Number30 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed1Number29 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed1Number28 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed1Number27 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed1Number26 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed1Number25 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed1Number24 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed1Number23 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed1Number22 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed1Number21 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed1Number20 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed1Number19 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed1Number18 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed1Number17 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed1Number16 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed1Number15 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed1Number14 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed1Number13 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed1Number12 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed1Number11 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed1Number10 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed1Number9 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed1Number8 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed1Number7 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed1Number6 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed1Number5 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed1Number4 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed1Number3 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed1Number2 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed1Number1 = new System.Windows.Forms.Button();
            this.btnRoulettePlayed1Number0 = new System.Windows.Forms.Button();
            this.lblRoulettePlayed3 = new System.Windows.Forms.Label();
            this.lblRoulettePlayed2 = new System.Windows.Forms.Label();
            this.lblRoulettePlayed1 = new System.Windows.Forms.Label();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.checkSkipPostSculping = new System.Windows.Forms.CheckBox();
            this.checkPragmaticFilter = new System.Windows.Forms.CheckBox();
            this.buttonBet = new System.Windows.Forms.Button();
            this.timeElapsedValueToChange = new System.Windows.Forms.Label();
            this.labelTimeElapsed = new System.Windows.Forms.Label();
            this.textAreaPuntare = new System.Windows.Forms.TextBox();
            this.textAreaPlayer = new System.Windows.Forms.TextBox();
            this.textAreaBench = new System.Windows.Forms.TextBox();
            this.textAreaWin = new System.Windows.Forms.TextBox();
            this.textAreaTie = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.numberDeckValueToChange = new System.Windows.Forms.Label();
            this.labelNumberDeck = new System.Windows.Forms.Label();
            this.typeGamenInfobtn = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.baccaratDemoBtnRadioEnabled = new System.Windows.Forms.RadioButton();
            this.baccaratDemoBtnRadioDisabled = new System.Windows.Forms.RadioButton();
            this.label6 = new System.Windows.Forms.Label();
            this.labelEnvironment = new System.Windows.Forms.Label();
            this.textAreaInfoBtn = new System.Windows.Forms.Button();
            this.labelTextAreaGiocatore = new System.Windows.Forms.Label();
            this.labelTextAreaBanco = new System.Windows.Forms.Label();
            this.labelTextAreaVince = new System.Windows.Forms.Label();
            this.labelTextAreaTie = new System.Windows.Forms.Label();
            this.numberChangeEndDeck = new System.Windows.Forms.NumericUpDown();
            this.labelChangeNumberEndDeck = new System.Windows.Forms.Label();
            this.labelZoomPerc = new System.Windows.Forms.Label();
            this.autoBalanceLabel = new System.Windows.Forms.Label();
            this.checkBoxAutoSaldo = new System.Windows.Forms.CheckBox();
            this.mainareehelpbtn = new System.Windows.Forms.Button();
            this.stopwinlossinfobtn = new System.Windows.Forms.Button();
            this.balanceinfobtn = new System.Windows.Forms.Button();
            this.cardcolorsinfobtn = new System.Windows.Forms.Button();
            this.martingalaHelpBtn = new System.Windows.Forms.Button();
            this.mainhelpbtn = new System.Windows.Forms.Button();
            this.customFichesPanel = new System.Windows.Forms.Panel();
            this.noFichesLabel = new System.Windows.Forms.Label();
            this.customFichesEditBtn = new System.Windows.Forms.Button();
            this.lblNameConfig = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnSaveConfig = new System.Windows.Forms.Button();
            this.labelRiconoscimentoFiches = new System.Windows.Forms.Label();
            this.labelStatus = new System.Windows.Forms.Label();
            this.balanceTotalValueText = new System.Windows.Forms.Label();
            this.labelStartBalance = new System.Windows.Forms.Label();
            this.checkSafeWin = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtZoomMonitor = new System.Windows.Forms.NumericUpDown();
            this.labelNumberProfittoSculping = new System.Windows.Forms.Label();
            this.labelProfittoSculping = new System.Windows.Forms.Label();
            this.labelNumerLose = new System.Windows.Forms.Label();
            this.labelNumberProfittoGlobale = new System.Windows.Forms.Label();
            this.labelNumerWin = new System.Windows.Forms.Label();
            this.labelProfittoGlobale = new System.Windows.Forms.Label();
            this.labelVinte = new System.Windows.Forms.Label();
            this.panelMartingala = new System.Windows.Forms.Panel();
            this.labelPerse = new System.Windows.Forms.Label();
            this.labelMartingala = new System.Windows.Forms.Label();
            this.labelPerc = new System.Windows.Forms.Label();
            this.safeWinPerc = new System.Windows.Forms.NumericUpDown();
            this.btnAddMartingala = new System.Windows.Forms.Button();
            this.labelSafeWin = new System.Windows.Forms.Label();
            this.buttonLoadConfig = new System.Windows.Forms.Button();
            this.labelStopWinGlob = new System.Windows.Forms.Label();
            this.labelStopLoss = new System.Windows.Forms.Label();
            this.buttonStart = new System.Windows.Forms.Button();
            this.groupBoxMode = new System.Windows.Forms.GroupBox();
            this.radioModeMonocolore = new System.Windows.Forms.RadioButton();
            this.radioModeAlternata = new System.Windows.Forms.RadioButton();
            this.labelMode = new System.Windows.Forms.Label();
            this.labelRiconoscimentoArea = new System.Windows.Forms.Label();
            this.groupBoxStartColor = new System.Windows.Forms.GroupBox();
            this.radioColorBlu = new System.Windows.Forms.RadioButton();
            this.radioColorRed = new System.Windows.Forms.RadioButton();
            this.labelColorStart = new System.Windows.Forms.Label();
            this.labelStopWin = new System.Windows.Forms.Label();
            this.buttonFish250 = new System.Windows.Forms.Button();
            this.buttonRed = new System.Windows.Forms.Button();
            this.buttonFish100 = new System.Windows.Forms.Button();
            this.buttonBlu = new System.Windows.Forms.Button();
            this.buttonFish1 = new System.Windows.Forms.Button();
            this.buttonDoubling = new System.Windows.Forms.Button();
            this.buttonBalanceArea = new System.Windows.Forms.Button();
            this.buttonFish500 = new System.Windows.Forms.Button();
            this.buttonAreaVincita = new System.Windows.Forms.Button();
            this.buttonDeckArea = new System.Windows.Forms.Button();
            this.buttonAreaCentrale = new System.Windows.Forms.Button();
            this.buttonFish5 = new System.Windows.Forms.Button();
            this.buttonFish25 = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.txtComandiRicevuti = new System.Windows.Forms.TextBox();
            this.tabPage4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.martingala4IndexAlarmValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.martingala4ChangeColorValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.martingala4EndDeckValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.martingala4StartDeckValue)).BeginInit();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.martingala3IndexAlarmValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.martingala3ChangeColorValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.martingala3EndDeckValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.martingala3StartDeckValue)).BeginInit();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.martingala2IndexAlarmValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.martingala2ChangeColorValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.martingala2EndDeckValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.martingala2StartDeckValue)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.martingala1IndexAlarmValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.martingala1ChangeColorValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.martingala1EndDeckValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.martingala1StartDeckValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown7)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown8)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown9)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown10)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown11)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown12)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown13)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown14)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown15)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown16)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown17)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.balanceRouletteStartValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown18)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericRouletteValueHand3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown19)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericRouletteValueHand2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown20)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericRouletteValueHand1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown21)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.globalRouletteStopLoss)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown22)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.globalRouletteStopWin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown23)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.balanceStartValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown24)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.stopLossValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown25)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.globalStopWinValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown26)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.stopWinValue)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.panelRoulettePlayed3.SuspendLayout();
            this.panelRoulettePlayed2.SuspendLayout();
            this.panelRoulettePlayed1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numberChangeEndDeck)).BeginInit();
            this.customFichesPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtZoomMonitor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.safeWinPerc)).BeginInit();
            this.groupBoxMode.SuspendLayout();
            this.groupBoxStartColor.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // saldoLettoCorrect
            // 
            this.saldoLettoCorrect.AutoSize = true;
            this.saldoLettoCorrect.Location = new System.Drawing.Point(607, 696);
            this.saldoLettoCorrect.Name = "saldoLettoCorrect";
            this.saldoLettoCorrect.Size = new System.Drawing.Size(16, 13);
            this.saldoLettoCorrect.TabIndex = 85;
            this.saldoLettoCorrect.Text = "---";
            this.saldoLettoCorrect.Visible = false;
            // 
            // saldoLetto
            // 
            this.saldoLetto.AutoSize = true;
            this.saldoLetto.Location = new System.Drawing.Point(586, 696);
            this.saldoLetto.Name = "saldoLetto";
            this.saldoLetto.Size = new System.Drawing.Size(16, 13);
            this.saldoLetto.TabIndex = 84;
            this.saldoLetto.Text = "---";
            this.saldoLetto.Visible = false;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(232, 687);
            this.button2.Margin = new System.Windows.Forms.Padding(2);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(56, 19);
            this.button2.TabIndex = 104;
            this.button2.Text = "button2";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Visible = false;
            this.button2.Click += new System.EventHandler(this.button2_Click_1);
            // 
            // readsaldo
            // 
            this.readsaldo.Location = new System.Drawing.Point(404, 690);
            this.readsaldo.Name = "readsaldo";
            this.readsaldo.Size = new System.Drawing.Size(79, 20);
            this.readsaldo.TabIndex = 87;
            this.readsaldo.Visible = false;
            // 
            // showboxbtn
            // 
            this.showboxbtn.Location = new System.Drawing.Point(316, 687);
            this.showboxbtn.Name = "showboxbtn";
            this.showboxbtn.Size = new System.Drawing.Size(75, 23);
            this.showboxbtn.TabIndex = 77;
            this.showboxbtn.Text = "showbox";
            this.showboxbtn.UseVisualStyleBackColor = true;
            this.showboxbtn.Click += new System.EventHandler(this.showboxbtn_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(496, 687);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 64;
            this.button1.Text = "Leggi OCR";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // labelVersion
            // 
            this.labelVersion.AutoSize = true;
            this.labelVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelVersion.Location = new System.Drawing.Point(1051, 682);
            this.labelVersion.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(51, 16);
            this.labelVersion.TabIndex = 73;
            this.labelVersion.Text = "version";
            this.labelVersion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // testBtnWindowOnTop
            // 
            this.testBtnWindowOnTop.Location = new System.Drawing.Point(10, 685);
            this.testBtnWindowOnTop.Name = "testBtnWindowOnTop";
            this.testBtnWindowOnTop.Size = new System.Drawing.Size(166, 23);
            this.testBtnWindowOnTop.TabIndex = 105;
            this.testBtnWindowOnTop.Text = "Mostra Finestra Primo Piano";
            this.testBtnWindowOnTop.UseVisualStyleBackColor = true;
            this.testBtnWindowOnTop.Visible = false;
            this.testBtnWindowOnTop.Click += new System.EventHandler(this.testBtnWindowOnTop_Click);
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.label8);
            this.tabPage4.Controls.Add(this.groupBox5);
            this.tabPage4.Controls.Add(this.groupBox4);
            this.tabPage4.Controls.Add(this.groupBox3);
            this.tabPage4.Controls.Add(this.groupBox2);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage4.Size = new System.Drawing.Size(1086, 642);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Configurazione Martingale";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.Color.Red;
            this.label8.Location = new System.Drawing.Point(4, 618);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(700, 17);
            this.label8.TabIndex = 6;
            this.label8.Text = "Per non considerare una Martingala lasciare i campi \"Mano Iniziale\" e \"Mano Final" +
    "e\" al valore 0";
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.martingala4IndexAlarmValue);
            this.groupBox5.Controls.Add(this.martingala4LblAlarmMartingala);
            this.groupBox5.Controls.Add(this.martingala4ChangeColorValue);
            this.groupBox5.Controls.Add(this.martingala4LblChangeColor);
            this.groupBox5.Controls.Add(this.martingala4EndDeckValue);
            this.groupBox5.Controls.Add(this.martingala4StartDeckValue);
            this.groupBox5.Controls.Add(this.martingala4LblEndDeck);
            this.groupBox5.Controls.Add(this.martingala4LblStartDeck);
            this.groupBox5.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox5.Location = new System.Drawing.Point(814, 5);
            this.groupBox5.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox5.Size = new System.Drawing.Size(262, 603);
            this.groupBox5.TabIndex = 3;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "MARTINGALA #4";
            // 
            // martingala4IndexAlarmValue
            // 
            this.martingala4IndexAlarmValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.martingala4IndexAlarmValue.Location = new System.Drawing.Point(200, 120);
            this.martingala4IndexAlarmValue.Name = "martingala4IndexAlarmValue";
            this.martingala4IndexAlarmValue.Size = new System.Drawing.Size(51, 20);
            this.martingala4IndexAlarmValue.TabIndex = 7;
            this.martingala4IndexAlarmValue.Tag = "controlInput";
            // 
            // martingala4LblAlarmMartingala
            // 
            this.martingala4LblAlarmMartingala.AutoSize = true;
            this.martingala4LblAlarmMartingala.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.martingala4LblAlarmMartingala.Location = new System.Drawing.Point(134, 104);
            this.martingala4LblAlarmMartingala.Name = "martingala4LblAlarmMartingala";
            this.martingala4LblAlarmMartingala.Size = new System.Drawing.Size(123, 13);
            this.martingala4LblAlarmMartingala.TabIndex = 6;
            this.martingala4LblAlarmMartingala.Text = "Allarme Colpo Martingala";
            // 
            // martingala4ChangeColorValue
            // 
            this.martingala4ChangeColorValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.martingala4ChangeColorValue.Location = new System.Drawing.Point(7, 120);
            this.martingala4ChangeColorValue.Margin = new System.Windows.Forms.Padding(2);
            this.martingala4ChangeColorValue.Name = "martingala4ChangeColorValue";
            this.martingala4ChangeColorValue.Size = new System.Drawing.Size(51, 19);
            this.martingala4ChangeColorValue.TabIndex = 5;
            this.martingala4ChangeColorValue.Tag = "controlInput";
            // 
            // martingala4LblChangeColor
            // 
            this.martingala4LblChangeColor.AutoSize = true;
            this.martingala4LblChangeColor.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.martingala4LblChangeColor.Location = new System.Drawing.Point(4, 104);
            this.martingala4LblChangeColor.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.martingala4LblChangeColor.Name = "martingala4LblChangeColor";
            this.martingala4LblChangeColor.Size = new System.Drawing.Size(75, 13);
            this.martingala4LblChangeColor.TabIndex = 4;
            this.martingala4LblChangeColor.Text = "Cambio Colore";
            // 
            // martingala4EndDeckValue
            // 
            this.martingala4EndDeckValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.martingala4EndDeckValue.Location = new System.Drawing.Point(200, 61);
            this.martingala4EndDeckValue.Margin = new System.Windows.Forms.Padding(2);
            this.martingala4EndDeckValue.Name = "martingala4EndDeckValue";
            this.martingala4EndDeckValue.Size = new System.Drawing.Size(51, 19);
            this.martingala4EndDeckValue.TabIndex = 3;
            this.martingala4EndDeckValue.Tag = "controlInput";
            // 
            // martingala4StartDeckValue
            // 
            this.martingala4StartDeckValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.martingala4StartDeckValue.Location = new System.Drawing.Point(7, 61);
            this.martingala4StartDeckValue.Margin = new System.Windows.Forms.Padding(2);
            this.martingala4StartDeckValue.Name = "martingala4StartDeckValue";
            this.martingala4StartDeckValue.Size = new System.Drawing.Size(51, 19);
            this.martingala4StartDeckValue.TabIndex = 2;
            this.martingala4StartDeckValue.Tag = "controlInput";
            // 
            // martingala4LblEndDeck
            // 
            this.martingala4LblEndDeck.AutoSize = true;
            this.martingala4LblEndDeck.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.martingala4LblEndDeck.Location = new System.Drawing.Point(193, 46);
            this.martingala4LblEndDeck.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.martingala4LblEndDeck.Name = "martingala4LblEndDeck";
            this.martingala4LblEndDeck.Size = new System.Drawing.Size(65, 13);
            this.martingala4LblEndDeck.TabIndex = 1;
            this.martingala4LblEndDeck.Text = "Mano Finale";
            // 
            // martingala4LblStartDeck
            // 
            this.martingala4LblStartDeck.AutoSize = true;
            this.martingala4LblStartDeck.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.martingala4LblStartDeck.Location = new System.Drawing.Point(4, 46);
            this.martingala4LblStartDeck.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.martingala4LblStartDeck.Name = "martingala4LblStartDeck";
            this.martingala4LblStartDeck.Size = new System.Drawing.Size(69, 13);
            this.martingala4LblStartDeck.TabIndex = 0;
            this.martingala4LblStartDeck.Text = "Mano Iniziale";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.martingala3IndexAlarmValue);
            this.groupBox4.Controls.Add(this.martingala3LblAlarmMartingala);
            this.groupBox4.Controls.Add(this.martingala3ChangeColorValue);
            this.groupBox4.Controls.Add(this.martingala3LblChangeColor);
            this.groupBox4.Controls.Add(this.martingala3EndDeckValue);
            this.groupBox4.Controls.Add(this.martingala3StartDeckValue);
            this.groupBox4.Controls.Add(this.martingala3LblEndDeck);
            this.groupBox4.Controls.Add(this.martingala3LblStartDeck);
            this.groupBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox4.Location = new System.Drawing.Point(544, 5);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox4.Size = new System.Drawing.Size(262, 603);
            this.groupBox4.TabIndex = 2;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "MARTINGALA #3";
            // 
            // martingala3IndexAlarmValue
            // 
            this.martingala3IndexAlarmValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.martingala3IndexAlarmValue.Location = new System.Drawing.Point(200, 120);
            this.martingala3IndexAlarmValue.Name = "martingala3IndexAlarmValue";
            this.martingala3IndexAlarmValue.Size = new System.Drawing.Size(51, 20);
            this.martingala3IndexAlarmValue.TabIndex = 7;
            this.martingala3IndexAlarmValue.Tag = "controlInput";
            // 
            // martingala3LblAlarmMartingala
            // 
            this.martingala3LblAlarmMartingala.AutoSize = true;
            this.martingala3LblAlarmMartingala.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.martingala3LblAlarmMartingala.Location = new System.Drawing.Point(134, 104);
            this.martingala3LblAlarmMartingala.Name = "martingala3LblAlarmMartingala";
            this.martingala3LblAlarmMartingala.Size = new System.Drawing.Size(123, 13);
            this.martingala3LblAlarmMartingala.TabIndex = 6;
            this.martingala3LblAlarmMartingala.Text = "Allarme Colpo Martingala";
            // 
            // martingala3ChangeColorValue
            // 
            this.martingala3ChangeColorValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.martingala3ChangeColorValue.Location = new System.Drawing.Point(7, 120);
            this.martingala3ChangeColorValue.Margin = new System.Windows.Forms.Padding(2);
            this.martingala3ChangeColorValue.Name = "martingala3ChangeColorValue";
            this.martingala3ChangeColorValue.Size = new System.Drawing.Size(51, 19);
            this.martingala3ChangeColorValue.TabIndex = 5;
            this.martingala3ChangeColorValue.Tag = "controlInput";
            // 
            // martingala3LblChangeColor
            // 
            this.martingala3LblChangeColor.AutoSize = true;
            this.martingala3LblChangeColor.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.martingala3LblChangeColor.Location = new System.Drawing.Point(4, 104);
            this.martingala3LblChangeColor.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.martingala3LblChangeColor.Name = "martingala3LblChangeColor";
            this.martingala3LblChangeColor.Size = new System.Drawing.Size(75, 13);
            this.martingala3LblChangeColor.TabIndex = 4;
            this.martingala3LblChangeColor.Text = "Cambio Colore";
            // 
            // martingala3EndDeckValue
            // 
            this.martingala3EndDeckValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.martingala3EndDeckValue.Location = new System.Drawing.Point(200, 61);
            this.martingala3EndDeckValue.Margin = new System.Windows.Forms.Padding(2);
            this.martingala3EndDeckValue.Name = "martingala3EndDeckValue";
            this.martingala3EndDeckValue.Size = new System.Drawing.Size(51, 19);
            this.martingala3EndDeckValue.TabIndex = 3;
            this.martingala3EndDeckValue.Tag = "controlInput";
            // 
            // martingala3StartDeckValue
            // 
            this.martingala3StartDeckValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.martingala3StartDeckValue.Location = new System.Drawing.Point(7, 61);
            this.martingala3StartDeckValue.Margin = new System.Windows.Forms.Padding(2);
            this.martingala3StartDeckValue.Name = "martingala3StartDeckValue";
            this.martingala3StartDeckValue.Size = new System.Drawing.Size(51, 19);
            this.martingala3StartDeckValue.TabIndex = 2;
            this.martingala3StartDeckValue.Tag = "controlInput";
            // 
            // martingala3LblEndDeck
            // 
            this.martingala3LblEndDeck.AutoSize = true;
            this.martingala3LblEndDeck.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.martingala3LblEndDeck.Location = new System.Drawing.Point(193, 46);
            this.martingala3LblEndDeck.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.martingala3LblEndDeck.Name = "martingala3LblEndDeck";
            this.martingala3LblEndDeck.Size = new System.Drawing.Size(65, 13);
            this.martingala3LblEndDeck.TabIndex = 1;
            this.martingala3LblEndDeck.Text = "Mano Finale";
            // 
            // martingala3LblStartDeck
            // 
            this.martingala3LblStartDeck.AutoSize = true;
            this.martingala3LblStartDeck.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.martingala3LblStartDeck.Location = new System.Drawing.Point(4, 46);
            this.martingala3LblStartDeck.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.martingala3LblStartDeck.Name = "martingala3LblStartDeck";
            this.martingala3LblStartDeck.Size = new System.Drawing.Size(69, 13);
            this.martingala3LblStartDeck.TabIndex = 0;
            this.martingala3LblStartDeck.Text = "Mano Iniziale";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.martingala2IndexAlarmValue);
            this.groupBox3.Controls.Add(this.martingala2LblAlarmMartingala);
            this.groupBox3.Controls.Add(this.martingala2ChangeColorValue);
            this.groupBox3.Controls.Add(this.martingala2LblChangeColor);
            this.groupBox3.Controls.Add(this.martingala2EndDeckValue);
            this.groupBox3.Controls.Add(this.martingala2StartDeckValue);
            this.groupBox3.Controls.Add(this.martingala2LblEndDeck);
            this.groupBox3.Controls.Add(this.martingala2LblStartDeck);
            this.groupBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox3.Location = new System.Drawing.Point(274, 5);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox3.Size = new System.Drawing.Size(262, 603);
            this.groupBox3.TabIndex = 1;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "MARTINGALA #2";
            // 
            // martingala2IndexAlarmValue
            // 
            this.martingala2IndexAlarmValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.martingala2IndexAlarmValue.Location = new System.Drawing.Point(200, 120);
            this.martingala2IndexAlarmValue.Name = "martingala2IndexAlarmValue";
            this.martingala2IndexAlarmValue.Size = new System.Drawing.Size(51, 20);
            this.martingala2IndexAlarmValue.TabIndex = 7;
            this.martingala2IndexAlarmValue.Tag = "controlInput";
            // 
            // martingala2LblAlarmMartingala
            // 
            this.martingala2LblAlarmMartingala.AutoSize = true;
            this.martingala2LblAlarmMartingala.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.martingala2LblAlarmMartingala.Location = new System.Drawing.Point(134, 104);
            this.martingala2LblAlarmMartingala.Name = "martingala2LblAlarmMartingala";
            this.martingala2LblAlarmMartingala.Size = new System.Drawing.Size(123, 13);
            this.martingala2LblAlarmMartingala.TabIndex = 6;
            this.martingala2LblAlarmMartingala.Text = "Allarme Colpo Martingala";
            // 
            // martingala2ChangeColorValue
            // 
            this.martingala2ChangeColorValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.martingala2ChangeColorValue.Location = new System.Drawing.Point(7, 120);
            this.martingala2ChangeColorValue.Margin = new System.Windows.Forms.Padding(2);
            this.martingala2ChangeColorValue.Name = "martingala2ChangeColorValue";
            this.martingala2ChangeColorValue.Size = new System.Drawing.Size(51, 19);
            this.martingala2ChangeColorValue.TabIndex = 5;
            this.martingala2ChangeColorValue.Tag = "controlInput";
            // 
            // martingala2LblChangeColor
            // 
            this.martingala2LblChangeColor.AutoSize = true;
            this.martingala2LblChangeColor.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.martingala2LblChangeColor.Location = new System.Drawing.Point(4, 104);
            this.martingala2LblChangeColor.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.martingala2LblChangeColor.Name = "martingala2LblChangeColor";
            this.martingala2LblChangeColor.Size = new System.Drawing.Size(75, 13);
            this.martingala2LblChangeColor.TabIndex = 4;
            this.martingala2LblChangeColor.Text = "Cambio Colore";
            // 
            // martingala2EndDeckValue
            // 
            this.martingala2EndDeckValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.martingala2EndDeckValue.Location = new System.Drawing.Point(200, 61);
            this.martingala2EndDeckValue.Margin = new System.Windows.Forms.Padding(2);
            this.martingala2EndDeckValue.Name = "martingala2EndDeckValue";
            this.martingala2EndDeckValue.Size = new System.Drawing.Size(51, 19);
            this.martingala2EndDeckValue.TabIndex = 3;
            this.martingala2EndDeckValue.Tag = "controlInput";
            // 
            // martingala2StartDeckValue
            // 
            this.martingala2StartDeckValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.martingala2StartDeckValue.Location = new System.Drawing.Point(7, 61);
            this.martingala2StartDeckValue.Margin = new System.Windows.Forms.Padding(2);
            this.martingala2StartDeckValue.Name = "martingala2StartDeckValue";
            this.martingala2StartDeckValue.Size = new System.Drawing.Size(51, 19);
            this.martingala2StartDeckValue.TabIndex = 2;
            this.martingala2StartDeckValue.Tag = "controlInput";
            // 
            // martingala2LblEndDeck
            // 
            this.martingala2LblEndDeck.AutoSize = true;
            this.martingala2LblEndDeck.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.martingala2LblEndDeck.Location = new System.Drawing.Point(192, 46);
            this.martingala2LblEndDeck.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.martingala2LblEndDeck.Name = "martingala2LblEndDeck";
            this.martingala2LblEndDeck.Size = new System.Drawing.Size(65, 13);
            this.martingala2LblEndDeck.TabIndex = 1;
            this.martingala2LblEndDeck.Text = "Mano Finale";
            // 
            // martingala2LblStartDeck
            // 
            this.martingala2LblStartDeck.AutoSize = true;
            this.martingala2LblStartDeck.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.martingala2LblStartDeck.Location = new System.Drawing.Point(4, 46);
            this.martingala2LblStartDeck.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.martingala2LblStartDeck.Name = "martingala2LblStartDeck";
            this.martingala2LblStartDeck.Size = new System.Drawing.Size(69, 13);
            this.martingala2LblStartDeck.TabIndex = 0;
            this.martingala2LblStartDeck.Text = "Mano Iniziale";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.martingala1IndexAlarmValue);
            this.groupBox2.Controls.Add(this.martingala1LblAlarmMartingala);
            this.groupBox2.Controls.Add(this.martingala1ChangeColorValue);
            this.groupBox2.Controls.Add(this.martingala1LblChangeColor);
            this.groupBox2.Controls.Add(this.martingala1EndDeckValue);
            this.groupBox2.Controls.Add(this.martingala1StartDeckValue);
            this.groupBox2.Controls.Add(this.martingala1LblEndDeck);
            this.groupBox2.Controls.Add(this.martingala1LblStartDeck);
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(4, 5);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox2.Size = new System.Drawing.Size(262, 603);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "MARTINGALA #1";
            // 
            // martingala1IndexAlarmValue
            // 
            this.martingala1IndexAlarmValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.martingala1IndexAlarmValue.Location = new System.Drawing.Point(200, 120);
            this.martingala1IndexAlarmValue.Name = "martingala1IndexAlarmValue";
            this.martingala1IndexAlarmValue.Size = new System.Drawing.Size(51, 20);
            this.martingala1IndexAlarmValue.TabIndex = 7;
            this.martingala1IndexAlarmValue.Tag = "controlInput";
            // 
            // martingala1LblAlarmMartingala
            // 
            this.martingala1LblAlarmMartingala.AutoSize = true;
            this.martingala1LblAlarmMartingala.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.martingala1LblAlarmMartingala.Location = new System.Drawing.Point(134, 104);
            this.martingala1LblAlarmMartingala.Name = "martingala1LblAlarmMartingala";
            this.martingala1LblAlarmMartingala.Size = new System.Drawing.Size(123, 13);
            this.martingala1LblAlarmMartingala.TabIndex = 6;
            this.martingala1LblAlarmMartingala.Text = "Allarme Colpo Martingala";
            // 
            // martingala1ChangeColorValue
            // 
            this.martingala1ChangeColorValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.martingala1ChangeColorValue.Location = new System.Drawing.Point(7, 120);
            this.martingala1ChangeColorValue.Margin = new System.Windows.Forms.Padding(2);
            this.martingala1ChangeColorValue.Name = "martingala1ChangeColorValue";
            this.martingala1ChangeColorValue.Size = new System.Drawing.Size(51, 19);
            this.martingala1ChangeColorValue.TabIndex = 5;
            this.martingala1ChangeColorValue.Tag = "controlInput";
            // 
            // martingala1LblChangeColor
            // 
            this.martingala1LblChangeColor.AutoSize = true;
            this.martingala1LblChangeColor.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.martingala1LblChangeColor.Location = new System.Drawing.Point(4, 104);
            this.martingala1LblChangeColor.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.martingala1LblChangeColor.Name = "martingala1LblChangeColor";
            this.martingala1LblChangeColor.Size = new System.Drawing.Size(75, 13);
            this.martingala1LblChangeColor.TabIndex = 4;
            this.martingala1LblChangeColor.Text = "Cambio Colore";
            // 
            // martingala1EndDeckValue
            // 
            this.martingala1EndDeckValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.martingala1EndDeckValue.Location = new System.Drawing.Point(200, 61);
            this.martingala1EndDeckValue.Margin = new System.Windows.Forms.Padding(2);
            this.martingala1EndDeckValue.Name = "martingala1EndDeckValue";
            this.martingala1EndDeckValue.Size = new System.Drawing.Size(51, 19);
            this.martingala1EndDeckValue.TabIndex = 3;
            this.martingala1EndDeckValue.Tag = "controlInput";
            // 
            // martingala1StartDeckValue
            // 
            this.martingala1StartDeckValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.martingala1StartDeckValue.Location = new System.Drawing.Point(7, 61);
            this.martingala1StartDeckValue.Margin = new System.Windows.Forms.Padding(2);
            this.martingala1StartDeckValue.Name = "martingala1StartDeckValue";
            this.martingala1StartDeckValue.Size = new System.Drawing.Size(51, 19);
            this.martingala1StartDeckValue.TabIndex = 2;
            this.martingala1StartDeckValue.Tag = "controlInput";
            // 
            // martingala1LblEndDeck
            // 
            this.martingala1LblEndDeck.AutoSize = true;
            this.martingala1LblEndDeck.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.martingala1LblEndDeck.Location = new System.Drawing.Point(192, 46);
            this.martingala1LblEndDeck.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.martingala1LblEndDeck.Name = "martingala1LblEndDeck";
            this.martingala1LblEndDeck.Size = new System.Drawing.Size(65, 13);
            this.martingala1LblEndDeck.TabIndex = 1;
            this.martingala1LblEndDeck.Text = "Mano Finale";
            // 
            // martingala1LblStartDeck
            // 
            this.martingala1LblStartDeck.AutoSize = true;
            this.martingala1LblStartDeck.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.martingala1LblStartDeck.Location = new System.Drawing.Point(4, 46);
            this.martingala1LblStartDeck.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.martingala1LblStartDeck.Name = "martingala1LblStartDeck";
            this.martingala1LblStartDeck.Size = new System.Drawing.Size(69, 13);
            this.martingala1LblStartDeck.TabIndex = 0;
            this.martingala1LblStartDeck.Text = "Mano Iniziale";
            // 
            // numericUpDown
            // 
            this.numericUpDown.Location = new System.Drawing.Point(0, 0);
            this.numericUpDown.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDown.Name = "numericUpDown";
            this.numericUpDown.Size = new System.Drawing.Size(120, 20);
            this.numericUpDown.TabIndex = 0;
            // 
            // numericUpDown2
            // 
            this.numericUpDown2.Location = new System.Drawing.Point(0, 0);
            this.numericUpDown2.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDown2.Name = "numericUpDown2";
            this.numericUpDown2.Size = new System.Drawing.Size(120, 20);
            this.numericUpDown2.TabIndex = 0;
            // 
            // numericUpDown3
            // 
            this.numericUpDown3.Location = new System.Drawing.Point(0, 0);
            this.numericUpDown3.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDown3.Name = "numericUpDown3";
            this.numericUpDown3.Size = new System.Drawing.Size(120, 20);
            this.numericUpDown3.TabIndex = 0;
            // 
            // numericUpDown4
            // 
            this.numericUpDown4.Location = new System.Drawing.Point(0, 0);
            this.numericUpDown4.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDown4.Name = "numericUpDown4";
            this.numericUpDown4.Size = new System.Drawing.Size(120, 20);
            this.numericUpDown4.TabIndex = 0;
            // 
            // numericUpDown5
            // 
            this.numericUpDown5.Location = new System.Drawing.Point(0, 0);
            this.numericUpDown5.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDown5.Name = "numericUpDown5";
            this.numericUpDown5.Size = new System.Drawing.Size(120, 20);
            this.numericUpDown5.TabIndex = 0;
            // 
            // numericUpDown6
            // 
            this.numericUpDown6.Location = new System.Drawing.Point(0, 0);
            this.numericUpDown6.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDown6.Name = "numericUpDown6";
            this.numericUpDown6.Size = new System.Drawing.Size(120, 20);
            this.numericUpDown6.TabIndex = 0;
            // 
            // numericUpDown7
            // 
            this.numericUpDown7.Location = new System.Drawing.Point(0, 0);
            this.numericUpDown7.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDown7.Name = "numericUpDown7";
            this.numericUpDown7.Size = new System.Drawing.Size(120, 20);
            this.numericUpDown7.TabIndex = 0;
            // 
            // numericUpDown8
            // 
            this.numericUpDown8.Location = new System.Drawing.Point(0, 0);
            this.numericUpDown8.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDown8.Name = "numericUpDown8";
            this.numericUpDown8.Size = new System.Drawing.Size(120, 20);
            this.numericUpDown8.TabIndex = 0;
            // 
            // numericUpDown9
            // 
            this.numericUpDown9.Location = new System.Drawing.Point(0, 0);
            this.numericUpDown9.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDown9.Name = "numericUpDown9";
            this.numericUpDown9.Size = new System.Drawing.Size(120, 20);
            this.numericUpDown9.TabIndex = 0;
            // 
            // numericUpDown10
            // 
            this.numericUpDown10.Location = new System.Drawing.Point(0, 0);
            this.numericUpDown10.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDown10.Name = "numericUpDown10";
            this.numericUpDown10.Size = new System.Drawing.Size(120, 20);
            this.numericUpDown10.TabIndex = 0;
            // 
            // numericUpDown11
            // 
            this.numericUpDown11.Location = new System.Drawing.Point(0, 0);
            this.numericUpDown11.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDown11.Name = "numericUpDown11";
            this.numericUpDown11.Size = new System.Drawing.Size(120, 20);
            this.numericUpDown11.TabIndex = 0;
            // 
            // numericUpDown12
            // 
            this.numericUpDown12.Location = new System.Drawing.Point(0, 0);
            this.numericUpDown12.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDown12.Name = "numericUpDown12";
            this.numericUpDown12.Size = new System.Drawing.Size(120, 20);
            this.numericUpDown12.TabIndex = 0;
            // 
            // numericUpDown13
            // 
            this.numericUpDown13.Location = new System.Drawing.Point(0, 0);
            this.numericUpDown13.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDown13.Name = "numericUpDown13";
            this.numericUpDown13.Size = new System.Drawing.Size(120, 20);
            this.numericUpDown13.TabIndex = 0;
            // 
            // numericUpDown14
            // 
            this.numericUpDown14.Location = new System.Drawing.Point(0, 0);
            this.numericUpDown14.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDown14.Name = "numericUpDown14";
            this.numericUpDown14.Size = new System.Drawing.Size(120, 20);
            this.numericUpDown14.TabIndex = 0;
            // 
            // numericUpDown15
            // 
            this.numericUpDown15.Location = new System.Drawing.Point(0, 0);
            this.numericUpDown15.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDown15.Name = "numericUpDown15";
            this.numericUpDown15.Size = new System.Drawing.Size(120, 20);
            this.numericUpDown15.TabIndex = 0;
            // 
            // numericUpDown16
            // 
            this.numericUpDown16.Location = new System.Drawing.Point(0, 0);
            this.numericUpDown16.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDown16.Name = "numericUpDown16";
            this.numericUpDown16.Size = new System.Drawing.Size(120, 20);
            this.numericUpDown16.TabIndex = 0;
            // 
            // numericUpDown17
            // 
            this.numericUpDown17.Location = new System.Drawing.Point(0, 0);
            this.numericUpDown17.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numericUpDown17.Name = "numericUpDown17";
            this.numericUpDown17.Size = new System.Drawing.Size(120, 20);
            this.numericUpDown17.TabIndex = 0;
            // 
            // balanceRouletteStartValue
            // 
            this.balanceRouletteStartValue.DecimalPlaces = 2;
            this.balanceRouletteStartValue.Location = new System.Drawing.Point(758, 76);
            this.balanceRouletteStartValue.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.balanceRouletteStartValue.Name = "balanceRouletteStartValue";
            this.balanceRouletteStartValue.Size = new System.Drawing.Size(80, 20);
            this.balanceRouletteStartValue.TabIndex = 27;
            this.balanceRouletteStartValue.Tag = "controlInputRoulette";
            // 
            // numericUpDown18
            // 
            this.numericUpDown18.Location = new System.Drawing.Point(0, 0);
            this.numericUpDown18.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numericUpDown18.Name = "numericUpDown18";
            this.numericUpDown18.Size = new System.Drawing.Size(120, 20);
            this.numericUpDown18.TabIndex = 0;
            // 
            // numericRouletteValueHand3
            // 
            this.numericRouletteValueHand3.DecimalPlaces = 2;
            this.numericRouletteValueHand3.Location = new System.Drawing.Point(710, 427);
            this.numericRouletteValueHand3.Name = "numericRouletteValueHand3";
            this.numericRouletteValueHand3.Size = new System.Drawing.Size(90, 20);
            this.numericRouletteValueHand3.TabIndex = 24;
            this.numericRouletteValueHand3.Tag = "controlInputRoulette";
            // 
            // numericUpDown19
            // 
            this.numericUpDown19.Location = new System.Drawing.Point(0, 0);
            this.numericUpDown19.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDown19.Name = "numericUpDown19";
            this.numericUpDown19.Size = new System.Drawing.Size(120, 20);
            this.numericUpDown19.TabIndex = 0;
            // 
            // numericRouletteValueHand2
            // 
            this.numericRouletteValueHand2.DecimalPlaces = 2;
            this.numericRouletteValueHand2.Location = new System.Drawing.Point(599, 430);
            this.numericRouletteValueHand2.Name = "numericRouletteValueHand2";
            this.numericRouletteValueHand2.Size = new System.Drawing.Size(90, 20);
            this.numericRouletteValueHand2.TabIndex = 23;
            this.numericRouletteValueHand2.Tag = "controlInputRoulette";
            // 
            // numericUpDown20
            // 
            this.numericUpDown20.Location = new System.Drawing.Point(0, 0);
            this.numericUpDown20.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numericUpDown20.Name = "numericUpDown20";
            this.numericUpDown20.Size = new System.Drawing.Size(120, 20);
            this.numericUpDown20.TabIndex = 0;
            // 
            // numericRouletteValueHand1
            // 
            this.numericRouletteValueHand1.DecimalPlaces = 2;
            this.numericRouletteValueHand1.Location = new System.Drawing.Point(488, 430);
            this.numericRouletteValueHand1.Name = "numericRouletteValueHand1";
            this.numericRouletteValueHand1.Size = new System.Drawing.Size(90, 20);
            this.numericRouletteValueHand1.TabIndex = 22;
            this.numericRouletteValueHand1.Tag = "controlInputRoulette";
            // 
            // numericUpDown21
            // 
            this.numericUpDown21.Location = new System.Drawing.Point(0, 0);
            this.numericUpDown21.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numericUpDown21.Name = "numericUpDown21";
            this.numericUpDown21.Size = new System.Drawing.Size(120, 20);
            this.numericUpDown21.TabIndex = 0;
            // 
            // globalRouletteStopLoss
            // 
            this.globalRouletteStopLoss.DecimalPlaces = 2;
            this.globalRouletteStopLoss.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.globalRouletteStopLoss.Location = new System.Drawing.Point(589, 285);
            this.globalRouletteStopLoss.Name = "globalRouletteStopLoss";
            this.globalRouletteStopLoss.Size = new System.Drawing.Size(60, 20);
            this.globalRouletteStopLoss.TabIndex = 18;
            this.globalRouletteStopLoss.Tag = "controlInputRoulette";
            // 
            // numericUpDown22
            // 
            this.numericUpDown22.Location = new System.Drawing.Point(0, 0);
            this.numericUpDown22.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numericUpDown22.Name = "numericUpDown22";
            this.numericUpDown22.Size = new System.Drawing.Size(120, 20);
            this.numericUpDown22.TabIndex = 0;
            // 
            // globalRouletteStopWin
            // 
            this.globalRouletteStopWin.DecimalPlaces = 2;
            this.globalRouletteStopWin.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.globalRouletteStopWin.Location = new System.Drawing.Point(488, 285);
            this.globalRouletteStopWin.Name = "globalRouletteStopWin";
            this.globalRouletteStopWin.Size = new System.Drawing.Size(60, 20);
            this.globalRouletteStopWin.TabIndex = 17;
            this.globalRouletteStopWin.Tag = "controlInputRoulette";
            // 
            // numericUpDown23
            // 
            this.numericUpDown23.Location = new System.Drawing.Point(0, 0);
            this.numericUpDown23.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numericUpDown23.Name = "numericUpDown23";
            this.numericUpDown23.Size = new System.Drawing.Size(120, 20);
            this.numericUpDown23.TabIndex = 0;
            // 
            // balanceStartValue
            // 
            this.balanceStartValue.DecimalPlaces = 2;
            this.balanceStartValue.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.balanceStartValue.Location = new System.Drawing.Point(854, 80);
            this.balanceStartValue.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.balanceStartValue.Name = "balanceStartValue";
            this.balanceStartValue.Size = new System.Drawing.Size(80, 20);
            this.balanceStartValue.TabIndex = 67;
            this.balanceStartValue.Tag = "controlInput";
            // 
            // numericUpDown24
            // 
            this.numericUpDown24.Location = new System.Drawing.Point(0, 0);
            this.numericUpDown24.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numericUpDown24.Name = "numericUpDown24";
            this.numericUpDown24.Size = new System.Drawing.Size(120, 20);
            this.numericUpDown24.TabIndex = 0;
            // 
            // stopLossValue
            // 
            this.stopLossValue.DecimalPlaces = 2;
            this.stopLossValue.Location = new System.Drawing.Point(305, 120);
            this.stopLossValue.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.stopLossValue.Name = "stopLossValue";
            this.stopLossValue.Size = new System.Drawing.Size(60, 20);
            this.stopLossValue.TabIndex = 3;
            this.stopLossValue.Tag = "controlInput";
            // 
            // numericUpDown25
            // 
            this.numericUpDown25.Location = new System.Drawing.Point(0, 0);
            this.numericUpDown25.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numericUpDown25.Name = "numericUpDown25";
            this.numericUpDown25.Size = new System.Drawing.Size(120, 20);
            this.numericUpDown25.TabIndex = 0;
            // 
            // globalStopWinValue
            // 
            this.globalStopWinValue.DecimalPlaces = 2;
            this.globalStopWinValue.Location = new System.Drawing.Point(136, 120);
            this.globalStopWinValue.Maximum = new decimal(new int[] {
            20000,
            0,
            0,
            0});
            this.globalStopWinValue.Name = "globalStopWinValue";
            this.globalStopWinValue.Size = new System.Drawing.Size(60, 20);
            this.globalStopWinValue.TabIndex = 1;
            this.globalStopWinValue.Tag = "controlInput";
            // 
            // numericUpDown26
            // 
            this.numericUpDown26.Location = new System.Drawing.Point(0, 0);
            this.numericUpDown26.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numericUpDown26.Name = "numericUpDown26";
            this.numericUpDown26.Size = new System.Drawing.Size(120, 20);
            this.numericUpDown26.TabIndex = 0;
            // 
            // stopWinValue
            // 
            this.stopWinValue.DecimalPlaces = 2;
            this.stopWinValue.Location = new System.Drawing.Point(222, 120);
            this.stopWinValue.Name = "stopWinValue";
            this.stopWinValue.Size = new System.Drawing.Size(60, 20);
            this.stopWinValue.TabIndex = 2;
            this.stopWinValue.Tag = "controlInput";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.sendEndSculpingMessage);
            this.tabPage2.Controls.Add(this.btnSendTelegram);
            this.tabPage2.Controls.Add(this.btnStartTelegram);
            this.tabPage2.Controls.Add(this.textChatName);
            this.tabPage2.Controls.Add(this.textVerifiedCode);
            this.tabPage2.Controls.Add(this.textActualPhone);
            this.tabPage2.Controls.Add(this.labelChatName);
            this.tabPage2.Controls.Add(this.labelVerifiedCode);
            this.tabPage2.Controls.Add(this.labelActualPhone);
            this.tabPage2.Controls.Add(this.labelTGminiguide);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1086, 642);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Configurazione Telegram";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // sendEndSculpingMessage
            // 
            this.sendEndSculpingMessage.AutoSize = true;
            this.sendEndSculpingMessage.Location = new System.Drawing.Point(35, 210);
            this.sendEndSculpingMessage.Name = "sendEndSculpingMessage";
            this.sendEndSculpingMessage.Size = new System.Drawing.Size(187, 17);
            this.sendEndSculpingMessage.TabIndex = 10;
            this.sendEndSculpingMessage.Text = "Abilita messaggio di End Sculping.";
            this.sendEndSculpingMessage.UseVisualStyleBackColor = true;
            // 
            // btnSendTelegram
            // 
            this.btnSendTelegram.Location = new System.Drawing.Point(144, 144);
            this.btnSendTelegram.Name = "btnSendTelegram";
            this.btnSendTelegram.Size = new System.Drawing.Size(96, 48);
            this.btnSendTelegram.TabIndex = 9;
            this.btnSendTelegram.Text = "VERIFICA NUMERO";
            this.btnSendTelegram.UseVisualStyleBackColor = true;
            this.btnSendTelegram.Click += new System.EventHandler(this.btnSendTelegram_Click);
            // 
            // btnStartTelegram
            // 
            this.btnStartTelegram.Location = new System.Drawing.Point(32, 144);
            this.btnStartTelegram.Name = "btnStartTelegram";
            this.btnStartTelegram.Size = new System.Drawing.Size(96, 48);
            this.btnStartTelegram.TabIndex = 8;
            this.btnStartTelegram.Text = "CONNETTI";
            this.btnStartTelegram.UseVisualStyleBackColor = true;
            this.btnStartTelegram.Click += new System.EventHandler(this.btnStartTelegram_Click);
            // 
            // textChatName
            // 
            this.textChatName.Location = new System.Drawing.Point(128, 96);
            this.textChatName.Name = "textChatName";
            this.textChatName.Size = new System.Drawing.Size(128, 20);
            this.textChatName.TabIndex = 7;
            // 
            // textVerifiedCode
            // 
            this.textVerifiedCode.Location = new System.Drawing.Point(128, 64);
            this.textVerifiedCode.Name = "textVerifiedCode";
            this.textVerifiedCode.Size = new System.Drawing.Size(128, 20);
            this.textVerifiedCode.TabIndex = 5;
            // 
            // textActualPhone
            // 
            this.textActualPhone.Location = new System.Drawing.Point(128, 32);
            this.textActualPhone.Name = "textActualPhone";
            this.textActualPhone.Size = new System.Drawing.Size(128, 20);
            this.textActualPhone.TabIndex = 3;
            this.textActualPhone.Text = "+39 ";
            // 
            // labelChatName
            // 
            this.labelChatName.AutoSize = true;
            this.labelChatName.Location = new System.Drawing.Point(32, 96);
            this.labelChatName.Name = "labelChatName";
            this.labelChatName.Size = new System.Drawing.Size(59, 13);
            this.labelChatName.TabIndex = 6;
            this.labelChatName.Text = "Nome chat";
            // 
            // labelVerifiedCode
            // 
            this.labelVerifiedCode.AutoSize = true;
            this.labelVerifiedCode.Location = new System.Drawing.Point(32, 64);
            this.labelVerifiedCode.Name = "labelVerifiedCode";
            this.labelVerifiedCode.Size = new System.Drawing.Size(88, 13);
            this.labelVerifiedCode.TabIndex = 4;
            this.labelVerifiedCode.Text = "Codice di verifica";
            // 
            // labelActualPhone
            // 
            this.labelActualPhone.AutoSize = true;
            this.labelActualPhone.Location = new System.Drawing.Point(32, 32);
            this.labelActualPhone.Name = "labelActualPhone";
            this.labelActualPhone.Size = new System.Drawing.Size(65, 13);
            this.labelActualPhone.TabIndex = 1;
            this.labelActualPhone.Text = "Numero Tel.";
            // 
            // labelTGminiguide
            // 
            this.labelTGminiguide.Location = new System.Drawing.Point(288, 32);
            this.labelTGminiguide.Name = "labelTGminiguide";
            this.labelTGminiguide.Size = new System.Drawing.Size(256, 160);
            this.labelTGminiguide.TabIndex = 1;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.labelStatusRoulette);
            this.tabPage3.Controls.Add(this.autoBalanceLabelRoulette);
            this.tabPage3.Controls.Add(this.checkBoxAutoSaldoRoulette);
            this.tabPage3.Controls.Add(this.buttonBalanceAreaRoulette);
            this.tabPage3.Controls.Add(this.roulettemainhelpbtn);
            this.tabPage3.Controls.Add(this.roulettestopwinlossinfobtn);
            this.tabPage3.Controls.Add(this.roulettebalanceinfobtn);
            this.tabPage3.Controls.Add(this.btnRouletteOCRWaitingArea);
            this.tabPage3.Controls.Add(this.lblRouletteHandLossText);
            this.tabPage3.Controls.Add(this.lblRouletteHandLoss);
            this.tabPage3.Controls.Add(this.lblRouletteHandWinText);
            this.tabPage3.Controls.Add(this.lblRouletteHandWin);
            this.tabPage3.Controls.Add(this.lblRouletteGlobalProfitText);
            this.tabPage3.Controls.Add(this.lblRouletteGlobalProfit);
            this.tabPage3.Controls.Add(this.balanceRouletteTotalValueText);
            this.tabPage3.Controls.Add(this.balanceRouletteStartValue);
            this.tabPage3.Controls.Add(this.label5);
            this.tabPage3.Controls.Add(this.btnRouletteStart);
            this.tabPage3.Controls.Add(this.numericRouletteValueHand3);
            this.tabPage3.Controls.Add(this.numericRouletteValueHand2);
            this.tabPage3.Controls.Add(this.numericRouletteValueHand1);
            this.tabPage3.Controls.Add(this.lblRouletteValueHand3);
            this.tabPage3.Controls.Add(this.lblRouletteValueHand2);
            this.tabPage3.Controls.Add(this.lblRouletteValueHand1);
            this.tabPage3.Controls.Add(this.globalRouletteStopLoss);
            this.tabPage3.Controls.Add(this.globalRouletteStopWin);
            this.tabPage3.Controls.Add(this.label4);
            this.tabPage3.Controls.Add(this.label3);
            this.tabPage3.Controls.Add(this.lblRouletteNameConfig);
            this.tabPage3.Controls.Add(this.lblRouletteTextConfigUpload);
            this.tabPage3.Controls.Add(this.btnRouletteSaveConfig);
            this.tabPage3.Controls.Add(this.btnRouletteLoadConfig);
            this.tabPage3.Controls.Add(this.btnRouletteOCRWinArea);
            this.tabPage3.Controls.Add(this.btnRouletteOCRHand3);
            this.tabPage3.Controls.Add(this.btnRouletteOCRHand2);
            this.tabPage3.Controls.Add(this.btnRouletteOCRHand1);
            this.tabPage3.Controls.Add(this.labelRouletteOCRArea);
            this.tabPage3.Controls.Add(this.panelRoulettePlayed3);
            this.tabPage3.Controls.Add(this.panelRoulettePlayed2);
            this.tabPage3.Controls.Add(this.panelRoulettePlayed1);
            this.tabPage3.Controls.Add(this.lblRoulettePlayed3);
            this.tabPage3.Controls.Add(this.lblRoulettePlayed2);
            this.tabPage3.Controls.Add(this.lblRoulettePlayed1);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(1086, 642);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Configurazione Roulette";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // labelStatusRoulette
            // 
            this.labelStatusRoulette.AutoSize = true;
            this.labelStatusRoulette.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStatusRoulette.Location = new System.Drawing.Point(492, 519);
            this.labelStatusRoulette.Name = "labelStatusRoulette";
            this.labelStatusRoulette.Size = new System.Drawing.Size(111, 24);
            this.labelStatusRoulette.TabIndex = 92;
            this.labelStatusRoulette.Text = "Bot Inattivo";
            this.labelStatusRoulette.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // autoBalanceLabelRoulette
            // 
            this.autoBalanceLabelRoulette.AutoSize = true;
            this.autoBalanceLabelRoulette.Location = new System.Drawing.Point(768, 55);
            this.autoBalanceLabelRoulette.Name = "autoBalanceLabelRoulette";
            this.autoBalanceLabelRoulette.Size = new System.Drawing.Size(70, 13);
            this.autoBalanceLabelRoulette.TabIndex = 91;
            this.autoBalanceLabelRoulette.Text = "Saldo Autom.";
            // 
            // checkBoxAutoSaldoRoulette
            // 
            this.checkBoxAutoSaldoRoulette.AutoSize = true;
            this.checkBoxAutoSaldoRoulette.Location = new System.Drawing.Point(747, 55);
            this.checkBoxAutoSaldoRoulette.Name = "checkBoxAutoSaldoRoulette";
            this.checkBoxAutoSaldoRoulette.Size = new System.Drawing.Size(15, 14);
            this.checkBoxAutoSaldoRoulette.TabIndex = 90;
            this.checkBoxAutoSaldoRoulette.UseVisualStyleBackColor = true;
            // 
            // buttonBalanceAreaRoulette
            // 
            this.buttonBalanceAreaRoulette.BackColor = System.Drawing.Color.Transparent;
            this.buttonBalanceAreaRoulette.Location = new System.Drawing.Point(653, 50);
            this.buttonBalanceAreaRoulette.Name = "buttonBalanceAreaRoulette";
            this.buttonBalanceAreaRoulette.Size = new System.Drawing.Size(80, 23);
            this.buttonBalanceAreaRoulette.TabIndex = 85;
            this.buttonBalanceAreaRoulette.Tag = "controlInputRoulette";
            this.buttonBalanceAreaRoulette.Text = "Area Saldo";
            this.buttonBalanceAreaRoulette.UseVisualStyleBackColor = false;
            this.buttonBalanceAreaRoulette.Click += new System.EventHandler(this.buttonBalanceAreaRoulette_Click);
            // 
            // roulettemainhelpbtn
            // 
            this.roulettemainhelpbtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.roulettemainhelpbtn.ForeColor = System.Drawing.SystemColors.Highlight;
            this.roulettemainhelpbtn.Location = new System.Drawing.Point(878, 572);
            this.roulettemainhelpbtn.Name = "roulettemainhelpbtn";
            this.roulettemainhelpbtn.Size = new System.Drawing.Size(37, 23);
            this.roulettemainhelpbtn.TabIndex = 84;
            this.roulettemainhelpbtn.Text = "?";
            this.roulettemainhelpbtn.UseVisualStyleBackColor = true;
            this.roulettemainhelpbtn.Click += new System.EventHandler(this.roulettemainhelpbtn_Click);
            // 
            // roulettestopwinlossinfobtn
            // 
            this.roulettestopwinlossinfobtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.roulettestopwinlossinfobtn.ForeColor = System.Drawing.SystemColors.Highlight;
            this.roulettestopwinlossinfobtn.Location = new System.Drawing.Point(878, 282);
            this.roulettestopwinlossinfobtn.Name = "roulettestopwinlossinfobtn";
            this.roulettestopwinlossinfobtn.Size = new System.Drawing.Size(37, 23);
            this.roulettestopwinlossinfobtn.TabIndex = 83;
            this.roulettestopwinlossinfobtn.Text = "?";
            this.roulettestopwinlossinfobtn.UseVisualStyleBackColor = true;
            this.roulettestopwinlossinfobtn.Click += new System.EventHandler(this.roulettestopwinlossinfobtn_Click);
            // 
            // roulettebalanceinfobtn
            // 
            this.roulettebalanceinfobtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.roulettebalanceinfobtn.ForeColor = System.Drawing.SystemColors.Highlight;
            this.roulettebalanceinfobtn.Location = new System.Drawing.Point(878, 73);
            this.roulettebalanceinfobtn.Name = "roulettebalanceinfobtn";
            this.roulettebalanceinfobtn.Size = new System.Drawing.Size(37, 23);
            this.roulettebalanceinfobtn.TabIndex = 82;
            this.roulettebalanceinfobtn.Text = "?";
            this.roulettebalanceinfobtn.UseVisualStyleBackColor = true;
            this.roulettebalanceinfobtn.Click += new System.EventHandler(this.roulettebalanceinfobtn_Click);
            // 
            // btnRouletteOCRWaitingArea
            // 
            this.btnRouletteOCRWaitingArea.Location = new System.Drawing.Point(608, 370);
            this.btnRouletteOCRWaitingArea.Name = "btnRouletteOCRWaitingArea";
            this.btnRouletteOCRWaitingArea.Size = new System.Drawing.Size(117, 23);
            this.btnRouletteOCRWaitingArea.TabIndex = 35;
            this.btnRouletteOCRWaitingArea.Tag = "controlInputRoulette";
            this.btnRouletteOCRWaitingArea.Text = "Area Riposo";
            this.btnRouletteOCRWaitingArea.UseVisualStyleBackColor = true;
            this.btnRouletteOCRWaitingArea.Click += new System.EventHandler(this.btnRouletteAreaWait_Click);
            // 
            // lblRouletteHandLossText
            // 
            this.lblRouletteHandLossText.AutoSize = true;
            this.lblRouletteHandLossText.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lblRouletteHandLossText.ForeColor = System.Drawing.Color.IndianRed;
            this.lblRouletteHandLossText.Location = new System.Drawing.Point(737, 213);
            this.lblRouletteHandLossText.Name = "lblRouletteHandLossText";
            this.lblRouletteHandLossText.Size = new System.Drawing.Size(51, 20);
            this.lblRouletteHandLossText.TabIndex = 34;
            this.lblRouletteHandLossText.Text = "label1";
            // 
            // lblRouletteHandLoss
            // 
            this.lblRouletteHandLoss.AutoSize = true;
            this.lblRouletteHandLoss.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lblRouletteHandLoss.ForeColor = System.Drawing.Color.IndianRed;
            this.lblRouletteHandLoss.Location = new System.Drawing.Point(647, 213);
            this.lblRouletteHandLoss.Name = "lblRouletteHandLoss";
            this.lblRouletteHandLoss.Size = new System.Drawing.Size(92, 20);
            this.lblRouletteHandLoss.TabIndex = 33;
            this.lblRouletteHandLoss.Text = "Mani Perse:";
            // 
            // lblRouletteHandWinText
            // 
            this.lblRouletteHandWinText.AutoSize = true;
            this.lblRouletteHandWinText.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lblRouletteHandWinText.ForeColor = System.Drawing.Color.ForestGreen;
            this.lblRouletteHandWinText.Location = new System.Drawing.Point(735, 191);
            this.lblRouletteHandWinText.Name = "lblRouletteHandWinText";
            this.lblRouletteHandWinText.Size = new System.Drawing.Size(51, 20);
            this.lblRouletteHandWinText.TabIndex = 32;
            this.lblRouletteHandWinText.Text = "label1";
            // 
            // lblRouletteHandWin
            // 
            this.lblRouletteHandWin.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lblRouletteHandWin.ForeColor = System.Drawing.Color.ForestGreen;
            this.lblRouletteHandWin.Location = new System.Drawing.Point(647, 191);
            this.lblRouletteHandWin.Name = "lblRouletteHandWin";
            this.lblRouletteHandWin.Size = new System.Drawing.Size(88, 20);
            this.lblRouletteHandWin.TabIndex = 31;
            this.lblRouletteHandWin.Text = "Mani Vinte:";
            // 
            // lblRouletteGlobalProfitText
            // 
            this.lblRouletteGlobalProfitText.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F);
            this.lblRouletteGlobalProfitText.Location = new System.Drawing.Point(787, 155);
            this.lblRouletteGlobalProfitText.Name = "lblRouletteGlobalProfitText";
            this.lblRouletteGlobalProfitText.Size = new System.Drawing.Size(81, 24);
            this.lblRouletteGlobalProfitText.TabIndex = 30;
            this.lblRouletteGlobalProfitText.Text = "label1";
            // 
            // lblRouletteGlobalProfit
            // 
            this.lblRouletteGlobalProfit.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F);
            this.lblRouletteGlobalProfit.Location = new System.Drawing.Point(649, 156);
            this.lblRouletteGlobalProfit.Name = "lblRouletteGlobalProfit";
            this.lblRouletteGlobalProfit.Size = new System.Drawing.Size(141, 24);
            this.lblRouletteGlobalProfit.TabIndex = 29;
            this.lblRouletteGlobalProfit.Text = "Profitto Globale:";
            // 
            // balanceRouletteTotalValueText
            // 
            this.balanceRouletteTotalValueText.AutoEllipsis = true;
            this.balanceRouletteTotalValueText.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F);
            this.balanceRouletteTotalValueText.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.balanceRouletteTotalValueText.Location = new System.Drawing.Point(647, 108);
            this.balanceRouletteTotalValueText.Name = "balanceRouletteTotalValueText";
            this.balanceRouletteTotalValueText.Size = new System.Drawing.Size(286, 31);
            this.balanceRouletteTotalValueText.TabIndex = 28;
            this.balanceRouletteTotalValueText.Text = "Saldo: € 0";
            this.balanceRouletteTotalValueText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.label5.Location = new System.Drawing.Point(649, 76);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(103, 20);
            this.label5.TabIndex = 26;
            this.label5.Text = "Saldo Iniziale";
            // 
            // btnRouletteStart
            // 
            this.btnRouletteStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F);
            this.btnRouletteStart.Location = new System.Drawing.Point(488, 471);
            this.btnRouletteStart.Name = "btnRouletteStart";
            this.btnRouletteStart.Size = new System.Drawing.Size(424, 37);
            this.btnRouletteStart.TabIndex = 25;
            this.btnRouletteStart.Text = "AVVIA ▶";
            this.btnRouletteStart.UseVisualStyleBackColor = true;
            this.btnRouletteStart.Click += new System.EventHandler(this.buttonRouletteStart_Click);
            // 
            // lblRouletteValueHand3
            // 
            this.lblRouletteValueHand3.AutoSize = true;
            this.lblRouletteValueHand3.Location = new System.Drawing.Point(707, 411);
            this.lblRouletteValueHand3.Name = "lblRouletteValueHand3";
            this.lblRouletteValueHand3.Size = new System.Drawing.Size(93, 13);
            this.lblRouletteValueHand3.TabIndex = 21;
            this.lblRouletteValueHand3.Text = "Valore Giocata #3";
            // 
            // lblRouletteValueHand2
            // 
            this.lblRouletteValueHand2.AutoSize = true;
            this.lblRouletteValueHand2.Location = new System.Drawing.Point(596, 411);
            this.lblRouletteValueHand2.Name = "lblRouletteValueHand2";
            this.lblRouletteValueHand2.Size = new System.Drawing.Size(93, 13);
            this.lblRouletteValueHand2.TabIndex = 20;
            this.lblRouletteValueHand2.Text = "Valore Giocata #2";
            // 
            // lblRouletteValueHand1
            // 
            this.lblRouletteValueHand1.AutoSize = true;
            this.lblRouletteValueHand1.Location = new System.Drawing.Point(485, 411);
            this.lblRouletteValueHand1.Name = "lblRouletteValueHand1";
            this.lblRouletteValueHand1.Size = new System.Drawing.Size(93, 13);
            this.lblRouletteValueHand1.TabIndex = 19;
            this.lblRouletteValueHand1.Text = "Valore Giocata #1";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(586, 265);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(87, 13);
            this.label4.TabIndex = 16;
            this.label4.Text = "Global Stop Loss";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(485, 265);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(84, 13);
            this.label3.TabIndex = 15;
            this.label3.Text = "Global Stop Win";
            // 
            // lblRouletteNameConfig
            // 
            this.lblRouletteNameConfig.AutoSize = true;
            this.lblRouletteNameConfig.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.lblRouletteNameConfig.Location = new System.Drawing.Point(354, 25);
            this.lblRouletteNameConfig.Name = "lblRouletteNameConfig";
            this.lblRouletteNameConfig.Size = new System.Drawing.Size(130, 16);
            this.lblRouletteNameConfig.TabIndex = 14;
            this.lblRouletteNameConfig.Text = "Nome_File_Caricato";
            // 
            // lblRouletteTextConfigUpload
            // 
            this.lblRouletteTextConfigUpload.AutoSize = true;
            this.lblRouletteTextConfigUpload.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.lblRouletteTextConfigUpload.Location = new System.Drawing.Point(180, 25);
            this.lblRouletteTextConfigUpload.Name = "lblRouletteTextConfigUpload";
            this.lblRouletteTextConfigUpload.Size = new System.Drawing.Size(174, 16);
            this.lblRouletteTextConfigUpload.TabIndex = 13;
            this.lblRouletteTextConfigUpload.Text = "Configurazione caricata:";
            // 
            // btnRouletteSaveConfig
            // 
            this.btnRouletteSaveConfig.Location = new System.Drawing.Point(12, 55);
            this.btnRouletteSaveConfig.Name = "btnRouletteSaveConfig";
            this.btnRouletteSaveConfig.Size = new System.Drawing.Size(145, 30);
            this.btnRouletteSaveConfig.TabIndex = 12;
            this.btnRouletteSaveConfig.Text = "Salva Configurazione";
            this.btnRouletteSaveConfig.UseVisualStyleBackColor = true;
            this.btnRouletteSaveConfig.Click += new System.EventHandler(this.buttonSaveRouletteConfig_Click);
            // 
            // btnRouletteLoadConfig
            // 
            this.btnRouletteLoadConfig.Location = new System.Drawing.Point(12, 18);
            this.btnRouletteLoadConfig.Name = "btnRouletteLoadConfig";
            this.btnRouletteLoadConfig.Size = new System.Drawing.Size(145, 30);
            this.btnRouletteLoadConfig.TabIndex = 11;
            this.btnRouletteLoadConfig.Text = "Carica Configurazione";
            this.btnRouletteLoadConfig.UseVisualStyleBackColor = true;
            this.btnRouletteLoadConfig.Click += new System.EventHandler(this.buttonLoadRouletteConfig_Click);
            // 
            // btnRouletteOCRWinArea
            // 
            this.btnRouletteOCRWinArea.Location = new System.Drawing.Point(485, 370);
            this.btnRouletteOCRWinArea.Name = "btnRouletteOCRWinArea";
            this.btnRouletteOCRWinArea.Size = new System.Drawing.Size(117, 23);
            this.btnRouletteOCRWinArea.TabIndex = 10;
            this.btnRouletteOCRWinArea.Tag = "controlInputRoulette";
            this.btnRouletteOCRWinArea.Text = "Area Vincita";
            this.btnRouletteOCRWinArea.UseVisualStyleBackColor = true;
            this.btnRouletteOCRWinArea.Click += new System.EventHandler(this.btnRouletteAreaWin_Click);
            // 
            // btnRouletteOCRHand3
            // 
            this.btnRouletteOCRHand3.Location = new System.Drawing.Point(651, 341);
            this.btnRouletteOCRHand3.Name = "btnRouletteOCRHand3";
            this.btnRouletteOCRHand3.Size = new System.Drawing.Size(75, 23);
            this.btnRouletteOCRHand3.TabIndex = 9;
            this.btnRouletteOCRHand3.Tag = "controlInputRoulette";
            this.btnRouletteOCRHand3.Text = "Giocata #3";
            this.btnRouletteOCRHand3.UseVisualStyleBackColor = true;
            this.btnRouletteOCRHand3.Click += new System.EventHandler(this.btnRouletteAreaHand3_Click);
            // 
            // btnRouletteOCRHand2
            // 
            this.btnRouletteOCRHand2.Location = new System.Drawing.Point(568, 341);
            this.btnRouletteOCRHand2.Name = "btnRouletteOCRHand2";
            this.btnRouletteOCRHand2.Size = new System.Drawing.Size(75, 23);
            this.btnRouletteOCRHand2.TabIndex = 8;
            this.btnRouletteOCRHand2.Tag = "controlInputRoulette";
            this.btnRouletteOCRHand2.Text = "Giocata #2";
            this.btnRouletteOCRHand2.UseVisualStyleBackColor = true;
            this.btnRouletteOCRHand2.Click += new System.EventHandler(this.btnRouletteAreaHand2_Click);
            // 
            // btnRouletteOCRHand1
            // 
            this.btnRouletteOCRHand1.Location = new System.Drawing.Point(485, 341);
            this.btnRouletteOCRHand1.Name = "btnRouletteOCRHand1";
            this.btnRouletteOCRHand1.Size = new System.Drawing.Size(75, 23);
            this.btnRouletteOCRHand1.TabIndex = 7;
            this.btnRouletteOCRHand1.Tag = "controlInputRoulette";
            this.btnRouletteOCRHand1.Text = "Giocata #1";
            this.btnRouletteOCRHand1.UseVisualStyleBackColor = true;
            this.btnRouletteOCRHand1.Click += new System.EventHandler(this.btnRouletteAreaHand1_Click);
            // 
            // labelRouletteOCRArea
            // 
            this.labelRouletteOCRArea.AutoSize = true;
            this.labelRouletteOCRArea.Location = new System.Drawing.Point(482, 323);
            this.labelRouletteOCRArea.Name = "labelRouletteOCRArea";
            this.labelRouletteOCRArea.Size = new System.Drawing.Size(108, 13);
            this.labelRouletteOCRArea.TabIndex = 6;
            this.labelRouletteOCRArea.Text = "Riconoscimento Area";
            // 
            // panelRoulettePlayed3
            // 
            this.panelRoulettePlayed3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelRoulettePlayed3.Controls.Add(this.btnRoulettePlayed3Number36);
            this.panelRoulettePlayed3.Controls.Add(this.btnRoulettePlayed3Number35);
            this.panelRoulettePlayed3.Controls.Add(this.btnRoulettePlayed3Number34);
            this.panelRoulettePlayed3.Controls.Add(this.btnRoulettePlayed3Number33);
            this.panelRoulettePlayed3.Controls.Add(this.btnRoulettePlayed3Number32);
            this.panelRoulettePlayed3.Controls.Add(this.btnRoulettePlayed3Number31);
            this.panelRoulettePlayed3.Controls.Add(this.btnRoulettePlayed3Number30);
            this.panelRoulettePlayed3.Controls.Add(this.btnRoulettePlayed3Number29);
            this.panelRoulettePlayed3.Controls.Add(this.btnRoulettePlayed3Number28);
            this.panelRoulettePlayed3.Controls.Add(this.btnRoulettePlayed3Number27);
            this.panelRoulettePlayed3.Controls.Add(this.btnRoulettePlayed3Number26);
            this.panelRoulettePlayed3.Controls.Add(this.btnRoulettePlayed3Number25);
            this.panelRoulettePlayed3.Controls.Add(this.btnRoulettePlayed3Number24);
            this.panelRoulettePlayed3.Controls.Add(this.btnRoulettePlayed3Number23);
            this.panelRoulettePlayed3.Controls.Add(this.btnRoulettePlayed3Number22);
            this.panelRoulettePlayed3.Controls.Add(this.btnRoulettePlayed3Number21);
            this.panelRoulettePlayed3.Controls.Add(this.btnRoulettePlayed3Number20);
            this.panelRoulettePlayed3.Controls.Add(this.btnRoulettePlayed3Number19);
            this.panelRoulettePlayed3.Controls.Add(this.btnRoulettePlayed3Number18);
            this.panelRoulettePlayed3.Controls.Add(this.btnRoulettePlayed3Number17);
            this.panelRoulettePlayed3.Controls.Add(this.btnRoulettePlayed3Number16);
            this.panelRoulettePlayed3.Controls.Add(this.btnRoulettePlayed3Number15);
            this.panelRoulettePlayed3.Controls.Add(this.btnRoulettePlayed3Number14);
            this.panelRoulettePlayed3.Controls.Add(this.btnRoulettePlayed3Number13);
            this.panelRoulettePlayed3.Controls.Add(this.btnRoulettePlayed3Number12);
            this.panelRoulettePlayed3.Controls.Add(this.btnRoulettePlayed3Number11);
            this.panelRoulettePlayed3.Controls.Add(this.btnRoulettePlayed3Number10);
            this.panelRoulettePlayed3.Controls.Add(this.btnRoulettePlayed3Number9);
            this.panelRoulettePlayed3.Controls.Add(this.btnRoulettePlayed3Number8);
            this.panelRoulettePlayed3.Controls.Add(this.btnRoulettePlayed3Number7);
            this.panelRoulettePlayed3.Controls.Add(this.btnRoulettePlayed3Number6);
            this.panelRoulettePlayed3.Controls.Add(this.btnRoulettePlayed3Number5);
            this.panelRoulettePlayed3.Controls.Add(this.btnRoulettePlayed3Number4);
            this.panelRoulettePlayed3.Controls.Add(this.btnRoulettePlayed3Number3);
            this.panelRoulettePlayed3.Controls.Add(this.btnRoulettePlayed3Number2);
            this.panelRoulettePlayed3.Controls.Add(this.btnRoulettePlayed3Number1);
            this.panelRoulettePlayed3.Controls.Add(this.btnRoulettePlayed3Number0);
            this.panelRoulettePlayed3.Location = new System.Drawing.Point(312, 130);
            this.panelRoulettePlayed3.Name = "panelRoulettePlayed3";
            this.panelRoulettePlayed3.Size = new System.Drawing.Size(113, 378);
            this.panelRoulettePlayed3.TabIndex = 5;
            // 
            // btnRoulettePlayed3Number36
            // 
            this.btnRoulettePlayed3Number36.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed3Number36.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number36.Location = new System.Drawing.Point(78, 347);
            this.btnRoulettePlayed3Number36.Name = "btnRoulettePlayed3Number36";
            this.btnRoulettePlayed3Number36.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed3Number36.TabIndex = 36;
            this.btnRoulettePlayed3Number36.Tag = "btnHand3Roulette";
            this.btnRoulettePlayed3Number36.Text = "36";
            this.btnRoulettePlayed3Number36.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed3Number35
            // 
            this.btnRoulettePlayed3Number35.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed3Number35.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number35.Location = new System.Drawing.Point(40, 347);
            this.btnRoulettePlayed3Number35.Name = "btnRoulettePlayed3Number35";
            this.btnRoulettePlayed3Number35.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed3Number35.TabIndex = 35;
            this.btnRoulettePlayed3Number35.Tag = "btnHand3Roulette";
            this.btnRoulettePlayed3Number35.Text = "35";
            this.btnRoulettePlayed3Number35.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed3Number34
            // 
            this.btnRoulettePlayed3Number34.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed3Number34.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number34.Location = new System.Drawing.Point(2, 347);
            this.btnRoulettePlayed3Number34.Name = "btnRoulettePlayed3Number34";
            this.btnRoulettePlayed3Number34.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed3Number34.TabIndex = 34;
            this.btnRoulettePlayed3Number34.Tag = "btnHand3Roulette";
            this.btnRoulettePlayed3Number34.Text = "34";
            this.btnRoulettePlayed3Number34.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed3Number33
            // 
            this.btnRoulettePlayed3Number33.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed3Number33.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number33.Location = new System.Drawing.Point(78, 318);
            this.btnRoulettePlayed3Number33.Name = "btnRoulettePlayed3Number33";
            this.btnRoulettePlayed3Number33.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed3Number33.TabIndex = 33;
            this.btnRoulettePlayed3Number33.Tag = "btnHand3Roulette";
            this.btnRoulettePlayed3Number33.Text = "33";
            this.btnRoulettePlayed3Number33.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed3Number32
            // 
            this.btnRoulettePlayed3Number32.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed3Number32.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number32.Location = new System.Drawing.Point(40, 318);
            this.btnRoulettePlayed3Number32.Name = "btnRoulettePlayed3Number32";
            this.btnRoulettePlayed3Number32.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed3Number32.TabIndex = 32;
            this.btnRoulettePlayed3Number32.Tag = "btnHand3Roulette";
            this.btnRoulettePlayed3Number32.Text = "32";
            this.btnRoulettePlayed3Number32.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed3Number31
            // 
            this.btnRoulettePlayed3Number31.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed3Number31.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number31.Location = new System.Drawing.Point(2, 318);
            this.btnRoulettePlayed3Number31.Name = "btnRoulettePlayed3Number31";
            this.btnRoulettePlayed3Number31.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed3Number31.TabIndex = 31;
            this.btnRoulettePlayed3Number31.Tag = "btnHand3Roulette";
            this.btnRoulettePlayed3Number31.Text = "31";
            this.btnRoulettePlayed3Number31.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed3Number30
            // 
            this.btnRoulettePlayed3Number30.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed3Number30.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number30.Location = new System.Drawing.Point(78, 289);
            this.btnRoulettePlayed3Number30.Name = "btnRoulettePlayed3Number30";
            this.btnRoulettePlayed3Number30.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed3Number30.TabIndex = 30;
            this.btnRoulettePlayed3Number30.Tag = "btnHand3Roulette";
            this.btnRoulettePlayed3Number30.Text = "30";
            this.btnRoulettePlayed3Number30.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed3Number29
            // 
            this.btnRoulettePlayed3Number29.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed3Number29.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number29.Location = new System.Drawing.Point(40, 289);
            this.btnRoulettePlayed3Number29.Name = "btnRoulettePlayed3Number29";
            this.btnRoulettePlayed3Number29.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed3Number29.TabIndex = 29;
            this.btnRoulettePlayed3Number29.Tag = "btnHand3Roulette";
            this.btnRoulettePlayed3Number29.Text = "29";
            this.btnRoulettePlayed3Number29.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed3Number28
            // 
            this.btnRoulettePlayed3Number28.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed3Number28.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number28.Location = new System.Drawing.Point(2, 289);
            this.btnRoulettePlayed3Number28.Name = "btnRoulettePlayed3Number28";
            this.btnRoulettePlayed3Number28.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed3Number28.TabIndex = 28;
            this.btnRoulettePlayed3Number28.Tag = "btnHand3Roulette";
            this.btnRoulettePlayed3Number28.Text = "28";
            this.btnRoulettePlayed3Number28.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed3Number27
            // 
            this.btnRoulettePlayed3Number27.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed3Number27.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number27.Location = new System.Drawing.Point(78, 260);
            this.btnRoulettePlayed3Number27.Name = "btnRoulettePlayed3Number27";
            this.btnRoulettePlayed3Number27.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed3Number27.TabIndex = 27;
            this.btnRoulettePlayed3Number27.Tag = "btnHand3Roulette";
            this.btnRoulettePlayed3Number27.Text = "27";
            this.btnRoulettePlayed3Number27.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed3Number26
            // 
            this.btnRoulettePlayed3Number26.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed3Number26.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number26.Location = new System.Drawing.Point(40, 260);
            this.btnRoulettePlayed3Number26.Name = "btnRoulettePlayed3Number26";
            this.btnRoulettePlayed3Number26.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed3Number26.TabIndex = 26;
            this.btnRoulettePlayed3Number26.Tag = "btnHand3Roulette";
            this.btnRoulettePlayed3Number26.Text = "26";
            this.btnRoulettePlayed3Number26.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed3Number25
            // 
            this.btnRoulettePlayed3Number25.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed3Number25.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number25.Location = new System.Drawing.Point(2, 260);
            this.btnRoulettePlayed3Number25.Name = "btnRoulettePlayed3Number25";
            this.btnRoulettePlayed3Number25.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed3Number25.TabIndex = 25;
            this.btnRoulettePlayed3Number25.Tag = "btnHand3Roulette";
            this.btnRoulettePlayed3Number25.Text = "25";
            this.btnRoulettePlayed3Number25.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed3Number24
            // 
            this.btnRoulettePlayed3Number24.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed3Number24.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number24.Location = new System.Drawing.Point(78, 231);
            this.btnRoulettePlayed3Number24.Name = "btnRoulettePlayed3Number24";
            this.btnRoulettePlayed3Number24.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed3Number24.TabIndex = 24;
            this.btnRoulettePlayed3Number24.Tag = "btnHand3Roulette";
            this.btnRoulettePlayed3Number24.Text = "24";
            this.btnRoulettePlayed3Number24.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed3Number23
            // 
            this.btnRoulettePlayed3Number23.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed3Number23.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number23.Location = new System.Drawing.Point(40, 231);
            this.btnRoulettePlayed3Number23.Name = "btnRoulettePlayed3Number23";
            this.btnRoulettePlayed3Number23.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed3Number23.TabIndex = 23;
            this.btnRoulettePlayed3Number23.Tag = "btnHand3Roulette";
            this.btnRoulettePlayed3Number23.Text = "23";
            this.btnRoulettePlayed3Number23.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed3Number22
            // 
            this.btnRoulettePlayed3Number22.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed3Number22.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number22.Location = new System.Drawing.Point(2, 231);
            this.btnRoulettePlayed3Number22.Name = "btnRoulettePlayed3Number22";
            this.btnRoulettePlayed3Number22.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed3Number22.TabIndex = 22;
            this.btnRoulettePlayed3Number22.Tag = "btnHand3Roulette";
            this.btnRoulettePlayed3Number22.Text = "22";
            this.btnRoulettePlayed3Number22.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed3Number21
            // 
            this.btnRoulettePlayed3Number21.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed3Number21.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number21.Location = new System.Drawing.Point(78, 202);
            this.btnRoulettePlayed3Number21.Name = "btnRoulettePlayed3Number21";
            this.btnRoulettePlayed3Number21.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed3Number21.TabIndex = 21;
            this.btnRoulettePlayed3Number21.Tag = "btnHand3Roulette";
            this.btnRoulettePlayed3Number21.Text = "21";
            this.btnRoulettePlayed3Number21.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed3Number20
            // 
            this.btnRoulettePlayed3Number20.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed3Number20.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number20.Location = new System.Drawing.Point(40, 202);
            this.btnRoulettePlayed3Number20.Name = "btnRoulettePlayed3Number20";
            this.btnRoulettePlayed3Number20.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed3Number20.TabIndex = 20;
            this.btnRoulettePlayed3Number20.Tag = "btnHand3Roulette";
            this.btnRoulettePlayed3Number20.Text = "20";
            this.btnRoulettePlayed3Number20.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed3Number19
            // 
            this.btnRoulettePlayed3Number19.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed3Number19.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number19.Location = new System.Drawing.Point(2, 202);
            this.btnRoulettePlayed3Number19.Name = "btnRoulettePlayed3Number19";
            this.btnRoulettePlayed3Number19.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed3Number19.TabIndex = 19;
            this.btnRoulettePlayed3Number19.Tag = "btnHand3Roulette";
            this.btnRoulettePlayed3Number19.Text = "19";
            this.btnRoulettePlayed3Number19.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed3Number18
            // 
            this.btnRoulettePlayed3Number18.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed3Number18.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number18.Location = new System.Drawing.Point(78, 173);
            this.btnRoulettePlayed3Number18.Name = "btnRoulettePlayed3Number18";
            this.btnRoulettePlayed3Number18.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed3Number18.TabIndex = 18;
            this.btnRoulettePlayed3Number18.Tag = "btnHand3Roulette";
            this.btnRoulettePlayed3Number18.Text = "18";
            this.btnRoulettePlayed3Number18.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed3Number17
            // 
            this.btnRoulettePlayed3Number17.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed3Number17.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number17.Location = new System.Drawing.Point(40, 173);
            this.btnRoulettePlayed3Number17.Name = "btnRoulettePlayed3Number17";
            this.btnRoulettePlayed3Number17.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed3Number17.TabIndex = 17;
            this.btnRoulettePlayed3Number17.Tag = "btnHand3Roulette";
            this.btnRoulettePlayed3Number17.Text = "17";
            this.btnRoulettePlayed3Number17.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed3Number16
            // 
            this.btnRoulettePlayed3Number16.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed3Number16.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number16.Location = new System.Drawing.Point(2, 173);
            this.btnRoulettePlayed3Number16.Name = "btnRoulettePlayed3Number16";
            this.btnRoulettePlayed3Number16.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed3Number16.TabIndex = 16;
            this.btnRoulettePlayed3Number16.Tag = "btnHand3Roulette";
            this.btnRoulettePlayed3Number16.Text = "16";
            this.btnRoulettePlayed3Number16.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed3Number15
            // 
            this.btnRoulettePlayed3Number15.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed3Number15.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number15.Location = new System.Drawing.Point(78, 144);
            this.btnRoulettePlayed3Number15.Name = "btnRoulettePlayed3Number15";
            this.btnRoulettePlayed3Number15.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed3Number15.TabIndex = 15;
            this.btnRoulettePlayed3Number15.Tag = "btnHand3Roulette";
            this.btnRoulettePlayed3Number15.Text = "15";
            this.btnRoulettePlayed3Number15.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed3Number14
            // 
            this.btnRoulettePlayed3Number14.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed3Number14.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number14.Location = new System.Drawing.Point(40, 144);
            this.btnRoulettePlayed3Number14.Name = "btnRoulettePlayed3Number14";
            this.btnRoulettePlayed3Number14.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed3Number14.TabIndex = 14;
            this.btnRoulettePlayed3Number14.Tag = "btnHand3Roulette";
            this.btnRoulettePlayed3Number14.Text = "14";
            this.btnRoulettePlayed3Number14.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed3Number13
            // 
            this.btnRoulettePlayed3Number13.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed3Number13.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number13.Location = new System.Drawing.Point(2, 144);
            this.btnRoulettePlayed3Number13.Name = "btnRoulettePlayed3Number13";
            this.btnRoulettePlayed3Number13.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed3Number13.TabIndex = 13;
            this.btnRoulettePlayed3Number13.Tag = "btnHand3Roulette";
            this.btnRoulettePlayed3Number13.Text = "13";
            this.btnRoulettePlayed3Number13.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed3Number12
            // 
            this.btnRoulettePlayed3Number12.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed3Number12.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number12.Location = new System.Drawing.Point(78, 115);
            this.btnRoulettePlayed3Number12.Name = "btnRoulettePlayed3Number12";
            this.btnRoulettePlayed3Number12.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed3Number12.TabIndex = 12;
            this.btnRoulettePlayed3Number12.Tag = "btnHand3Roulette";
            this.btnRoulettePlayed3Number12.Text = "12";
            this.btnRoulettePlayed3Number12.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed3Number11
            // 
            this.btnRoulettePlayed3Number11.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed3Number11.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number11.Location = new System.Drawing.Point(40, 115);
            this.btnRoulettePlayed3Number11.Name = "btnRoulettePlayed3Number11";
            this.btnRoulettePlayed3Number11.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed3Number11.TabIndex = 11;
            this.btnRoulettePlayed3Number11.Tag = "btnHand3Roulette";
            this.btnRoulettePlayed3Number11.Text = "11";
            this.btnRoulettePlayed3Number11.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed3Number10
            // 
            this.btnRoulettePlayed3Number10.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed3Number10.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number10.Location = new System.Drawing.Point(2, 115);
            this.btnRoulettePlayed3Number10.Name = "btnRoulettePlayed3Number10";
            this.btnRoulettePlayed3Number10.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed3Number10.TabIndex = 10;
            this.btnRoulettePlayed3Number10.Tag = "btnHand3Roulette";
            this.btnRoulettePlayed3Number10.Text = "10";
            this.btnRoulettePlayed3Number10.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed3Number9
            // 
            this.btnRoulettePlayed3Number9.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed3Number9.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number9.Location = new System.Drawing.Point(78, 86);
            this.btnRoulettePlayed3Number9.Name = "btnRoulettePlayed3Number9";
            this.btnRoulettePlayed3Number9.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed3Number9.TabIndex = 9;
            this.btnRoulettePlayed3Number9.Tag = "btnHand3Roulette";
            this.btnRoulettePlayed3Number9.Text = "9";
            this.btnRoulettePlayed3Number9.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed3Number8
            // 
            this.btnRoulettePlayed3Number8.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed3Number8.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number8.Location = new System.Drawing.Point(40, 86);
            this.btnRoulettePlayed3Number8.Name = "btnRoulettePlayed3Number8";
            this.btnRoulettePlayed3Number8.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed3Number8.TabIndex = 8;
            this.btnRoulettePlayed3Number8.Tag = "btnHand3Roulette";
            this.btnRoulettePlayed3Number8.Text = "8";
            this.btnRoulettePlayed3Number8.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed3Number7
            // 
            this.btnRoulettePlayed3Number7.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed3Number7.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number7.Location = new System.Drawing.Point(2, 86);
            this.btnRoulettePlayed3Number7.Name = "btnRoulettePlayed3Number7";
            this.btnRoulettePlayed3Number7.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed3Number7.TabIndex = 7;
            this.btnRoulettePlayed3Number7.Tag = "btnHand3Roulette";
            this.btnRoulettePlayed3Number7.Text = "7";
            this.btnRoulettePlayed3Number7.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed3Number6
            // 
            this.btnRoulettePlayed3Number6.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed3Number6.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number6.Location = new System.Drawing.Point(78, 57);
            this.btnRoulettePlayed3Number6.Name = "btnRoulettePlayed3Number6";
            this.btnRoulettePlayed3Number6.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed3Number6.TabIndex = 6;
            this.btnRoulettePlayed3Number6.Tag = "btnHand3Roulette";
            this.btnRoulettePlayed3Number6.Text = "6";
            this.btnRoulettePlayed3Number6.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed3Number5
            // 
            this.btnRoulettePlayed3Number5.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed3Number5.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number5.Location = new System.Drawing.Point(40, 57);
            this.btnRoulettePlayed3Number5.Name = "btnRoulettePlayed3Number5";
            this.btnRoulettePlayed3Number5.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed3Number5.TabIndex = 5;
            this.btnRoulettePlayed3Number5.Tag = "btnHand3Roulette";
            this.btnRoulettePlayed3Number5.Text = "5";
            this.btnRoulettePlayed3Number5.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed3Number4
            // 
            this.btnRoulettePlayed3Number4.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed3Number4.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number4.Location = new System.Drawing.Point(2, 57);
            this.btnRoulettePlayed3Number4.Name = "btnRoulettePlayed3Number4";
            this.btnRoulettePlayed3Number4.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed3Number4.TabIndex = 4;
            this.btnRoulettePlayed3Number4.Tag = "btnHand3Roulette";
            this.btnRoulettePlayed3Number4.Text = "4";
            this.btnRoulettePlayed3Number4.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed3Number3
            // 
            this.btnRoulettePlayed3Number3.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed3Number3.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number3.Location = new System.Drawing.Point(78, 28);
            this.btnRoulettePlayed3Number3.Name = "btnRoulettePlayed3Number3";
            this.btnRoulettePlayed3Number3.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed3Number3.TabIndex = 3;
            this.btnRoulettePlayed3Number3.Tag = "btnHand3Roulette";
            this.btnRoulettePlayed3Number3.Text = "3";
            this.btnRoulettePlayed3Number3.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed3Number2
            // 
            this.btnRoulettePlayed3Number2.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed3Number2.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number2.Location = new System.Drawing.Point(40, 28);
            this.btnRoulettePlayed3Number2.Name = "btnRoulettePlayed3Number2";
            this.btnRoulettePlayed3Number2.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed3Number2.TabIndex = 2;
            this.btnRoulettePlayed3Number2.Tag = "btnHand3Roulette";
            this.btnRoulettePlayed3Number2.Text = "2";
            this.btnRoulettePlayed3Number2.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed3Number1
            // 
            this.btnRoulettePlayed3Number1.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed3Number1.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number1.Location = new System.Drawing.Point(2, 28);
            this.btnRoulettePlayed3Number1.Name = "btnRoulettePlayed3Number1";
            this.btnRoulettePlayed3Number1.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed3Number1.TabIndex = 1;
            this.btnRoulettePlayed3Number1.Tag = "btnHand3Roulette";
            this.btnRoulettePlayed3Number1.Text = "1";
            this.btnRoulettePlayed3Number1.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed3Number0
            // 
            this.btnRoulettePlayed3Number0.BackColor = System.Drawing.Color.SeaGreen;
            this.btnRoulettePlayed3Number0.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed3Number0.Location = new System.Drawing.Point(14, 3);
            this.btnRoulettePlayed3Number0.Name = "btnRoulettePlayed3Number0";
            this.btnRoulettePlayed3Number0.Size = new System.Drawing.Size(85, 23);
            this.btnRoulettePlayed3Number0.TabIndex = 0;
            this.btnRoulettePlayed3Number0.Tag = "btnHand3Roulette";
            this.btnRoulettePlayed3Number0.Text = "0";
            this.btnRoulettePlayed3Number0.UseVisualStyleBackColor = false;
            // 
            // panelRoulettePlayed2
            // 
            this.panelRoulettePlayed2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelRoulettePlayed2.Controls.Add(this.btnRoulettePlayed2Number36);
            this.panelRoulettePlayed2.Controls.Add(this.btnRoulettePlayed2Number35);
            this.panelRoulettePlayed2.Controls.Add(this.btnRoulettePlayed2Number34);
            this.panelRoulettePlayed2.Controls.Add(this.btnRoulettePlayed2Number33);
            this.panelRoulettePlayed2.Controls.Add(this.btnRoulettePlayed2Number32);
            this.panelRoulettePlayed2.Controls.Add(this.btnRoulettePlayed2Number31);
            this.panelRoulettePlayed2.Controls.Add(this.btnRoulettePlayed2Number30);
            this.panelRoulettePlayed2.Controls.Add(this.btnRoulettePlayed2Number29);
            this.panelRoulettePlayed2.Controls.Add(this.btnRoulettePlayed2Number28);
            this.panelRoulettePlayed2.Controls.Add(this.btnRoulettePlayed2Number27);
            this.panelRoulettePlayed2.Controls.Add(this.btnRoulettePlayed2Number26);
            this.panelRoulettePlayed2.Controls.Add(this.btnRoulettePlayed2Number25);
            this.panelRoulettePlayed2.Controls.Add(this.btnRoulettePlayed2Number24);
            this.panelRoulettePlayed2.Controls.Add(this.btnRoulettePlayed2Number23);
            this.panelRoulettePlayed2.Controls.Add(this.btnRoulettePlayed2Number22);
            this.panelRoulettePlayed2.Controls.Add(this.btnRoulettePlayed2Number21);
            this.panelRoulettePlayed2.Controls.Add(this.btnRoulettePlayed2Number20);
            this.panelRoulettePlayed2.Controls.Add(this.btnRoulettePlayed2Number19);
            this.panelRoulettePlayed2.Controls.Add(this.btnRoulettePlayed2Number18);
            this.panelRoulettePlayed2.Controls.Add(this.btnRoulettePlayed2Number17);
            this.panelRoulettePlayed2.Controls.Add(this.btnRoulettePlayed2Number16);
            this.panelRoulettePlayed2.Controls.Add(this.btnRoulettePlayed2Number15);
            this.panelRoulettePlayed2.Controls.Add(this.btnRoulettePlayed2Number14);
            this.panelRoulettePlayed2.Controls.Add(this.btnRoulettePlayed2Number13);
            this.panelRoulettePlayed2.Controls.Add(this.btnRoulettePlayed2Number12);
            this.panelRoulettePlayed2.Controls.Add(this.btnRoulettePlayed2Number11);
            this.panelRoulettePlayed2.Controls.Add(this.btnRoulettePlayed2Number10);
            this.panelRoulettePlayed2.Controls.Add(this.btnRoulettePlayed2Number9);
            this.panelRoulettePlayed2.Controls.Add(this.btnRoulettePlayed2Number8);
            this.panelRoulettePlayed2.Controls.Add(this.btnRoulettePlayed2Number7);
            this.panelRoulettePlayed2.Controls.Add(this.btnRoulettePlayed2Number6);
            this.panelRoulettePlayed2.Controls.Add(this.btnRoulettePlayed2Number5);
            this.panelRoulettePlayed2.Controls.Add(this.btnRoulettePlayed2Number4);
            this.panelRoulettePlayed2.Controls.Add(this.btnRoulettePlayed2Number3);
            this.panelRoulettePlayed2.Controls.Add(this.btnRoulettePlayed2Number2);
            this.panelRoulettePlayed2.Controls.Add(this.btnRoulettePlayed2Number1);
            this.panelRoulettePlayed2.Controls.Add(this.btnRoulettePlayed2Number0);
            this.panelRoulettePlayed2.Location = new System.Drawing.Point(162, 130);
            this.panelRoulettePlayed2.Name = "panelRoulettePlayed2";
            this.panelRoulettePlayed2.Size = new System.Drawing.Size(113, 378);
            this.panelRoulettePlayed2.TabIndex = 4;
            // 
            // btnRoulettePlayed2Number36
            // 
            this.btnRoulettePlayed2Number36.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed2Number36.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number36.Location = new System.Drawing.Point(78, 347);
            this.btnRoulettePlayed2Number36.Name = "btnRoulettePlayed2Number36";
            this.btnRoulettePlayed2Number36.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed2Number36.TabIndex = 36;
            this.btnRoulettePlayed2Number36.Tag = "btnHand2Roulette";
            this.btnRoulettePlayed2Number36.Text = "36";
            this.btnRoulettePlayed2Number36.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed2Number35
            // 
            this.btnRoulettePlayed2Number35.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed2Number35.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number35.Location = new System.Drawing.Point(40, 347);
            this.btnRoulettePlayed2Number35.Name = "btnRoulettePlayed2Number35";
            this.btnRoulettePlayed2Number35.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed2Number35.TabIndex = 35;
            this.btnRoulettePlayed2Number35.Tag = "btnHand2Roulette";
            this.btnRoulettePlayed2Number35.Text = "35";
            this.btnRoulettePlayed2Number35.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed2Number34
            // 
            this.btnRoulettePlayed2Number34.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed2Number34.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number34.Location = new System.Drawing.Point(2, 347);
            this.btnRoulettePlayed2Number34.Name = "btnRoulettePlayed2Number34";
            this.btnRoulettePlayed2Number34.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed2Number34.TabIndex = 34;
            this.btnRoulettePlayed2Number34.Tag = "btnHand2Roulette";
            this.btnRoulettePlayed2Number34.Text = "34";
            this.btnRoulettePlayed2Number34.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed2Number33
            // 
            this.btnRoulettePlayed2Number33.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed2Number33.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number33.Location = new System.Drawing.Point(78, 318);
            this.btnRoulettePlayed2Number33.Name = "btnRoulettePlayed2Number33";
            this.btnRoulettePlayed2Number33.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed2Number33.TabIndex = 33;
            this.btnRoulettePlayed2Number33.Tag = "btnHand2Roulette";
            this.btnRoulettePlayed2Number33.Text = "33";
            this.btnRoulettePlayed2Number33.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed2Number32
            // 
            this.btnRoulettePlayed2Number32.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed2Number32.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number32.Location = new System.Drawing.Point(40, 318);
            this.btnRoulettePlayed2Number32.Name = "btnRoulettePlayed2Number32";
            this.btnRoulettePlayed2Number32.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed2Number32.TabIndex = 32;
            this.btnRoulettePlayed2Number32.Tag = "btnHand2Roulette";
            this.btnRoulettePlayed2Number32.Text = "32";
            this.btnRoulettePlayed2Number32.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed2Number31
            // 
            this.btnRoulettePlayed2Number31.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed2Number31.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number31.Location = new System.Drawing.Point(2, 318);
            this.btnRoulettePlayed2Number31.Name = "btnRoulettePlayed2Number31";
            this.btnRoulettePlayed2Number31.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed2Number31.TabIndex = 31;
            this.btnRoulettePlayed2Number31.Tag = "btnHand2Roulette";
            this.btnRoulettePlayed2Number31.Text = "31";
            this.btnRoulettePlayed2Number31.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed2Number30
            // 
            this.btnRoulettePlayed2Number30.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed2Number30.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number30.Location = new System.Drawing.Point(78, 289);
            this.btnRoulettePlayed2Number30.Name = "btnRoulettePlayed2Number30";
            this.btnRoulettePlayed2Number30.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed2Number30.TabIndex = 30;
            this.btnRoulettePlayed2Number30.Tag = "btnHand2Roulette";
            this.btnRoulettePlayed2Number30.Text = "30";
            this.btnRoulettePlayed2Number30.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed2Number29
            // 
            this.btnRoulettePlayed2Number29.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed2Number29.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number29.Location = new System.Drawing.Point(40, 289);
            this.btnRoulettePlayed2Number29.Name = "btnRoulettePlayed2Number29";
            this.btnRoulettePlayed2Number29.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed2Number29.TabIndex = 29;
            this.btnRoulettePlayed2Number29.Tag = "btnHand2Roulette";
            this.btnRoulettePlayed2Number29.Text = "29";
            this.btnRoulettePlayed2Number29.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed2Number28
            // 
            this.btnRoulettePlayed2Number28.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed2Number28.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number28.Location = new System.Drawing.Point(2, 289);
            this.btnRoulettePlayed2Number28.Name = "btnRoulettePlayed2Number28";
            this.btnRoulettePlayed2Number28.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed2Number28.TabIndex = 28;
            this.btnRoulettePlayed2Number28.Tag = "btnHand2Roulette";
            this.btnRoulettePlayed2Number28.Text = "28";
            this.btnRoulettePlayed2Number28.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed2Number27
            // 
            this.btnRoulettePlayed2Number27.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed2Number27.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number27.Location = new System.Drawing.Point(78, 260);
            this.btnRoulettePlayed2Number27.Name = "btnRoulettePlayed2Number27";
            this.btnRoulettePlayed2Number27.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed2Number27.TabIndex = 27;
            this.btnRoulettePlayed2Number27.Tag = "btnHand2Roulette";
            this.btnRoulettePlayed2Number27.Text = "27";
            this.btnRoulettePlayed2Number27.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed2Number26
            // 
            this.btnRoulettePlayed2Number26.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed2Number26.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number26.Location = new System.Drawing.Point(40, 260);
            this.btnRoulettePlayed2Number26.Name = "btnRoulettePlayed2Number26";
            this.btnRoulettePlayed2Number26.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed2Number26.TabIndex = 26;
            this.btnRoulettePlayed2Number26.Tag = "btnHand2Roulette";
            this.btnRoulettePlayed2Number26.Text = "26";
            this.btnRoulettePlayed2Number26.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed2Number25
            // 
            this.btnRoulettePlayed2Number25.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed2Number25.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number25.Location = new System.Drawing.Point(2, 260);
            this.btnRoulettePlayed2Number25.Name = "btnRoulettePlayed2Number25";
            this.btnRoulettePlayed2Number25.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed2Number25.TabIndex = 25;
            this.btnRoulettePlayed2Number25.Tag = "btnHand2Roulette";
            this.btnRoulettePlayed2Number25.Text = "25";
            this.btnRoulettePlayed2Number25.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed2Number24
            // 
            this.btnRoulettePlayed2Number24.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed2Number24.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number24.Location = new System.Drawing.Point(78, 231);
            this.btnRoulettePlayed2Number24.Name = "btnRoulettePlayed2Number24";
            this.btnRoulettePlayed2Number24.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed2Number24.TabIndex = 24;
            this.btnRoulettePlayed2Number24.Tag = "btnHand2Roulette";
            this.btnRoulettePlayed2Number24.Text = "24";
            this.btnRoulettePlayed2Number24.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed2Number23
            // 
            this.btnRoulettePlayed2Number23.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed2Number23.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number23.Location = new System.Drawing.Point(40, 231);
            this.btnRoulettePlayed2Number23.Name = "btnRoulettePlayed2Number23";
            this.btnRoulettePlayed2Number23.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed2Number23.TabIndex = 23;
            this.btnRoulettePlayed2Number23.Tag = "btnHand2Roulette";
            this.btnRoulettePlayed2Number23.Text = "23";
            this.btnRoulettePlayed2Number23.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed2Number22
            // 
            this.btnRoulettePlayed2Number22.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed2Number22.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number22.Location = new System.Drawing.Point(2, 231);
            this.btnRoulettePlayed2Number22.Name = "btnRoulettePlayed2Number22";
            this.btnRoulettePlayed2Number22.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed2Number22.TabIndex = 22;
            this.btnRoulettePlayed2Number22.Tag = "btnHand2Roulette";
            this.btnRoulettePlayed2Number22.Text = "22";
            this.btnRoulettePlayed2Number22.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed2Number21
            // 
            this.btnRoulettePlayed2Number21.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed2Number21.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number21.Location = new System.Drawing.Point(78, 202);
            this.btnRoulettePlayed2Number21.Name = "btnRoulettePlayed2Number21";
            this.btnRoulettePlayed2Number21.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed2Number21.TabIndex = 21;
            this.btnRoulettePlayed2Number21.Tag = "btnHand2Roulette";
            this.btnRoulettePlayed2Number21.Text = "21";
            this.btnRoulettePlayed2Number21.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed2Number20
            // 
            this.btnRoulettePlayed2Number20.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed2Number20.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number20.Location = new System.Drawing.Point(40, 202);
            this.btnRoulettePlayed2Number20.Name = "btnRoulettePlayed2Number20";
            this.btnRoulettePlayed2Number20.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed2Number20.TabIndex = 20;
            this.btnRoulettePlayed2Number20.Tag = "btnHand2Roulette";
            this.btnRoulettePlayed2Number20.Text = "20";
            this.btnRoulettePlayed2Number20.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed2Number19
            // 
            this.btnRoulettePlayed2Number19.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed2Number19.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number19.Location = new System.Drawing.Point(2, 202);
            this.btnRoulettePlayed2Number19.Name = "btnRoulettePlayed2Number19";
            this.btnRoulettePlayed2Number19.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed2Number19.TabIndex = 19;
            this.btnRoulettePlayed2Number19.Tag = "btnHand2Roulette";
            this.btnRoulettePlayed2Number19.Text = "19";
            this.btnRoulettePlayed2Number19.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed2Number18
            // 
            this.btnRoulettePlayed2Number18.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed2Number18.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number18.Location = new System.Drawing.Point(78, 173);
            this.btnRoulettePlayed2Number18.Name = "btnRoulettePlayed2Number18";
            this.btnRoulettePlayed2Number18.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed2Number18.TabIndex = 18;
            this.btnRoulettePlayed2Number18.Tag = "btnHand2Roulette";
            this.btnRoulettePlayed2Number18.Text = "18";
            this.btnRoulettePlayed2Number18.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed2Number17
            // 
            this.btnRoulettePlayed2Number17.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed2Number17.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number17.Location = new System.Drawing.Point(40, 173);
            this.btnRoulettePlayed2Number17.Name = "btnRoulettePlayed2Number17";
            this.btnRoulettePlayed2Number17.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed2Number17.TabIndex = 17;
            this.btnRoulettePlayed2Number17.Tag = "btnHand2Roulette";
            this.btnRoulettePlayed2Number17.Text = "17";
            this.btnRoulettePlayed2Number17.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed2Number16
            // 
            this.btnRoulettePlayed2Number16.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed2Number16.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number16.Location = new System.Drawing.Point(2, 173);
            this.btnRoulettePlayed2Number16.Name = "btnRoulettePlayed2Number16";
            this.btnRoulettePlayed2Number16.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed2Number16.TabIndex = 16;
            this.btnRoulettePlayed2Number16.Tag = "btnHand2Roulette";
            this.btnRoulettePlayed2Number16.Text = "16";
            this.btnRoulettePlayed2Number16.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed2Number15
            // 
            this.btnRoulettePlayed2Number15.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed2Number15.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number15.Location = new System.Drawing.Point(78, 144);
            this.btnRoulettePlayed2Number15.Name = "btnRoulettePlayed2Number15";
            this.btnRoulettePlayed2Number15.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed2Number15.TabIndex = 15;
            this.btnRoulettePlayed2Number15.Tag = "btnHand2Roulette";
            this.btnRoulettePlayed2Number15.Text = "15";
            this.btnRoulettePlayed2Number15.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed2Number14
            // 
            this.btnRoulettePlayed2Number14.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed2Number14.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number14.Location = new System.Drawing.Point(40, 144);
            this.btnRoulettePlayed2Number14.Name = "btnRoulettePlayed2Number14";
            this.btnRoulettePlayed2Number14.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed2Number14.TabIndex = 14;
            this.btnRoulettePlayed2Number14.Tag = "btnHand2Roulette";
            this.btnRoulettePlayed2Number14.Text = "14";
            this.btnRoulettePlayed2Number14.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed2Number13
            // 
            this.btnRoulettePlayed2Number13.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed2Number13.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number13.Location = new System.Drawing.Point(2, 144);
            this.btnRoulettePlayed2Number13.Name = "btnRoulettePlayed2Number13";
            this.btnRoulettePlayed2Number13.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed2Number13.TabIndex = 13;
            this.btnRoulettePlayed2Number13.Tag = "btnHand2Roulette";
            this.btnRoulettePlayed2Number13.Text = "13";
            this.btnRoulettePlayed2Number13.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed2Number12
            // 
            this.btnRoulettePlayed2Number12.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed2Number12.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number12.Location = new System.Drawing.Point(78, 115);
            this.btnRoulettePlayed2Number12.Name = "btnRoulettePlayed2Number12";
            this.btnRoulettePlayed2Number12.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed2Number12.TabIndex = 12;
            this.btnRoulettePlayed2Number12.Tag = "btnHand2Roulette";
            this.btnRoulettePlayed2Number12.Text = "12";
            this.btnRoulettePlayed2Number12.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed2Number11
            // 
            this.btnRoulettePlayed2Number11.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed2Number11.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number11.Location = new System.Drawing.Point(40, 115);
            this.btnRoulettePlayed2Number11.Name = "btnRoulettePlayed2Number11";
            this.btnRoulettePlayed2Number11.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed2Number11.TabIndex = 11;
            this.btnRoulettePlayed2Number11.Tag = "btnHand2Roulette";
            this.btnRoulettePlayed2Number11.Text = "11";
            this.btnRoulettePlayed2Number11.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed2Number10
            // 
            this.btnRoulettePlayed2Number10.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed2Number10.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number10.Location = new System.Drawing.Point(2, 115);
            this.btnRoulettePlayed2Number10.Name = "btnRoulettePlayed2Number10";
            this.btnRoulettePlayed2Number10.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed2Number10.TabIndex = 10;
            this.btnRoulettePlayed2Number10.Tag = "btnHand2Roulette";
            this.btnRoulettePlayed2Number10.Text = "10";
            this.btnRoulettePlayed2Number10.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed2Number9
            // 
            this.btnRoulettePlayed2Number9.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed2Number9.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number9.Location = new System.Drawing.Point(78, 86);
            this.btnRoulettePlayed2Number9.Name = "btnRoulettePlayed2Number9";
            this.btnRoulettePlayed2Number9.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed2Number9.TabIndex = 9;
            this.btnRoulettePlayed2Number9.Tag = "btnHand2Roulette";
            this.btnRoulettePlayed2Number9.Text = "9";
            this.btnRoulettePlayed2Number9.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed2Number8
            // 
            this.btnRoulettePlayed2Number8.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed2Number8.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number8.Location = new System.Drawing.Point(40, 86);
            this.btnRoulettePlayed2Number8.Name = "btnRoulettePlayed2Number8";
            this.btnRoulettePlayed2Number8.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed2Number8.TabIndex = 8;
            this.btnRoulettePlayed2Number8.Tag = "btnHand2Roulette";
            this.btnRoulettePlayed2Number8.Text = "8";
            this.btnRoulettePlayed2Number8.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed2Number7
            // 
            this.btnRoulettePlayed2Number7.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed2Number7.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number7.Location = new System.Drawing.Point(2, 86);
            this.btnRoulettePlayed2Number7.Name = "btnRoulettePlayed2Number7";
            this.btnRoulettePlayed2Number7.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed2Number7.TabIndex = 7;
            this.btnRoulettePlayed2Number7.Tag = "btnHand2Roulette";
            this.btnRoulettePlayed2Number7.Text = "7";
            this.btnRoulettePlayed2Number7.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed2Number6
            // 
            this.btnRoulettePlayed2Number6.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed2Number6.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number6.Location = new System.Drawing.Point(78, 57);
            this.btnRoulettePlayed2Number6.Name = "btnRoulettePlayed2Number6";
            this.btnRoulettePlayed2Number6.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed2Number6.TabIndex = 6;
            this.btnRoulettePlayed2Number6.Tag = "btnHand2Roulette";
            this.btnRoulettePlayed2Number6.Text = "6";
            this.btnRoulettePlayed2Number6.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed2Number5
            // 
            this.btnRoulettePlayed2Number5.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed2Number5.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number5.Location = new System.Drawing.Point(40, 57);
            this.btnRoulettePlayed2Number5.Name = "btnRoulettePlayed2Number5";
            this.btnRoulettePlayed2Number5.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed2Number5.TabIndex = 5;
            this.btnRoulettePlayed2Number5.Tag = "btnHand2Roulette";
            this.btnRoulettePlayed2Number5.Text = "5";
            this.btnRoulettePlayed2Number5.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed2Number4
            // 
            this.btnRoulettePlayed2Number4.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed2Number4.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number4.Location = new System.Drawing.Point(2, 57);
            this.btnRoulettePlayed2Number4.Name = "btnRoulettePlayed2Number4";
            this.btnRoulettePlayed2Number4.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed2Number4.TabIndex = 4;
            this.btnRoulettePlayed2Number4.Tag = "btnHand2Roulette";
            this.btnRoulettePlayed2Number4.Text = "4";
            this.btnRoulettePlayed2Number4.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed2Number3
            // 
            this.btnRoulettePlayed2Number3.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed2Number3.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number3.Location = new System.Drawing.Point(78, 28);
            this.btnRoulettePlayed2Number3.Name = "btnRoulettePlayed2Number3";
            this.btnRoulettePlayed2Number3.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed2Number3.TabIndex = 3;
            this.btnRoulettePlayed2Number3.Tag = "btnHand2Roulette";
            this.btnRoulettePlayed2Number3.Text = "3";
            this.btnRoulettePlayed2Number3.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed2Number2
            // 
            this.btnRoulettePlayed2Number2.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed2Number2.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number2.Location = new System.Drawing.Point(40, 28);
            this.btnRoulettePlayed2Number2.Name = "btnRoulettePlayed2Number2";
            this.btnRoulettePlayed2Number2.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed2Number2.TabIndex = 2;
            this.btnRoulettePlayed2Number2.Tag = "btnHand2Roulette";
            this.btnRoulettePlayed2Number2.Text = "2";
            this.btnRoulettePlayed2Number2.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed2Number1
            // 
            this.btnRoulettePlayed2Number1.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed2Number1.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number1.Location = new System.Drawing.Point(2, 28);
            this.btnRoulettePlayed2Number1.Name = "btnRoulettePlayed2Number1";
            this.btnRoulettePlayed2Number1.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed2Number1.TabIndex = 1;
            this.btnRoulettePlayed2Number1.Tag = "btnHand2Roulette";
            this.btnRoulettePlayed2Number1.Text = "1";
            this.btnRoulettePlayed2Number1.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed2Number0
            // 
            this.btnRoulettePlayed2Number0.BackColor = System.Drawing.Color.SeaGreen;
            this.btnRoulettePlayed2Number0.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed2Number0.Location = new System.Drawing.Point(14, 3);
            this.btnRoulettePlayed2Number0.Name = "btnRoulettePlayed2Number0";
            this.btnRoulettePlayed2Number0.Size = new System.Drawing.Size(85, 23);
            this.btnRoulettePlayed2Number0.TabIndex = 0;
            this.btnRoulettePlayed2Number0.Tag = "btnHand2Roulette";
            this.btnRoulettePlayed2Number0.Text = "0";
            this.btnRoulettePlayed2Number0.UseVisualStyleBackColor = false;
            // 
            // panelRoulettePlayed1
            // 
            this.panelRoulettePlayed1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelRoulettePlayed1.Controls.Add(this.btnRoulettePlayed1Number36);
            this.panelRoulettePlayed1.Controls.Add(this.btnRoulettePlayed1Number35);
            this.panelRoulettePlayed1.Controls.Add(this.btnRoulettePlayed1Number34);
            this.panelRoulettePlayed1.Controls.Add(this.btnRoulettePlayed1Number33);
            this.panelRoulettePlayed1.Controls.Add(this.btnRoulettePlayed1Number32);
            this.panelRoulettePlayed1.Controls.Add(this.btnRoulettePlayed1Number31);
            this.panelRoulettePlayed1.Controls.Add(this.btnRoulettePlayed1Number30);
            this.panelRoulettePlayed1.Controls.Add(this.btnRoulettePlayed1Number29);
            this.panelRoulettePlayed1.Controls.Add(this.btnRoulettePlayed1Number28);
            this.panelRoulettePlayed1.Controls.Add(this.btnRoulettePlayed1Number27);
            this.panelRoulettePlayed1.Controls.Add(this.btnRoulettePlayed1Number26);
            this.panelRoulettePlayed1.Controls.Add(this.btnRoulettePlayed1Number25);
            this.panelRoulettePlayed1.Controls.Add(this.btnRoulettePlayed1Number24);
            this.panelRoulettePlayed1.Controls.Add(this.btnRoulettePlayed1Number23);
            this.panelRoulettePlayed1.Controls.Add(this.btnRoulettePlayed1Number22);
            this.panelRoulettePlayed1.Controls.Add(this.btnRoulettePlayed1Number21);
            this.panelRoulettePlayed1.Controls.Add(this.btnRoulettePlayed1Number20);
            this.panelRoulettePlayed1.Controls.Add(this.btnRoulettePlayed1Number19);
            this.panelRoulettePlayed1.Controls.Add(this.btnRoulettePlayed1Number18);
            this.panelRoulettePlayed1.Controls.Add(this.btnRoulettePlayed1Number17);
            this.panelRoulettePlayed1.Controls.Add(this.btnRoulettePlayed1Number16);
            this.panelRoulettePlayed1.Controls.Add(this.btnRoulettePlayed1Number15);
            this.panelRoulettePlayed1.Controls.Add(this.btnRoulettePlayed1Number14);
            this.panelRoulettePlayed1.Controls.Add(this.btnRoulettePlayed1Number13);
            this.panelRoulettePlayed1.Controls.Add(this.btnRoulettePlayed1Number12);
            this.panelRoulettePlayed1.Controls.Add(this.btnRoulettePlayed1Number11);
            this.panelRoulettePlayed1.Controls.Add(this.btnRoulettePlayed1Number10);
            this.panelRoulettePlayed1.Controls.Add(this.btnRoulettePlayed1Number9);
            this.panelRoulettePlayed1.Controls.Add(this.btnRoulettePlayed1Number8);
            this.panelRoulettePlayed1.Controls.Add(this.btnRoulettePlayed1Number7);
            this.panelRoulettePlayed1.Controls.Add(this.btnRoulettePlayed1Number6);
            this.panelRoulettePlayed1.Controls.Add(this.btnRoulettePlayed1Number5);
            this.panelRoulettePlayed1.Controls.Add(this.btnRoulettePlayed1Number4);
            this.panelRoulettePlayed1.Controls.Add(this.btnRoulettePlayed1Number3);
            this.panelRoulettePlayed1.Controls.Add(this.btnRoulettePlayed1Number2);
            this.panelRoulettePlayed1.Controls.Add(this.btnRoulettePlayed1Number1);
            this.panelRoulettePlayed1.Controls.Add(this.btnRoulettePlayed1Number0);
            this.panelRoulettePlayed1.Location = new System.Drawing.Point(12, 130);
            this.panelRoulettePlayed1.Name = "panelRoulettePlayed1";
            this.panelRoulettePlayed1.Size = new System.Drawing.Size(113, 378);
            this.panelRoulettePlayed1.TabIndex = 3;
            // 
            // btnRoulettePlayed1Number36
            // 
            this.btnRoulettePlayed1Number36.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed1Number36.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number36.Location = new System.Drawing.Point(78, 347);
            this.btnRoulettePlayed1Number36.Name = "btnRoulettePlayed1Number36";
            this.btnRoulettePlayed1Number36.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed1Number36.TabIndex = 36;
            this.btnRoulettePlayed1Number36.Tag = "btnHand1Roulette";
            this.btnRoulettePlayed1Number36.Text = "36";
            this.btnRoulettePlayed1Number36.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed1Number35
            // 
            this.btnRoulettePlayed1Number35.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed1Number35.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number35.Location = new System.Drawing.Point(40, 347);
            this.btnRoulettePlayed1Number35.Name = "btnRoulettePlayed1Number35";
            this.btnRoulettePlayed1Number35.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed1Number35.TabIndex = 35;
            this.btnRoulettePlayed1Number35.Tag = "btnHand1Roulette";
            this.btnRoulettePlayed1Number35.Text = "35";
            this.btnRoulettePlayed1Number35.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed1Number34
            // 
            this.btnRoulettePlayed1Number34.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed1Number34.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number34.Location = new System.Drawing.Point(2, 347);
            this.btnRoulettePlayed1Number34.Name = "btnRoulettePlayed1Number34";
            this.btnRoulettePlayed1Number34.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed1Number34.TabIndex = 34;
            this.btnRoulettePlayed1Number34.Tag = "btnHand1Roulette";
            this.btnRoulettePlayed1Number34.Text = "34";
            this.btnRoulettePlayed1Number34.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed1Number33
            // 
            this.btnRoulettePlayed1Number33.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed1Number33.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number33.Location = new System.Drawing.Point(78, 318);
            this.btnRoulettePlayed1Number33.Name = "btnRoulettePlayed1Number33";
            this.btnRoulettePlayed1Number33.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed1Number33.TabIndex = 33;
            this.btnRoulettePlayed1Number33.Tag = "btnHand1Roulette";
            this.btnRoulettePlayed1Number33.Text = "33";
            this.btnRoulettePlayed1Number33.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed1Number32
            // 
            this.btnRoulettePlayed1Number32.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed1Number32.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number32.Location = new System.Drawing.Point(40, 318);
            this.btnRoulettePlayed1Number32.Name = "btnRoulettePlayed1Number32";
            this.btnRoulettePlayed1Number32.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed1Number32.TabIndex = 32;
            this.btnRoulettePlayed1Number32.Tag = "btnHand1Roulette";
            this.btnRoulettePlayed1Number32.Text = "32";
            this.btnRoulettePlayed1Number32.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed1Number31
            // 
            this.btnRoulettePlayed1Number31.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed1Number31.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number31.Location = new System.Drawing.Point(2, 318);
            this.btnRoulettePlayed1Number31.Name = "btnRoulettePlayed1Number31";
            this.btnRoulettePlayed1Number31.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed1Number31.TabIndex = 31;
            this.btnRoulettePlayed1Number31.Tag = "btnHand1Roulette";
            this.btnRoulettePlayed1Number31.Text = "31";
            this.btnRoulettePlayed1Number31.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed1Number30
            // 
            this.btnRoulettePlayed1Number30.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed1Number30.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number30.Location = new System.Drawing.Point(78, 289);
            this.btnRoulettePlayed1Number30.Name = "btnRoulettePlayed1Number30";
            this.btnRoulettePlayed1Number30.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed1Number30.TabIndex = 30;
            this.btnRoulettePlayed1Number30.Tag = "btnHand1Roulette";
            this.btnRoulettePlayed1Number30.Text = "30";
            this.btnRoulettePlayed1Number30.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed1Number29
            // 
            this.btnRoulettePlayed1Number29.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed1Number29.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number29.Location = new System.Drawing.Point(40, 289);
            this.btnRoulettePlayed1Number29.Name = "btnRoulettePlayed1Number29";
            this.btnRoulettePlayed1Number29.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed1Number29.TabIndex = 29;
            this.btnRoulettePlayed1Number29.Tag = "btnHand1Roulette";
            this.btnRoulettePlayed1Number29.Text = "29";
            this.btnRoulettePlayed1Number29.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed1Number28
            // 
            this.btnRoulettePlayed1Number28.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed1Number28.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number28.Location = new System.Drawing.Point(2, 289);
            this.btnRoulettePlayed1Number28.Name = "btnRoulettePlayed1Number28";
            this.btnRoulettePlayed1Number28.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed1Number28.TabIndex = 28;
            this.btnRoulettePlayed1Number28.Tag = "btnHand1Roulette";
            this.btnRoulettePlayed1Number28.Text = "28";
            this.btnRoulettePlayed1Number28.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed1Number27
            // 
            this.btnRoulettePlayed1Number27.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed1Number27.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number27.Location = new System.Drawing.Point(78, 260);
            this.btnRoulettePlayed1Number27.Name = "btnRoulettePlayed1Number27";
            this.btnRoulettePlayed1Number27.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed1Number27.TabIndex = 27;
            this.btnRoulettePlayed1Number27.Tag = "btnHand1Roulette";
            this.btnRoulettePlayed1Number27.Text = "27";
            this.btnRoulettePlayed1Number27.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed1Number26
            // 
            this.btnRoulettePlayed1Number26.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed1Number26.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number26.Location = new System.Drawing.Point(40, 260);
            this.btnRoulettePlayed1Number26.Name = "btnRoulettePlayed1Number26";
            this.btnRoulettePlayed1Number26.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed1Number26.TabIndex = 26;
            this.btnRoulettePlayed1Number26.Tag = "btnHand1Roulette";
            this.btnRoulettePlayed1Number26.Text = "26";
            this.btnRoulettePlayed1Number26.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed1Number25
            // 
            this.btnRoulettePlayed1Number25.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed1Number25.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number25.Location = new System.Drawing.Point(2, 260);
            this.btnRoulettePlayed1Number25.Name = "btnRoulettePlayed1Number25";
            this.btnRoulettePlayed1Number25.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed1Number25.TabIndex = 25;
            this.btnRoulettePlayed1Number25.Tag = "btnHand1Roulette";
            this.btnRoulettePlayed1Number25.Text = "25";
            this.btnRoulettePlayed1Number25.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed1Number24
            // 
            this.btnRoulettePlayed1Number24.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed1Number24.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number24.Location = new System.Drawing.Point(78, 231);
            this.btnRoulettePlayed1Number24.Name = "btnRoulettePlayed1Number24";
            this.btnRoulettePlayed1Number24.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed1Number24.TabIndex = 24;
            this.btnRoulettePlayed1Number24.Tag = "btnHand1Roulette";
            this.btnRoulettePlayed1Number24.Text = "24";
            this.btnRoulettePlayed1Number24.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed1Number23
            // 
            this.btnRoulettePlayed1Number23.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed1Number23.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number23.Location = new System.Drawing.Point(40, 231);
            this.btnRoulettePlayed1Number23.Name = "btnRoulettePlayed1Number23";
            this.btnRoulettePlayed1Number23.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed1Number23.TabIndex = 23;
            this.btnRoulettePlayed1Number23.Tag = "btnHand1Roulette";
            this.btnRoulettePlayed1Number23.Text = "23";
            this.btnRoulettePlayed1Number23.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed1Number22
            // 
            this.btnRoulettePlayed1Number22.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed1Number22.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number22.Location = new System.Drawing.Point(2, 231);
            this.btnRoulettePlayed1Number22.Name = "btnRoulettePlayed1Number22";
            this.btnRoulettePlayed1Number22.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed1Number22.TabIndex = 22;
            this.btnRoulettePlayed1Number22.Tag = "btnHand1Roulette";
            this.btnRoulettePlayed1Number22.Text = "22";
            this.btnRoulettePlayed1Number22.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed1Number21
            // 
            this.btnRoulettePlayed1Number21.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed1Number21.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number21.Location = new System.Drawing.Point(78, 202);
            this.btnRoulettePlayed1Number21.Name = "btnRoulettePlayed1Number21";
            this.btnRoulettePlayed1Number21.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed1Number21.TabIndex = 21;
            this.btnRoulettePlayed1Number21.Tag = "btnHand1Roulette";
            this.btnRoulettePlayed1Number21.Text = "21";
            this.btnRoulettePlayed1Number21.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed1Number20
            // 
            this.btnRoulettePlayed1Number20.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed1Number20.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number20.Location = new System.Drawing.Point(40, 202);
            this.btnRoulettePlayed1Number20.Name = "btnRoulettePlayed1Number20";
            this.btnRoulettePlayed1Number20.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed1Number20.TabIndex = 20;
            this.btnRoulettePlayed1Number20.Tag = "btnHand1Roulette";
            this.btnRoulettePlayed1Number20.Text = "20";
            this.btnRoulettePlayed1Number20.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed1Number19
            // 
            this.btnRoulettePlayed1Number19.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed1Number19.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number19.Location = new System.Drawing.Point(2, 202);
            this.btnRoulettePlayed1Number19.Name = "btnRoulettePlayed1Number19";
            this.btnRoulettePlayed1Number19.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed1Number19.TabIndex = 19;
            this.btnRoulettePlayed1Number19.Tag = "btnHand1Roulette";
            this.btnRoulettePlayed1Number19.Text = "19";
            this.btnRoulettePlayed1Number19.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed1Number18
            // 
            this.btnRoulettePlayed1Number18.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed1Number18.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number18.Location = new System.Drawing.Point(78, 173);
            this.btnRoulettePlayed1Number18.Name = "btnRoulettePlayed1Number18";
            this.btnRoulettePlayed1Number18.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed1Number18.TabIndex = 18;
            this.btnRoulettePlayed1Number18.Tag = "btnHand1Roulette";
            this.btnRoulettePlayed1Number18.Text = "18";
            this.btnRoulettePlayed1Number18.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed1Number17
            // 
            this.btnRoulettePlayed1Number17.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed1Number17.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number17.Location = new System.Drawing.Point(40, 173);
            this.btnRoulettePlayed1Number17.Name = "btnRoulettePlayed1Number17";
            this.btnRoulettePlayed1Number17.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed1Number17.TabIndex = 17;
            this.btnRoulettePlayed1Number17.Tag = "btnHand1Roulette";
            this.btnRoulettePlayed1Number17.Text = "17";
            this.btnRoulettePlayed1Number17.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed1Number16
            // 
            this.btnRoulettePlayed1Number16.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed1Number16.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number16.Location = new System.Drawing.Point(2, 173);
            this.btnRoulettePlayed1Number16.Name = "btnRoulettePlayed1Number16";
            this.btnRoulettePlayed1Number16.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed1Number16.TabIndex = 16;
            this.btnRoulettePlayed1Number16.Tag = "btnHand1Roulette";
            this.btnRoulettePlayed1Number16.Text = "16";
            this.btnRoulettePlayed1Number16.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed1Number15
            // 
            this.btnRoulettePlayed1Number15.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed1Number15.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number15.Location = new System.Drawing.Point(78, 144);
            this.btnRoulettePlayed1Number15.Name = "btnRoulettePlayed1Number15";
            this.btnRoulettePlayed1Number15.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed1Number15.TabIndex = 15;
            this.btnRoulettePlayed1Number15.Tag = "btnHand1Roulette";
            this.btnRoulettePlayed1Number15.Text = "15";
            this.btnRoulettePlayed1Number15.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed1Number14
            // 
            this.btnRoulettePlayed1Number14.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed1Number14.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number14.Location = new System.Drawing.Point(40, 144);
            this.btnRoulettePlayed1Number14.Name = "btnRoulettePlayed1Number14";
            this.btnRoulettePlayed1Number14.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed1Number14.TabIndex = 14;
            this.btnRoulettePlayed1Number14.Tag = "btnHand1Roulette";
            this.btnRoulettePlayed1Number14.Text = "14";
            this.btnRoulettePlayed1Number14.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed1Number13
            // 
            this.btnRoulettePlayed1Number13.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed1Number13.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number13.Location = new System.Drawing.Point(2, 144);
            this.btnRoulettePlayed1Number13.Name = "btnRoulettePlayed1Number13";
            this.btnRoulettePlayed1Number13.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed1Number13.TabIndex = 13;
            this.btnRoulettePlayed1Number13.Tag = "btnHand1Roulette";
            this.btnRoulettePlayed1Number13.Text = "13";
            this.btnRoulettePlayed1Number13.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed1Number12
            // 
            this.btnRoulettePlayed1Number12.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed1Number12.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number12.Location = new System.Drawing.Point(78, 115);
            this.btnRoulettePlayed1Number12.Name = "btnRoulettePlayed1Number12";
            this.btnRoulettePlayed1Number12.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed1Number12.TabIndex = 12;
            this.btnRoulettePlayed1Number12.Tag = "btnHand1Roulette";
            this.btnRoulettePlayed1Number12.Text = "12";
            this.btnRoulettePlayed1Number12.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed1Number11
            // 
            this.btnRoulettePlayed1Number11.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed1Number11.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number11.Location = new System.Drawing.Point(40, 115);
            this.btnRoulettePlayed1Number11.Name = "btnRoulettePlayed1Number11";
            this.btnRoulettePlayed1Number11.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed1Number11.TabIndex = 11;
            this.btnRoulettePlayed1Number11.Tag = "btnHand1Roulette";
            this.btnRoulettePlayed1Number11.Text = "11";
            this.btnRoulettePlayed1Number11.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed1Number10
            // 
            this.btnRoulettePlayed1Number10.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed1Number10.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number10.Location = new System.Drawing.Point(2, 115);
            this.btnRoulettePlayed1Number10.Name = "btnRoulettePlayed1Number10";
            this.btnRoulettePlayed1Number10.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed1Number10.TabIndex = 10;
            this.btnRoulettePlayed1Number10.Tag = "btnHand1Roulette";
            this.btnRoulettePlayed1Number10.Text = "10";
            this.btnRoulettePlayed1Number10.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed1Number9
            // 
            this.btnRoulettePlayed1Number9.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed1Number9.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number9.Location = new System.Drawing.Point(78, 86);
            this.btnRoulettePlayed1Number9.Name = "btnRoulettePlayed1Number9";
            this.btnRoulettePlayed1Number9.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed1Number9.TabIndex = 9;
            this.btnRoulettePlayed1Number9.Tag = "btnHand1Roulette";
            this.btnRoulettePlayed1Number9.Text = "9";
            this.btnRoulettePlayed1Number9.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed1Number8
            // 
            this.btnRoulettePlayed1Number8.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed1Number8.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number8.Location = new System.Drawing.Point(40, 86);
            this.btnRoulettePlayed1Number8.Name = "btnRoulettePlayed1Number8";
            this.btnRoulettePlayed1Number8.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed1Number8.TabIndex = 8;
            this.btnRoulettePlayed1Number8.Tag = "btnHand1Roulette";
            this.btnRoulettePlayed1Number8.Text = "8";
            this.btnRoulettePlayed1Number8.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed1Number7
            // 
            this.btnRoulettePlayed1Number7.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed1Number7.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number7.Location = new System.Drawing.Point(2, 86);
            this.btnRoulettePlayed1Number7.Name = "btnRoulettePlayed1Number7";
            this.btnRoulettePlayed1Number7.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed1Number7.TabIndex = 7;
            this.btnRoulettePlayed1Number7.Tag = "btnHand1Roulette";
            this.btnRoulettePlayed1Number7.Text = "7";
            this.btnRoulettePlayed1Number7.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed1Number6
            // 
            this.btnRoulettePlayed1Number6.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed1Number6.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number6.Location = new System.Drawing.Point(78, 57);
            this.btnRoulettePlayed1Number6.Name = "btnRoulettePlayed1Number6";
            this.btnRoulettePlayed1Number6.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed1Number6.TabIndex = 6;
            this.btnRoulettePlayed1Number6.Tag = "btnHand1Roulette";
            this.btnRoulettePlayed1Number6.Text = "6";
            this.btnRoulettePlayed1Number6.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed1Number5
            // 
            this.btnRoulettePlayed1Number5.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed1Number5.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number5.Location = new System.Drawing.Point(40, 57);
            this.btnRoulettePlayed1Number5.Name = "btnRoulettePlayed1Number5";
            this.btnRoulettePlayed1Number5.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed1Number5.TabIndex = 5;
            this.btnRoulettePlayed1Number5.Tag = "btnHand1Roulette";
            this.btnRoulettePlayed1Number5.Text = "5";
            this.btnRoulettePlayed1Number5.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed1Number4
            // 
            this.btnRoulettePlayed1Number4.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed1Number4.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number4.Location = new System.Drawing.Point(2, 57);
            this.btnRoulettePlayed1Number4.Name = "btnRoulettePlayed1Number4";
            this.btnRoulettePlayed1Number4.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed1Number4.TabIndex = 4;
            this.btnRoulettePlayed1Number4.Tag = "btnHand1Roulette";
            this.btnRoulettePlayed1Number4.Text = "4";
            this.btnRoulettePlayed1Number4.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed1Number3
            // 
            this.btnRoulettePlayed1Number3.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed1Number3.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number3.Location = new System.Drawing.Point(78, 28);
            this.btnRoulettePlayed1Number3.Name = "btnRoulettePlayed1Number3";
            this.btnRoulettePlayed1Number3.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed1Number3.TabIndex = 3;
            this.btnRoulettePlayed1Number3.Tag = "btnHand1Roulette";
            this.btnRoulettePlayed1Number3.Text = "3";
            this.btnRoulettePlayed1Number3.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed1Number2
            // 
            this.btnRoulettePlayed1Number2.BackColor = System.Drawing.Color.Black;
            this.btnRoulettePlayed1Number2.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number2.Location = new System.Drawing.Point(40, 28);
            this.btnRoulettePlayed1Number2.Name = "btnRoulettePlayed1Number2";
            this.btnRoulettePlayed1Number2.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed1Number2.TabIndex = 2;
            this.btnRoulettePlayed1Number2.Tag = "btnHand1Roulette";
            this.btnRoulettePlayed1Number2.Text = "2";
            this.btnRoulettePlayed1Number2.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed1Number1
            // 
            this.btnRoulettePlayed1Number1.BackColor = System.Drawing.Color.Firebrick;
            this.btnRoulettePlayed1Number1.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number1.Location = new System.Drawing.Point(2, 28);
            this.btnRoulettePlayed1Number1.Name = "btnRoulettePlayed1Number1";
            this.btnRoulettePlayed1Number1.Size = new System.Drawing.Size(32, 23);
            this.btnRoulettePlayed1Number1.TabIndex = 1;
            this.btnRoulettePlayed1Number1.Tag = "btnHand1Roulette";
            this.btnRoulettePlayed1Number1.Text = "1";
            this.btnRoulettePlayed1Number1.UseVisualStyleBackColor = false;
            // 
            // btnRoulettePlayed1Number0
            // 
            this.btnRoulettePlayed1Number0.BackColor = System.Drawing.Color.SeaGreen;
            this.btnRoulettePlayed1Number0.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnRoulettePlayed1Number0.Location = new System.Drawing.Point(14, 3);
            this.btnRoulettePlayed1Number0.Name = "btnRoulettePlayed1Number0";
            this.btnRoulettePlayed1Number0.Size = new System.Drawing.Size(85, 23);
            this.btnRoulettePlayed1Number0.TabIndex = 0;
            this.btnRoulettePlayed1Number0.Tag = "btnHand1Roulette";
            this.btnRoulettePlayed1Number0.Text = "0";
            this.btnRoulettePlayed1Number0.UseVisualStyleBackColor = false;
            // 
            // lblRoulettePlayed3
            // 
            this.lblRoulettePlayed3.AutoSize = true;
            this.lblRoulettePlayed3.Location = new System.Drawing.Point(323, 107);
            this.lblRoulettePlayed3.Name = "lblRoulettePlayed3";
            this.lblRoulettePlayed3.Size = new System.Drawing.Size(96, 13);
            this.lblRoulettePlayed3.TabIndex = 2;
            this.lblRoulettePlayed3.Text = "Numeri Giocata #3";
            // 
            // lblRoulettePlayed2
            // 
            this.lblRoulettePlayed2.AutoSize = true;
            this.lblRoulettePlayed2.Location = new System.Drawing.Point(173, 107);
            this.lblRoulettePlayed2.Name = "lblRoulettePlayed2";
            this.lblRoulettePlayed2.Size = new System.Drawing.Size(96, 13);
            this.lblRoulettePlayed2.TabIndex = 1;
            this.lblRoulettePlayed2.Text = "Numeri Giocata #2";
            // 
            // lblRoulettePlayed1
            // 
            this.lblRoulettePlayed1.AutoSize = true;
            this.lblRoulettePlayed1.Location = new System.Drawing.Point(23, 107);
            this.lblRoulettePlayed1.Name = "lblRoulettePlayed1";
            this.lblRoulettePlayed1.Size = new System.Drawing.Size(96, 13);
            this.lblRoulettePlayed1.TabIndex = 0;
            this.lblRoulettePlayed1.Text = "Numeri Giocata #1";
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.txtComandiRicevuti);
            this.tabPage1.Controls.Add(this.checkSkipPostSculping);
            this.tabPage1.Controls.Add(this.checkPragmaticFilter);
            this.tabPage1.Controls.Add(this.buttonBet);
            this.tabPage1.Controls.Add(this.timeElapsedValueToChange);
            this.tabPage1.Controls.Add(this.labelTimeElapsed);
            this.tabPage1.Controls.Add(this.textAreaPuntare);
            this.tabPage1.Controls.Add(this.textAreaPlayer);
            this.tabPage1.Controls.Add(this.textAreaBench);
            this.tabPage1.Controls.Add(this.textAreaWin);
            this.tabPage1.Controls.Add(this.textAreaTie);
            this.tabPage1.Controls.Add(this.label7);
            this.tabPage1.Controls.Add(this.numberDeckValueToChange);
            this.tabPage1.Controls.Add(this.labelNumberDeck);
            this.tabPage1.Controls.Add(this.typeGamenInfobtn);
            this.tabPage1.Controls.Add(this.groupBox1);
            this.tabPage1.Controls.Add(this.label6);
            this.tabPage1.Controls.Add(this.labelEnvironment);
            this.tabPage1.Controls.Add(this.textAreaInfoBtn);
            this.tabPage1.Controls.Add(this.labelTextAreaGiocatore);
            this.tabPage1.Controls.Add(this.labelTextAreaBanco);
            this.tabPage1.Controls.Add(this.labelTextAreaVince);
            this.tabPage1.Controls.Add(this.labelTextAreaTie);
            this.tabPage1.Controls.Add(this.numberChangeEndDeck);
            this.tabPage1.Controls.Add(this.labelChangeNumberEndDeck);
            this.tabPage1.Controls.Add(this.labelZoomPerc);
            this.tabPage1.Controls.Add(this.autoBalanceLabel);
            this.tabPage1.Controls.Add(this.checkBoxAutoSaldo);
            this.tabPage1.Controls.Add(this.mainareehelpbtn);
            this.tabPage1.Controls.Add(this.stopwinlossinfobtn);
            this.tabPage1.Controls.Add(this.balanceinfobtn);
            this.tabPage1.Controls.Add(this.cardcolorsinfobtn);
            this.tabPage1.Controls.Add(this.martingalaHelpBtn);
            this.tabPage1.Controls.Add(this.mainhelpbtn);
            this.tabPage1.Controls.Add(this.customFichesPanel);
            this.tabPage1.Controls.Add(this.customFichesEditBtn);
            this.tabPage1.Controls.Add(this.lblNameConfig);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.btnSaveConfig);
            this.tabPage1.Controls.Add(this.labelRiconoscimentoFiches);
            this.tabPage1.Controls.Add(this.labelStatus);
            this.tabPage1.Controls.Add(this.balanceTotalValueText);
            this.tabPage1.Controls.Add(this.balanceStartValue);
            this.tabPage1.Controls.Add(this.labelStartBalance);
            this.tabPage1.Controls.Add(this.checkSafeWin);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.txtZoomMonitor);
            this.tabPage1.Controls.Add(this.labelNumberProfittoSculping);
            this.tabPage1.Controls.Add(this.labelProfittoSculping);
            this.tabPage1.Controls.Add(this.labelNumerLose);
            this.tabPage1.Controls.Add(this.labelNumberProfittoGlobale);
            this.tabPage1.Controls.Add(this.labelNumerWin);
            this.tabPage1.Controls.Add(this.labelProfittoGlobale);
            this.tabPage1.Controls.Add(this.labelVinte);
            this.tabPage1.Controls.Add(this.panelMartingala);
            this.tabPage1.Controls.Add(this.labelPerse);
            this.tabPage1.Controls.Add(this.labelMartingala);
            this.tabPage1.Controls.Add(this.labelPerc);
            this.tabPage1.Controls.Add(this.safeWinPerc);
            this.tabPage1.Controls.Add(this.btnAddMartingala);
            this.tabPage1.Controls.Add(this.labelSafeWin);
            this.tabPage1.Controls.Add(this.buttonLoadConfig);
            this.tabPage1.Controls.Add(this.stopLossValue);
            this.tabPage1.Controls.Add(this.labelStopWinGlob);
            this.tabPage1.Controls.Add(this.labelStopLoss);
            this.tabPage1.Controls.Add(this.globalStopWinValue);
            this.tabPage1.Controls.Add(this.buttonStart);
            this.tabPage1.Controls.Add(this.groupBoxMode);
            this.tabPage1.Controls.Add(this.labelMode);
            this.tabPage1.Controls.Add(this.labelRiconoscimentoArea);
            this.tabPage1.Controls.Add(this.groupBoxStartColor);
            this.tabPage1.Controls.Add(this.stopWinValue);
            this.tabPage1.Controls.Add(this.labelColorStart);
            this.tabPage1.Controls.Add(this.labelStopWin);
            this.tabPage1.Controls.Add(this.buttonFish250);
            this.tabPage1.Controls.Add(this.buttonRed);
            this.tabPage1.Controls.Add(this.buttonFish100);
            this.tabPage1.Controls.Add(this.buttonBlu);
            this.tabPage1.Controls.Add(this.buttonFish1);
            this.tabPage1.Controls.Add(this.buttonDoubling);
            this.tabPage1.Controls.Add(this.buttonBalanceArea);
            this.tabPage1.Controls.Add(this.buttonFish500);
            this.tabPage1.Controls.Add(this.buttonAreaVincita);
            this.tabPage1.Controls.Add(this.buttonDeckArea);
            this.tabPage1.Controls.Add(this.buttonAreaCentrale);
            this.tabPage1.Controls.Add(this.buttonFish5);
            this.tabPage1.Controls.Add(this.buttonFish25);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1086, 642);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Configurazione Baccarat";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // checkSkipPostSculping
            // 
            this.checkSkipPostSculping.AutoSize = true;
            this.checkSkipPostSculping.Location = new System.Drawing.Point(470, 153);
            this.checkSkipPostSculping.Margin = new System.Windows.Forms.Padding(2);
            this.checkSkipPostSculping.Name = "checkSkipPostSculping";
            this.checkSkipPostSculping.Size = new System.Drawing.Size(124, 17);
            this.checkSkipPostSculping.TabIndex = 117;
            this.checkSkipPostSculping.Tag = "controlInput";
            this.checkSkipPostSculping.Text = "Skip Pause Sculping";
            this.checkSkipPostSculping.UseVisualStyleBackColor = true;
            // 
            // checkPragmaticFilter
            // 
            this.checkPragmaticFilter.AutoSize = true;
            this.checkPragmaticFilter.Location = new System.Drawing.Point(488, 312);
            this.checkPragmaticFilter.Margin = new System.Windows.Forms.Padding(2);
            this.checkPragmaticFilter.Name = "checkPragmaticFilter";
            this.checkPragmaticFilter.Size = new System.Drawing.Size(133, 17);
            this.checkPragmaticFilter.TabIndex = 116;
            this.checkPragmaticFilter.Tag = "controlInput";
            this.checkPragmaticFilter.Text = "Applica filtro Pragmatic";
            this.checkPragmaticFilter.UseVisualStyleBackColor = true;
            // 
            // buttonBet
            // 
            this.buttonBet.Location = new System.Drawing.Point(371, 308);
            this.buttonBet.Margin = new System.Windows.Forms.Padding(2);
            this.buttonBet.Name = "buttonBet";
            this.buttonBet.Size = new System.Drawing.Size(112, 23);
            this.buttonBet.TabIndex = 115;
            this.buttonBet.Tag = "controlInput";
            this.buttonBet.Text = "Area Puntare";
            this.buttonBet.UseVisualStyleBackColor = true;
            this.buttonBet.Click += new System.EventHandler(this.buttonBet_Click);
            // 
            // timeElapsedValueToChange
            // 
            this.timeElapsedValueToChange.AutoSize = true;
            this.timeElapsedValueToChange.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.timeElapsedValueToChange.Location = new System.Drawing.Point(592, 64);
            this.timeElapsedValueToChange.Name = "timeElapsedValueToChange";
            this.timeElapsedValueToChange.Size = new System.Drawing.Size(63, 16);
            this.timeElapsedValueToChange.TabIndex = 114;
            this.timeElapsedValueToChange.Text = "00:00:00";
            // 
            // labelTimeElapsed
            // 
            this.labelTimeElapsed.AutoSize = true;
            this.labelTimeElapsed.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTimeElapsed.Location = new System.Drawing.Point(476, 64);
            this.labelTimeElapsed.Name = "labelTimeElapsed";
            this.labelTimeElapsed.Size = new System.Drawing.Size(119, 16);
            this.labelTimeElapsed.TabIndex = 113;
            this.labelTimeElapsed.Text = "Tempo Trascorso:";
            // 
            // textAreaPuntare
            // 
            this.textAreaPuntare.Location = new System.Drawing.Point(744, 310);
            this.textAreaPuntare.Name = "textAreaPuntare";
            this.textAreaPuntare.Size = new System.Drawing.Size(99, 20);
            this.textAreaPuntare.TabIndex = 112;
            // 
            // textAreaPlayer
            // 
            this.textAreaPlayer.Location = new System.Drawing.Point(964, 360);
            this.textAreaPlayer.Name = "textAreaPlayer";
            this.textAreaPlayer.Size = new System.Drawing.Size(99, 20);
            this.textAreaPlayer.TabIndex = 102;
            // 
            // textAreaBench
            // 
            this.textAreaBench.Location = new System.Drawing.Point(744, 360);
            this.textAreaBench.Name = "textAreaBench";
            this.textAreaBench.Size = new System.Drawing.Size(99, 20);
            this.textAreaBench.TabIndex = 100;
            // 
            // textAreaWin
            // 
            this.textAreaWin.Location = new System.Drawing.Point(889, 310);
            this.textAreaWin.Name = "textAreaWin";
            this.textAreaWin.Size = new System.Drawing.Size(99, 20);
            this.textAreaWin.TabIndex = 98;
            // 
            // textAreaTie
            // 
            this.textAreaTie.Location = new System.Drawing.Point(854, 360);
            this.textAreaTie.Name = "textAreaTie";
            this.textAreaTie.Size = new System.Drawing.Size(99, 20);
            this.textAreaTie.TabIndex = 96;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(744, 294);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(99, 13);
            this.label7.TabIndex = 111;
            this.label7.Text = "Testo \"PUNTARE\"";
            // 
            // numberDeckValueToChange
            // 
            this.numberDeckValueToChange.AutoSize = true;
            this.numberDeckValueToChange.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numberDeckValueToChange.Location = new System.Drawing.Point(404, 64);
            this.numberDeckValueToChange.Name = "numberDeckValueToChange";
            this.numberDeckValueToChange.Size = new System.Drawing.Size(15, 16);
            this.numberDeckValueToChange.TabIndex = 110;
            this.numberDeckValueToChange.Text = "0";
            // 
            // labelNumberDeck
            // 
            this.labelNumberDeck.AutoSize = true;
            this.labelNumberDeck.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelNumberDeck.Location = new System.Drawing.Point(302, 64);
            this.labelNumberDeck.Name = "labelNumberDeck";
            this.labelNumberDeck.Size = new System.Drawing.Size(103, 16);
            this.labelNumberDeck.TabIndex = 109;
            this.labelNumberDeck.Text = "Numero Mazzo: ";
            // 
            // typeGamenInfobtn
            // 
            this.typeGamenInfobtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.typeGamenInfobtn.ForeColor = System.Drawing.SystemColors.Highlight;
            this.typeGamenInfobtn.Location = new System.Drawing.Point(959, 424);
            this.typeGamenInfobtn.Name = "typeGamenInfobtn";
            this.typeGamenInfobtn.Size = new System.Drawing.Size(37, 23);
            this.typeGamenInfobtn.TabIndex = 108;
            this.typeGamenInfobtn.Text = "?";
            this.typeGamenInfobtn.UseVisualStyleBackColor = true;
            this.typeGamenInfobtn.Click += new System.EventHandler(this.typeGamenInfobtn_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.Transparent;
            this.groupBox1.Controls.Add(this.baccaratDemoBtnRadioEnabled);
            this.groupBox1.Controls.Add(this.baccaratDemoBtnRadioDisabled);
            this.groupBox1.Location = new System.Drawing.Point(746, 415);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(206, 34);
            this.groupBox1.TabIndex = 107;
            this.groupBox1.TabStop = false;
            // 
            // baccaratDemoBtnRadioEnabled
            // 
            this.baccaratDemoBtnRadioEnabled.AutoSize = true;
            this.baccaratDemoBtnRadioEnabled.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.baccaratDemoBtnRadioEnabled.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.baccaratDemoBtnRadioEnabled.Location = new System.Drawing.Point(114, 9);
            this.baccaratDemoBtnRadioEnabled.Name = "baccaratDemoBtnRadioEnabled";
            this.baccaratDemoBtnRadioEnabled.Size = new System.Drawing.Size(79, 24);
            this.baccaratDemoBtnRadioEnabled.TabIndex = 23;
            this.baccaratDemoBtnRadioEnabled.TabStop = true;
            this.baccaratDemoBtnRadioEnabled.Tag = "controlInput";
            this.baccaratDemoBtnRadioEnabled.Text = "DEMO";
            this.baccaratDemoBtnRadioEnabled.UseVisualStyleBackColor = true;
            // 
            // baccaratDemoBtnRadioDisabled
            // 
            this.baccaratDemoBtnRadioDisabled.AutoSize = true;
            this.baccaratDemoBtnRadioDisabled.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.baccaratDemoBtnRadioDisabled.ForeColor = System.Drawing.Color.Green;
            this.baccaratDemoBtnRadioDisabled.Location = new System.Drawing.Point(6, 9);
            this.baccaratDemoBtnRadioDisabled.Name = "baccaratDemoBtnRadioDisabled";
            this.baccaratDemoBtnRadioDisabled.Size = new System.Drawing.Size(67, 24);
            this.baccaratDemoBtnRadioDisabled.TabIndex = 22;
            this.baccaratDemoBtnRadioDisabled.TabStop = true;
            this.baccaratDemoBtnRadioDisabled.Tag = "controlInput";
            this.baccaratDemoBtnRadioDisabled.Text = "LIVE";
            this.baccaratDemoBtnRadioDisabled.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(744, 393);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(144, 20);
            this.label6.TabIndex = 106;
            this.label6.Text = "Tipologia GIOCO";
            // 
            // labelEnvironment
            // 
            this.labelEnvironment.AutoSize = true;
            this.labelEnvironment.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelEnvironment.Location = new System.Drawing.Point(775, 3);
            this.labelEnvironment.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelEnvironment.Name = "labelEnvironment";
            this.labelEnvironment.Size = new System.Drawing.Size(192, 29);
            this.labelEnvironment.TabIndex = 105;
            this.labelEnvironment.Text = "ENVIRONMENT";
            // 
            // textAreaInfoBtn
            // 
            this.textAreaInfoBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textAreaInfoBtn.ForeColor = System.Drawing.SystemColors.Highlight;
            this.textAreaInfoBtn.Location = new System.Drawing.Point(1022, 307);
            this.textAreaInfoBtn.Name = "textAreaInfoBtn";
            this.textAreaInfoBtn.Size = new System.Drawing.Size(37, 23);
            this.textAreaInfoBtn.TabIndex = 103;
            this.textAreaInfoBtn.Text = "?";
            this.textAreaInfoBtn.UseVisualStyleBackColor = true;
            this.textAreaInfoBtn.Click += new System.EventHandler(this.textAreaInfoBtn_Click);
            // 
            // labelTextAreaGiocatore
            // 
            this.labelTextAreaGiocatore.AutoSize = true;
            this.labelTextAreaGiocatore.Location = new System.Drawing.Point(964, 344);
            this.labelTextAreaGiocatore.Name = "labelTextAreaGiocatore";
            this.labelTextAreaGiocatore.Size = new System.Drawing.Size(110, 13);
            this.labelTextAreaGiocatore.TabIndex = 101;
            this.labelTextAreaGiocatore.Text = "Testo \"GIOCATORE\"";
            // 
            // labelTextAreaBanco
            // 
            this.labelTextAreaBanco.AutoSize = true;
            this.labelTextAreaBanco.Location = new System.Drawing.Point(744, 344);
            this.labelTextAreaBanco.Name = "labelTextAreaBanco";
            this.labelTextAreaBanco.Size = new System.Drawing.Size(84, 13);
            this.labelTextAreaBanco.TabIndex = 99;
            this.labelTextAreaBanco.Text = "Testo \"BANCO\"";
            // 
            // labelTextAreaVince
            // 
            this.labelTextAreaVince.AutoSize = true;
            this.labelTextAreaVince.Location = new System.Drawing.Point(889, 294);
            this.labelTextAreaVince.Name = "labelTextAreaVince";
            this.labelTextAreaVince.Size = new System.Drawing.Size(79, 13);
            this.labelTextAreaVince.TabIndex = 97;
            this.labelTextAreaVince.Text = "Testo \"VINCE\"";
            // 
            // labelTextAreaTie
            // 
            this.labelTextAreaTie.AutoSize = true;
            this.labelTextAreaTie.Location = new System.Drawing.Point(854, 344);
            this.labelTextAreaTie.Name = "labelTextAreaTie";
            this.labelTextAreaTie.Size = new System.Drawing.Size(64, 13);
            this.labelTextAreaTie.TabIndex = 95;
            this.labelTextAreaTie.Text = "Testo \"TIE\"";
            // 
            // numberChangeEndDeck
            // 
            this.numberChangeEndDeck.Location = new System.Drawing.Point(139, 184);
            this.numberChangeEndDeck.Name = "numberChangeEndDeck";
            this.numberChangeEndDeck.Size = new System.Drawing.Size(40, 20);
            this.numberChangeEndDeck.TabIndex = 92;
            this.numberChangeEndDeck.Tag = "controlInput";
            // 
            // labelChangeNumberEndDeck
            // 
            this.labelChangeNumberEndDeck.AutoSize = true;
            this.labelChangeNumberEndDeck.BackColor = System.Drawing.Color.Transparent;
            this.labelChangeNumberEndDeck.Location = new System.Drawing.Point(137, 167);
            this.labelChangeNumberEndDeck.Name = "labelChangeNumberEndDeck";
            this.labelChangeNumberEndDeck.Size = new System.Drawing.Size(139, 13);
            this.labelChangeNumberEndDeck.TabIndex = 91;
            this.labelChangeNumberEndDeck.Text = "Numero Cambio Fine Mazzo";
            // 
            // labelZoomPerc
            // 
            this.labelZoomPerc.AutoSize = true;
            this.labelZoomPerc.Location = new System.Drawing.Point(565, 251);
            this.labelZoomPerc.Name = "labelZoomPerc";
            this.labelZoomPerc.Size = new System.Drawing.Size(15, 13);
            this.labelZoomPerc.TabIndex = 90;
            this.labelZoomPerc.Text = "%";
            // 
            // autoBalanceLabel
            // 
            this.autoBalanceLabel.AutoSize = true;
            this.autoBalanceLabel.Location = new System.Drawing.Point(861, 51);
            this.autoBalanceLabel.Name = "autoBalanceLabel";
            this.autoBalanceLabel.Size = new System.Drawing.Size(90, 13);
            this.autoBalanceLabel.TabIndex = 89;
            this.autoBalanceLabel.Text = "Saldo Automatico";
            // 
            // checkBoxAutoSaldo
            // 
            this.checkBoxAutoSaldo.AutoSize = true;
            this.checkBoxAutoSaldo.Location = new System.Drawing.Point(840, 51);
            this.checkBoxAutoSaldo.Name = "checkBoxAutoSaldo";
            this.checkBoxAutoSaldo.Size = new System.Drawing.Size(15, 14);
            this.checkBoxAutoSaldo.TabIndex = 88;
            this.checkBoxAutoSaldo.UseVisualStyleBackColor = true;
            // 
            // mainareehelpbtn
            // 
            this.mainareehelpbtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mainareehelpbtn.ForeColor = System.Drawing.SystemColors.Highlight;
            this.mainareehelpbtn.Location = new System.Drawing.Point(595, 245);
            this.mainareehelpbtn.Name = "mainareehelpbtn";
            this.mainareehelpbtn.Size = new System.Drawing.Size(37, 23);
            this.mainareehelpbtn.TabIndex = 83;
            this.mainareehelpbtn.Text = "?";
            this.mainareehelpbtn.UseVisualStyleBackColor = true;
            this.mainareehelpbtn.Click += new System.EventHandler(this.mainareehelpbtn_Click);
            // 
            // stopwinlossinfobtn
            // 
            this.stopwinlossinfobtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.stopwinlossinfobtn.ForeColor = System.Drawing.SystemColors.Highlight;
            this.stopwinlossinfobtn.Location = new System.Drawing.Point(543, 90);
            this.stopwinlossinfobtn.Name = "stopwinlossinfobtn";
            this.stopwinlossinfobtn.Size = new System.Drawing.Size(37, 23);
            this.stopwinlossinfobtn.TabIndex = 82;
            this.stopwinlossinfobtn.Text = "?";
            this.stopwinlossinfobtn.UseVisualStyleBackColor = true;
            this.stopwinlossinfobtn.Click += new System.EventHandler(this.stopwinlossinfobtn_Click);
            // 
            // balanceinfobtn
            // 
            this.balanceinfobtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.balanceinfobtn.ForeColor = System.Drawing.SystemColors.Highlight;
            this.balanceinfobtn.Location = new System.Drawing.Point(974, 77);
            this.balanceinfobtn.Name = "balanceinfobtn";
            this.balanceinfobtn.Size = new System.Drawing.Size(37, 23);
            this.balanceinfobtn.TabIndex = 81;
            this.balanceinfobtn.Text = "?";
            this.balanceinfobtn.UseVisualStyleBackColor = true;
            this.balanceinfobtn.Click += new System.EventHandler(this.balanceinfobtn_Click);
            // 
            // cardcolorsinfobtn
            // 
            this.cardcolorsinfobtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cardcolorsinfobtn.ForeColor = System.Drawing.SystemColors.Highlight;
            this.cardcolorsinfobtn.Location = new System.Drawing.Point(566, 534);
            this.cardcolorsinfobtn.Name = "cardcolorsinfobtn";
            this.cardcolorsinfobtn.Size = new System.Drawing.Size(37, 23);
            this.cardcolorsinfobtn.TabIndex = 80;
            this.cardcolorsinfobtn.Text = "?";
            this.cardcolorsinfobtn.UseVisualStyleBackColor = true;
            this.cardcolorsinfobtn.Click += new System.EventHandler(this.cardcolorsinfobtn_Click);
            // 
            // martingalaHelpBtn
            // 
            this.martingalaHelpBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.martingalaHelpBtn.ForeColor = System.Drawing.SystemColors.Highlight;
            this.martingalaHelpBtn.Location = new System.Drawing.Point(106, 25);
            this.martingalaHelpBtn.Name = "martingalaHelpBtn";
            this.martingalaHelpBtn.Size = new System.Drawing.Size(16, 23);
            this.martingalaHelpBtn.TabIndex = 79;
            this.martingalaHelpBtn.Text = "?";
            this.martingalaHelpBtn.UseVisualStyleBackColor = true;
            this.martingalaHelpBtn.Click += new System.EventHandler(this.martingalaHelpBtn_Click);
            // 
            // mainhelpbtn
            // 
            this.mainhelpbtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mainhelpbtn.ForeColor = System.Drawing.SystemColors.Highlight;
            this.mainhelpbtn.Location = new System.Drawing.Point(1035, 593);
            this.mainhelpbtn.Name = "mainhelpbtn";
            this.mainhelpbtn.Size = new System.Drawing.Size(37, 23);
            this.mainhelpbtn.TabIndex = 78;
            this.mainhelpbtn.Text = "?";
            this.mainhelpbtn.UseVisualStyleBackColor = true;
            this.mainhelpbtn.Visible = false;
            this.mainhelpbtn.Click += new System.EventHandler(this.mainhelpbtn_Click);
            // 
            // customFichesPanel
            // 
            this.customFichesPanel.BackColor = System.Drawing.Color.LightGray;
            this.customFichesPanel.Controls.Add(this.noFichesLabel);
            this.customFichesPanel.Location = new System.Drawing.Point(370, 378);
            this.customFichesPanel.Name = "customFichesPanel";
            this.customFichesPanel.Size = new System.Drawing.Size(235, 88);
            this.customFichesPanel.TabIndex = 76;
            // 
            // noFichesLabel
            // 
            this.noFichesLabel.AutoSize = true;
            this.noFichesLabel.BackColor = System.Drawing.Color.Transparent;
            this.noFichesLabel.Location = new System.Drawing.Point(43, 39);
            this.noFichesLabel.Name = "noFichesLabel";
            this.noFichesLabel.Size = new System.Drawing.Size(151, 13);
            this.noFichesLabel.TabIndex = 6;
            this.noFichesLabel.Text = "Non ci sono fiches configurate";
            // 
            // customFichesEditBtn
            // 
            this.customFichesEditBtn.BackColor = System.Drawing.Color.Transparent;
            this.customFichesEditBtn.Location = new System.Drawing.Point(494, 349);
            this.customFichesEditBtn.Name = "customFichesEditBtn";
            this.customFichesEditBtn.Size = new System.Drawing.Size(112, 23);
            this.customFichesEditBtn.TabIndex = 75;
            this.customFichesEditBtn.Tag = "editFiches";
            this.customFichesEditBtn.Text = "Personalizza Fiches";
            this.customFichesEditBtn.UseVisualStyleBackColor = false;
            this.customFichesEditBtn.Click += new System.EventHandler(this.button2_Click);
            // 
            // lblNameConfig
            // 
            this.lblNameConfig.AutoSize = true;
            this.lblNameConfig.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNameConfig.Location = new System.Drawing.Point(476, 24);
            this.lblNameConfig.Name = "lblNameConfig";
            this.lblNameConfig.Size = new System.Drawing.Size(130, 16);
            this.lblNameConfig.TabIndex = 74;
            this.lblNameConfig.Text = "Nome_File_Caricato";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(302, 24);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(174, 16);
            this.label2.TabIndex = 73;
            this.label2.Text = "Configurazione caricata:";
            // 
            // btnSaveConfig
            // 
            this.btnSaveConfig.Location = new System.Drawing.Point(134, 57);
            this.btnSaveConfig.Name = "btnSaveConfig";
            this.btnSaveConfig.Size = new System.Drawing.Size(145, 30);
            this.btnSaveConfig.TabIndex = 72;
            this.btnSaveConfig.Text = "Salva Configurazione";
            this.btnSaveConfig.UseVisualStyleBackColor = true;
            this.btnSaveConfig.Click += new System.EventHandler(this.btnSaveConfig_Click);
            // 
            // labelRiconoscimentoFiches
            // 
            this.labelRiconoscimentoFiches.AutoSize = true;
            this.labelRiconoscimentoFiches.Location = new System.Drawing.Point(132, 360);
            this.labelRiconoscimentoFiches.Name = "labelRiconoscimentoFiches";
            this.labelRiconoscimentoFiches.Size = new System.Drawing.Size(117, 13);
            this.labelRiconoscimentoFiches.TabIndex = 71;
            this.labelRiconoscimentoFiches.Text = "Riconoscimento Fiches";
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStatus.Location = new System.Drawing.Point(566, 604);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(294, 24);
            this.labelStatus.TabIndex = 69;
            this.labelStatus.Text = "Stato Bot: Attesa Nuovo Mazzo";
            // 
            // balanceTotalValueText
            // 
            this.balanceTotalValueText.AutoEllipsis = true;
            this.balanceTotalValueText.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.balanceTotalValueText.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.balanceTotalValueText.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.balanceTotalValueText.Location = new System.Drawing.Point(742, 112);
            this.balanceTotalValueText.Name = "balanceTotalValueText";
            this.balanceTotalValueText.Size = new System.Drawing.Size(286, 31);
            this.balanceTotalValueText.TabIndex = 68;
            this.balanceTotalValueText.Text = "Saldo: € 0";
            this.balanceTotalValueText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelStartBalance
            // 
            this.labelStartBalance.AutoSize = true;
            this.labelStartBalance.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStartBalance.Location = new System.Drawing.Point(744, 80);
            this.labelStartBalance.Name = "labelStartBalance";
            this.labelStartBalance.Size = new System.Drawing.Size(103, 20);
            this.labelStartBalance.TabIndex = 66;
            this.labelStartBalance.Text = "Saldo Iniziale";
            // 
            // checkSafeWin
            // 
            this.checkSafeWin.AutoSize = true;
            this.checkSafeWin.Location = new System.Drawing.Point(470, 121);
            this.checkSafeWin.Name = "checkSafeWin";
            this.checkSafeWin.Size = new System.Drawing.Size(110, 17);
            this.checkSafeWin.TabIndex = 65;
            this.checkSafeWin.Tag = "controlInput";
            this.checkSafeWin.Text = "Safe Win Abilitato";
            this.checkSafeWin.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(458, 229);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 62;
            this.label1.Text = "Zoom Monitor";
            // 
            // txtZoomMonitor
            // 
            this.txtZoomMonitor.Location = new System.Drawing.Point(460, 245);
            this.txtZoomMonitor.Name = "txtZoomMonitor";
            this.txtZoomMonitor.Size = new System.Drawing.Size(100, 20);
            this.txtZoomMonitor.TabIndex = 61;
            this.txtZoomMonitor.Tag = "controlInput";
            // 
            // labelNumberProfittoSculping
            // 
            this.labelNumberProfittoSculping.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.labelNumberProfittoSculping.Location = new System.Drawing.Point(870, 194);
            this.labelNumberProfittoSculping.Name = "labelNumberProfittoSculping";
            this.labelNumberProfittoSculping.Size = new System.Drawing.Size(80, 20);
            this.labelNumberProfittoSculping.TabIndex = 60;
            this.labelNumberProfittoSculping.Text = "label1";
            // 
            // labelProfittoSculping
            // 
            this.labelProfittoSculping.AutoSize = true;
            this.labelProfittoSculping.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelProfittoSculping.Location = new System.Drawing.Point(744, 194);
            this.labelProfittoSculping.Name = "labelProfittoSculping";
            this.labelProfittoSculping.Size = new System.Drawing.Size(129, 20);
            this.labelProfittoSculping.TabIndex = 59;
            this.labelProfittoSculping.Text = "Profitto Sculping:";
            // 
            // labelNumerLose
            // 
            this.labelNumerLose.AutoSize = true;
            this.labelNumerLose.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.labelNumerLose.ForeColor = System.Drawing.Color.IndianRed;
            this.labelNumerLose.Location = new System.Drawing.Point(830, 266);
            this.labelNumerLose.Name = "labelNumerLose";
            this.labelNumerLose.Size = new System.Drawing.Size(51, 20);
            this.labelNumerLose.TabIndex = 58;
            this.labelNumerLose.Text = "label1";
            // 
            // labelNumberProfittoGlobale
            // 
            this.labelNumberProfittoGlobale.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelNumberProfittoGlobale.Location = new System.Drawing.Point(882, 159);
            this.labelNumberProfittoGlobale.Name = "labelNumberProfittoGlobale";
            this.labelNumberProfittoGlobale.Size = new System.Drawing.Size(80, 24);
            this.labelNumberProfittoGlobale.TabIndex = 57;
            this.labelNumberProfittoGlobale.Text = "label1";
            // 
            // labelNumerWin
            // 
            this.labelNumerWin.AutoSize = true;
            this.labelNumerWin.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelNumerWin.ForeColor = System.Drawing.Color.ForestGreen;
            this.labelNumerWin.Location = new System.Drawing.Point(826, 237);
            this.labelNumerWin.Name = "labelNumerWin";
            this.labelNumerWin.Size = new System.Drawing.Size(51, 20);
            this.labelNumerWin.TabIndex = 56;
            this.labelNumerWin.Text = "label1";
            // 
            // labelProfittoGlobale
            // 
            this.labelProfittoGlobale.AutoSize = true;
            this.labelProfittoGlobale.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelProfittoGlobale.Location = new System.Drawing.Point(744, 159);
            this.labelProfittoGlobale.Name = "labelProfittoGlobale";
            this.labelProfittoGlobale.Size = new System.Drawing.Size(141, 24);
            this.labelProfittoGlobale.TabIndex = 55;
            this.labelProfittoGlobale.Text = "Profitto Globale:";
            // 
            // labelVinte
            // 
            this.labelVinte.AutoSize = true;
            this.labelVinte.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelVinte.ForeColor = System.Drawing.Color.ForestGreen;
            this.labelVinte.Location = new System.Drawing.Point(742, 237);
            this.labelVinte.Name = "labelVinte";
            this.labelVinte.Size = new System.Drawing.Size(88, 20);
            this.labelVinte.TabIndex = 54;
            this.labelVinte.Text = "Mani Vinte:";
            // 
            // panelMartingala
            // 
            this.panelMartingala.AutoScroll = true;
            this.panelMartingala.BackColor = System.Drawing.Color.Transparent;
            this.panelMartingala.Location = new System.Drawing.Point(15, 57);
            this.panelMartingala.Name = "panelMartingala";
            this.panelMartingala.Size = new System.Drawing.Size(100, 420);
            this.panelMartingala.TabIndex = 1;
            // 
            // labelPerse
            // 
            this.labelPerse.AutoSize = true;
            this.labelPerse.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPerse.ForeColor = System.Drawing.Color.IndianRed;
            this.labelPerse.Location = new System.Drawing.Point(742, 266);
            this.labelPerse.Name = "labelPerse";
            this.labelPerse.Size = new System.Drawing.Size(92, 20);
            this.labelPerse.TabIndex = 53;
            this.labelPerse.Text = "Mani Perse:";
            // 
            // labelMartingala
            // 
            this.labelMartingala.AutoSize = true;
            this.labelMartingala.Location = new System.Drawing.Point(15, 29);
            this.labelMartingala.Name = "labelMartingala";
            this.labelMartingala.Size = new System.Drawing.Size(56, 13);
            this.labelMartingala.TabIndex = 0;
            this.labelMartingala.Text = "Martingala";
            // 
            // labelPerc
            // 
            this.labelPerc.AutoSize = true;
            this.labelPerc.BackColor = System.Drawing.Color.Transparent;
            this.labelPerc.Location = new System.Drawing.Point(449, 122);
            this.labelPerc.Name = "labelPerc";
            this.labelPerc.Size = new System.Drawing.Size(15, 13);
            this.labelPerc.TabIndex = 35;
            this.labelPerc.Text = "%";
            // 
            // safeWinPerc
            // 
            this.safeWinPerc.Location = new System.Drawing.Point(383, 120);
            this.safeWinPerc.Name = "safeWinPerc";
            this.safeWinPerc.Size = new System.Drawing.Size(60, 20);
            this.safeWinPerc.TabIndex = 4;
            this.safeWinPerc.Tag = "controlInput";
            // 
            // btnAddMartingala
            // 
            this.btnAddMartingala.BackColor = System.Drawing.Color.Transparent;
            this.btnAddMartingala.Location = new System.Drawing.Point(75, 24);
            this.btnAddMartingala.Name = "btnAddMartingala";
            this.btnAddMartingala.Size = new System.Drawing.Size(25, 25);
            this.btnAddMartingala.TabIndex = 0;
            this.btnAddMartingala.Tag = "";
            this.btnAddMartingala.Text = "+";
            this.btnAddMartingala.UseVisualStyleBackColor = true;
            this.btnAddMartingala.Click += new System.EventHandler(this.btnAddMartingala_Click);
            // 
            // labelSafeWin
            // 
            this.labelSafeWin.AutoSize = true;
            this.labelSafeWin.BackColor = System.Drawing.Color.Transparent;
            this.labelSafeWin.Location = new System.Drawing.Point(380, 104);
            this.labelSafeWin.Name = "labelSafeWin";
            this.labelSafeWin.Size = new System.Drawing.Size(51, 13);
            this.labelSafeWin.TabIndex = 33;
            this.labelSafeWin.Text = "Safe Win";
            // 
            // buttonLoadConfig
            // 
            this.buttonLoadConfig.BackColor = System.Drawing.Color.Transparent;
            this.buttonLoadConfig.Location = new System.Drawing.Point(134, 19);
            this.buttonLoadConfig.Name = "buttonLoadConfig";
            this.buttonLoadConfig.Size = new System.Drawing.Size(145, 30);
            this.buttonLoadConfig.TabIndex = 51;
            this.buttonLoadConfig.Tag = "";
            this.buttonLoadConfig.Text = "Carica Configurazione";
            this.buttonLoadConfig.UseVisualStyleBackColor = true;
            this.buttonLoadConfig.Click += new System.EventHandler(this.buttonLoadConfig_Click);
            // 
            // labelStopWinGlob
            // 
            this.labelStopWinGlob.AutoSize = true;
            this.labelStopWinGlob.BackColor = System.Drawing.Color.Transparent;
            this.labelStopWinGlob.Location = new System.Drawing.Point(133, 104);
            this.labelStopWinGlob.Name = "labelStopWinGlob";
            this.labelStopWinGlob.Size = new System.Drawing.Size(76, 13);
            this.labelStopWinGlob.TabIndex = 50;
            this.labelStopWinGlob.Text = "Stop Win Glob";
            // 
            // labelStopLoss
            // 
            this.labelStopLoss.AutoSize = true;
            this.labelStopLoss.BackColor = System.Drawing.Color.Transparent;
            this.labelStopLoss.Location = new System.Drawing.Point(302, 104);
            this.labelStopLoss.Name = "labelStopLoss";
            this.labelStopLoss.Size = new System.Drawing.Size(54, 13);
            this.labelStopLoss.TabIndex = 5;
            this.labelStopLoss.Text = "Stop Loss";
            // 
            // buttonStart
            // 
            this.buttonStart.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.buttonStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonStart.Location = new System.Drawing.Point(135, 593);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(424, 37);
            this.buttonStart.TabIndex = 24;
            this.buttonStart.Text = "AVVIA ▶";
            this.buttonStart.UseVisualStyleBackColor = false;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // groupBoxMode
            // 
            this.groupBoxMode.BackColor = System.Drawing.Color.Transparent;
            this.groupBoxMode.Controls.Add(this.radioModeMonocolore);
            this.groupBoxMode.Controls.Add(this.radioModeAlternata);
            this.groupBoxMode.Location = new System.Drawing.Point(353, 525);
            this.groupBoxMode.Name = "groupBoxMode";
            this.groupBoxMode.Size = new System.Drawing.Size(206, 52);
            this.groupBoxMode.TabIndex = 41;
            this.groupBoxMode.TabStop = false;
            // 
            // radioModeMonocolore
            // 
            this.radioModeMonocolore.AutoSize = true;
            this.radioModeMonocolore.Location = new System.Drawing.Point(114, 9);
            this.radioModeMonocolore.Name = "radioModeMonocolore";
            this.radioModeMonocolore.Size = new System.Drawing.Size(81, 17);
            this.radioModeMonocolore.TabIndex = 23;
            this.radioModeMonocolore.TabStop = true;
            this.radioModeMonocolore.Tag = "controlInput";
            this.radioModeMonocolore.Text = "Monocolore";
            this.radioModeMonocolore.UseVisualStyleBackColor = true;
            // 
            // radioModeAlternata
            // 
            this.radioModeAlternata.AutoSize = true;
            this.radioModeAlternata.Location = new System.Drawing.Point(6, 9);
            this.radioModeAlternata.Name = "radioModeAlternata";
            this.radioModeAlternata.Size = new System.Drawing.Size(67, 17);
            this.radioModeAlternata.TabIndex = 22;
            this.radioModeAlternata.TabStop = true;
            this.radioModeAlternata.Tag = "controlInput";
            this.radioModeAlternata.Text = "Alternata";
            this.radioModeAlternata.UseVisualStyleBackColor = true;
            // 
            // labelMode
            // 
            this.labelMode.AutoSize = true;
            this.labelMode.BackColor = System.Drawing.Color.Transparent;
            this.labelMode.Location = new System.Drawing.Point(350, 509);
            this.labelMode.Name = "labelMode";
            this.labelMode.Size = new System.Drawing.Size(47, 13);
            this.labelMode.TabIndex = 38;
            this.labelMode.Text = "Modalità";
            // 
            // labelRiconoscimentoArea
            // 
            this.labelRiconoscimentoArea.AutoSize = true;
            this.labelRiconoscimentoArea.BackColor = System.Drawing.Color.Transparent;
            this.labelRiconoscimentoArea.Location = new System.Drawing.Point(133, 229);
            this.labelRiconoscimentoArea.Name = "labelRiconoscimentoArea";
            this.labelRiconoscimentoArea.Size = new System.Drawing.Size(107, 13);
            this.labelRiconoscimentoArea.TabIndex = 21;
            this.labelRiconoscimentoArea.Text = "Riconoscimento area";
            // 
            // groupBoxStartColor
            // 
            this.groupBoxStartColor.BackColor = System.Drawing.Color.Transparent;
            this.groupBoxStartColor.Controls.Add(this.radioColorBlu);
            this.groupBoxStartColor.Controls.Add(this.radioColorRed);
            this.groupBoxStartColor.Location = new System.Drawing.Point(135, 525);
            this.groupBoxStartColor.Name = "groupBoxStartColor";
            this.groupBoxStartColor.Size = new System.Drawing.Size(193, 52);
            this.groupBoxStartColor.TabIndex = 42;
            this.groupBoxStartColor.TabStop = false;
            // 
            // radioColorBlu
            // 
            this.radioColorBlu.AutoSize = true;
            this.radioColorBlu.Location = new System.Drawing.Point(103, 9);
            this.radioColorBlu.Name = "radioColorBlu";
            this.radioColorBlu.Size = new System.Drawing.Size(40, 17);
            this.radioColorBlu.TabIndex = 21;
            this.radioColorBlu.TabStop = true;
            this.radioColorBlu.Tag = "controlInput";
            this.radioColorBlu.Text = "Blu";
            this.radioColorBlu.UseVisualStyleBackColor = true;
            // 
            // radioColorRed
            // 
            this.radioColorRed.AutoSize = true;
            this.radioColorRed.Location = new System.Drawing.Point(6, 9);
            this.radioColorRed.Name = "radioColorRed";
            this.radioColorRed.Size = new System.Drawing.Size(55, 17);
            this.radioColorRed.TabIndex = 20;
            this.radioColorRed.TabStop = true;
            this.radioColorRed.Tag = "controlInput";
            this.radioColorRed.Text = "Rosso";
            this.radioColorRed.UseVisualStyleBackColor = true;
            // 
            // labelColorStart
            // 
            this.labelColorStart.AutoSize = true;
            this.labelColorStart.BackColor = System.Drawing.Color.Transparent;
            this.labelColorStart.Location = new System.Drawing.Point(132, 509);
            this.labelColorStart.Name = "labelColorStart";
            this.labelColorStart.Size = new System.Drawing.Size(92, 13);
            this.labelColorStart.TabIndex = 23;
            this.labelColorStart.Text = "Colore di partenza";
            // 
            // labelStopWin
            // 
            this.labelStopWin.AutoSize = true;
            this.labelStopWin.BackColor = System.Drawing.Color.Transparent;
            this.labelStopWin.Location = new System.Drawing.Point(219, 104);
            this.labelStopWin.Name = "labelStopWin";
            this.labelStopWin.Size = new System.Drawing.Size(51, 13);
            this.labelStopWin.TabIndex = 3;
            this.labelStopWin.Text = "Stop Win";
            // 
            // buttonFish250
            // 
            this.buttonFish250.BackColor = System.Drawing.Color.Transparent;
            this.buttonFish250.Location = new System.Drawing.Point(135, 439);
            this.buttonFish250.Name = "buttonFish250";
            this.buttonFish250.Size = new System.Drawing.Size(112, 23);
            this.buttonFish250.TabIndex = 18;
            this.buttonFish250.Tag = "controlInput";
            this.buttonFish250.Text = "Fiches 250";
            this.buttonFish250.UseVisualStyleBackColor = true;
            this.buttonFish250.Click += new System.EventHandler(this.buttonFish250_Click);
            // 
            // buttonRed
            // 
            this.buttonRed.BackColor = System.Drawing.Color.Firebrick;
            this.buttonRed.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.buttonRed.Location = new System.Drawing.Point(136, 250);
            this.buttonRed.Name = "buttonRed";
            this.buttonRed.Size = new System.Drawing.Size(112, 23);
            this.buttonRed.TabIndex = 7;
            this.buttonRed.Tag = "controlInput";
            this.buttonRed.Text = "Rosso";
            this.buttonRed.UseVisualStyleBackColor = false;
            this.buttonRed.Click += new System.EventHandler(this.buttonRed_Click);
            // 
            // buttonFish100
            // 
            this.buttonFish100.BackColor = System.Drawing.Color.Transparent;
            this.buttonFish100.Location = new System.Drawing.Point(252, 410);
            this.buttonFish100.Name = "buttonFish100";
            this.buttonFish100.Size = new System.Drawing.Size(112, 23);
            this.buttonFish100.TabIndex = 17;
            this.buttonFish100.Tag = "controlInput";
            this.buttonFish100.Text = "Fiches 100";
            this.buttonFish100.UseVisualStyleBackColor = false;
            this.buttonFish100.Click += new System.EventHandler(this.buttonFish100_Click);
            // 
            // buttonBlu
            // 
            this.buttonBlu.BackColor = System.Drawing.Color.RoyalBlue;
            this.buttonBlu.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.buttonBlu.Location = new System.Drawing.Point(254, 250);
            this.buttonBlu.Name = "buttonBlu";
            this.buttonBlu.Size = new System.Drawing.Size(112, 23);
            this.buttonBlu.TabIndex = 8;
            this.buttonBlu.Tag = "controlInput";
            this.buttonBlu.Text = "Blu";
            this.buttonBlu.UseVisualStyleBackColor = false;
            this.buttonBlu.Click += new System.EventHandler(this.buttonBlu_Click);
            // 
            // buttonFish1
            // 
            this.buttonFish1.BackColor = System.Drawing.Color.Transparent;
            this.buttonFish1.Location = new System.Drawing.Point(135, 381);
            this.buttonFish1.Name = "buttonFish1";
            this.buttonFish1.Size = new System.Drawing.Size(112, 23);
            this.buttonFish1.TabIndex = 14;
            this.buttonFish1.Tag = "controlInput";
            this.buttonFish1.Text = "Fiches 1";
            this.buttonFish1.UseVisualStyleBackColor = true;
            this.buttonFish1.Click += new System.EventHandler(this.buttonFish1_Click);
            // 
            // buttonDoubling
            // 
            this.buttonDoubling.BackColor = System.Drawing.Color.Transparent;
            this.buttonDoubling.Location = new System.Drawing.Point(254, 308);
            this.buttonDoubling.Name = "buttonDoubling";
            this.buttonDoubling.Size = new System.Drawing.Size(112, 23);
            this.buttonDoubling.TabIndex = 11;
            this.buttonDoubling.Tag = "controlInput";
            this.buttonDoubling.Text = "Area Raddoppio";
            this.buttonDoubling.UseVisualStyleBackColor = false;
            this.buttonDoubling.Click += new System.EventHandler(this.buttonDoubling_Click);
            // 
            // buttonBalanceArea
            // 
            this.buttonBalanceArea.BackColor = System.Drawing.Color.Transparent;
            this.buttonBalanceArea.Location = new System.Drawing.Point(746, 46);
            this.buttonBalanceArea.Name = "buttonBalanceArea";
            this.buttonBalanceArea.Size = new System.Drawing.Size(80, 23);
            this.buttonBalanceArea.TabIndex = 13;
            this.buttonBalanceArea.Tag = "controlInput";
            this.buttonBalanceArea.Text = "Area Saldo";
            this.buttonBalanceArea.UseVisualStyleBackColor = false;
            this.buttonBalanceArea.Click += new System.EventHandler(this.buttonBalanceArea_Click);
            // 
            // buttonFish500
            // 
            this.buttonFish500.BackColor = System.Drawing.Color.Transparent;
            this.buttonFish500.Location = new System.Drawing.Point(252, 439);
            this.buttonFish500.Name = "buttonFish500";
            this.buttonFish500.Size = new System.Drawing.Size(112, 23);
            this.buttonFish500.TabIndex = 19;
            this.buttonFish500.Tag = "controlInput";
            this.buttonFish500.Text = "Fisches 500";
            this.buttonFish500.UseVisualStyleBackColor = true;
            this.buttonFish500.Click += new System.EventHandler(this.buttonFish500_Click);
            // 
            // buttonAreaVincita
            // 
            this.buttonAreaVincita.BackColor = System.Drawing.Color.Transparent;
            this.buttonAreaVincita.Cursor = System.Windows.Forms.Cursors.Default;
            this.buttonAreaVincita.Location = new System.Drawing.Point(253, 279);
            this.buttonAreaVincita.Name = "buttonAreaVincita";
            this.buttonAreaVincita.Size = new System.Drawing.Size(112, 23);
            this.buttonAreaVincita.TabIndex = 10;
            this.buttonAreaVincita.Tag = "controlInput";
            this.buttonAreaVincita.Text = "Area Vincita";
            this.buttonAreaVincita.UseVisualStyleBackColor = true;
            this.buttonAreaVincita.Click += new System.EventHandler(this.buttonAreaVincita_Click);
            // 
            // buttonDeckArea
            // 
            this.buttonDeckArea.BackColor = System.Drawing.Color.Transparent;
            this.buttonDeckArea.Location = new System.Drawing.Point(136, 308);
            this.buttonDeckArea.Name = "buttonDeckArea";
            this.buttonDeckArea.Size = new System.Drawing.Size(112, 23);
            this.buttonDeckArea.TabIndex = 12;
            this.buttonDeckArea.Tag = "controlInput";
            this.buttonDeckArea.Text = "Area Mazzo";
            this.buttonDeckArea.UseVisualStyleBackColor = true;
            this.buttonDeckArea.Click += new System.EventHandler(this.buttonDeckArea_Click);
            // 
            // buttonAreaCentrale
            // 
            this.buttonAreaCentrale.BackColor = System.Drawing.Color.Transparent;
            this.buttonAreaCentrale.Location = new System.Drawing.Point(136, 279);
            this.buttonAreaCentrale.Name = "buttonAreaCentrale";
            this.buttonAreaCentrale.Size = new System.Drawing.Size(112, 23);
            this.buttonAreaCentrale.TabIndex = 9;
            this.buttonAreaCentrale.Tag = "controlInput";
            this.buttonAreaCentrale.Text = "Area Riposo";
            this.buttonAreaCentrale.UseVisualStyleBackColor = true;
            this.buttonAreaCentrale.Click += new System.EventHandler(this.buttonAreaCentrale_Click);
            // 
            // buttonFish5
            // 
            this.buttonFish5.BackColor = System.Drawing.Color.Transparent;
            this.buttonFish5.Location = new System.Drawing.Point(252, 381);
            this.buttonFish5.Name = "buttonFish5";
            this.buttonFish5.Size = new System.Drawing.Size(112, 23);
            this.buttonFish5.TabIndex = 15;
            this.buttonFish5.Tag = "controlInput";
            this.buttonFish5.Text = "Fiches 5";
            this.buttonFish5.UseVisualStyleBackColor = true;
            this.buttonFish5.Click += new System.EventHandler(this.buttonFish5_Click);
            // 
            // buttonFish25
            // 
            this.buttonFish25.BackColor = System.Drawing.Color.Transparent;
            this.buttonFish25.Location = new System.Drawing.Point(135, 410);
            this.buttonFish25.Name = "buttonFish25";
            this.buttonFish25.Size = new System.Drawing.Size(112, 23);
            this.buttonFish25.TabIndex = 16;
            this.buttonFish25.Tag = "controlInput";
            this.buttonFish25.Text = "Fiches 25";
            this.buttonFish25.UseVisualStyleBackColor = true;
            this.buttonFish25.Click += new System.EventHandler(this.buttonFish25_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(8, 11);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1094, 668);
            this.tabControl1.TabIndex = 54;
            // 
            // txtComandiRicevuti
            // 
            this.txtComandiRicevuti.AcceptsReturn = true;
            this.txtComandiRicevuti.AcceptsTab = true;
            this.txtComandiRicevuti.Location = new System.Drawing.Point(744, 455);
            this.txtComandiRicevuti.Multiline = true;
            this.txtComandiRicevuti.Name = "txtComandiRicevuti";
            this.txtComandiRicevuti.Size = new System.Drawing.Size(311, 133);
            this.txtComandiRicevuti.TabIndex = 118;
            // 
            // Configuratore
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.ClientSize = new System.Drawing.Size(1111, 711);
            this.Controls.Add(this.testBtnWindowOnTop);
            this.Controls.Add(this.labelVersion);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.showboxbtn);
            this.Controls.Add(this.readsaldo);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.saldoLetto);
            this.Controls.Add(this.saldoLettoCorrect);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Configuratore";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "EUGENIO";
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.martingala4IndexAlarmValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.martingala4ChangeColorValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.martingala4EndDeckValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.martingala4StartDeckValue)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.martingala3IndexAlarmValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.martingala3ChangeColorValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.martingala3EndDeckValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.martingala3StartDeckValue)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.martingala2IndexAlarmValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.martingala2ChangeColorValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.martingala2EndDeckValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.martingala2StartDeckValue)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.martingala1IndexAlarmValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.martingala1ChangeColorValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.martingala1EndDeckValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.martingala1StartDeckValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown7)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown8)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown9)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown10)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown11)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown12)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown13)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown14)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown15)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown16)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown17)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.balanceRouletteStartValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown18)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericRouletteValueHand3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown19)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericRouletteValueHand2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown20)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericRouletteValueHand1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown21)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.globalRouletteStopLoss)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown22)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.globalRouletteStopWin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown23)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.balanceStartValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown24)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.stopLossValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown25)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.globalStopWinValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown26)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.stopWinValue)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.panelRoulettePlayed3.ResumeLayout(false);
            this.panelRoulettePlayed2.ResumeLayout(false);
            this.panelRoulettePlayed1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numberChangeEndDeck)).EndInit();
            this.customFichesPanel.ResumeLayout(false);
            this.customFichesPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtZoomMonitor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.safeWinPerc)).EndInit();
            this.groupBoxMode.ResumeLayout(false);
            this.groupBoxMode.PerformLayout();
            this.groupBoxStartColor.ResumeLayout(false);
            this.groupBoxStartColor.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        public TextBox txtComandiRicevuti;
    }
}

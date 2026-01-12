namespace MosaicLibary
{
    partial class cpiNMF
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cb_KeepUdating = new System.Windows.Forms.CheckBox();
            this.btnCreateModel = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.nudForgetFactor = new System.Windows.Forms.NumericUpDown();
            this.nudBatchSize = new System.Windows.Forms.NumericUpDown();
            this.nudNFeatures = new System.Windows.Forms.NumericUpDown();
            this.nudNComponents = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.nudSynergyReg = new System.Windows.Forms.NumericUpDown();
            this.nudEncodingReg = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.nudEpsilon = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.nudMaxIterFit = new System.Windows.Forms.NumericUpDown();
            this.nudMaxIterTransform = new System.Windows.Forms.NumericUpDown();
            this.btnLoadModel = new System.Windows.Forms.Button();
            this.btnSaveModel = new System.Windows.Forms.Button();
            this.btnGetSynergies = new System.Windows.Forms.Button();
            this.btnAddComponent = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.label6 = new System.Windows.Forms.Label();
            this.pb_reconstructionErr = new System.Windows.Forms.ProgressBar();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.smInputEMG = new ScopeMonitor();
            this.smReconstrucedEMG = new ScopeMonitor();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.spInputSignal = new SpiderPlotMonitor();
            this.smSynergyActivity = new ScopeMonitor();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.bt_resetSynergy = new System.Windows.Forms.Button();
            this.cb_resetSynergyX = new System.Windows.Forms.ComboBox();
            this.label14 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.cb_normalizeSynergies = new System.Windows.Forms.CheckBox();
            this.flowLayoutPanel4 = new System.Windows.Forms.FlowLayoutPanel();
            this.spS1 = new SpiderPlotMonitor();
            this.spS2 = new SpiderPlotMonitor();
            this.spS3 = new SpiderPlotMonitor();
            this.spS4 = new SpiderPlotMonitor();
            this.spS5 = new SpiderPlotMonitor();
            this.spS6 = new SpiderPlotMonitor();
            this.tcScopeMonitors = new System.Windows.Forms.TabControl();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.btn_resetSynergySensibility = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.cb_resetSynergySensitivty = new System.Windows.Forms.ComboBox();
            this.label15 = new System.Windows.Forms.Label();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tp = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudForgetFactor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudBatchSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudNFeatures)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudNComponents)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSynergyReg)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudEncodingReg)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudEpsilon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxIterFit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxIterTransform)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.flowLayoutPanel3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.flowLayoutPanel4.SuspendLayout();
            this.tcScopeMonitors.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cb_KeepUdating);
            this.groupBox1.Controls.Add(this.btnCreateModel);
            this.groupBox1.Controls.Add(this.tableLayoutPanel1);
            this.groupBox1.Controls.Add(this.btnLoadModel);
            this.groupBox1.Controls.Add(this.btnSaveModel);
            this.groupBox1.Controls.Add(this.btnGetSynergies);
            this.groupBox1.Controls.Add(this.btnAddComponent);
            this.groupBox1.Location = new System.Drawing.Point(8, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(800, 226);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Set up ";
            // 
            // cb_KeepUdating
            // 
            this.cb_KeepUdating.AutoSize = true;
            this.cb_KeepUdating.Location = new System.Drawing.Point(536, 174);
            this.cb_KeepUdating.Name = "cb_KeepUdating";
            this.cb_KeepUdating.Size = new System.Drawing.Size(116, 20);
            this.cb_KeepUdating.TabIndex = 6;
            this.cb_KeepUdating.Text = "Keep updating";
            this.cb_KeepUdating.UseVisualStyleBackColor = true;
            this.cb_KeepUdating.CheckedChanged += new System.EventHandler(this.cb_KeepUdating_CheckedChanged);
            // 
            // btnCreateModel
            // 
            this.btnCreateModel.Location = new System.Drawing.Point(249, 190);
            this.btnCreateModel.Name = "btnCreateModel";
            this.btnCreateModel.Size = new System.Drawing.Size(263, 26);
            this.btnCreateModel.TabIndex = 2;
            this.btnCreateModel.Text = "create new model";
            this.btnCreateModel.UseVisualStyleBackColor = true;
            this.btnCreateModel.Click += new System.EventHandler(this.btnCreateModel_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.nudForgetFactor, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.nudBatchSize, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.nudNFeatures, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.nudNComponents, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label8, 2, 5);
            this.tableLayoutPanel1.Controls.Add(this.nudSynergyReg, 3, 4);
            this.tableLayoutPanel1.Controls.Add(this.nudEncodingReg, 3, 2);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label10, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.label9, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.nudEpsilon, 3, 5);
            this.tableLayoutPanel1.Controls.Add(this.label2, 2, 4);
            this.tableLayoutPanel1.Controls.Add(this.label7, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.label11, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.label12, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.nudMaxIterFit, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.nudMaxIterTransform, 3, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(16, 19);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(503, 165);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // nudForgetFactor
            // 
            this.nudForgetFactor.DecimalPlaces = 2;
            this.nudForgetFactor.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nudForgetFactor.Location = new System.Drawing.Point(100, 3);
            this.nudForgetFactor.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudForgetFactor.Name = "nudForgetFactor";
            this.nudForgetFactor.Size = new System.Drawing.Size(96, 22);
            this.nudForgetFactor.TabIndex = 5;
            this.nudForgetFactor.Value = new decimal(new int[] {
            85,
            0,
            0,
            131072});
            this.nudForgetFactor.ValueChanged += new System.EventHandler(this.nudForgetFactor_ValueChanged);
            // 
            // nudBatchSize
            // 
            this.nudBatchSize.Location = new System.Drawing.Point(100, 87);
            this.nudBatchSize.Maximum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.nudBatchSize.Name = "nudBatchSize";
            this.nudBatchSize.Size = new System.Drawing.Size(96, 22);
            this.nudBatchSize.TabIndex = 13;
            this.nudBatchSize.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.nudBatchSize.ValueChanged += new System.EventHandler(this.nudBatchSize_ValueChanged);
            // 
            // nudNFeatures
            // 
            this.nudNFeatures.Location = new System.Drawing.Point(100, 59);
            this.nudNFeatures.Maximum = new decimal(new int[] {
            400,
            0,
            0,
            0});
            this.nudNFeatures.Name = "nudNFeatures";
            this.nudNFeatures.Size = new System.Drawing.Size(96, 22);
            this.nudNFeatures.TabIndex = 11;
            // 
            // nudNComponents
            // 
            this.nudNComponents.Location = new System.Drawing.Point(100, 31);
            this.nudNComponents.Name = "nudNComponents";
            this.nudNComponents.Size = new System.Drawing.Size(96, 22);
            this.nudNComponents.TabIndex = 3;
            this.nudNComponents.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "n components";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(202, 112);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(51, 16);
            this.label8.TabIndex = 7;
            this.label8.Text = "epsilon";
            // 
            // nudSynergyReg
            // 
            this.nudSynergyReg.DecimalPlaces = 5;
            this.nudSynergyReg.Location = new System.Drawing.Point(354, 87);
            this.nudSynergyReg.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.nudSynergyReg.Name = "nudSynergyReg";
            this.nudSynergyReg.Size = new System.Drawing.Size(96, 22);
            this.nudSynergyReg.TabIndex = 4;
            this.nudSynergyReg.Value = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.nudSynergyReg.ValueChanged += new System.EventHandler(this.nudSynergyReg_ValueChanged);
            // 
            // nudEncodingReg
            // 
            this.nudEncodingReg.DecimalPlaces = 5;
            this.nudEncodingReg.Location = new System.Drawing.Point(354, 59);
            this.nudEncodingReg.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.nudEncodingReg.Name = "nudEncodingReg";
            this.nudEncodingReg.Size = new System.Drawing.Size(96, 22);
            this.nudEncodingReg.TabIndex = 8;
            this.nudEncodingReg.Value = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.nudEncodingReg.ValueChanged += new System.EventHandler(this.nudEncodingReg_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(81, 16);
            this.label3.TabIndex = 2;
            this.label3.Text = "forget_factor";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(3, 84);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(67, 16);
            this.label10.TabIndex = 12;
            this.label10.Text = "batch size";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(3, 56);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(65, 16);
            this.label9.TabIndex = 10;
            this.label9.Text = "n features";
            // 
            // nudEpsilon
            // 
            this.nudEpsilon.DecimalPlaces = 6;
            this.nudEpsilon.Location = new System.Drawing.Point(354, 115);
            this.nudEpsilon.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudEpsilon.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            393216});
            this.nudEpsilon.Name = "nudEpsilon";
            this.nudEpsilon.Size = new System.Drawing.Size(96, 22);
            this.nudEpsilon.TabIndex = 9;
            this.nudEpsilon.Value = new decimal(new int[] {
            1,
            0,
            0,
            327680});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(202, 84);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(138, 16);
            this.label2.TabIndex = 1;
            this.label2.Text = "synergy regularization";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(202, 56);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(146, 16);
            this.label7.TabIndex = 6;
            this.label7.Text = "encoding regularization";
            // 
            // label11
            // 
            this.label11.AccessibleRole = System.Windows.Forms.AccessibleRole.MenuBar;
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(202, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(94, 16);
            this.label11.TabIndex = 14;
            this.label11.Text = "max iteration fit";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(202, 28);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(140, 16);
            this.label12.TabIndex = 15;
            this.label12.Text = "max iteration transform";
            // 
            // nudMaxIterFit
            // 
            this.nudMaxIterFit.Location = new System.Drawing.Point(354, 3);
            this.nudMaxIterFit.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudMaxIterFit.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudMaxIterFit.Name = "nudMaxIterFit";
            this.nudMaxIterFit.Size = new System.Drawing.Size(96, 22);
            this.nudMaxIterFit.TabIndex = 16;
            this.nudMaxIterFit.Value = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.nudMaxIterFit.ValueChanged += new System.EventHandler(this.nudMaxIterFit_ValueChanged);
            // 
            // nudMaxIterTransform
            // 
            this.nudMaxIterTransform.Location = new System.Drawing.Point(354, 31);
            this.nudMaxIterTransform.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudMaxIterTransform.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudMaxIterTransform.Name = "nudMaxIterTransform";
            this.nudMaxIterTransform.Size = new System.Drawing.Size(96, 22);
            this.nudMaxIterTransform.TabIndex = 17;
            this.nudMaxIterTransform.Value = new decimal(new int[] {
            200,
            0,
            0,
            0});
            // 
            // btnLoadModel
            // 
            this.btnLoadModel.Location = new System.Drawing.Point(537, 93);
            this.btnLoadModel.Name = "btnLoadModel";
            this.btnLoadModel.Size = new System.Drawing.Size(256, 29);
            this.btnLoadModel.TabIndex = 5;
            this.btnLoadModel.Text = "load existing NMF model";
            this.btnLoadModel.UseVisualStyleBackColor = true;
            this.btnLoadModel.Click += new System.EventHandler(this.btnLoadModel_Click);
            // 
            // btnSaveModel
            // 
            this.btnSaveModel.Location = new System.Drawing.Point(537, 54);
            this.btnSaveModel.Name = "btnSaveModel";
            this.btnSaveModel.Size = new System.Drawing.Size(256, 29);
            this.btnSaveModel.TabIndex = 4;
            this.btnSaveModel.Text = "save NMF model";
            this.btnSaveModel.UseVisualStyleBackColor = true;
            this.btnSaveModel.Click += new System.EventHandler(this.btnSaveModel_Click);
            // 
            // btnGetSynergies
            // 
            this.btnGetSynergies.Location = new System.Drawing.Point(537, 15);
            this.btnGetSynergies.Name = "btnGetSynergies";
            this.btnGetSynergies.Size = new System.Drawing.Size(256, 29);
            this.btnGetSynergies.TabIndex = 1;
            this.btnGetSynergies.Text = "save synergies";
            this.btnGetSynergies.UseVisualStyleBackColor = true;
            this.btnGetSynergies.Click += new System.EventHandler(this.btnGetSynergies_Click);
            // 
            // btnAddComponent
            // 
            this.btnAddComponent.Location = new System.Drawing.Point(537, 135);
            this.btnAddComponent.Name = "btnAddComponent";
            this.btnAddComponent.Size = new System.Drawing.Size(256, 29);
            this.btnAddComponent.TabIndex = 3;
            this.btnAddComponent.Text = "add one component to the model";
            this.btnAddComponent.UseVisualStyleBackColor = true;
            this.btnAddComponent.Click += new System.EventHandler(this.btnAddComponent_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.flowLayoutPanel1);
            this.tabPage2.Controls.Add(this.tableLayoutPanel2);
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(739, 867);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Input EMG and reconstructed EMG";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.label6);
            this.flowLayoutPanel1.Controls.Add(this.pb_reconstructionErr);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(9, 357);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(706, 47);
            this.flowLayoutPanel1.TabIndex = 3;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(133, 16);
            this.label6.TabIndex = 3;
            this.label6.Text = "Reconstruction error: ";
            // 
            // pb_reconstructionErr
            // 
            this.pb_reconstructionErr.Location = new System.Drawing.Point(142, 3);
            this.pb_reconstructionErr.Name = "pb_reconstructionErr";
            this.pb_reconstructionErr.Size = new System.Drawing.Size(285, 34);
            this.pb_reconstructionErr.TabIndex = 4;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.smInputEMG, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.smReconstrucedEMG, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.label4, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.label5, 1, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(6, 6);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(709, 345);
            this.tableLayoutPanel2.TabIndex = 2;
            // 
            // smInputEMG
            // 
            this.smInputEMG.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.smInputEMG.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.smInputEMG.Location = new System.Drawing.Point(3, 53);
            this.smInputEMG.Name = "smInputEMG";
            this.smInputEMG.Size = new System.Drawing.Size(314, 289);
            this.smInputEMG.TabIndex = 0;
            // 
            // smReconstrucedEMG
            // 
            this.smReconstrucedEMG.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.smReconstrucedEMG.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.smReconstrucedEMG.Location = new System.Drawing.Point(357, 53);
            this.smReconstrucedEMG.Name = "smReconstrucedEMG";
            this.smReconstrucedEMG.Size = new System.Drawing.Size(315, 289);
            this.smReconstrucedEMG.TabIndex = 1;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(68, 16);
            this.label4.TabIndex = 2;
            this.label4.Text = "Input EMG";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(357, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(127, 16);
            this.label5.TabIndex = 3;
            this.label5.Text = "Reconstructed EMG";
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.flowLayoutPanel3);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(739, 867);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Synergy Activity";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel3
            // 
            this.flowLayoutPanel3.AutoScroll = true;
            this.flowLayoutPanel3.Controls.Add(this.groupBox2);
            this.flowLayoutPanel3.Controls.Add(this.groupBox3);
            this.flowLayoutPanel3.Location = new System.Drawing.Point(6, 6);
            this.flowLayoutPanel3.Name = "flowLayoutPanel3";
            this.flowLayoutPanel3.Size = new System.Drawing.Size(709, 624);
            this.flowLayoutPanel3.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.spInputSignal);
            this.groupBox2.Controls.Add(this.smSynergyActivity);
            this.groupBox2.Location = new System.Drawing.Point(3, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(658, 274);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Activation/ Encoding & Input signal";
            // 
            // spInputSignal
            // 
            this.spInputSignal.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.spInputSignal.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.spInputSignal.Location = new System.Drawing.Point(347, 25);
            this.spInputSignal.Name = "spInputSignal";
            this.spInputSignal.Size = new System.Drawing.Size(304, 239);
            this.spInputSignal.TabIndex = 1;
            // 
            // smSynergyActivity
            // 
            this.smSynergyActivity.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.smSynergyActivity.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.smSynergyActivity.Location = new System.Drawing.Point(23, 25);
            this.smSynergyActivity.Name = "smSynergyActivity";
            this.smSynergyActivity.Size = new System.Drawing.Size(312, 239);
            this.smSynergyActivity.TabIndex = 0;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.bt_resetSynergy);
            this.groupBox3.Controls.Add(this.cb_resetSynergyX);
            this.groupBox3.Controls.Add(this.label14);
            this.groupBox3.Controls.Add(this.label13);
            this.groupBox3.Controls.Add(this.cb_normalizeSynergies);
            this.groupBox3.Controls.Add(this.flowLayoutPanel4);
            this.groupBox3.Location = new System.Drawing.Point(3, 283);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(657, 906);
            this.groupBox3.TabIndex = 1;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Synergies/ Base vectors";
            // 
            // bt_resetSynergy
            // 
            this.bt_resetSynergy.Location = new System.Drawing.Point(490, 28);
            this.bt_resetSynergy.Name = "bt_resetSynergy";
            this.bt_resetSynergy.Size = new System.Drawing.Size(76, 27);
            this.bt_resetSynergy.TabIndex = 7;
            this.bt_resetSynergy.Text = "reset";
            this.bt_resetSynergy.UseVisualStyleBackColor = true;
            this.bt_resetSynergy.Click += new System.EventHandler(this.bt_resetSynergy_Click);
            // 
            // cb_resetSynergyX
            // 
            this.cb_resetSynergyX.FormattingEnabled = true;
            this.cb_resetSynergyX.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6"});
            this.cb_resetSynergyX.Location = new System.Drawing.Point(398, 28);
            this.cb_resetSynergyX.Name = "cb_resetSynergyX";
            this.cb_resetSynergyX.Size = new System.Drawing.Size(78, 24);
            this.cb_resetSynergyX.TabIndex = 6;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(279, 31);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(97, 16);
            this.label14.TabIndex = 5;
            this.label14.Text = "Reset synergy:";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(3, 22);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(0, 16);
            this.label13.TabIndex = 4;
            // 
            // cb_normalizeSynergies
            // 
            this.cb_normalizeSynergies.AutoSize = true;
            this.cb_normalizeSynergies.Location = new System.Drawing.Point(23, 30);
            this.cb_normalizeSynergies.Name = "cb_normalizeSynergies";
            this.cb_normalizeSynergies.Size = new System.Drawing.Size(149, 20);
            this.cb_normalizeSynergies.TabIndex = 3;
            this.cb_normalizeSynergies.Text = "normalize synergies";
            this.tp.SetToolTip(this.cb_normalizeSynergies, "Normalizes the visualized synergies to the range [0,1)");
            this.cb_normalizeSynergies.UseVisualStyleBackColor = true;
            this.cb_normalizeSynergies.CheckedChanged += new System.EventHandler(this.cb_normalizeSynergies_CheckedChanged);
            // 
            // flowLayoutPanel4
            // 
            this.flowLayoutPanel4.Controls.Add(this.spS1);
            this.flowLayoutPanel4.Controls.Add(this.spS2);
            this.flowLayoutPanel4.Controls.Add(this.spS3);
            this.flowLayoutPanel4.Controls.Add(this.spS4);
            this.flowLayoutPanel4.Controls.Add(this.spS5);
            this.flowLayoutPanel4.Controls.Add(this.spS6);
            this.flowLayoutPanel4.Location = new System.Drawing.Point(20, 64);
            this.flowLayoutPanel4.Name = "flowLayoutPanel4";
            this.flowLayoutPanel4.Size = new System.Drawing.Size(638, 833);
            this.flowLayoutPanel4.TabIndex = 0;
            // 
            // spS1
            // 
            this.spS1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.spS1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.spS1.Location = new System.Drawing.Point(3, 3);
            this.spS1.Name = "spS1";
            this.spS1.Size = new System.Drawing.Size(311, 268);
            this.spS1.TabIndex = 0;
            // 
            // spS2
            // 
            this.spS2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.spS2.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.spS2.Location = new System.Drawing.Point(320, 3);
            this.spS2.Name = "spS2";
            this.spS2.Size = new System.Drawing.Size(311, 268);
            this.spS2.TabIndex = 2;
            // 
            // spS3
            // 
            this.spS3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.spS3.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.spS3.Location = new System.Drawing.Point(3, 277);
            this.spS3.Name = "spS3";
            this.spS3.Size = new System.Drawing.Size(311, 268);
            this.spS3.TabIndex = 1;
            // 
            // spS4
            // 
            this.spS4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.spS4.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.spS4.Location = new System.Drawing.Point(320, 277);
            this.spS4.Name = "spS4";
            this.spS4.Size = new System.Drawing.Size(311, 268);
            this.spS4.TabIndex = 3;
            // 
            // spS5
            // 
            this.spS5.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.spS5.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.spS5.Location = new System.Drawing.Point(3, 551);
            this.spS5.Name = "spS5";
            this.spS5.Size = new System.Drawing.Size(311, 268);
            this.spS5.TabIndex = 6;
            // 
            // spS6
            // 
            this.spS6.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.spS6.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.spS6.Location = new System.Drawing.Point(320, 551);
            this.spS6.Name = "spS6";
            this.spS6.Size = new System.Drawing.Size(311, 268);
            this.spS6.TabIndex = 7;
            // 
            // tcScopeMonitors
            // 
            this.tcScopeMonitors.Controls.Add(this.tabPage1);
            this.tcScopeMonitors.Controls.Add(this.tabPage2);
            this.tcScopeMonitors.Controls.Add(this.tabPage3);
            this.tcScopeMonitors.Controls.Add(this.tabPage4);
            this.tcScopeMonitors.Location = new System.Drawing.Point(12, 234);
            this.tcScopeMonitors.Name = "tcScopeMonitors";
            this.tcScopeMonitors.SelectedIndex = 0;
            this.tcScopeMonitors.Size = new System.Drawing.Size(747, 896);
            this.tcScopeMonitors.TabIndex = 2;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.btn_resetSynergySensibility);
            this.tabPage3.Controls.Add(this.button1);
            this.tabPage3.Controls.Add(this.cb_resetSynergySensitivty);
            this.tabPage3.Controls.Add(this.label15);
            this.tabPage3.Location = new System.Drawing.Point(4, 25);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(739, 867);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Reset synergy sensibility";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // btn_resetSynergySensibility
            // 
            this.btn_resetSynergySensibility.Location = new System.Drawing.Point(239, 21);
            this.btn_resetSynergySensibility.Name = "btn_resetSynergySensibility";
            this.btn_resetSynergySensibility.Size = new System.Drawing.Size(169, 39);
            this.btn_resetSynergySensibility.TabIndex = 10;
            this.btn_resetSynergySensibility.Text = "reset";
            this.btn_resetSynergySensibility.UseVisualStyleBackColor = true;
            this.btn_resetSynergySensibility.Click += new System.EventHandler(this.btn_resetSynergySensibility_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(-72, 358);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 9;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // cb_resetSynergySensitivty
            // 
            this.cb_resetSynergySensitivty.FormattingEnabled = true;
            this.cb_resetSynergySensitivty.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6"});
            this.cb_resetSynergySensitivty.Location = new System.Drawing.Point(141, 19);
            this.cb_resetSynergySensitivty.Name = "cb_resetSynergySensitivty";
            this.cb_resetSynergySensitivty.Size = new System.Drawing.Size(78, 24);
            this.cb_resetSynergySensitivty.TabIndex = 8;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(22, 22);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(97, 16);
            this.label15.TabIndex = 7;
            this.label15.Text = "Reset synergy:";
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.panel1);
            this.tabPage4.Location = new System.Drawing.Point(4, 25);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(739, 867);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "tabPage4";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(200, 100);
            this.panel1.TabIndex = 0;
            // 
            // tp
            // 
            this.tp.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.tp.ToolTipTitle = "Normalize synergies";
            // 
            // cpiNMF
            // 
            this.ClientSize = new System.Drawing.Size(808, 953);
            this.Controls.Add(this.tcScopeMonitors);
            this.Controls.Add(this.groupBox1);
            this.Name = "cpiNMF";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudForgetFactor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudBatchSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudNFeatures)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudNComponents)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSynergyReg)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudEncodingReg)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudEpsilon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxIterFit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaxIterTransform)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tabPage1.ResumeLayout(false);
            this.flowLayoutPanel3.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.flowLayoutPanel4.ResumeLayout(false);
            this.tcScopeMonitors.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.NumericUpDown nudForgetFactor;
        private System.Windows.Forms.NumericUpDown nudSynergyReg;
        private System.Windows.Forms.NumericUpDown nudNComponents;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button btnCreateModel;
        private System.Windows.Forms.Button btnGetSynergies;
        private System.Windows.Forms.Button btnAddComponent;
        private System.Windows.Forms.Button btnSaveModel;
        private System.Windows.Forms.Button btnLoadModel;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown nudEncodingReg;
        private System.Windows.Forms.NumericUpDown nudEpsilon;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.NumericUpDown nudNFeatures;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ProgressBar pb_reconstructionErr;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private ScopeMonitor smInputEMG;
        private ScopeMonitor smReconstrucedEMG;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel3;
        private System.Windows.Forms.GroupBox groupBox2;
        private ScopeMonitor smSynergyActivity;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TabControl tcScopeMonitors;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.NumericUpDown nudBatchSize;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.NumericUpDown nudMaxIterFit;
        private System.Windows.Forms.NumericUpDown nudMaxIterTransform;
        private SpiderPlotMonitor spInputSignal;
        private System.Windows.Forms.CheckBox cb_normalizeSynergies;
        private System.Windows.Forms.ToolTip tp;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.CheckBox cb_KeepUdating;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Button btn_resetSynergySensibility;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ComboBox cb_resetSynergySensitivty;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Integration.ElementHost elementHost1;
        private System.Windows.Forms.Button bt_resetSynergy;
        private System.Windows.Forms.ComboBox cb_resetSynergyX;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel4;
        private SpiderPlotMonitor spS1;
        private SpiderPlotMonitor spS2;
        private SpiderPlotMonitor spS3;
        private SpiderPlotMonitor spS4;
        private SpiderPlotMonitor spS5;
        private SpiderPlotMonitor spS6;
    }
}

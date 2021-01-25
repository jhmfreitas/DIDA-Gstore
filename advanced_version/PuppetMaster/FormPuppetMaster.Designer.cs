namespace PuppetMaster
{
    partial class FormPuppetMaster
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtBoxScriptLocation = new System.Windows.Forms.TextBox();
            this.btnRunScript = new System.Windows.Forms.Button();
            this.txtBoxOutput = new System.Windows.Forms.TextBox();
            this.btnRunCommand = new System.Windows.Forms.Button();
            this.txtBoxCommand = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // txtBoxScriptLocation
            // 
            this.txtBoxScriptLocation.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtBoxScriptLocation.Location = new System.Drawing.Point(315, 19);
            this.txtBoxScriptLocation.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtBoxScriptLocation.Name = "txtBoxScriptLocation";
            this.txtBoxScriptLocation.PlaceholderText = "Enter the script name";
            this.txtBoxScriptLocation.Size = new System.Drawing.Size(556, 32);
            this.txtBoxScriptLocation.TabIndex = 2;
            // 
            // btnRunScript
            // 
            this.btnRunScript.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnRunScript.Location = new System.Drawing.Point(39, 16);
            this.btnRunScript.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnRunScript.Name = "btnRunScript";
            this.btnRunScript.Size = new System.Drawing.Size(239, 39);
            this.btnRunScript.TabIndex = 0;
            this.btnRunScript.Text = "Run Script";
            this.btnRunScript.UseVisualStyleBackColor = true;
            this.btnRunScript.Click += new System.EventHandler(this.btnRunScript_Click);
            // 
            // txtBoxOutput
            // 
            this.txtBoxOutput.BackColor = System.Drawing.SystemColors.InfoText;
            this.txtBoxOutput.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtBoxOutput.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtBoxOutput.ForeColor = System.Drawing.SystemColors.Window;
            this.txtBoxOutput.HideSelection = false;
            this.txtBoxOutput.Location = new System.Drawing.Point(39, 144);
            this.txtBoxOutput.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtBoxOutput.Multiline = true;
            this.txtBoxOutput.Name = "txtBoxOutput";
            this.txtBoxOutput.ReadOnly = true;
            this.txtBoxOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtBoxOutput.Size = new System.Drawing.Size(833, 440);
            this.txtBoxOutput.TabIndex = 1;
            // 
            // btnRunCommand
            // 
            this.btnRunCommand.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnRunCommand.Location = new System.Drawing.Point(39, 79);
            this.btnRunCommand.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnRunCommand.Name = "btnRunCommand";
            this.btnRunCommand.Size = new System.Drawing.Size(239, 39);
            this.btnRunCommand.TabIndex = 0;
            this.btnRunCommand.Text = "Run Command";
            this.btnRunCommand.UseVisualStyleBackColor = true;
            this.btnRunCommand.Click += new System.EventHandler(this.btnRunCommand_Click);
            // 
            // txtBoxCommand
            // 
            this.txtBoxCommand.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtBoxCommand.Location = new System.Drawing.Point(315, 81);
            this.txtBoxCommand.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtBoxCommand.Name = "txtBoxCommand";
            this.txtBoxCommand.PlaceholderText = "Use \'help\' command if you need help";
            this.txtBoxCommand.Size = new System.Drawing.Size(556, 32);
            this.txtBoxCommand.TabIndex = 2;
            this.txtBoxCommand.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtBoxCommand_KeyPress);
            // 
            // FormPuppetMaster
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(914, 600);
            this.Controls.Add(this.txtBoxCommand);
            this.Controls.Add(this.btnRunCommand);
            this.Controls.Add(this.txtBoxOutput);
            this.Controls.Add(this.btnRunScript);
            this.Controls.Add(this.txtBoxScriptLocation);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "FormPuppetMaster";
            this.Text = "PuppetMaster";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtBoxScriptLocation;
        private System.Windows.Forms.Button btnRunScript;
        private System.Windows.Forms.TextBox txtBoxOutput;
        private System.Windows.Forms.Button btnRunCommand;
        private System.Windows.Forms.TextBox txtBoxCommand;
    }
}


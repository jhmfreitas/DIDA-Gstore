using Grpc.Net.Client;
using PuppetMaster.Commands;
using PuppetMaster.Commands.PCSCommands;
using PuppetMaster.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utils;

namespace PuppetMaster
{
    public partial class FormPuppetMaster : Form
    {

        private static readonly CommandDispatcher CommandDispatcher = new CommandDispatcher();
        private static readonly ConnectionManager ConnectionManager = new ConnectionManager();

        // always starts with the configuration
        private static bool isConfiguring = true;
        private static bool unhandledException = false;

        public FormPuppetMaster()
        {
            InitializeComponent();
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            txtBoxOutput.AppendText("Output:");
            RegisterCommands();
        }

        private void FatalException(Exception e, string title = "Unhandled Exception")
        {
            txtBoxCommand.BackColor = txtBoxCommand.BackColor; // without this line it doesn't work for some reason
            txtBoxCommand.ForeColor = System.Drawing.Color.Red;
            txtBoxCommand.Text = title + " - shutdown servers / clients and restart.";

            txtBoxOutput.AppendText(Environment.NewLine + Environment.NewLine);
            txtBoxOutput.AppendText("EXCEPTION:");
            txtBoxOutput.AppendText(Environment.NewLine + e.ToString());
            unhandledException = true;
            SetReadOnly(true);
        }

        private void SetReadOnly(bool option)
        {
            txtBoxCommand.ReadOnly = option || unhandledException;
            txtBoxScriptLocation.ReadOnly = option || unhandledException;
        }

        private void txtBoxCommand_KeyPress(object sender, KeyPressEventArgs e)
        {
            // press <Enter> is the same as click in 'Run Command'
            if (e.KeyChar == (char)13)
            {
                btnRunCommand_Click(sender, e);
            }
        }

        private async void btnRunScript_Click(object sender, EventArgs e)
        {
            string filename = txtBoxScriptLocation.Text + ".txt";
            if (string.IsNullOrEmpty(filename)) return;

            string[] lines;
            SetReadOnly(true);
            txtBoxScriptLocation.Clear();
            try
            {
                List<Task> tasks = new List<Task>();
                lines = System.IO.File.ReadAllLines(filename.Trim());

                foreach (string line in lines)
                {
                    if (line[0].Equals('#')) continue;
                    string lineLower = line.ToLower();
                    bool isConcurrent = CommandDispatcher.IsConcurrent(lineLower);

                    if (isConcurrent)
                    {
                        Task task = ExecuteCommand(lineLower);
                        tasks.Add(task);
                    }
                    else
                    {
                        await ExecuteCommand(lineLower);
                    }
                }
                await Task.WhenAll(tasks);
            }
            catch (System.IO.FileNotFoundException fileNotFoundException)
            {
                txtBoxOutput.AppendText(Environment.NewLine + "ERROR: File " + filename + " not found in current directory.");
                txtBoxOutput.AppendText(Environment.NewLine + fileNotFoundException.Message);
            }
            catch (CommandNotRegisteredException exception)
            {
                if (isConfiguring) FatalException(exception, "Configuration error");
                else txtBoxOutput.AppendText(Environment.NewLine + exception.Message);
            }
            catch (ApplySystemConfigurationException exception)
            {
                FatalException(exception);
            }
            catch (Exception exception)
            {
                // Unhandled exception
                FatalException(exception);
            }
            SetReadOnly(false);
        }

        private async void btnRunCommand_Click(object sender, EventArgs e)
        {
            string inputLine = txtBoxCommand.Text.ToLower();
            if (String.IsNullOrWhiteSpace(inputLine))
            {
                txtBoxOutput.AppendText(Environment.NewLine);
                return;
            }

            // clean the command textbox
            SetReadOnly(true);
            txtBoxCommand.Clear();
            bool isConcurrent;
            try
            {
                isConcurrent = CommandDispatcher.IsConcurrent(inputLine);
                if (isConcurrent) SetReadOnly(false);
                await ExecuteCommand(inputLine);
            }
            catch (CommandNotRegisteredException exception)
            {
                txtBoxOutput.AppendText(Environment.NewLine + exception.Message);
            }
            catch (ApplySystemConfigurationException exception)
            {
                FatalException(exception);
            }
            catch (Exception exception)
            {
                // Unhandled exception
                FatalException(exception);
            }
            SetReadOnly(false);
        }

        private static async Task ExecuteCommand(string inputLine)
        {
            //hard style
            string commandName = CommandDispatcher.ExtractCommandName(inputLine);
            if (isConfiguring && CommandDispatcher.IsValidCommand(commandName) && !commandName.Equals("replicationfactor") && !commandName.Equals("partition") && !commandName.Equals("server")
                && !commandName.Equals("help") && !commandName.Equals("quit") && !commandName.Equals("clear"))
            {
                isConfiguring = false;
                await CommandDispatcher.ExecuteAsync("applysystemconfiguration");
            }
            await CommandDispatcher.ExecuteAsync(inputLine);
        }

        private void RegisterCommands()
        {
            // possible commands for configuration
            CommandDispatcher.Register("replicationfactor", new ReplicationFactorCommand(txtBoxOutput));
            CommandDispatcher.Register("partition", new PartitionCommand(txtBoxOutput));
            CommandDispatcher.Register("server", new CreateServerCommand(txtBoxOutput, ConnectionManager));
            CommandDispatcher.Register("client", new CreateClientCommand(txtBoxOutput, ConnectionManager));
            CommandDispatcher.Register("status", new StatusCommand(txtBoxOutput, ConnectionManager));
            CommandDispatcher.Register("applysystemconfiguration", new ApplySystemConfigurationCommand(txtBoxOutput, ConnectionManager));
            // possible commands for Replicas (debug)
            CommandDispatcher.Register("crash", new CrashServerCommand(txtBoxOutput, ConnectionManager));
            CommandDispatcher.Register("freeze", new FreezeServerCommand(txtBoxOutput, ConnectionManager));
            CommandDispatcher.Register("unfreeze", new UnfreezeServerCommand(txtBoxOutput, ConnectionManager));
            // help commands
            CommandDispatcher.Register("wait", new WaitCommand(txtBoxOutput));
            CommandDispatcher.Register("help", new HelpCommand(txtBoxOutput));
            CommandDispatcher.Register("clear", new ClearCommand(txtBoxOutput));
            CommandDispatcher.Register("quit", new QuitCommand(txtBoxOutput));
        }
    }
}

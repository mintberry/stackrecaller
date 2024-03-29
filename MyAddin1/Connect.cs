using System;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using System.Resources;
using System.Reflection;


namespace MyAddin1
{
    /// <summary>The object for implementing an Add-in.</summary>
    /// <seealso class='IDTExtensibility2' />
    public class Connect : IDTExtensibility2, IDTCommandTarget
    {
        /// <summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
        public Connect()
        {
        }

        /// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
        /// <param term='application'>Root object of the host application.</param>
        /// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
        /// <param term='addInInst'>Object representing this Add-in.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
        {
            _applicationObject = (DTE2)application;
            _addInInstance = (AddIn)addInInst;

            System.Diagnostics.Trace.WriteLine(string.Format("Event: OnConnection, connectMode: {0}", connectMode));
            if (connectMode == ext_ConnectMode.ext_cm_AfterStartup || connectMode == ext_ConnectMode.ext_cm_Startup)
            {
                //Console.WriteLine("addin start");
                System.Diagnostics.Trace.WriteLine("addin start");

                object[] contextGUIDS = new object[] { };
                Commands2 commands = (Commands2)_applicationObject.Commands;

                try
                {
                    Command closeAllDocsCmd = CmdIsCreated(this.GetType().ToString() + ".CloseAllDocuments", commands);
                    Command cmdViewerCmd = CmdIsCreated(this.GetType().ToString() + ".CommandViewer", commands);
                    // get commandbar from name
                    CommandBar mdiDocCommandBar =
                        ((CommandBars)_applicationObject.CommandBars)["Easy MDI Document Window"];
                    // add command
                    if (closeAllDocsCmd == null)
                    {
                        closeAllDocsCmd = commands.AddNamedCommand2(_addInInstance, "CloseAllDocuments", "Close All Documents",
                               "Close All Documents", false, 0, ref contextGUIDS,
                               (int)vsCommandStatus.vsCommandStatusSupported + (int)vsCommandStatus.vsCommandStatusEnabled,
                               (int)vsCommandStyle.vsCommandStylePictAndText, vsCommandControlType.vsCommandControlTypeButton);

                        cmdViewerCmd = commands.AddNamedCommand2(_addInInstance, "CommandViewer", "Display All Commands",
                               "Display All Commands", false, 0, ref contextGUIDS,
                               (int)vsCommandStatus.vsCommandStatusSupported + (int)vsCommandStatus.vsCommandStatusEnabled,
                               (int)vsCommandStyle.vsCommandStylePictAndText, vsCommandControlType.vsCommandControlTypeButton);
                        System.Diagnostics.Trace.WriteLine(closeAllDocsCmd.Name);
                    }
                    // place it below Close All But This menu item
                    CommandBarControl closeAllButThisCmd = mdiDocCommandBar.Controls["Close All But This"];
                    // set the menu item index, notice that the index begins from 1
                    int closeAllCmdIndex = (closeAllButThisCmd == null) ? 1 : (closeAllButThisCmd.Index + 1);
                    //closeAllDocsCmd.AddControl(mdiDocCommandBar, closeAllCmdIndex);
                    cmdViewerCmd.AddControl(mdiDocCommandBar, closeAllCmdIndex);
                }
                catch (System.Exception e)
                {
                    System.Diagnostics.Trace.WriteLine(e.ToString());
                }
            }

        }

        /// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
        /// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
        {
            System.Diagnostics.Trace.WriteLine(string.Format("Event: OnDisconnection, disconnectMode: {0}", disconnectMode));
            CommandBar mdiDocCommandBar =
                        ((CommandBars)_applicationObject.CommandBars)["Easy MDI Document Window"];

            //mdiDocCommandBar.Controls["Close All Documents"].Delete();
            mdiDocCommandBar.Controls["Display All Commands"].Delete();
        }

        /// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />		
        public void OnAddInsUpdate(ref Array custom)
        {
        }

        /// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnStartupComplete(ref Array custom)
        {
        }

        /// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnBeginShutdown(ref Array custom)
        {
        }

        /// <summary>Implements the QueryStatus method of the IDTCommandTarget interface. This is called when the command's availability is updated</summary>
        /// <param term='commandName'>The name of the command to determine state for.</param>
        /// <param term='neededText'>Text that is needed for the command.</param>
        /// <param term='status'>The state of the command in the user interface.</param>
        /// <param term='commandText'>Text requested by the neededText parameter.</param>
        /// <seealso class='Exec' />
        public void QueryStatus(string commandName, vsCommandStatusTextWanted neededText,
            ref vsCommandStatus status, ref object commandText)
        {
            if (neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
            {
                if (commandName == "MyAddin1.Connect.CloseAllDocuments")
                {
                    status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
                    return;
                }
                else if (commandName == "MyAddin1.Connect.CommandViewer")
                {
                    status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
                    return;
                }
            }
        }


        /// <summary>Implements the Exec method of the IDTCommandTarget interface. This is called when the command is invoked.</summary>
        /// <param term='commandName'>The name of the command to execute.</param>
        /// <param term='executeOption'>Describes how the command should be run.</param>
        /// <param term='varIn'>Parameters passed from the caller to the command handler.</param>
        /// <param term='varOut'>Parameters passed from the command handler to the caller.</param>
        /// <param term='handled'>Informs the caller if the command was handled or not.</param>
        /// <seealso class='Exec' />
        public void Exec(string commandName, vsCommandExecOption executeOption,
            ref object varIn, ref object varOut, ref bool handled)
        {
            handled = false;
            if (executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault)
            {
                if (commandName == "MyAddin1.Connect.CloseAllDocuments")
                {
                    System.Diagnostics.Trace.WriteLine("The command is executed.");
                    //_applicationObject.ExecuteCommand("Window.CloseAllDocuments", String.Empty);
                    ShowAboutBochs();

                    handled = true;
                    return;
                }
                else if (commandName == "MyAddin1.Connect.CommandViewer")
                {
                    ShowAboutBochs();
                    handled = true;
                    return;
                }
            }
        }

        private void ShowAboutBochs()
        {
            AboutBochs ABTest = new AboutBochs();
            ABTest.DTEObject = _applicationObject;
            ABTest.testDialog();
        }

        private Command CmdIsCreated(string cmdname, Commands2 commands)
        {
            foreach (Command cmd in commands)
            {
                if (cmd.Name.Equals(cmdname))
                {
                    return cmd;
                }
            }
            return null;
        }

        private DTE2 _applicationObject;
        private AddIn _addInInstance;
    }
}
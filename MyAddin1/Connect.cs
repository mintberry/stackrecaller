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
            }

            try
            {
                // 通过名称获取要使用的命令栏
                CommandBar mdiDocCommandBar =
                    ((CommandBars)_applicationObject.CommandBars)["Easy MDI Document Window"];
                // 添加命令
                Command closeAllDocsCmd = Commands2.AddNamedCommand2(_addInInstance, "CloseAllDocuments", "Close All Documents",
                        "Close All Documents", false, 0, ref contextGUIDS,
                        (int)vsCommandStatus.vsCommandStatusSupported + (int)vsCommandStatus.vsCommandStatusEnabled,
                        (int)vsCommandStyle.vsCommandStylePictAndText, vsCommandControlType.vsCommandControlTypeButton);

                // 要放在Close All But This菜单项的下面
                CommandBarControl closeAllButThisCmd = mdiDocCommandBar.Controls["Close All But This"];
                // 设定菜单项的索引，注意索引是从1开始的
                int closeAllCmdIndex = (closeAllButThisCmd == null) ? 1 : (closeAllButThisCmd.Index + 1);
                closeAllDocsCmd.AddControl(mdiDocCommandBar, closeAllCmdIndex);
            }
            catch (System.ArgumentException)
            {

            }
        }

        /// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
        /// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
        {
            System.Diagnostics.Trace.WriteLine(string.Format("Event: OnDisconnection, disconnectMode: {0}", disconnectMode));
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

        /// <summary></summary>
        /// <param term=''></param>
        /// <seealso class='IDTCommandTarget' />
        public void QueryStatus(string commandName, vsCommandStatusTextWanted neededText,
            ref vsCommandStatus status, ref object commandText)
        {
            if (neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
            {
                if (commandName == "NEnhancer.Connect.CommandViewer")
                {
                    status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
                    return;
                }
            }
        }


        /// <summary></summary>
        /// <param term=''></param>
        /// <seealso class='IDTCommandTarget' />
        public void Exec(string commandName, vsCommandExecOption executeOption,
            ref object varIn, ref object varOut, ref bool handled)
        {
            handled = false;
            if (executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault)
            {
                if (commandName == "NEnhancer.Connect.CommandViewer")
                {
                    handled = true;
                    return;
                }
            }
        }


        private DTE2 _applicationObject;
        private AddIn _addInInstance;
    }
}
using System;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using Extensibility;
using NetOffice;
using Office = NetOffice.OfficeApi;
using NetOffice.OfficeApi.Enums;
using Excel = NetOffice.ExcelApi;
using NetOffice.ExcelApi.Enums;

namespace MyAddin
{
    [GuidAttribute("A1BB5F35-CAA3-4FAE-96EB-A8B156B7170E"), ProgId("MyAddin.Addin"), ComVisible(true)]
    public class Addin : IDTExtensibility2, Office.IRibbonExtensibility
    {
        Excel.Application _application;

        public Addin()
        {
        }

        #region IDTExtensibility2 Members

        public void OnConnection(object Application, ext_ConnectMode ConnectMode, object AddInInst, ref Array custom)
        {
            _application = new Excel.Application(null, Application);

            // If the addin not connected during startup, we call OnStartupComplete at hand
            if (ConnectMode != ext_ConnectMode.ext_cm_Startup)
                OnStartupComplete(ref custom);
        }

        public void OnStartupComplete(ref Array custom)
        {
            CreateUserInterface();
        }

        public void OnDisconnection(ext_DisconnectMode RemoveMode, ref Array custom)
        {
            // If this is not because of host shutdown(removed by user for example) we call OnBeginShutdown at hand
            if (RemoveMode != ext_DisconnectMode.ext_dm_HostShutdown)
                OnBeginShutdown(ref custom);

            if (null != _application)
            {
                _application.Dispose();
                _application = null;
            }

        }

        public void OnBeginShutdown(ref Array custom)
        {
            RemoveUserInterface();
        }

        public void OnAddInsUpdate(ref Array custom)
        {

        }

        #endregion

        #region IRibbonExtensibility Members

        public string GetCustomUI(string RibbonID)
        {
            try
            {
                return ReadRessourceFile("RibbonUI.xml");
            }
            catch (Exception throwedException)
            {
                string details = string.Format("{1}{1}Details:{1}{1}{0}", throwedException.Message, Environment.NewLine);
                MessageBox.Show("An error occured in GetCustomUI: " + details, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "";
            }
        }

        public void OnAction(Office.IRibbonControl control)
        {
            try
            {
                switch (control.Id)
                {
                    case "customButton1":
                        MessageBox.Show("This is the first sample button.");
                        break;
                    case "customButton2":
                        MessageBox.Show("This is the second sample button.");
                        break;
                    default:
                        MessageBox.Show("Unkown Control Id: " + control.Id);
                        break;
                }
            }
            catch (Exception throwedException)
            {
                string details = string.Format("{1}{1}Details:{1}{1}{0}", throwedException.Message, Environment.NewLine);
                MessageBox.Show("An error occured in OnAction: " + details, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region UserInterface

        private void CreateUserInterface()
        {
            // TODO: create UI items
        }

        private void RemoveUserInterface()
        {

        }
        #endregion

        #region COM Register Functions

        [ComRegisterFunctionAttribute]
        public static void RegisterFunction(Type type)
        {
            try
            {
                // add codebase value
                Assembly thisAssembly = Assembly.GetAssembly(typeof(Addin));
                RegistryKey key = Registry.ClassesRoot.CreateSubKey("CLSID\\{" + type.GUID.ToString().ToUpper() + "}\\InprocServer32\\1.0.0.0");
                key.SetValue("CodeBase", thisAssembly.CodeBase);
                key.Close();

                key = Registry.ClassesRoot.CreateSubKey("CLSID\\{" + type.GUID.ToString().ToUpper() + "}\\InprocServer32");
                key.SetValue("CodeBase", thisAssembly.CodeBase);
                key.Close();

                // add bypass key
                // http://support.microsoft.com/kb/948461
                key = Registry.ClassesRoot.CreateSubKey("Interface\\{000C0601-0000-0000-C000-000000000046}");
                string defaultValue = key.GetValue("") as string;
                if (null == defaultValue)
                    key.SetValue("", "Office .NET Framework Lockback Bypass Key");
                key.Close();

                // we dont register the addin in excel
                // the shim is registered in excel and load this addin
            }
            catch (Exception ex)
            {
                string details = string.Format("{1}{1}Details:{1}{1}{0}", ex.Message, Environment.NewLine);
                MessageBox.Show("An error occured." + details, "Register Addin", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        [ComUnregisterFunctionAttribute]
        public static void UnregisterFunction(Type type)
        {
            try
            {
                // unregister addin
                Registry.ClassesRoot.DeleteSubKey(@"CLSID\{" + type.GUID.ToString().ToUpper() + @"}\Programmable", false);
            }
            catch (Exception throwedException)
            {
                string details = string.Format("{1}{1}Details:{1}{1}{0}", throwedException.Message, Environment.NewLine);
                MessageBox.Show("An error occured." + details, "Unregister Addin", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// reads text file from ressource
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        protected internal static string ReadRessourceFile(string fileName)
        {
            Assembly assembly = typeof(Addin).Assembly;
            System.IO.Stream ressourceStream = assembly.GetManifestResourceStream(assembly.GetName().Name + "." + fileName);
            if (ressourceStream == null)
                throw (new System.IO.IOException("Error accessing resource Stream."));

            System.IO.StreamReader textStreamReader = new System.IO.StreamReader(ressourceStream);
            if (textStreamReader == null)
                throw (new System.IO.IOException("Error accessing resource File."));

            string text = textStreamReader.ReadToEnd();
            ressourceStream.Close();
            textStreamReader.Close();
            return text;
        }

        #endregion
    }
}

using System;
using System.Security.Permissions;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Configuration;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ComerAndonService
{
    public partial class Service1 : ServiceBase
    {
        Thread tr = null;
        private readonly object padlock = new object();
        string ParametersToPass = string.Empty;
        string timeIntervalToProcess = string.Empty;

        public Service1()
        {
            InitializeComponent();
        }

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        protected override void OnStart(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            try
            {
                Logger.WriteDebugLog("Starting Service.");
                ThreadStart job = new ThreadStart(ExecuteProcForLineStationStatus);
                tr = new Thread(job);
                tr.Name = "LineStationStatus";
                tr.Start();
                Logger.WriteDebugLog("Service thread has been started.");
            }
            catch (Exception e)
            {
                Logger.WriteErrorLog(e.ToString());
            }
        }

        private void ExecuteProcForLineStationStatus()
        {
            Logger.WriteDebugLog(string.Format("{0} thread started Processing.", Thread.CurrentThread.Name.ToString()));

            timeIntervalToProcess = ConfigurationManager.AppSettings["TimeInterval"].ToString();
            int timeIntervalInsec = 0;
            int.TryParse(timeIntervalToProcess, out timeIntervalInsec);
            ParametersToPass = ConfigurationManager.AppSettings["ParametersForProc"].ToString();
            List<string> _allPlants = DatabaseAccess.GetAllPlants();
            //List<AllPlantsAndMachinesDTO> allPlantsMachines = DatabaseAccess.GetAllPlantsAndMachines();
            while (true)
            {
                try
                {
                    Logger.WriteDebugLog("Started Updating Line Station Status at " + DateTime.Now.ToString("dd-MMM-yyyy HH:mm"));
                    if (ParametersToPass.Equals("Plant", StringComparison.OrdinalIgnoreCase))
                    {                    
                        foreach(string plant in _allPlants)
                        {
                            if (DatabaseAccess.SaveLineStationStatus(plant, ""))
                                Logger.WriteDebugLog(string.Format("Line Station Status Updated For : {0}", plant));
                            else
                                Logger.WriteErrorLog(string.Format("Failed To Update Line Station Status For : {0}", plant));
                        }
                    }
                    //else if (ParametersToPass.Equals("Machine", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    foreach (AllPlantsAndMachinesDTO data in allPlantsMachines)
                    //    {
                    //        DatabaseAccess.SaveLineStationStatus(data.Plant, data.Machine);
                    //        Logger.WriteDebugLog(string.Format("Line Station Status Updated For Plant : {0} and Machine : {1}", data.Plant, data.Machine));
                    //    }
                    //}
                }
                catch (Exception ex)
                {
                    Logger.WriteErrorLog(ex.Message);
                }
                finally
                {
                    lock (padlock)
                    {
                        Monitor.Wait(padlock, TimeSpan.FromSeconds(timeIntervalInsec));
                    }
                }
            }
        }

        protected override void OnStop()
        {
            lock (padlock)
            {
                Monitor.Pulse(padlock);
            }
            Logger.WriteDebugLog("Service has been stopped.");
            if (tr != null && tr.ThreadState == ThreadState.Running)
            {
                try
                {
                    tr.Abort();
                }
                catch (Exception ex)
                { }
            }
        }
        internal void StartDebug()
        {
            Logger.WriteDebugLog("Service started in DEBUG mode.");
            OnStart(null);
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = args.ExceptionObject as Exception;
            if (e != null)
            {
                Logger.WriteErrorLog("Unhandled Exception caught : " + e.ToString());
                Logger.WriteErrorLog("Runtime terminating:" + args.IsTerminating);
                var threadName = Thread.CurrentThread.Name;
                Logger.WriteErrorLog("Exception from Thread = " + threadName);
                System.Diagnostics.Process p = System.Diagnostics.Process.GetCurrentProcess();
                StringBuilder str = new StringBuilder();
                if (p != null)
                {
                    str.AppendLine("Total Handle count = " + p.HandleCount);
                    str.AppendLine("Total Threads count = " + p.Threads.Count);
                    str.AppendLine("Total Physical memory usage: " + p.WorkingSet64);

                    str.AppendLine("Peak physical memory usage of the process: " + p.PeakWorkingSet64);
                    str.AppendLine("Peak paged memory usage of the process: " + p.PeakPagedMemorySize64);
                    str.AppendLine("Peak virtual memory usage of the process: " + p.PeakVirtualMemorySize64);
                    Logger.WriteErrorLog(str.ToString());
                }
                Thread.CurrentThread.Abort();
                //while (true)
                //    Thread.Sleep(TimeSpan.FromHours(1));

            }
        }
    }
}

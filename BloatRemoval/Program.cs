using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.IO;
using System.Net;
using System.Diagnostics;

namespace BloatRemoval
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> list = new List<string>();
            
            Console.WriteLine("Starting with GUID's.. Downloading List, please wait..");
            var url = "https://raw.githubusercontent.com/mitchellurgero/bloatlist/master/GUID.txt";
            var client = new WebClient();
            using (var stream = client.OpenRead(url))
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.StartsWith("::"))
                    {
                        continue;
                    } else
                    {
                        list.Add(line);
                    }
                }
            }
            Console.WriteLine("Starting uninstall(s)");
            ListPrograms(list);
            Console.WriteLine();
            Console.WriteLine("Program Finished!");

            Console.ReadLine();
        }
        private static List<string> ListPrograms(List<string> names = null)
        {
            List<string> programs = new List<string>();
            if(names == null)
            {
                return null;
            }
            try
            {
                ManagementObjectSearcher mos =
                  new ManagementObjectSearcher("SELECT * FROM Win32_Product");
                foreach (ManagementObject mo in mos.Get())
                {
                    try
                    {
                        //more properties:
                        //http://msdn.microsoft.com/en-us/library/windows/desktop/aa394378(v=vs.85).aspx
                        if(names != null)
                        {
                            foreach(string prog in names)
                            {
                                if (mo["IdentifyingNumber"].ToString() == prog)
                                {
                                    //Console.Write(mo.ToString());
                                    UninstallProgram(mo["Name"].ToString(), mo["IdentifyingNumber"].ToString());
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //this program may not have a name property
                    }
                }

                return programs;

            }
            catch (Exception ex)
            {
                return programs;
            }
        }

        private static void UninstallProgram(string v1, string v2)
        {
            Console.WriteLine("===============================");
            Console.WriteLine(v1 + "::" + v2);
            //Uninstall the app:
            // Prepare the process to run
            ProcessStartInfo start = new ProcessStartInfo();
            // Enter in the command line arguments, everything you would enter after the executable name itself
            start.Arguments = "/x " + v2 + "/qn";
            // Enter the executable to run, including the complete path
            start.FileName = "msiexec.exe";
            // Do you want to show a console window?
            start.WindowStyle = ProcessWindowStyle.Hidden;
            start.CreateNoWindow = true;
            int exitCode;
            // Run the external process & wait for it to finish
            using (Process proc = Process.Start(start))
            {
                Console.WriteLine("Waiting for uninstall to finish....");
                proc.WaitForExit();

                // Retrieve the app's exit code
                exitCode = proc.ExitCode;
            }
            Console.WriteLine("Finished with exit code " + exitCode.ToString());
            Console.WriteLine("===============================");
        }
    }
}

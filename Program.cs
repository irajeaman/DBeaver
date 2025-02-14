using System;
using System.Diagnostics;
using System.IO;
using System.Management; // Add reference to System.Management
using System.Threading;

namespace RdpFileCreator
{
    class Program
    {
        static void Main(string[] args)
        {
            // Check if sufficient arguments are provided
            if (args.Length < 5)
            {
                Console.WriteLine("Usage: RdpFileCreator <IP_Address> <ApplicationName> <Username> <Password> <DB_ConnectionString>");
                Console.WriteLine("Example: RdpFileCreator 172.28.91.133 sqldeveloper myUser myPassword \"driver=Oracle|host=172.15.16.12|port=1521|database=XEPDB1|user=system|password=ora123\"");
                return;
            }

            // Retrieve values from command-line arguments
            string server = args[0];          // IP Address
            string applicationName = args[1]; // Application Name
            string username = args[2];        // Username
            string password = args[3];        // Password
            string dbConnectionString = args[4]; // Database connection string

            // Define the RDP file path and log file path
            string rdpFilePath = $"C:\\temp\\AutoLogin_{username}.rdp"; // Save RDP file with username in the name
            string logFilePath = "C:\\temp\\irajeapp.txt";             // Log file path

            // Log off the user if they are already logged in
            LogOffUser(username);

            // Wait for a moment to ensure logoff is processed
            Thread.Sleep(2000);

            // Create the RDP file content using the parameters
            string remoteAppCommandLine = $"-con \"{dbConnectionString}\"";
            string rdpContent = $@"
full address:s:{server}
alternate shell:s:rdpinit.exe
remoteapplicationmode:i:1
remoteapplicationname:s:{applicationName}
remoteapplicationprogram:s:||{applicationName}
remoteapplicationcmdline:s:{remoteAppCommandLine}
disableremoteappcapscheck:i:1
drivestoredirect:s:*
promptcredentialonce:i:1
redirectcomports:i:1
span monitors:i:1
use multimon:i:1";

            try
            {
                // Create the RDP file with the specified content
                Directory.CreateDirectory(Path.GetDirectoryName(rdpFilePath));
                File.WriteAllText(rdpFilePath, rdpContent);
                LogActivity(logFilePath, $"RDP file created successfully at {rdpFilePath}.");

                // Start the RDP session using mstsc in RemoteApp mode
                Process rdpProcess = Process.Start("mstsc.exe", $"{rdpFilePath}");
                LogActivity(logFilePath, $"Remote application {applicationName} started remotely on {server}.");

                // Monitor the remote application
                MonitorApplicationClose(applicationName, logFilePath, username);

                // Wait for 3 seconds before deleting the RDP file
                Thread.Sleep(3000);

                // Delete the RDP file
                if (File.Exists(rdpFilePath))
                {
                    File.Delete(rdpFilePath);
                    LogActivity(logFilePath, $"RDP file {rdpFilePath} deleted after 3 seconds.");
                }
            }
            catch (Exception ex)
            {
                LogActivity(logFilePath, $"An error occurred: {ex.Message}");
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        // Method to log activity to a specified log file
        static void LogActivity(string logFilePath, string message)
        {
            try
            {
                // Append message to log file with timestamp
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    writer.WriteLine($"{DateTime.Now}: {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to log activity: {ex.Message}");
            }
        }

        // Method to monitor the remote application process
        static void MonitorApplicationClose(string applicationName, string logFilePath, string username)
        {
            // Continuously monitor until the application is closed
            while (true)
            {
                // Check if the application process is running
                var processes = Process.GetProcessesByName(applicationName);
                if (processes.Length == 0)
                {
                    LogActivity(logFilePath, $"{applicationName} has been closed. Logging off user {username}.");
                    LogOffUser(username); // Log off the user

                    // After logging off the user, delete the log files on the remote server
                    DeleteLogFiles(username);

                    break; // Exit the monitoring loop
                }

                // Removed Thread.Sleep(1000); to check continuously without delay
            }
        }

        // Method to log off the user using WMI
        static void LogOffUser(string username)
        {
            try
            {
                // Get the current user session ID using WMI
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
                foreach (ManagementObject obj in searcher.Get())
                {
                    string loggedUser = obj["UserName"]?.ToString();
                    if (loggedUser != null && loggedUser.Contains(username))
                    {
                        // Log off the user by calling the process
                        Process.Start("taskkill", $"/F /IM {username}.exe"); // This assumes that username.exe is the process name
                        LogActivity("C:\\temp\\irajeapp.txt", $"User {username} logged off using WMI.");
                        return;
                    }
                }
                LogActivity("C:\\temp\\irajeapp.txt", $"No active session found for user {username}.");
            }
            catch (Exception ex)
            {
                LogActivity("C:\\temp\\irajeapp.txt", $"Failed to log off user: {ex.Message}");
                Console.WriteLine($"Failed to log off user: {ex.Message}");
            }
        }

        // Method to delete log files on the remote server
        static void DeleteLogFiles(string username)
        {
            try
            {
                string remoteLogPath = $@"C:\Users\{username}\AppData\Roaming\DBeaverData\workspace6\.metadata\*.log";
                Process deleteProcess = new Process();
                deleteProcess.StartInfo.FileName = "cmd.exe";
                deleteProcess.StartInfo.Arguments = $"/c del \"{remoteLogPath}\""; // Enclose in quotes for spaces
                deleteProcess.StartInfo.RedirectStandardOutput = true;
                deleteProcess.StartInfo.UseShellExecute = false;
                deleteProcess.StartInfo.CreateNoWindow = true;

                deleteProcess.Start();
                deleteProcess.WaitForExit();

                LogActivity("C:\\temp\\irajeapp.txt", $"Log files deleted from {remoteLogPath}.");
            }
            catch (Exception ex)
            {
                LogActivity("C:\\temp\\irajeapp.txt", $"Failed to delete log files: {ex.Message}");
                Console.WriteLine($"Failed to delete log files: {ex.Message}");
            }
        }
    }
}

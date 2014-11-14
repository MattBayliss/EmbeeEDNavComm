using Coho.IpcLibrary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EmbeeEDNav
{
    public class NavServerConnection
    {
        private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private static PipeStream ConnectToPipe(IpcClientPipe pipeClient)
        {
            PipeStream pipe = null;

            try
            {
                pipe = pipeClient.Connect(5000);

                        if (pipe.IsConnected)
                        {
                            Logger.Debug("Connected");
                        }
                        else
                        {
                            pipe = null;
                        }
            }
            catch (System.TimeoutException te)
            {
                Logger.Error("Connection attempt failed", te);
            }

            return pipe;
        }

        public static void EnsureServerIsRunning()
        {
            //get setting for nav server exe
            var serverPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Embee\EDNavComm", "NavServerExe", string.Empty);

            if (string.IsNullOrEmpty(serverPath))
            {
                throw new ApplicationException("Missing the Nav Server e x e setting from the registry");
            }

            var serverProcess = Path.GetFileNameWithoutExtension(serverPath);

            var isRunning = IsProcessRunning(serverProcess);

            if (!isRunning)
            {
                try
                {
                    Logger.Debug("{0} isn't running - trying to launch it...", serverPath);

                    var processStartInfo = new ProcessStartInfo(serverPath);
                    processStartInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                    var process = Process.Start(processStartInfo);
                    process.WaitForInputIdle(5000);
                }
                catch (Exception e)
                {
                    Logger.Error("Failed to launch " + Path.GetFileName(serverPath), e);
                }
            }
        }

        private static bool IsProcessRunning(string processName)
        {
            bool isrunning = false;

            if (!string.IsNullOrEmpty(processName))
            {
                Process[] processes = Process.GetProcessesByName(processName);
                isrunning = processes.Length > 0;
            }

            return isrunning;
        }

        public static async Task<string> SendMessageToServerAsync(string message)
        {
            var pipeAddress = "EmbeeEDNavServer";
            var pipeClient = new IpcClientPipe(".", pipeAddress);

            int connectattempt = 1;

            PipeStream pipe = ConnectToPipe(pipeClient);

            if (pipe == null)
            {
                //no current NavServer listening - start it up
                EnsureServerIsRunning();

                while ((pipe == null) && (connectattempt <= 10))
                {
                    Logger.Debug("Attempting to connect to named pipe: {0} - attempt {1}", pipeAddress, connectattempt);

                    try
                    {
                        pipe = ConnectToPipe(pipeClient);
                    }
                    catch (System.TimeoutException te)
                    {
                        Logger.Error("Connection attempt failed", te);
                    }

                    connectattempt++;
                }

                if (pipe == null)
                {
                    throw new ApplicationException("Can't connect to the NavServer");
                }
            }
            Logger.Trace("Sending data: {0}", message);

            // Asynchronously send data to the server
            var output = Encoding.UTF8.GetBytes(message);
            Debug.Assert(output.Length < IpcServer.SERVER_IN_BUFFER_SIZE);
            await pipe.WriteAsync(output, 0, output.Length);

            // Read the result
            Logger.Trace("Waiting for response...");
            var data = new Byte[IpcServer.SERVER_OUT_BUFFER_SIZE];
            int bytesRead = await pipe.ReadAsync(data, 0, data.Length);

            Logger.Trace("Received response from server");

            Logger.Trace("Server response: {0}", Encoding.UTF8.GetString(data, 0, bytesRead));

            // Done with this one
            Logger.Debug("Closing named pipe...");
            pipe.Close();
            Logger.Debug("Pipe closed");

            return Encoding.UTF8.GetString(data);
        }
    }
}

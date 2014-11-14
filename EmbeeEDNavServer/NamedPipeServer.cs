using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EmbeeEDNavServer
{
    public class NamedPipeServer : Coho.IpcLibrary.IpcCallback
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private Controller _controller;

        private int _totalThreads = 0;
        private string _pipeAddress = string.Empty;
        private Int32 _count;

        private Coho.IpcLibrary.IpcServer _ipcServer;

        private static Thread[] _servers = new Thread[] { };

        private Queue<string> _incomingMessages = new Queue<string>();
        private static readonly object padlock = new object();

        public NamedPipeServer(Controller controller, int totalThreads)
        {
            _controller = controller;
            _totalThreads = totalThreads;
            _pipeAddress = "EmbeeEDNavServer";            
        }

        public async Task StartAsync()
        {
            logger.Debug("Started on thread {0}", Thread.CurrentThread.ManagedThreadId);

            _ipcServer = new Coho.IpcLibrary.IpcServer(_pipeAddress, this, _totalThreads);

            await _controller.InitAsync();
        }

        public void Stop()
        {
            logger.Debug("Stop");

            _ipcServer.IpcServerStop();
        }

        public void OnAsyncConnect(PipeStream pipe, out object state)
        {
            var count = Interlocked.Increment(ref _count);

            logger.Trace("Connected: " + count);

            state = count;
        }

        public void OnAsyncDisconnect(PipeStream pipe, object state)
        {
            logger.Trace("Disconnected: {0}", (Int32)state);
        }

        public void OnAsyncMessage(PipeStream pipe, byte[] data, int bytes, object state)
        {
            logger.Trace("OnAsyncMessage called");
            
            var message = Encoding.UTF8.GetString(data, 0, bytes);
            var result = string.Empty;
            try
            {
                logger.Trace("Sending message to controller: {0}", message);
                result = _controller.ProcessMessageAsync(message).Result;
                if (string.IsNullOrEmpty(result))
                {
                    throw new ApplicationException(string.Format("Failed to process the message \"{0}\". An empty result was received", message));
                }
                logger.Trace("Result received from controller: {0}", result);

            }
            catch(Exception ex)
            {
                logger.Error("Failed to process message " + message, ex);
                result = string.Format(" |{0}", ex.Message);
            }

            data = Encoding.UTF8.GetBytes(result);

            // Write results
            try
            {
                pipe.BeginWrite(data, 0, data.Length, OnAsyncWriteComplete, pipe);
            }
            catch (Exception wex)
            {
                logger.Error("Failed to write response", wex);
                pipe.Close();
            }
        }

        private void OnAsyncWriteComplete(IAsyncResult result)
        {
            var pipe = (PipeStream)result.AsyncState;

            pipe.EndWrite(result);
        }
    }
}

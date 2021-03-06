﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace LogExpert
{
    internal class SysoutPipe
    {
        #region Fields

        private readonly StreamReader sysout;
        private StreamWriter writer;

        #endregion

        #region cTor

        public SysoutPipe(StreamReader sysout)
        {
            this.sysout = sysout;
            this.FileName = Path.GetTempFileName();
            Logger.logInfo("sysoutPipe created temp file: " + this.FileName);
            FileStream fStream = new FileStream(this.FileName, FileMode.Append, FileAccess.Write, FileShare.Read);
            this.writer = new StreamWriter(fStream, Encoding.Unicode);
            Thread thread = new Thread(new ThreadStart(this.ReaderThread));
            thread.IsBackground = true;
            thread.Start();
        }

        #endregion

        #region Properties

        public string FileName { get; }

        #endregion

        #region Public methods

        public void ClosePipe()
        {
            this.writer.Close();
            this.writer = null;
        }


        public void DataReceivedEventHandler(object sender, DataReceivedEventArgs e)
        {
            this.writer.WriteLine(e.Data);
        }

        public void ProcessExitedEventHandler(object sender, System.EventArgs e)
        {
            //ClosePipe();
            if (sender.GetType() == typeof(Process))
            {
                ((Process) sender).Exited -= this.ProcessExitedEventHandler;
                ((Process) sender).OutputDataReceived -= this.DataReceivedEventHandler;
            }
        }

        #endregion

        protected void ReaderThread()
        {
            char[] buff = new char[256];
            while (true)
            {
                try
                {
                    int read = this.sysout.Read(buff, 0, 256);
                    if (read == 0)
                    {
                        break;
                    }
                    writer.Write(buff, 0, read);
                }
                catch (IOException)
                {
                    break;
                }
            }
            ClosePipe();
        }
    }
}
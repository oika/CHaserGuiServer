using Oika.Libs.MeLogg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Oika.Apps.CHaserGuiServer.Line
{
    class LineCommunicator : IDisposable
    {
        static readonly Encoding LineEncoding = Encoding.GetEncoding("Shift_JIS");

        readonly int port;
        readonly Dumper sentDumper;
        readonly Dumper recvDumper;
        
        Socket socket;


        public LineCommunicator(int port, ILogger sentDumpLogger, ILogger recvDumpLogger)
        {
            this.port = port;
            this.sentDumper = new Dumper(sentDumpLogger);
            this.recvDumper = new Dumper(recvDumpLogger);
        }

        public string Accept()
        {
            using (var server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                server.Bind(new IPEndPoint(IPAddress.Any, port));
                server.Listen(1);
                this.socket = server.Accept();
            }

            return receive(1);
        }

        public CalledInfo RequestCall(ResponseData preInfo)
        {
            send("@");
            var resGr = receive(2);
            if (resGr != "gr")
            {
                throw new InvalidOperationException("受信電文 "+ resGr + " は不正な値です");
            }

            send(preInfo.ToSendChars());
            if (preInfo.IsGameSet) return new CalledInfo(); //ゲーム終了時はここで終了

            var res = receive(2);

            return CalledInfo.Parse(res);
        }

        public void NotifyResult(ResponseData result)
        {
            send(result.ToSendChars());
            var resEnd = receive(1);
            if (resEnd != "#")
            {
                throw new InvalidOperationException("受信電文 " + resEnd + " は不正なシンボルです");
            }
        }


        private void send(string message)
        {
            var bytes = LineEncoding.GetBytes(message + "\r\n");
            socket.Send(bytes);
            sentDumper.Dump(bytes);
        }


        private string receive(int expectedLength)
        {
            expectedLength += LineEncoding.GetByteCount("\r\n");

            var received = new List<byte>();
            var buff = new byte[256];

            while (received.Count < expectedLength)
            {
                var len = socket.Receive(buff);
                if (len == 0) continue;

                received.AddRange(buff.Take(len));
            }

            var bytes = received.ToArray();
            recvDumper.Dump(bytes);

            return LineEncoding.GetString(bytes).TrimEnd('\r', '\n');
        }


        public void Dispose()
        {
            if (socket != null)
            {
                socket.Dispose();
                socket = null;
            }
        }
    }
}

using Oika.Apps.CHaserGuiServer.Views;
using Oika.Libs.MeLogg;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oika.Apps.CHaserGuiServer.Line
{
    class LineManager
    {
        readonly Dictionary<bool, string> isCool_teamDic = new Dictionary<bool, string>();

        public string GetTeamName(bool isCool)
        {
            string name;
            return isCool_teamDic.TryGetValue(isCool, out name) ? name : "";
        }

        readonly Dictionary<bool, LineCommunicator> isCool_CommDic = new Dictionary<bool, LineCommunicator>();

        ILogger logger = new TextBoxLogger();

        public event EventHandler<ConnectionChangedEventArgs> ConnectionChanged;

        private void raiseConnectionChanged(bool isCool, ConnectionState state)
        {
            var h = ConnectionChanged;
            if (h != null) h(this, new ConnectionChangedEventArgs(isCool, state));
        }

        /// <summary>
        /// 静的コンストラクタ
        /// </summary>
        static LineManager()
        {
            var logDir = Path.Combine(App.Current.StartUpPath, "dump");
            LogSettings.Update(new LogSetting.Builder("cool", logDir) { MaxFileCount = 10 }.Build());
            LogSettings.Update(new LogSetting.Builder("hot", logDir) { MaxFileCount = 10 }.Build());
        }

        public LineManager(int coolPort, int hotPort)
        {
            isCool_CommDic[true] = new LineCommunicator(coolPort, new Logger("cool", "sent"), new Logger("cool", "recv"));
            isCool_CommDic[false] = new LineCommunicator(hotPort, new Logger("hot", "sent"), new Logger("hot", "recv"));
        }


        public void StartAccept()
        {
            startAccept(isCool_CommDic[true], true);
            startAccept(isCool_CommDic[false], false);
        }

        private void startAccept(LineCommunicator line, bool isCool)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    isCool_teamDic[isCool] = line.Accept();
                    raiseConnectionChanged(isCool, ConnectionState.Connected);
                }
                catch (Exception ex)
                {
                    logger.Fatal(makeLogPrefix(isCool) + ex.ToString());
                    raiseConnectionChanged(isCool, ConnectionState.AbortedWithError);
                }
            });
        }


        public CalledInfo RequestCall(bool isCool, ResponseData preInfo)
        {
            var line = isCool_CommDic[isCool];

            try
            {
                return line.RequestCall(preInfo);
            }
            catch (Exception ex)
            {
                logger.Fatal("{0}メソッド要求失敗：{1}", makeLogPrefix(isCool), ex);
                line.Dispose();
                raiseConnectionChanged(isCool, ConnectionState.Disconnected);

                return new CalledInfo();
            }
        }

        public bool NotifyResult(bool isCool, ResponseData result)
        {
            var line = isCool_CommDic[isCool];

            try
            {
                line.NotifyResult(result);
                return true;
            }
            catch (Exception ex)
            {
                logger.Fatal("{0}実行結果通知異常：", makeLogPrefix(isCool), ex);
                return false;
            }
        }



        private static string makeLogPrefix(bool isCool)
        {
            return isCool ? "[Cool]" : "[Hot]";
        }

    }
}

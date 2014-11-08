using Oika.Libs.MeLogg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace Oika.Apps.CHaserGuiServer.Views
{
    public class TextBoxLogger : ILogger
    {
        static TextBox _target;

        public static void SetTarget(TextBox target)
        {
            _target = target;
        }

        public void Debug(string message, params object[] args)
        {
            return;
        }

        public void Detail(string message, params object[] args)
        {
            log("Detail", string.Format(message, args));
        }

        public void Fatal(string message, params object[] args)
        {
            log("Fatal", string.Format(message, args));
        }

        public void Info(string message, params object[] args)
        {
            log("Info", string.Format(message, args));
        }

        public void Warn(string message, params object[] args)
        {
            log("Warn", string.Format(message, args));
        }

        private void log(string level, string msg)
        {
            var text = string.Format("{0} [{1}]{2}", DateTime.Now.ToString("HH:mm:ss.fff"), level, msg);

            if (_target.CheckAccess())
            {
                _target.AppendText(text);
                _target.AppendText(Environment.NewLine);
                _target.ScrollToEnd();
            }
            else
            {
                _target.Dispatcher.Invoke(new Action(() =>
                {
                    _target.AppendText(text);
                    _target.AppendText(Environment.NewLine);
                    _target.ScrollToEnd();
                }));
            }
        }

        #region 出力可否判定

        public bool LogsDebug
        {
            get
            {
                return false;
            }
        }

        public bool LogsDetail
        {
            get
            {
                return true;
            }
        }

        public bool LogsFatal
        {
            get
            {
                return true;
            }
        }

        public bool LogsInfo
        {
            get
            {
                return true;
            }
        }

        public bool LogsWarn
        {
            get
            {
                return true;
            }
        }

        #endregion


    }
}

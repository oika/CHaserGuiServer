using Oika.Apps.CHaserGuiServer.Line;
using Oika.Apps.CHaserGuiServer.MVVM;
using Oika.Apps.CHaserGuiServer.ViewModels;
using Oika.Libs.MeLogg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Oika.Apps.CHaserGuiServer
{
    class GameState : NotifyObject
    {
        const int CoolPort = 40000;
        const int HotPort = 50000;

        readonly MapPanelViewModel mapContext;
        readonly int turnCount;
        readonly ILogger logger;

        LineManager line;
        bool isCoolConnected;
        bool isHotConnected;

        #region 公開プロパティ

        private int _currentTurn;

        public int CurrentTurn
        {
            get
            {
                return _currentTurn;
            }
            private set
            {
                if (_currentTurn == value) return;
                _currentTurn = value;
                RaisePropertyChanged<GameState, int>(me => me.CurrentTurn);
            }
        }

        private int _hotItemCount;

        public int HotItemCount
        {
            get
            {
                return _hotItemCount;
            }
            private set
            {
                if (_hotItemCount == value) return;
                _hotItemCount = value;
                RaisePropertyChanged<GameState, int>(me => me.HotItemCount);
            }
        }

        private int _coolItemCount;

        public int CoolItemCount
        {
            get
            {
                return _coolItemCount;
            }
            private set
            {
                if (_coolItemCount == value) return;
                _coolItemCount = value;
                RaisePropertyChanged<GameState, int>(me => me.CoolItemCount);
            }
        }

        private string _hotName;

        public string HotName
        {
            get
            {
                return _hotName;
            }
            private set
            {
                if (_hotName == value) return;
                _hotName = value;
                RaisePropertyChanged<GameState, string>(me => me.HotName);
            }
        }

        private string _coolName;

        public string CoolName
        {
            get
            {
                return _coolName;
            }
            private set
            {
                if (_coolName == value) return;
                _coolName = value;
                RaisePropertyChanged<GameState, string>(me => me.CoolName);
            }
        }

        #endregion

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="mapContext"></param>
        /// <param name="turnCount"></param>
        /// <param name="logger"></param>
        public GameState(MapPanelViewModel mapContext, int turnCount, ILogger logger)
        {
            if (mapContext == null) throw new ArgumentNullException("mapContext");

            this.mapContext = mapContext;
            this.turnCount = turnCount;
            this.logger = logger;
        }

        public void Start()
        {
            logger.Info("待受け開始");

            line = new LineManager(CoolPort, HotPort);
            line.ConnectionChanged += line_ConnectionChanged;

            line.StartAccept();
        }

        private void line_ConnectionChanged(object sender, ConnectionChangedEventArgs e)
        {
            if (e.State == ConnectionState.Disconnected)
            {
                logger.Info("{0}接続断", e.IsCool ? "Cool" : "Hot");
                return;
            }

            if (e.State == ConnectionState.Connected)
            {
                if (e.IsCool)
                {
                    this.isCoolConnected = true;
                    this.CoolName = line.GetTeamName(true);
                    logger.Info("Cool接続完了");
                }
                else
                {
                    this.isHotConnected = true;
                    this.HotName = line.GetTeamName(false);
                    logger.Info("Hot接続完了");
                }

                if (isCoolConnected && isHotConnected)
                {
                    Task.Factory.StartNew(() =>
                    {
                        logger.Info("ゲーム開始");
                        beginTurns();
                    });
                }
                return;
            }
        }

        private void beginTurns()
        {
            while (true)
            {
                CurrentTurn++;

                var gameset = false;
                var res = playTurn(true, ref gameset);
                if (!res) return;

                res = playTurn(false, ref gameset);
                if (!res) return;

                if (gameset)
                {
                    logger.Info("ゲーム終了");
                    return;
                }

                if (0 <= turnCount && turnCount <= CurrentTurn) break;
            }

            logger.Info("ゲーム終了：ターンアップ");
        }

        private bool playTurn(bool isCool, ref bool isGameSet)
        {
            if (!isGameSet) isGameSet = mapContext.IsGameSet();

            var preInfo = mapContext.GetAroundInfo(isCool);
            if (isGameSet)
            {
                line.RequestCall(isCool, new ResponseData(true, preInfo));
                return true;
            }

            var callInfo = line.RequestCall(isCool, new ResponseData(false, preInfo));
            if (callInfo.Method == MethodKind.Unknown)
            {
                logger.Warn("異常終了：" + (isCool ? "Cool" : "Hot"));
                return false;
            }

            var res = mapContext.InvokeCall(isCool, callInfo.Method, callInfo.Direction);
            isGameSet = mapContext.IsGameSet();

            if (!line.NotifyResult(isCool, new ResponseData(isGameSet, res)))
            {
                logger.Warn("異常終了：" + (isCool ? "Cool" : "Hot"));
                return false;
            }

            //アイテム数更新
            if (isCool)
            {
                this.CoolItemCount = mapContext.GetItemCount(isCool);
            }
            else
            {
                this.HotItemCount = mapContext.GetItemCount(isCool);
            }

            return true;
        }
    }
}

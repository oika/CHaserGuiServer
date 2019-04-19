using Oika.Apps.CHaserGuiServer.Line;
using Oika.Apps.CHaserGuiServer.MVVM;
using Oika.Apps.CHaserGuiServer.ViewModels;
using Oika.Libs.MeLogg;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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

        readonly GameSoundPlayer coolSoundPlayer;
        readonly GameSoundPlayer hotSoundPlayer;

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

            //サウンド種別決定（とりあえずランダムに決めることにする）
            var clients = GameSoundPlayer.EnumerateClientNames().ToArray();
            if (clients.Length == 0)
            {
                coolSoundPlayer = new GameSoundPlayer("");
                hotSoundPlayer = new GameSoundPlayer("");
            }
            else if (clients.Length == 1)
            {
                coolSoundPlayer = new GameSoundPlayer(clients[0]);
                hotSoundPlayer = new GameSoundPlayer(clients[0]);
            }
            else
            {
                var rand = new Random();

                var idxCool = rand.Next(clients.Length);
                int idxHot;
                while (true)
                {
                    idxHot = rand.Next(clients.Length);
                    if (idxCool != idxHot) break;
                }

                coolSoundPlayer = new GameSoundPlayer(clients[idxCool]);
                hotSoundPlayer = new GameSoundPlayer(clients[idxHot]);
            }
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

                        coolSoundPlayer.PlayGameStart();

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

                GameResultKind result;

                var res = playTurn(true, out result);
                if (!res) return;
                if (result != GameResultKind.Continue)
                {
                    logger.Info("ゲーム終了：" + result.ToName());
                    notifyGameEnd(false);
                    playGameSetSound(result);
                    return;
                }

                res = playTurn(false, out result);
                if (!res) return;
                if (result != GameResultKind.Continue)
                {
                    logger.Info("ゲーム終了：" + result.ToName());
                    notifyGameEnd(true);
                    playGameSetSound(result);
                    return;
                }

                if (0 <= turnCount && turnCount <= CurrentTurn) break;
            }

            logger.Info("ゲーム終了：ターンアップ");
            notifyGameEnd(true);
            notifyGameEnd(false);
            playGameSetSound(GameResultKind.Draw);
        }



        private void playGameSetSound(GameResultKind result)
        {
            Thread.Sleep(100);

            if (result == GameResultKind.CoolWon)
            {
                hotSoundPlayer.PlayLose();
            }
            else if (result == GameResultKind.HotWon)
            {
                coolSoundPlayer.PlayLose();
            }

            coolSoundPlayer.PlayGameSet();

            if (result == GameResultKind.CoolWon)
            {
                coolSoundPlayer.PlayWin();
            }
            else if (result == GameResultKind.HotWon)
            {
                hotSoundPlayer.PlayWin();
            }
        }

        private void notifyGameEnd(bool isCool)
        {
            var around = mapContext.GetAroundInfo(isCool);
            line.RequestCall(isCool, new ResponseData(true, around, isCool));
        }

        private bool playTurn(bool isCool, out GameResultKind result)
        {
            result = mapContext.GetResult();
            Debug.Assert(result == GameResultKind.Continue);    //この時点では終了していないこと

            var preInfo = mapContext.GetAroundInfo(isCool);

            var callInfo = line.RequestCall(isCool, new ResponseData(false, preInfo, isCool));
            if (callInfo.Method == MethodKind.Unknown)
            {
                logger.Warn("異常終了：" + (isCool ? "Cool" : "Hot"));
                return false;
            }

            //音
            var soundPlayer = isCool ? coolSoundPlayer : hotSoundPlayer;
            soundPlayer.Play(callInfo.Method);

            var res = mapContext.InvokeCall(isCool, callInfo.Method, callInfo.Direction);
            result = mapContext.GetResult();

            var gameSet = result != GameResultKind.Continue;
            if (!line.NotifyResult(isCool, new ResponseData(gameSet, res, isCool)))
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

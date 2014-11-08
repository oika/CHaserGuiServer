using Oika.Apps.CHaserGuiServer.Messages;
using Oika.Apps.CHaserGuiServer.MVVM;
using Oika.Apps.CHaserGuiServer.Views;
using Oika.Libs.MeLogg;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Oika.Apps.CHaserGuiServer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        readonly ILogger viewLogger = new TextBoxLogger();

        #region コマンドと有効状態

        private bool _isAcceptStarted = false;

        public bool IsAcceptStarted {
            get {
                return _isAcceptStarted;
            }
            private set {
                if (_isAcceptStarted == value) return;
                _isAcceptStarted = value;
                RaisePropertyChanged<MainWindowViewModel, bool>(me => me.IsAcceptStarted);
            }
        }

        public ICommand SelectMapFileCommand { get; private set; }

        public ICommand BeginAcceptCommand { get; private set; }

        public bool CanStartAccept
        {
            get
            {
                return !string.IsNullOrWhiteSpace(MapFilePath);
            }
        }

        #endregion

        #region 公開プロパティ

        private MapPanelViewModel _mapPanelContext;

        public MapPanelViewModel MapPanelContext
        {
            get
            {
                return _mapPanelContext;
            }
            private set
            {
                if (object.ReferenceEquals(_mapPanelContext, value)) return;
                _mapPanelContext = value;
                RaisePropertyChanged<MainWindowViewModel, MapPanelViewModel>(me => me.MapPanelContext);
            }
        }

        private string _mapFilePath = "";

        public string MapFilePath
        {
            get
            {
                return _mapFilePath;
            }
            set
            {
                if (_mapFilePath == value) return;
                _mapFilePath = value;
                RaisePropertyChanged<MainWindowViewModel, string>(me => me.MapFilePath);

                RaisePropertyChanged<MainWindowViewModel, bool>(me => me.CanStartAccept);
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
                RaisePropertyChanged<MainWindowViewModel, string>(me => me.CoolName);
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
                RaisePropertyChanged<MainWindowViewModel, string>(me => me.HotName);
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
                RaisePropertyChanged<MainWindowViewModel, int>(me => me.CoolItemCount);
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
                RaisePropertyChanged<MainWindowViewModel, int>(me => me.HotItemCount);
            }
        }

        private int _currentTurnCount;

        public int CurrentTurnCount
        {
            get
            {
                return _currentTurnCount;
            }
            private set
            {
                if (_currentTurnCount == value) return;
                _currentTurnCount = value;
                RaisePropertyChanged<MainWindowViewModel, int>(me => me.CurrentTurnCount);
            }
        }

        #endregion


        public MainWindowViewModel()
        {
            this.SelectMapFileCommand = new CommonCommand(selectMapFileExecute);
            this.BeginAcceptCommand = new CommonCommand(beginAcceptExecute);
        }

        #region マップ選択コマンド処理

        /// <summary>
        /// SelectMapFileコマンド実行メソッド
        /// </summary>
        private void selectMapFileExecute()
        {
            var msg = new OpenFileDialogMessage();
            msg.AddFilter("mapファイル(*.map)", new[] { "*.map" });
            msg.AddFilter("すべてのファイル(*.*)", new[] { "*.*" });
            Messenger.Instance.Send<OpenFileDialogMessage, MainWindow>(msg);

            if (msg.Result == true)
            {
                this.MapFilePath = msg.FileName;
            }
        }

        #endregion

        #region 待受け開始コマンド処理

        GameState state;

        /// <summary>
        /// BeginAcceptコマンド実行メソッド
        /// </summary>
        private void beginAcceptExecute()
        {
            //マップ読み込み
            var info = MapFileInfo.Load(this.MapFilePath, viewLogger);
            if (info == null) return;

            this.MapPanelContext = new MapPanelViewModel(info);
            viewLogger.Info("map[{0}]読込み完了", info.Name);

            Debug.Assert(state == null);

            state = new GameState(this.MapPanelContext, info.TurnCount, viewLogger);
            state.RegisterPropertyChangedHandler(s => s.HotName, (s, e) => this.HotName = (s as GameState).HotName);
            state.RegisterPropertyChangedHandler(s => s.CoolName, (s, e) => this.CoolName = (s as GameState).CoolName);
            state.RegisterPropertyChangedHandler(s => s.HotItemCount, (s, e) => this.HotItemCount = (s as GameState).HotItemCount);
            state.RegisterPropertyChangedHandler(s => s.CoolItemCount, (s, e) => this.CoolItemCount = (s as GameState).CoolItemCount);
            state.RegisterPropertyChangedHandler(s => s.CurrentTurn, (s, e) => this.CurrentTurnCount = (s as GameState).CurrentTurn);
            state.Start();

            IsAcceptStarted = true;
        }

        #endregion

    }
}

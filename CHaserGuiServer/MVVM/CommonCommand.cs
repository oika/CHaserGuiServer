﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Oika.Apps.CHaserGuiServer.MVVM
{
    /// <summary>
    /// 実行時にパラメータをとらない<see cref="System.Windows.Input.ICommand"/>の実装クラスです。
    /// </summary>
    public class CommonCommand : ICommand
    {

        readonly Action _action;

        /// <summary>
        /// コマンドの実行可否状態に変更があったときに発生します。
        /// </summary>
        public event EventHandler CanExecuteChanged;

        #region コンストラクタ

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="executeAction">コマンドの実処理を指定します。</param>
        public CommonCommand(Action executeAction)
        {
            this._action = executeAction;
        }

        #endregion

        /// <summary>
        /// CanExecuteChangedイベントを発生します。
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            var changed = this.CanExecuteChanged;

            if (changed != null) changed(this, EventArgs.Empty);
        }

        #region ICommandの明示的実装

        bool ICommand.CanExecute(object parameter)
        {
            return true;
        }

        void ICommand.Execute(object parameter)
        {
            this._action();
        }

        #endregion

        /// <summary>
        /// 現在の状態でこのコマンドが実行できるかどうかを判断します。
        /// </summary>
        /// <returns></returns>
        public bool CanExecute()
        {
            return (this as ICommand).CanExecute(null);
        }

        /// <summary>
        /// コマンドを実行します。
        /// </summary>
        public void Execute()
        {
            (this as ICommand).Execute(null);
        }
    }
}

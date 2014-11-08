using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Oika.Apps.CHaserGuiServer.MVVM
{
    /// <summary>
    /// <see cref="System.Windows.Input.ICommand"/>の共通の実装クラスです。
    /// </summary>
    /// <typeparam name="T">パラメータ型。</typeparam>
    public class CommonCommand<T> : ICommand
    {

        readonly Action<T> _action;

        /// <summary>
        /// コマンドの実行可否状態に変更があったときに発生します。
        /// </summary>
        public event EventHandler CanExecuteChanged;

        #region コンストラクタ

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="executeAction">コマンドの実処理を指定します。</param>
        public CommonCommand(Action<T> executeAction)
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
            this._action((T)parameter);
        }

        #endregion

        /// <summary>
        /// 現在の状態でこのコマンドが実行できるかどうかを判断します。
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecute(T parameter)
        {
            return (this as ICommand).CanExecute(parameter);
        }

        /// <summary>
        /// コマンドを実行します。
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(T parameter)
        {
            (this as ICommand).Execute(parameter);
        }

    }
}

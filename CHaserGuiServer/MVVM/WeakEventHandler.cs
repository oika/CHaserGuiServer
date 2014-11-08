using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Oika.Apps.CHaserGuiServer.MVVM
{
    /// <summary>
    /// イベントハンドラを弱参照で保持するインタフェースです。
    /// </summary>
    public interface IWeakEventHandler
    {
        /// <summary>
        /// このハンドラの参照が生きているかどうかを取得します。
        /// </summary>
        bool IsAlive { get; }
        /// <summary>
        /// ハンドラの参照を削除します。
        /// </summary>
        void ClearReference();
        /// <summary>
        /// ハンドラへの参照が生きていればハンドラメソッドを呼び出します。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns>ハンドラへの参照が生きていなかった場合はFalseを返します。</returns>
        bool RaiseIfAlive(object sender, EventArgs e);
        /// <summary>
        /// 指定したデリゲートがこのオブジェクトの保持するハンドラと等しいかどうか調べます。
        /// このオブジェクトのハンドラへの参照が生きていない場合は
        /// 常にFalseを返します。
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        bool DelegateEquals(Delegate target);
    }



    /// <summary>
    /// イベントハンドラを弱参照で保持するためのクラスです。
    /// </summary>
    /// <typeparam name="TEventArgs"></typeparam>
    public class WeakEventHandler<TEventArgs> : IWeakEventHandler where TEventArgs : EventArgs
    {
        /// <summary>
        /// ハンドラの対象オブジェクトへの弱参照
        /// </summary>
        WeakReference _targetReference;
        /// <summary>
        /// ハンドラのメソッド情報を保持
        /// </summary>
        readonly MethodInfo _handleMethod;
        /// <summary>
        /// ハンドラのメソッド情報を取得します。
        /// </summary>
        public MethodInfo HandleMethod
        {
            get
            {
                return _handleMethod;
            }
        }
        /// <summary>
        /// このハンドラの参照が生きているかどうかを取得します。
        /// </summary>
        public bool IsAlive
        {
            get
            {
                var trg = _targetReference;
                if (trg == null) return false;

                if (HandleMethod.IsStatic) return true;

                return trg.IsAlive;
            }
        }

        /// <summary>
        /// 指定したデリゲートがこのオブジェクトの保持するハンドラと等しいかどうか調べます。
        /// このオブジェクトのハンドラへの参照が生きていない場合は
        /// 常にFalseを返します。
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool DelegateEquals(Delegate target)
        {
            if (target == null) return false;

            if (this.HandleMethod != target.Method) return false;

            var trgRef = _targetReference;
            if (trgRef == null) return false;

            //静的メソッドならインスタンス比較なしで終了
            if (this.HandleMethod.IsStatic) return true;

            var myTrgObj = trgRef.Target;
            if (myTrgObj == null) return false;

            return object.ReferenceEquals(myTrgObj, target.Target);
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="handler"></param>
        public WeakEventHandler(EventHandler<TEventArgs> handler)
            : this(handler.Target, handler.Method)
        {
        }
        /// <summary>
        /// プロテクトコンストラクタです。
        /// </summary>
        /// <param name="target"></param>
        /// <param name="handleMethod"></param>
        protected WeakEventHandler(object target, MethodInfo handleMethod)
        {
            this._targetReference = new WeakReference(target);
            this._handleMethod = handleMethod;
        }

        /// <summary>
        /// ハンドラの参照を削除します。
        /// </summary>
        public void ClearReference()
        {
            //staticなメソッドの場合は最初からTargetプロパティがnullなので
            //常にWeakReferenceオブジェクト自体への参照を削除しておく
            _targetReference = null;
        }



        /// <summary>
        /// ハンドラへの参照が生きていればハンドラメソッドを呼び出します。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns>ハンドラへの参照が生きていなかった場合はFalseを返します。</returns>
        public bool RaiseIfAlive(object sender, TEventArgs e)
        {
            return ((IWeakEventHandler)this).RaiseIfAlive(sender, e);
        }

        /// <summary>
        /// ハンドラへの参照が生きていればハンドラメソッドを呼び出します。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns>ハンドラへの参照が生きていなかった場合はFalseを返します。</returns>
        bool IWeakEventHandler.RaiseIfAlive(object sender, EventArgs e)
        {
            var trgRef = _targetReference;
            if (trgRef == null) return false;

            object trgObj;

            if (HandleMethod.IsStatic)
            {
                trgObj = null;
            }
            else
            {
                trgObj = trgRef.Target;
                if (trgObj == null) return false;
            }

            HandleMethod.Invoke(trgObj, new object[] { sender, e });
            return true;
        }
    }

    /// <summary>
    /// イベントハンドラを弱参照で利用するためのクラスです。
    /// </summary>
    public class WeakEventHandler : WeakEventHandler<EventArgs>
    {
        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="handler"></param>
        public WeakEventHandler(EventHandler handler)
            : base(handler.Target, handler.Method)
        {
        }
    }
}

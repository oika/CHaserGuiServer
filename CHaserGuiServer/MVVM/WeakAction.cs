using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Oika.Apps.CHaserGuiServer.MVVM
{
    /// <summary>
    /// <see cref="System.Action{T}"/>を弱参照で利用するクラスのインタフェースです。
    /// </summary>
    internal interface IWeakAction
    {
        /// <summary>
        /// 指定したパラメータを使ってメソッドを実行します。
        /// </summary>
        /// <param name="parameter"></param>
        void ExecuteWithObject(object parameter);
        /// <summary>
        /// このオブジェクトの管理するメソッド名を取得します。
        /// </summary>
        string MethodName { get; }
        /// <summary>
        /// メソッドのインスタンスを取得します。
        /// </summary>
        object Target { get; }
        /// <summary>
        /// このオブジェクトの管理するメソッドのインスタンスへの参照が
        /// 生きているかどうかを取得します。
        /// </summary>
        bool IsAlive { get; }
        /// <summary>
        /// このオブジェクトの管理するメソッドのインスタンスへの参照を削除します。
        /// </summary>
        void Clear();
    }

    /// <summary>
    /// <see cref="System.Action{T}"/>を弱参照で利用するクラスです。
    /// </summary>
    /// <typeparam name="T">メソッドのパラメータ型です。</typeparam>
    internal class WeakAction<T> : IWeakAction
    {
        /// <summary>
        /// メソッドがstaticかどうか
        /// </summary>
        readonly bool isStatic;
        /// <summary>
        /// メソッド
        /// </summary>
        MethodInfo method;
        /// <summary>
        /// メソッドのターゲットインスタンスへの参照
        /// </summary>
        WeakReference reference;

        /// <summary>
        /// このオブジェクトの管理するメソッド名を取得します。
        /// </summary>
        public string MethodName
        {
            get
            {
                return method == null ? null : method.Name;
            }
        }

        /// <summary>
        /// メソッドのインスタンスを取得します。
        /// </summary>
        public object Target
        {
            get
            {
                if (reference == null) return null;
                return reference.Target;
            }
        }

        /// <summary>
        /// このオブジェクトの管理するメソッドのインスタンスへの参照が
        /// 生きているかどうかを取得します。
        /// </summary>
        public bool IsAlive
        {
            get
            {
                if (method == null) return false;

                if (isStatic)
                {
                    if (reference == null) return true;
                    return reference.IsAlive;
                }

                if (reference == null) return false;
                return reference.IsAlive;
            }
        }


        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="action">管理対象のアクションを指定します。</param>
        /// <param name="target">actionパラメータにstaticなメソッドを指定する場合に、
        /// オブジェクトの生存期間を管理するためのインスタンスを指定します。</param>
        public WeakAction(Action<T> action, object target)
        {
            this.method = action.Method;

            if (action.Method.IsStatic)
            {
                this.isStatic = true;
                if (target != null) this.reference = new WeakReference(target);

            }
            else
            {
                this.isStatic = false;
                reference = new WeakReference(action.Target);
            }
        }

        /// <summary>
        /// 指定したパラメータを使ってメソッドを実行します。
        /// </summary>
        /// <param name="parameter">メソッドのパラメータを指定します。</param>
        public void ExecuteWithObject(object parameter)
        {
            Execute((T)parameter);
        }

        /// <summary>
        /// メソッドを実行します。
        /// </summary>
        /// <param name="parameter">メソッドのパラメータを指定します。</param>
        public void Execute(T parameter)
        {
            if (!IsAlive) return;

            if (isStatic)
            {
                method.Invoke(null, new object[] { parameter });
                return;
            }

            method.Invoke(reference.Target, new object[] { parameter });
        }

        /// <summary>
        /// このオブジェクトの管理するメソッドのインスタンスへの参照を削除します。
        /// </summary>
        public void Clear()
        {
            this.reference = null;
            this.method = null;
        }


    }
}

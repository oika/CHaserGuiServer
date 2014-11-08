using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows.Threading;

namespace Oika.Apps.CHaserGuiServer.MVVM
{
    /// <summary>
    /// プロパティ変更イベントハンドラの弱参照クラスです。
    /// </summary>
    class WeakPropertyChangedHandler : WeakEventHandler<PropertyChangedEventArgs>
    {
        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="handler"></param>
        internal WeakPropertyChangedHandler(PropertyChangedEventHandler handler)
            : base(handler.Target, handler.Method)
        {
        }
    }


    /// <summary>
    /// プロパティ名をキーにプロパティ変更ハンドラを管理するスレッドセーフなコレクションクラスです。
    /// </summary>
    class PropertyChangedHandlerTable
    {
        /// <summary>
        /// プロパティ変更イベントハンドラをプロパティ名をキーに保持
        /// </summary>
        readonly Dictionary<string, List<WeakPropertyChangedHandler>> handlerDic
                                    = new Dictionary<string, List<WeakPropertyChangedHandler>>();

        /// <summary>
        /// ハンドラを登録します。
        /// </summary>
        /// <param name="propName"></param>
        /// <param name="handler"></param>
        public void Add(string propName, PropertyChangedEventHandler handler)
        {

            lock (handlerDic)
            {
                if (!handlerDic.ContainsKey(propName)) handlerDic.Add(propName, new List<WeakPropertyChangedHandler>());

                handlerDic[propName].Add(new WeakPropertyChangedHandler(handler));
            }

            requestCleanUp();
        }

        /// <summary>
        /// 指定したハンドラに一致する最初のものをリストから削除します。
        /// </summary>
        /// <param name="propName"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public bool Remove(string propName, PropertyChangedEventHandler handler)
        {

            lock (handlerDic)
            {
                List<WeakPropertyChangedHandler> list;
                if (!handlerDic.TryGetValue(propName, out list)) return false;

                var target = list.FirstOrDefault(a => a.DelegateEquals(handler));
                if (target == null) return false;

                target.ClearReference();
            }

            requestCleanUp();
            return true;
        }

        /// <summary>
        /// リスト掃除をディスパッチャに予約
        /// </summary>
        private void requestCleanUp()
        {
            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
            {
                cleanUp();
            }), DispatcherPriority.ApplicationIdle);
        }

        /// <summary>
        /// リスト掃除
        /// </summary>
        private void cleanUp()
        {

            lock (handlerDic)
            {

                var rmNameList = new List<string>();

                foreach (var item in handlerDic)
                {
                    var list = item.Value;

                    var rmIdxs = list.Select((h, i) => new { Idx = i, Handler = h })
                                     .Where(a => !a.Handler.IsAlive)
                                     .Select(a => a.Idx)
                                     .Reverse()
                                     .ToList();
                    rmIdxs.ForEach(i => list.RemoveAt(i));

                    if (!list.Any()) rmNameList.Add(item.Key);
                }

                rmNameList.ForEach(t => handlerDic.Remove(t));
            }
        }

        /// <summary>
        /// 指定したプロパティ名に対応する購読中ハンドラの一覧を取得します。
        /// </summary>
        /// <param name="propName"></param>
        /// <returns></returns>
        public WeakEventHandler<PropertyChangedEventArgs>[] GetHandlers(string propName)
        {

            lock (handlerDic)
            {
                List<WeakPropertyChangedHandler> list;
                handlerDic.TryGetValue(propName, out list);

                if (list == null) return new WeakEventHandler<PropertyChangedEventArgs>[] { };

                return list.Where(h => h.IsAlive).ToArray();
            }
        }
    }

    public static class NotifyObjectExt
    {
        /// <summary>
        /// プロパティ変更イベントハンドラを登録します。
        /// </summary>
        /// <typeparam name="TObj"></typeparam>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="vm"></param>
        /// <param name="propertyName"></param>
        /// <param name="handler"></param>
        public static void RegisterPropertyChangedHandler<TObj, TProp>(this TObj vm,
                                                                       Expression<Func<TObj, TProp>> propertyName,
                                                                       PropertyChangedEventHandler handler)
                                                                        where TObj : NotifyObject
        {

            string pName = ((MemberExpression)propertyName.Body).Member.Name;
            vm.RegisterPropertyChangedHandler(pName, handler);
        }


        /// <summary>
        /// 登録済みのプロパティ変更イベントハンドラを削除します。
        /// </summary>
        /// <typeparam name="TObj"></typeparam>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="vm"></param>
        /// <param name="propertyName"></param>
        /// <param name="handler"></param>
        /// <returns>指定されたハンドラが登録されていない場合はFalseを返します。</returns>
        public static bool RemovePropertyChangedHandler<TObj, TProp>(this TObj vm,
                                                                     Expression<Func<TObj, TProp>> propertyName,
                                                                     PropertyChangedEventHandler handler)
                                                                        where TObj : NotifyObject
        {

            string pName = ((MemberExpression)propertyName.Body).Member.Name;
            return vm.RemovePropertyChangedHandler(pName, handler);
        }
    }

    public class NotifyObject : INotifyPropertyChanged
    {
        /// <summary>
        /// ハンドラリスト
        /// </summary>
        readonly PropertyChangedHandlerTable handlerTable = new PropertyChangedHandlerTable();

        /// <summary>
        /// プロパティに変更があった場合に発生するイベントです。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// プロパティ変更イベントを発生させます。
        /// </summary>
        /// <param name="propertyName">プロパティ名</param>
        protected void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        /// <summary>
        /// プロパティ変更イベントを発生させます。
        /// </summary>
        /// <typeparam name="TObj"></typeparam>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="propertyName"></param>
        protected void RaisePropertyChanged<TObj, TProp>(Expression<Func<TObj, TProp>> propertyName) where TObj : INotifyPropertyChanged
        {
            string pName = ((MemberExpression)propertyName.Body).Member.Name;
            RaisePropertyChanged(pName);
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        protected NotifyObject()
        {
            this.PropertyChanged += onPropertyChanged;
        }

        /// <summary>
        /// プロパティ変更イベントハンドラを弱参照で登録します。
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="handler"></param>
        public void RegisterPropertyChangedHandler(string propertyName, PropertyChangedEventHandler handler)
        {
            handlerTable.Add(propertyName, handler);
        }

        /// <summary>
        /// 登録済みのプロパティ変更イベントハンドラを削除します。
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="handler"></param>
        /// <returns>指定されたハンドラが登録されていない場合はFalseを返します。</returns>
        public bool RemovePropertyChangedHandler(string propertyName, PropertyChangedEventHandler handler)
        {
            return handlerTable.Remove(propertyName, handler);
        }

        /// <summary>
        /// プロパティ変更時ハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onPropertyChanged(object sender, PropertyChangedEventArgs e)
        {

            foreach (var handler in handlerTable.GetHandlers(e.PropertyName))
            {
                handler.RaiseIfAlive(sender, e);
            }
        }

    }
}

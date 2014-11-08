using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace Oika.Apps.CHaserGuiServer.MVVM
{
    /// <summary>
    /// メッセンジャークラスです。
    /// </summary>
    public class Messenger
    {

        #region シングルトン実装関係

        /// <summary>
        /// このクラスの唯一のインスタンスを保持
        /// </summary>
        static readonly Messenger _instance = new Messenger();
        /// <summary>
        /// このクラスの唯一のインスタンスを取得します。
        /// </summary>
        public static Messenger Instance
        {
            get
            {
                return _instance;
            }
        }
        /// <summary>
        /// 静的コンストラクタ
        /// </summary>
        static Messenger()
        {
        }
        /// <summary>
        /// プライベート・コンストラクタ
        /// </summary>
        private Messenger()
        {
        }

        #endregion

        /// <summary>
        /// メッセージ型とアクションリストの対応リスト
        /// </summary>
        readonly TypedActionList actionList = new TypedActionList();

        #region メッセージの登録

        /// <summary>
        /// メッセージを登録します。
        /// </summary>
        /// <typeparam name="TMessage">メッセージの型</typeparam>
        /// <param name="recipient">メッセージを受け取るオブジェクト</param>
        /// <param name="action">実行するアクション</param>
        /// <param name="token">メッセージを一意にするためのトークン</param>
        public void Register<TMessage>(object recipient, Action<TMessage> action, object token = null) where TMessage : IMessage
        {

            Type msgType = typeof(TMessage);

            var act = new ActionAndToken
            {
                ActionSet = new WeakAction<TMessage>(action, recipient),
                Token = token,
            };

            actionList.Add(msgType, act);
        }

        #endregion

        #region メッセージの送信

        /// <summary>
        /// メッセージを送信します。
        /// </summary>
        /// <typeparam name="TMessage">メッセージの型</typeparam>
        /// <typeparam name="TTarget">メッセージ受信者の型</typeparam>
        /// <param name="message">送信メッセージ</param>
        public void Send<TMessage, TTarget>(TMessage message) where TMessage : IMessage
        {
            sendToTargetOrType(message, typeof(TTarget), null);
        }

        /// <summary>
        /// メッセージを送信します。
        /// </summary>
        /// <typeparam name="TMessage">メッセージの型</typeparam>
        /// <param name="message">送信メッセージ</param>
        /// <param name="token">メッセージ受信者を一意に識別するためのトークン</param>
        public void Send<TMessage>(TMessage message, object token) where TMessage : IMessage
        {
            sendToTargetOrType(message, null, token);
        }

        /// <summary>
        /// メッセージを送信します。
        /// </summary>
        /// <typeparam name="TMessage">メッセージの型</typeparam>
        /// <typeparam name="TTarget">メッセージ受信者の型</typeparam>
        /// <param name="message">送信メッセージ</param>
        /// <param name="token">メッセージ受信者を一意に識別するためのトークン</param>
        public void Send<TMessage, TTarget>(TMessage message, object token) where TMessage : IMessage
        {
            sendToTargetOrType(message, typeof(TTarget), token);
        }

        /// <summary>
        /// 受信者の型を指定してメッセージを送信します。
        /// </summary>
        /// <typeparam name="TMessage">メッセージの型</typeparam>
        /// <param name="message">送信メッセージ</param>
        /// <param name="targetType">メッセージ受信者の型</param>
        public void SendToType<TMessage>(TMessage message, Type targetType) where TMessage : IMessage
        {
            sendToTargetOrType(message, targetType, null);
        }


        /// <summary>
        /// メッセージ送信の実メソッド
        /// </summary>
        /// <typeparam name="TMessage">メッセージの型</typeparam>
        /// <param name="message">送信メッセージ</param>
        /// <param name="messageTargetType">メッセージ受信者の型</param>
        /// <param name="token">メッセージを一意にするためのトークン</param>
        private void sendToTargetOrType<TMessage>(TMessage message, Type messageTargetType, object token)
        {
            Type messageType = typeof(TMessage);

            var listClone = actionList.GetActions(messageType);
            if (listClone == null) return;  //登録された受信者がいなければ終了

            foreach (var action in enumerateActions(listClone, messageTargetType, token))
            {
                //ターゲットのスレッドからアクション実行
                var uiElm = action.Target as FrameworkElement;
                if (uiElm == null || uiElm.CheckAccess())
                {
                    action.ExecuteWithObject(message);

                }
                else
                {
                    uiElm.Dispatcher.Invoke(new Action(() =>
                    {
                        action.ExecuteWithObject(message);
                    }));
                }
            }
        }


        /// <summary>
        /// 登録済みアクションリストから条件に一致するアクションを列挙する
        /// </summary>
        /// <param name="list"></param>
        /// <param name="msgTargetType"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private static IEnumerable<IWeakAction> enumerateActions(ActionAndToken[] list, Type msgTargetType, object token)
        {
            foreach (var item in list)
            {
                var action = item.ActionSet;

                if (action == null || !action.IsAlive) continue;

                //アクションのターゲット型が一致しないものは無視
                if (action.Target != null && msgTargetType != null)
                {
                    var trgType = action.Target.GetType();
                    //※ターゲット型はサブクラスも対象とする
                    if (trgType != msgTargetType && !msgTargetType.IsAssignableFrom(trgType)) continue;
                }
                //トークンの一致しないものは無視
                if (item.Token != null || token != null)
                {
                    if (item.Token == null || !item.Token.Equals(token)) continue;
                }

                yield return action;
            }
        }


        #endregion

        #region メッセージの削除

        /// <summary>
        /// 指定したメッセージ受信者に対するメッセージの登録を削除します。
        /// </summary>
        /// <param name="recipient">削除対象のメッセージ受信者</param>
        public void Unregister(object recipient)
        {
            if (recipient == null) return;

            actionList.RemoveWithTarget(recipient);
        }


        /// <summary>
        /// 指定した条件に一致するメッセージ登録を削除します。
        /// </summary>
        /// <typeparam name="TMessage">削除対象のメッセージ型</typeparam>
        /// <param name="recipient">削除対象のメッセージ受信者</param>
        /// <param name="token">メッセージを一意にするためのトークン（指定しない場合はnull）</param>
        /// <param name="action">削除対象のアクション（指定しない場合はnull）</param>
        public void Unregister<TMessage>(object recipient, object token = null, Action<TMessage> action = null)
        {

            actionList.Remove(typeof(TMessage), recipient, token, action == null ? null : action.Method.Name);
        }

        #endregion

        /// <summary>
        /// 受信者の参照がなくなったメッセージのクリーンアップを
        /// ただちに実行します。
        /// </summary>
        /// <remarks>
        /// メッセージリストのクリーンアップは、通常、メッセージの登録および
        /// 削除実行後に自動で実行されるため、このメソッドを手動で呼び出す必要はありません。
        /// リソースを即座に解放する必要のある場合のみ、このメソッドを使用します。
        /// </remarks>
        public void CleanUpImmediately()
        {
            actionList.CleanUp();
        }

        /// <summary>
        /// アクションとトークンを持つ構造体
        /// </summary>
        private struct ActionAndToken
        {
            /// <summary>
            /// アクション
            /// </summary>
            public IWeakAction ActionSet;
            /// <summary>
            /// トークン
            /// </summary>
            public object Token;
        }



        /// <summary>
        /// メッセージ型と対応するアクションのコレクションクラス
        /// </summary>
        private class TypedActionList
        {
            /// <summary>
            /// メッセージ型との対応でアクションを保持
            /// </summary>
            readonly Dictionary<Type, List<ActionAndToken>> typeActDic = new Dictionary<Type, List<ActionAndToken>>();

            /// <summary>
            /// アクションをリストに追加します。
            /// </summary>
            /// <param name="msgType"></param>
            /// <param name="actionAndToken"></param>
            public void Add(Type msgType, ActionAndToken actionAndToken)
            {
                lock (typeActDic)
                {
                    if (!typeActDic.ContainsKey(msgType)) typeActDic[msgType] = new List<ActionAndToken>();

                    typeActDic[msgType].Add(actionAndToken);
                }
                requestCleanUp();
            }

            /// <summary>
            /// 指定したメッセージ型に対応するアクションの一覧を取得します。
            /// 指定した型に一致する登録済みアクションがない場合はNullを返します。
            /// </summary>
            /// <param name="msgType"></param>
            /// <returns></returns>
            public ActionAndToken[] GetActions(Type msgType)
            {
                lock (typeActDic)
                {
                    List<ActionAndToken> list;
                    if (!typeActDic.TryGetValue(msgType, out list)) return null;

                    return list.ToArray();
                }
            }

            /// <summary>
            /// 指定したアクションターゲットに対する登録を全て削除します。
            /// </summary>
            /// <param name="target"></param>
            public void RemoveWithTarget(object target)
            {

                lock (typeActDic)
                {

                    foreach (var actList in typeActDic.Values)
                    {

                        for (int i = 0; i < actList.Count; i++)
                        {
                            var act = actList[i].ActionSet;

                            if (act == null || !act.IsAlive) continue;   //アクションが削除済みのものは無視（処理後にCleanUpで削除）

                            if (target.Equals(act.Target)) act.Clear();
                        }
                    }
                }
                requestCleanUp();
            }


            /// <summary>
            /// 指定した条件に一致するアクションの登録を削除します。
            /// </summary>
            /// <param name="msgType"></param>
            /// <param name="target"></param>
            /// <param name="token"></param>
            /// <param name="methodName"></param>
            public void Remove(Type msgType, object target, object token, string methodName)
            {

                lock (typeActDic)
                {
                    List<ActionAndToken> actList;
                    if (!typeActDic.TryGetValue(msgType, out actList)) return;

                    for (int i = 0; i < actList.Count; i++)
                    {

                        var actset = actList[i].ActionSet;

                        if (actset == null || !actset.IsAlive) continue;   //アクションが削除済みのものは無視（処理後にCleanUpで削除）

                        if (target != null && !target.Equals(actset.Target)) continue; //受信者の一致確認
                        if (methodName != null && methodName != actset.MethodName) continue; //メソッド名の一致確認
                        if (token != null && !token.Equals(actList[i].Token)) continue;   //トークンの一致確認

                        actset.Clear();
                    }
                }
                requestCleanUp();
            }

            /// <summary>
            /// クリーンアップ要求済みフラグ
            /// </summary>
            bool isCleanUpRequested;

            /// <summary>
            /// クリーンアップ実行を予約
            /// </summary>
            private void requestCleanUp()
            {
                if (isCleanUpRequested) return;
                isCleanUpRequested = true;

                var act = new Action(() =>
                {
                    try
                    {
                        CleanUp();

                    }
                    finally
                    {
                        isCleanUpRequested = false;
                    }
                });

                Dispatcher.CurrentDispatcher.BeginInvoke(act, DispatcherPriority.ApplicationIdle);
            }

            /// <summary>
            /// リストのクリーンアップを即座に実行します。
            /// </summary>
            public void CleanUp()
            {
                lock (typeActDic)
                {
                    var remTypes = new List<Type>();

                    foreach (var tp_acts in typeActDic)
                    {
                        var idxsToRem = tp_acts.Value.Select((a, i) => (a.ActionSet == null || !a.ActionSet.IsAlive) ? i : -1)
                                               .Where(i => i != -1).Reverse().ToList();

                        idxsToRem.ForEach(i => tp_acts.Value.RemoveAt(i));

                        if (tp_acts.Value.Count == 0) remTypes.Add(tp_acts.Key);
                    }

                    remTypes.ForEach(t => typeActDic.Remove(t));
                }
            }
        }
    }
}

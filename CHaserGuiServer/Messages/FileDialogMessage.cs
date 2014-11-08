using Oika.Apps.CHaserGuiServer.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oika.Apps.CHaserGuiServer.Messages
{
    /// <summary>
    /// ファイルを開くダイアログ表示のためのメッセージクラスです。
    /// </summary>
    public class OpenFileDialogMessage : FileDialogMessage
    {
    }
    /// <summary>
    /// 名前を付けて保存ダイアログ表示のためのメッセージクラスです。
    /// </summary>
    public class SaveFileDialogMessage : FileDialogMessage
    {
    }


    /// <summary>
    /// ファイルアクセスのためのメッセージの基本クラスです。
    /// </summary>
    public abstract class FileDialogMessage : IMessage
    {
        /// <summary>
        /// 保存ファイル名を取得・設定します。
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// ダイアログ表示結果を取得・設定します。
        /// </summary>
        public bool? Result { get; set; }

        #region フィルタの取得・設定

        /// <summary>
        /// フィルタの表示ラベルと対応拡張子リスト
        /// </summary>
        Dictionary<string, List<string>> filterLab_extDic = new Dictionary<string, List<string>>();

        /// <summary>
        /// フィルタ要素を追加します。
        /// </summary>
        /// <param name="label"></param>
        /// <param name="extentions"></param>
        public void AddFilter(string label, IEnumerable<string> extentions)
        {
            if (!filterLab_extDic.ContainsKey(label)) filterLab_extDic.Add(label, new List<string>());

            var trgList = filterLab_extDic[label];

            foreach (var ext in extentions)
            {
                if (!trgList.Contains(ext)) trgList.Add(ext);
            }
        }

        /// <summary>
        /// フィルターテキストを取得します。
        /// </summary>
        public string FilterText
        {
            get
            {
                var typeList = filterLab_extDic.Select(p => string.Format("{0}|{1}", p.Key, string.Join(";", p.Value)));
                return string.Join("|", typeList);
            }
        }

        #endregion
    }
}

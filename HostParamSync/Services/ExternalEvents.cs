using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HostParamSync.Models;

namespace HostParamSync.Services
{
    /// <summary>
    /// 手動同期をRevitのAPIコンテキストで実行するハンドラ
    /// </summary>
    public class ManualSyncHandler : IExternalEventHandler
    {
        public Mapping Mapping { get; set; }

        public void Execute(UIApplication app)
        {
            try
            {
                var doc = app.ActiveUIDocument?.Document;
                if (doc == null)
                {
                    TaskDialog.Show("HostParamSync", "ドキュメントが開かれていません。");
                    return;
                }

                using (var t = new Transaction(doc, "HostParamSync Manual"))
                {
                    t.Start();
                    int n = SyncService.CopyHostParamToHosted(doc, Mapping);
                    t.Commit();
                    TaskDialog.Show("HostParamSync", $"同期完了：{n} 要素を更新しました。");
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("HostParamSync", "手動同期でエラー:\n" + ex.Message);
            }
        }

        public string GetName() => "HostParamSync ManualSyncHandler";
    }

    /// <summary>
    /// 常時同期（Updater）のON/OFFをAPIコンテキストで切り替えるハンドラ
    /// </summary>
    public class ToggleAutoHandler : IExternalEventHandler
    {
        public bool Enable { get; set; }
        public Mapping Mapping { get; set; }

        public void Execute(UIApplication app)
        {
            try
            {
                if (Enable)
                {
                    SyncUpdater.RegisterUpdater(app, Mapping ?? SyncUpdater.DefaultMapping());
                }
                else
                {
                    SyncUpdater.UnregisterUpdater();
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("HostParamSync", "常時同期の切替でエラー:\n" + ex.Message);
            }
        }

        public string GetName() => "HostParamSync ToggleAutoHandler";
    }
}

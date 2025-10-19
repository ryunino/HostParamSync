using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HostParamSync.Models;

namespace HostParamSync.Services
{
    /// <summary>
    /// IUpdater：ソース（壁）のパラメータ変更を検知し、ターゲット（ドア/窓）へ自動コピー
    /// </summary>
    public class SyncUpdater : IUpdater
    {
        private static UpdaterId _uid;
        private static bool _registered = false;

        // 現在のマッピング（MVP：UIから1本設定）
        public static Mapping CurrentMapping { get; set; }

        public SyncUpdater(AddInId addInId)
        {
            _uid = new UpdaterId(addInId, new Guid("c2b2f8ab-8d9e-4c9c-9f6b-1e6a6d20b7a4"));
        }

        public static void RegisterUpdater(UIApplication uiapp, Mapping mapping)
        {
            CurrentMapping = mapping ?? DefaultMapping();

            var doc = uiapp.ActiveUIDocument?.Document;
            if (doc == null) return;

            var updater = new SyncUpdater(uiapp.ActiveAddInId);

            if (!UpdaterRegistry.IsUpdaterRegistered(_uid))
            {
                UpdaterRegistry.RegisterUpdater(updater, doc);
            }

            // 既存のトリガーを一旦クリア
            UpdaterRegistry.RemoveAllTriggers(_uid);

            // 壁カテゴリの「コメント」パラメータ変更を監視
            ElementFilter wallFilter = new ElementCategoryFilter(CurrentMapping.SourceCategory);
            // ★ BuiltInParameter → ElementId へ変換が必須（CS1503対策）
            var paramId = new ElementId(CurrentMapping.SourceParameter);
            UpdaterRegistry.AddTrigger(_uid, wallFilter, Element.GetChangeTypeParameter(paramId));

            _registered = true;
        }

        public static void UnregisterUpdater()
        {
            if (UpdaterRegistry.IsUpdaterRegistered(_uid))
                UpdaterRegistry.UnregisterUpdater(_uid);
            _registered = false;
        }

        public static bool IsRegistered() => _registered && UpdaterRegistry.IsUpdaterRegistered(_uid);

        public void Execute(UpdaterData data)
        {
            var doc = data.GetDocument();
            if (doc == null || CurrentMapping == null) return;

            var ids = data.GetModifiedElementIds(); // 変更された壁

            using (var t = new Transaction(doc, "HostParamSync Auto"))
            {
                t.Start();
                try
                {
                    SyncService.CopyHostParamToHosted(doc, CurrentMapping, ids);
                    t.Commit();
                }
                catch
                {
                    if (t.HasStarted()) t.RollBack();
                    throw;
                }
            }
        }

        public string GetAdditionalInformation() => "Copy host parameter to hosted instances.";
        public ChangePriority GetChangePriority() => ChangePriority.DoorsOpeningsWindows;
        public UpdaterId GetUpdaterId() => _uid;
        public string GetUpdaterName() => "HostParamSyncUpdater";

        public static Mapping DefaultMapping()
        {
            return new Mapping
            {
                SourceCategory = BuiltInCategory.OST_Walls,
                SourceParameter = BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS,
                TargetCategory = BuiltInCategory.OST_Doors,
                TargetParameter = BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS
            };
        }
    }
}

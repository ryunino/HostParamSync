using System.Collections.Generic;
using Autodesk.Revit.DB;
using HostParamSync.Models;

namespace HostParamSync.Services
{
    /// <summary>
    /// 実際のコピー処理（Transaction内で使用）
    /// </summary>
    public static class SyncService
    {
        public static int CopyHostParamToHosted(Document doc, Mapping map, ICollection<ElementId> specificSourceIds = null)
        {
            if (doc == null) return 0;

            var count = 0;

            // 対象のソース要素（壁）を列挙
            IList<Element> sources = new List<Element>();
            if (specificSourceIds != null && specificSourceIds.Count > 0)
            {
                foreach (var id in specificSourceIds)
                {
                    var e = doc.GetElement(id);
                    // ★ IntegerValue は非推奨。ElementId 同士で比較（または .Value を使う）
                    if (e != null && e.Category != null && e.Category.Id == new ElementId(map.SourceCategory))
                        sources.Add(e);
                }
            }
            else
            {
                var f = new FilteredElementCollector(doc)
                    .OfCategory(map.SourceCategory)
                    .WhereElementIsNotElementType();
                foreach (var e in f) sources.Add(e);
            }

            foreach (var src in sources)
            {
                var srcParam = src.get_Parameter(map.SourceParameter);
                if (srcParam == null) continue;
                var value = srcParam.AsString() ?? string.Empty;

                // 壁にぶら下がる FamilyInstance を取得（ドア/窓など）
                var dependents = src.GetDependentElements(new ElementClassFilter(typeof(FamilyInstance)));

                foreach (var depId in dependents)
                {
                    var inst = doc.GetElement(depId) as FamilyInstance;
                    if (inst?.Category == null) continue;

                    // ★ IntegerValue 非推奨 → ElementId 同士の比較
                    if (inst.Category.Id != new ElementId(map.TargetCategory))
                        continue;

                    var dstParam = inst.get_Parameter(map.TargetParameter);
                    if (dstParam == null || dstParam.IsReadOnly) continue;

                    string cur = dstParam.AsString();
                    if (cur != value)
                    {
                        dstParam.Set(value ?? string.Empty);
                        count++;
                    }
                }
            }

            return count;
        }
    }
}

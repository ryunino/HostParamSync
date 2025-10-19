using Autodesk.Revit.DB;

namespace HostParamSync.Models
{
    /// <summary>
    /// ホスト→被ホスト（例：壁→ドア）のパラメータ同期ルール（MVP版）
    /// </summary>
    public class Mapping
    {
        public BuiltInCategory SourceCategory { get; set; }  // 例：Walls
        public BuiltInParameter SourceParameter { get; set; } // 例：ALL_MODEL_INSTANCE_COMMENTS

        public BuiltInCategory TargetCategory { get; set; }   // 例：Doors or Windows
        public BuiltInParameter TargetParameter { get; set; } // 例：ALL_MODEL_INSTANCE_COMMENTS
    }
}

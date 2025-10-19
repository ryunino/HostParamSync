using System;
using System.Reflection;
using System.Windows.Interop;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace HostParamSync.Addin
{
    public class ExtApp : IExternalApplication
    {
        internal static UIControlledApplication UiApp;
        internal static UIApplication UiApiApp; // set in Execute
        internal const string RibbonTabName = "HostParamSync";
        internal const string RibbonPanelName = "Sync";

        public Result OnStartup(UIControlledApplication application)
        {
            UiApp = application;

            try { application.CreateRibbonTab(RibbonTabName); } catch { /* already exists */ }
            var panel = GetOrCreatePanel(application, RibbonTabName, RibbonPanelName);

            var btnData = new PushButtonData(
                "OpenHostParamSync",
                "HostParamSync",
                Assembly.GetExecutingAssembly().Location,
                "HostParamSync.Addin.OpenUiCommand");

            btnData.ToolTip = "壁→ドア/窓のコメント同期（手動/常時）";
            panel.AddItem(btnData);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            Services.SyncUpdater.UnregisterUpdater();
            return Result.Succeeded;
        }

        private RibbonPanel GetOrCreatePanel(UIControlledApplication app, string tab, string panelName)
        {
            foreach (var p in app.GetRibbonPanels(tab))
                if (p.Name == panelName) return p;
            return app.CreateRibbonPanel(tab, panelName);
        }
    }

    /// <summary>
    /// リボンのボタンからUIを開くための外部コマンド。
    /// モデルレス表示だが、Owner を Revit 本体へ明示的に設定する。
    /// またウィンドウ参照を静的に保持し、多重起動を避ける。
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class OpenUiCommand : IExternalCommand
    {
        private static UI.MainWindow _window; // 参照保持（多重起動防止）

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            ExtApp.UiApiApp = commandData.Application;

            try
            {
                if (_window == null || !_window.IsLoaded)
                {
                    _window = new UI.MainWindow(commandData.Application);

                    // ★ Revit メインウィンドウを Owner に設定（Add-In Manager 実行時のクラッシュ回避に重要）
                    var helper = new WindowInteropHelper(_window);
                    helper.Owner = commandData.Application.MainWindowHandle;

                    _window.Closed += (s, e) => _window = null;
                    _window.Show();          // モデルレス
                }
                else
                {
                    if (_window.WindowState == System.Windows.WindowState.Minimized)
                        _window.WindowState = System.Windows.WindowState.Normal;
                    _window.Activate();
                }

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("HostParamSync", "UI 起動で例外が発生しました:\n" + ex.Message);
                return Result.Failed;
            }
        }
    }
}

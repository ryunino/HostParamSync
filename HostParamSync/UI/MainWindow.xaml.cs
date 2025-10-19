using System;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HostParamSync.Models;
using HostParamSync.Services;

namespace HostParamSync.UI
{
    public partial class MainWindow : Window
    {
        private readonly UIApplication _uiapp;

        // ExternalEvent 一式
        private readonly ManualSyncHandler _syncHandler = new ManualSyncHandler();
        private readonly ExternalEvent _syncEvent;
        private readonly ToggleAutoHandler _toggleHandler = new ToggleAutoHandler();
        private readonly ExternalEvent _toggleEvent;

        public MainWindow(UIApplication uiapp)
        {
            InitializeComponent();
            _uiapp = uiapp;

            // ExternalEvent生成
            _syncEvent = ExternalEvent.Create(_syncHandler);
            _toggleEvent = ExternalEvent.Create(_toggleHandler);

            chkAuto.IsChecked = SyncUpdater.IsRegistered();
            UpdateStatusText();

            chkAuto.Checked += (s, e) => ToggleAuto(true);
            chkAuto.Unchecked += (s, e) => ToggleAuto(false);
        }

        private Mapping BuildMappingFromUi()
        {
            var srcCat = (BuiltInCategory)Enum.Parse(typeof(BuiltInCategory), (cmbSrcCat.SelectedItem as System.Windows.Controls.ComboBoxItem).Tag.ToString());
            var srcPar = (BuiltInParameter)Enum.Parse(typeof(BuiltInParameter), (cmbSrcParam.SelectedItem as System.Windows.Controls.ComboBoxItem).Tag.ToString());
            var dstCat = (BuiltInCategory)Enum.Parse(typeof(BuiltInCategory), (cmbDstCat.SelectedItem as System.Windows.Controls.ComboBoxItem).Tag.ToString());
            var dstPar = (BuiltInParameter)Enum.Parse(typeof(BuiltInParameter), (cmbDstParam.SelectedItem as System.Windows.Controls.ComboBoxItem).Tag.ToString());

            return new Mapping
            {
                SourceCategory = srcCat,
                SourceParameter = srcPar,
                TargetCategory = dstCat,
                TargetParameter = dstPar
            };
        }

        private void ToggleAuto(bool enable)
        {
            try
            {
                // ExternalEvent に引数を渡してRaise
                _toggleHandler.Enable = enable;
                _toggleHandler.Mapping = BuildMappingFromUi();
                _toggleEvent.Raise();

                // UI表示の更新は少し遅れて反映されることがある点だけ注意
                UpdateStatusText();
            }
            catch (Exception ex)
            {
                TaskDialog.Show("HostParamSync", "常時同期の切替でエラー:\n" + ex.Message);
                chkAuto.IsChecked = SyncUpdater.IsRegistered();
            }
        }

        private void UpdateStatusText()
        {
            txtStatus.Text = SyncUpdater.IsRegistered() ? "常時同期：ON" : "常時同期：OFF";
        }

        private void btnSync_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // ExternalEvent にマッピングを渡してRaise
                _syncHandler.Mapping = BuildMappingFromUi();
                _syncEvent.Raise();
            }
            catch (Exception ex)
            {
                TaskDialog.Show("HostParamSync", "手動同期の起動でエラー:\n" + ex.Message);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e) => Close();
    }
}

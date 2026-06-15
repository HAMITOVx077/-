using System.Windows;

namespace МаршрутСборки
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            System.Windows.Media.RenderOptions.ProcessRenderMode =
                System.Windows.Interop.RenderMode.Default;
        }
    }
}
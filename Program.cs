using UgnayDesktop.Forms;
using UgnayDesktop.Data;
using UgnayDesktop.Services;

namespace UgnayDesktop;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        DbInitializer.Seed();

        ApplicationConfiguration.Initialize();
        UdpSensorListener.Shared.Start();
        Application.ApplicationExit += (_, _) => UdpSensorListener.Shared.Dispose();
        Application.Run(new LoginForm());
    }
}

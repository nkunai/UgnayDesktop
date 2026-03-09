using UgnayDesktop.Forms;
using UgnayDesktop.Data;
using UgnayDesktop.Services;

namespace UgnayDesktop;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        AlertOutboxDispatcher? outboxDispatcher = null;

        try
        {
            DbInitializer.Seed();

            ApplicationConfiguration.Initialize();

            outboxDispatcher = new AlertOutboxDispatcher();
            outboxDispatcher.Start();

            Application.Run(new LoginForm());
        }
        finally
        {
            outboxDispatcher?.Dispose();
        }
    }
}

using Tailors.Web.Helper;

[assembly: System.Reflection.Metadata.MetadataUpdateHandler(typeof(HotReloadDelayer))]

namespace Tailors.Web.Helper;

public static class HotReloadDelayer
{
    private static readonly TimeSpan DelayHotReload = TimeSpan.FromSeconds(3);

    public static void ClearCache(Type[]? updatedTypes)
    {
        Console.WriteLine("Clearing cache...");
    }

    public static void UpdateApplication(Type[]? updatedTypes)
    {
        Console.WriteLine($"Giving Tailwind {DelayHotReload.Seconds} seconds to settle...");
        Thread.Sleep(DelayHotReload);
        Console.WriteLine("Reloading...");
    }
}

using System.Threading.Tasks;

namespace Storage
{
    public static class Program
    {
        private static async Task Main()
        {
            await Blob.RunAsync();
        }
    }
}

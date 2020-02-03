using System.Threading.Tasks;

namespace StorageTables
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            await Tables.RunAsync();
        }
    }
}

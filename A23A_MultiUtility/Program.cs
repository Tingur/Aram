using EloBuddy;
using EloBuddy.SDK.Events;

namespace A23A_MultiUtility
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            Hacks.RenderWatermark = false;
            Loading.OnLoadingComplete += TudoJuntoEmisturado.ComeçouAbrincadeira;
        }
    }
}

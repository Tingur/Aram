using EloBuddy.SDK.Events;

namespace A23A_MultiUtility
{
    internal static class Program
    {
       private static void Main()
        {
            EloBuddy.Hacks.RenderWatermark = false;
            Loading.OnLoadingComplete += TudoJuntoEmisturado.ComeçouAbrincadeira;
        }
    }
}

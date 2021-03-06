//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GamePad
{
    
    internal partial class Resources
    {
        private static System.Resources.ResourceManager manager;
        internal static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if ((Resources.manager == null))
                {
                    Resources.manager = new System.Resources.ResourceManager("GamePad.Resources", typeof(Resources).Assembly);
                }
                return Resources.manager;
            }
        }
        internal static Microsoft.SPOT.Font GetFont(Resources.FontResources id)
        {
            return ((Microsoft.SPOT.Font)(Microsoft.SPOT.ResourceUtility.GetObject(ResourceManager, id)));
        }
        internal static string GetString(Resources.StringResources id)
        {
            return ((string)(Microsoft.SPOT.ResourceUtility.GetObject(ResourceManager, id)));
        }
        internal static byte[] GetBytes(Resources.BinaryResources id)
        {
            return ((byte[])(Microsoft.SPOT.ResourceUtility.GetObject(ResourceManager, id)));
        }
        [System.SerializableAttribute()]
        internal enum StringResources : short
        {
            MyRoomForm = -31995,
            GameForm = -31693,
            RegisterForm = -31383,
            SplashForm = -26253,
            MenuForm = -18109,
            InboxForm = -17612,
            GalleryForm = 1860,
        }
        [System.SerializableAttribute()]
        internal enum BinaryResources : short
        {
            game = -27337,
            light = -18232,
            LOSE = -14382,
            linehor = -13078,
            blank = -4503,
            gallery = 8993,
            draw = 11545,
            WIN = 13352,
            logo = 17715,
            linever = 21274,
            temperature = 22240,
            humidity = 23225,
            message = 25577,
            o = 31241,
            x = 31260,
            myroom = 32208,
        }
        [System.SerializableAttribute()]
        internal enum FontResources : short
        {
            small = 13070,
            NinaB = 18060,
        }
    }
}

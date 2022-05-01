#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Program
{
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// 
        /// </summary>
        [STAThread]
        static void Main()
        {
            using ( var game = new MiniGolf() )
                game.Run();
        }
    }
#endif
}
/// <summary>
/// 
/// </summary>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCS_bot_test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            
            try
            {
                TelegramBotHelper helper = new TelegramBotHelper(token: "TOKEN");
                helper.GetUpdate();
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }

        }
    }
}

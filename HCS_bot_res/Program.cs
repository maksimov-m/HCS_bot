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
            //6081288192:AAFJABHQMJ4bDrYRCkCZ-xR0ZoG3SUGX-5M
            try
            {
                TelegramBotHelper helper = new TelegramBotHelper(token: "6081288192:AAFJABHQMJ4bDrYRCkCZ-xR0ZoG3SUGX-5M");
                helper.GetUpdate();
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }

        }
    }
}
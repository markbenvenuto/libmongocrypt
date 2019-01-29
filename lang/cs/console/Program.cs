using System;
using MongoDB.MongoCrypt;

namespace console
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            MongoDB.MongoCrypt.Stuff.Init();
        }
    }
}

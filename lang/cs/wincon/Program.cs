using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.MongoCrypt;

namespace wincon
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Console.WriteLine("Version: " + Library.Version);

            using (CryptOptions options = new CryptOptions())
            {
                options.AwsRegion = "us-east-1";
                options.AwsSecretAccessKey = "us-east-1";
                options.AwsAccessKeyId = "us-east-1";

                using (var foo = MongoDB.MongoCrypt.CryptClientFactory.Create(options))
                {
                    foo.Foo();
                }
            }
        }
    }
}

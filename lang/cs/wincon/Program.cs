/*
 * Copyright 2018-present MongoDB, Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Crypt;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using System.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using System.Diagnostics;

namespace wincon
{
    

    class Program
    {
        static BsonDocument ToDocument(Binary bin)
        {
            MemoryStream stream = new MemoryStream(bin.ToArray());
            using (var jsonReader = new BsonBinaryReader(stream))
            {
                var context = BsonDeserializationContext.CreateRoot(jsonReader);
                return BsonDocumentSerializer.Instance.Deserialize(context);
            }
        }

        static BsonDocument FromJSON(string str)
        {
            using (var jsonReader = new JsonReader(str))
            {
                var context = BsonDeserializationContext.CreateRoot(jsonReader);
                return BsonDocumentSerializer.Instance.Deserialize(context);
            }
        }

        static byte[] ToBytes(BsonDocument doc)
        {
            BsonBinaryWriterSettings settings = new BsonBinaryWriterSettings();
            // C# driver "magically" changes UUIDs underneath by default so tell it not to
            settings.GuidRepresentation = GuidRepresentation.Standard;
            return doc.ToBson(null, settings);
        }
        
        static BsonDocument ReadJSONTestFile(string file)
        {
            string root = @"d:\repo\libmongocrypt\test\example";
            string full = Path.Combine(root, file);
            string json = File.ReadAllText(full);

            return FromJSON(json) ;
        }

        static string ReadPlainTestFile(string file)
        {
            string root = @"d:\repo\libmongocrypt\test\example";
            string full = Path.Combine(root, file);
            return File.ReadAllText(full);
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Console.WriteLine("Version: " + Library.Version);

            using (CryptOptions options = new CryptOptions())
            {
                options.AwsRegion = "us-east-1";
                options.AwsSecretAccessKey = "us-east-1";
                options.AwsAccessKeyId = "us-east-1";

                //using (var foo = CryptClientFactory.Create(options))
                //using (var context = foo.StartEncryptionContext("test.test"))
                //{
                //    ProcessState(context);

                //}

                using (var foo = CryptClientFactory.Create(options))
                using (var context = foo.StartDecryptionContext(ToBytes(ReadJSONTestFile("encrypted-document.json"))))
                {
                    ProcessState(context);

                }
            }
        }

        private static void ProcessState(CryptContext context)
        {
            while (!context.IsDone)
            {
                Console.WriteLine("\n----------------------------------\nState:" + context.State);
                switch (context.State)
                {
                    case CryptContext.StateCode.MONGOCRYPT_CTX_NEED_MONGO_COLLINFO:
                        {
                            var binary = context.GetOperation();
                            var doc = ToDocument(binary);
                            Console.WriteLine("ListCollections: " + doc);
                            var reply = ReadJSONTestFile("collection-info.json");
                            Console.WriteLine("Reply:" + reply);
                            context.Feed(ToBytes(reply));
                            context.MarkDone();
                            break;
                        }
                    case CryptContext.StateCode.MONGOCRYPT_CTX_NEED_MONGO_MARKINGS:
                        {
                            var binary = context.GetOperation();
                            var doc = ToDocument(binary);
                            Console.WriteLine("Markings: " + doc);
                            var reply = ReadJSONTestFile("mongocryptd-reply.json");
                            Console.WriteLine("Reply:" + reply);
                            context.Feed(ToBytes(reply));
                            context.MarkDone();
                            break;
                        }
                    case CryptContext.StateCode.MONGOCRYPT_CTX_NEED_MONGO_KEYS:
                        {
                            var binary = context.GetOperation();
                            var doc = ToDocument(binary);
                            Console.WriteLine("Key Document: " + doc);
                            var reply = ReadJSONTestFile("key-document.json");
                            Console.WriteLine("Reply:" + reply);
                            context.Feed(ToBytes(reply));
                            context.MarkDone();
                            break;
                        }
                    case CryptContext.StateCode.MONGOCRYPT_CTX_NEED_KMS:
                        {
                            var requests = context.GetKmsMessageRequests();
                            foreach(var req in requests)
                            {
                                var binary = req.GetMessage();
                                Console.WriteLine("Key Document: " + binary);
                                var reply = ReadPlainTestFile("kms-reply.txt");
                                Console.WriteLine("Reply:" + reply);
                                req.Feed(reply);
                                Debug.Assert(req.BytesNeeded == 0);
                            }
                            context.MarkKmsDone();
                            break;
                        }
                    case CryptContext.StateCode.MONGOCRYPT_CTX_READY:
                        {
                            Binary b = context.FinalizeForEncryption();
                            Console.WriteLine("Buffer:" + b.ToArray());
                            break;
                        }
                    case CryptContext.StateCode.MONGOCRYPT_CTX_DONE:
                        {
                            Console.WriteLine("DONE!!");
                            return;
                        }
                    case CryptContext.StateCode.MONGOCRYPT_CTX_NOTHING_TO_DO:
                        {
                            Console.WriteLine("NOTHING TO DO");
                            return;
                        }
                    case CryptContext.StateCode.MONGOCRYPT_CTX_ERROR:
                        {
                            throw new NotImplementedException();
                        }
                }
            }
        }
    }
}

/*
 * Copyright 2019-present MongoDB, Inc.
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

using MongoDB.Bson;
using System;
using System.Diagnostics;
using System.IO;
using Xunit;
using System.Text;

namespace MongoDB.Crypt.Test
{
    public class BasicTests
    {
        CryptOptions CreateOptions()
        {
            CryptOptions options = new CryptOptions();
            options.AwsSecretAccessKey = "us-east-1";
            options.AwsAccessKeyId = "us-east-1";

            return options;
        }

        AwsKeyId CreateKey()
        {
            return new AwsKeyId() { CustomerMasterKey = "cmk", Region = "us-east-1" };
        }

        [Fact]
        public void EncryptQuery()
        {
            using (var foo = CryptClientFactory.Create(CreateOptions()))
            using (var context = foo.StartEncryptionContext("test.test", CreateKey()))
            {
                ProcessState(context);

            }
        }

        [Fact]
        public void DecryptQuery()
        {
            using (var foo = CryptClientFactory.Create(CreateOptions()))
            using (var context = foo.StartDecryptionContext(BsonUtil.ToBytes(ReadJSONTestFile("encrypted-document.json")), CreateKey()))
            {
                ProcessState(context);
            }
        }

        [Fact]
        public void EncryptBadBSON()
        {
            using (var foo = CryptClientFactory.Create(CreateOptions()))
            using (var context = foo.StartEncryptionContext("test.test", CreateKey()))
            {
                var binary = context.GetOperation();
                var doc = BsonUtil.ToDocument(binary);
                Console.WriteLine("ListCollections: " + doc);

                // Ensure if we encrypt non-sense, it throws an exception demonstrating our exception code is good
                Xunit.Assert.Throws<CryptException>( () => context.Feed(new byte[] { 0x1, 0x2, 0x3 }) );
            }
        }

        static string FindTestDirectory()
        {
            // Assume we are child directory of the repo
            string searchPath = Path.Combine("..", "test", "example");
            string cwd = Directory.GetCurrentDirectory();
            for(int i = 0; i < 10; i++)
            {
                string testPath = Path.Combine(cwd, searchPath);
                if (Directory.Exists(testPath))
                {
                    return testPath;
                }

                searchPath = Path.Combine("..", searchPath);
            }

            throw new DirectoryNotFoundException("test/example");
        }


        static string ReadHttpTestFile(string file)
        {
            // The HTTP tests assume \r\n
            // And git strips \r on Unix machines by default so fix up the files
            string root = FindTestDirectory();
            string full = Path.Combine(root, file);
            string text = File.ReadAllText(full);

            StringBuilder builder = new StringBuilder(text.Length);
            for(int i = 0; i < text.Length; i++) {
                if(text[i] == '\n' && text[i - 1] != '\r' )
                    builder.Append('\r');
            builder.Append(text[i]);
            }
            return builder.ToString();
        }

        static BsonDocument ReadJSONTestFile(string file)
        {
            string root = FindTestDirectory();
            string full = Path.Combine(root, file);
            string text = File.ReadAllText(full);
            return BsonUtil.FromJSON(text);
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
                            var doc = BsonUtil.ToDocument(binary);
                            Console.WriteLine("ListCollections: " + doc);
                            var reply = ReadJSONTestFile("collection-info.json");
                            Console.WriteLine("Reply:" + reply);
                            context.Feed(BsonUtil.ToBytes(reply));
                            context.MarkDone();
                            break;
                        }
                    case CryptContext.StateCode.MONGOCRYPT_CTX_NEED_MONGO_MARKINGS:
                        {
                            var binary = context.GetOperation();
                            var doc = BsonUtil.ToDocument(binary);
                            Console.WriteLine("Markings: " + doc);
                            var reply = ReadJSONTestFile("mongocryptd-reply.json");
                            Console.WriteLine("Reply:" + reply);
                            context.Feed(BsonUtil.ToBytes(reply));
                            context.MarkDone();
                            break;
                        }
                    case CryptContext.StateCode.MONGOCRYPT_CTX_NEED_MONGO_KEYS:
                        {
                            var binary = context.GetOperation();
                            var doc = BsonUtil.ToDocument(binary);
                            Console.WriteLine("Key Document: " + doc);
                            var reply = ReadJSONTestFile("key-document.json");
                            Console.WriteLine("Reply:" + reply);
                            context.Feed(BsonUtil.ToBytes(reply));
                            context.MarkDone();
                            break;
                        }
                    case CryptContext.StateCode.MONGOCRYPT_CTX_NEED_KMS:
                        {
                            var requests = context.GetKmsMessageRequests();
                            foreach (var req in requests)
                            {
                                var binary = req.GetMessage();
                                Console.WriteLine("Key Document: " + binary);
                                var reply = ReadHttpTestFile("kms-decrypt-reply.txt");
                                Console.WriteLine("Reply:" + reply);
                                req.Feed(reply);
                                Xunit.Assert.Equal(0.0, req.BytesNeeded);
                            }
                            requests.MarkDone();
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
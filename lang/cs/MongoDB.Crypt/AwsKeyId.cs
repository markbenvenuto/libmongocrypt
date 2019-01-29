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

namespace MongoDB.Crypt
{
    /// <summary>Contains all the information needed to find a AWS KMS CMK.</summary>
    public class AwsKeyId : IKmsKeyId
    {
        public KmsKeyType KeyType => KmsKeyType.Aws;

        /// <summary>Gets or sets the region.</summary>
        /// <value>The region.</value>
        public string Region { get; set; }

        /// <summary>
        /// Gets or sets the customer master key.
        /// </summary>
        /// <value>
        /// The customer master key.
        /// </value>
        public string CustomerMasterKey { get; set; }
    }
}

// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using Newtonsoft.Json;

namespace Monai.Deploy.WorkloadManager.Configuration
{
    public class WorkloadManagerOptions
    {
        /// <summary>
        /// Name of the key for retrieve database connection string.
        /// </summary>
        public const string DatabaseConnectionStringKey = "WorkloadManagerDatabase";

        /// <summary>
        /// Represents the <c>messaging</c> section of the configuration file.
        /// </summary>
        [JsonProperty(PropertyName = "messaging")]
        public MessageBrokerConfiguration Messaging { get; set; }

        public WorkloadManagerOptions()
        {

        }
    }
}

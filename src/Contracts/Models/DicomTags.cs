﻿// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Monai.Deploy.WorkflowManager.Contracts.Models
{
    public class DicomTags
    {
        [JsonProperty(PropertyName = "study_id")]
        public string StudyId { get; set; }

        [JsonProperty(PropertyName = "tags")]
        public Dictionary<string, string> Tags { get; set; }

        [JsonProperty(PropertyName = "series")]
        public List<string> Series { get; set; }
    }
}

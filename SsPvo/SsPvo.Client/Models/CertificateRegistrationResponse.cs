﻿using Newtonsoft.Json;
using SsPvo.Client.Messages.Base;

namespace SsPvo.Client.Models
{
    public class CertificateRegistrationResponse : IJsonResponse
    {
        /// <summary>
        /// Первая буква кириллическая (баг в API СС)
        /// </summary>
        [JsonProperty("сertificate")]
        public bool Сertificate { get; set; }
    }
}

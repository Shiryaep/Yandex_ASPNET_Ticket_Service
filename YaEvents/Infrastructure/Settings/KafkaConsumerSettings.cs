using System;
using System.Collections.Generic;
using System.Text;
using YaContracts;

namespace YaEvents.Infrastructure.Settings
{
    public class KafkaConsumerSettings
    {
        public string BootstrapServers { get; set; } = string.Empty;
        public string ConsumerGroup { get; set; } = string.Empty;
        public KafkaTopics Topics { get; set; } = new();
    }

    public class KafkaTopics
    {
        public string BookingConfirmed { get; set; } = Constants.BookingConfirmedTopicName;
    }
}

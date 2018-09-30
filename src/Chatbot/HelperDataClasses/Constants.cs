using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Chatbot.HelperDataClasses
{
    public class Constants
    {
        public const string AWSProfileName = "Hackathon";

        public const string MESSAGE_CONTENT_TYPE = "PlainText";

        public const string CURRENT_REQUEST = "currentRequest";
        public const string PREVIOUS_REQUEST = "previousRequest";

        public const string PREVIOUS_SETUP_REQUEST = "previousSetupRequest";
        public const string CURRENT_SETUP_REQUEST = "currentSetupRequest";

        public const string ACTION_SLOT = "action";
        public const string SERVER_NAME_SLOT = "serverName";
        public const string AMI_SLOT = "AMI";
        public const string INSTANCE_TYPE_SLOT = "InstanceTypes";
        public const string STORAGE_TYPE_SLOT = "StorageTypes";
        public const string NUMBER_TYPE_SLOT = "NumberTypes";
        public const string AZ_SLOT = "AvailabilityZones";

        public const string CONFIRMATION = "Confirmation";

        public const string KEY_PAIR_BUCKET_NAME = "infra-helper-keypairstore";
    }
}
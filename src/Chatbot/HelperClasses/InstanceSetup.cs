using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chatbot.HelperClasses
{
    public class InstanceSetup
    {
        public string InstanceName { get; set; }
        public string KeyPairName { get; set; }
        public string SecurityGroupName { get; set; }
        public string AMIType { get; set; }
        public string InstanceType { get; set; }
        public int NumOfInstances { get; set; }
        public string StorageType { get; set; }
        public string AvailabilityZone { get; set; }

        public InstanceSetup(string InstanceName, string KeyPairName,
                               string SecurityGroupName, string AMIType , string InstanceType , int NumOfInstances, string StorageType , string AvailabilityZone)
        {
            this.InstanceName = InstanceName;
            this.KeyPairName = KeyPairName;
            this.SecurityGroupName = SecurityGroupName;
            this.AMIType = AMIType;
            this.InstanceType = InstanceType;
            this.NumOfInstances = NumOfInstances;
            this.StorageType = StorageType;
            this.AvailabilityZone = AvailabilityZone;
        }
    }
}

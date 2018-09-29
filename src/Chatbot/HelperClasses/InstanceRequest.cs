using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chatbot.HelperClasses
{
    public class InstanceRequest
    {
        public string InstanceName { get; set; }
        public string InstanceAction { get; set; }
        public string InstanceID { get; set; }
        public string InstanceState { get; set; }

        [JsonIgnore]
        public bool HasRequiredServerFields
        {
            get
            {
                return !string.IsNullOrEmpty(InstanceName) &&
                       !string.IsNullOrEmpty(InstanceAction);
            }
        }

         public InstanceRequest(string InstanceName , string InstanceAction,
                                string InstanceID , string InstanceState)
        {
            this.InstanceName = InstanceName;
            this.InstanceState = InstanceState;
            this.InstanceID = InstanceID;
            this.InstanceAction = InstanceAction;
        }

        private enum SupportedActions
        {
            START,
            STOP,
            TERMINATE,
            DESTROY
        }
    }
}

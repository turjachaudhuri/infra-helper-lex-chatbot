using Amazon.Lambda.Core;
using Amazon.Lambda.LexEvents;
using Chatbot.HelperClasses;
using Chatbot.HelperDataClasses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Chatbot.IntentProcessors
{
    class DescribeIntentProcessor : AbstractIntentProcessor
    {
        private bool IsLocalDebug { get; set; }
        private ServerOperationsHelper serverOperationsHelper = null;
        private IDictionary<string, string> sessionAttributes = new Dictionary<string, string>();

        public DescribeIntentProcessor()
        {
            this.IsLocalDebug = false;
            Setup();
        }

        public DescribeIntentProcessor(bool isLocalDebug)
        {
            this.IsLocalDebug = isLocalDebug;
            Setup();
        }

        private void Setup()
        {
            serverOperationsHelper = new ServerOperationsHelper(IsLocalDebug);
        }

        public override LexResponse Process(LexEvent lexEvent, ILambdaContext context)
        {
            context.Logger.LogLine("Input Request: " + JsonConvert.SerializeObject(lexEvent));

            List<InstanceRequest> instances = new List<InstanceRequest>();
            if (string.Equals(lexEvent.InvocationSource, "FulfillmentCodeHook", StringComparison.Ordinal))
            {
                instances = serverOperationsHelper.validEC2Instances();
            }

            var groupedInstances = instances.GroupBy(item => item.InstanceState)
                     .Select(group => new { state = group.Key, items = group.ToList() , count = group.ToList().Count })
                     .ToList()
                     .ConvertAll(x=> x.count > 1 ? $"{x.count} instances are in {x.state} state" : $"{x.count} instance is in {x.state} state");

            string responseMessage = string.Empty;
            if(instances.Count == 0)
            {
                responseMessage = "Currently , you donot have any instances in your AWS account.";
            }
            else
            {
                responseMessage = $"You have {instances.Count} instance(s) in your AWS account in total , out of which " +
                    $"{string.Join(" , ", groupedInstances)}." +
                    $"The instances are as follows : {string.Join(" , ", instances.ConvertAll(x => $"{x.InstanceName}({x.InstanceState})"))}";
            }

            return Close(
                        sessionAttributes,
                        "Fulfilled",
                        new LexResponse.LexMessage
                        {
                            ContentType = Constants.MESSAGE_CONTENT_TYPE,
                            Content = $"Your existing instances are : { string.Join(',', instances.ConvertAll(x => $"{x.InstanceName}({x.InstanceState})"))}"
                        }
                    );
        }
    }
}

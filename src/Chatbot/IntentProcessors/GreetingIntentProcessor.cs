using Amazon.Lambda.Core;
using Amazon.Lambda.LexEvents;
using Chatbot.HelperDataClasses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chatbot.IntentProcessors
{
    class GreetingIntentProcessor : AbstractIntentProcessor
    {
        private IDictionary<string, string> sessionAttributes = new Dictionary<string, string>();
        public override LexResponse Process(LexEvent lexEvent, ILambdaContext context)
        {
            context.Logger.LogLine("Input Request: " + JsonConvert.SerializeObject(lexEvent));

            int index = new Random().Next() % 3;
            return Close(
                        sessionAttributes,
                        "Fulfilled",
                        new LexResponse.LexMessage
                        {
                            ContentType = Constants.MESSAGE_CONTENT_TYPE,
                            Content = SampleData.SAMPLE_GREETING_RESPONSES[index]
                        }
                    );
        }
    }
}

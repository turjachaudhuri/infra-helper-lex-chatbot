using System;
using System.Collections.Generic;
using System.Text;
using Amazon.Lambda.Core;
using Amazon.Lambda.LexEvents;
using Chatbot.HelperDataClasses;
using Newtonsoft.Json;

namespace Chatbot.IntentProcessors
{
    class HelperIntentProcessor : AbstractIntentProcessor
    {
        private IDictionary<string, string> sessionAttributes = new Dictionary<string, string>();
        private ILambdaContext context;

        public HelperIntentProcessor(ILambdaContext context)
        {
            this.context = context;
        }
        public override LexResponse Process(LexEvent lexEvent)
        {
            context.Logger.LogLine("Input Request: " + JsonConvert.SerializeObject(lexEvent));

            int index = new Random().Next() % 4;
            return Close(
                        sessionAttributes,
                        "Fulfilled",
                        new LexResponse.LexMessage
                        {
                            ContentType = Constants.MESSAGE_CONTENT_TYPE,
                            Content = SampleData.SAMPLE_EXAMPLE_RESPONSES[index]
                        },
                        null
                    );
        }
    }
}

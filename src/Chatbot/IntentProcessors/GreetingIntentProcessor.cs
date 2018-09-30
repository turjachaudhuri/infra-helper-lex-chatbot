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
        private ILambdaContext context;

        public GreetingIntentProcessor(ILambdaContext context)
        {
            this.context = context;
        }

        public override LexResponse Process(LexEvent lexEvent)
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
                        //,createResponseCard()
                    );
        }

        private LexResponse.LexResponseCard createResponseCard()
        {
            LexResponse.LexResponseCard card = new LexResponse.LexResponseCard();
            List<LexResponse.LexButton> cardButtons = new List<LexResponse.LexButton>();
            LexResponse.LexGenericAttachments cardGenericAttachments = new LexResponse.LexGenericAttachments();

            card.Version = 1;
            card.ContentType = "application/vnd.amazonaws.card.generic";
            cardGenericAttachments.Title = "WolfBot";
            cardGenericAttachments.SubTitle = "Your friendly AWS Infrastructure helper";
            cardGenericAttachments.ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/5/5c/AWS_Simple_Icons_AWS_Cloud.svg";
            card.GenericAttachments = new List<LexResponse.LexGenericAttachments>()
                                                                    { cardGenericAttachments };
            return card;            
        }
    }
}

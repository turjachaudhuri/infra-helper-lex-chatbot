using System;
using System.Collections.Generic;
using System.Text;
using Amazon.Lambda.Core;
using Amazon.Lambda.LexEvents;
using Chatbot.HelperClasses;
using Chatbot.HelperDataClasses;
using Newtonsoft.Json;

namespace Chatbot.IntentProcessors
{
    /// <summary>
    /// Base class for intent processors.
    /// </summary>
    public abstract class AbstractIntentProcessor : IIntentProcessor
    {
        /// <summary>
        /// Main method for proccessing the lex event for the intent.
        /// </summary>
        /// <param name="lexEvent"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public abstract LexResponse Process(LexEvent lexEvent);

        protected string SerializeRequest(InstanceRequest instanceRequest)
        {
            return JsonConvert.SerializeObject(instanceRequest, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
        }

        protected InstanceRequest DeserializeRequest(string json)
        {
            return JsonConvert.DeserializeObject<InstanceRequest>(json);
        }

        protected string SerializeLaunchRequest(InstanceSetup instanceSetupRequest)
        {
            return JsonConvert.SerializeObject(instanceSetupRequest, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
        }

        protected InstanceSetup DeserializeLaunchRequest(string json)
        {
            return JsonConvert.DeserializeObject<InstanceSetup>(json);
        }

        protected LexResponse Close(IDictionary<string, string> sessionAttributes, 
                                    string fulfillmentState, 
                                    LexResponse.LexMessage message,
                                    LexResponse.LexResponseCard responseCard = null)
        {
            return new LexResponse
            {
                SessionAttributes = sessionAttributes,
                DialogAction = new LexResponse.LexDialogAction
                {
                    Type = "Close",
                    FulfillmentState = fulfillmentState,
                    Message = message,
                    ResponseCard = responseCard
                }
            };
        }

        protected LexResponse Delegate(IDictionary<string, string> sessionAttributes, IDictionary<string, string> slots)
        {
            return new LexResponse
            {
                SessionAttributes = sessionAttributes,
                DialogAction = new LexResponse.LexDialogAction
                {
                    Type = "Delegate",
                    Slots = slots
                }
            };
        }

        protected LexResponse ElicitSlot(
                                    IDictionary<string, string> sessionAttributes,
                                    string intentName,
                                    IDictionary<string, string> slots,
                                    string slotToElicit,
                                    LexResponse.LexMessage message,
                                    LexResponse.LexResponseCard responseCard
                                    )
        {
            return new LexResponse
            {
                SessionAttributes = sessionAttributes,
                DialogAction = new LexResponse.LexDialogAction
                {
                    Type = "ElicitSlot",
                    IntentName = intentName,
                    Slots = slots,
                    SlotToElicit = slotToElicit,
                    Message = message,
                    ResponseCard = responseCard
                }
            };
        }

        protected LexResponse ConfirmIntent(IDictionary<string, string> sessionAttributes, string intentName,
                                            IDictionary<string, string> slots, LexResponse.LexMessage message,
                                            LexResponse.LexResponseCard responseCard = null)
        {
            return new LexResponse
            {
                SessionAttributes = sessionAttributes,
                DialogAction = new LexResponse.LexDialogAction
                {
                    Type = "ConfirmIntent",
                    IntentName = intentName,
                    Slots = slots,
                    Message = message,
                    ResponseCard = responseCard
                }
            };
        }

        protected LexResponse.LexResponseCard createResponseCard(string SlotType)
        {
            LexResponse.LexResponseCard card = new LexResponse.LexResponseCard();
            List<LexResponse.LexButton> cardButtons = new List<LexResponse.LexButton>();
            LexResponse.LexGenericAttachments cardGenericAttachments = new LexResponse.LexGenericAttachments();

            switch (SlotType)
            {
                case Constants.NUMBER_TYPE_SLOT:
                    card.Version = 1;
                    card.ContentType = "application/vnd.amazonaws.card.generic";
                    cardGenericAttachments.Title = "Instance count";
                    cardGenericAttachments.SubTitle = "Select the number of instances to launch";
                    cardButtons = new List<LexResponse.LexButton>()
                    {
                        new LexResponse.LexButton(){Text = "1" , Value = "1"},
                        new LexResponse.LexButton(){Text = "2" , Value = "2"},
                        new LexResponse.LexButton(){Text = "3" , Value = "3"},
                        new LexResponse.LexButton(){Text = "4" , Value = "4"},
                        new LexResponse.LexButton(){Text = "5" , Value = "5"},
                    };
                    cardGenericAttachments.Buttons = cardButtons;
                    card.GenericAttachments = new List<LexResponse.LexGenericAttachments>()
                                                                            { cardGenericAttachments };
                    return card;

                case Constants.INSTANCE_TYPE_SLOT:
                    card.Version = 1;
                    card.ContentType = "application/vnd.amazonaws.card.generic";
                    cardGenericAttachments.Title = "Instance Types";
                    cardGenericAttachments.SubTitle = "Choose an instance type";
                    cardGenericAttachments.ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/b/b9/AWS_Simple_Icons_Compute_Amazon_EC2_Instances.svg/200px-AWS_Simple_Icons_Compute_Amazon_EC2_Instances.svg.png";
                    cardButtons = new List<LexResponse.LexButton>()
                    {
                        new LexResponse.LexButton(){Text = "t2micro" , Value = "t2micro"},
                        new LexResponse.LexButton(){Text = "t2small" , Value = "t2small"},
                        new LexResponse.LexButton(){Text = "t2medium" , Value = "t2medium"}
                    };
                    cardGenericAttachments.Buttons = cardButtons;
                    card.GenericAttachments = new List<LexResponse.LexGenericAttachments>()
                                                                            { cardGenericAttachments };
                    return card;

                case Constants.AMI_SLOT:
                    card.Version = 1;
                    card.ContentType = "application/vnd.amazonaws.card.generic";
                    cardGenericAttachments.Title = "AMI";
                    cardGenericAttachments.SubTitle = "Please choose an AMI out of the following : ";
                    cardGenericAttachments.ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/6/6b/AWS_Simple_Icons_Compute_Amazon_EC2_AMI.svg/2000px-AWS_Simple_Icons_Compute_Amazon_EC2_AMI.svg.png";
                    cardButtons = new List<LexResponse.LexButton>()
                    {
                        new LexResponse.LexButton(){Text = "Ubuntu" , Value = "Ubuntu"},
                        new LexResponse.LexButton(){Text = "Red Hat" , Value = "Red Hat"},
                        new LexResponse.LexButton(){Text = "Windows" , Value = "Windows"},
                        new LexResponse.LexButton(){Text = "Amazon Linux" , Value = "Amazon Linux"},
                    };
                    cardGenericAttachments.Buttons = cardButtons;
                    card.GenericAttachments = new List<LexResponse.LexGenericAttachments>()
                                                                            { cardGenericAttachments };
                    return card;

                case Constants.AZ_SLOT:
                    card.Version = 1;
                    card.ContentType = "application/vnd.amazonaws.card.generic";
                    cardGenericAttachments.Title = "Availability Zone";
                    cardGenericAttachments.SubTitle = "Please choose an AZ out of the following : ";
                    cardButtons = new List<LexResponse.LexButton>()
                    {
                        new LexResponse.LexButton(){Text = "us-east-1a" , Value = "us-east-1a"},
                        new LexResponse.LexButton(){Text = "us-east-1b" , Value = "us-east-1b"},
                        new LexResponse.LexButton(){Text = "us-east-1c" , Value = "us-east-1c"},
                        new LexResponse.LexButton(){Text = "us-east-1e" , Value = "us-east-1e"},
                    };
                    cardGenericAttachments.Buttons = cardButtons;
                    card.GenericAttachments = new List<LexResponse.LexGenericAttachments>()
                                                                            { cardGenericAttachments };
                    return card;

                case Constants.STORAGE_TYPE_SLOT:
                    card.Version = 1;
                    card.ContentType = "application/vnd.amazonaws.card.generic";
                    cardGenericAttachments.Title = "Storage Type";
                    cardGenericAttachments.SubTitle = "Please choose a storage type out of the following : ";
                    cardButtons = new List<LexResponse.LexButton>()
                    {
                        new LexResponse.LexButton(){Text = "Magnetic" , Value = "Magnetic"},
                        new LexResponse.LexButton(){Text = "Provisioned" , Value = "Provisioned"},
                        new LexResponse.LexButton(){Text = "General purpose" , Value = "General purpose"}
                    };
                    cardGenericAttachments.Buttons = cardButtons;
                    card.GenericAttachments = new List<LexResponse.LexGenericAttachments>()
                                                                            { cardGenericAttachments };
                    return card;

                case Constants.CONFIRMATION:
                    card.Version = 1;
                    card.ContentType = "application/vnd.amazonaws.card.generic";
                    cardGenericAttachments.Title = "Confirm";
                    cardGenericAttachments.SubTitle = "Please confirm Yes/No : ";
                    cardButtons = new List<LexResponse.LexButton>()
                    {
                        new LexResponse.LexButton(){Text = "Yes" , Value = "Yes"},
                        new LexResponse.LexButton(){Text = "No" , Value = "No"}
                    };
                    cardGenericAttachments.Buttons = cardButtons;
                    card.GenericAttachments = new List<LexResponse.LexGenericAttachments>()
                                                                            { cardGenericAttachments };
                    return card;

                case Constants.ACTION_SLOT:
                    card.Version = 1;
                    card.ContentType = "application/vnd.amazonaws.card.generic";
                    cardGenericAttachments.Title = "Instance action";
                    cardGenericAttachments.SubTitle = "Please select an action to perform on the instance : ";
                    cardButtons = new List<LexResponse.LexButton>()
                    {
                        new LexResponse.LexButton(){Text = "Start" , Value = "Start"},
                        new LexResponse.LexButton(){Text = "Stop" , Value = "Stop"},
                        new LexResponse.LexButton(){Text = "Terminate" , Value = "Terminate"},
                    };
                    cardGenericAttachments.Buttons = cardButtons;
                    card.GenericAttachments = new List<LexResponse.LexGenericAttachments>()
                                                                            { cardGenericAttachments };
                    return card;
            }
            return null;
        }
    }
}
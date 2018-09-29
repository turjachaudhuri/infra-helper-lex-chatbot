using Amazon.Lambda.Core;
using Amazon.Lambda.LexEvents;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Chatbot.HelperClasses;
using Chatbot.HelperDataClasses;

namespace Chatbot.IntentProcessors
{
    class ServerIntentProcessor : AbstractIntentProcessor
    {
        private bool IsLocalDebug { get; set; }
        private ServerOperationsHelper serverOperationsHelper = null;
        private IDictionary<string, string> sessionAttributes = new Dictionary<string, string>();

        public ServerIntentProcessor()
        {
            this.IsLocalDebug = false;
            Setup();
        }

        public ServerIntentProcessor(bool isLocalDebug)
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
            bool actionSucceeded = false;
            string actionResponseMessage = string.Empty;

            context.Logger.LogLine("Input Request: " + JsonConvert.SerializeObject(lexEvent));

            var slots = lexEvent.CurrentIntent.Slots;
            sessionAttributes = lexEvent.SessionAttributes ?? new Dictionary<string, string>();

            InstanceRequest previousRequest = null;           

            if (sessionAttributes.ContainsKey(Constants.PREVIOUS_REQUEST))
            {
                previousRequest = DeserializeRequest(sessionAttributes[Constants.PREVIOUS_REQUEST]);
            }

            InstanceRequest instanceRequest = new InstanceRequest
            (
                slots.ContainsKey(Constants.SERVER_NAME_SLOT) ? slots[Constants.SERVER_NAME_SLOT] : null,
                slots.ContainsKey(Constants.ACTION_SLOT) ? slots[Constants.ACTION_SLOT] : null,
                string.Empty,
                string.Empty
            );

            sessionAttributes[Constants.CURRENT_REQUEST] = SerializeRequest(instanceRequest);

            if (string.Equals(lexEvent.InvocationSource, "DialogCodeHook", StringComparison.Ordinal))
            {
                // If any slots are invalid, re-elicit for their value
                TryInferSlots(instanceRequest, previousRequest);
                ValidationResult validateResult = Validate(instanceRequest);
                if (!validateResult.IsValid)
                {
                    lexEvent.CurrentIntent.Slots[validateResult.ViolationSlot] = null;
                    return ElicitSlot(sessionAttributes, lexEvent.CurrentIntent.Name, lexEvent.CurrentIntent.Slots,
                                      validateResult.ViolationSlot, validateResult.Message);
                }
                //This means that the slots are valid

                if (instanceRequest.InstanceAction.ToLower() == "describe")
                    {
                        OperateAWSServer(instanceRequest,context , ref actionSucceeded , ref actionResponseMessage);
                        return Close(
                                     sessionAttributes,
                                     "Fulfilled",
                                     new LexResponse.LexMessage
                                     {
                                         ContentType = Constants.MESSAGE_CONTENT_TYPE,
                                         Content = actionResponseMessage
                                     }
                                 );
                    }

                //We might have already asked for confirmation , but it was either denied or provided
                //Case 1 : We still do not have confirmation
                if (string.Equals(lexEvent.CurrentIntent.ConfirmationStatus, "None", StringComparison.Ordinal))
                {
                    return ConfirmIntent(
                                    sessionAttributes,
                                    lexEvent.CurrentIntent.Name,
                                    new Dictionary<string, string>
                                    {
                                        {Constants.SERVER_NAME_SLOT, instanceRequest.InstanceName },
                                        {Constants.ACTION_SLOT, instanceRequest.InstanceAction }
                                    },
                                    new LexResponse.LexMessage
                                    {
                                        ContentType = Constants.MESSAGE_CONTENT_TYPE,
                                        Content = $"Are you sure you want to {instanceRequest.InstanceAction} the server {instanceRequest.InstanceName}"
                                    }
                                );
                }
                else if (string.Equals(lexEvent.CurrentIntent.ConfirmationStatus, "Confirmed", StringComparison.Ordinal))
                {
                    OperateAWSServer(instanceRequest, context , ref actionSucceeded , ref actionResponseMessage);
                    sessionAttributes.Remove(Constants.CURRENT_REQUEST);
                    sessionAttributes[Constants.PREVIOUS_REQUEST] = SerializeRequest(instanceRequest);

                    return Close(
                            sessionAttributes,
                            "Fulfilled",
                            new LexResponse.LexMessage
                            {
                                ContentType = Constants.MESSAGE_CONTENT_TYPE,
                                Content = actionResponseMessage
                            }
                        );
                }
                else if (string.Equals(lexEvent.CurrentIntent.ConfirmationStatus, "Denied", StringComparison.Ordinal))
                {
                    sessionAttributes.Remove(Constants.CURRENT_REQUEST);
                    sessionAttributes.Remove(Constants.PREVIOUS_REQUEST);

                    return Close(
                                sessionAttributes,
                                "Fulfilled",
                                new LexResponse.LexMessage
                                {
                                    ContentType = Constants.MESSAGE_CONTENT_TYPE,
                                    Content = "Current request has been cancelled . Please start a brand new request if need be."
                                }
                            );
                }
            }
            return Delegate(sessionAttributes, slots);
        }

        private void TryInferSlots(InstanceRequest instanceRequest, InstanceRequest previousRequest)
        {
            if (previousRequest != null)
            {                
                if (string.IsNullOrEmpty(instanceRequest.InstanceName) && !string.IsNullOrEmpty(previousRequest.InstanceName))
                {
                    instanceRequest.InstanceName = previousRequest.InstanceName;
                }
                if (string.IsNullOrEmpty(instanceRequest.InstanceAction) && !string.IsNullOrEmpty(previousRequest.InstanceAction))
                {
                    instanceRequest.InstanceAction = previousRequest.InstanceAction;
                }
            }
        }

        private ValidationResult Validate(InstanceRequest instanceRequest)
        {
            if (string.IsNullOrEmpty(instanceRequest.InstanceName))
            {
                return new ValidationResult(false, Constants.SERVER_NAME_SLOT,
                    $"Please enter the instance name you want to operate on");
            }
            if (!string.IsNullOrEmpty(instanceRequest.InstanceName))
            {
                List<string> instanceNames = serverOperationsHelper.validEC2Instances().ConvertAll(x=>x.InstanceName);
                if (!instanceNames.ConvertAll(x=>x.ToLower()).Contains(instanceRequest.InstanceName.ToLower()))
                {
                    return new ValidationResult(false,
                                                Constants.SERVER_NAME_SLOT,
                                                $"{instanceRequest.InstanceName} is not a valid instance in your account ." +
                                                $"Please select one out of the following instances from your account ?" +
                                                string.Join(',', instanceNames)
                                                );
                }
            }
            if (string.IsNullOrEmpty(instanceRequest.InstanceAction) && !string.IsNullOrEmpty(instanceRequest.InstanceName))
            {
                return new ValidationResult(false, Constants.ACTION_SLOT,
                    $"Please enter the action you want to perform on {instanceRequest.InstanceName}");
            }
            if (string.IsNullOrEmpty(instanceRequest.InstanceAction) && !string.IsNullOrEmpty(instanceRequest.InstanceName))
            {
                return new ValidationResult(false, Constants.ACTION_SLOT,
                    $"Please enter the action you want to perform on {instanceRequest.InstanceName}");
            }
            if (!string.IsNullOrEmpty(instanceRequest.InstanceAction) && !TypeValidators.IsValidActionType(instanceRequest.InstanceAction))
            {
                return new ValidationResult(false, Constants.ACTION_SLOT,
                    $"We currently do not support {instanceRequest.InstanceAction} as a valid action ." +
                    $"Can you try an action out of the following ones ? \n" +
                    TypeValidators.ListOfValidActionTypes()
                        );
            }
            return ValidationResult.VALID_RESULT;
        }

        private void OperateAWSServer(InstanceRequest instanceRequest, ILambdaContext context , ref bool actionSucceeded , ref string actionMessage)
        {
            try
            {
                serverOperationsHelper.InstanceRequestObj = instanceRequest;
                serverOperationsHelper.updateInstanceIdFromName();
                instanceRequest = serverOperationsHelper.ServerOperation(context , ref actionSucceeded , ref actionMessage);
            }
            catch (Exception ex)
            {
                context.Logger.LogLine($"StartServerIntentProcessor::OperateAWSServer => {ex.Message}");
                context.Logger.LogLine($"StartServerIntentProcessor::OperateAWSServer => {ex.StackTrace}");
            }
        }
    }
}

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
    class LaunchIntentProcessor : AbstractIntentProcessor
    {
        private bool IsLocalDebug { get; set; }
        private ServerOperationsHelper serverOperationsHelper = null;
        private IDictionary<string, string> sessionAttributes = new Dictionary<string, string>();

        public LaunchIntentProcessor()
        {
            this.IsLocalDebug = false;
            Setup();
        }

        public LaunchIntentProcessor(bool isLocalDebug)
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

            InstanceSetup previousLaunchRequest = null;

            if (sessionAttributes.ContainsKey(Constants.PREVIOUS_REQUEST))
            {
                previousLaunchRequest = DeserializeLaunchRequest(sessionAttributes[Constants.PREVIOUS_SETUP_REQUEST]);
            }

            InstanceSetup instanceLaunchRequest = new InstanceSetup
            (
                string.Empty,
                string.Empty,
                string.Empty,
                slots.ContainsKey(Constants.AMI_SLOT) ? slots[Constants.AMI_SLOT] : null,
                slots.ContainsKey(Constants.INSTANCE_TYPE_SLOT) ? slots[Constants.INSTANCE_TYPE_SLOT] : null,
                slots.ContainsKey(Constants.NUMBER_TYPE_SLOT) ? Convert.ToInt32(slots[Constants.NUMBER_TYPE_SLOT]) : 0,
                slots.ContainsKey(Constants.STORAGE_TYPE_SLOT) ? slots[Constants.STORAGE_TYPE_SLOT] : null,
                slots.ContainsKey(Constants.AZ_SLOT) ? slots[Constants.AZ_SLOT] : null
            );

            sessionAttributes[Constants.CURRENT_SETUP_REQUEST] = SerializeLaunchRequest(instanceLaunchRequest);

            if (string.Equals(lexEvent.InvocationSource, "DialogCodeHook", StringComparison.Ordinal))
            {
                // If any slots are invalid, re-elicit for their value
                TryInferSlots(instanceLaunchRequest, previousLaunchRequest);
                ValidationResult validateResult = Validate(instanceLaunchRequest , context);
                if (!validateResult.IsValid)
                {
                    lexEvent.CurrentIntent.Slots[validateResult.ViolationSlot] = null;
                    return ElicitSlot(sessionAttributes, lexEvent.CurrentIntent.Name, lexEvent.CurrentIntent.Slots,
                                      validateResult.ViolationSlot, validateResult.Message);
                }
                //This means that the slots are valid

                //We might have already asked for confirmation , but it was either denied or provided
                //Case 1 : We still do not have confirmation
                if (string.Equals(lexEvent.CurrentIntent.ConfirmationStatus, "None", StringComparison.Ordinal))
                {
                    return ConfirmIntent(
                                    sessionAttributes,
                                    lexEvent.CurrentIntent.Name,
                                    new Dictionary<string, string>
                                    {
                                        {Constants.AMI_SLOT, instanceLaunchRequest.AMIType },
                                        {Constants.INSTANCE_TYPE_SLOT, instanceLaunchRequest.InstanceType },
                                        {Constants.STORAGE_TYPE_SLOT, instanceLaunchRequest.StorageType },
                                        {Constants.NUMBER_TYPE_SLOT, instanceLaunchRequest.NumOfInstances.ToString() },
                                        {Constants.AZ_SLOT, instanceLaunchRequest.AvailabilityZone }
                                    },
                                    new LexResponse.LexMessage
                                    {
                                        ContentType = Constants.MESSAGE_CONTENT_TYPE,
                                        Content = $"Are you sure you want to launch {instanceLaunchRequest.NumOfInstances.ToString()} instance(s) with AMI - {instanceLaunchRequest.AMIType} " +
                                                  $", Instance Type - {instanceLaunchRequest.InstanceType} , Storage Type - {instanceLaunchRequest.StorageType} ?"
                                                                
                                    }
                                );
                }
                else if (string.Equals(lexEvent.CurrentIntent.ConfirmationStatus, "Confirmed", StringComparison.Ordinal))
                {
                    OperateAWSServer(instanceLaunchRequest, context, ref actionSucceeded, ref actionResponseMessage);
                    sessionAttributes.Remove(Constants.CURRENT_SETUP_REQUEST);
                    sessionAttributes[Constants.PREVIOUS_SETUP_REQUEST] = SerializeLaunchRequest(instanceLaunchRequest);

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
                    return Close(
                                sessionAttributes,
                                "Fulfilled",
                                new LexResponse.LexMessage
                                {
                                    ContentType = Constants.MESSAGE_CONTENT_TYPE,
                                    Content = "Current request has been cancelled . Please start a new request if need be."
                                }
                            );
                }
            }
            return Delegate(sessionAttributes, slots);
        }

        private void TryInferSlots(InstanceSetup instanceLaunchRequest, InstanceSetup previousLaunchRequest)
        {
            if (previousLaunchRequest != null)
            {
                if (string.IsNullOrEmpty(instanceLaunchRequest.AMIType) && !string.IsNullOrEmpty(previousLaunchRequest.AMIType))
                {
                    instanceLaunchRequest.AMIType = previousLaunchRequest.AMIType;
                }
                if (string.IsNullOrEmpty(instanceLaunchRequest.StorageType) && !string.IsNullOrEmpty(previousLaunchRequest.StorageType))
                {
                    instanceLaunchRequest.StorageType = previousLaunchRequest.StorageType;
                }
                if (string.IsNullOrEmpty(instanceLaunchRequest.InstanceType) && !string.IsNullOrEmpty(previousLaunchRequest.InstanceType))
                {
                    instanceLaunchRequest.InstanceType = previousLaunchRequest.InstanceType;
                }
            }
        }

        private ValidationResult Validate(InstanceSetup instanceLaunchRequest , ILambdaContext context)
        {
            if (string.IsNullOrEmpty(instanceLaunchRequest.AMIType))
            {
                return new ValidationResult(false, Constants.AMI_SLOT,
                    $"Please enter the AMI you want to use . I currently support the following : {TypeValidators.ListOfValidAMITypes()} ");
            }
            if (!string.IsNullOrEmpty(instanceLaunchRequest.AMIType) && !TypeValidators.IsValidAMIType(instanceLaunchRequest.AMIType))
            {
                return new ValidationResult(false, Constants.AMI_SLOT,
                    $"We currently do not support {instanceLaunchRequest.AMIType} as a valid AMI ." +
                    $"Can you try an AMI out of the following ones ?" +
                    TypeValidators.ListOfValidAMITypes()
                        );
            }

            if (string.IsNullOrEmpty(instanceLaunchRequest.StorageType))
            {
                return new ValidationResult(false, Constants.STORAGE_TYPE_SLOT,
                    $"Please enter the Storage type you want to use . I currently support the following : {TypeValidators.ListOfValidStorageTypes()}");
            }
            if (!string.IsNullOrEmpty(instanceLaunchRequest.StorageType) && !TypeValidators.IsValidStorageType(instanceLaunchRequest.StorageType))
            {
                return new ValidationResult(false, Constants.STORAGE_TYPE_SLOT,
                    $"We currently do not support {instanceLaunchRequest.StorageType} as a valid Storage Type ." +
                    $"Can you try a Storage Type out of the following ones ? \n" +
                    TypeValidators.ListOfValidStorageTypes()
                        );
            }

            if (string.IsNullOrEmpty(instanceLaunchRequest.InstanceType))
            {
                return new ValidationResult(false, Constants.INSTANCE_TYPE_SLOT,
                    $"Please enter the Instance type you want to use . I currently support the following : {TypeValidators.ListOfValidInstanceTypes()}");
            }
            if (!string.IsNullOrEmpty(instanceLaunchRequest.InstanceType) && !TypeValidators.IsValidInstanceType(instanceLaunchRequest.InstanceType))
            {
                return new ValidationResult(false, Constants.INSTANCE_TYPE_SLOT,
                    $"We currently do not support {instanceLaunchRequest.InstanceType} as a valid Instance Type ." +
                    $"Can you try a Instance Type out of the following ones ? {TypeValidators.ListOfValidInstanceTypes()}"
                        );
            }

            if (string.IsNullOrEmpty(instanceLaunchRequest.AvailabilityZone))
            {
                return new ValidationResult(false, Constants.AZ_SLOT,
                    $"Please enter the AZ you want to use . List of AZ(s) for your region are - : {string.Join(" , ",serverOperationsHelper.getAvailabilityZones(context))}");
            }
            if (!string.IsNullOrEmpty(instanceLaunchRequest.AvailabilityZone) && !serverOperationsHelper.getAvailabilityZones(context).Contains(instanceLaunchRequest.AvailabilityZone))
            {
                return new ValidationResult(false, Constants.AZ_SLOT,
                    $" {instanceLaunchRequest.AvailabilityZone} is not a valid Availability Zone for your region ." +
                    $"Can you try a AZ out of the following ones ? {serverOperationsHelper.getAvailabilityZones(context)}" );
            }

            if (instanceLaunchRequest.NumOfInstances == 0)
            {
                return new ValidationResult(false, Constants.NUMBER_TYPE_SLOT,
                    "Please enter the number of instances you want to launch . I currently support a maximum of 5 instances .");
            }
            if (instanceLaunchRequest.NumOfInstances > 5)
            {
                return new ValidationResult(false, Constants.NUMBER_TYPE_SLOT,
                    "I currently only support upto 5 instances. Could you please try with any number between 1 and 5 ");
            }
            return ValidationResult.VALID_RESULT;
        }

        private void OperateAWSServer(InstanceSetup instanceLaunchRequest, ILambdaContext context, ref bool actionSucceeded, ref string actionMessage)
        {
            try
            {
                serverOperationsHelper.LaunchRequest = instanceLaunchRequest;
                serverOperationsHelper.LaunchServer(context, ref actionSucceeded, ref actionMessage);
            }
            catch (Exception ex)
            {
                context.Logger.LogLine($"StartServerIntentProcessor::OperateAWSServer => {ex.Message}");
                context.Logger.LogLine($"StartServerIntentProcessor::OperateAWSServer => {ex.StackTrace}");
            }
        }
    }
}

using Amazon.EC2;
using Amazon.EC2.Model;
using Amazon.Lambda.Core;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Chatbot.HelperDataClasses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chatbot.HelperClasses
{
    public class ServerOperationsHelper
    {
        public ServerOperationsHelper(bool isLocalDebug)
        {
            this.IsLocalDebug = isLocalDebug;
            if (isLocalDebug)
            {
                var chain = new CredentialProfileStoreChain();
                AWSCredentials awsCredentials;
                if (chain.TryGetAWSCredentials(Constants.AWSProfileName, out awsCredentials))
                {
                    // use awsCredentials
                    Ec2Client = new AmazonEC2Client(awsCredentials, Amazon.RegionEndpoint.USEast1);
                }
            }
            else
            {
                Ec2Client = new AmazonEC2Client();
            }
        }

        public ServerOperationsHelper(bool isLocalDebug, InstanceRequest instanceRequest)
        {
            this.IsLocalDebug = isLocalDebug;
            this.InstanceRequestObj = instanceRequest;
            if (isLocalDebug)
            {
                var chain = new CredentialProfileStoreChain();
                AWSCredentials awsCredentials;
                if (chain.TryGetAWSCredentials(Constants.AWSProfileName, out awsCredentials))
                {
                    // use awsCredentials
                    Ec2Client = new AmazonEC2Client(awsCredentials, Amazon.RegionEndpoint.USEast1);
                }
            }
            else
            {
                Ec2Client = new AmazonEC2Client();
            }
            updateInstanceIdFromName();
        }

        public void updateInstanceIdFromName()
        {
            List<InstanceRequest> InstanceRequestList = validEC2Instances();
            InstanceRequest filteredObj = InstanceRequestList.Find(x => x.InstanceName.ToLower() == InstanceRequestObj.InstanceName.ToLower());
            InstanceRequestObj.InstanceID = filteredObj.InstanceID;
            InstanceRequestObj.InstanceState = filteredObj.InstanceState;
        }

        public InstanceRequest InstanceRequestObj { get; set; }
        public AmazonEC2Client Ec2Client { get; set; }
        public bool IsLocalDebug { get; set; }

        public InstanceRequest ServerOperation(ILambdaContext context , ref bool actionSucceeded , ref string actionMessage)
        {
            switch(InstanceRequestObj.InstanceAction.ToLower())
            {
                case "start":
                    StartInstance(context, ref actionSucceeded, ref actionMessage);
                    break;
                case "stop":
                    StopInstance(context, ref actionSucceeded, ref actionMessage);
                    break;
                case "describe":
                    StartInstance(context, ref actionSucceeded, ref actionMessage);
                    break;
                case "terminate":
                    TerminateInstance(context, ref actionSucceeded, ref actionMessage);
                    break;
            }
            return InstanceRequestObj;
        }

        private void TerminateInstance(ILambdaContext context, ref bool actionSucceeded, ref string actionMessage)
        {
            if (InstanceRequestObj.InstanceState.ToLower() != "running" && InstanceRequestObj.InstanceState.ToLower()!= "stopped")
            {
                actionSucceeded = false;
                actionMessage = $"The instance {InstanceRequestObj.InstanceName} is currently in {InstanceRequestObj.InstanceState} state , and cannot be terminated at this time.";
                return;
            }
            var request = new TerminateInstancesRequest
            {
                InstanceIds = new List<string>() { InstanceRequestObj.InstanceID }
            };
            try
            {
                TerminateInstancesResponse response = Ec2Client.TerminateInstancesAsync(request).GetAwaiter().GetResult();
                foreach (InstanceStateChange item in response.TerminatingInstances)
                {
                    Console.WriteLine("Stopped instance: " + item.InstanceId);
                    Console.WriteLine("Instance state: " + item.CurrentState.Name);
                }
                actionSucceeded = true;
                actionMessage = $"The instance {InstanceRequestObj.InstanceName} has been terminated. Please check the AWS Console to verify.";
            }
            catch (Exception ex)
            {
                context.Logger.LogLine($"ServerOperationsHelper::TerminateInstance {ex.Message}");
                context.Logger.LogLine($"ServerOperationsHelper::TerminateInstance {ex.StackTrace}");
                actionSucceeded = false;
                actionMessage = $"Could not terminate {InstanceRequestObj.InstanceName} . Please contact your administrator.";
            }
        }

        private void DescribeInstance(ILambdaContext context , ref bool actionSucceeded, ref string actionMessage)
        {
            try
            {
                List<InstanceRequest> InstanceRequestList = validEC2Instances();
                InstanceRequest filteredInstance = InstanceRequestList.Find(x => x.InstanceID == InstanceRequestObj.InstanceID);
                InstanceRequestObj.InstanceState = filteredInstance.InstanceState;

                actionSucceeded = true;
                actionMessage = $"Your instance {InstanceRequestObj.InstanceName} is currently in {InstanceRequestObj.InstanceState} state.";
            }
            catch(Exception ex)
            {
                context.Logger.LogLine($"ServerOperationsHelper::DescribeInstance {ex.Message}");
                context.Logger.LogLine($"ServerOperationsHelper::DescribeInstance {ex.StackTrace}");
                actionSucceeded = false;
                actionMessage = $"Could not start {InstanceRequestObj.InstanceName} . Please contact your administrator.";
            }
        }

        public List<InstanceRequest> validEC2Instances()
        {
            DescribeInstancesRequest request = new DescribeInstancesRequest();
            DescribeInstancesResponse response = Ec2Client.DescribeInstancesAsync().GetAwaiter().GetResult();
            List<InstanceRequest> instanceRequestList = new List<InstanceRequest>();
            foreach (Reservation item in response.Reservations)
            {
                foreach (Instance instance in item.Instances)
                {
                    foreach (Tag tag in instance.Tags)
                    {
                        instanceRequestList.Add( 
                                    new InstanceRequest(
                                                        !String.IsNullOrEmpty(tag.Value)? tag.Value: instance.InstanceId,
                                                        string.Empty,
                                                        instance.InstanceId,
                                                        instance.State.Name.Value
                                                        )
                                            );
                    }
                }
            }
            return instanceRequestList;
        }

        public void StopInstance(ILambdaContext context , ref bool actionSucceeded, ref string actionMessage)
        {
            if (InstanceRequestObj.InstanceState.ToLower() != "running")
            {
                actionSucceeded = false;
                actionMessage = $"The instance {InstanceRequestObj.InstanceName} is currently in {InstanceRequestObj.InstanceState} , and cannot be stopped at this time.";
                return;
            }
            var request = new StopInstancesRequest
            {
                InstanceIds = new List<string>() { InstanceRequestObj.InstanceID }
            };
            try
            {
                StopInstancesResponse response = Ec2Client.StopInstancesAsync(request).GetAwaiter().GetResult();
                foreach (InstanceStateChange item in response.StoppingInstances)
                {
                    Console.WriteLine("Stopped instance: " + item.InstanceId);
                    Console.WriteLine("Instance state: " + item.CurrentState.Name);
                }
                actionSucceeded = true;
                actionMessage = $"The instance {InstanceRequestObj.InstanceName} has been stopped. Please check the AWS Console to verify.";
            }
            catch (Exception ex)
            {
                context.Logger.LogLine($"ServerOperationsHelper::StopInstance {ex.Message}");
                context.Logger.LogLine($"ServerOperationsHelper::StopInstance {ex.StackTrace}");
                actionSucceeded = false;
                actionMessage = $"Could not stop {InstanceRequestObj.InstanceName} . Please contact your administrator.";
            }
        }

        public void StartInstance(ILambdaContext context , ref bool actionSucceeded, ref string actionMessage)
        {
            if(InstanceRequestObj.InstanceState.ToLower() != "stopped")
            {
                actionSucceeded = false;
                actionMessage = $"The instance {InstanceRequestObj.InstanceName} is currently in {InstanceRequestObj.InstanceState} state , and cannot be started at this time.";
                return;
            }
            var request = new StartInstancesRequest
            {
                InstanceIds = new List<string>() { InstanceRequestObj.InstanceID }
            };

            try
            {                
                StartInstancesResponse response = Ec2Client.StartInstancesAsync(request).GetAwaiter().GetResult();
                foreach (InstanceStateChange item in response.StartingInstances)
                {
                    Console.WriteLine("Started instance: " + item.InstanceId);
                    Console.WriteLine("Instance state: " + item.CurrentState.Name);
                }
                actionSucceeded = true;
                actionMessage = $"The instance {InstanceRequestObj.InstanceName} has been started. Please check the AWS Console to verify.";
            }
            catch (Exception ex)
            {
                context.Logger.LogLine($"ServerOperationsHelper::StopInstance {ex.Message}");
                context.Logger.LogLine($"ServerOperationsHelper::StopInstance {ex.StackTrace}");
                actionSucceeded = false;
                actionMessage = $"Could not start {InstanceRequestObj.InstanceName} . Please contact your administrator.";
            }
        }
    }
}

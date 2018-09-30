using Amazon.EC2;
using Amazon.EC2.Model;
using Amazon.Lambda.Core;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Chatbot.HelperDataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Chatbot.HelperClasses
{
    public class ServerOperationsHelper
    {
        public InstanceRequest InstanceRequestObj { get; set; }
        public InstanceSetup LaunchRequest { get; set; }
        public AmazonEC2Client Ec2Client { get; set; }
        public bool IsLocalDebug { get; set; }
        public ILambdaContext context { get; set; }

        public ServerOperationsHelper(bool isLocalDebug , ILambdaContext context)
        {
            this.IsLocalDebug = isLocalDebug;
            this.context = context;
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

        public ServerOperationsHelper(bool isLocalDebug, InstanceRequest instanceRequest , ILambdaContext context)
        {
            this.IsLocalDebug = isLocalDebug;
            this.InstanceRequestObj = instanceRequest;
            this.context = context;
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

        public InstanceRequest ServerOperation(ref bool actionSucceeded, ref string actionMessage)
        {
            switch (InstanceRequestObj.InstanceAction.ToLower())
            {
                case "start":
                    StartInstance(ref actionSucceeded, ref actionMessage);
                    break;
                case "stop":
                    StopInstance(ref actionSucceeded, ref actionMessage);
                    break;
                case "terminate":
                    TerminateInstance( ref actionSucceeded, ref actionMessage);
                    break;
            }
            return InstanceRequestObj;
        }

        private void TerminateInstance(ref bool actionSucceeded, ref string actionMessage)
        {
            if (InstanceRequestObj.InstanceState.ToLower() != "running" && InstanceRequestObj.InstanceState.ToLower() != "stopped")
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
                actionMessage = $"The instance {InstanceRequestObj.InstanceName} is being terminated. Please check the AWS Console to verify.";
            }
            catch (Exception ex)
            {
                context.Logger.LogLine($"ServerOperationsHelper::TerminateInstance {ex.Message}");
                context.Logger.LogLine($"ServerOperationsHelper::TerminateInstance {ex.StackTrace}");
                actionSucceeded = false;
                actionMessage = $"Could not terminate {InstanceRequestObj.InstanceName} . Please contact your administrator.";
            }
        }

        private void DescribeInstance(ref bool actionSucceeded, ref string actionMessage)
        {
            try
            {
                List<InstanceRequest> InstanceRequestList = validEC2Instances();
                InstanceRequest filteredInstance = InstanceRequestList.Find(x => x.InstanceID == InstanceRequestObj.InstanceID);
                InstanceRequestObj.InstanceState = filteredInstance.InstanceState;

                actionSucceeded = true;
                actionMessage = $"Your instance {InstanceRequestObj.InstanceName} is currently in {InstanceRequestObj.InstanceState} state.";
            }
            catch (Exception ex)
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
                    Tag nameTag = instance.Tags.Where(x => x.Key == "Name").FirstOrDefault();
                    string nameOfInstance = nameTag != null ? nameTag.Value : instance.InstanceId;

                    instanceRequestList.Add(
                                    new InstanceRequest(
                                                        nameOfInstance,
                                                        string.Empty,
                                                        instance.InstanceId,
                                                        instance.State.Name.Value
                                                        )
                                            );                   
                }
            }
            return instanceRequestList;
        }

        public void StopInstance(ref bool actionSucceeded, ref string actionMessage)
        {
            if (InstanceRequestObj.InstanceState.ToLower() != "running")
            {
                actionSucceeded = false;
                actionMessage = $"The instance {InstanceRequestObj.InstanceName} is currently in {InstanceRequestObj.InstanceState} state, and cannot be stopped at this time.";
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
                actionMessage = $"The instance {InstanceRequestObj.InstanceName} is being stopped. Please check the AWS Console to verify.";
            }
            catch (Exception ex)
            {
                context.Logger.LogLine($"ServerOperationsHelper::StopInstance {ex.Message}");
                context.Logger.LogLine($"ServerOperationsHelper::StopInstance {ex.StackTrace}");
                actionSucceeded = false;
                actionMessage = $"Could not stop {InstanceRequestObj.InstanceName} . Please contact your administrator.";
            }
        }

        public void StartInstance(ref bool actionSucceeded, ref string actionMessage)
        {
            if (InstanceRequestObj.InstanceState.ToLower() != "stopped")
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
                actionMessage = $"The instance {InstanceRequestObj.InstanceName} is being started. Please check the AWS Console to verify.";
            }
            catch (Exception ex)
            {
                context.Logger.LogLine($"ServerOperationsHelper::StopInstance {ex.Message}");
                context.Logger.LogLine($"ServerOperationsHelper::StopInstance {ex.StackTrace}");
                actionSucceeded = false;
                actionMessage = $"Could not start {InstanceRequestObj.InstanceName} . Please contact your administrator.";
            }
        }


        public bool CheckKeyPair(string keyPairName)
        {
            var dkpRequest = new DescribeKeyPairsRequest();
            var dkpResponse = Ec2Client.DescribeKeyPairsAsync(dkpRequest).GetAwaiter().GetResult();
            List<KeyPairInfo> myKeyPairs = dkpResponse.KeyPairs;

            return myKeyPairs.Find(x => x.KeyName == keyPairName) != null;
        }

        public bool CreateKeyPair(string keyPairName , ref string keyPairPath)
        {
            if (!CheckKeyPair(keyPairName))
            {
                var newKeyRequest = new CreateKeyPairRequest()
                {
                    KeyName = keyPairName
                };
                var ckpResponse = Ec2Client.CreateKeyPairAsync(newKeyRequest).GetAwaiter().GetResult();

                
                string bucketName = Constants.KEY_PAIR_BUCKET_NAME;
                string bucketkeyName = $"{keyPairName}.pem";
                bool successfulUpload = false;
                S3Helper s3Helper = new S3Helper(this.IsLocalDebug, bucketName, bucketkeyName);
                s3Helper.PushTextFileToS3Bucket(ckpResponse.KeyPair.KeyMaterial , ref successfulUpload);

                keyPairPath = $"Private key for this instance is stored in {bucketName} bucket and {bucketkeyName} key . Please download from there for connecting to the instance.";
                return true;
            }
            return false;
        }

        public List<string> getAvailabilityZones()
        {
            try
            {
                DescribeAvailabilityZonesResponse AZresponse = Ec2Client.DescribeAvailabilityZonesAsync().GetAwaiter().GetResult();
                return AZresponse.AvailabilityZones.ConvertAll(x => x.ZoneName);
            }
            catch (Exception ex)
            {
                context.Logger.LogLine($"ServerOperationsHelper::StopInstance {ex.Message}");
                context.Logger.LogLine($"ServerOperationsHelper::StopInstance {ex.StackTrace}");
            }
            return new List<string>();
        }


        internal void LaunchServer(ref bool actionSucceeded, ref string actionMessage)
        {
            try
            {
                string keyPairPath = string.Empty;
                LaunchRequest.KeyPairName = $"KeyPair-{Guid.NewGuid().ToString()}";
                while (!CreateKeyPair(LaunchRequest.KeyPairName , ref keyPairPath))
                {
                    LaunchRequest.KeyPairName = Guid.NewGuid().ToString();
                }

                DescribeVpcsRequest vpcRequest = new DescribeVpcsRequest();
                DescribeVpcsResponse vpcResponse = Ec2Client.DescribeVpcsAsync(vpcRequest).GetAwaiter().GetResult();

                Vpc defaultVPC = vpcResponse.Vpcs.Find(x => x.IsDefault); //get the default vpc

                List<Filter> subnetFilter = new List<Filter>()
                {
                    new Filter(){ Name = "availability-zone", Values = new List<string>() { LaunchRequest.AvailabilityZone }},
                    new Filter(){ Name = "vpc-id", Values = new List<string>() { defaultVPC.VpcId }}
                };

                DescribeSubnetsRequest subnetRequest = new DescribeSubnetsRequest();
                subnetRequest.Filters = subnetFilter;
                DescribeSubnetsResponse subnetResponse = Ec2Client.DescribeSubnetsAsync(subnetRequest).GetAwaiter().GetResult();
                Subnet defaultSubnet = subnetResponse.Subnets.FirstOrDefault();

                Filter SGFilter = new Filter
                {
                    Name = "vpc-id",
                    Values = new List<string>() { defaultVPC.VpcId }
                };

                DescribeSecurityGroupsRequest SGrequest = new DescribeSecurityGroupsRequest();
                SGrequest.Filters.Add(SGFilter);
                DescribeSecurityGroupsResponse SGresponse = Ec2Client.DescribeSecurityGroupsAsync(SGrequest).GetAwaiter().GetResult();
                SecurityGroup defaultSG = SGresponse.SecurityGroups.FirstOrDefault();

                InstanceNetworkInterfaceSpecification defaultENI = new InstanceNetworkInterfaceSpecification()
                {
                    DeviceIndex = 0,
                    SubnetId = defaultSubnet.SubnetId,
                    Groups = new List<string>() { defaultSG.GroupId },
                    AssociatePublicIpAddress = true
                };

                List<InstanceNetworkInterfaceSpecification> enis = new List<InstanceNetworkInterfaceSpecification>() { defaultENI };

                EbsBlockDevice ebsBlockDevice = new EbsBlockDevice
                {
                    VolumeSize = 10,
                    VolumeType = GetActualStorageType(LaunchRequest.StorageType)
                };
                BlockDeviceMapping blockDeviceMapping = new BlockDeviceMapping
                {
                    DeviceName = "/dev/xvda"
                };
                blockDeviceMapping.Ebs = ebsBlockDevice;

                var launchRequest = new RunInstancesRequest()
                {
                    ImageId = GetImageID(LaunchRequest.AMIType),
                    InstanceType = GetActualInstanceType(LaunchRequest.InstanceType),
                    MinCount = LaunchRequest.NumOfInstances,
                    MaxCount = LaunchRequest.NumOfInstances,
                    KeyName = LaunchRequest.KeyPairName,
                    Placement = new Placement(LaunchRequest.AvailabilityZone),
                    NetworkInterfaces = enis,
                    BlockDeviceMappings = new List<BlockDeviceMapping>() { blockDeviceMapping }
                };

                RunInstancesResponse launchResponse = Ec2Client.RunInstancesAsync(launchRequest).GetAwaiter().GetResult();

                List<String> instanceIds = new List<string>();
                foreach (Instance instance in launchResponse.Reservation.Instances)
                {
                    Console.WriteLine(instance.InstanceId);
                    instanceIds.Add(instance.InstanceId);
                }

                actionSucceeded = true;
                actionMessage = $"The instance(s) are being launched. Please check the AWS Console to verify. {keyPairPath}";
            }
            catch (Exception ex)
            {
                context.Logger.LogLine($"ServerOperationsHelper::LaunchServer {ex.Message}");
                context.Logger.LogLine($"ServerOperationsHelper::LaunchServer {ex.StackTrace}");
                actionSucceeded = false;
                actionMessage = $"Could not launch the server . Please contact your administrator.";
            }
        }

        private string GetImageID(string AMIType)
        {
            return SampleData.AMI_DICT.GetValueOrDefault(Regex.Replace(AMIType.ToLower().Trim(), @"\s+", ""));
        }

        private string GetActualInstanceType(string InstanceType)
        {
            return SampleData.INSTANC_TYPE_DICT.GetValueOrDefault(Regex.Replace(InstanceType.ToLower().Trim(), @"\s+", ""));
        }

        private string GetActualStorageType(string StorageType)
        {
            return SampleData.STORAGE_TYPE_DICT.GetValueOrDefault(Regex.Replace(StorageType.ToLower().Trim(), @"\s+", ""));
        }
    }
}

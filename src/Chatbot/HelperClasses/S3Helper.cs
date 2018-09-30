using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Amazon.S3.Model;
using Chatbot.HelperDataClasses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chatbot.HelperClasses
{
    class S3Helper
    {
        private bool IsLocalDebug;
        private string  bucketName;
        private string keyName;
        private Amazon.S3.IAmazonS3 S3Client;

        public S3Helper(bool isLocalDebug , string bucketName , string keyName)
        {
            this.IsLocalDebug = isLocalDebug;
            this.bucketName = bucketName;
            this.keyName = keyName;
            if (IsLocalDebug)
            {
                var chain = new CredentialProfileStoreChain();
                AWSCredentials awsCredentials;
                if (chain.TryGetAWSCredentials(Constants.AWSProfileName, out awsCredentials))
                {
                    // use awsCredentials
                    S3Client = new AmazonS3Client(awsCredentials, Amazon.RegionEndpoint.USEast1);
                }
            }
            else
            {
                S3Client = new AmazonS3Client();
            }
        }

        private List<string> ListingBuckets()
        {
            try
            {
                ListBucketsResponse response = S3Client.ListBucketsAsync().GetAwaiter().GetResult();
                return response.Buckets.ConvertAll(x => x.BucketName);
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    Console.WriteLine("Please check the provided AWS Credentials.");
                    Console.WriteLine("If you haven't signed up for Amazon S3, please visit http://aws.amazon.com/s3");
                }
                else
                {
                    Console.WriteLine("An Error, number {0}, occurred when listing buckets with the message '{1}", amazonS3Exception.ErrorCode, amazonS3Exception.Message);
                }
            }
            return new List<string>();
        }

        private void CreateBucket()
        {
            try
            {
                if(!ListingBuckets().Contains(this.bucketName))
                {
                    PutBucketRequest request = new PutBucketRequest
                    {
                        BucketName = this.bucketName
                    };
                    S3Client.PutBucketAsync(request).GetAwaiter().GetResult();
                }
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null && (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") || amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    Console.WriteLine("Please check the provided AWS Credentials.");
                    Console.WriteLine("If you haven't signed up for Amazon S3, please visit http://aws.amazon.com/s3");
                }
                else
                {
                    Console.WriteLine("An Error, number {0}, occurred when creating a bucket with the message '{1}", amazonS3Exception.ErrorCode, amazonS3Exception.Message);
                }
            }
        }

        public void PushTextFileToS3Bucket(string objectToUpload ,ref bool successfulUpload)
        {
            try
            {
                CreateBucket();
                // simple object put
                PutObjectRequest request = new PutObjectRequest()
                {
                    ContentBody = objectToUpload,
                    BucketName = this.bucketName,
                    Key = this.keyName
                };

                PutObjectResponse response = S3Client.PutObjectAsync(request).GetAwaiter().GetResult();
                successfulUpload = true;
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    Console.WriteLine("Please check the provided AWS Credentials.");
                    Console.WriteLine("If you haven't signed up for Amazon S3, please visit http://aws.amazon.com/s3");
                }
                else
                {
                    Console.WriteLine("An error occurred with the message '{0}' when writing an object", amazonS3Exception.Message);
                }
                successfulUpload = false;
            }
        }



    }
}

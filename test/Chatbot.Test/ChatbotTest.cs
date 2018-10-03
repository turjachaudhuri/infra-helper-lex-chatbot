using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

using Newtonsoft.Json;
using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.APIGatewayEvents;
using System.IO;
using Amazon.Lambda.LexEvents;

namespace InfraHelperChatbot.Tests
{
    public class ChatbotTest
    {
        [Fact]
        public void TestFulfillmentCodeHook()
        {
            var json = File.ReadAllText(@"SampleTestFiles\start-ec2-lex-event.json");

            var lexEvent = JsonConvert.DeserializeObject<LexEvent>(json);
            var lambdaFunction = new ChatbotStartupProgram(true);

            var context = new TestLambdaContext();
            var response = lambdaFunction.LambdaFunctionHandler(lexEvent, context);

            Assert.Equal("Close", response.DialogAction.Type);
        }

        [Fact]
        public void TestDialogCodeHook()
        {
            var json = File.ReadAllText(@"SampleTestFiles\start-ec2-lex-dialog-code-hook.json");

            var lexEvent = JsonConvert.DeserializeObject<LexEvent>(json);
            var lambdaFunction = new ChatbotStartupProgram(true);

            var context = new TestLambdaContext();
            var response = lambdaFunction.LambdaFunctionHandler(lexEvent, context);

            Assert.Equal("ElicitSlot", response.DialogAction.Type);
        }

        [Fact]
        public void TestLaunchInstanceConfirmationEvent()
        {
            var json = File.ReadAllText(@"SampleTestFiles\LaunchInstanceConfirmation.json");

            var lexEvent = JsonConvert.DeserializeObject<LexEvent>(json);
            var lambdaFunction = new ChatbotStartupProgram(true);

            var context = new TestLambdaContext();
            var response = lambdaFunction.LambdaFunctionHandler(lexEvent, context);

            Assert.Equal("ElicitSlot", response.DialogAction.Type);
        }

        [Fact]
        public void TestDescribeInstancesEvent()
        {
            var json = File.ReadAllText(@"SampleTestFiles\DescribeInstanceEvent.json");

            var lexEvent = JsonConvert.DeserializeObject<LexEvent>(json);
            var lambdaFunction = new ChatbotStartupProgram(true);

            var context = new TestLambdaContext();
            var response = lambdaFunction.LambdaFunctionHandler(lexEvent, context);

            Assert.Equal("ElicitSlot", response.DialogAction.Type);
        }

        [Fact]
        public void TestLaunchInstanceElicitSlotEvent()
        {
            var json = File.ReadAllText(@"SampleTestFiles\LaunchInstanceElicitSlot.json");

            var lexEvent = JsonConvert.DeserializeObject<LexEvent>(json);
            var lambdaFunction = new ChatbotStartupProgram(true);

            var context = new TestLambdaContext();
            var response = lambdaFunction.LambdaFunctionHandler(lexEvent, context);

            Assert.Equal("ElicitSlot", response.DialogAction.Type);
        }

        [Fact]
        public void TestLaunchInstanceElicitNumberSlotEventFromSlack()
        {
            var json = File.ReadAllText(@"SampleTestFiles\LaunchInstanceElicitNumberSlotSlack.json");

            var lexEvent = JsonConvert.DeserializeObject<LexEvent>(json);
            var lambdaFunction = new ChatbotStartupProgram(true);

            var context = new TestLambdaContext();
            var response = lambdaFunction.LambdaFunctionHandler(lexEvent, context);

            Assert.Equal("ElicitSlot", response.DialogAction.Type);
        }

        [Fact]
        public void TestLaunchInstanceElicitAMISlotWithResponseCard()
        {
            var json = File.ReadAllText(@"SampleTestFiles\LaunchInstanceElicitAMISlotSlackResponseCard.json");

            var lexEvent = JsonConvert.DeserializeObject<LexEvent>(json);
            var lambdaFunction = new ChatbotStartupProgram(true);

            var context = new TestLambdaContext();
            var response = lambdaFunction.LambdaFunctionHandler(lexEvent, context);

            Assert.Equal("ElicitSlot", response.DialogAction.Type);
        }

        [Fact]
        public void TestLaunchInstanceStartEvent()
        {
            var json = File.ReadAllText(@"SampleTestFiles\LaunchInstanceStartEvent.json");

            var lexEvent = JsonConvert.DeserializeObject<LexEvent>(json);
            var lambdaFunction = new ChatbotStartupProgram(true);

            var context = new TestLambdaContext();
            var response = lambdaFunction.LambdaFunctionHandler(lexEvent, context);

            Assert.Equal("ElicitSlot", response.DialogAction.Type);
        }
    }
}

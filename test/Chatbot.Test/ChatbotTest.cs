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
            var json = File.ReadAllText(@"C:\\Office\\Projects\\2018\\AWS\\AWS SAM\\LexChatbot\\Chatbot\\start-ec2-lex-event.json");

            var lexEvent = JsonConvert.DeserializeObject<LexEvent>(json);
            var lambdaFunction = new ChatbotStartupProgram(true);

            var context = new TestLambdaContext();
            var response = lambdaFunction.LambdaFunctionHandler(lexEvent, context);

            Assert.Equal("Close", response.DialogAction.Type);
        }

        [Fact]
        public void TestDialogCodeHook()
        {
            var json = File.ReadAllText(@"C:\\Office\\Projects\\2018\\AWS\\AWS SAM\\LexChatbot\\Chatbot\\start-ec2-lex-dialog-code-hook.json");

            var lexEvent = JsonConvert.DeserializeObject<LexEvent>(json);
            var lambdaFunction = new ChatbotStartupProgram(true);

            var context = new TestLambdaContext();
            var response = lambdaFunction.LambdaFunctionHandler(lexEvent, context);

            Assert.Equal("ElicitSlot", response.DialogAction.Type);
        }

        [Fact]
        public void TestLaunchInstanceConfirmationEvent()
        {
            var json = File.ReadAllText(@"C:\\Office\\Projects\\2018\\AWS\\AWS SAM\\LexChatbot\\Chatbot\\LaunchInstanceConfirmation.json");

            var lexEvent = JsonConvert.DeserializeObject<LexEvent>(json);
            var lambdaFunction = new ChatbotStartupProgram(true);

            var context = new TestLambdaContext();
            var response = lambdaFunction.LambdaFunctionHandler(lexEvent, context);

            Assert.Equal("ElicitSlot", response.DialogAction.Type);
        }

        [Fact]
        public void TestDescribeInstancesEvent()
        {
            var json = File.ReadAllText(@"C:\\Office\\Projects\\2018\\AWS\\AWS SAM\\LexChatbot\\Chatbot\\DescribeInstanceEvent.json");

            var lexEvent = JsonConvert.DeserializeObject<LexEvent>(json);
            var lambdaFunction = new ChatbotStartupProgram(true);

            var context = new TestLambdaContext();
            var response = lambdaFunction.LambdaFunctionHandler(lexEvent, context);

            Assert.Equal("ElicitSlot", response.DialogAction.Type);
        }

        [Fact]
        public void TestLaunchInstanceElicitSlotEvent()
        {
            var json = File.ReadAllText(@"C:\\Office\\Projects\\2018\\AWS\\AWS SAM\\LexChatbot\\Chatbot\\LaunchInstanceElicitSlot.json");

            var lexEvent = JsonConvert.DeserializeObject<LexEvent>(json);
            var lambdaFunction = new ChatbotStartupProgram(true);

            var context = new TestLambdaContext();
            var response = lambdaFunction.LambdaFunctionHandler(lexEvent, context);

            Assert.Equal("ElicitSlot", response.DialogAction.Type);
        }

        [Fact]
        public void TestLaunchInstanceElicitNumberSlotEventFromSlack()
        {
            var json = File.ReadAllText(@"C:\\Office\\Projects\\2018\\AWS\\AWS SAM\\LexChatbot\\Chatbot\\LaunchInstanceElicitNumberSlotSlack.json");

            var lexEvent = JsonConvert.DeserializeObject<LexEvent>(json);
            var lambdaFunction = new ChatbotStartupProgram(true);

            var context = new TestLambdaContext();
            var response = lambdaFunction.LambdaFunctionHandler(lexEvent, context);

            Assert.Equal("ElicitSlot", response.DialogAction.Type);
        }
    }
}

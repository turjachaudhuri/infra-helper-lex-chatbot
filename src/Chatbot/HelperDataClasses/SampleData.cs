using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Chatbot.HelperDataClasses
{
    public class SampleData
    {
        public static readonly ImmutableArray<string> SAMPLE_GREETING_RESPONSES =
               ImmutableArray.Create<string>(new string[]
                           {
                              "Hey there , I am WolfBot , your friendly AWS Infra helper ! Hope you are doing well . Let me know what I can help you with today. If in doubt , type help.",
                              "Hola mate , I am WolfBot , your friendly AWS Infra helper ! Hope you are doing well . Please tell me how I can help you today . If in doubt , type examples.",
                              "Hello , I am WolfBot , your friendly AWS Infra helper ! Hope you are fine . Please tell me how I can solve your worries . If in doubt , type show examples.",
                           });

        public static readonly ImmutableArray<string> SAMPLE_THANKS_RESPONSES =
              ImmutableArray.Create<string>(new string[]
                          {
                              "Anytime man, I am here to help .Let me know if I can be of any further use.",
                              "No worries mate , I got you covered .Let me know if I can be of any further use.",
                              "Thank you too , you have been wonderful . Let me know if I can be of any further use."
                          });

        public static readonly ImmutableArray<string> SAMPLE_EXAMPLE_RESPONSES =
              ImmutableArray.Create<string>(new string[]
                          {
                              "I know all about your AWS instances . Ask me : list instances.",
                              "I can start an AWS instance for you . Ask me : start DEV.",
                              "I can stop an AWS instance for you . Ask me : stop QA.",
                              "I can terminate an AWS instance for you . Ask me : terminate PROD."
                          });

        public static Dictionary<string, string> AMI_DICT = new Dictionary<string, string>
                {
                    {"ubuntu", "ami-0ac019f4fcb7cb7e6"},
                    {"redhat", "ami-6871a115"},
                    {"windows", "ami-01945499792201081"},
                    {"amazonlinux", "ami-04681a1dbd79675a5"}
                };

        public static Dictionary<string, string> INSTANC_TYPE_DICT = new Dictionary<string, string>
                {
                    {"t2micro", "t2.micro"},
                    {"t2small", "t2.small"},
                    {"t2medium", "t2.medium"}
                };

        public static Dictionary<string, string> STORAGE_TYPE_DICT = new Dictionary<string, string>
                {
                    {"generalpurpose", "gp2"},
                    {"provisioned", "io1"},
                    {"magnetic", "standard"}
                };
    }
}

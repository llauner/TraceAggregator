using Google.Cloud.Functions.Testing;
using NUnit.Framework;
using System.Threading.Tasks;


namespace TraceAggregator.Tests
{
    public class FunctionTest : FunctionTestBase<Function>
    {
        [SetUp]
        public void Setup()
        {
            // Initial setup
            Server.ClearLogs();
            System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "C:\\llauner\\GoogleCloud_Credentials\\Service_Account_Key-igcheatmap-f012be117f9c.json");
        }

        [Test]
        public async Task RequestWritesMessage()
        {
            //string text = await ExecuteHttpGetRequestAsync("?reduce=true&factor=1.5&keepBacklog=true");
            string text = await ExecuteHttpGetRequestAsync("");
            Assert.AreEqual("[AggregatorService] Done !", text);
        }
    }
}
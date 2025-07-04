using Apps.Marketo.Actions;
using Apps.Marketo.Models.Emails.Requests;
using Apps.Marketo.Models.LandingPages.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.Marketo.Base;

namespace Tests.Marketo
{
    [TestClass]
    public class LandingPageTests :TestBase
    {
        [TestMethod]
        public async Task GetLandingPages()
        {
            var action = new LandingPageActions(InvocationContext,FileManager);
            var response = await action.GetLandingPageAsHtml(new GetLandingInfoRequest { Id= "1006" },
                new GetSegmentationRequest {  SegmentationId= "1002" },
                new GetSegmentBySegmentationRequest { Segment= "Test" },
                new GetLandingPageAsHtmlRequest { });
            Assert.IsNotNull(response);

        }
    }
}

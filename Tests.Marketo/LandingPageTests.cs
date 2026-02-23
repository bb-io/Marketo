using Apps.Marketo.Actions;
using Apps.Marketo.Models.Emails.Requests;
using Apps.Marketo.Models.Identifiers;
using Apps.Marketo.Models.LandingPages.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var response = await action.GetLandingPageAsHtml(new LandingPageIdentifier { LandingPageId = "1006" },
                new SegmentationIdentifier { SegmentationId= "1002" },
                new SegmentIdentifier { Segment = "Test" },
                new GetLandingPageAsHtmlRequest { });
            Assert.IsNotNull(response);

        }
    }
}

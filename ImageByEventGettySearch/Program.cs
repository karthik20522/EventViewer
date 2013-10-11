using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WatiN.Core;
using WatiN.Core.DialogHandlers;
using WatiN.Core.Exceptions;
using WatiN.Core.Interfaces;
using WatiN.Core.Logging;
using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using ImageByEventLib.Dalc;
using ImageByEventLib.Model;
using Api.Getty;
using Api.Getty.Requests;
using Api.Getty.Models;

namespace ImageByEventGettySearch
{
    class Program
    {
        static AuthInfo authInfo;
        static Client apiClient;

        [STAThread]
        static void Main(string[] args)
        {
            authInfo = new AuthInfo
            {
                SystemId = "",
                SystemPassword = "",
                UserName = "",
                Password = "",
                ConnectionMode = ConnectionMode.Production
            };
            apiClient = new Client(authInfo);
            apiClient.CreateSession();

            while (true)
            {
                QuerySQS();
                Console.WriteLine("Waiting for new Message");
                System.Threading.Thread.Sleep(5000);
            }
        }

        static List<Api.Getty.Models.Image> SearchEventImagesFromConnect(int eventId)
        {
            var searchRequest = new SearchForImages2RequestBody();
            searchRequest.Query = new Api.Getty.Models.Query() { EventId = eventId };
            searchRequest.ResultOptions = new ResultOptions { ItemCount = 75, ItemStartNumber = 1 };
            var result = apiClient.Search(searchRequest);

            return result.Images;
        }

        static void QuerySQS()
        {
            var searchDalc = new SearchDalc();
            var imageDalc = new ImageDalc();
            var colorExtract = new DominantColor();

            var sqs = AWSClientFactory.CreateAmazonSQSClient();
            var receiveMessageRequest = new ReceiveMessageRequest();
            receiveMessageRequest.QueueUrl = "https://sqs.us-west-2.amazonaws.com/xxx";
            var receiveMessageResponse = sqs.ReceiveMessage(receiveMessageRequest);
                
            if (receiveMessageResponse.IsSetReceiveMessageResult())
            {
                var receiveMessageResult = receiveMessageResponse.ReceiveMessageResult;
                foreach (var message in receiveMessageResult.Message)
                {
                    var eventDetails = new List<EventDetail>();

                    if (message.IsSetBody())
                        eventDetails = FetchEvents(message.Body);

                    if (eventDetails != null && eventDetails.Count > 0)
                    {
                        var searchResult = searchDalc.Add(new Search() { EventDetails = eventDetails, SearchTerm = message.Body, SqsId = message.MessageId });
                        if (!string.IsNullOrEmpty(searchResult._id))
                        {
                            eventDetails.Take(25).ToList().ForEach(e =>
                            {
                                Console.WriteLine(e.EventName);
                                var imageResults = SearchEventImagesFromConnect(e.EventId);

                                Parallel.ForEach(imageResults, image =>
                                {
                                    var img = new ImageDetail()
                                    {
                                        EventId = e.EventId,
                                        ImageId = image.ImageId,
                                        Thumbnail = image.UrlThumb,
                                        DominantColor = colorExtract.GetDominantColor(image.UrlThumb).Result
                                    };
                                    imageDalc.Add(img);
                                    Console.WriteLine(img._id);
                                });                                
                            });
                        }
                    }

                    var messageRecieptHandle = message.ReceiptHandle;
                    var deleteRequest = new DeleteMessageRequest();
                    deleteRequest.QueueUrl = "https://sqs.us-west-2.amazonaws.com/xx";
                    deleteRequest.ReceiptHandle = messageRecieptHandle;
                    sqs.DeleteMessage(deleteRequest);
                }
            }

            searchDalc.disconnect();
            imageDalc.disconnect();
        }

        /// <summary>
        /// Screen Scrape Events
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        static List<EventDetail> FetchEvents(string query)
        {
            var eventDetails = new List<EventDetail>();
            using (var _browser = new IE("http://www.gettyimages.com", false))
            {
                _browser.ShowWindow(WatiN.Core.Native.Windows.NativeMethods.WindowShowStyle.Hide);
                _browser.TextField(Find.ById("txtPhrase")).Clear();
                _browser.TextField(Find.ById("txtPhrase")).TypeText(query);
                var editorialChkfield = _browser.CheckBox(Find.ById("cbxEditorial"));

                if (!editorialChkfield.Checked)
                    editorialChkfield.Click();

                _browser.Button(Find.ById("btnSearch")).Click();

                if (_browser.Link(Find.ById("ctl00_ctl00_ctl12_gcc_mc_re_flEvent_lnkSeeMore")).Exists)
                {
                    _browser.Link(Find.ById("ctl00_ctl00_ctl12_gcc_mc_re_flEvent_lnkSeeMore")).Click();
                    _browser.Div(Find.ById("ctl00_ctl00_ctl12_gcc_mc_re_flEvent_refinementContent")).WaitUntilExists();

                    var filterContentDiv = _browser.Div(Find.ById("ctl00_ctl00_ctl12_gcc_mc_re_flEvent_refinementContent"));

                    foreach (var link in filterContentDiv.Links.Filter(Find.ByClass("refineItem")))
                    {
                        var splitList = link.OuterHtml.Split('\'');

                        if (splitList.Length > 5)
                            eventDetails.Add(new EventDetail() { EventId = int.Parse(splitList[1]), EventName = splitList[5].Trim() });
                    }
                }
            }

            return eventDetails;
        }
    }
}

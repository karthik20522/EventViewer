using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using ImageByEvent.Models;
using ImageByEventLib.Dalc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ImageByEvent.Controllers
{
    [SessionState(System.Web.SessionState.SessionStateBehavior.Disabled)]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var imageDalc = new ImageDalc();
            var searchDalc = new SearchDalc();
            
            var imgData = imageDalc.GetAll();//.Take(25);
            var searchData = searchDalc.GetAll().LastOrDefault();

            var homeViews = new List<HomeModel>();

            searchData.EventDetails.Take(25).ToList().ForEach(s =>
            {
                homeViews.Add(new HomeModel()
                {
                    EventId = s.EventId,
                    EventName = s.EventName,
                    ImageDetails = imgData.Where(d => d.EventId == s.EventId).ToList()
                });
            });
                
             
            
            imageDalc.disconnect();
            searchDalc.disconnect();

            return View(homeViews);
        }

        [HttpPost]
        public string Search(string term)
        {
            var sqs = AWSClientFactory.CreateAmazonSQSClient();            
            var sendMessageRequest = new SendMessageRequest();
            sendMessageRequest.QueueUrl = "https://sqs.us-west-2.amazonaws.com/x"; //URL from initial queue creation
            sendMessageRequest.MessageBody = term.ToLower().Trim();
            var sentMsg = sqs.SendMessage(sendMessageRequest);

            return sentMsg.SendMessageResult.MessageId;
        }

        [HttpPost]
        public JsonResult Ping(string sqsId)
        {
            var jsonResult = new JsonResult();

            using (var searchDalc = new SearchDalc())
            {
                var eventData = searchDalc.BySqsId(sqsId);
                if (eventData != null)
                    jsonResult.Data = eventData;
                else
                    jsonResult.Data = "";
            }
            
            return jsonResult;
        }
    }
}

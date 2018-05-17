using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.Configuration;
using System.Linq.Expressions;
using Twilio.Rest.Taskrouter.V1.Workspace;
using Twilio.Rest.Taskrouter.V1.Workspace.Task;
using Twilio;
using Twilio.Rest.Api.V2010.Account.Conference;
using Twilio.TwiML;
using Twilio.TwiML.Voice;
using Twilio.Jwt.Taskrouter;
using Twilio.Jwt.Client;
using Twilio.Http;
using Twilio.Jwt;

namespace ContactCenter.Controllers
{
    public class HomeController : Controller
    {
        
        protected string _accountSid = ConfigurationManager.AppSettings["TWILIO_ACME_ACCOUNT_SID"];
        protected string  _authToken = ConfigurationManager.AppSettings["TWILIO_ACME_AUTH_TOKEN"];
        protected string _applicationSid =  ConfigurationManager.AppSettings["TWILIO_ACME_TWIML_APP_SID"];
        protected string _workspaceSid = ConfigurationManager.AppSettings["TWILIO_ACME_WORKSPACE_SID"];
        protected string _manager_workflow = ConfigurationManager.AppSettings["TWILIO_ACME_MANAGER_WORKFLOW_SID"];
        protected string _support_workflow = ConfigurationManager.AppSettings["TWILIO_ACME_SUPPORT_WORKFLOW_SID"];
        protected string _sales_workflow = ConfigurationManager.AppSettings["TWILIO_ACME_SALES_WORKFLOW_SID"];
        protected string _billing_workflow = ConfigurationManager.AppSettings["TWILIO_ACME_BILLING_WORKFLOW_SID"];
        protected string _called_id = ConfigurationManager.AppSettings["TWILIO_ACME_CALLERID"];
        protected string _wrap_up_activity = ConfigurationManager.AppSettings[""]
      
        class PolicyUrlUtils
        {
            const string taskRouterBaseUrl = "https://taskrouter.twilio.com";
            const string taskRouterVersion = "v1";

            readonly string _workspaceSid;
            readonly string _workerSid;

            public PolicyUrlUtils(string workspaceSid, string workerSid)
            {
                _workspaceSid = workspaceSid;
                _workerSid = workerSid;
            }

            public string AllTasks => $"{Workspace}/Tasks/**";
            public string Worker => $"{Workspace}/Workers/{_workerSid}";
            public string AllReservations => $"{Worker}/Reservations/**";
            public string Workspace =>
                $"{taskRouterBaseUrl}/{taskRouterVersion}/Workspaces/{_workspaceSid}";
            public string Activities => $"{Workspace}/Activities";

        }
        
      
        
        public ActionResult Index()
       
        {
            return View();
        }

        public ActionResult Incoming_call()
        {
            var action = new Uri("/Home/ProcessDigits");
            var response = new VoiceResponse();
            var gather = new Gather(timeout: 3, numDigits: 1, action: action);
            gather.Say("Welcome to ACME corp, please select your department");
            gather.Say("For Sales press one, for Support press two, for billing press three", language: "en-gb");

            response.Append(gather);
            
           
            return Content(response.ToString(), contentType: "text/xml");
        }

        public ActionResult ProcessDigits()

        {
            var response = new VoiceResponse();
            
            Dictionary<string, string> department = new Dictionary<string, string>();
            department.Add("1", "sales");
            department.Add("2", "support");
            department.Add("3", "billing");

            Dictionary<string, string> workflowDictionary = new Dictionary<string, string>();
            workflowDictionary.Add("1", _sales_workflow);
            workflowDictionary.Add("2", _support_workflow);
            workflowDictionary.Add("3", _billing_workflow);

            var enqueue = new Enqueue(workflowSid:workflowDictionary[Request.Params.Get("digits")]);

            enqueue.Task("{'selected_product':'@" + department[Request.Params.Get("digits")] + @"'}");
            
              
            response.Append(enqueue);
            return Content(response.ToString(), contentType: "text/xml");
            
        }

        public ActionResult Agent_list()
        {

            TwilioClient.Init(_accountSid, _authToken);

            var workers = WorkerResource.Read(
                targetWorkersExpression: "worker.channel.voice.configured_capacity > 0",
                pathWorkspaceSid: _workspaceSid
            );

            foreach (var vw in workers){

                Console.Write(vw.Sid);
            }
   
            ViewBag.voice_worker = workers;

            return View();
            
        }

        public ActionResult Chat()
        {

            return View();
        }

        
        [HttpPost]
        public ActionResult CallTransfer()
        {
           
            TwilioClient.Init(_accountSid, _authToken);
        
        
                    string conferenceSid = Request.Params.Get("conference");
                    string callSid = Request.Params.Get("participant");
                    ParticipantResource.Update(
                        conferenceSid,
                        callSid,
                        hold: true
                    );    

            try
            {

                string json = @"{
               'selected_product': 'manager',
              'conference': '" + Request.Params.Get("conference") + @"',
              'customer': '" + Request.Params.Get("participant") + @"',
              'customer_taskSid': '" + Request.Params.Get("taskSid") + @"',
              'from': '" + Request.Params.Get("from") + @"',
               }";

                var task = TaskResource.Create(
                    _workspaceSid, attributes: JsonConvert.DeserializeObject(json).ToString(),
                    workflowSid: _manager_workflow
                );

            }
            catch (Exception e)
            {
                
                Console.Write(e.ToString());
            }
        
        return View();
        }

        public ActionResult CallMute()
        {
            
            TwilioClient.Init(_accountSid, _authToken);

                string conferenceSid = Request.Params.Get("conference");
                string callSid = Request.Params.Get("participant");
                bool muted = Convert.ToBoolean(Request.Params.Get("muted"));
            
                ParticipantResource.Update(
                    conferenceSid,
                    callSid,
                    hold: muted
                );

       
            var response = new VoiceResponse();
          
            return Content(response.ToString(), contentType: "text/xml");
        }

        public ActionResult TransferTwiml()
        {

            var response = new VoiceResponse();
            var  dial = new Dial();
            dial.Conference(Request.Params.Get("conference"));

            response.Append(dial);
            
            Console.Write(response.ToString());
            
            return Content(response.ToString(), contentType: "text/xml");
        }
       
		public ActionResult assignment_callback()
        {

            var wrap_up = "";

            TwilioClient.Init(_accountSid, _authToken);

            var task = Request.Params.Get("TaskSid");
            var reservation = Request.Params.Get("ReservationSid");

            var reservation_update = ReservationResource.Update(
                _workspaceSid,
                task,
                reservation, ReservationResource.StatusEnum.Accepted);

            var ret = "{'instruction':'dequeue', 'from': @" + _called_id + @",'post_work_activity_sid':'" + wrap_up + @"'}";

            return View();

        }

        public ActionResult Agent_desktop()
        {

            string workerSid = Request.Params.Get("WorkerSid");
            TwilioClient.Init(_accountSid, _authToken);

            var activityDictionary = new Dictionary<string, string>();
            
            
            var activities = ActivityResource.Read(_workspaceSid);
            foreach(var activity in activities) {
                activityDictionary.Add(activity.FriendlyName, activity.Sid);
                
            }
            
            
            var updateActivityFilter = new Dictionary<string, Policy.FilterRequirement>
            {
                { "ActivitySid", Policy.FilterRequirement.Required }
            };

            var urls = new PolicyUrlUtils(_workspaceSid, workerSid);

            var allowActivityUpdates = new Policy(urls.Worker,
                HttpMethod.Post,
                postFilter: updateActivityFilter);
            var allowTasksUpdate = new Policy(urls.AllTasks, HttpMethod.Post);
            var allowReservationUpdate = new Policy(urls.AllReservations, HttpMethod.Post);
            var allowWorkerFetches = new Policy(urls.Worker, HttpMethod.Get);
            var allowTasksFetches = new Policy(urls.AllTasks, HttpMethod.Get );
            var allowReservationFetches = new Policy(urls.AllReservations, HttpMethod.Get);
            var allowActivityFetches = new Policy(urls.Activities, HttpMethod.Get);

            var policies = new List<Policy>
            {
                allowActivityUpdates,
                allowTasksUpdate,
                allowReservationUpdate,
                allowWorkerFetches,
                allowTasksFetches,
                allowReservationFetches

            };

            var capability = new TaskRouterCapability(
                _accountSid,
                _authToken,
                _workspaceSid,
                workerSid,
                policies: policies); 

            var workerToken = capability.ToJwt();
            
            var scopes = new HashSet<IScope>
            {
                { new IncomingClientScope(Request.Params.Get("WorkerSid")) },
                { new OutgoingClientScope(_applicationSid) }
            };
            
            var webClientCapability = new ClientCapability(_accountSid, _authToken, scopes: scopes);
            var token = capability.ToJwt();
            
            ViewBag.worker_token = workerToken;
            ViewBag.client_token = webClientCapability.ToJwt();
            ViewBag.caller_ID = _called_id;
            ViewBag.activties = activityDictionary;
            return View();

        }
    }
}
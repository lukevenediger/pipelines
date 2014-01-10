using Quartz;
using Quartz.Impl;
using System.Text;
using System.Linq;
using Pipelines.Data.Context;
using System.Net.Mail;
using Pipelines.Data.Models;
using System;

namespace Pipelines.Service.Core.Schedulers
{
    public class MailScheduler
    {
        private ISchedulerFactory _factory;
        private IScheduler _scheduler;

        public MailScheduler()
        {
            if (_factory == null)
            {
                _factory = new StdSchedulerFactory();
                _scheduler = _factory.GetScheduler();

                IJobDetail instantmailer = JobBuilder.Create<InstantMailJob>()
                                                     .WithIdentity("Instant Mail Job")
                                                     .Build();

                ITrigger instantmailertrigger = TriggerBuilder.Create()
                                                              .WithIdentity("Instant Mail Job Trigger")
                                                              .WithDailyTimeIntervalSchedule(t => t.WithInterval(30, IntervalUnit.Second))
                                                              .Build();

                _scheduler.ScheduleJob(instantmailer, instantmailertrigger);
            }
        }

        public IScheduler GetScheduler()
        {
            return _scheduler;
        }
    }

    public class InstantMailJob : IJob
    {
        public virtual void Execute(IJobExecutionContext context)
        {
            using (var ctx = new PipelinesDBContext())
            {
                var lastrecord = ctx.EventMailTrackers.First(x => x.Name == "Instant Mail");
                var records = ctx.CardEvents.Where(x => x.EventId > lastrecord.CardEvent.EventId).OrderBy(r => r.EventId);

                foreach (var rec in records)
                {
                    var pipelinesubscriptions = ctx.PipelineSubscriptions.Where(ps => ps.Pipeline.PipelineId == rec.PastPipeline.PipelineId || ps.Pipeline.PipelineId == rec.Pipeline.PipelineId)
                                                                         .Select(x => new { HistoryID = rec.EventId, Email = x.User.Email, Weight = 1 })
                                                                         .ToList();

                    var stagesubscriptions = ctx.StageSubscriptions.Where(ss => ss.Stage.StageId == rec.Stage.StageId || ss.Stage.StageId == rec.PastStage.StageId)
                                                .Select(x => new { HistoryID = rec.EventId, Email = x.User.Email, Weight = 2 })
                                                .ToList();


                    var cardsubscriptions = ctx.CardSubscriptions.Where(cs => cs.Card.CardId == rec.Card.CardId)
                                               .Select(x => new { HistoryID = rec.EventId, Email = x.User.Email, Weight = 3 })
                                               .ToList();

                    var subscriptions = pipelinesubscriptions.Concat(stagesubscriptions)
                                                             .Concat(cardsubscriptions)
                                                             .GroupBy(x => new { x.Email, x.HistoryID })
                                                             .ToList();

                    foreach (var sub in subscriptions)
                    {
                        SendMail(rec, sub.Where(x => x.Weight == sub.Max(c => c.Weight)).First());
                    }
                }

                lastrecord.CardEvent = records.Where(m => m.EventId == records.Max(c => c.EventId)).First();
                ctx.SaveChanges();
            }
        }
        
        internal void SendMail(CardEvent logevent, dynamic notification)
        {
            using (var mail = new MailMessage("USERNAME", notification.Email))
            {
                mail.IsBodyHtml = true;
                
                if (logevent.PastStage != null && logevent.PastBoard != null && logevent.PastPipeline != null)
                {
                    if (logevent.PastPipeline == logevent.Pipeline)
                    {
                        mail.Subject = String.Format(@"{0} ({1}) was moved to {2} from {3} by {4}", (logevent.Card.Title.Length > 20 ? (logevent.Card.Title.Substring(0, 20)) : logevent.Card.Title),
                                                                                                     logevent.Card.CardId,
                                                                                                     logevent.Stage.Name,
                                                                                                     logevent.PastStage.Name,
                                                                                                     logevent.User.Username.Substring(logevent.User.Username.IndexOf('\\') + 1));
                    }
                    else if (logevent.PastPipeline != logevent.Pipeline)
                    {
                        if (logevent.PastStage != logevent.Stage)
                        {
                            mail.Subject = String.Format(@"{0} ({1}) was moved from {2} in the {3} pipeline to {4} in the {5} pipeline by {6}", (logevent.Card.Title.Length > 20 ? (logevent.Card.Title.Substring(0, 20)) : logevent.Card.Title),
                                                                                                                                                 logevent.Card.CardId,
                                                                                                                                                 logevent.PastStage.Name,
                                                                                                                                                 logevent.PastPipeline.Name,
                                                                                                                                                 logevent.Stage.Name,
                                                                                                                                                 logevent.Pipeline.Name,
                                                                                                                                                 logevent.User.Username.Substring(logevent.User.Username.IndexOf('\\') + 1));
                        }
                        else
                        {
                            mail.Subject = String.Format(@"{0} ({1}) was moved from {2} in the {3} pipeline to {4} in the {5} pipeline by {6}", (logevent.Card.Title.Length > 20 ? (logevent.Card.Title.Substring(0, 20)) : logevent.Card.Title),
                                                                                                                                                 logevent.Card.CardId,
                                                                                                                                                 logevent.PastStage.Name,
                                                                                                                                                 logevent.PastPipeline.Name,
                                                                                                                                                 logevent.Stage.Name,
                                                                                                                                                 logevent.Pipeline.Name,
                                                                                                                                                 logevent.User.Username.Substring(logevent.User.Username.IndexOf('\\') + 1));
                        }
                    }
                }


                var reason = (notification.Weight == 1 ? String.Format("You are receiving this because you are subscribed to the {0} pipeline.", logevent.Pipeline.Name)
                                                  : (notification.Weight == 2 ? String.Format("You are receiving this because you are subscribed to the {0} stage.", logevent.Stage.Name)
                                                                              : (notification.Weight == 3 ? String.Format("You are receiving this because you are subscribed to the {0} card.", logevent.Card.Title)
                                                                                                          : "")));

                mail.Body = String.Format(@"Hi, <br><br><b>{0}</b><br><font size=""2"">{1}</font><br><br>{2}<br><br>Thanks,<br>{3}", logevent.Card.Title, logevent.Card.Description, reason, String.Format("http://pipelines/Board/{0}", logevent.Board.Name));
  
                using (var client = new SmtpClient("mail", 25))
                {
                    client.Credentials = new System.Net.NetworkCredential("USERNAME", "PASSWORD");
                    client.Send(mail);
                }
            }
        }
    }
}
